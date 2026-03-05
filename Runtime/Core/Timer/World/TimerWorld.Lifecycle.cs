using System;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        public static TimerHandle CreateCountdown(MonoBehaviour owner, float durationSeconds)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            var handle = AllocSlot();
            ref var hot = ref _hotSlots[handle.Slot];
            ref var cold = ref _coldSlots[handle.Slot];

            hot.Kind = TimerKind.Countdown;
            hot.Owner = owner;
            hot.CountdownRemaining = Mathf.Max(0f, durationSeconds);
            cold.CountdownTotal = hot.CountdownRemaining;

            cold.LoopCount = 0;
            cold.LoopIteration = 0;
            cold.LoopStopWhen.Clear();

            RegisterActive(handle.Slot);
            return handle;
        }

        public static TimerHandle CreateStopwatch(MonoBehaviour owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            var handle = AllocSlot();
            ref var hot = ref _hotSlots[handle.Slot];

            hot.Kind = TimerKind.Stopwatch;
            hot.Owner = owner;
            hot.StopwatchElapsed = 0f;
            _coldSlots[handle.Slot].StopwatchStopWhen.Clear();

            RegisterActive(handle.Slot);
            return handle;
        }

        public static void Start(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return;

            ref var h = ref _hotSlots[slot];
            if (h.IsPendingKill) return;

            if (h.Owner == null)
            {
                KillSlot(slot);
                return;
            }

            if (h.IsRunning) return;

            if (h.Kind == TimerKind.Countdown && _coldSlots[slot].CountdownTotal <= 0f)
            {
                Log.Warn("[NekoLib.Timer] Countdown duration is 0; it will complete immediately.");
                h.IsRunning = true;
                _coldSlots[slot].OnStart.Invoke();
                h.OnUpdate.Invoke(0f);
                Stop(handle);
                return;
            }

            h.IsRunning = true;
            _coldSlots[slot].OnStart.Invoke();
        }

        public static void Pause(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            _hotSlots[slot].IsRunning = false;
        }

        public static void Resume(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return;

            ref var h = ref _hotSlots[slot];
            if (h.IsPendingKill) return;

            if (h.Owner == null)
            {
                KillSlot(slot);
                return;
            }

            h.IsRunning = true;
        }

        public static void Stop(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return;

            ref var h = ref _hotSlots[slot];
            if (h.IsPendingKill) return;

            if (h.Owner == null)
            {
                KillSlot(slot);
                return;
            }

            h.IsRunning = false;
            _coldSlots[slot].OnStop.Invoke();
            KillSlot(slot);
        }

        public static void Cancel(TimerHandle handle)
        {
            if (!TryGetSlot(handle, out int slot)) return;
            KillSlot(slot);
        }
    }
}
