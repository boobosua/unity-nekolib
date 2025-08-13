using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NekoLib.Extensions;

using Random = UnityEngine.Random;

namespace NekoLib.Utilities
{
    [Flags]
    public enum Axis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
        XY = X | Y,
        XZ = X | Z,
        YZ = Y | Z,
        XYZ = X | Y | Z
    }

    public static class Utils
    {
        #region  Coroutine Cache
        // Dictionary caching all WaitForSeconds to avoid unnecessary new memory allocations.
        private static readonly Dictionary<float, WaitForSeconds> s_waitForSecondsDictionary = new();

        /// <summary>
        /// Retrieve a cached WaitForSeconds stored in a dictionary to avoid new allocation of memory.
        /// </summary>
        public static WaitForSeconds GetWaitForSeconds(float waitSeconds)
        {
            if (s_waitForSecondsDictionary.TryGetValue(waitSeconds, out var wait))
            {
                return wait;
            }

            s_waitForSecondsDictionary.Add(waitSeconds, new WaitForSeconds(waitSeconds));

            return s_waitForSecondsDictionary[waitSeconds];
        }

        private static readonly Dictionary<float, WaitForSecondsRealtime> s_waitForSecondsRealtimeDictionary = new();

        /// <summary>
        /// Retrieve a cached WaitForSecondsRealtime stored in a dictionary to avoid new allocation of memory.
        /// </summary>
        public static WaitForSecondsRealtime GetWaitForSecondsRealtime(float waitSeconds)
        {
            if (s_waitForSecondsRealtimeDictionary.TryGetValue(waitSeconds, out var wait))
            {
                return wait;
            }

            s_waitForSecondsRealtimeDictionary.Add(waitSeconds, new WaitForSecondsRealtime(waitSeconds));

            return s_waitForSecondsRealtimeDictionary[waitSeconds];
        }
        #endregion

        #region 3D Object Detection
        public static bool IsPointerOverAny3DObject(int layerMask = Physics.DefaultRaycastLayers)
        {
            var mouseRay = GetMouseRay();
            return Physics.Raycast(mouseRay, Mathf.Infinity, layerMask);
        }

        public static bool IsPointerOverAny3DObject(float maxDistance, int layerMask = Physics.DefaultRaycastLayers)
        {
            var mouseRay = GetMouseRay();
            return Physics.Raycast(mouseRay, maxDistance, layerMask);
        }

        public static bool IsPointerOverAny3DObject(Camera camera, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            var mouseRay = GetMouseRay(camera);
            return Physics.Raycast(mouseRay, maxDistance, layerMask);
        }

        public static bool IsPointerOverAny3DObject(out RaycastHit hit, Camera camera = null, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            var mouseRay = GetMouseRay(camera);
            return Physics.Raycast(mouseRay, out hit, maxDistance, layerMask);
        }

        public static bool IsPointerOver3DObject(GameObject objectToCheck)
        {
            if (objectToCheck == null)
            {
                Debug.LogWarning("Object to check is null.");
                return false;
            }

            var ray = GetMouseRay();
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity))
            {
                return hit.transform == objectToCheck.transform ||
                    hit.transform.IsChildOf(objectToCheck.transform);
            }

            return false;
        }

        public static bool IsPointerOver3DObject<T>() where T : Component
        {
            var ray = GetMouseRay();
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity))
            {
                return hit.transform.GetComponent<T>() != null;
            }

            return false;
        }

        public static bool IsPointerOver3DObject<T>(out T component) where T : Component
        {
            var ray = GetMouseRay();
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity))
            {
                component = hit.transform.GetComponent<T>();
                return component != null;
            }

            component = null;
            return false;
        }
        #endregion

        #region  2D Object Detection
        /// <summary>
        /// Returns 'true' if the mouse is over any 2D object.
        /// Only works if camera is orthographic.
        /// </summary>
        public static bool IsPointerOverAny2DObject(int layerMask = Physics2D.DefaultRaycastLayers)
        {
            var position = GetMousePosition2D();
            var hit = Physics2D.OverlapPoint(position, layerMask);
            return hit != null;
        }

        /// <summary>
        /// Returns 'true' if the mouse is over any 2D object.
        /// Only works if camera is orthographic.
        /// </summary>
        public static bool IsPointerOverAny2DObject(Camera camera, int layerMask = Physics2D.DefaultRaycastLayers)
        {
            var position = GetMousePosition2D(camera);
            var hit = Physics2D.OverlapPoint(position, layerMask);
            return hit != null;
        }

        /// <summary>
        /// Returns 'true' if the mouse is over any 2D object.
        /// Only works if camera is orthographic.
        /// </summary>
        public static bool IsPointerOverAny2DObject(out Collider2D hit, Camera camera = null, int layerMask = Physics2D.DefaultRaycastLayers)
        {
            var position = GetMousePosition2D(camera);
            hit = Physics2D.OverlapPoint(position, layerMask);
            return hit != null;
        }

        public static bool IsPointerOver2DObject(GameObject objectToCheck)
        {
            if (objectToCheck == null)
            {
                Debug.LogWarning("Object to check is null.");
                return false;
            }

            var position = GetMousePosition2D();
            var hit = Physics2D.OverlapPoint(position);
            return hit != null && (hit.transform == objectToCheck.transform || hit.transform.IsChildOf(objectToCheck.transform));
        }

        public static bool IsPointerOver2DObject<T>() where T : Component
        {
            var position = GetMousePosition2D();
            var hit = Physics2D.OverlapPoint(position);
            return hit != null && hit.GetComponent<T>() != null;
        }

        public static bool IsPointerOver2DObject<T>(out T component) where T : Component
        {
            var position = GetMousePosition2D();
            var hit = Physics2D.OverlapPoint(position);
            if (hit != null)
            {
                component = hit.GetComponent<T>();
                return component != null;
            }

            component = null;
            return false;
        }

        /// <summary>
        /// Returns 'true' if we touched or hovering on Unity UI element.
        /// </summary>
        public static bool IsPointerOverUI(LayerMask layer)
        {
            var eventSystemRaycastResults = GetEventSystemRaycastResults();
            if (eventSystemRaycastResults.IsNullOrEmpty())
                return false;

            for (int i = 0; i < eventSystemRaycastResults.Count; i++)
            {
                RaycastResult curRaycastResult = eventSystemRaycastResults[i];
                if (curRaycastResult.gameObject.IsInLayer(layer))
                    return true;
            }

            return false;
        }

        // Gets all event system raycast results of current mouse or touch position.
        private static List<RaycastResult> GetEventSystemRaycastResults()
        {
            if (EventSystem.current == null)
            {
                Debug.LogWarning("EventSystem is null. Returning empty raycast results.");
                return new List<RaycastResult>();
            }

            PointerEventData eventData = new(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> raycastResults = new();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            return raycastResults;
        }
        #endregion

        #region Mouse Detection
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
        #endregion

        #region Rotation
        /// <summary>
        /// Returns the angle of a Vector3 in degrees.
        /// </summary>
        public static float GetAngleFromVector(Vector3 vector3)
        {
            var radian = Mathf.Atan2(vector3.y, vector3.x);
            var degrees = radian * Mathf.Rad2Deg;
            return degrees;
        }

        /// <summary>
        /// Returns a random rotation based on the given axis.
        /// </summary>
        public static Quaternion GetRandomRotation(Axis axis, Vector2 xRange = default, Vector2 yRange = default, Vector2 zRange = default)
        {
            // If ranges not specified, default to 0-360
            if (xRange == default) xRange = new Vector2(0f, 360f);
            if (yRange == default) yRange = new Vector2(0f, 360f);
            if (zRange == default) zRange = new Vector2(0f, 360f);

            float angleX = axis.HasFlag(Axis.X) ? Random.Range(xRange.x, xRange.y) : 0f;
            float angleY = axis.HasFlag(Axis.Y) ? Random.Range(yRange.x, yRange.y) : 0f;
            float angleZ = axis.HasFlag(Axis.Z) ? Random.Range(zRange.x, zRange.y) : 0f;

            return Quaternion.Euler(angleX, angleY, angleZ);
        }
        #endregion

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
#endif
    }
}
