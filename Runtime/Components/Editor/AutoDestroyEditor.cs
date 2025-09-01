#if UNITY_EDITOR
using UnityEditor;

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AutoDestroy))]
    public class AutoDestroyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif