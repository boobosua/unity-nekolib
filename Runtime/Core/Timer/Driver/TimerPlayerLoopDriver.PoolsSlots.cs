using UnityEngine;

namespace NekoLib.Core
{
    internal static partial class TimerPlayerLoopDriver
    {
        internal static void PrewarmCountdown(int count)
        {
            if (count <= 0) return;

            _maxCountdownPoolSize = Mathf.Max(_maxCountdownPoolSize, count);

            while (CountdownPool.Count < count)
            {
                var h = new CountdownHandler();
                h.ResetForPool();
                CountdownPool.Push(h);
            }
        }

        internal static void PrewarmStopwatch(int count)
        {
            if (count <= 0) return;

            _maxStopwatchPoolSize = Mathf.Max(_maxStopwatchPoolSize, count);

            while (StopwatchPool.Count < count)
            {
                var h = new StopwatchHandler();
                h.ResetForPool();
                StopwatchPool.Push(h);
            }
        }

        // Slot/Id are used to invalidate stale struct references after a timer is returned to the pool.
        private static int AllocateSlot(TimerHandlerBase timer)
        {
            if (FreeSlots.Count > 0)
            {
                var slot = FreeSlots.Pop();
                var rec = Slots[slot];

                rec.Timer = timer;
                rec.Id++;
                if (rec.Id == 0) rec.Id = 1;

                Slots[slot] = rec;
                return slot;
            }

            var newSlot = Slots.Count;
            Slots.Add(new SlotRecord { Id = 1, Timer = timer });
            return newSlot;
        }

        private static void ReleaseSlot(TimerHandlerBase timer)
        {
            var slot = timer.Slot;
            if (slot < 0 || slot >= Slots.Count)
            {
                timer.ClearHandle();
                return;
            }

            var rec = Slots[slot];
            if (ReferenceEquals(rec.Timer, timer))
            {
                rec.Timer = null;
                Slots[slot] = rec;
                FreeSlots.Push(slot);
            }

            timer.ClearHandle();
        }

        private static bool TryGet(int slot, int id, out TimerHandlerBase timer)
        {
            timer = null;

            if (slot < 0 || slot >= Slots.Count) return false;

            var rec = Slots[slot];
            if (rec.Timer == null || rec.Id != id) return false;

            timer = rec.Timer;
            return true;
        }

        private static CountdownHandler RentCountdown()
        {
            return CountdownPool.Count > 0 ? CountdownPool.Pop() : new CountdownHandler();
        }

        private static StopwatchHandler RentStopwatch()
        {
            return StopwatchPool.Count > 0 ? StopwatchPool.Pop() : new StopwatchHandler();
        }

        private static void ReturnToPool(TimerHandlerBase timer)
        {
            timer.ResetForPool();

            if (timer is CountdownHandler cd)
            {
                if (CountdownPool.Count < _maxCountdownPoolSize)
                {
                    CountdownPool.Push(cd);
                }

                return;
            }

            if (timer is StopwatchHandler sw)
            {
                if (StopwatchPool.Count < _maxStopwatchPoolSize)
                {
                    StopwatchPool.Push(sw);
                }
            }
        }
    }
}
