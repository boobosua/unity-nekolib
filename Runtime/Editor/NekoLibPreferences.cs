#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    internal static class NekoLibPreferences
    {
        private const string Root = "NekoLib";
        private const string KeySceneSwitcher = Root + ":SceneSwitcherEnabled";
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

                    GUILayout.Space(6);
                    EditorGUILayout.HelpBox("Controls whether the Scene Switcher appears in the Unity main toolbar.", MessageType.Info);
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
