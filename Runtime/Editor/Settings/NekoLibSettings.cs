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

        private const string SettingsFolder = "Assets/Plugins/NekoLib/Settings";
        private const string AssetPath = SettingsFolder + "/NekoLibSettings.asset";

        public static NekoLibSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<NekoLibSettings>(AssetPath);
            if (settings != null) return settings;

            if (!AssetDatabase.IsValidFolder(SettingsFolder))
            {
                var parent = "Assets/Plugins/NekoLib";
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    AssetDatabase.CreateFolder("Assets/Plugins", "NekoLib");
                }
                AssetDatabase.CreateFolder(parent, "Settings");
            }

            settings = CreateInstance<NekoLibSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }
    }
}
#endif