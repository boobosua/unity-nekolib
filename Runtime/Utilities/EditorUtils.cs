using System;
using System.Collections.Generic;
using UnityEngine;
using NekoLib.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
#if UNITY_EDITOR
        /// <summary>
        /// Finds all *.asset files of type T in the given directory.
        /// Only works on Unity Editor.
        /// Use a path starting at "Assets/asset-folder/".
        /// </summary>
        public static T[] FindAllAssets<T>(string directoryPath) where T : UnityEngine.Object
        {
            try
            {
                // Check if directoryPath is null or empty
                if (string.IsNullOrEmpty(directoryPath))
                {
                    throw new ArgumentNullException(nameof(directoryPath), "Directory path cannot be null or empty");
                }

                // Find all asset files in the directory
                string[] files = System.IO.Directory.GetFiles(directoryPath, "*.asset");

                // Filter files to only include assets of type T
                List<T> assets = new();
                foreach (string file in files)
                {
                    string filePath = file;
                    try
                    {
                        T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(filePath);
                        if (asset != null)
                        {
                            assets.Add(asset);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error loading asset at path {filePath}: {ex.Message}");
                    }
                }

                return assets.ToArray();
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                Debug.LogError($"Directory not found: {directoryPath}. Error: {ex.Message}");
                return new T[0];
            }
            catch (System.IO.IOException ex)
            {
                Debug.LogError($"Error reading directory: {directoryPath}. Error: {ex.Message}");
                return new T[0];
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex.Message}");
                return new T[0];
            }
        }

        /// <summary>
        /// Draws an annulus (ring) gizmo in the Scene view.
        /// </summary>
        public static void DrawAnnulusGizmo(Vector3 center, float innerRadius, float outerRadius, Vector3 up = default, Color color = default, int segments = 36)
        {
            if (color == default) color = Swatch.ME;
            if (up == default) up = Vector3.up;

            // Create a coordinate system based on the up vector
            Vector3 forward = Vector3.Cross(up, Vector3.right);
            if (forward.sqrMagnitude < 0.001f) // up is parallel to right, use forward instead
                forward = Vector3.Cross(up, Vector3.forward);
            forward = forward.normalized;
            Vector3 right = Vector3.Cross(forward, up).normalized;

            float angleStep = 360f / segments;
            Vector3 lastInnerPoint = Vector3.zero;
            Vector3 lastOuterPoint = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                // Create points in the plane defined by the up vector
                Vector3 direction = right * cos + forward * sin;
                Vector3 innerPoint = center + direction * innerRadius;
                Vector3 outerPoint = center + direction * outerRadius;

                if (i > 0)
                {
                    Gizmos.color = color;
                    Gizmos.DrawLine(lastInnerPoint, innerPoint);
                    Gizmos.DrawLine(lastOuterPoint, outerPoint);
                    Gizmos.DrawLine(innerPoint, outerPoint);
                }

                lastInnerPoint = innerPoint;
                lastOuterPoint = outerPoint;
            }
        }

        /// <summary>
        /// Draws a circle gizmo in the Scene view.
        /// </summary>
        public static void DrawCircleGizmo(Vector3 center, float radius, Vector3 up = default, Color color = default, int segments = 64)
        {
            if (color == default) color = Swatch.ME;
            if (up == default) up = Vector3.up;

            Vector3 forward = Vector3.Cross(up, Vector3.right);
            if (forward.sqrMagnitude < 0.001f) // up is parallel to right, use forward instead
                forward = Vector3.Cross(up, Vector3.forward);
            forward = forward.normalized;
            Vector3 right = Vector3.Cross(forward, up).normalized;

            float angleStep = 360f / segments;
            Vector3 lastPoint = center + right * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                Vector3 direction = right * cos + forward * sin;
                Vector3 point = center + direction * radius;

                Gizmos.color = color;
                Gizmos.DrawLine(lastPoint, point);
                lastPoint = point;
            }
        }

        /// <summary>
        /// Checks if the reload domain is disabled in the editor.
        /// </summary>
        public static bool IsReloadDomainDisabled()
        {
            return EditorSettings.enterPlayModeOptionsEnabled &&
                    EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload);
        }
#endif
    }
}
