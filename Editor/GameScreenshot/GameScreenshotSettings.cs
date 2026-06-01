#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TRnK.Toolkit
{
    internal enum CaptureMode { GameView, SpecificCamera }

    internal class GameScreenshotSettings : ScriptableObject
    {
        public CaptureMode captureMode = CaptureMode.GameView;
        public int supersize = 2;
        public string saveFolder = "";
        public bool revealOnSave = true;

        private const string SettingsFolder = "Assets/Plugins/TRnK/Toolkit/Editor";
        private const string AssetPath = SettingsFolder + "/GameScreenshotSettings.asset";

        public static GameScreenshotSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameScreenshotSettings>(AssetPath);
            if (settings != null) return settings;

            EnsureFolders();

            settings = CreateInstance<GameScreenshotSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            EditorAssetUtils.SaveAndRefresh();
            return settings;
        }

        private static void EnsureFolders() => EditorAssetUtils.EnsureFolderPath(SettingsFolder);
    }
}
#endif
