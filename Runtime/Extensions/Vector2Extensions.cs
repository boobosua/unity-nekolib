using UnityEngine;

namespace NekoLib.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(x ?? vector.x, y ?? vector.y);
        }

        public static Vector2 Add(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x + (x ?? 0), vector.y + (y ?? 0));
        }

        public static Vector2 Subtract(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x - (x ?? 0), vector.y - (y ?? 0));
        }

        public static Vector2 Multiply(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x * (x ?? 1), vector.y * (y ?? 1));
        }

        public static Vector2 Divide(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(
                x.HasValue && x.Value != 0 ? vector.x / x.Value : vector.x,
                y.HasValue && y.Value != 0 ? vector.y / y.Value : vector.y
            );
        }

        /// <summary>
        /// Returns a Boolean indicating whether the current Vector2 is in a given range from another Vector2
        /// </summary>
        public static bool InRangeOf(this Vector2 current, Vector2 target, float range)
        {
            return (current - target).sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Computes a random point in an annulus (a ring-shaped area) based on minimum and 
        /// maximum radius values around a central Vector2 point (origin).
        /// </summary>
        public static Vector2 RandomPointInAnnulus(this Vector2 origin, float minRadius, float maxRadius)
        {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

            // Squaring and then square-rooting radii to ensure uniform distribution within the annulus
            float minRadiusSquared = minRadius * minRadius;
            float maxRadiusSquared = maxRadius * maxRadius;
            float distance = Mathf.Sqrt(Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

            // Calculate the position vector
            Vector2 position = direction * distance;
            return origin + position;
        }
    }
}

