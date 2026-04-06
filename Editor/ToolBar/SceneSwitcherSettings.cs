#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    [Serializable]
    internal class SceneGroup
    {
        public string label = "Group";
        public List<string> scenePaths = new List<string>();
    }

    internal class SceneSwitcherSettings : ScriptableObject
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ReadOnly]
#endif
        public List<SceneGroup> groups = new List<SceneGroup>();

        private const string SettingsFolder = "Assets/Plugins/NekoLib/Editor";
        private const string AssetPath = SettingsFolder + "/SceneSwitcherSettings.asset";

        public static SceneSwitcherSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<SceneSwitcherSettings>(AssetPath);
            if (settings != null) return settings;

            EnsureFolders();

            settings = CreateInstance<SceneSwitcherSettings>();
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
            string full = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
