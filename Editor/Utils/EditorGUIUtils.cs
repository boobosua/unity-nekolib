#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace TRnK.Toolkit
{
    public static class EditorGUIUtils
    {
        public static void DrawHorizontalSeparator()
            => DrawHorizontalSeparator(EditorGUIUtility.isProSkin
                ? new Color(0.13f, 0.13f, 0.13f)
                : new Color(0.6f, 0.6f, 0.6f));

        public static void DrawHorizontalSeparator(Color color)
        {
            EditorGUILayout.Space(2);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f, GUILayout.ExpandWidth(true)), color);
            EditorGUILayout.Space(2);
        }
    }

    public readonly struct LabelWidthScope : IDisposable
    {
        private readonly float _previous;

        public LabelWidthScope(float width)
        {
            _previous = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
        }

        public void Dispose() => EditorGUIUtility.labelWidth = _previous;
    }
}
#endif
