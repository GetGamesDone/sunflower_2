using System;
using System.Collections.Generic;

namespace VirtueSky.RecycleView
{
    /// Ready-made adapter for the usual case: one list of data, one holder prefab.
    ///
    ///     _adapter = new RecycleViewAdapter&lt;Entry, EntryRow&gt;(80f, (row, entry, index) => row.Bind(entry));
    ///     listView.SetAdapter(_adapter);
    ///     _adapter.SetItems(entries);
    ///     listView.NotifyDataSetChanged();
    public class RecycleViewAdapter<TData, THolder> : IRecycleViewAdapter where THolder : RecycleViewHolder
    {
        private readonly List<TData> _items = new();
        private readonly Action<THolder, TData, int> _binder;
        private readonly Func<TData, int, float> _sizeProvider;
        private readonly float _fixedItemSize;

        public RecycleViewAdapter(float fixedItemSize, Action<THolder, TData, int> binder)
        {
            _fixedItemSize = fixedItemSize;
            _binder = binder;
        }

        public RecycleViewAdapter(Func<TData, int, float> sizeProvider, Action<THolder, TData, int> binder)
        {
            _sizeProvider = sizeProvider;
            _binder = binder;
        }

        public IReadOnlyList<TData> Items => _items;

        public int Count => _items.Count;

        public void SetItems(IEnumerable<TData> items)
        {
            _items.Clear();
            if (items != null) _items.AddRange(items);
        }

        public TData GetItem(int index) => index >= 0 && index < _items.Count ? _items[index] : default;

        public float GetItemSize(int index) =>
            _sizeProvider != null ? _sizeProvider(GetItem(index), index) : _fixedItemSize;

        public void Bind(int index, RecycleViewHolder holder)
        {
            if (_binder == null || holder is not THolder typed) return;
            _binder(typed, GetItem(index), index);
        }
    }
}
