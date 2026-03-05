using System;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        public static void TickPlayerLoop()
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

            h.CountdownRemaining = 0f;
            h.OnUpdate.Invoke(0f);

            ref var c = ref _coldSlots[slot];

            bool shouldLoop;
            switch (c.LoopCount)
            {
                case -1:
                    shouldLoop = true;
                    break;
                case 0:
                    shouldLoop = false;
                    break;
                default:
                    c.LoopIteration++;
                    shouldLoop = c.LoopIteration < c.LoopCount;
                    break;
            }

            if (!shouldLoop && c.LoopStopWhen.HasAny)
            {
                bool stopNow;
                try { stopNow = c.LoopStopWhen.InvokeOrFalse(); }
                catch (Exception ex)
                {
                    Log.Error($"[NekoLib.Timer] loop stopWhen threw; stopping loop. {ex}");
                    c.LoopStopWhen.Clear();
                    stopNow = true;
                }

                shouldLoop = !stopNow;
            }

            if (shouldLoop)
            {
                c.OnLoop.Invoke();
                h.CountdownRemaining = c.CountdownTotal;
                return;
            }

            h.IsRunning = false;
            c.OnStop.Invoke();
            KillSlot(slot);
        }

        private static void TickStopwatch(int slot, ref TimerSlotHot h, float dt)
        {
            h.StopwatchElapsed += dt;
            h.OnUpdate.Invoke(h.StopwatchElapsed);

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

            h.IsRunning = false;
            _coldSlots[slot].OnStop.Invoke();
            KillSlot(slot);
        }
    }
}
