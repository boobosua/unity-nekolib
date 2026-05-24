#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TRnK.Toolkit
{
    [CustomEditor(typeof(TRnKSettings))]
    internal class TRnKSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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
                EditorGUILayout.PropertyField(p, label, true);
        }
    }
}
#endif
