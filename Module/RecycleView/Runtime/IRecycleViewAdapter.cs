namespace VirtueSky.RecycleView
{
    /// Supplies the row count, row sizes and the binding step to a RecycleView. Implement it
    /// directly for custom behaviour, or use RecycleViewAdapter&lt;TData, THolder&gt; for the
    /// common "a list of items, one prefab" case.
    public interface IRecycleViewAdapter
    {
        int Count { get; }

        /// Height for a vertical view, width for a horizontal one. Called once per item on
        /// every NotifyDataSetChanged, so keep it cheap - return a constant unless rows really
        /// do differ in size.
        float GetItemSize(int index);

        void Bind(int index, RecycleViewHolder holder);
    }
}
