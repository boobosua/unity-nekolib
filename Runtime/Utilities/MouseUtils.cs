using System;
using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
        /// <summary>
        /// Detect whether the cursor is focused on the game window or not.
        /// </summary>
        public static bool IsMouseInGameWindow()
        {
            Vector3 mp = Input.mousePosition;
            return !(0 > mp.x || 0 > mp.y || Screen.width < mp.x || Screen.height < mp.y);
        }

        /// <summary>
        /// Returns the world position of the mouse cursor in 2D.
        /// Designed for orthographic cameras. For perspective cameras, use GetMousePosition3D.
        /// </summary>
        public static Vector2 GetMousePosition2D(Camera camera = null)
        {
            var cam = camera == null ? Camera.main : camera;
            if (cam == null)
            {
                Debug.LogWarning("Camera is null. Mouse position will be returned as Vector2.zero.");
                return Vector2.zero;
            }

            var mousePos = Input.mousePosition;
            return cam.ScreenToWorldPoint(mousePos);
        }

        /// <summary>
        /// Returns the world position of the mouse cursor in 3D at a specified distance from the camera.
        /// </summary>
        /// <param name="distance">Distance from camera in world units</param>
        public static Vector3 GetMousePosition3D(float distance = 10f, Camera camera = null)
        {
            var cam = camera == null ? Camera.main : camera;
            if (cam == null)
            {
                Debug.LogWarning("Camera is null. Mouse position will be returned as Vector3.zero.");
                return Vector3.zero;
            }

            var mousePos = Input.mousePosition.With(z: distance);
            return cam.ScreenToWorldPoint(mousePos);
        }

        /// <summary>
        /// Returns the 3D position of the mouse cursor from a raycast.
        /// </summary>
        public static Vector3 GetMousePosition3DFromRaycast(int layerMask = Physics.DefaultRaycastLayers)
        {
            Ray ray = GetMouseRay();
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return hit.point;
            }

            Debug.LogWarning("No object hit by raycast. Returning Vector3.zero.");
            return Vector3.zero;
        }

        /// <summary>
        /// Returns the mouse ray.
        /// </summary>
        public static Ray GetMouseRay(Camera camera = null)
        {
            var cam = camera != null ? camera : Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("Camera is null. Mouse ray will be returned as default.");
                return default;
            }

            return cam.ScreenPointToRay(Input.mousePosition);
        }

        [Obsolete("Use IsMouseInGameWindow() instead.")]
        public static bool IsMouseOverGameWindow => IsMouseInGameWindow();

        [Obsolete("Use GetMousePosition2D() instead.")]
        public static Vector2 MouseWorldPosition => GetMousePosition2D();
    }
}
