using UnityEngine;

namespace VirtueSky.RecycleView
{
    /// Base for anything a RecycleView spawns. One holder instance is reused for many data
    /// indices, so treat OnBind as "this row now shows a different item" rather than as a
    /// one-time setup point.
    [RequireComponent(typeof(RectTransform))]
    public abstract class RecycleViewHolder : MonoBehaviour
    {
        private RectTransform _rectTransform;

        public RectTransform RectTransform =>
            _rectTransform != null ? _rectTransform : _rectTransform = (RectTransform)transform;

        /// Data index currently displayed, or -1 while the holder sits in the pool.
        public int Index { get; internal set; } = -1;

        /// Called when the holder is taken out of the pool and pointed at a new data index.
        public virtual void OnBind(int index)
        {
        }

        /// Called when the holder scrolls out of view and returns to the pool. Use it to stop
        /// tweens or cancel per-row async work that would otherwise write into a row now
        /// showing someone else's data.
        public virtual void OnRecycled()
        {
        }
    }
}
