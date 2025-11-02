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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // When null, we only show the size inputs and Create button line
            if (property.managedReferenceValue == null)
                return Line;

            // Prefer SerializedProperty to compute UI height accurately (especially for serializable classes)
            var widthSP = property.FindPropertyRelative("_width");
            var heightSP = property.FindPropertyRelative("_height");
            var dataSP = property.FindPropertyRelative("_data");
            if (widthSP == null || heightSP == null)
                return Line;

            int width = widthSP.intValue;
            int height = heightSP.intValue;

            // Always at least the size line.
            float total = Line;

            bool hasRows = width > 0 && height > 0;
            if (!hasRows)
                return total; // no pager/list when empty

            // Pager height
            float pagerBlock = Line + (VSpace * 2);

            // Elements block: sum element property heights for the selected row
            float elementsBlock = CalcElementsBlockHeight(property, dataSP, width, height);

            float section = (SectionPad * 2) + pagerBlock + elementsBlock;
            total += VSpace + section;
            return total;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var gridObj = property.managedReferenceValue;
            if (gridObj == null)
            {
                // Allow creating a new grid when null: [width] [height] [Create]
                var content = EditorGUI.PrefixLabel(position, label);
                string key = property.propertyPath;
                if (!PendingDims.TryGetValue(key, out var dims)) dims = new Vector2Int(1, 1);

                float btnW = 70f;
                float halfN = (content.width - 8f - btnW);
                halfN *= 0.5f;
                var wRectN = new Rect(content.x, content.y, halfN, Line);
                var hRectN = new Rect(wRectN.xMax + 4f, content.y, halfN, Line);
                var bRect = new Rect(hRectN.xMax + 8f, content.y, btnW, Line);

                dims.x = Mathf.Max(1, EditorGUI.IntField(wRectN, dims.x));
                dims.y = Mathf.Max(1, EditorGUI.IntField(hRectN, dims.y));
                PendingDims[key] = dims;

                if (GUI.Button(bRect, "Create"))
                {
                    var tN = fieldInfo.FieldType; // closed generic Grid<T>
                    var ctor = tN.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int), typeof(int) }, null);
                    if (ctor != null)
                    {
                        var newGrid = ctor.Invoke(new object[] { dims.x, dims.y });
                        Undo.RecordObject(property.serializedObject.targetObject, "Create Grid");
                        property.serializedObject.Update();
                        property.managedReferenceValue = newGrid;
                        property.serializedObject.ApplyModifiedProperties();
                        PendingDims.Remove(key);
                    }
                }

                EditorGUI.EndProperty();
                return;
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

            // First line: label + Vector2Int-style [Width] [Height]
            var r = new Rect(position.x, position.y, position.width, Line);
            var contentRect = EditorGUI.PrefixLabel(r, label);
            var half = (contentRect.width - 4f) * 0.5f;
            var wRect = new Rect(contentRect.x, contentRect.y, half, Line);
            var hRect = new Rect(contentRect.x + half + 4f, contentRect.y, half, Line);

            EditorGUI.BeginChangeCheck();
            int newW = Mathf.Max(1, EditorGUI.IntField(wRect, width));
            int newH = Mathf.Max(1, EditorGUI.IntField(hRect, height));
            if (EditorGUI.EndChangeCheck())
            {
                var newGrid = CreateResizedGrid(gridObj, gridType, newW, newH);
                if (newGrid != null)
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Resize Grid");
                    property.serializedObject.Update();
                    property.managedReferenceValue = newGrid;
                    property.serializedObject.ApplyModifiedProperties();

                    gridObj = newGrid;
                    widthSP = property.FindPropertyRelative("_width");
                    heightSP = property.FindPropertyRelative("_height");
                    dataSP = property.FindPropertyRelative("_data");
                    width = newW; height = newH;
                }
            }
            r.y += Line + VSpace;

            // Pager + list only when grid has rows
            if (width > 0 && height > 0)
            {
                // Calculate section rect (help-box style) using dynamic element heights
                string key = property.propertyPath;
                if (!RowPage.TryGetValue(key, out int rowForHeight)) rowForHeight = 0;
                rowForHeight = Mathf.Clamp(rowForHeight, 0, Math.Max(0, height - 1));
                float pagerBlockH = Line + (VSpace * 2);
                float elementsBlockH = CalcElementsBlockHeight(property, dataSP, width, height);
                float sectionHeight = (SectionPad * 2) + pagerBlockH + elementsBlockH;

                var sectionRect = new Rect(position.x, r.y, position.width, sectionHeight);
                GUI.Box(sectionRect, GUIContent.none, EditorStyles.helpBox);

                // Inner content rect
                var inner = new Rect(sectionRect.x + SectionPad, sectionRect.y + SectionPad, sectionRect.width - (SectionPad * 2), sectionRect.height - (SectionPad * 2));

                // Pager with small spacing above/below
                var pager = inner;
                pager.height = Line;

                // Persist and clamp row
                if (!RowPage.TryGetValue(key, out int row)) row = 0;
                row = Mathf.Clamp(row, 0, Math.Max(0, height - 1));

                // Center Prev | Row x/y | Next
                var labelText = $"Row {row + 1}/{Math.Max(1, height)}";
                float labelW = GUI.skin.label.CalcSize(new GUIContent(labelText)).x + 8f;
                float btnW = 60f;
                float totalW = btnW + 8f + labelW + 8f + btnW;
                float startX = pager.x + (pager.width - totalW) * 0.5f;

                var prevRect = new Rect(startX, pager.y, btnW, Line);
                var infoRect = new Rect(prevRect.xMax + 8f, pager.y, labelW, Line);
                var nextRect = new Rect(infoRect.xMax + 8f, pager.y, btnW, Line);

                if (GUI.Button(prevRect, "Prev")) row = Mathf.Max(0, row - 1);
                GUI.Label(infoRect, labelText, EditorStyles.label);
                if (GUI.Button(nextRect, "Next")) row = Mathf.Min(Math.Max(0, height - 1), row + 1);
                RowPage[key] = row;

                // Move below pager with tiny spacing
                var listRect = inner;
                listRect.y = pager.y + Line + VSpace;
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
                        // If this is a non-Unity class element and it's currently null, offer a small inline "New" button
                        if (t.IsClass && !typeof(UnityEngine.Object).IsAssignableFrom(t) && (currentVal == null))
                        {
                            var btnRect = new Rect(lineRect.xMax - 54f, lineRect.y, 50f, Line);
                            if (GUI.Button(btnRect, "New"))
                            {
                                try
                                {
                                    var inst = Activator.CreateInstance(t);
                                    if (TrySetCell(gridObj, gridType, t, xi, row, inst))
                                    {
                                        Undo.RecordObject(property.serializedObject.targetObject, "Create Element");
                                        property.serializedObject.Update();
                                        property.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogException(e);
                                }
                            }
                        }
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
