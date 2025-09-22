#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    internal static class NekoLibPreferences
    {
        private const string Root = "NekoLib";
        private const string KeySceneSwitcher = Root + ":SceneSwitcherEnabled";
        private const string KeyActivateLoadedAdditive = Root + ":ActivateLoadedAdditiveOnSelect";
        private const string KeyTimeScaleTool = Root + ":TimeScaleToolEnabled";
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


        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Preferences/" + Root, SettingsScope.User)
            {
                label = "NekoLib",
                guiHandler = ctx =>
                {
                    const float desiredLabelWidth = 210f; // wide enough for full text
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

                    GUILayout.Space(6);
                    EditorGUILayout.HelpBox("Enable Scene Switcher: Shows/hides scene dropdown.\nPrefer Activating Loaded Additives: Selecting a loaded additive scene just activates it.\nEnable Time Scale Toolbar: Shows/hides time scale slider.", MessageType.Info);
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
