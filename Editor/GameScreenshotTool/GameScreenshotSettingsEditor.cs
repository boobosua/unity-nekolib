#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    [CustomEditor(typeof(GameScreenshotSettings))]
    internal class GameScreenshotSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Edit these settings via Tools \u2192 Neko Framework \u2192 Screenshot \u2192 Open Settings.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(true))
            {
                DrawProp("captureMode", new GUIContent("Capture Mode"));
                DrawProp("supersize", new GUIContent("Supersize"));
                DrawProp("saveFolder", new GUIContent("Save Folder"));
                DrawProp("revealOnSave", new GUIContent("Reveal On Save"));
            }
        }

        private void DrawProp(string propName, GUIContent label)
        {
            var p = serializedObject.FindProperty(propName);
            if (p != null)
            {
                EditorGUILayout.PropertyField(p, label, true);
            }
        }
    }
}
#endif
