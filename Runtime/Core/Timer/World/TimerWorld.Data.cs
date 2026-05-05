using System;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        private const int DefaultCapacity = 32;

        internal enum TimerKind : byte
        {
            None = 0,
            Countdown = 1,
            Stopwatch = 2
        }

        private struct Callback0
        {
            public Action Action;

            public object Target;
            public Delegate TargetDelegate;
            public Action<object, Delegate> Invoker;

            public void Clear()
            {
                Action = null;
                Target = null;
                TargetDelegate = null;
                Invoker = null;
            }

            public readonly void Invoke()
            {
                Action?.Invoke();
                if (Target != null && TargetDelegate != null && Invoker != null)
                    Invoker(Target, TargetDelegate);
            }

            public readonly bool HasAny => Action != null || (Target != null && TargetDelegate != null && Invoker != null);
        }

        private struct Callback1
        {
            public Action<float> Action;

            public object Target;
            public Delegate TargetDelegate;
            public Action<object, Delegate, float> Invoker;

            public void Clear()
            {
                Action = null;
                Target = null;
                TargetDelegate = null;
                Invoker = null;
            }

            public readonly void Invoke(float v)
            {
                Action?.Invoke(v);
                if (Target != null && TargetDelegate != null && Invoker != null)
                    Invoker(Target, TargetDelegate, v);
            }

            public readonly bool HasAny => Action != null || (Target != null && TargetDelegate != null && Invoker != null);
        }

        private struct BoolPredicate
        {
            public Func<bool> Func;

            public object Target;
            public Delegate TargetDelegate;
            public Func<object, Delegate, bool> Invoker;

            public void Clear()
            {
                Func = null;
                Target = null;
                TargetDelegate = null;
                Invoker = null;
            }

            public readonly bool InvokeOrTrue()
            {
                if (Func != null) return Func.Invoke();
                if (Target != null && TargetDelegate != null && Invoker != null) return Invoker(Target, TargetDelegate);
                return true;
            }

            public readonly bool InvokeOrFalse()
            {
                if (Func != null) return Func.Invoke();
                if (Target != null && TargetDelegate != null && Invoker != null) return Invoker(Target, TargetDelegate);
                return false;
            }

            public readonly bool HasAny => Func != null || (Target != null && TargetDelegate != null && Invoker != null);
        }

        private static class Invokers<T> where T : class
        {
            public static readonly Action<object, Delegate> Action0 = InvokeAction0;
            public static readonly Action<object, Delegate, float> Action1 = InvokeAction1;
            public static readonly Func<object, Delegate, bool> FuncBool = InvokeFuncBool;

            private static void InvokeAction0(object target, Delegate del) => ((Action<T>)del).Invoke((T)target);
            private static void InvokeAction1(object target, Delegate del, float v) => ((Action<T, float>)del).Invoke((T)target, v);
            private static bool InvokeFuncBool(object target, Delegate del) => ((Func<T, bool>)del).Invoke((T)target);
        }

        // ─── Hot slot (~64 bytes, one cache line) ────────────────────────────────
        // Accessed every frame for every active timer.
        // Only Owner + OnUpdate hold GC references (4 total, down from 29).
        private struct TimerSlotHot
        {
            public int Version;
            public TimerKind Kind;
            public bool IsRunning;
            public bool IsPendingKill;
            public bool UseUnscaledTime;
            public bool HasUpdateWhen;  // cached: cold UpdateWhen.HasAny
            public bool HasStopWhen;    // cached: cold StopwatchStopWhen.HasAny

            public UnityEngine.Object Owner;

            public float CountdownRemaining;
            public float StopwatchElapsed;

            public Callback1 OnUpdate;

            public void ClearHot()
            {
                Kind = TimerKind.None;
                IsRunning = false;
                IsPendingKill = false;
                UseUnscaledTime = false;
                HasUpdateWhen = false;
                HasStopWhen = false;
                Owner = null;
                CountdownRemaining = 0f;
                StopwatchElapsed = 0f;
                OnUpdate.Clear();
            }
        }

        // ─── Cold slot ───────────────────────────────────────────────────────────
        // Accessed only when a timer fires its OnComplete event.
        private struct TimerSlotCold
        {
            public BoolPredicate UpdateWhen;

            public float CountdownTotal;
            public int LoopCount;
            public int LoopIteration;

            public BoolPredicate StopwatchStopWhen;

            public Callback0 OnComplete;

            public void ClearCold()
            {
                UpdateWhen.Clear();
                CountdownTotal = 0f;
                LoopCount = 0;
                LoopIteration = 0;
                StopwatchStopWhen.Clear();
                OnComplete.Clear();
            }
        }

        private static TimerSlotHot[] _hotSlots;
        private static TimerSlotCold[] _coldSlots;

        private static int[] _free;
        private static int _freeCount;

        private static int[] _active;
        private static int _activeCount;
    }
}
