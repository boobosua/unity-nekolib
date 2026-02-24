#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    [Serializable]
    public class SetupFoldersSettings : ScriptableObject
    {
        private const string AssetDir = "Assets/Plugins/NekoLib/Editor";
        private const string AssetName = "SetupFoldersSettings.asset";

        [Serializable]
        public class FolderOption
        {
            public string name;
            public bool enabled = true;
        }

        [HideInInspector]
        [SerializeField] private string _namespaceRoot = null;
        [HideInInspector]
        [SerializeField] private string _rootPath = "Assets/Project";
        [HideInInspector]
        [SerializeField] private List<FolderOption> _folders = new();

        public string NamespaceRoot
        {
            get => string.IsNullOrEmpty(_namespaceRoot) ? DeriveDefaultNamespaceRoot() : _namespaceRoot;
            set
            {
                // remove whitespace characters from user input
                string sanitized = string.IsNullOrWhiteSpace(value)
                    ? string.Empty
                    : new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
                _namespaceRoot = sanitized;
            }
        }
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
            if (settings != null)
                return settings;

            if (settings == null)
            {
                EnsureFolders();

                settings = CreateInstance<SetupFoldersSettings>();
                settings.SetDefaults();
                // initialize namespace root from project settings or derived default
                settings._namespaceRoot = settings.DeriveInitialNamespaceRoot();
                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                // no-op; fields are hidden via [HideInInspector]
            }
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

        private string DeriveInitialNamespaceRoot()
        {
            // Prefer Unity's Project Settings root namespace if set
            string projNs = UnityEditor.EditorSettings.projectGenerationRootNamespace;
            if (!string.IsNullOrWhiteSpace(projNs))
            {
                return new string(projNs.Where(c => !char.IsWhiteSpace(c)).ToArray());
            }
            return DeriveDefaultNamespaceRoot();
        }

        private string DeriveDefaultNamespaceRoot()
        {
            // Use product name with whitespace removed as default
            string name = Application.productName;
            if (string.IsNullOrEmpty(name)) name = "Project";
            return new string(name.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }
}
#endif
