using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
        /// <summary>
        /// Returns 'true' if the mouse is over any 3D object.
        /// </summary>
        public static bool IsPointerOverAny3DObject(int layerMask = Physics.DefaultRaycastLayers)
        {
            var mouseRay = GetMouseRay();
            return Physics.Raycast(mouseRay, Mathf.Infinity, layerMask);
        }

        /// <summary>
        /// Returns 'true' if the mouse is over any 3D object.
        /// </summary>
        public static bool IsPointerOverAny3DObject(float maxDistance, int layerMask = Physics.DefaultRaycastLayers)
        {
            var mouseRay = GetMouseRay();
            return Physics.Raycast(mouseRay, maxDistance, layerMask);
        }

        /// <summary>
        /// Returns 'true' if the mouse is over any 3D object.
        /// </summary>
        public static bool IsPointerOverAny3DObject(Camera camera, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            var mouseRay = GetMouseRay(camera);
            return Physics.Raycast(mouseRay, maxDistance, layerMask);
        }

        /// <summary>
        /// Returns 'true' if the mouse is over any 3D object.
        /// </summary>
        public static bool IsPointerOverAny3DObject(out RaycastHit hit, Camera camera = null, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            var mouseRay = GetMouseRay(camera);
            return Physics.Raycast(mouseRay, out hit, maxDistance, layerMask);
        }

        /// <summary>
        /// Checks if the pointer is over a 3D object.
        /// </summary>
        public static bool IsPointerOver3DObject(GameObject objectToCheck)
        {
            if (objectToCheck == null)
            {
                Log.Warn("Object to check is null.");
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

        /// <summary>
        /// Checks if the pointer is over a 3D object of the specified type.
        /// </summary>
        public static bool IsPointerOver3DObject<T>() where T : Component
        {
            var ray = GetMouseRay();
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity))
            {
                return hit.transform.GetComponent<T>() != null;
            }

            return false;
        }

        /// <summary>
        /// Checks if the pointer is over a 3D object of the specified type.
        /// </summary>
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
    }
}
