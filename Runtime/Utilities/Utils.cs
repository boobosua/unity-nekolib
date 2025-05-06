using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NekoLib.Extensions;
using Random = UnityEngine.Random;

namespace NekoLib.Utilities
{
    public static class Utils
    {
        // Dictionary caching all WaitForSeconds to avoid unnecessary new memory allocations.
        private static readonly Dictionary<float, WaitForSeconds> s_waitForSecondsDictionary = new();

        /// <summary>
        /// Retrieve a cached WaitForSeconds stored in a dictionary to avoid new allocation of memory.
        /// </summary>
        /// <param name="waitSeconds"> Seconds to wait for or alternatively a key to retrieve a WaitForSeconds in the dictionary. </param>
        /// <returns></returns>
        public static WaitForSeconds GetWaitForSeconds(float waitSeconds)
        {
            if (s_waitForSecondsDictionary.TryGetValue(waitSeconds, out var wait))
            {
                return wait;
            }

            s_waitForSecondsDictionary.Add(waitSeconds, new WaitForSeconds(waitSeconds));

            return s_waitForSecondsDictionary[waitSeconds];
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

        /// <summary>
        /// Returns a random rotation based on the given axis.
        /// </summary>
        public static Quaternion GetRandomRotation(Axis axis)
        {
            float angleX = axis.HasFlag(Axis.X) ? UnityEngine.Random.Range(0f, 360f) : 0f;
            float angleY = axis.HasFlag(Axis.Y) ? UnityEngine.Random.Range(0f, 360f) : 0f;
            float angleZ = axis.HasFlag(Axis.Z) ? UnityEngine.Random.Range(0f, 360f) : 0f;

            return Quaternion.Euler(angleX, angleY, angleZ);
        }

        /// <summary>
        /// Returns 'true' if the random roll between min and max is less than or equal to the target roll.
        /// </summary>
        public static bool IsValidRoll(float targetRoll, float min = 0f, float max = 1f)
        {
            var randomRoll = Random.Range(min, max);
            return randomRoll <= targetRoll;
        }

        /// <summary>
        /// Returns 'true' if the random roll between min and max is less than or equal to the target roll.
        /// </summary>
        public static bool IsValidRoll(int targetRoll, int min = 0, int max = 100)
        {
            var randomRoll = Random.Range(min, max);
            return randomRoll <= targetRoll;
        }
    }
}
