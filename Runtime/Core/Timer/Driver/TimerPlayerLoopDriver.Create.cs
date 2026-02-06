using System;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Core
{
    internal static partial class TimerPlayerLoopDriver
    {
        internal static Countdown CreateCountdown(MonoBehaviour owner, float duration)
        {
            duration = duration.AtLeast(0f);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (duration == 0f)
            {
                Log.Warn($"[{nameof(Countdown)}] Duration is 0. Countdown will stop immediately on start.");
            }
#endif

            var handler = RentCountdown();
            handler.ReInitialize(owner, duration);

            var slot = AllocateSlot(handler);
            handler.AssignHandle(slot, Slots[slot].Id);

            handler.ActiveIndex = ActiveTimers.Count;
            ActiveTimers.Add(handler);

            return new Countdown(slot, Slots[slot].Id);
        }

        internal static Stopwatch CreateStopwatch(MonoBehaviour owner, Func<bool> stopCondition)
        {
            var handler = RentStopwatch();
            handler.ReInitialize(owner, stopCondition);

            var slot = AllocateSlot(handler);
            handler.AssignHandle(slot, Slots[slot].Id);

            handler.ActiveIndex = ActiveTimers.Count;
            ActiveTimers.Add(handler);

            return new Stopwatch(slot, Slots[slot].Id);
        }
    }
}
