#if UNITY_EDITOR && !ODIN_INSPECTOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    [CustomEditor(typeof(SceneSwitcherSettings))]
    internal class SceneSwitcherSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                var p = serializedObject.FindProperty("groups");
                if (p != null)
                    EditorGUILayout.PropertyField(p, new GUIContent("Groups"), true);
            }
        }
    }
}
#endif