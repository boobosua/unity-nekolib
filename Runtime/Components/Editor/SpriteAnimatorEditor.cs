#if UNITY_EDITOR
using UnityEditor;

namespace NekoLib.Components
{
    [CustomEditor(typeof(SpriteAnimator))]
    public class SpriteAnimatorEditor : SpriteAnimatorEditorBase
    {

    }
}
#endif