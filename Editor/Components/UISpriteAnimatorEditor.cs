#if UNITY_EDITOR
using UnityEditor;

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UISpriteAnimator))]
    public class UISpriteAnimatorEditor : SpriteAnimatorEditorBase
    {
        private SerializedProperty _canvasGroups;

        protected override void OnEnable()
        {
            base.OnEnable();
            _canvasGroups = serializedObject.FindProperty("_canvasGroups");
        }

        protected override bool HasAdditionalProperties() => true;

        protected override void DrawAdditionalProperties()
        {
#if ODIN_INSPECTOR
            DrawOdinUnityProperty("_canvasGroups");
#else
            EditorGUILayout.PropertyField(_canvasGroups);
#endif

            if (_canvasGroups == null || _canvasGroups.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No Canvas Groups assigned. Visibility checks will use only the Image component (enabled + alpha).", MessageType.Info);
            }
        }
    }
}
#endif