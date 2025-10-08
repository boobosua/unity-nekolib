#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    [Serializable]
    public class SetupFoldersSettings : ScriptableObject
    {
        // Store SO under Plugins per request
        private const string AssetDir = "Assets/Plugins/NekoLib/Setup";
        private const string AssetName = "SetupFoldersSettings.asset";

        [Serializable]
        public class FolderOption
        {
            public string name;
            public bool enabled = true;
        }

        [HideInInspector]
        [SerializeField] private string _rootPath = "Assets/Project";
        [HideInInspector]
        [SerializeField] private List<FolderOption> _folders = new();

        public string RootPath { get => _rootPath; set => _rootPath = string.IsNullOrWhiteSpace(value) ? "Assets/Project" : value; }
        public List<FolderOption> Folders => _folders;

        public void SetDefaults()
        {
            _rootPath = "Assets/Project";
            _folders = new List<FolderOption>
            {
                new FolderOption{ name = "Scripts", enabled = true },
                new FolderOption{ name = "Scenes", enabled = true },
                new FolderOption{ name = "Prefabs", enabled = true },
                new FolderOption{ name = "Animations", enabled = true },
                new FolderOption{ name = "Textures", enabled = true },
                new FolderOption{ name = "Models", enabled = true },
                new FolderOption{ name = "Scriptables", enabled = true },
                new FolderOption{ name = "Audio", enabled = true },
                new FolderOption{ name = "Materials", enabled = true },
                new FolderOption{ name = "Shaders", enabled = false },
                new FolderOption{ name = "Editor", enabled = false },
                new FolderOption{ name = "Resources", enabled = false },
            };
        }

        public static SetupFoldersSettings LoadOrCreate()
        {
            string assetPath = Path.Combine(AssetDir, AssetName).Replace("\\", "/");
            var settings = AssetDatabase.LoadAssetAtPath<SetupFoldersSettings>(assetPath);
            if (settings == null)
            {
                // Ensure directory structure under Assets/Plugins/NekoLib/Setup
                if (!AssetDatabase.IsValidFolder("Assets/Plugins")) AssetDatabase.CreateFolder("Assets", "Plugins");
                if (!AssetDatabase.IsValidFolder("Assets/Plugins/NekoLib")) AssetDatabase.CreateFolder("Assets/Plugins", "NekoLib");
                if (!AssetDatabase.IsValidFolder("Assets/Plugins/NekoLib/Setup")) AssetDatabase.CreateFolder("Assets/Plugins/NekoLib", "Setup");

                settings = CreateInstance<SetupFoldersSettings>();
                settings.SetDefaults();
                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                // no-op; fields are hidden via [HideInInspector]
            }
            return settings;
        }
    }
}
#endif
