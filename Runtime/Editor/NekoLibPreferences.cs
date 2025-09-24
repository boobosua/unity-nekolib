#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    internal static class NekoLibPreferences
    {
        private const string Root = ToolbarUtils.PrefKeys.PreferencesRoot;
        private const string KeySceneSwitcher = ToolbarUtils.PrefKeys.SceneSwitcherEnabled;
        private const string KeyActivateLoadedAdditive = ToolbarUtils.PrefKeys.ActivateLoadedAdditive;
        private const string KeyTimeScaleTool = ToolbarUtils.PrefKeys.TimeScaleToolEnabled;
        private const string KeyAutoReenterPlayAfterClear = ToolbarUtils.PrefKeys.AutoReenterPlayAfterClear;
        internal static bool SceneSwitcherEnabled
        {
            get => EditorPrefs.GetBool(KeySceneSwitcher, true);
            set
            {
                if (value == SceneSwitcherEnabled) return;
                EditorPrefs.SetBool(KeySceneSwitcher, value);
                SceneSwitcherToolbar.ApplyPreferenceChange(value);
            }
        }

        internal static bool ActivateLoadedAdditiveOnSelect
        {
            get => EditorPrefs.GetBool(KeyActivateLoadedAdditive, false);
            set => EditorPrefs.SetBool(KeyActivateLoadedAdditive, value);
        }

        internal static bool TimeScaleToolEnabled
        {
            get => EditorPrefs.GetBool(KeyTimeScaleTool, true);
            set
            {
                if (value == TimeScaleToolEnabled) return;
                EditorPrefs.SetBool(KeyTimeScaleTool, value);
                TimeScaleTool.ApplyPreferenceChange(value);
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
                    EditorGUI.BeginChangeCheck();
                    var toggleSceneSwitcher = new GUIContent("Enable Scene Switcher Toolbar", "Show the scene switcher dropdown in the main toolbar.");
                    bool ss = EditorGUILayout.Toggle(toggleSceneSwitcher, SceneSwitcherEnabled);
                    if (EditorGUI.EndChangeCheck()) SceneSwitcherEnabled = ss;

                    EditorGUI.BeginChangeCheck();
                    var toggleActivateAdditive = new GUIContent("Prefer Activating Loaded Additives", "If the selected scene is already loaded additively, make it the active scene instead of reopening (preserves other loaded scenes).");
                    bool act = EditorGUILayout.Toggle(toggleActivateAdditive, ActivateLoadedAdditiveOnSelect);
                    if (EditorGUI.EndChangeCheck()) ActivateLoadedAdditiveOnSelect = act;

                    EditorGUI.BeginChangeCheck();
                    var toggleTimeScale = new GUIContent("Enable Time Scale Toolbar", "Show the time scale slider + reset control in the main toolbar.");
                    bool ts = EditorGUILayout.Toggle(toggleTimeScale, TimeScaleToolEnabled);
                    if (EditorGUI.EndChangeCheck()) TimeScaleToolEnabled = ts;

                    EditorGUI.BeginChangeCheck();
                    var toggleAutoReenter = new GUIContent("Auto re-enter Play Mode after clearing prefs", "After clearing PlayerPrefs, automatically enter Play Mode if you were previously playing or if you requested play.");
                    bool ar = EditorGUILayout.Toggle(toggleAutoReenter, AutoReenterPlayAfterClear);
                    if (EditorGUI.EndChangeCheck()) AutoReenterPlayAfterClear = ar;

                    GUILayout.Space(6);
                    EditorGUILayout.HelpBox("Enable Scene Switcher: Shows/hides scene dropdown.\nPrefer Activating Loaded Additives: Selecting a loaded additive scene just activates it.\nEnable Time Scale Toolbar: Shows/hides time scale slider.\nAuto re-enter Play Mode: After clearing PlayerPrefs, automatically re-enter Play Mode (default on).", MessageType.Info);
                    EditorGUILayout.EndVertical();

                    EditorGUIUtility.labelWidth = oldLabelWidth; // restore
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "NekoLib", "Scene", "Switcher", "Toolbar" })
            };
            return provider;
        }
    }
}
#endif
