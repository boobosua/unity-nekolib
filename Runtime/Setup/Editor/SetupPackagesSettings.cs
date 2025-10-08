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
        private const string AssetDir = "Assets/Plugins/NekoLib/Setup";
        private const string AssetName = "SetupPackagesSettings.asset";

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
            _packages = new List<GitPackage>
            {
                new GitPackage { url = "https://github.com/boobosua/unity-neko-signal.git" },
                new GitPackage { url = "https://github.com/boobosua/unity-neko-flow.git" },
                new GitPackage { url = "https://github.com/boobosua/unity-neko-serialize.git" },
            };
        }

        public static SetupPackagesSettings LoadOrCreate()
        {
            string assetPath = Path.Combine(AssetDir, AssetName).Replace("\\", "/");
            var settings = AssetDatabase.LoadAssetAtPath<SetupPackagesSettings>(assetPath);
            if (settings == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Plugins")) AssetDatabase.CreateFolder("Assets", "Plugins");
                if (!AssetDatabase.IsValidFolder("Assets/Plugins/NekoLib")) AssetDatabase.CreateFolder("Assets/Plugins", "NekoLib");
                if (!AssetDatabase.IsValidFolder("Assets/Plugins/NekoLib/Setup")) AssetDatabase.CreateFolder("Assets/Plugins/NekoLib", "Setup");

                settings = CreateInstance<SetupPackagesSettings>();
                settings.SetDefaults();
                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                // Enforce code-defined list even if the asset already exists
                settings.SetDefaults();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
    }
}
#endif
