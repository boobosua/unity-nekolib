#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    [InitializeOnLoad]
    public static class SetupSettingsHider
    {
        static SetupSettingsHider()
        {
            EnsureHidden<SetupFoldersSettings>("Assets/Plugins/NekoLib/Setup/SetupFoldersSettings.asset");
            EnsureHidden<SetupPackagesSettings>("Assets/Plugins/NekoLib/Setup/SetupPackagesSettings.asset");
        }

        private static void EnsureHidden<T>(string assetPath) where T : ScriptableObject
        {
            var obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (obj == null) return;
            var flags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
            if ((obj.hideFlags & flags) != flags)
            {
                obj.hideFlags = flags;
                EditorUtility.SetDirty(obj);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
