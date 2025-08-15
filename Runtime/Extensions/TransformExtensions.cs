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
        public static void Clear(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
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

        /// <summary>
        /// Gets all direct children of this transform.
        /// </summary>
        public static Transform[] GetChildren(this Transform transform, bool includeInactive = false)
        {
            var children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (includeInactive || child.gameObject.activeInHierarchy)
                {
                    children[i] = child;
                }
            }
            return children;
        }

        [Obsolete("Use Clear() instead.")]
        public static void DestroyChildren(this Transform transform) => Clear(transform);
    }
}

