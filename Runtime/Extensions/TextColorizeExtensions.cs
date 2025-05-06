using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class TextColorizeExtensions
    {
        private static readonly Regex s_wordRegex = new(@"\b\w+\b");
        private static readonly Regex s_charRegex = new(@".");

        public static string Colorize(this string text, string hexColorCode)
        {
            if (!ColorUtility.TryParseHtmlString(hexColorCode, out var _))
            {
                Debug.LogWarning($"Cannot parse hex color code: {hexColorCode}");
                return text;
            }

            return $"<color={hexColorCode}>{text}</color>";
        }

        public static string Colorize(this string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        public static string Colorize(this char character, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{character}</color>";
        }

        public static string Colorize(this string text, Color color, string word)
        {
            return s_wordRegex.Replace(text, match => match.Value == word ? $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{word}</color>" : match.Value);
        }

        public static string Colorize(this string text, Color color, char character)
        {
            return s_charRegex.Replace(text, match => match.Value[0] == character ? $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{character}</color>" : match.Value);
        }

        public static string Colorize(this string text, Color color, params string[] words)
        {
            foreach (var word in words)
            {
                text = text.Colorize(color, word);
            }
            return text;
        }

        public static string Colorize(this string text, Color color, params char[] characters)
        {
            foreach (var character in characters)
            {
                text = text.Colorize(color, character);
            }
            return text;
        }

        public static string Colorize(this string text, Color color, Func<char, bool> predicate)
        {
            return s_charRegex.Replace(text, match => predicate(match.Value[0]) ? $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{match.Value}</color>" : match.Value);
        }

        public static string Colorize(this string text, Color color, Func<string, bool> predicate)
        {
            return s_wordRegex.Replace(text, match => predicate(match.Value) ? $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{match.Value}</color>" : match.Value);
        }
    }
}

