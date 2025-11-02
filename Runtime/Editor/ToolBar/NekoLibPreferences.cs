#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    internal static class NekoLibPreferences
    {
        private const string Root = ToolbarUtils.PrefKeys.PreferencesRoot;
        private const string KeyActivateLoadedAdditive = ToolbarUtils.PrefKeys.ActivateLoadedAdditive;
        private const string KeyHideToolbar = ToolbarUtils.PrefKeys.HideToolbar;
        private const string KeyTimeScaleMax = ToolbarUtils.PrefKeys.TimeScaleMax;
        private const string KeyAutoReenterPlayAfterClear = ToolbarUtils.PrefKeys.AutoReenterPlayAfterClear;


        internal static bool ActivateLoadedAdditiveOnSelect
        {
            get => EditorPrefs.GetBool(KeyActivateLoadedAdditive, false);
            set => EditorPrefs.SetBool(KeyActivateLoadedAdditive, value);
        }

        internal static int TimeScaleMax
        {
            get => EditorPrefs.GetInt(KeyTimeScaleMax, 10);
            set => EditorPrefs.SetInt(KeyTimeScaleMax, Mathf.Clamp(value, 10, 100));
        }

        internal static bool HideToolbar
        {
            // Default to false (toolbar visible) per revised request
            get => EditorPrefs.GetBool(KeyHideToolbar, false);
            set
            {
                if (value == HideToolbar) return;
                EditorPrefs.SetBool(KeyHideToolbar, value);
                // When HideToolbar is true we should disable individual toolbar features.
                // Apply the inverse because existing ApplyPreferenceChange expects "enabled" semantics.
                bool enabled = !value;
                SceneSwitcherToolbar.ApplyPreferenceChange(enabled);
                TimeScaleTool.ApplyPreferenceChange(enabled);
                try { ClearPlayerPrefsTool.ApplyPreferenceChange(enabled); } catch { }
            }
        }

        internal static bool AutoReenterPlayAfterClear
        {
            get => EditorPrefs.GetBool(KeyAutoReenterPlayAfterClear, true);
            set => EditorPrefs.SetBool(KeyAutoReenterPlayAfterClear, value);
        }


        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Preferences/" + Root, SettingsScope.User)
            {
                label = "NekoLib",
                guiHandler = ctx =>
                {
                    const float desiredLabelWidth = 300f; // wide enough for full text
                    float oldLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = desiredLabelWidth;

                    EditorGUILayout.BeginVertical("box");
                    // Top-level: hide toolbar toggle (shows/hides other toolbar settings)
                    EditorGUI.BeginChangeCheck();
                    var toggleHideToolbar = new GUIContent("Hide NekoLib toolbar", "Hide all NekoLib toolbar controls (Scene Switcher, Time Scale, Clear PlayerPrefs). Default: Off.");
                    bool ht = EditorGUILayout.Toggle(toggleHideToolbar, HideToolbar);
                    if (EditorGUI.EndChangeCheck()) HideToolbar = ht;

                    // If toolbar is hidden, don't show other toolbar-related options
                    if (!HideToolbar)
                    {
                        // SceneSwitcher is part of the global toolbar and is controlled by "Hide NekoLib toolbar".

                        EditorGUI.BeginChangeCheck();
                        var toggleActivateAdditive = new GUIContent("Prefer Activating Loaded Additives", "If the selected scene is already loaded additively, make it the active scene instead of reopening (preserves other loaded scenes).");
                        bool act = EditorGUILayout.Toggle(toggleActivateAdditive, ActivateLoadedAdditiveOnSelect);
                        if (EditorGUI.EndChangeCheck()) ActivateLoadedAdditiveOnSelect = act;

                        EditorGUI.BeginChangeCheck();
                        var labelMaxTime = new GUIContent("Max Time Scale", "Maximum value for the Time Scale slider (10–100). Default: 10.");
                        int maxVal = EditorGUILayout.IntSlider(labelMaxTime, TimeScaleMax, 10, 100);
                        if (EditorGUI.EndChangeCheck()) TimeScaleMax = maxVal;

                        EditorGUI.BeginChangeCheck();
                        var toggleAutoReenter = new GUIContent("Auto re-enter Play Mode after clearing prefs", "After clearing PlayerPrefs, automatically enter Play Mode if you were previously playing or if you requested play.");
                        bool ar = EditorGUILayout.Toggle(toggleAutoReenter, AutoReenterPlayAfterClear);
                        if (EditorGUI.EndChangeCheck()) AutoReenterPlayAfterClear = ar;
                    }

                    GUILayout.Space(6);
                    // Tooltips are provided on each control; keep the help area minimal.
                    // EditorGUILayout.HelpBox("Tooltips on each setting provide contextual help.", MessageType.Info);
                    EditorGUILayout.EndVertical();

                    EditorGUIUtility.labelWidth = oldLabelWidth; // restore
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "NekoLib", "Scene", "Switcher", "Toolbar", "Hide" })
            };
            return provider;
        }
    }
}
#endif
