#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    /// <summary>
    /// Utility methods for creating project folders under a chosen Assets-relative root.
    /// Editor-only.
    /// </summary>
    public static class SetupFoldersTool
    {
        private const string DefaultRoot = "Assets/Project";

        public static void CreateFolders(SetupFoldersSettings settings, List<string> folderNames)
        {
            if (settings == null) settings = SetupFoldersSettings.LoadOrCreate();
            string root = SanitizeRoot(settings.RootPath);

            // Determine if anything is missing first
            bool rootMissing = !AssetDatabase.IsValidFolder(root);
            var distinctNames = folderNames.Distinct().Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToList();
            bool anyMissing = rootMissing || distinctNames.Any(n => !AssetDatabase.IsValidFolder(CombineUnityPath(root, n)));

            if (!anyMissing)
            {
                EditorUtility.DisplayDialog("Project Setup", "All selected folders already exist.", "OK");
                return;
            }

            bool createdAny = false;
            if (rootMissing)
            {
                createdAny |= EnsureFolder(root);
            }

            foreach (var name in distinctNames)
            {
                string folderPath = CombineUnityPath(root, name);
                createdAny |= EnsureFolder(folderPath);
            }

            if (createdAny)
            {
                AssetDatabase.Refresh();
            }
            EditorUtility.DisplayDialog("Project Setup", "Requested folders have been created.", "OK");
        }

        public static string CombineUnityPath(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) a = DefaultRoot;
            if (string.IsNullOrEmpty(b)) return a.Replace("\\", "/");
            var path = a.Trim().TrimEnd('/', '\\') + "/" + b.Trim().TrimStart('/', '\\');
            return path.Replace("\\", "/");
        }

        public static string AbsoluteToAssetsRelative(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath)) return null;
            absolutePath = absolutePath.Replace("\\", "/");
            string dataPath = Application.dataPath.Replace("\\", "/");
            if (!absolutePath.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase)) return null;
            string rel = "Assets" + absolutePath.Substring(dataPath.Length);
            return rel.Replace("\\", "/");
        }

        private static string SanitizeRoot(string root)
        {
            if (string.IsNullOrWhiteSpace(root)) return DefaultRoot;
            root = root.Replace("\\", "/").Trim();
            if (!root.StartsWith("Assets")) root = DefaultRoot; // enforce Assets-relative
            return root.TrimEnd('/');
        }

        private static bool EnsureFolder(string assetsRelativePath)
        {
            assetsRelativePath = assetsRelativePath.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(assetsRelativePath)) return false;

            // Make sure all segments exist
            string[] parts = assetsRelativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts[0] != "Assets") return false;

            string current = parts[0]; // Assets
            bool createdAny = false;
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                    createdAny = true;
                }
                current = next;
            }
            return createdAny;
        }
    }
}
#endif
