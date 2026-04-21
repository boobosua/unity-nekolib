#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace NekoLib
{
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    internal class TmpFontReplacerSettings : ScriptableObject
    {
#if ODIN_INSPECTOR
        [ReadOnly] public TMP_FontAsset lastFont;
        [ReadOnly] public Material lastMaterial;
        [ReadOnly] public List<DefaultAsset> excludedFolders = new();
        [ReadOnly] public bool lastScanCompleted;
        [ReadOnly] public long lastScanTimestamp;
        [ReadOnly] public List<SerializedAssetEntry> lastScanScenes = new();
        [ReadOnly] public List<SerializedAssetEntry> lastScanPrefabs = new();
#else
        public TMP_FontAsset      lastFont;
        public Material           lastMaterial;
        public List<DefaultAsset> excludedFolders = new();
        public bool lastScanCompleted;
        public long lastScanTimestamp;
        public List<SerializedAssetEntry> lastScanScenes  = new();
        public List<SerializedAssetEntry> lastScanPrefabs = new();
#endif

        [System.Serializable]
        internal class SerializedObjectEntry
        {
            public string HierarchyPath;
            public string Name;
            public string FontName;
            public string MaterialName;
        }

        [System.Serializable]
        internal class SerializedAssetEntry
        {
            public string Path;
            public string Name;
            public List<SerializedObjectEntry> Objects = new();
        }

        private const string SettingsFolder = "Assets/Plugins/NekoLib/Editor";
        private const string AssetPath = SettingsFolder + "/TmpFontReplacerSettings.asset";

        public static TmpFontReplacerSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TmpFontReplacerSettings>(AssetPath);
            if (settings != null) return settings;

            EnsureFolders();

            settings = CreateInstance<TmpFontReplacerSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Plugins");
            EnsureFolder("Assets/Plugins", "NekoLib");
            EnsureFolder("Assets/Plugins/NekoLib", "Editor");
        }

        private static void EnsureFolder(string parent, string folderName)
        {
            string full = parent + "/" + folderName;
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif