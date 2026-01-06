#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    [Serializable]
    public class SetupPackagesSettings : ScriptableObject
    {
        private const string AssetDir = "Assets/Plugins/NekoLib/Editor";
        private const string AssetName = "SetupPackagesSettings.asset";

        private static readonly string[] DefaultGitUrls =
        {
            "https://github.com/boobosua/unity-neko-signal.git",
            "https://github.com/boobosua/unity-neko-flow.git",
            "https://github.com/boobosua/unity-neko-serializer.git",
        };

        [Serializable]
        public class GitPackage
        {
            public string url;
        }

        [HideInInspector]
        [SerializeField] private List<GitPackage> _packages = new();
        public IReadOnlyList<GitPackage> Packages => _packages;

        public void SetDefaults()
        {
            _packages = new List<GitPackage>();
            foreach (var url in DefaultGitUrls)
            {
                if (string.IsNullOrWhiteSpace(url)) continue;
                _packages.Add(new GitPackage { url = url });
            }
        }

        /// <summary>
        /// Adds any newly-introduced default Git URLs that are missing from this asset.
        /// Does not remove or overwrite existing entries.
        /// Returns how many entries were added.
        /// </summary>
        public int RefreshMissingDefaults()
        {
            _packages ??= new List<GitPackage>();

            var existing = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in _packages)
            {
                if (p == null) continue;
                string key = SetupPackagesTool.NormalizeGitUrl(p.url);
                if (!string.IsNullOrEmpty(key)) existing.Add(key);
            }

            int added = 0;
            foreach (var url in DefaultGitUrls)
            {
                if (string.IsNullOrWhiteSpace(url)) continue;
                string key = SetupPackagesTool.NormalizeGitUrl(url);
                if (string.IsNullOrEmpty(key)) continue;
                if (existing.Contains(key)) continue;
                _packages.Add(new GitPackage { url = url });
                existing.Add(key);
                added++;
            }
            return added;
        }

        public static SetupPackagesSettings LoadOrCreate()
        {
            string assetPath = Path.Combine(AssetDir, AssetName).Replace("\\", "/");
            var settings = AssetDatabase.LoadAssetAtPath<SetupPackagesSettings>(assetPath);
            if (settings != null)
                return settings;

            if (settings == null)
            {
                EnsureFolders();

                settings = CreateInstance<SetupPackagesSettings>();
                settings.SetDefaults();
                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                // no-op; do not modify existing asset; fields are hidden via [HideInInspector]
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
    }
}
#endif
