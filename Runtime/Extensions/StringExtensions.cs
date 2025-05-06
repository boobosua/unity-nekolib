using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NekoLib.Extensions
{
    public static class StringExtensions
    {
        public static float ParseCommaToFloat(this string input)
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

        public static string ToPercentage(this float value, int decimalPlaces = 0)
        {
            return value.ToString($"P{decimalPlaces}")
                .Replace(" ", "");
        }

        public static string RemoveSpaces(this string str)
        {
            return str.Replace(" ", string.Empty);
        }

        public static string SplitByUppercase(this string str)
        {
            var split = Regex.Split(str, @"(?<!^)(?=[A-Z])");
            return string.Join(" ", split);
        }

        private static readonly string[] s_units =
        {
            "K", "M", "B", "T",
            "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj",
            "kk", "ll", "mm", "nn", "oo", "pp", "qq", "rr", "ss", "tt",
            "uu", "vv", "ww", "xx", "yy", "zz"
        };

        public static string BigFormat(this decimal value, int decimalPlaces = 1)
        {
            if (value < 1000)
            {
                return value.ToString();
            }

            int unitIndex = -1;
            while (value >= 1000 && unitIndex < s_units.Length - 1)
            {
                value /= 1000;
                unitIndex++;
            }

            string formattedValue = value.ToString($"F{decimalPlaces}");
            if (decimalPlaces > 0 && formattedValue.EndsWith(new string('0', decimalPlaces)))
            {
                formattedValue = formattedValue.TrimEnd('0');
                if (formattedValue.EndsWith("."))
                {
                    formattedValue = formattedValue.TrimEnd('.');
                }
            }

            return formattedValue + s_units[unitIndex];
        }

        public static string BigFormat(this long value, int decimalPlaces = 1)
        {
            return BigFormat((decimal)value, decimalPlaces);
        }

        public static string BigFormat(this int value, int decimalPlaces = 1)
        {
            return BigFormat((decimal)value, decimalPlaces);
        }
    }
}

