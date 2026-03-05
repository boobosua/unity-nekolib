using System;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        // OnStart, OnLoop, OnStop live in cold — only touched when timer fires.
        public static void SetOnStart(TimerHandle handle, Action cb)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            _coldSlots[slot].OnStart.Action = cb;
        }

        public static void SetOnStart<T>(TimerHandle handle, T target, Action<T> cb) where T : class
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var c = ref _coldSlots[slot].OnStart;
            c.Target = target;
            c.TargetDelegate = cb;
            c.Invoker = cb != null ? Invokers<T>.Action0 : null;
        }

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

        public static void SetOnLoop(TimerHandle handle, Action cb)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            _coldSlots[slot].OnLoop.Action = cb;
        }

        public static void SetOnLoop<T>(TimerHandle handle, T target, Action<T> cb) where T : class
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var c = ref _coldSlots[slot].OnLoop;
            c.Target = target;
            c.TargetDelegate = cb;
            c.Invoker = cb != null ? Invokers<T>.Action0 : null;
        }

        public static void SetOnStop(TimerHandle handle, Action cb)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            _coldSlots[slot].OnStop.Action = cb;
        }

        public static void SetOnStop<T>(TimerHandle handle, T target, Action<T> cb) where T : class
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var c = ref _coldSlots[slot].OnStop;
            c.Target = target;
            c.TargetDelegate = cb;
            c.Invoker = cb != null ? Invokers<T>.Action0 : null;
        }
    }
}
