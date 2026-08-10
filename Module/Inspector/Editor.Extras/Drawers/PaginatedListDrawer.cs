using System.Collections;
using VirtueSky.Inspector;
using VirtueSky.Inspector.Drawers;
using VirtueSky.Inspector.Elements;
using VirtueSky.Inspector.Utilities;
using UnityEditor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(PaginatedListDrawer), TriDrawerOrder.Drawer)]

namespace VirtueSky.Inspector.Drawers
{
    /// <summary>Draws [PaginatedList] lists PageSize elements at a time instead of building a UI row for
    /// every element up front (what [TableList]'s TriListElement.GenerateChildren does) - see
    /// PaginatedListAttribute for why.</summary>
    public class PaginatedListDrawer : TriAttributeDrawer<PaginatedListAttribute>
    {
        private const float KeyColumnWidth = 120f;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!propertyDefinition.IsArray)
            {
                return "[PaginatedList] valid only on lists";
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new PaginatedListElement(property, Attribute.PageSize);
        }

        private class PaginatedListElement : TriElement
        {
            private const float RowSpacing = 2f;
            private const float ButtonWidth = 24f;
            private const float PageFieldWidth = 40f;
            private const float TotalLabelWidth = 40f;
            private const float HeaderControlsWidth = ButtonWidth * 2 + PageFieldWidth + TotalLabelWidth;

            private const float BoxInsetTop = 4f;
            private const float BoxInsetBottom = 4f;
            private const float BoxInsetLeft = 4f;
            private const float BoxInsetRight = 4f;

            private readonly TriProperty _property;
            private readonly int _pageSize;
            private readonly ListPropertyOverrideContext _overrideContext;

            private int _page;
            private int _lastCount = -1;
            private int _lastPage = -1;

            public PaginatedListElement(TriProperty property, int pageSize)
            {
                _property = property;
                _pageSize = Mathf.Max(1, pageSize);
                _overrideContext = new ListPropertyOverrideContext(property);
            }

            public override bool Update()
            {
                var dirty = false;

                if (_property.IsExpanded)
                {
                    var count = _property.ArrayElementProperties.Count;
                    var pageCount = PageCount(count);
                    _page = Mathf.Clamp(_page, 0, pageCount - 1);

                    if (count != _lastCount || _page != _lastPage)
                    {
                        _lastCount = count;
                        _lastPage = _page;
                        RebuildRows(count);
                        dirty = true;
                    }
                }
                else if (ChildrenCount > 0)
                {
                    RemoveAllChildren();
                    _lastCount = -1;
                    _lastPage = -1;
                    dirty = true;
                }

                dirty |= base.Update();
                return dirty;
            }

            private int PageCount(int count) => Mathf.Max(1, Mathf.CeilToInt(count / (float) _pageSize));

            private void RebuildRows(int count)
            {
                RemoveAllChildren();

                var start = _page * _pageSize;
                var end = Mathf.Min(start + _pageSize, count);
                for (var i = start; i < end; i++)
                {
                    var index = i;
                    var rowProperty = _property.ArrayElementProperties[index];
                    AddChild(new PaginatedRowElement(rowProperty, () => RemoveRow(index)));
                }
            }

            private void RemoveRow(int index)
            {
                if (_property.Value is not IList list) return;
                if (index < 0 || index >= list.Count) return;

                list.RemoveAt(index);
                _property.SetValue(list);

                // Force RebuildRows on the next Update() even if count/page end up unchanged (e.g.
                // removing the only row on the last page).
                _lastCount = -1;
            }

            public override float GetHeight(float width)
            {
                var height = EditorGUIUtility.singleLineHeight;

                if (_property.IsExpanded && ChildrenCount > 0)
                {
                    height += RowSpacing + GetBoxContentHeight(width) + BoxInsetTop + BoxInsetBottom;
                }

                return height;
            }

            private float GetBoxContentHeight(float width)
            {
                var innerWidth = width - BoxInsetLeft - BoxInsetRight;
                return EditorGUIUtility.singleLineHeight + RowSpacing + base.GetHeight(innerWidth);
            }

            public override void OnGUI(Rect position)
            {
                var headerRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
                DrawHeader(headerRect);

                if (!_property.IsExpanded || ChildrenCount == 0)
                {
                    return;
                }

                var boxRect = new Rect(position)
                {
                    yMin = headerRect.yMax + RowSpacing,
                    height = GetBoxContentHeight(position.width) + BoxInsetTop + BoxInsetBottom,
                };
                TriEditorGUI.DrawBox(boxRect, TriEditorStyles.Box);

                var contentRect = new Rect(boxRect)
                {
                    xMin = boxRect.xMin + BoxInsetLeft,
                    xMax = boxRect.xMax - BoxInsetRight,
                    yMin = boxRect.yMin + BoxInsetTop,
                    yMax = boxRect.yMax - BoxInsetBottom,
                };

                var columnHeaderRect = new Rect(contentRect) {height = EditorGUIUtility.singleLineHeight};
                DrawColumnHeaders(columnHeaderRect);

                var bodyRect = new Rect(contentRect) {yMin = columnHeaderRect.yMax + RowSpacing};

                // Same trick TableListDrawer uses: suppress the redundant "Key"/"Value" field label
                // inside each cell since the column header above already says it - but only for the
                // key/value primitives themselves, not nested sub-fields (e.g. LevelRef's packIndex/
                // offset/length keep their own labels, same as the old table did).
                using (TriPropertyOverrideContext.BeginOverride(_overrideContext))
                {
                    base.OnGUI(bodyRect);
                }
            }

            private void DrawColumnHeaders(Rect position)
            {
                var keyRect = new Rect(position) {width = KeyColumnWidth};
                var valueRect = new Rect(position) {xMin = keyRect.xMax + PaginatedRowElement.ColumnSpacing};

                EditorGUI.LabelField(keyRect, "Key", EditorStyles.boldLabel);
                EditorGUI.LabelField(valueRect, "Value", EditorStyles.boldLabel);
            }

            private void DrawHeader(Rect position)
            {
                var count = _property.ArrayElementProperties.Count;

                var foldoutRect = new Rect(position) {xMax = position.xMax - HeaderControlsWidth};
                _property.IsExpanded = EditorGUI.Foldout(foldoutRect, _property.IsExpanded,
                    $"{_property.DisplayName} ({count})", true);

                if (!_property.IsExpanded)
                {
                    return;
                }

                var pageCount = PageCount(count);
                var x = position.xMax - HeaderControlsWidth;

                var prevRect = new Rect(x, position.y, ButtonWidth, position.height);
                using (new EditorGUI.DisabledScope(_page <= 0))
                {
                    if (GUI.Button(prevRect, "◀")) _page--;
                }

                x += ButtonWidth;

                var pageFieldRect = new Rect(x, position.y, PageFieldWidth, position.height);
                var displayedPage = EditorGUI.IntField(pageFieldRect, _page + 1);
                _page = Mathf.Clamp(displayedPage - 1, 0, pageCount - 1);
                x += PageFieldWidth;

                var totalLabelRect = new Rect(x, position.y, TotalLabelWidth, position.height);
                GUI.Label(totalLabelRect, $"/ {pageCount}");
                x += TotalLabelWidth;

                var nextRect = new Rect(x, position.y, ButtonWidth, position.height);
                using (new EditorGUI.DisabledScope(_page >= pageCount - 1))
                {
                    if (GUI.Button(nextRect, "▶")) _page++;
                }
            }
        }

        /// <summary>Mirrors TableListDrawer.TableListPropertyOverrideContext exactly: suppresses the
        /// field label for a primitive property one level below the row (i.e. key/value themselves when
        /// primitive) since the column header already names it, leaving deeper nested fields (e.g.
        /// LevelRef's packIndex/offset/length) with their normal labels.</summary>
        private class ListPropertyOverrideContext : TriPropertyOverrideContext
        {
            private static readonly GUIContent NoneLabel = GUIContent.none;

            private readonly TriProperty _grandParentProperty;

            public ListPropertyOverrideContext(TriProperty grandParentProperty)
            {
                _grandParentProperty = grandParentProperty;
            }

            public override bool TryGetDisplayName(TriProperty property, out GUIContent displayName)
            {
                if (property.PropertyType == TriPropertyType.Primitive &&
                    property.Parent?.Parent == _grandParentProperty &&
                    !property.TryGetAttribute(out GroupAttribute _))
                {
                    displayName = NoneLabel;
                    return true;
                }

                displayName = default;
                return false;
            }
        }

        /// <summary>One dictionary entry (a DictionaryCustomData&lt;TKey,TValue&gt;) drawn as 2 columns -
        /// key on the left (fixed width, matches DrawColumnHeaders), value filling the rest - instead of
        /// key/value stacked vertically like a plain inline generic field would.</summary>
        private class PaginatedRowElement : TriElement
        {
            public const float ColumnSpacing = 4f;
            private const float RemoveButtonWidth = 20f;
            private const float RemoveButtonSpacing = 4f;

            private readonly TriPropertyElement _keyElement;
            private readonly TriPropertyElement _valueElement;
            private readonly System.Action _onRemove;

            public PaginatedRowElement(TriProperty rowProperty, System.Action onRemove)
            {
                _onRemove = onRemove;

                var fields = rowProperty.ChildrenProperties; // DictionaryCustomData: key, value (declaration order)
                var props = new TriPropertyElement.Props {forceInline = true};
                _keyElement = new TriPropertyElement(fields[0], props);
                _valueElement = new TriPropertyElement(fields[1], props);

                AddChild(_keyElement);
                AddChild(_valueElement);
            }

            public override float GetHeight(float width)
            {
                var valueWidth = width - KeyColumnWidth - ColumnSpacing - RemoveButtonWidth - RemoveButtonSpacing;
                return Mathf.Max(
                    EditorGUIUtility.singleLineHeight,
                    _keyElement.GetHeight(KeyColumnWidth),
                    _valueElement.GetHeight(valueWidth));
            }

            public override void OnGUI(Rect position)
            {
                var keyRect = new Rect(position) {width = KeyColumnWidth};
                var removeRect = new Rect(position.xMax - RemoveButtonWidth, position.y,
                    RemoveButtonWidth, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(position)
                {
                    xMin = keyRect.xMax + ColumnSpacing,
                    xMax = removeRect.x - RemoveButtonSpacing,
                };

                _keyElement.OnGUI(keyRect);
                _valueElement.OnGUI(valueRect);

                if (GUI.Button(removeRect, "✕"))
                {
                    _onRemove?.Invoke();
                }
            }
        }
    }
}
