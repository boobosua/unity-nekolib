using System;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class ColorExtensions
    {
        public static Color SetAlpha(this Color color, float alpha)
        {
            return new(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        }

        public static string ToHex(this Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        }

        public static Color FromHex(this string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            throw new ArgumentException("Invalid hex string", nameof(hex));
        }
    }
}

