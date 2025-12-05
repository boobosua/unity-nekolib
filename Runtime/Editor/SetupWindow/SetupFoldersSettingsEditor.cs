#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
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
