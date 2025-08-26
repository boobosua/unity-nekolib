using System;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets or adds a component of type T to the specified MonoBehaviour.
        /// </summary>
        public static T GetOrAdd<T>(this MonoBehaviour monoBehaviour) where T : Component
        {
            return monoBehaviour.gameObject.GetOrAdd<T>();
        }

        /// <summary>
        /// Gets or adds a component of type T to the specified GameObject.
        /// </summary>
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out var requestedComponent))
            {
                return requestedComponent;
            }

            return gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Sets the active state of the specified MonoBehaviour to true.
        /// </summary>
        public static void SetActive<T>(this T obj) where T : MonoBehaviour
        {
            obj.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets the active state of the specified MonoBehaviour to false.
        /// </summary>
        public static void SetInactive<T>(this T obj) where T : MonoBehaviour
        {
            obj.gameObject.SetActive(false);
        }

        /// <summary>
        /// Checks if the specified GameObject is in the given LayerMask.
        /// </summary>
        public static bool IsInLayer(this GameObject gameObject, LayerMask layer)
        {
            return (layer & (1 << gameObject.layer)) != 0;
        }

        /// <summary>
        /// Clears all child transforms of the specified GameObject.
        /// </summary>
        public static void ClearChildTransforms(this GameObject gameObject)
        {
            gameObject.transform.Clear();
        }

        [Obsolete("Use ClearChildTransforms instead.")]
        public static void DestroyChildren(this GameObject gameObject) => ClearChildTransforms(gameObject);

        /// <summary>
        /// Set GameObject to a specific layer.
        /// </summary>
        public static void SetLayer(this GameObject gameObject, int newLayer)
        {
            gameObject.layer = newLayer;
        }

        /// <summary>
        /// Set GameObject to a layer using LayerMask (uses the first layer found in the mask).
        /// </summary>
        public static void SetLayer(this GameObject gameObject, LayerMask layerMask)
        {
            // Convert LayerMask to layer number by finding the first set bit
            int layerNumber = 0;
            int mask = layerMask.value;
            while (mask > 1)
            {
                mask >>= 1;
                layerNumber++;
            }
            gameObject.layer = layerNumber;
        }

        /// <summary>
        /// Set GameObject to a layer by name.
        /// </summary>
        public static void SetLayer(this GameObject gameObject, string layerName)
        {
            int layerNumber = LayerMask.NameToLayer(layerName);
            if (layerNumber != -1)
            {
                gameObject.layer = layerNumber;
            }
            else
            {
                Debug.LogWarning($"Layer '{layerName}' not found!");
            }
        }
    }
}

