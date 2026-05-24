#if UNITY_EDITOR
using UnityEditor;

namespace TRnK.Toolkit
{
    [CustomEditor(typeof(SetupFoldersSettings))]
    internal class SetupFoldersSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Intentionally show nothing in inspector for this asset
        }
    }
}
#endif
