using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
        /// <summary>Returns the angle of a Vector3 in degrees.</summary>
        public static float GetAngleFromVector(Vector3 vector3)
        {
            var radian = Mathf.Atan2(vector3.y, vector3.x);
            var degrees = radian * Mathf.Rad2Deg;
            return degrees;
        }

        private static readonly Vector2 s_defaultAngleRange = new(0f, 360f);

        /// <summary>Returns a random rotation based on the specified axis and ranges for each axis.</summary>
        public static Quaternion GetRandomRotation(Axis axis, Vector2? xRange = null, Vector2? yRange = null, Vector2? zRange = null)
        {
            var rx = xRange ?? s_defaultAngleRange;
            var ry = yRange ?? s_defaultAngleRange;
            var rz = zRange ?? s_defaultAngleRange;

            float angleX = axis.HasFlag(Axis.X) ? Random.Range(rx.x, rx.y) : 0f;
            float angleY = axis.HasFlag(Axis.Y) ? Random.Range(ry.x, ry.y) : 0f;
            float angleZ = axis.HasFlag(Axis.Z) ? Random.Range(rz.x, rz.y) : 0f;

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
