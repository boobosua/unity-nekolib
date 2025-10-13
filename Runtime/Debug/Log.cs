using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace NekoLib.Logger
{
    /// <summary>
    /// Simple compile-time conditional logger.
    /// Appears only in Editor, Development builds, or when NEKOLIB_LOG is defined.
    /// </summary>
    public static class Log
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Info(object message) => Debug.Log(message);

        /// <summary>
        /// Logs an info message with a context object to ping/select in the Unity Console.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Info(object message, Object context) => Debug.Log(message, context);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Warn(object message) => Debug.LogWarning(message);

        /// <summary>
        /// Logs a warning message with a context object to ping/select in the Unity Console.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Warn(object message, Object context) => Debug.LogWarning(message, context);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Error(object message) => Debug.LogError(message);

        /// <summary>
        /// Logs an error message with a context object to ping/select in the Unity Console.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Error(object message, Object context) => Debug.LogError(message, context);

        /// <summary>
        /// Logs an exception.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Exception(Exception exception) => Debug.LogException(exception);

        /// <summary>
        /// Logs an exception with a context object to ping/select in the Unity Console.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Exception(Exception exception, Object context) => Debug.LogException(exception, context);
    }
}
