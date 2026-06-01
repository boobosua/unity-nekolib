#if UNITY_EDITOR
using UnityEditor;

namespace TRnK.Toolkit
{
    [CustomEditor(typeof(FoldersSettings))]
    internal class FoldersSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Intentionally show nothing in inspector for this asset
        }
    }
}
#endif
