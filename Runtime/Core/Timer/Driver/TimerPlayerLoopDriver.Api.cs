using System;
using NekoLib.Extensions;
using NekoLib.Logger;

namespace NekoLib.Core
{
    internal static partial class TimerPlayerLoopDriver
    {
        internal static bool IsAlive(int slot, int id)
        {
            if (slot < 0 || slot >= Slots.Count) return false;
            var rec = Slots[slot];
            return rec.Timer != null && rec.Id == id;
        }

        internal static bool IsRunning(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return false;
            return timer.IsRunning;
        }

        internal static bool IsPaused(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return false;
            return !timer.IsRunning;
        }

        internal static void Start(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;

            timer.Start();

            // duration==0 countdown stops itself during Start(); return to pool immediately.
            if (!timer.IsRunning)
            {
                Unregister(timer);
            }
        }

        internal static void Pause(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.Pause();
        }

        internal static void Resume(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.Resume();
        }

        internal static void Stop(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;

            timer.StopInvoke();
            Unregister(timer);
        }

        internal static void Cancel(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;

            timer.StopSilent();
            Unregister(timer);
        }

        internal static void SetUnscaledTime(int slot, int id, bool unscaled)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.SetUnscaledTime(unscaled);
        }

        internal static void SetUpdateWhen(int slot, int id, Func<bool> updateWhen)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.SetUpdateWhen(updateWhen);
        }

        internal static void AddOnStart(int slot, int id, Action cb)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.AddOnStart(cb);
        }

        internal static void AddOnUpdate(int slot, int id, Action<float> cb)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.AddOnUpdate(cb);
        }

        internal static void AddOnStop(int slot, int id, Action cb)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.AddOnStop(cb);
        }

        internal static void AddOnLoop(int slot, int id, Action cb)
        {
            if (!TryGet(slot, id, out var timer)) return;
            if (timer is CountdownHandler cd) cd.AddOnLoop(cb);
        }

        internal static void SetCountdownLoopCount(int slot, int id, int loopCount)
        {
            if (!TryGet(slot, id, out var timer)) return;
            if (timer is CountdownHandler cd) cd.SetLoop(loopCount);
        }

        internal static void SetCountdownLoopCondition(int slot, int id, Func<bool> stopCondition)
        {
            if (!TryGet(slot, id, out var timer)) return;
            if (timer is CountdownHandler cd) cd.SetLoop(stopCondition);
        }

        internal static void SetCountdownTotalTime(int slot, int id, float totalTime)
        {
            totalTime = totalTime.AtLeast(0f);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (totalTime == 0f)
            {
                Log.Warn($"[{nameof(Countdown)}] Duration is 0. Countdown will stop immediately on start.");
            }
#endif

            if (!TryGet(slot, id, out var timer)) return;
            if (timer is CountdownHandler cd) cd.SetTotalTime(totalTime);
        }

        internal static void SetStopwatchStopCondition(int slot, int id, Func<bool> stopCondition)
        {
            if (!TryGet(slot, id, out var timer)) return;
            if (timer is StopwatchHandler sw) sw.SetStopCondition(stopCondition);
        }

        internal static void AddCountdownTime(int slot, int id, float seconds)
        {
            if (!TryGet(slot, id, out var timer)) return;
            if (timer is not CountdownHandler c) return;

            c.AddTime(seconds);
        }

        internal static void ReduceCountdownTime(int slot, int id, float seconds)
        {
            if (!TryGet(slot, id, out var timer)) return;
            if (timer is not CountdownHandler c) return;

            c.ReduceTime(seconds);

            if (c.RemainingTime <= 0f)
            {
                Unregister(timer);
            }
        }

        internal static float GetCountdownRemainingTime(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return 0f;
            return timer is CountdownHandler c ? c.RemainingTime : 0f;
        }

        internal static float GetCountdownTotalTime(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return 0f;
            return timer is CountdownHandler c ? c.TotalTime : 0f;
        }

        internal static int GetCountdownLoopIteration(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return 0;
            return timer is CountdownHandler c ? c.CurrentLoopIteration : 0;
        }

        internal static float GetStopwatchElapsedTime(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return 0f;
            return timer is StopwatchHandler s ? s.ElapsedTime : 0f;
        }
    }
}
