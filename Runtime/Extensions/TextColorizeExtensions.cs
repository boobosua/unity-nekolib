using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class TextColorizeExtensions
    {
        private static readonly Regex s_wordRegex = new(@"\b\w+\b");
        private static readonly Regex s_charRegex = new(@".");

        /// <summary>Wraps the text in a color tag using a hex string. Invalid hex returns the text unchanged.</summary>
        public static string Colorize(this string text, string hexColorCode)
        {
            if (!ColorUtility.TryParseHtmlString(hexColorCode, out var _))
            {
                Log.Warn($"Cannot parse hex color code: {hexColorCode}");
                return text;
            }

            return $"<color={hexColorCode}>{text}</color>";
        }

        /// <summary>Wraps the entire text in a color tag.</summary>
        public static string Colorize(this string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        /// <summary>Wraps a single character in a color tag, returning the tagged string.</summary>
        public static string Colorize(this char character, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{character}</color>";
        }

        /// <summary>Wraps every word-boundary match of the given word in a color tag.</summary>
        public static string Colorize(this string text, Color color, string word)
        {
            return s_wordRegex.Replace(text, match => match.Value == word ? $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{word}</color>" : match.Value);
        }

        /// <summary>Wraps every occurrence of the given character in a color tag.</summary>
        public static string Colorize(this string text, Color color, char character)
        {
            return s_charRegex.Replace(text, match => match.Value[0] == character ? $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{character}</color>" : match.Value);
        }

        /// <summary>Wraps every word-boundary match of any of the given words in a color tag.</summary>
        public static string Colorize(this string text, Color color, params string[] words)
        {
            var set = new HashSet<string>(words);
            string hex = ColorUtility.ToHtmlStringRGB(color);
            return s_wordRegex.Replace(text, match => set.Contains(match.Value)
                ? $"<color=#{hex}>{match.Value}</color>"
                : match.Value);
        }

        /// <summary>Wraps every occurrence of any of the given characters in a color tag.</summary>
        public static string Colorize(this string text, Color color, params char[] characters)
        {
            var set = new HashSet<char>(characters);
            string hex = ColorUtility.ToHtmlStringRGB(color);
            return s_charRegex.Replace(text, match => set.Contains(match.Value[0])
                ? $"<color=#{hex}>{match.Value[0]}</color>"
                : match.Value);
        }

        /// <summary>Wraps every word for which the predicate returns true in a color tag.</summary>
        public static string Colorize(this string text, Color color, Func<string, bool> predicate)
        {
            return s_wordRegex.Replace(text, match => predicate(match.Value) ? $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{match.Value}</color>" : match.Value);
        }
    }
}
