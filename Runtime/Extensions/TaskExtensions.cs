using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task, Action<Exception> onException = null)
        {
            // Handle synchronously completed tasks immediately
            if (task.IsCompleted)
            {
                if (task.Exception != null)
                    HandleException(task.Exception.GetBaseException(), onException);
                return;
            }

            // Handle future completion - NO ExecuteSynchronously in Unity
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                    HandleException(t.Exception.GetBaseException(), onException);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static void HandleException(Exception ex, Action<Exception> onException)
        {
            // Skip logging OperationCanceledException
            if (ex is OperationCanceledException)
                return;

            if (onException != null)
            {
                onException(ex);
            }
            else
            {
                Debug.LogError($"Unhandled task exception: {ex}");
            }
        }
    }
}
