using System;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        internal static void TickPlayerLoop()
        {
            if (_hotSlots == null) return;

            float dt = Time.deltaTime;
            float udt = Time.unscaledDeltaTime;

            for (int i = 0; i < _activeCount; i++)
            {
                int slot = _active[i];
                ref var h = ref _hotSlots[slot];

                if (h.IsPendingKill) continue;

                if (h.Owner == null)
                {
                    KillSlot(slot);
                    continue;
                }

                if (!h.IsRunning) continue;

                if (h.HasUpdateWhen)
                {
                    bool shouldTick;
                    try { shouldTick = _coldSlots[slot].UpdateWhen.InvokeOrTrue(); }
                    catch (Exception ex)
                    {
                        Log.Error($"[NekoLib.Timer] updateWhen threw; disabling predicate. {ex}");
                        _coldSlots[slot].UpdateWhen.Clear();
                        h.HasUpdateWhen = false;
                        continue;
                    }

                    if (!shouldTick) continue;
                }

                float usedDt = h.UseUnscaledTime ? udt : dt;
                if (usedDt <= 0f) continue;

                switch (h.Kind)
                {
                    case TimerKind.Countdown:
                        TickCountdown(slot, ref h, usedDt);
                        break;
                    case TimerKind.Stopwatch:
                        TickStopwatch(slot, ref h, usedDt);
                        break;
                }
            }

            CleanupPending();
        }

        private static void TickCountdown(int slot, ref TimerSlotHot h, float dt)
        {
            h.CountdownRemaining -= dt;

            if (h.CountdownRemaining > 0f)
            {
                h.OnUpdate.Invoke(h.CountdownRemaining);
                return;
            }

            h.OnUpdate.Invoke(0f);
            if (h.IsPendingKill) return;

            HandleCountdownExpired(slot, ref h);
        }

        // Shared expiry handler — called by TickCountdown, ReduceCountdownTime, and the
        // immediate-completion path in Start. Fires OnComplete on every iteration boundary
        // including the final one. Resets remaining to CountdownTotal when looping;
        // kills the slot when not.
        private static void HandleCountdownExpired(int slot, ref TimerSlotHot h)
        {
            ref var c = ref _coldSlots[slot];

            c.LoopIteration++;

            bool shouldLoop = c.LoopCount switch
            {
                -1 => true,
                0 => false,
                _ => c.LoopIteration < c.LoopCount
            };

            c.OnComplete.Invoke();
            if (h.IsPendingKill) return;

            if (shouldLoop)
            {
                h.CountdownRemaining = c.CountdownTotal;
                return;
            }

            KillSlot(slot);
        }

        private static void TickStopwatch(int slot, ref TimerSlotHot h, float dt)
        {
            h.StopwatchElapsed += dt;
            h.OnUpdate.Invoke(h.StopwatchElapsed);

            if (h.IsPendingKill) return;
            if (!h.HasStopWhen) return;

            bool shouldStop;
            try { shouldStop = _coldSlots[slot].StopwatchStopWhen.InvokeOrFalse(); }
            catch (Exception ex)
            {
                Log.Error($"[NekoLib.Timer] stopWhen threw; disabling predicate. {ex}");
                _coldSlots[slot].StopwatchStopWhen.Clear();
                h.HasStopWhen = false;
                return;
            }

            if (!shouldStop) return;

            _coldSlots[slot].OnComplete.Invoke();
            if (h.IsPendingKill) return;
            KillSlot(slot);
        }
    }
}
