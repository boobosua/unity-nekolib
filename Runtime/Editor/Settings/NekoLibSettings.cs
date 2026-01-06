#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    // Project-scoped settings container for NekoLib
    internal class NekoLibSettings : ScriptableObject
    {
        public string startupScenePath;
        public bool activateLoadedAdditiveOnSelect = false;
        public int timeScaleMax = 10;
        public bool hideToolbar = false;
        public bool autoReenterPlayAfterClear = true;

        private const string SettingsFolder = "Assets/Plugins/NekoLib/Editor";
        private const string AssetPath = SettingsFolder + "/NekoLibSettings.asset";

        public static NekoLibSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<NekoLibSettings>(AssetPath);
            if (settings != null) return settings;

            EnsureFolders();

            settings = CreateInstance<NekoLibSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Plugins");
            EnsureFolder("Assets/Plugins", "NekoLib");
            EnsureFolder("Assets/Plugins/NekoLib", "Editor");
        }

        private static void EnsureFolder(string parent, string folderName)
        {
            string full = parent.EndsWith("/") ? parent + folderName : parent + "/" + folderName;
            if (AssetDatabase.IsValidFolder(full))
                return;

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif