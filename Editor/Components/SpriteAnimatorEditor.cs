#if UNITY_EDITOR
using UnityEditor;

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SpriteAnimator))]
    public class SpriteAnimatorEditor : SpriteAnimatorEditorBase
    {

    }
}
#endif