using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class CameraExtensions
    {
        /// <summary>
        /// Check if a layer is in the camera's culling mask.
        /// </summary>
        public static bool IsLayerInCullingMask(this Camera camera, LayerMask layerMask)
        {
            return (camera.cullingMask & layerMask) != 0;
        }

        /// <summary>
        /// Add a layer to the camera's culling mask.
        /// </summary>
        public static void AddToCullingMask(this Camera camera, LayerMask layerMask)
        {
            if (!camera.IsLayerInCullingMask(layerMask))
            {
                camera.cullingMask |= layerMask;
            }
        }

        /// <summary>
        /// Remove a layer from the camera's culling mask.
        /// </summary>
        public static void RemoveFromCullingMask(this Camera camera, LayerMask layerMask)
        {
            if (camera.IsLayerInCullingMask(layerMask))
            {
                camera.cullingMask &= ~layerMask;
            }
        }

        /// <summary>
        /// Set the camera's culling mask to only render specific layers (replaces current mask).
        /// </summary>
        public static void SetCullingMask(this Camera camera, LayerMask layerMask)
        {
            camera.cullingMask = layerMask;
        }

        /// <summary>
        /// Set the camera to render all layers.
        /// </summary>
        public static void ShowAllLayers(this Camera camera)
        {
            camera.cullingMask = -1;
        }

        /// <summary>
        /// Set the camera to render no layers.
        /// </summary>
        public static void HideAllLayers(this Camera camera)
        {
            camera.cullingMask = 0;
        }

        /// <summary>
        /// Zoom in by reducing field of view.
        /// </summary>
        public static void ZoomIn(this Camera camera, float amount = 10f)
        {
            camera.fieldOfView = Mathf.Max(1f, camera.fieldOfView - amount);
        }

        /// <summary>
        /// Zoom out by increasing field of view.
        /// </summary>
        public static void ZoomOut(this Camera camera, float amount = 10f)
        {
            camera.fieldOfView = Mathf.Min(179f, camera.fieldOfView + amount);
        }

        /// <summary>
        /// Set field of view with clamping.
        /// </summary>
        public static void SetFOV(this Camera camera, float fov)
        {
            camera.fieldOfView = Mathf.Clamp(fov, 1f, 179f);
        }

        /// <summary>
        /// Get the camera's screen size in pixels.
        /// </summary>
        public static Vector2 GetScreenSize(this Camera camera)
        {
            return new Vector2(camera.pixelWidth, camera.pixelHeight);
        }

        /// <summary>
        /// Set orthographic size with validation.
        /// </summary>
        public static void SetOrthographicSize(this Camera camera, float size)
        {
            if (camera.orthographic)
            {
                camera.orthographicSize = Mathf.Max(0.1f, size);
            }
            else
            {
                Log.Warn("Camera is not orthographic!");
            }
        }

        /// <summary>
        /// Fit bounds within the camera's orthographic view.
        /// </summary>
        public static void FitBoundsInView(this Camera camera, Bounds bounds)
        {
            if (!camera.orthographic)
            {
                Log.Warn("FitBoundsInView only works with orthographic cameras!");
                return;
            }

            float requiredSizeX = bounds.size.x / (2f * camera.aspect);
            float requiredSizeY = bounds.size.y / 2f;
            float requiredSize = Mathf.Max(requiredSizeX, requiredSizeY);

            camera.orthographicSize = requiredSize;
            camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, camera.transform.position.z);
        }
    }
}
