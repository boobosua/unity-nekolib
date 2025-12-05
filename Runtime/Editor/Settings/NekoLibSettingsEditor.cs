#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    [CustomEditor(typeof(NekoLibSettings))]
    internal class NekoLibSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            so.Update();

            // Explicitly render only the core settings fields as read-only, hide script
            using (new EditorGUI.DisabledScope(true))
            {
                DrawProp("startupScenePath", new GUIContent("Startup Scene Path"));
                DrawProp("activateLoadedAdditiveOnSelect", new GUIContent("Activate Loaded Additives"));
                DrawProp("timeScaleMax", new GUIContent("Time Scale Max"));
                DrawProp("hideToolbar", new GUIContent("Hide Toolbar"));
                DrawProp("autoReenterPlayAfterClear", new GUIContent("Auto Reenter Play After Clear"));
            }
        }

        private void DrawProp(string name, GUIContent label)
        {
            var p = serializedObject.FindProperty(name);
            if (p != null)
            {
                EditorGUILayout.PropertyField(p, label, true);
            }
        }
    }
}
#endif
