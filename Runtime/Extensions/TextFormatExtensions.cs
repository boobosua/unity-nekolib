using System;
using System.Text.RegularExpressions;

namespace NekoLib.Extensions
{
    public static class TextFormatExtensions
    {
        private static readonly Regex s_wordRegex = new(@"\b\w+\b");

        /// <summary>
        /// Wraps the entire string in <b> tags.
        /// </summary>
        public static string Bold(this string text)
        {
            return $"<b>{text}</b>";
        }

        /// <summary>
        /// Wraps the specified word in <b> tags.
        /// </summary>
        public static string Bold(this string text, string word)
        {
            return s_wordRegex.Replace(text, match => match.Value == word ? $"<b>{word}</b>" : match.Value);
        }

        /// <summary>
        /// Wraps the specified words in <b> tags.
        /// </summary>
        public static string Bold(this string text, params string[] words)
        {
            foreach (var word in words)
            {
                text = text.Bold(word);
            }
            return text;
        }

        /// <summary>
        /// Wraps the entire string in <b> tags if the predicate is true.
        /// </summary>
        public static string Bold(this string text, Func<bool> predicate)
        {
            return predicate() ? $"<b>{text}</b>" : text;
        }

        /// <summary>
        /// Wraps the specified words in <b> tags if the predicate is true.
        /// </summary>
        public static string Bold(this string text, Func<string, bool> predicate)
        {
            return s_wordRegex.Replace(text, match => predicate(match.Value) ? $"<b>{match.Value}</b>" : match.Value);
        }

        /// <summary>
        /// Wraps the character in <b> tags.
        /// </summary>
        public static string Bold(this char character)
        {
            return $"<b>{character}</b>";
        }

        /// <summary>
        /// Wraps the specified character in <b> tags.
        /// </summary>
        public static string Bold(this string text, char character)
        {
            return text.Replace(character.ToString(), $"<b>{character}</b>");
        }

        /// <summary>
        /// Wraps the entire string in <i> tags.
        /// </summary>
        public static string Italic(this string text)
        {
            return $"<i>{text}</i>";
        }

        /// <summary>
        /// Wraps the specified word in <i> tags.
        /// </summary>
        public static string Italic(this string text, string word)
        {
            return s_wordRegex.Replace(text, match => match.Value == word ? $"<i>{word}</i>" : match.Value);
        }

        /// <summary>
        /// Wraps the specified words in <i> tags.
        /// </summary>
        public static string Italic(this string text, params string[] words)
        {
            foreach (var word in words)
            {
                text = text.Italic(word);
            }
            return text;
        }

        /// <summary>
        /// Wraps the entire string in <i> tags if the predicate is true.
        /// </summary>
        public static string Italic(this string text, Func<bool> predicate)
        {
            return predicate() ? $"<i>{text}</i>" : text;
        }

        /// <summary>
        /// Wraps the specified words in <i> tags if the predicate is true.
        /// </summary>
        public static string Italic(this string text, Func<string, bool> predicate)
        {
            return s_wordRegex.Replace(text, match => predicate(match.Value) ? $"<i>{match.Value}</i>" : match.Value);
        }

        /// <summary>
        /// Wraps the character in <i> tags.
        /// </summary>
        public static string Italic(this char character)
        {
            return $"<i>{character}</i>";
        }

        /// <summary>
        /// Wraps the specified character in <i> tags.
        /// </summary>
        public static string Italic(this string text, char character)
        {
            return text.Replace(character.ToString(), $"<i>{character}</i>");
        }

        /// <summary>
        /// Wraps the entire string in <u> tags.
        /// </summary>
        public static string Underline(this string text)
        {
            return $"<u>{text}</u>";
        }

        /// <summary>
        /// Wraps the specified word in <u> tags.
        /// </summary>
        public static string Underline(this string text, string word)
        {
            return s_wordRegex.Replace(text, match => match.Value == word ? $"<u>{word}</u>" : match.Value);
        }

        /// <summary>
        /// Wraps the specified words in <u> tags.
        /// </summary>
        public static string Underline(this string text, params string[] words)
        {
            foreach (var word in words)
            {
                text = text.Underline(word);
            }
            return text;
        }

        /// <summary>
        /// Wraps the entire string in <u> tags if the predicate is true.
        /// </summary>
        public static string Underline(this string text, Func<bool> predicate)
        {
            return predicate() ? $"<u>{text}</u>" : text;
        }

        /// <summary>
        /// Wraps the specified words in <u> tags if the predicate is true.
        /// </summary>
        public static string Underline(this string text, Func<string, bool> predicate)
        {
            return s_wordRegex.Replace(text, match => predicate(match.Value) ? $"<u>{match.Value}</u>" : match.Value);
        }

        /// <summary>
        /// Wraps the character in <u> tags.
        /// </summary>
        public static string Underline(this char character)
        {
            return $"<u>{character}</u>";
        }

        /// <summary>
        /// Wraps the specified character in <u> tags.
        /// </summary>
        public static string Underline(this string text, char character)
        {
            return text.Replace(character.ToString(), $"<u>{character}</u>");
        }
    }
}
