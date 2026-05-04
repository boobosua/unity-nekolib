using System;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        // OnUpdate lives in hot — invoked every tick frame.
        public static void SetOnUpdate(TimerHandle handle, Action<float> cb)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            _hotSlots[slot].OnUpdate.Action = cb;
        }

        public static void SetOnUpdate<T>(TimerHandle handle, T target, Action<T, float> cb) where T : class
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var c = ref _hotSlots[slot].OnUpdate;
            c.Target = target;
            c.TargetDelegate = cb;
            c.Invoker = cb != null ? Invokers<T>.Action1 : null;
        }

        // OnElapsed lives in cold — fires when the timer's natural event occurs:
        // for Countdowns, every iteration boundary (including the final one);
        // for Stopwatches, when the stop predicate becomes true.
        public static void SetOnElapsed(TimerHandle handle, Action cb)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            _coldSlots[slot].OnElapsed.Action = cb;
        }
    }
}
