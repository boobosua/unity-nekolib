#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TRnK.Toolkit
{
    public static class EditorAssetUtils
    {
        // Creates every missing folder segment in the given Assets-relative path.
        public static void EnsureFolderPath(string assetRelativePath)
        {
            string[] parts = assetRelativePath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        public static void SaveAndRefresh()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void MarkDirtyAndSave(Object obj)
        {
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
