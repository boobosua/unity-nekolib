#if UNITY_EDITOR && !ODIN_INSPECTOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    [CustomEditor(typeof(TmpFontReplacerSettings))]
    internal class TmpFontReplacerSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                DrawProp("lastFont",        new GUIContent("Last Font"));
                DrawProp("lastMaterial",    new GUIContent("Last Material"));
                DrawProp("excludedFolders", new GUIContent("Excluded Folders"));
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