using System;
using System.Diagnostics;

namespace VirtueSky.Inspector
{
    /// <summary>Draws a list N elements at a time (prev/next + jump-to-page) instead of all at once -
    /// see PaginatedListDrawer. Use on large lists where the default [TableList] causes editor lag from
    /// building a UI row for every element up front.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class PaginatedListAttribute : Attribute
    {
        public int PageSize { get; set; } = 10;
    }
}
