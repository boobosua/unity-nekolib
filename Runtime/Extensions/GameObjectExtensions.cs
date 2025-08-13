using System;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class GameObjectExtensions
    {
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out var requestedComponent))
            {
                return requestedComponent;
            }

            return gameObject.AddComponent<T>();
        }

        public static void SetActive<T>(this T obj) where T : MonoBehaviour
        {
            obj.gameObject.SetActive(true);
        }

        public static void SetInactive<T>(this T obj) where T : MonoBehaviour
        {
            obj.gameObject.SetActive(false);
        }

        public static bool IsInLayer(this GameObject gameObject, LayerMask layer)
        {
            return (layer & (1 << gameObject.layer)) != 0;
        }

        public static void ClearChildTransforms(this GameObject gameObject)
        {
            gameObject.transform.Clear();
        }

        [Obsolete("Use ClearChildTransforms instead.")]
        public static void DestroyChildren(this GameObject gameObject) => ClearChildTransforms(gameObject);
    }
}

