using System;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Core
{
    internal static partial class TimerPlayerLoopDriver
    {
        /// <summary>Called from the PlayerLoop; ticks active timers and processes removals.</summary>
        internal static void UpdateTimers()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            _isUpdating = true;

            for (int i = 0; i < ActiveTimers.Count; i++)
            {
                var timer = ActiveTimers[i];
                if (timer == null)
                {
                    RemoveAtIndex(i);
                    i--;
                    continue;
                }

                if (!timer.IsOwnerValid)
                {
                    MarkForRemoval(timer);
                    continue;
                }

                if (!timer.ShouldTick) continue;

                var dt = timer.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                bool completed;
                try
                {
                    completed = timer.Tick(dt);
                }
                catch (Exception ex)
                {
                    Log.Error($"[TimerPlayerLoopDriver] Timer tick threw: {ex}");
                    completed = true;
                }

                if (completed)
                {
                    MarkForRemoval(timer);
                }
            }

            _isUpdating = false;

            if (ToRemove.Count > 0)
            {
                for (int i = 0; i < ToRemove.Count; i++)
                {
                    var t = ToRemove[i];
                    if (t == null) continue;

                    t.IsPendingRemoval = false;

                    RemoveActiveTimer(t);
                    ReleaseSlot(t);
                    ReturnToPool(t);
                }

                ToRemove.Clear();
            }
        }

        private static void MarkForRemoval(TimerHandlerBase timer)
        {
            if (timer.IsPendingRemoval) return;
            timer.IsPendingRemoval = true;
            ToRemove.Add(timer);
        }

        private static void Unregister(TimerHandlerBase timer)
        {
            if (_isUpdating)
            {
                MarkForRemoval(timer);
                return;
            }

            RemoveActiveTimer(timer);
            ReleaseSlot(timer);
            ReturnToPool(timer);
        }

        private static void RemoveActiveTimer(TimerHandlerBase timer)
        {
            var index = timer.ActiveIndex;
            if (index < 0 || index >= ActiveTimers.Count) return;
            if (!ReferenceEquals(ActiveTimers[index], timer)) return;

            RemoveAtIndex(index);
        }

        private static void RemoveAtIndex(int index)
        {
            var lastIndex = ActiveTimers.Count - 1;
            var removed = ActiveTimers[index];

            if (index != lastIndex)
            {
                var last = ActiveTimers[lastIndex];
                ActiveTimers[index] = last;
                last.ActiveIndex = index;
            }

            ActiveTimers.RemoveAt(lastIndex);

            if (removed != null)
            {
                removed.ActiveIndex = -1;
                removed.IsPendingRemoval = false;
            }
        }
    }
}
