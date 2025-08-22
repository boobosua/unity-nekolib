using UnityEngine;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
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

        /// <summary>
        /// Checks if the pointer is over a 2D object.
        /// </summary>
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

        /// <summary>
        /// Checks if the pointer is over a 2D object.
        /// </summary>
        public static bool IsPointerOver2DObject<T>() where T : Component
        {
            var position = GetMousePosition2D();
            var hit = Physics2D.OverlapPoint(position);
            return hit != null && hit.GetComponent<T>() != null;
        }

        /// <summary>
        /// Checks if the pointer is over a 2D object.
        /// </summary>
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
    }
}
