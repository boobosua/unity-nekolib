#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NekoLib
{
    // Central watcher to notify registered callbacks when the main editor toolbar layout width changes.
    // Does NOT alter any styling or positioning logic; tools re-run their own existing PositionContainer.
    [InitializeOnLoad]
    internal static class ToolbarLayoutWatcher
    {
        private static VisualElement toolbarRoot;
        private static readonly List<Action> callbacks = new List<Action>(8);
        private static float lastWidth = -1f;
        private static double lastInvokeTime;
        private const double MinInterval = 0.02d; // throttle rapid duplicate events

        static ToolbarLayoutWatcher()
        {
            EditorApplication.delayCall += TryHook;
        }

        private static void TryHook()
        {
            if (toolbarRoot != null) return;
            toolbarRoot = FindToolbarRoot();
            if (toolbarRoot == null)
            {
                // Retry next editor tick until toolbar exists
                EditorApplication.delayCall += TryHook;
                return;
            }
            toolbarRoot.RegisterCallback<GeometryChangedEvent>(OnToolbarGeometryChanged);
            // Initial invoke so tools position immediately once width known
            ForceInvoke();
        }

        private static VisualElement FindToolbarRoot()
        {
            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null) return null;
            var instances = Resources.FindObjectsOfTypeAll(toolbarType);
            if (instances == null || instances.Length == 0) return null;
            object inst = instances[0];
            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance) ?? toolbarType.GetField("m_RootVisualElement", BindingFlags.NonPublic | BindingFlags.Instance);
            return rootField?.GetValue(inst) as VisualElement;
        }

        private static void OnToolbarGeometryChanged(GeometryChangedEvent evt)
        {
            if (toolbarRoot == null) return;
            float w = toolbarRoot.resolvedStyle.width;
            if (Mathf.Approximately(w, lastWidth)) return;
            lastWidth = w;
            // Throttle excessive invocations in rapid resize scenarios
            double now = EditorApplication.timeSinceStartup;
            if (now - lastInvokeTime < MinInterval) return;
            lastInvokeTime = now;
            InvokeAll();
        }

        private static void InvokeAll()
        {
            for (int i = 0; i < callbacks.Count; i++)
            {
                try { callbacks[i]?.Invoke(); } catch { /* swallow to avoid breaking chain */ }
            }
        }

        internal static void ForceInvoke()
        {
            if (toolbarRoot == null) return;
            lastWidth = toolbarRoot.resolvedStyle.width;
            InvokeAll();
        }

        internal static void Register(Action reposition)
        {
            if (reposition == null) return;
            if (!callbacks.Contains(reposition)) callbacks.Add(reposition);
            // Attempt immediate positioning (toolbar may still be null early; we'll retry once hooked)
            if (toolbarRoot != null)
            {
                try { reposition(); } catch { }
            }
            else
            {
                TryHook();
            }
        }
    }
}
#endif
