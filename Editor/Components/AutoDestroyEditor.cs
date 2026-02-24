#if UNITY_EDITOR
using UnityEditor;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AutoDestroy))]
#if ODIN_INSPECTOR
    public class AutoDestroyEditor : OdinEditor
    {
        private PropertyTree _tree;

        protected override void OnEnable()
        {
            base.OnEnable();
            _tree = PropertyTree.Create(serializedObject);
            _tree.DrawMonoScriptObjectField = false;
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
            _tree.Draw(false);
            _tree.ApplyChanges();
            _tree.InvokeDelayedActions();
        }
    }
#else
    public class AutoDestroyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif