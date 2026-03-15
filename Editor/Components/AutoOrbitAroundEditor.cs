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
        private InspectorProperty _speed;
        private InspectorProperty _startAngle;
        private InspectorProperty _elevationAngle;
        private InspectorProperty _bearingAngle;
        private InspectorProperty _facing;

        protected override void OnEnable()
        {
            base.OnEnable();

            _tree = PropertyTree.Create(serializedObject);
            _tree.DrawMonoScriptObjectField = false;

            _target = _tree.GetPropertyAtUnityPath("_target");
            _distance = _tree.GetPropertyAtUnityPath("_distance");
            _mode = _tree.GetPropertyAtUnityPath("_mode");
            _speed = _tree.GetPropertyAtUnityPath("_speed");
            _startAngle = _tree.GetPropertyAtUnityPath("_startAngle");
            _elevationAngle = _tree.GetPropertyAtUnityPath("_elevationAngle");
            _bearingAngle = _tree.GetPropertyAtUnityPath("_bearingAngle");
            _facing = _tree.GetPropertyAtUnityPath("_facing");
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

            _speed?.Draw();
            _startAngle?.Draw();

            int modeValue = 0;
            object modeObj = _mode?.ValueEntry?.WeakSmartValue;
            if (modeObj is Enum enumValue)
                modeValue = Convert.ToInt32(enumValue);

            if (modeValue == 0) // AutoHorizontalOnly
                _elevationAngle?.Draw();
            else
                _bearingAngle?.Draw();

            _facing?.Draw();

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
        private SerializedProperty _speedProperty;
        private SerializedProperty _startAngleProperty;
        private SerializedProperty _elevationAngleProperty;
        private SerializedProperty _bearingAngleProperty;
        private SerializedProperty _facingProperty;

        private void OnEnable()
        {
            _targetProperty = serializedObject.FindProperty("_target");
            _distanceProperty = serializedObject.FindProperty("_distance");
            _modeProperty = serializedObject.FindProperty("_mode");
            _speedProperty = serializedObject.FindProperty("_speed");
            _startAngleProperty = serializedObject.FindProperty("_startAngle");
            _elevationAngleProperty = serializedObject.FindProperty("_elevationAngle");
            _bearingAngleProperty = serializedObject.FindProperty("_bearingAngle");
            _facingProperty = serializedObject.FindProperty("_facing");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_targetProperty);
            EditorGUILayout.PropertyField(_distanceProperty);
            EditorGUILayout.PropertyField(_modeProperty);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_speedProperty);
            EditorGUILayout.PropertyField(_startAngleProperty);

            if (_modeProperty.enumValueIndex == 0) // AutoHorizontalOnly
                EditorGUILayout.PropertyField(_elevationAngleProperty);
            else
                EditorGUILayout.PropertyField(_bearingAngleProperty);

            EditorGUILayout.PropertyField(_facingProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif
