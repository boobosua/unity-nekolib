namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        public static void AddCountdownTime(TimerHandle handle, float seconds)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var h = ref _hotSlots[slot];
            if (h.Kind != TimerKind.Countdown) return;
            if (h.IsPendingKill) return;

            if (float.IsNaN(seconds) || seconds < 0f) return;

            if (h.Owner == null)
            {
                KillSlot(slot);
                return;
            }

            h.CountdownRemaining += seconds;

            // Fire OnUpdate only when paused; while running, the next Tick reports the value.
            if (!h.IsRunning) h.OnUpdate.Invoke(h.CountdownRemaining);
        }

        public static void ReduceCountdownTime(TimerHandle handle, float seconds)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            ref var h = ref _hotSlots[slot];
            if (h.Kind != TimerKind.Countdown) return;
            if (h.IsPendingKill) return;

            if (float.IsNaN(seconds) || seconds < 0f) return;

            if (h.Owner == null)
            {
                KillSlot(slot);
                return;
            }

            float next = h.CountdownRemaining - seconds;

            if (next > 0f)
            {
                h.CountdownRemaining = next;
                if (!h.IsRunning) h.OnUpdate.Invoke(next);
                return;
            }

            // Clamp to zero — no overshoot carry into subsequent loop iterations.
            h.CountdownRemaining = 0f;
            if (!h.IsRunning) h.OnUpdate.Invoke(0f);
            if (h.IsPendingKill) return;

            HandleCountdownExpired(slot, ref h);
        }
    }
}
