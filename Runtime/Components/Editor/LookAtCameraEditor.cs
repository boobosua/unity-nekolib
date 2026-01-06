#if UNITY_EDITOR
using UnityEditor;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LookAtCamera))]
#if ODIN_INSPECTOR
    public class LookAtCameraEditor : OdinEditor
    {
        private PropertyTree _tree;
        private InspectorProperty _mode;
        private InspectorProperty _useCustomCamera;
        private InspectorProperty _cameraToLookAt;

        protected override void OnEnable()
        {
            base.OnEnable();

            _tree = PropertyTree.Create(serializedObject);
            _tree.DrawMonoScriptObjectField = false;

            _mode = _tree.GetPropertyAtUnityPath("_mode");
            _useCustomCamera = _tree.GetPropertyAtUnityPath("_useCustomCamera");
            _cameraToLookAt = _tree.GetPropertyAtUnityPath("_cameraToLookAt");
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

            _mode?.Draw();
            _useCustomCamera?.Draw();

            bool useCustomCamera = false;
            object useCustomCameraObj = _useCustomCamera?.ValueEntry?.WeakSmartValue;
            if (useCustomCameraObj is bool boolValue)
            {
                useCustomCamera = boolValue;
            }

            if (useCustomCamera)
            {
                _cameraToLookAt?.Draw();
            }

            _tree.ApplyChanges();
            _tree.InvokeDelayedActions();
        }
    }
#else
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
#endif
}
#endif
