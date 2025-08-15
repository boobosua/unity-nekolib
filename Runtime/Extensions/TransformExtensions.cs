using System;
using System.Collections.Generic;
using UnityEngine;
using NekoLib.Utilities;

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
            if (includeInactive)
            {
                var children = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                {
                    children[i] = transform.GetChild(i);
                }
                return children;
            }
            else
            {
                var activeChildren = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (child.gameObject.activeInHierarchy)
                    {
                        activeChildren.Add(child);
                    }
                }
                return activeChildren.ToArray();
            }
        }

        [Obsolete("Use Clear() instead.")]
        public static void DestroyChildren(this Transform transform) => Clear(transform);

        /// <summary>
        /// Orbits this transform around a target transform.
        /// </summary>
        public static void OrbitAround(this Transform transform, Transform target, float horizontal, float vertical, float distance)
        {
            if (target == null)
            {
                Debug.LogWarning($"OrbitAround: Target transform is null for {transform.name}");
                return;
            }
            OrbitAround(transform, target.position, horizontal, vertical, distance);
            transform.LookAt(target);
        }

        /// <summary>
        /// Orbits this transform around a target position.
        /// </summary>
        public static void OrbitAround(this Transform transform, Vector3 targetPosition, float horizontal, float vertical, float distance)
        {
            // Calculate orbital position
            var offset = Vector3.back * distance;
            offset = Quaternion.AngleAxis(vertical, Vector3.right) * offset;
            offset = Quaternion.AngleAxis(horizontal, Vector3.up) * offset;

            transform.position = targetPosition + offset;
        }

        /// <summary>
        /// Orbits around a target with clamped vertical angles to prevent flipping.
        /// </summary>
        public static void OrbitAroundClamped(this Transform transform, Transform target, float horizontal, float vertical, float distance, float minVertical = -80f, float maxVertical = 80f)
        {
            if (target == null)
            {
                Debug.LogWarning($"OrbitAroundClamped: Target transform is null for {transform.name}");
                return;
            }
            vertical = Mathf.Clamp(vertical, minVertical, maxVertical);
            OrbitAround(transform, target, horizontal, vertical, distance);
        }

        /// <summary>
        /// Rotates this transform around a point in world space.
        /// </summary>
        public static void RotateAround(this Transform transform, Vector3 center, Vector3 axis, float degrees)
        {
            var offset = transform.position - center;
            offset = Quaternion.AngleAxis(degrees, axis) * offset;
            transform.position = center + offset;
            transform.Rotate(axis, degrees, Space.World);
        }

        /// <summary>
        /// Makes transform look at target in 2D (Z-axis rotation only).
        /// </summary>
        public static void LookAt2D(this Transform transform, Vector2 target)
        {
            Vector2 direction = ((Vector2)transform.position).DirectionTo(target);
            float angle = Utils.GetAngleFromVector(direction);
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        /// <summary>
        /// Makes transform look at target in 2D with offset angle.
        /// </summary>
        public static void LookAt2D(this Transform transform, Vector2 target, float angleOffset)
        {
            var direction = ((Vector2)transform.position).DirectionTo(target);
            float angle = Utils.GetAngleFromVector(direction) + angleOffset;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        /// <summary>
        /// Makes transform look at another transform in 2D.
        /// </summary>
        public static void LookAt2D(this Transform transform, Transform target)
        {
            if (target == null)
            {
                Debug.LogWarning($"LookAt2D: Target transform is null for {transform.name}");
                return;
            }
            transform.LookAt2D(target.position);
        }

        /// <summary>
        /// Makes transform look at another transform in 2D with offset angle.
        /// </summary>
        public static void LookAt2D(this Transform transform, Transform target, float angleOffset)
        {
            if (target == null)
            {
                Debug.LogWarning($"LookAt2D: Target transform is null for {transform.name}");
                return;
            }
            transform.LookAt2D(target.position, angleOffset);
        }

        /// <summary>
        /// Gets distance to another transform.
        /// </summary>
        public static float DistanceTo(this Transform transform, Transform other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), $"DistanceTo: Target transform is null for {transform.name}");
            }
            return transform.position.DistanceTo(other.position);
        }

        /// <summary>
        /// Gets direction to another transform.
        /// </summary>
        public static Vector3 DirectionTo(this Transform transform, Transform other)
        {
            if (other == null)
            {
                Debug.LogWarning($"DirectionTo: Target transform is null for {transform.name}");
                return Vector3.zero;
            }
            return transform.position.DirectionTo(other.position);
        }

        /// <summary>
        /// Checks if transform is within range of another transform.
        /// </summary>
        public static bool InRangeOf(this Transform transform, Transform other, float range)
        {
            if (other == null)
            {
                Debug.LogWarning($"InRangeOf: Target transform is null for {transform.name}");
                return false;
            }
            return transform.position.InRangeOf(other.position, range);
        }

        /// <summary>
        /// Resets transform to default values (position: zero, rotation: identity, scale: one).
        /// </summary>
        public static void ResetTransform(this Transform transform)
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Resets local transform to default values.
        /// </summary>
        public static void ResetLocalTransform(this Transform transform)
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }
    }
}

