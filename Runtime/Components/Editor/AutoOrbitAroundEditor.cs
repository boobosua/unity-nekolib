#if UNITY_EDITOR
using System;
using UnityEditor;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AutoOrbitAround))]
#if ODIN_INSPECTOR
    public class AutoOrbitAroundEditor : OdinEditor
    {
        private PropertyTree _tree;
        private InspectorProperty _target;
        private InspectorProperty _distance;
        private InspectorProperty _mode;
        private InspectorProperty _horizontalSpeed;
        private InspectorProperty _staticVerticalAngle;
        private InspectorProperty _verticalSpeed;
        private InspectorProperty _staticHorizontalAngle;

        protected override void OnEnable()
        {
            base.OnEnable();

            _tree = PropertyTree.Create(serializedObject);
            _tree.DrawMonoScriptObjectField = false;

            _target = _tree.GetPropertyAtUnityPath("_target");
            _distance = _tree.GetPropertyAtUnityPath("_distance");
            _mode = _tree.GetPropertyAtUnityPath("_mode");
            _horizontalSpeed = _tree.GetPropertyAtUnityPath("_horizontalSpeed");
            _staticVerticalAngle = _tree.GetPropertyAtUnityPath("_staticVerticalAngle");
            _verticalSpeed = _tree.GetPropertyAtUnityPath("_verticalSpeed");
            _staticHorizontalAngle = _tree.GetPropertyAtUnityPath("_staticHorizontalAngle");
        }

        protected override void OnDisable()
        {
            _tree?.Dispose();
            _tree = null;
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            if (_tree == null)
            {
                _tree = PropertyTree.Create(serializedObject);
                _tree.DrawMonoScriptObjectField = false;
            }

            _tree.UpdateTree();

            _target?.Draw();
            _distance?.Draw();
            _mode?.Draw();

            EditorGUILayout.Space();

            int modeValue = 0;
            object modeObj = _mode?.ValueEntry?.WeakSmartValue;
            if (modeObj is Enum enumValue)
            {
                modeValue = Convert.ToInt32(enumValue);
            }
            else if (modeObj is int intValue)
            {
                modeValue = intValue;
            }

            if (modeValue == 0) // AutoHorizontalOnly
            {
                _horizontalSpeed?.Draw();
                _staticVerticalAngle?.Draw();
            }
            else if (modeValue == 1) // AutoVerticalOnly
            {
                _verticalSpeed?.Draw();
                _staticHorizontalAngle?.Draw();
            }
            else
            {
                // Fallback (e.g., mixed values): show both sets
                _horizontalSpeed?.Draw();
                _staticVerticalAngle?.Draw();
                _verticalSpeed?.Draw();
                _staticHorizontalAngle?.Draw();
            }

            _tree.ApplyChanges();
            _tree.InvokeDelayedActions();
        }
    }
#else
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
#endif
}
#endif
