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
            if (h.IsPendingKill) return;

            if (seconds < 0f) return;

            if (h.Owner == null)
            {
                KillSlot(slot);
                return;
            }

            float next = h.CountdownRemaining - seconds;

            if (next > 0f)
            {
                h.CountdownRemaining = next;
                h.OnUpdate.Invoke(h.CountdownRemaining);
                return;
            }

            // Keep as negative — overflow is carried forward through loop iterations.
            h.CountdownRemaining = next;
            h.OnUpdate.Invoke(0f);

            do
            {
                HandleCountdownExpired(slot, ref h);
            }
            while (h.IsRunning && h.CountdownRemaining <= 0f);

            if (h.IsRunning)
                h.OnUpdate.Invoke(h.CountdownRemaining);
        }
    }
}
