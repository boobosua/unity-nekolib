namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        public static bool IsAlive(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return false;
            ref var h = ref _hotSlots[slot];
            return h.Owner != null && !h.IsPendingKill;
        }

        public static bool IsRunning(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return false;
            ref var h = ref _hotSlots[slot];
            return h.Owner != null && h.IsRunning && !h.IsPendingKill;
        }

        public static bool IsPaused(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return false;
            ref var h = ref _hotSlots[slot];
            return h.Owner != null && !h.IsRunning && !h.IsPendingKill;
        }

        public static float GetCountdownRemaining(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return 0f;
            ref var h = ref _hotSlots[slot];
            return h.Kind == TimerKind.Countdown ? h.CountdownRemaining : 0f;
        }

        public static float GetCountdownTotal(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return 0f;
            return _hotSlots[slot].Kind == TimerKind.Countdown ? _coldSlots[slot].CountdownTotal : 0f;
        }

        public static int GetCountdownLoopIteration(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return 0;
            return _hotSlots[slot].Kind == TimerKind.Countdown ? _coldSlots[slot].LoopIteration : 0;
        }

        public static float GetStopwatchElapsed(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return 0f;
            ref var h = ref _hotSlots[slot];
            return h.Kind == TimerKind.Stopwatch ? h.StopwatchElapsed : 0f;
        }
    }
}
