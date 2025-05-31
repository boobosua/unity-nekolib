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

        /// <summary>
        /// Returns 'true' if we touched or hovering on Unity UI element.
        /// </summary>
        public static bool IsPointerOverUIElement(LayerMask layer)
        {
            var eventSystemRaycastResults = GetEventSystemRaycastResults();

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
            PointerEventData eventData = new(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> raycastResults = new();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            return raycastResults;
        }

        /// <summary>
        /// Detect whether the cursor is focused on the game window or not.
        /// </summary>
        public static bool IsMouseOverGameWindow
        {
            get
            {
                Vector3 mp = Input.mousePosition;
                return !(0 > mp.x || 0 > mp.y || Screen.width < mp.x || Screen.height < mp.y);
            }
        }

        /// <summary>
        /// Returns the world position of the mouse cursor.
        /// </summary>
        public static Vector3 MouseWorldPosition
        {
            get
            {
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0f;
                return mouseWorldPos;
            }
        }

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
        public static Quaternion GetRandomRotation(Axis axis)
        {
            float angleX = axis.HasFlag(Axis.X) ? Random.Range(0f, 360f) : 0f;
            float angleY = axis.HasFlag(Axis.Y) ? Random.Range(0f, 360f) : 0f;
            float angleZ = axis.HasFlag(Axis.Z) ? Random.Range(0f, 360f) : 0f;

            return Quaternion.Euler(angleX, angleY, angleZ);
        }

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
