#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TRnK.Toolkit
{
    [CustomEditor(typeof(PackagesSettings))]
    internal class PackagesSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Intentionally show nothing in inspector for this asset
        }
    }
}
#endif
