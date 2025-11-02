#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NekoLib
{
    internal static class ToolbarUtils
    {
        // Standard horizontal spacing between adjacent toolbar controls
        public const float AfterControlSpacing = 15f;

        public static class PrefKeys
        {
            public const string PreferencesRoot = "NekoLib";
            public const string HideToolbar = PreferencesRoot + ":HideToolbar";
            public const string ActivateLoadedAdditive = PreferencesRoot + ":ActivateLoadedAdditiveOnSelect";
            public const string TimeScaleMax = PreferencesRoot + ":TimeScaleMax";
            // TimeScaleToolEnabled removed: TimeScale is now controlled by global HideToolbar
            public const string AutoReenterPlayAfterClear = PreferencesRoot + ":AutoReenterPlayAfterClear";
        }

        public static VisualElement FindByName(VisualElement root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindByName(root[i], name);
                if (found != null) return found;
            }
            return null;
        }

        public static VisualElement GetToolbarRoot()
        {
            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null) return null;
            var instances = Resources.FindObjectsOfTypeAll(toolbarType);
            if (instances == null || instances.Length == 0) return null;
            object toolbarInstance = instances[0];
            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance) ??
                            toolbarType.GetField("m_RootVisualElement", BindingFlags.NonPublic | BindingFlags.Instance);
            return rootField?.GetValue(toolbarInstance) as VisualElement;
        }

        public static bool LooksLikeToolbarButton(VisualElement ve)
        {
            var r = ve.layout;
            return r.width >= 14 && r.width <= 55 && r.height >= 14 && r.height <= 40;
        }

        public static float GetWorldX(VisualElement target, VisualElement root)
        {
            float x = 0f; var c = target;
            while (c != null && c != root) { x += c.layout.x; c = c.parent; }
            return x;
        }

        public static float GetWorldY(VisualElement target, VisualElement root)
        {
            float y = 0f; var c = target;
            while (c != null && c != root) { y += c.layout.y; c = c.parent; }
            return y;
        }

        public static void ApplyRoundedStyling(VisualElement ve)
        {
#if UNITY_2022_1_OR_NEWER
            int r = 6;
            ve.style.borderTopLeftRadius = r;
            ve.style.borderTopRightRadius = r;
            ve.style.borderBottomLeftRadius = r;
            ve.style.borderBottomRightRadius = r;
            ve.style.paddingLeft = 4;
            ve.style.paddingRight = 4;
#else
            ve.style.paddingLeft = 3;
            ve.style.paddingRight = 3;
#endif
        }

        public static void TryRegisterLayoutWatcher(Action cb)
        {
            try
            {
                var watcherType = Type.GetType("NekoLib.ToolbarLayoutWatcher, Assembly-CSharp-Editor") ??
                                Type.GetType("NekoLib.ToolbarLayoutWatcher");
                if (watcherType == null) return;
                var register = watcherType.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (register == null) return;
                register.Invoke(null, new object[] { cb });
            }
            catch { }
        }

        public static VisualElement FindPlayControlsLeftMost(VisualElement root) => FindPlayControlsRecursive(root, 0, true);
        public static VisualElement FindPlayControlsRightMost(VisualElement root) => FindPlayControlsRecursive(root, 0, false);

        private static VisualElement FindPlayControlsRecursive(VisualElement ve, int depth, bool leftMost)
        {
            if (ve == null || depth > 6) return null;
            int buttons = 0;
            for (int i = 0; i < ve.childCount; i++) if (LooksLikeToolbarButton(ve[i])) buttons++;
            if (buttons >= 3)
            {
                if (ve.childCount == 0) return ve;
                return leftMost ? ve[0] : ve[ve.childCount - 1];
            }
            for (int i = 0; i < ve.childCount; i++)
            {
                var found = FindPlayControlsRecursive(ve[i], depth + 1, leftMost);
                if (found != null) return found;
            }
            return null;
        }

        // Icon helpers
        public static Texture2D GetBestIcon(params string[] names)
        {
            Texture2D best = null;
            int bestArea = 0;
            foreach (var n in names)
            {
                if (string.IsNullOrEmpty(n)) continue;
                var tex = EditorGUIUtility.IconContent(n).image as Texture2D;
                if (tex == null) continue;
                int area = tex.width * tex.height;
                if (area > bestArea)
                {
                    best = tex; bestArea = area;
                }
            }
            return best;
        }

        public static int ComputeCrispIconSize(float buttonHeight, Texture2D tex, float padding = 4f)
        {
            float fallback = Mathf.Max(12f, buttonHeight - padding);
            if (tex == null) return Mathf.RoundToInt(fallback);
            int native = Mathf.Max(tex.width, tex.height);
            float target = Mathf.Min(buttonHeight - padding, native);
            return Mathf.RoundToInt(target);
        }
    }
}
#endif
