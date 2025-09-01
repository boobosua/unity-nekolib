#if UNITY_EDITOR
using UnityEditor;

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScrollingRawImage))]
    public class ScrollingRawImageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();

            // Info box with setup instructions
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Set texture Wrap Mode to 'Repeat' in import settings",
                MessageType.Info);
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScrollingImage))]
    public class ScrollingImageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();

            // Info box with setup instructions
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "• Use UI/Default material for proper texture offset handling\n" +
                "• Set Image Type to 'Simple'\n" +
                "• Set sprite texture Wrap Mode to 'Repeat' in import settings",
                MessageType.Info);
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScrollingSpriteRenderer))]
    public class ScrollingSpriteRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();

            // Info box with setup instructions
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "• Use UI/Default material to avoid offset/scale warnings\n" +
                "• Set sprite Mesh Type to 'Full Rect' in import settings\n" +
                "• Set texture Wrap Mode to 'Repeat' in import settings",
                MessageType.Info);
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScrollingMeshRenderer))]
    public class ScrollingMeshRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();

            // Info box with setup instructions
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "• Use a Quad mesh in the MeshFilter component\n" +
                "• Use Unlit/Transparent material for best performance\n" +
                "• Set texture Wrap Mode to 'Repeat' in import settings",
                MessageType.Info);
        }
    }
}
#endif