using System;
using System.Collections;
using UnityEngine;
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
            return mono.StartCoroutine(ExecuteSequence(coroutines));
        }

        /// <summary>
        /// Starts a coroutine after a specified delay.
        /// </summary>
        public static Coroutine StartCoroutineDelayed(this MonoBehaviour mono, IEnumerator coroutine, float delay)
        {
            return mono.StartCoroutine(DelayedExecution(coroutine, delay));
        }

        /// <summary>
        /// Starts a coroutine when a condition becomes true.
        /// </summary>
        public static Coroutine StartCoroutineWhen(this MonoBehaviour mono, IEnumerator coroutine, Func<bool> condition)
        {
            return mono.StartCoroutine(ConditionalExecution(coroutine, condition));
        }

        /// <summary>
        /// Starts multiple coroutines in parallel.
        /// </summary>
        public static Coroutine StartCoroutineParallel(this MonoBehaviour mono, params IEnumerator[] coroutines)
        {
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
    }
}
