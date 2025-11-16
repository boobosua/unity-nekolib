#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib.Core
{
    /// <summary>
    /// Unified tab bar drawing helper for Neko editor windows.
    /// Matches reference: flat horizontal bar, blue selected tab, gray others.
    /// </summary>
    public static class NekoEditorTabBar
    {
        /// <summary>
        /// Draws a styled tab bar and returns the (possibly) updated selected index.
        /// </summary>
        public static int Draw(int selectedIndex, string[] tabs, float height = 26f)
        {
            if (tabs == null || tabs.Length == 0) return selectedIndex;

            // Reserve a single rect for the whole tab bar
            Rect fullRect = EditorGUILayout.GetControlRect(false, height + 10f, GUILayout.ExpandWidth(true));

            // No custom background: use window's native background to match reference
            Color border = EditorGUIUtility.isProSkin ? new Color(0.28f, 0.28f, 0.28f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f);

            float padX = 10f;
            // Vertical center: place tabs in the middle of the reserved strip
            float padY = Mathf.Floor((fullRect.height - height) * 0.5f);
            float spacing = 0f; // zero gap between tabs per spec

            // Measure desired widths
            var textStyleProbe = new GUIStyle(EditorStyles.miniBoldLabel) { fontSize = 11, alignment = TextAnchor.MiddleCenter };
            float available = Mathf.Max(0f, fullRect.width - padX * 2f);
            float minWidth = 110f;
            float maxWidth = 180f;
            // Prefer equal widths like the reference
            float perTab = Mathf.Clamp((available - spacing * (tabs.Length - 1)) / Mathf.Max(1, tabs.Length), minWidth, maxWidth);
            float totalGroup = perTab * tabs.Length + spacing * (tabs.Length - 1);
            // Center the whole group
            float leftOffset = (fullRect.width - totalGroup) * 0.5f;
            float startX = Mathf.Floor(fullRect.x + Mathf.Max(padX, leftOffset));

            // Draw buttons
            float x = startX;
            for (int i = 0; i < tabs.Length; i++)
            {
                bool isSelected = i == selectedIndex;
                float w = perTab;
                var btnRect = new Rect(x, fullRect.y + padY, w, height);

                // Colors
                Color selectedBg = EditorGUIUtility.isProSkin ? new Color(0.23f, 0.45f, 0.77f, 1f) : new Color(0.32f, 0.54f, 0.88f, 1f);
                Color unselectedBg = EditorGUIUtility.isProSkin ? new Color(0.30f, 0.30f, 0.30f, 1f) : new Color(0.92f, 0.92f, 0.92f, 1f);
                Color selectedText = Color.white;
                Color unselectedText = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f, 1f) : new Color(0.25f, 0.25f, 0.25f, 1f);

                // Background
                EditorGUI.DrawRect(btnRect, isSelected ? selectedBg : unselectedBg);
                // No extra highlights; keep clean blocks per reference
                // Borders (avoid double-thick separators by drawing left on first, right on all)
                EditorGUI.DrawRect(new Rect(btnRect.x, btnRect.y, btnRect.width, 1), border); // top
                EditorGUI.DrawRect(new Rect(btnRect.x, btnRect.yMax - 1, btnRect.width, 1), border); // bottom
                if (i == 0)
                    EditorGUI.DrawRect(new Rect(btnRect.x, btnRect.y, 1, btnRect.height), border); // left on first
                EditorGUI.DrawRect(new Rect(btnRect.xMax - 1, btnRect.y, 1, btnRect.height), border); // right on all

                // Text
                var textStyle = new GUIStyle(textStyleProbe)
                {
                    normal = { textColor = isSelected ? selectedText : unselectedText },
                    hover = { textColor = isSelected ? selectedText : unselectedText },
                    active = { textColor = selectedText },
                    fontStyle = FontStyle.Bold
                };

                if (GUI.Button(btnRect, tabs[i], textStyle))
                {
                    selectedIndex = i;
                }

                x += w + spacing;
            }

            return selectedIndex;
        }
    }
}
#endif
