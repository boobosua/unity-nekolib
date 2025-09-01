#if UNITY_EDITOR
using UnityEditor;

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LookAtCamera))]
    public class LookAtCameraEditor : Editor
    {
        private SerializedProperty _modeProperty;
        private SerializedProperty _useCustomCameraProperty;
        private SerializedProperty _cameraToLookAtProperty;

        private void OnEnable()
        {
            _modeProperty = serializedObject.FindProperty("_mode");
            _useCustomCameraProperty = serializedObject.FindProperty("_useCustomCamera");
            _cameraToLookAtProperty = serializedObject.FindProperty("_cameraToLookAt");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the mode property
            EditorGUILayout.PropertyField(_modeProperty);

            // Draw the use custom camera property
            EditorGUILayout.PropertyField(_useCustomCameraProperty);

            // Only show the camera to look at field if use custom camera is true
            if (_useCustomCameraProperty.boolValue)
            {
                EditorGUILayout.PropertyField(_cameraToLookAtProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
