using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using NekoLib.Utilities;

namespace NekoLib.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Extension method to safely ignore a Task and handle exceptions.
        /// </summary>
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

        /// <summary>
        /// Handles exceptions by invoking the provided callback or logging them.
        /// </summary>
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

        // <summary>
        /// Converts a Task to a Coroutine using YieldTask
        /// </summary>
        public static IEnumerator AsCoroutine(this Task task)
        {
            yield return new YieldTask(task);

            if (task.IsFaulted)
            {
                throw task.Exception?.GetBaseException() ?? new Exception("Task faulted with no exception.");
            }
            else if (task.IsCanceled)
            {
                throw new OperationCanceledException("Task was canceled.");
            }
        }
    }
}
