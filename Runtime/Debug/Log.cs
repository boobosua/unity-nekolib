using System.Diagnostics;

namespace NekoLib.Logger
{
    /// <summary>
    /// Simple compile-time conditional logger.
    /// Appears only in Editor, Development builds, or when NEKOLIB_LOG is defined.
    /// </summary>
    public static class Log
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Info(object message) => UnityEngine.Debug.Log(message);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Warn(object message) => UnityEngine.Debug.LogWarning(message);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Error(object message) => UnityEngine.Debug.LogError(message);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("NEKOLIB_LOG")]
        public static void Exception(System.Exception exception) => UnityEngine.Debug.LogException(exception);
    }
}
