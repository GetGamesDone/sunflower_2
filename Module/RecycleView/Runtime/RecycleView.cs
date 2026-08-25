using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VirtueSky.RecycleView
{
    public enum RecycleViewDirection
    {
        Vertical = 0,
        Horizontal = 1
    }

    /// Scrolling list that keeps only the on-screen rows alive: a list of 10,000 entries costs
    /// the same instantiate count as a list of 20.
    ///
    /// Rows are positioned by absolute offset rather than by a LayoutGroup - a LayoutGroup
    /// would have to walk every child on every change, which is exactly the cost recycling
    /// exists to avoid. That also means the content object must not carry a LayoutGroup or a
    /// ContentSizeFitter; this component owns the content size itself.
    [RequireComponent(typeof(RectTransform))]
    public class RecycleView : MonoBehaviour
    {
        [Header("Refs")] [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RecycleViewHolder itemPrefab;

        [Header("Layout")] [SerializeField] private RecycleViewDirection direction = RecycleViewDirection.Vertical;
        [SerializeField] private float spacing = 8f;
        [SerializeField] private RectOffset padding = new();
        [SerializeField] private float defaultItemSize = 120f;

        [Header("Pooling")] [SerializeField] private int extraVisibleItems = 2;
        [SerializeField] private int prewarmCount = 0;

        private readonly List<float> _offsets = new();
        private readonly Dictionary<int, RecycleViewHolder> _active = new();
        private readonly Stack<RecycleViewHolder> _pool = new();
        private readonly List<int> _recycleBuffer = new();

        private IRecycleViewAdapter _adapter;
        private RectTransform _content;
        private RectTransform _viewport;
        private float _totalSize;
        private int _firstVisible = -1;
        private int _lastVisible = -2;
        private bool _subscribed;

        public int Count => _adapter?.Count ?? 0;
        public ScrollRect ScrollRect => scrollRect;
        public float DefaultItemSize => defaultItemSize;

        private void Awake()
        {
            Resolve();
        }

        private void OnEnable()
        {
            Resolve();
            if (scrollRect != null && !_subscribed)
            {
                scrollRect.onValueChanged.AddListener(OnScrolled);
                _subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (scrollRect != null && _subscribed)
            {
                scrollRect.onValueChanged.RemoveListener(OnScrolled);
                _subscribed = false;
            }
        }

        private void LateUpdate()
        {
            // The visible window also changes when the viewport resizes or when the list is
            // populated before layout has run, neither of which raises onValueChanged.
            if (_adapter != null) UpdateVisible();
        }

        public void SetAdapter(IRecycleViewAdapter adapter)
        {
            _adapter = adapter;
            NotifyDataSetChanged();
        }

        /// Recomputes sizes and offsets, then rebinds whatever is on screen. Call after the
        /// backing data changes in any way other than a single row's contents.
        public void NotifyDataSetChanged(bool keepScrollPosition = true)
        {
            Resolve();
            if (_content == null) return;

            var previousOffset = keepScrollPosition ? ScrollOffset : 0f;

            RecycleAll();
            RebuildOffsets();
            ApplyContentSize();

            if (!keepScrollPosition) SetScrollOffset(0f);
            else SetScrollOffset(Mathf.Clamp(previousOffset, 0f, Mathf.Max(0f, _totalSize - ViewportSize)));

            _firstVisible = -1;
            _lastVisible = -2;
            UpdateVisible();
        }

        /// Rebinds the rows currently on screen without touching sizes or scroll position -
        /// the cheap path for "the same rows, new values".
        public void RefreshVisible()
        {
            if (_adapter == null) return;
            foreach (var pair in _active)
            {
                if (pair.Key < 0 || pair.Key >= _adapter.Count) continue;
                _adapter.Bind(pair.Key, pair.Value);
            }
        }

        public void ScrollToIndex(int index, float viewportAnchor = 0f)
        {
            if (_adapter == null || index < 0 || index >= _offsets.Count) return;
            var target = _offsets[index] - ViewportSize * Mathf.Clamp01(viewportAnchor);
            SetScrollOffset(Mathf.Clamp(target, 0f, Mathf.Max(0f, _totalSize - ViewportSize)));
            UpdateVisible();
        }

        public void ScrollToTop() => ScrollToIndex(0);

        public bool TryGetActiveHolder(int index, out RecycleViewHolder holder) =>
            _active.TryGetValue(index, out holder);

        public void Clear()
        {
            _adapter = null;
            RecycleAll();
            _offsets.Clear();
            _totalSize = 0f;
            ApplyContentSize();
        }

        private void Resolve()
        {
            if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
            if (scrollRect == null) return;

            _content = scrollRect.content;
            _viewport = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;

            if (_content == null) return;

            if (direction == RecycleViewDirection.Vertical)
            {
                _content.anchorMin = new Vector2(0f, 1f);
                _content.anchorMax = new Vector2(1f, 1f);
                _content.pivot = new Vector2(0.5f, 1f);
            }
            else
            {
                _content.anchorMin = new Vector2(0f, 0f);
                _content.anchorMax = new Vector2(0f, 1f);
                _content.pivot = new Vector2(0f, 0.5f);
            }

            if (prewarmCount > 0) Prewarm(prewarmCount);
        }

        private void Prewarm(int count)
        {
            if (itemPrefab == null || _content == null) return;
            while (_pool.Count < count)
            {
                var holder = Instantiate(itemPrefab, _content);
                holder.gameObject.SetActive(false);
                _pool.Push(holder);
            }

            prewarmCount = 0;
        }

        private void RebuildOffsets()
        {
            _offsets.Clear();
            var count = Count;
            var cursor = direction == RecycleViewDirection.Vertical ? padding.top : padding.left;

            for (var i = 0; i < count; i++)
            {
                _offsets.Add(cursor);
                var size = _adapter.GetItemSize(i);
                if (size <= 0f) size = defaultItemSize;
                cursor += size;
                if (i < count - 1) cursor += spacing;
            }

            cursor += direction == RecycleViewDirection.Vertical ? padding.bottom : padding.right;
            _totalSize = count == 0 ? 0f : cursor;
        }

        private void ApplyContentSize()
        {
            if (_content == null) return;
            var size = _content.sizeDelta;
            if (direction == RecycleViewDirection.Vertical) size.y = _totalSize;
            else size.x = _totalSize;
            _content.sizeDelta = size;
        }

        private void OnScrolled(Vector2 _) => UpdateVisible();

        private void UpdateVisible()
        {
            if (_adapter == null || _content == null) return;

            var count = _adapter.Count;
            if (count == 0)
            {
                if (_active.Count > 0) RecycleAll();
                return;
            }

            if (_offsets.Count != count)
            {
                NotifyDataSetChanged();
                return;
            }

            var viewportSize = ViewportSize;
            if (viewportSize <= 0f) return;

            var start = ScrollOffset;
            var first = Mathf.Max(0, FindFirstVisible(start) - extraVisibleItems);
            var last = first;
            var limit = start + viewportSize;

            while (last + 1 < count && _offsets[last + 1] < limit) last++;
            last = Mathf.Min(count - 1, last + extraVisibleItems);

            if (first == _firstVisible && last == _lastVisible) return;

            _recycleBuffer.Clear();
            foreach (var pair in _active)
            {
                if (pair.Key < first || pair.Key > last) _recycleBuffer.Add(pair.Key);
            }

            foreach (var index in _recycleBuffer) Recycle(index);

            for (var i = first; i <= last; i++)
            {
                if (_active.ContainsKey(i)) continue;
                Spawn(i);
            }

            _firstVisible = first;
            _lastVisible = last;
        }

        private int FindFirstVisible(float scrollOffset)
        {
            var low = 0;
            var high = _offsets.Count - 1;
            var result = 0;

            while (low <= high)
            {
                var mid = (low + high) / 2;
                if (_offsets[mid] <= scrollOffset)
                {
                    result = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return result;
        }

        private void Spawn(int index)
        {
            if (itemPrefab == null || _content == null) return;

            var holder = _pool.Count > 0 ? _pool.Pop() : Instantiate(itemPrefab, _content);
            holder.transform.SetParent(_content, false);
            holder.gameObject.SetActive(true);
            holder.Index = index;

            ApplyItemTransform(holder.RectTransform, index);

            _adapter.Bind(index, holder);
            holder.OnBind(index);

            _active[index] = holder;
        }

        private void Recycle(int index)
        {
            if (!_active.TryGetValue(index, out var holder)) return;
            _active.Remove(index);

            holder.OnRecycled();
            holder.Index = -1;
            holder.gameObject.SetActive(false);
            _pool.Push(holder);
        }

        private void RecycleAll()
        {
            _recycleBuffer.Clear();
            foreach (var pair in _active) _recycleBuffer.Add(pair.Key);
            foreach (var index in _recycleBuffer) Recycle(index);
            _firstVisible = -1;
            _lastVisible = -2;
        }

        private void ApplyItemTransform(RectTransform rect, int index)
        {
            var size = _adapter.GetItemSize(index);
            if (size <= 0f) size = defaultItemSize;
            var offset = _offsets[index];

            if (direction == RecycleViewDirection.Vertical)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.sizeDelta = new Vector2(-(padding.left + padding.right), size);
                rect.anchoredPosition = new Vector2((padding.left - padding.right) * 0.5f, -offset);
            }
            else
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(size, -(padding.top + padding.bottom));
                rect.anchoredPosition = new Vector2(offset, (padding.bottom - padding.top) * 0.5f);
            }
        }

        private float ViewportSize
        {
            get
            {
                if (_viewport == null) return 0f;
                var rect = _viewport.rect;
                return direction == RecycleViewDirection.Vertical ? rect.height : rect.width;
            }
        }

        /// How far the list is scrolled from its start, in pixels, always positive - the raw
        /// anchoredPosition is negative on the horizontal axis and positive on the vertical one.
        private float ScrollOffset
        {
            get
            {
                if (_content == null) return 0f;
                return direction == RecycleViewDirection.Vertical
                    ? _content.anchoredPosition.y
                    : -_content.anchoredPosition.x;
            }
        }

        private void SetScrollOffset(float value)
        {
            if (_content == null) return;
            var position = _content.anchoredPosition;
            if (direction == RecycleViewDirection.Vertical) position.y = value;
            else position.x = -value;
            _content.anchoredPosition = position;
        }
    }
}
