#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NekoLib.Collections
{
    /// <summary>
    /// Property drawer for Grid<T> that shows a pagination UI where each page is a row of the grid.
    /// Works for common primitive types, strings, enums, and UnityEngine.Object references.
    /// Unknown types are displayed read-only via ToString().
    /// </summary>
    [CustomPropertyDrawer(typeof(Grid<>), true)]
    public class GridPropertyDrawer : PropertyDrawer
    {
        private static float Line => EditorGUIUtility.singleLineHeight;
        private const float VSpace = 2f;
        private const float SectionPad = 6f;
        private const float ListLeftMargin = 8f; // extra padding so foldout icon/label don't hug the box border
        private static readonly Dictionary<string, int> RowPage = new();
        private static readonly Dictionary<string, Vector2Int> PendingDims = new();
        private static readonly Dictionary<string, string> CreationErrors = new();

        private static float CalcElementsBlockHeight(SerializedProperty rootProperty, SerializedProperty dataSP, int width, int height)
        {
            float elementsBlock = 0f;
            if (dataSP != null && dataSP.isArray && dataSP.arraySize >= 0)
            {
                string key = rootProperty.propertyPath;
                if (!RowPage.TryGetValue(key, out int row)) row = 0;
                row = Mathf.Clamp(row, 0, Math.Max(0, height - 1));

                for (int x = 0; x < width; x++)
                {
                    int idx = x + (row * Math.Max(0, width));
                    if (idx >= 0 && idx < dataSP.arraySize)
                    {
                        var elem = dataSP.GetArrayElementAtIndex(idx);
                        elementsBlock += EditorGUI.GetPropertyHeight(elem, new GUIContent($"Element [{x}]"), includeChildren: true);
                    }
                    else
                    {
                        elementsBlock += Line;
                    }
                    if (x < width - 1) elementsBlock += VSpace;
                }
            }
            else
            {
                elementsBlock = (width * Line) + Math.Max(0, width - 1) * VSpace;
            }

            return elementsBlock;
        }

        private object GetGridValue(SerializedProperty property)
        {
            try { return fieldInfo?.GetValue(property.serializedObject.targetObject); }
            catch { return null; }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var gridVal = GetGridValue(property);
            // Always one line (header) if null
            if (gridVal == null)
                return Line + EditorGUIUtility.standardVerticalSpacing;

            // Prefer SerializedProperty to compute UI height accurately (especially for serializable classes)
            var widthSP = property.FindPropertyRelative("_width");
            var heightSP = property.FindPropertyRelative("_height");
            var dataSP = property.FindPropertyRelative("_data");
            if (widthSP == null || heightSP == null)
                return Line;

            int width = widthSP.intValue;
            int height = heightSP.intValue;

            // Always at least the header line.
            if (!property.isExpanded)
                return Line + EditorGUIUtility.standardVerticalSpacing; // collapsed: header only

            float total = Line; // header line

            bool hasRows = width > 0 && height > 0;
            if (!hasRows)
                return total; // no pager/list when empty

            // Pager height
            float pagerBlock = Line + (VSpace * 2);

            // Elements block: sum element property heights for the selected row
            float elementsBlock = CalcElementsBlockHeight(property, dataSP, width, height);

            float section = (SectionPad * 2) + pagerBlock + elementsBlock;
            total += VSpace + section;
            return total + EditorGUIUtility.standardVerticalSpacing; // bottom spacing so following properties are not jammed
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Header foldout: restrict clickable/toggle area to label region only so input fields are fully interactive.
            var headerRect = new Rect(position.x, position.y, position.width, Line);
            float foldoutLabelWidth = Mathf.Max(EditorGUIUtility.labelWidth, GUI.skin.label.CalcSize(label).x + 18f); // include arrow padding
            var foldoutRect = new Rect(headerRect.x, headerRect.y, foldoutLabelWidth, Line);
            // Only toggle on the foldout arrow/label, not the entire header row
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

            // Compute content rect on the same line, to the right of the foldout/label region
            var indentedHeader = EditorGUI.IndentedRect(headerRect);
            float contentStartX = Mathf.Min(position.x + position.width - 50f, indentedHeader.x + foldoutLabelWidth); // keep some room
            var headerContentRect = new Rect(contentStartX, headerRect.y, position.x + position.width - contentStartX, Line);

            // We'll still draw width/height on the header even when collapsed
            bool collapsed = !property.isExpanded;

            var gridObj = GetGridValue(property);
            if (gridObj == null)
            {
                // Allow creating a new grid when null: [width] [height] [Create]
                var content = headerContentRect;
                string key = property.propertyPath;
                if (!PendingDims.TryGetValue(key, out var dims)) dims = new Vector2Int(1, 1);

                float btnW = 70f;
                float halfN = content.width - 8f - btnW;
                halfN *= 0.5f;

                var wLabelN = new GUIContent("w:", "Grid width (columns)");
                var hLabelN = new GUIContent("h:", "Grid height (rows)");
                float wLabWN = Mathf.Max(14f, GUI.skin.label.CalcSize(wLabelN).x);
                float hLabWN = Mathf.Max(14f, GUI.skin.label.CalcSize(hLabelN).x);

                var wLabelRectN = new Rect(content.x, content.y, wLabWN, Line);
                var wRectN = new Rect(wLabelRectN.xMax + 2f, content.y, halfN - (wLabWN + 2f), Line);
                var hStartXN = wRectN.xMax + 4f;
                var hLabelRectN = new Rect(hStartXN, content.y, hLabWN, Line);
                var hRectN = new Rect(hLabelRectN.xMax + 2f, content.y, halfN - (hLabWN + 2f), Line);
                var bRect = new Rect(hRectN.xMax + 8f, content.y, btnW, Line);

                // Labels with tooltips and adjacent fields (allow 0/negative temporarily for validation feedback)
                EditorGUI.LabelField(wLabelRectN, wLabelN);
                dims.x = EditorGUI.IntField(wRectN, dims.x);
                EditorGUI.LabelField(hLabelRectN, hLabelN);
                dims.y = EditorGUI.IntField(hRectN, dims.y);
                PendingDims[key] = dims;

                bool dimsValid = dims.x > 0 && dims.y > 0;
                string createTooltip = "Create a new Grid instance with the specified width (columns) and height (rows).";
                var btnContent = new GUIContent("Create", createTooltip);

                if (!dimsValid)
                {
                    var warnRect = new Rect(content.x, content.y + Line + 2f, content.width, Line * 1.2f);
                    EditorGUI.HelpBox(warnRect, "Width and Height must be > 0", MessageType.Warning);
                }
                if (CreationErrors.TryGetValue(key, out var errMsg))
                {
                    var errRect = new Rect(content.x, content.y + Line + (dimsValid ? 2f : Line + 6f), content.width, Line * 1.4f);
                    EditorGUI.HelpBox(errRect, errMsg, MessageType.Error);
                }

                using (new EditorGUI.DisabledScope(!dimsValid))
                {
                    if (GUI.Button(bRect, btnContent))
                    {
                        try
                        {
                            var tN = fieldInfo.FieldType; // closed generic Grid<T>
                            var ctor = tN.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int), typeof(int) }, null);
                            if (ctor == null)
                            {
                                CreationErrors[key] = "No (int,int) constructor found.";
                            }
                            else
                            {
                                var newGrid = ctor.Invoke(new object[] { Mathf.Max(1, dims.x), Mathf.Max(1, dims.y) }); // ensure >=1
                                Undo.RecordObject(property.serializedObject.targetObject, "Create Grid");
                                fieldInfo.SetValue(property.serializedObject.targetObject, newGrid); // reflection assignment for non-managed
                                if (property.propertyType == SerializedPropertyType.ManagedReference)
                                    property.managedReferenceValue = newGrid;
                                property.serializedObject.Update();
                                property.serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(property.serializedObject.targetObject);
                                property.isExpanded = true; // auto-expand after creation for visibility
                                PendingDims.Remove(key);
                                CreationErrors.Remove(key);
                            }
                        }
                        catch (Exception ex)
                        {
                            CreationErrors[key] = ex.Message;
                            Debug.LogException(ex);
                        }
                    }
                }

                EditorGUI.EndProperty();
                return; // for null grid we only draw the header/create row
            }

            var gridType = gridObj.GetType();
            var t = gridType.GetGenericArguments()[0];

            // Prefer SerializedProperty fields for width/height/data to ensure proper change tracking
            var widthSP = property.FindPropertyRelative("_width");
            var heightSP = property.FindPropertyRelative("_height");
            var dataSP = property.FindPropertyRelative("_data");
            if (widthSP == null || heightSP == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("(unusable Grid)"));
                EditorGUI.EndProperty();
                return;
            }

            int width = widthSP.intValue;
            int height = heightSP.intValue;

            // First line below is the same header line: vector-style [w] [h] to the right of the foldout label
            var r = new Rect(position.x, position.y, position.width, Line);
            var contentRect = headerContentRect;
            var half = (contentRect.width - 4f) * 0.5f;

            var wLabel = new GUIContent("w:", "Grid width (columns)");
            var hLabel = new GUIContent("h:", "Grid height (rows)");
            float wLabW = Mathf.Max(14f, GUI.skin.label.CalcSize(wLabel).x);
            float hLabW = Mathf.Max(14f, GUI.skin.label.CalcSize(hLabel).x);

            var wLabelRect = new Rect(contentRect.x, contentRect.y, wLabW, Line);
            var wRect = new Rect(wLabelRect.xMax + 2f, contentRect.y, half - (wLabW + 2f), Line);

            float hStartX = contentRect.x + half + 4f;
            var hLabelRect = new Rect(hStartX, contentRect.y, hLabW, Line);
            var hRect = new Rect(hLabelRect.xMax + 2f, contentRect.y, half - (hLabW + 2f), Line);

            EditorGUI.BeginChangeCheck();
            // Draw labels with tooltips right before the input fields
            EditorGUI.LabelField(wLabelRect, wLabel);
            int newW = Mathf.Max(1, EditorGUI.IntField(wRect, width));
            EditorGUI.LabelField(hLabelRect, hLabel);
            int newH = Mathf.Max(1, EditorGUI.IntField(hRect, height));
            if (EditorGUI.EndChangeCheck())
            {
                var newGrid = CreateResizedGrid(gridObj, gridType, newW, newH);
                if (newGrid != null)
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Resize Grid");
                    fieldInfo.SetValue(property.serializedObject.targetObject, newGrid);
                    if (property.propertyType == SerializedPropertyType.ManagedReference)
                        property.managedReferenceValue = newGrid;
                    property.serializedObject.Update();
                    property.serializedObject.ApplyModifiedProperties();

                    gridObj = newGrid;
                    widthSP = property.FindPropertyRelative("_width");
                    heightSP = property.FindPropertyRelative("_height");
                    dataSP = property.FindPropertyRelative("_data");
                    width = newW; height = newH;
                }
            }
            r.y += Line + VSpace;

            if (collapsed)
            {
                EditorGUI.EndProperty();
                return; // only header with w/h shown when collapsed
            }

            // Pager + list only when grid has rows
            if (width > 0 && height > 0)
            {
                // Calculate section rect (help-box style) using dynamic element heights
                string key = property.propertyPath;
                // Reserve height for pager (line + vertical padding); no extra magic offset needed now
                float pagerBlockH = Line + (VSpace * 2);
                float elementsBlockH = CalcElementsBlockHeight(property, dataSP, width, height);
                float sectionHeight = (SectionPad * 2) + pagerBlockH + elementsBlockH;

                var sectionRect = new Rect(position.x, r.y, position.width, sectionHeight);
                GUI.Box(sectionRect, GUIContent.none, EditorStyles.helpBox);

                // Context menu (right-click) on header or section to clear grid

                // Inner content rect
                var inner = new Rect(sectionRect.x + SectionPad, sectionRect.y + SectionPad, sectionRect.width - (SectionPad * 2), sectionRect.height - (SectionPad * 2));

                // Pager with small spacing above/below — draw with immediate mode (no GUILayout) to keep layout isolated
                var pager = inner;
                pager.height = pagerBlockH; // use the same reserved height used for section sizing

                if (!RowPage.TryGetValue(key, out int row)) row = 0;
                row = Mathf.Clamp(row, 0, Math.Max(0, height - 1));

                // Left: total items label; Right: navigation cluster "◀ [n] / total ▶"
                int totalItems = Mathf.Max(0, width * height);
                string countLabel = $"{totalItems} {(totalItems == 1 ? "Item" : "Items")}";
                float countW = Mathf.Max(60f, GUI.skin.label.CalcSize(new GUIContent(countLabel)).x);
                float btnW = 22f;
                float fieldW = 22f;
                float space = 4f;
                float totalLabelW = GUI.skin.label.CalcSize(new GUIContent($"/ {height}")).x;
                float clusterW = btnW + space + fieldW + space + totalLabelW + space + btnW;

                float y = pager.y + VSpace;
                // Left-aligned total label (dimmed)
                var countRect = new Rect(pager.x, y, countW, Line);
                var prevColor = GUI.color;
                GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, 0.7f);
                EditorGUI.LabelField(countRect, countLabel, EditorStyles.miniLabel);
                GUI.color = prevColor;

                // Right-aligned nav cluster
                float startX = pager.x + pager.width - clusterW;
                // Ensure some gap from the left label if space is tight
                float minStart = countRect.xMax + 8f;
                if (startX < minStart) startX = minStart;

                var prevRect = new Rect(startX, y, btnW, Line);
                var pageRect = new Rect(prevRect.xMax + space, y, fieldW, Line);
                var totalRect = new Rect(pageRect.xMax + space, y, totalLabelW, Line);
                var nextRect = new Rect(totalRect.xMax + space, y, btnW, Line);

                using (new EditorGUI.DisabledScope(row <= 0))
                {
                    if (GUI.Button(prevRect, "\u25C0", EditorStyles.miniButton)) // ◀
                        row = Mathf.Max(0, row - 1);
                }

                string pageText = (row + 1).ToString();
                var tfStyle = new GUIStyle(EditorStyles.miniTextField)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fixedHeight = Line
                };
                string newText = EditorGUI.TextField(pageRect, pageText, tfStyle);
                if (newText != pageText && int.TryParse(newText, out int pageNum))
                {
                    if (pageNum >= 1 && pageNum <= height)
                        row = pageNum - 1;
                }

                EditorGUI.LabelField(totalRect, $"/ {height}", EditorStyles.miniLabel);

                using (new EditorGUI.DisabledScope(row >= height - 1))
                {
                    if (GUI.Button(nextRect, "\u25B6", EditorStyles.miniButton)) // ▶
                        row = Mathf.Min(Math.Max(0, height - 1), row + 1);
                }
                RowPage[key] = row;

                // Move below pager with tiny spacing
                var listRect = inner;
                // Place list below the actual pager height
                listRect.y = pager.y + pager.height + VSpace;
                listRect.height = inner.yMax - listRect.y;
                listRect.x += ListLeftMargin;
                listRect.width -= ListLeftMargin;

                // Draw elements using SerializedProperty so Unity tracks prefab/undo changes
                var rowY = listRect.y;
                for (int xi = 0; xi < width; xi++)
                {
                    var idx = xi + (row * Math.Max(0, width));
                    float elemHeight = Line;
                    SerializedProperty elemProp = null;
                    object currentVal = null;
                    bool hasCurrent = TryGetCell(gridObj, gridType, t, xi, row, out currentVal);
                    if (dataSP != null && dataSP.isArray && idx >= 0 && idx < dataSP.arraySize)
                    {
                        elemProp = dataSP.GetArrayElementAtIndex(idx);
                        elemHeight = EditorGUI.GetPropertyHeight(elemProp, new GUIContent($"Element [{xi}]"), includeChildren: true);
                    }

                    var lineRect = new Rect(listRect.x, rowY, listRect.width, elemHeight);
                    // Zebra stripe (only for first line height)
                    var stripeRect = new Rect(sectionRect.x + 1, lineRect.y, sectionRect.width - 2, Line);
                    if ((xi & 1) == 0)
                    {
                        var c = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.04f) : new Color(0f, 0f, 0f, 0.06f);
                        EditorGUI.DrawRect(stripeRect, c);
                    }

                    if (elemProp != null)
                    {
                        EditorGUI.PropertyField(lineRect, elemProp, new GUIContent($"Element [{xi}]"), includeChildren: true);
                    }
                    else
                    {
                        // Fallback to reflection-based drawer if SerializedProperty is unavailable
                        DrawElementField(new Rect(lineRect.x, lineRect.y, lineRect.width, Line), property, gridObj, gridType, t, xi, row);
                    }

                    rowY += elemHeight + VSpace;
                }

                // Advance r.y below the section
                r.y = sectionRect.yMax + VSpace;
            }
            else
            {
                // Provide context menu even for empty grid structure (width/height zero) on header
            }

            EditorGUI.EndProperty();
        }

        private static void DrawElementField(Rect r, SerializedProperty property, object gridObj, Type gridType, Type elemType, int x, int y)
        {
            // Read current value via TryGet to avoid ref-return reflection limitations.
            if (!TryGetCell(gridObj, gridType, elemType, x, y, out var current))
            {
                EditorGUI.LabelField(r, $"[{x}]", "<err>");
                return;
            }

            EditorGUI.BeginChangeCheck();
            object newValue = current;

            // Decide which field to draw based on element type
            if (elemType == typeof(int))
            {
                int v = current is int iv ? iv : 0;
                v = EditorGUI.IntField(r, $"Element [{x}]", v);
                newValue = v;
            }
            else if (elemType == typeof(float))
            {
                float v = current is float fv ? fv : 0f;
                v = EditorGUI.FloatField(r, $"Element [{x}]", v);
                newValue = v;
            }
            else if (elemType == typeof(double))
            {
                double v = current is double dv ? dv : 0d;
                v = EditorGUI.DoubleField(r, $"Element [{x}]", v);
                newValue = v;
            }
            else if (elemType == typeof(bool))
            {
                bool v = current is bool bv && bv;
                v = EditorGUI.ToggleLeft(r, $"Element [{x}]", v);
                newValue = v;
            }
            else if (elemType == typeof(string))
            {
                string v = current as string ?? string.Empty;
                v = EditorGUI.TextField(r, $"Element [{x}]", v);
                newValue = v;
            }
            else if (elemType.IsEnum)
            {
                var v = current ?? Activator.CreateInstance(elemType);
                v = EditorGUI.EnumPopup(r, $"Element [{x}]", (Enum)v);
                newValue = v;
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(elemType))
            {
                var obj = current as UnityEngine.Object;
                var result = EditorGUI.ObjectField(r, $"Element [{x}]", obj, elemType, allowSceneObjects: true);
                newValue = result;
            }
            else
            {
                // Unknown type: show read-only ToString
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.TextField(r, $"Element [{x}]", current != null ? current.ToString() : "null");
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (!TrySetCell(gridObj, gridType, elemType, x, y, newValue))
                {
                    Debug.LogWarning($"Grid drawer: failed to set value at ({x},{y})");
                }
                property.serializedObject.Update();
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private static bool TryGetCell(object grid, Type gridType, Type elemType, int x, int y, out object value)
        {
            var mi = gridType.GetMethod("TryGet", BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(int), typeof(int), elemType.MakeByRefType() }, null);
            if (mi == null)
            {
                value = null; return false;
            }
            object[] args = new object[] { x, y, GetDefault(elemType) };
            bool ok = (bool)mi.Invoke(grid, args);
            value = args[2];
            return ok;
        }

        private static bool TrySetCell(object grid, Type gridType, Type elemType, int x, int y, object value)
        {
            var mi = gridType.GetMethod("TrySet", BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(int), typeof(int), elemType.MakeByRefType() }, null);
            if (mi == null) return false;
            object coerced = Coerce(value, elemType);
            object[] args = new object[] { x, y, coerced };
            return (bool)mi.Invoke(grid, args);
        }

        private static object CreateResizedGrid(object oldGrid, Type gridType, int newW, int newH)
        {
            try
            {
                var ctor = gridType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int), typeof(int) }, null);
                if (ctor == null) return null;
                var newGrid = ctor.Invoke(new object[] { newW, newH });

                // Copy overlap
                var widthProp = gridType.GetProperty("Width");
                var heightProp = gridType.GetProperty("Height");
                int oldW = (int)widthProp.GetValue(oldGrid);
                int oldH = (int)heightProp.GetValue(oldGrid);
                int w = Math.Min(oldW, newW);
                int h = Math.Min(oldH, newH);
                var elemType = gridType.GetGenericArguments()[0];

                for (int yy = 0; yy < h; yy++)
                {
                    for (int xx = 0; xx < w; xx++)
                    {
                        if (TryGetCell(oldGrid, gridType, elemType, xx, yy, out var val))
                            TrySetCell(newGrid, gridType, elemType, xx, yy, val);
                    }
                }
                return newGrid;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private static object GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;

        private static object Coerce(object value, Type targetType)
        {
            if (value == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            var vType = value.GetType();
            if (targetType.IsAssignableFrom(vType)) return value;
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
        }
    }
}
#endif
