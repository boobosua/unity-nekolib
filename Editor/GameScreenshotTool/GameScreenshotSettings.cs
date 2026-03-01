#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    internal enum CaptureMode { GameView, SpecificCamera }

    internal class GameScreenshotSettings : ScriptableObject
    {
        public CaptureMode captureMode = CaptureMode.GameView;
        public int supersize = 2;
        public string saveFolder = "";
        public bool revealOnSave = true;

        private const string SettingsFolder = "Assets/Plugins/NekoLib/Editor";
        private const string AssetPath = SettingsFolder + "/GameScreenshotSettings.asset";

        public static GameScreenshotSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameScreenshotSettings>(AssetPath);
            if (settings != null) return settings;

            EnsureFolders();

            settings = CreateInstance<GameScreenshotSettings>();
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

        private static void EnsureFolder(string parent, string name)
        {
            var full = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
