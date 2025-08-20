using System;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Set red component with clamping.
        /// </summary>
        public static Color WithRed(this Color color, float red)
        {
            return new(Mathf.Clamp01(red), color.g, color.b, color.a);
        }

        /// <summary>
        /// Set green component with clamping.
        /// </summary>
        public static Color WithGreen(this Color color, float green)
        {
            return new(color.r, Mathf.Clamp01(green), color.b, color.a);
        }

        /// <summary>
        /// Set blue component with clamping.
        /// </summary>
        public static Color WithBlue(this Color color, float blue)
        {
            return new(color.r, color.g, Mathf.Clamp01(blue), color.a);
        }

        /// <summary>
        /// Set alpha component with clamping.
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        }

        /// <summary>
        /// Multiply RGB components with clamping.
        /// </summary>
        public static Color MultiplyRGB(this Color color, float multiplier)
        {
            multiplier = Mathf.Max(0f, multiplier);
            return new Color(
                Mathf.Clamp01(color.r * multiplier),
                Mathf.Clamp01(color.g * multiplier),
                Mathf.Clamp01(color.b * multiplier),
                color.a
            );
        }

        /// <summary>
        /// Add values to RGB components with clamping.
        /// </summary>
        public static Color AddRGB(this Color color, float value)
        {
            return new Color(
                Mathf.Clamp01(color.r + value),
                Mathf.Clamp01(color.g + value),
                Mathf.Clamp01(color.b + value),
                color.a
            );
        }

        /// <summary>
        /// Get perceived brightness using luminance formula.
        /// </summary>
        public static float GetLuminance(this Color color)
        {
            return 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
        }

        /// <summary>
        /// Invert color (preserves alpha).
        /// </summary>
        public static Color Invert(this Color color)
        {
            return new(1f - color.r, 1f - color.g, 1f - color.b, color.a);
        }

        /// <summary>
        /// Convert to grayscale using luminance.
        /// </summary>
        public static Color ToGrayscale(this Color color)
        {
            float gray = color.GetLuminance();
            return new(gray, gray, gray, color.a);
        }

        /// <summary>
        /// Lerp with parameter validation.
        /// </summary>
        public static Color LerpTo(this Color from, Color to, float t)
        {
            return Color.Lerp(from, to, Mathf.Clamp01(t));
        }

        /// <summary>
        /// Converts the color to a hex string.
        /// </summary>
        public static string ColorToHex(this Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        }

        /// <summary>
        /// Converts a hex string to a color.
        /// </summary>
        public static Color HexToColor(this string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            throw new ArgumentException("Invalid hex string", nameof(hex));
        }
    }
}

