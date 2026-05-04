using System;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        public static void SetUpdateWhen(TimerHandle handle, Func<bool> predicate)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var p = ref _coldSlots[slot].UpdateWhen;
            p.Func = predicate;
            p.Target = null;
            p.TargetDelegate = null;
            p.Invoker = null;
            _hotSlots[slot].HasUpdateWhen = p.HasAny;
        }

        public static void SetUpdateWhen<T>(TimerHandle handle, T target, Func<T, bool> predicate) where T : class
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var p = ref _coldSlots[slot].UpdateWhen;
            p.Func = null;
            p.Target = target;
            p.TargetDelegate = predicate;
            p.Invoker = predicate != null ? Invokers<T>.FuncBool : null;
            _hotSlots[slot].HasUpdateWhen = p.HasAny;
        }

        public static void SetCountdownLoopCount(TimerHandle handle, int loopCount)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            if (_hotSlots[slot].Kind != TimerKind.Countdown) return;

            if (loopCount < -1) throw new ArgumentException("Loop count cannot be less than -1", nameof(loopCount));

            ref var c = ref _coldSlots[slot];
            c.LoopCount = loopCount;
            c.LoopIteration = 0;
        }

        public static void SetStopwatchStopCondition(TimerHandle handle, Func<bool> stopWhen)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            if (_hotSlots[slot].Kind != TimerKind.Stopwatch) return;

            ref var p = ref _coldSlots[slot].StopwatchStopWhen;
            p.Func = stopWhen;
            p.Target = null;
            p.TargetDelegate = null;
            p.Invoker = null;
            _hotSlots[slot].HasStopWhen = p.HasAny;
        }

        public static void SetStopwatchStopCondition<T>(TimerHandle handle, T target, Func<T, bool> stopWhen) where T : class
        {
            if (!TryGetSlot(handle, out int slot)) return;
            if (_hotSlots[slot].Kind != TimerKind.Stopwatch) return;

            ref var p = ref _coldSlots[slot].StopwatchStopWhen;
            p.Func = null;
            p.Target = target;
            p.TargetDelegate = stopWhen;
            p.Invoker = stopWhen != null ? Invokers<T>.FuncBool : null;
            _hotSlots[slot].HasStopWhen = p.HasAny;
        }

        public static void SetUnscaledTime(TimerHandle handle, bool useUnscaled)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            _hotSlots[slot].UseUnscaledTime = useUnscaled;
        }
    }
}
