#if UNITY_EDITOR
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        internal struct TimerDebugEntry
        {
            public int Slot;
            public int Version;
            public TimerKind Kind;
            public bool IsRunning;
            public bool IsPendingKill;
            public bool UseUnscaledTime;
            public Object Owner;
            public float CountdownRemaining;
            public float CountdownTotal;
            public int LoopIteration;
            public int LoopCount;
            public float StopwatchElapsed;
        }

        internal static int EditorActiveCount => _activeCount;
        internal static int EditorCapacity => _hotSlots?.Length ?? 0;
        internal static int EditorFreeCount => _freeCount;
        internal static int EditorSlotMemoryBytes =>
            (_hotSlots?.Length ?? 0) * (Unsafe.SizeOf<TimerSlotHot>() + Unsafe.SizeOf<TimerSlotCold>());

        /// <summary>Fired on the main thread whenever the slot arrays are reallocated (Grow or SetCapacity).</summary>
        internal static event System.Action EditorOnCapacityChanged;

        internal static void GetDebugSnapshot(List<TimerDebugEntry> results)
        {
            results.Clear();
            if (_hotSlots == null) return;

            for (int i = 0; i < _activeCount; i++)
            {
                int slot = _active[i];
                ref var h = ref _hotSlots[slot];
                ref var c = ref _coldSlots[slot];

                results.Add(new TimerDebugEntry
                {
                    Slot = slot,
                    Version = h.Version,
                    Kind = h.Kind,
                    IsRunning = h.IsRunning,
                    IsPendingKill = h.IsPendingKill,
                    UseUnscaledTime = h.UseUnscaledTime,
                    Owner = h.Owner,
                    CountdownRemaining = h.CountdownRemaining,
                    CountdownTotal = c.CountdownTotal,
                    LoopIteration = c.LoopIteration,
                    LoopCount = c.LoopCount,
                    StopwatchElapsed = h.StopwatchElapsed,
                });
            }
        }
    }
}
#endif
