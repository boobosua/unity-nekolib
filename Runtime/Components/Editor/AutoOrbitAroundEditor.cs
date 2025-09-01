#if UNITY_EDITOR
using UnityEditor;

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AutoOrbitAround))]
    public class AutoOrbitAroundEditor : Editor
    {
        private SerializedProperty _targetProperty;
        private SerializedProperty _distanceProperty;
        private SerializedProperty _modeProperty;
        private SerializedProperty _horizontalSpeedProperty;
        private SerializedProperty _staticVerticalAngleProperty;
        private SerializedProperty _verticalSpeedProperty;
        private SerializedProperty _staticHorizontalAngleProperty;

        private void OnEnable()
        {
            _targetProperty = serializedObject.FindProperty("_target");
            _distanceProperty = serializedObject.FindProperty("_distance");
            _modeProperty = serializedObject.FindProperty("_mode");
            _horizontalSpeedProperty = serializedObject.FindProperty("_horizontalSpeed");
            _staticVerticalAngleProperty = serializedObject.FindProperty("_staticVerticalAngle");
            _verticalSpeedProperty = serializedObject.FindProperty("_verticalSpeed");
            _staticHorizontalAngleProperty = serializedObject.FindProperty("_staticHorizontalAngle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Common properties that are always visible
            EditorGUILayout.PropertyField(_targetProperty);
            EditorGUILayout.PropertyField(_distanceProperty);
            EditorGUILayout.PropertyField(_modeProperty);

            EditorGUILayout.Space();

            // Get the current mode value
            int modeValue = _modeProperty.enumValueIndex;

            // Show fields conditionally based on the orbit mode
            if (modeValue == 0) // AutoHorizontalOnly
            {
                EditorGUILayout.PropertyField(_horizontalSpeedProperty);
                EditorGUILayout.PropertyField(_staticVerticalAngleProperty);
            }
            else if (modeValue == 1) // AutoVerticalOnly
            {
                EditorGUILayout.PropertyField(_verticalSpeedProperty);
                EditorGUILayout.PropertyField(_staticHorizontalAngleProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
