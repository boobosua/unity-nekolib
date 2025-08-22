using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
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
        /// Returns a random rotation based on the specified axis and ranges for each axis.
        /// </summary>
        public static Quaternion GetRandomRotation(Axis axis, Vector2 xRange = default, Vector2 yRange = default, Vector2 zRange = default)
        {
            // If ranges not specified, default to 0-360
            if (xRange == default) xRange = new Vector2(0f, 360f);
            if (yRange == default) yRange = new Vector2(0f, 360f);
            if (zRange == default) zRange = new Vector2(0f, 360f);

            float angleX = axis.HasFlag(Axis.X) ? Random.Range(xRange.x, xRange.y) : 0f;
            float angleY = axis.HasFlag(Axis.Y) ? Random.Range(yRange.x, yRange.y) : 0f;
            float angleZ = axis.HasFlag(Axis.Z) ? Random.Range(zRange.x, zRange.y) : 0f;

            return Quaternion.Euler(angleX, angleY, angleZ);
        }
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

    public enum Orientation
    {
        Horizontal = 0,
        Vertical = 1
    }
}
