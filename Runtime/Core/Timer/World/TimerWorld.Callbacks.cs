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
            c.Target2 = null;
            c.TargetDelegate = cb;
            c.Invoker = cb != null ? Invokers<T>.Action0 : null;
            c.Invoker2 = null;
        }

        public static void SetOnStop<T1, T2>(TimerHandle handle, T1 target1, T2 target2, Action<T1, T2> cb)
            where T1 : class where T2 : class
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var c = ref _coldSlots[slot].OnStop;
            c.Target = target1;
            c.Target2 = target2;
            c.TargetDelegate = cb;
            c.Invoker = null;
            c.Invoker2 = cb != null ? Invokers<T1, T2>.Action0 : null;
        }
    }
}
