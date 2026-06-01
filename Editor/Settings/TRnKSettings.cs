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
            EditorAssetUtils.SaveAndRefresh();
            return settings;
        }

        private static void EnsureFolders() => EditorAssetUtils.EnsureFolderPath(SettingsFolder);
    }
}
#endif