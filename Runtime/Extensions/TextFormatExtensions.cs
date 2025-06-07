using System;
using System.Text.RegularExpressions;

namespace NekoLib.Extensions
{
    public static class TextFormatExtensions
    {
        private static readonly Regex s_wordRegex = new(@"\b\w+\b");
        private static readonly Regex s_charRegex = new(@".");

        public static string Bold(this string text)
        {
            return $"<b>{text}</b>";
        }

        public static string Bold(this string text, string word)
        {
            return s_wordRegex.Replace(text, match => match.Value == word ? $"<b>{word}</b>" : match.Value);
        }

        public static string Bold(this string text, params string[] words)
        {
            foreach (var word in words)
            {
                text = text.Bold(word);
            }
            return text;
        }

        public static string Bold(this string text, Func<bool> predicate)
        {
            return predicate() ? $"<b>{text}</b>" : text;
        }

        public static string Bold(this string text, Func<string, bool> predicate)
        {
            return s_wordRegex.Replace(text, match => predicate(match.Value) ? $"<b>{match.Value}</b>" : match.Value);
        }

        public static string Bold(this char character)
        {
            return $"<b>{character}</b>";
        }

        public static string Bold(this string text, char character)
        {
            return s_charRegex.Replace(text, match => match.Value == character.ToString() ? $"<b>{character}</b>" : match.Value);
        }

        public static string Italic(this string text)
        {
            return $"<i>{text}</i>";
        }

        public static string Italic(this string text, string word)
        {
            return s_wordRegex.Replace(text, match => match.Value == word ? $"<i>{word}</i>" : match.Value);
        }

        public static string Italic(this string text, params string[] words)
        {
            foreach (var word in words)
            {
                text = text.Italic(word);
            }
            return text;
        }

        public static string Italic(this string text, Func<bool> predicate)
        {
            return predicate() ? $"<i>{text}</i>" : text;
        }

        public static string Italic(this string text, Func<string, bool> predicate)
        {
            return s_wordRegex.Replace(text, match => predicate(match.Value) ? $"<i>{match.Value}</i>" : match.Value);
        }

        public static string Italic(this char character)
        {
            return $"<i>{character}</i>";
        }

        public static string Italic(this string text, char character)
        {
            return s_charRegex.Replace(text, match => match.Value == character.ToString() ? $"<i>{character}</i>" : match.Value);
        }

        public static string Underline(this string text)
        {
            return $"<u>{text}</u>";
        }

        public static string Underline(this string text, string word)
        {
            return s_wordRegex.Replace(text, match => match.Value == word ? $"<u>{word}</u>" : match.Value);
        }

        public static string Underline(this string text, params string[] words)
        {
            foreach (var word in words)
            {
                text = text.Underline(word);
            }
            return text;
        }

        public static string Underline(this string text, Func<bool> predicate)
        {
            return predicate() ? $"<u>{text}</u>" : text;
        }

        public static string Underline(this string text, Func<string, bool> predicate)
        {
            return s_wordRegex.Replace(text, match => predicate(match.Value) ? $"<u>{match.Value}</u>" : match.Value);
        }

        public static string Underline(this char character)
        {
            return $"<u>{character}</u>";
        }

        public static string Underline(this string text, char character)
        {
            return s_charRegex.Replace(text, match => match.Value == character.ToString() ? $"<u>{character}</u>" : match.Value);
        }
    }
}
