#if UNITY_EDITOR
using UnityEditor;

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UISpriteAnimator))]
    public class UISpriteAnimatorEditor : SpriteAnimatorEditorBase
    {
        private SerializedProperty _preserveAspect;
        private SerializedProperty _pauseWhenInvisible;
        private SerializedProperty _canvasGroup;

        protected override void OnEnable()
        {
            base.OnEnable();
            _preserveAspect = serializedObject.FindProperty("_preserveAspect");
            _pauseWhenInvisible = serializedObject.FindProperty("_pauseWhenInvisible");
            _canvasGroup = serializedObject.FindProperty("_canvasGroup");
        }

        protected override void DrawAdditionalProperties()
        {
            EditorGUILayout.LabelField("UI Specific", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_preserveAspect);
            EditorGUILayout.PropertyField(_pauseWhenInvisible);

            // Only show Canvas Group field when Pause When Invisible is enabled
            if (_pauseWhenInvisible.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_canvasGroup, new UnityEngine.GUIContent("Canvas Group", "Optional Canvas Group to check for visibility (alpha > 0)"));
                EditorGUI.indentLevel--;
            }
        }
    }
}
#endif