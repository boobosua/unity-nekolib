#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    internal static class NekoLibProjectSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Project/NekoLib", SettingsScope.Project)
            {
                label = "NekoLib",
                guiHandler = _ =>
                {
                    var settings = NekoLibSettings.GetOrCreate();

                    const float desiredLabelWidth = 300f;
                    float oldLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = desiredLabelWidth;

                    EditorGUILayout.BeginVertical("box");

                    EditorGUI.BeginChangeCheck();
                    bool hideToolbar = EditorGUILayout.Toggle(new GUIContent("Hide NekoLib toolbar", "Hide all NekoLib toolbar controls (Scene Switcher, Time Scale, Clear PlayerPrefs)."), settings.hideToolbar);
                    if (EditorGUI.EndChangeCheck())
                    {
                        settings.hideToolbar = hideToolbar;
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                        bool enabled = !hideToolbar;
                        SceneSwitcherToolbar.ApplyPreferenceChange(enabled);
                        TimeScaleTool.ApplyPreferenceChange(enabled);
                        try { ClearPlayerPrefsTool.ApplyPreferenceChange(enabled); } catch { }
                    }

                    if (!settings.hideToolbar)
                    {
                        EditorGUI.BeginChangeCheck();
                        bool act = EditorGUILayout.Toggle(new GUIContent("Prefer Activating Loaded Additives", "If selected scene is already loaded additively, make it active instead of reopening."), settings.activateLoadedAdditiveOnSelect);
                        if (EditorGUI.EndChangeCheck()) { settings.activateLoadedAdditiveOnSelect = act; EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets(); }

                        EditorGUI.BeginChangeCheck();
                        int maxVal = EditorGUILayout.IntSlider(new GUIContent("Max Time Scale", "Maximum value for the Time Scale slider (10–100)."), Mathf.Clamp(settings.timeScaleMax, 10, 100), 10, 100);
                        if (EditorGUI.EndChangeCheck())
                        {
                            settings.timeScaleMax = maxVal;
                            EditorUtility.SetDirty(settings);
                            // Avoid stutter: don't SaveAssets on every drag; just refresh tool instantly
                            TimeScaleTool.RefreshFromSettings();
                        }

                        EditorGUI.BeginChangeCheck();
                        bool ar = EditorGUILayout.Toggle(new GUIContent("Auto re-enter Play Mode after clearing prefs", "After clearing PlayerPrefs, automatically enter Play Mode if previously playing or explicitly requested."), settings.autoReenterPlayAfterClear);
                        if (EditorGUI.EndChangeCheck()) { settings.autoReenterPlayAfterClear = ar; EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets(); }
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUIUtility.labelWidth = oldLabelWidth;
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "NekoLib", "Scene", "Toolbar", "TimeScale", "PlayerPrefs" })
            };
            return provider;
        }
    }
}
#endif