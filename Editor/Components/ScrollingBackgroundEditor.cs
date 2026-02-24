#if UNITY_EDITOR
using UnityEditor;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScrollingRawImage))]
#if ODIN_INSPECTOR
    public class ScrollingRawImageEditor : OdinEditor
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

            // Info box with setup instructions
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Set texture Wrap Mode to 'Repeat' in import settings",
                MessageType.Info);
        }
    }
#else
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
#endif

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScrollingImage))]
#if ODIN_INSPECTOR
    public class ScrollingImageEditor : OdinEditor
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

            // Info box with setup instructions
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "• Use UI/Default material for proper texture offset handling\n" +
                "• Set Image Type to 'Simple'\n" +
                "• Set sprite texture Wrap Mode to 'Repeat' in import settings",
                MessageType.Info);
        }
    }
#else
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
#endif

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScrollingSpriteRenderer))]
#if ODIN_INSPECTOR
    public class ScrollingSpriteRendererEditor : OdinEditor
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

            // Info box with setup instructions
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "• Use UI/Default material to avoid offset/scale warnings\n" +
                "• Set sprite Mesh Type to 'Full Rect' in import settings\n" +
                "• Set texture Wrap Mode to 'Repeat' in import settings",
                MessageType.Info);
        }
    }
#else
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
#endif

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScrollingMeshRenderer))]
#if ODIN_INSPECTOR
    public class ScrollingMeshRendererEditor : OdinEditor
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

            // Info box with setup instructions
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "• Use a Quad mesh in the MeshFilter component\n" +
                "• Use Unlit/Transparent material for best performance\n" +
                "• Set texture Wrap Mode to 'Repeat' in import settings",
                MessageType.Info);
        }
    }
#else
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
#endif
}
#endif