#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

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

            EditorGUI.BeginChangeCheck();

            _target?.Draw();
            _distance?.Draw();

            _speed?.Draw();
            _startAngle?.Draw();

            _elevationAngle?.Draw();
            _bearingAngle?.Draw();

            _facing?.Draw();

            _tree.ApplyChanges();
            _tree.InvokeDelayedActions();

            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();

            DrawSnapButtons();
        }

        private void DrawSnapButtons()
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Snap XZ (flat)")) SnapTo(0f, 0f);
                if (GUILayout.Button("Snap XY (front)")) SnapTo(90f, 0f);
                if (GUILayout.Button("Snap YZ (side)")) SnapTo(90f, 90f);
            }
        }

        private void SnapTo(float elevation, float bearing)
        {
            Undo.RecordObject(target, "Snap Orbit Plane");
            if (_elevationAngle != null) _elevationAngle.ValueEntry.WeakSmartValue = elevation;
            if (_bearingAngle != null) _bearingAngle.ValueEntry.WeakSmartValue = bearing;
            _tree?.ApplyChanges();
            SceneView.RepaintAll();
        }
    }
#else
    public class AutoOrbitAroundEditor : Editor
    {
        private SerializedProperty _targetProperty;
        private SerializedProperty _distanceProperty;
        private SerializedProperty _speedProperty;
        private SerializedProperty _startAngleProperty;
        private SerializedProperty _elevationAngleProperty;
        private SerializedProperty _bearingAngleProperty;
        private SerializedProperty _facingProperty;

        private void OnEnable()
        {
            _targetProperty = serializedObject.FindProperty("_target");
            _distanceProperty = serializedObject.FindProperty("_distance");
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

            EditorGUILayout.PropertyField(_speedProperty);
            EditorGUILayout.PropertyField(_startAngleProperty);

            EditorGUILayout.PropertyField(_elevationAngleProperty);
            EditorGUILayout.PropertyField(_bearingAngleProperty);

            EditorGUILayout.PropertyField(_facingProperty);

            if (serializedObject.ApplyModifiedProperties())
                SceneView.RepaintAll();

            DrawSnapButtons();
        }

        private void DrawSnapButtons()
        {
            EditorGUILayout.LabelField("Snap Plane", EditorStyles.miniLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("XZ (flat)"))  SnapTo(0f,  0f);
                if (GUILayout.Button("XY (front)")) SnapTo(90f, 0f);
                if (GUILayout.Button("YZ (side)"))  SnapTo(90f, 90f);
            }
        }

        private void SnapTo(float elevation, float bearing)
        {
            Undo.RecordObject(target, "Snap Orbit Plane");
            _elevationAngleProperty.floatValue = elevation;
            _bearingAngleProperty.floatValue = bearing;
            serializedObject.ApplyModifiedProperties();
            SceneView.RepaintAll();
        }
    }
#endif
}
#endif
