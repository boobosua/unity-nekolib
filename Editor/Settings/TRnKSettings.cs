#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TRnK.Toolkit
{
    // Project-scoped settings container for TRnK.Toolkit
    internal class TRnKSettings : ScriptableObject
    {
        public string startupScenePath;
        public bool activateLoadedAdditiveOnSelect = false;
        public int timeScaleMax = 10;
        public bool hideToolbar = false;
        public bool autoReenterPlayAfterClear = true;

        private const string SettingsFolder = "Assets/Plugins/TRnK/Toolkit/Editor";
        private const string AssetPath = SettingsFolder + "/TRnKSettings.asset";

        public static TRnKSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TRnKSettings>(AssetPath);
            if (settings != null) return settings;

            EnsureFolders();

            settings = CreateInstance<TRnKSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Plugins");
            EnsureFolder("Assets/Plugins", "TRnK.Toolkit");
            EnsureFolder("Assets/Plugins/TRnK/Toolkit", "Editor");
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