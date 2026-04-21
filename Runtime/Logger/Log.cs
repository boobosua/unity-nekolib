using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace NekoLib.Logger
{
    /// <summary>
    /// Conditional logger. Info, Warn, and Assert are stripped in release builds (unless NEKOLIB_LOG is defined).
    /// Error and Exception always fire in all builds so crash reporters (e.g. Firebase, Sentry) capture them.
    /// </summary>
    public static class Log
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Info(object message) => Debug.Log(message);

        /// <summary>Logs an info message with a context object to ping/select in the Unity Console.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Info(object message, Object context) => Debug.Log(message, context);

        /// <summary>Logs a warning message.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Warn(object message) => Debug.LogWarning(message);

        /// <summary>Logs a warning message with a context object to ping/select in the Unity Console.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Warn(object message, Object context) => Debug.LogWarning(message, context);

        /// <summary>Logs an error message. Always fires in all builds so crash reporters can capture it.</summary>
        public static void Error(object message) => Debug.LogError(message);

        /// <summary>Logs an error message with a context object to ping/select in the Unity Console. Always fires in all builds.</summary>
        public static void Error(object message, Object context) => Debug.LogError(message, context);

        /// <summary>Logs an exception. Always fires in all builds so crash reporters can capture it.</summary>
        public static void Exception(Exception exception) => Debug.LogException(exception);

        /// <summary>Logs an exception with a context object to ping/select in the Unity Console. Always fires in all builds.</summary>
        public static void Exception(Exception exception, Object context) => Debug.LogException(exception, context);

        /// <summary>Asserts a condition and logs a message if the assertion fails.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Assert(bool condition, string message = "Assertion failed!") => Debug.Assert(condition, message);

        /// <summary>Asserts a condition with a context object to ping/select in the Unity Console if the assertion fails.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Assert(bool condition, string message, Object context) => Debug.Assert(condition, message, context);
    }
}
