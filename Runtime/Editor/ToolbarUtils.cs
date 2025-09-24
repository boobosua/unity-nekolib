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
    }
}
#endif
