using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Parses a string with comma as decimal separator to float.
        /// </summary>
        public static float ParseFloatWithComma(this string input)
        {
            // Null or empty check
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string cannot be null or empty.", nameof(input));
            }

            // Trim leading and trailing whitespace
            string trimmedInput = input.Trim();

            // Replace comma with period
            string standardizedInput = trimmedInput.Replace(",", ".");

            // Use InvariantCulture for parsing
            if (float.TryParse(standardizedInput, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }
            else
            {
                throw new FormatException($"Invalid input string: \"{input}\".  Could not parse to float.");
            }
        }

        /// <summary>
        /// Non-throwing version of ParseFloatWithComma.
        /// </summary>
        public static bool TryParseFloatWithComma(this string input, out float result)
        {
            result = 0f;

            if (string.IsNullOrEmpty(input))
                return false;

            // Trim leading and trailing whitespace
            string trimmedInput = input.Trim();

            // Replace comma with period
            string standardizedInput = trimmedInput.Replace(",", ".");

            // Use InvariantCulture for parsing
            return float.TryParse(standardizedInput, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Formats a number directly as percentage (25 → 25%).
        /// </summary>
        public static string AsPercent(this float value, int decimalPlaces = 0)
        {
            return value.ToString($"F{decimalPlaces}") + "%";
        }

        /// <summary>
        /// Converts progress value (0-1 range) to percentage string (0.5f → 50%).
        /// </summary>
        public static string AsProgressPercent(this float progress, int decimalPlaces = 0)
        {
            var clampedProgress = Mathf.Clamp01(progress);
            var result = clampedProgress.ToString($"P{decimalPlaces}");
            return result.WithoutSpaces();
        }

        /// <summary>
        /// Formats a float value as percentage string without spaces.
        /// </summary>
        [Obsolete("ToPercentString is obsolete. Use AsProgressPercent() for 0-1 range values or AsPercent() for direct percentage values.", false)]
        public static string ToPercentString(this float value, int decimalPlaces = 0)
        {
            return value.ToString($"P{decimalPlaces}")
                .Replace(" ", "");
        }

        /// <summary>
        /// Removes all spaces from string. Null-safe.
        /// </summary>
        public static string WithoutSpaces(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return str.Replace(" ", string.Empty);
        }

        private static readonly Regex s_uppercaseSplitRegex = new(@"(?<!^)(?=[A-Z])", RegexOptions.Compiled);

        /// <summary>
        /// Splits camelCase/PascalCase by inserting spaces before uppercase letters.
        /// </summary>
        public static string SplitCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var split = s_uppercaseSplitRegex.Split(str);
            return string.Join(" ", split);
        }

        private static readonly string[] s_units =
        {
            "K", "M", "B", "T",
            "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj",
            "kk", "ll", "mm", "nn", "oo", "pp", "qq", "rr", "ss", "tt",
            "uu", "vv", "ww", "xx", "yy", "zz"
        };

        private static readonly string[] s_alphaUnits =
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
        };

        /// <summary>
        /// Formats large numbers into short, readable format (1000 → 1K, 1000000 → 1M).
        /// </summary>
        public static string ToShortFormat(this decimal value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places cannot be negative.");

            // Handle negative values
            bool isNegative = value < 0;
            if (isNegative)
                value = -value;

            // Fast path for small values - fix the formatting issue
            if (value < 1000)
            {
                // For small values, only show decimals if they exist and are needed
                if (value == Math.Floor(value))
                    return (isNegative ? "-" : "") + value.ToString("0");
                else
                    return (isNegative ? "-" : "") + value.ToString("0.########").TrimEnd('0').TrimEnd('.');
            }

            int unitIndex = -1;
            while (value >= 1000 && unitIndex < s_units.Length - 1)
            {
                value /= 1000;
                unitIndex++;
            }

            // Use StringBuilder or direct formatting for better performance
            string formatString = decimalPlaces > 0 ? $"F{decimalPlaces}" : "F0";
            string formattedValue = value.ToString(formatString);

            // Optimize trailing zero removal
            if (decimalPlaces > 0 && formattedValue.Contains('.'))
            {
                formattedValue = formattedValue.TrimEnd('0').TrimEnd('.');
            }

            return (isNegative ? "-" : "") + formattedValue + s_units[unitIndex];
        }

        /// <summary>
        /// Formats large numbers into short, readable format. Long overload.
        /// </summary>
        public static string ToShortFormat(this long value, int decimalPlaces = 1)
        {
            return ToShortFormat((decimal)value, decimalPlaces);
        }

        /// <summary>
        /// Formats large numbers into short, readable format. Int overload.
        /// </summary>
        public static string ToShortFormat(this int value, int decimalPlaces = 1)
        {
            return ToShortFormat((decimal)value, decimalPlaces);
        }

        /// <summary>
        /// Formats large numbers with alphabetic suffixes (1000 → 1a, 1000000 → 1b).
        /// </summary>
        public static string ToShortABCFormat(this decimal value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places cannot be negative.");

            // Handle negative values
            bool isNegative = value < 0;
            if (isNegative)
                value = -value;

            // Fast path for small values
            if (value < 1000)
            {
                if (value == Math.Floor(value))
                    return (isNegative ? "-" : "") + value.ToString("0");
                else
                    return (isNegative ? "-" : "") + value.ToString("0.########").TrimEnd('0').TrimEnd('.');
            }

            int unitIndex = -1;
            while (value >= 1000 && unitIndex < s_alphaUnits.Length - 1)
            {
                value /= 1000;
                unitIndex++;
            }

            string formatString = decimalPlaces > 0 ? $"F{decimalPlaces}" : "F0";
            string formattedValue = value.ToString(formatString);

            if (decimalPlaces > 0 && formattedValue.Contains('.'))
            {
                formattedValue = formattedValue.TrimEnd('0').TrimEnd('.');
            }

            return (isNegative ? "-" : "") + formattedValue + s_alphaUnits[unitIndex];
        }

        /// <summary>
        /// Formats large numbers with alphabetic suffixes. Long overload.
        /// </summary>
        public static string ToShortABCFormat(this long value, int decimalPlaces = 1)
        {
            return ToShortABCFormat((decimal)value, decimalPlaces);
        }

        /// <summary>
        /// Formats large numbers with alphabetic suffixes. Int overload.
        /// </summary>
        public static string ToShortABCFormat(this int value, int decimalPlaces = 1)
        {
            return ToShortABCFormat((decimal)value, decimalPlaces);
        }
    }
}

