using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using NekoLib.Utilities;

namespace NekoLib.Extensions
{
    public static class CoroutineExtensions
    {
        /// <summary>
        /// Starts multiple coroutines in sequence, one after another.
        /// </summary>
        public static Coroutine StartCoroutineSequence(this MonoBehaviour mono, params IEnumerator[] coroutines)
        {
            if (mono == null) throw new ArgumentNullException(nameof(mono));
            if (coroutines.IsNullOrEmpty()) throw new ArgumentException("No coroutines provided.", nameof(coroutines));
            return mono.StartCoroutine(ExecuteSequence(coroutines));
        }

        /// <summary>
        /// Starts a coroutine after a specified delay.
        /// </summary>
        public static Coroutine StartCoroutineDelayed(this MonoBehaviour mono, IEnumerator coroutine, float delay)
        {
            if (mono == null) throw new ArgumentNullException(nameof(mono));
            if (coroutine == null) throw new ArgumentNullException(nameof(coroutine));
            return mono.StartCoroutine(DelayedExecution(coroutine, delay));
        }

        /// <summary>
        /// Starts a coroutine when a condition becomes true.
        /// </summary>
        public static Coroutine StartCoroutineWhen(this MonoBehaviour mono, IEnumerator coroutine, Func<bool> condition)
        {
            if (mono == null) throw new ArgumentNullException(nameof(mono));
            if (coroutine == null) throw new ArgumentNullException(nameof(coroutine));
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            return mono.StartCoroutine(ConditionalExecution(coroutine, condition));
        }

        /// <summary>
        /// Starts multiple coroutines in parallel.
        /// </summary>
        public static Coroutine StartCoroutineParallel(this MonoBehaviour mono, params IEnumerator[] coroutines)
        {
            if (mono == null) throw new ArgumentNullException(nameof(mono));
            if (coroutines.IsNullOrEmpty()) throw new ArgumentException("No coroutines provided.", nameof(coroutines));
            return mono.StartCoroutine(ExecuteParallel(mono, coroutines));
        }

        /// <summary>
        /// Executes a sequence of coroutines one after another.
        /// </summary>
        private static IEnumerator ExecuteSequence(IEnumerator[] coroutines)
        {
            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }
        }

        /// <summary>
        /// Executes a sequence of coroutines in parallel.
        /// </summary>
        private static IEnumerator ExecuteParallel(MonoBehaviour mono, IEnumerator[] coroutines)
        {
            var runningCoroutines = new Coroutine[coroutines.Length];

            for (int i = 0; i < coroutines.Length; i++)
            {
                runningCoroutines[i] = mono.StartCoroutine(coroutines[i]);
            }

            foreach (var coroutine in runningCoroutines)
            {
                yield return coroutine;
            }
        }

        /// <summary>
        /// Executes a coroutine after a specified delay.
        /// </summary>
        private static IEnumerator DelayedExecution(IEnumerator coroutine, float delay)
        {
            yield return Utils.GetWaitForSeconds(delay);
            yield return coroutine;
        }

        /// <summary>
        /// Executes a coroutine when a specified condition is met.
        /// </summary>
        private static IEnumerator ConditionalExecution(IEnumerator coroutine, Func<bool> condition)
        {
            yield return new WaitUntil(condition);
            yield return coroutine;
        }

        /// <summary>
        /// Converts a Coroutine to a Task for await support.
        /// </summary>
        public static Task AsTask(this Coroutine coroutine, MonoBehaviour mono)
        {
            if (coroutine == null) return Task.CompletedTask;
            if (mono == null) throw new ArgumentNullException(nameof(mono));

            var tcs = new TaskCompletionSource<bool>();
            mono.StartCoroutine(WaitForCoroutine(coroutine, tcs, mono.destroyCancellationToken));
            return tcs.Task;
        }

        /// <summary>
        /// Converts an IEnumerator to a Task for await support.
        /// </summary>
        public static Task AsTask(this IEnumerator coroutine, MonoBehaviour mono)
        {
            if (coroutine == null) return Task.CompletedTask;
            if (mono == null) throw new ArgumentNullException(nameof(mono));

            var tcs = new TaskCompletionSource<bool>();
            mono.StartCoroutine(WaitForRoutine(coroutine, tcs, mono.destroyCancellationToken));
            return tcs.Task;
        }

        /// <summary>
        /// Waits for a Coroutine to complete and sets the TaskCompletionSource result.
        /// </summary>
        private static IEnumerator WaitForCoroutine(Coroutine coroutine, TaskCompletionSource<bool> tcs, CancellationToken cancellationToken)
        {
            while (!coroutine.Equals(null) && !cancellationToken.IsCancellationRequested)
            {
                yield return null;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                tcs.SetCanceled();
            }
            else
            {
                tcs.SetResult(true);
            }
        }

        /// <summary>
        /// Runs multiple coroutines concurrently.
        /// </summary>
        public static Task WhenAll(this MonoBehaviour mono, params IEnumerator[] routines)
        {
            if (routines.IsNullOrEmpty())
                return Task.CompletedTask;

            var tasks = new Task[routines.Length];
            for (int i = 0; i < routines.Length; i++)
            {
                tasks[i] = routines[i].AsTask(mono);
            }
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Runs multiple coroutines and returns when any completes.
        /// </summary>
        public static Task<Task> WhenAny(this MonoBehaviour mono, params IEnumerator[] routines)
        {
            if (routines.IsNullOrEmpty())
                return Task.FromResult(Task.CompletedTask);

            var tasks = new Task[routines.Length];
            for (int i = 0; i < routines.Length; i++)
            {
                tasks[i] = routines[i].AsTask(mono);
            }
            return Task.WhenAny(tasks);
        }

        /// <summary>
        /// Converts an IEnumerator coroutine to a Task for await support.
        /// </summary>
        private static IEnumerator WaitForRoutine(IEnumerator routine, TaskCompletionSource<bool> tcs, CancellationToken cancellationToken)
        {
            bool hasException = false;
            Exception exception = null;

            while (!cancellationToken.IsCancellationRequested && !hasException)
            {
                bool moveNext;
                try
                {
                    moveNext = routine.MoveNext();
                }
                catch (Exception ex)
                {
                    exception = ex;
                    hasException = true;
                    break;
                }

                if (!moveNext)
                    break;

                yield return routine.Current;
            }

            if (hasException)
            {
                tcs.SetException(exception);
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                tcs.SetCanceled();
            }
            else
            {
                tcs.SetResult(true);
            }
        }
    }
}
