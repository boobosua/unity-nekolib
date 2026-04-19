using System;
using System.Collections.Generic;
using NekoLib.Constant;
using UnityEngine;
using NekoLib.Logger;
using NekoLib.ColorPalette;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
#if UNITY_EDITOR
        private const float NearParallelSqrMagnitude = 0.001f;

        /// <summary>Finds all assets of type T in a directory. Use a path starting at "Assets/".</summary>
        public static T[] FindAssets<T>(string directoryPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                Log.Error("Directory path cannot be null or empty.");
                return Array.Empty<T>();
            }

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { directoryPath });
            List<T> assets = new();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    assets.Add(asset);
            }
            return assets.ToArray();
        }

        /// <summary>Draws an annulus (ring) gizmo in the Scene view.</summary>
        public static void DrawAnnulusGizmo(Vector3 center, float innerRadius, float outerRadius, Vector3 up = default, Color color = default, int segments = 36)
        {
            if (color == default) color = Swatch.ME;
            if (up == default) up = Vector3.up;

            // Create a coordinate system based on the up vector
            Vector3 forward = Vector3.Cross(up, Vector3.right);
            if (forward.sqrMagnitude < NearParallelSqrMagnitude) // up is parallel to right, use forward instead
                forward = Vector3.Cross(up, Vector3.forward);
            forward = forward.normalized;
            Vector3 right = Vector3.Cross(forward, up).normalized;

            float angleStep = Constants.FullRotation / segments;
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

        /// <summary>Draws a circle gizmo in the Scene view.</summary>
        public static void DrawCircleGizmo(Vector3 center, float radius, Vector3 up = default, Color color = default, int segments = 64)
        {
            if (color == default) color = Swatch.ME;
            if (up == default) up = Vector3.up;

            Vector3 forward = Vector3.Cross(up, Vector3.right);
            if (forward.sqrMagnitude < NearParallelSqrMagnitude) // up is parallel to right, use forward instead
                forward = Vector3.Cross(up, Vector3.forward);
            forward = forward.normalized;
            Vector3 right = Vector3.Cross(forward, up).normalized;

            float angleStep = Constants.FullRotation / segments;
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

        /// <summary>Checks if the reload domain is disabled in the editor.</summary>
        public static bool IsReloadDomainDisabled()
        {
            return EditorSettings.enterPlayModeOptionsEnabled &&
                    EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload);
        }
#endif
    }
}
