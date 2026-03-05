namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        public static void AddCountdownTime(TimerHandle handle, float seconds)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var h = ref _hotSlots[slot];
            if (h.Kind != TimerKind.Countdown) return;

            if (seconds < 0f) return;

            if (h.Owner == null)
            {
                KillSlot(slot);
                return;
            }

            h.CountdownRemaining += seconds;
            h.OnUpdate.Invoke(h.CountdownRemaining);
        }

        public static void ReduceCountdownTime(TimerHandle handle, float seconds)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var h = ref _hotSlots[slot];
            if (h.Kind != TimerKind.Countdown) return;

            if (seconds < 0f) return;

            if (h.Owner == null)
            {
                KillSlot(slot);
                return;
            }

            float next = h.CountdownRemaining - seconds;
            h.CountdownRemaining = next > 0f ? next : 0f;

            h.OnUpdate.Invoke(h.CountdownRemaining);

            if (h.CountdownRemaining <= 0f)
            {
                h.IsRunning = false;
                _coldSlots[slot].OnStop.Invoke();
                KillSlot(slot);
            }
        }
    }
}
