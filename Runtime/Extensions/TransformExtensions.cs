using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NekoLib.Extensions
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Destroys all child objects of the specified transform.
        /// </summary>
        /// <param name="transform">The transform whose children will be destroyed.</param>
        public static void Clear(this Transform transform)
        {
            while (transform.childCount > 0)
            {
                Transform child = transform.transform.GetChild(0);
                child.SetParent(null);
#if UNITY_EDITOR
                if (Application.isPlaying)
                    Object.Destroy(child.gameObject);
                else
                    Object.DestroyImmediate(child.gameObject);
#else
                Object.Destroy(child.gameObject);
#endif
            }
        }

        [Obsolete("Use Clear() instead.")]
        public static void DestroyChildren(this Transform transform) => Clear(transform);
    }
}

