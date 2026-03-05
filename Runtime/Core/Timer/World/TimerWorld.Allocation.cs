using System;
using NekoLib.Logger;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        private static bool _capacitySet;

        private static void EnsureInitialized()
        {
            if (_hotSlots != null) return;

            _hotSlots = new TimerSlotHot[DefaultCapacity];
            _coldSlots = new TimerSlotCold[DefaultCapacity];
            _free = new int[DefaultCapacity];
            _active = new int[DefaultCapacity];

            _freeCount = DefaultCapacity;
            for (int i = 0; i < DefaultCapacity; i++)
            {
                _hotSlots[i].Version = 1;
                _free[i] = DefaultCapacity - 1 - i;
            }
        }

        internal static void SetCapacity(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

            EnsureInitialized();
            _capacitySet = true;

            if (capacity <= _hotSlots.Length) return;

            int oldCap = _hotSlots.Length;

            Array.Resize(ref _hotSlots, capacity);
            Array.Resize(ref _coldSlots, capacity);
            Array.Resize(ref _free, capacity);
            Array.Resize(ref _active, capacity);

            for (int i = oldCap; i < capacity; i++)
            {
                _hotSlots[i].Version = 1;
                _free[_freeCount++] = capacity - 1 - (i - oldCap);
            }
#if UNITY_EDITOR
            EditorOnCapacityChanged?.Invoke();
#endif
        }

        private static TimerHandle AllocSlot()
        {
            EnsureInitialized();

            if (_freeCount == 0) Grow();

            int slot = _free[--_freeCount];
            ref var h = ref _hotSlots[slot];

            int version = h.Version;
            h.ClearHot();
            _coldSlots[slot].ClearCold();
            return new TimerHandle(slot, version);
        }

        private static void FreeSlot(int slot)
        {
            ref var h = ref _hotSlots[slot];
            h.ClearHot();
            _coldSlots[slot].ClearCold();
            h.Version++;
            if (h.Version == int.MaxValue) h.Version = 1;

            _free[_freeCount++] = slot;
        }

        private static void Grow()
        {
            int oldCap = _hotSlots.Length;
            int newCap = oldCap * 2;

            if (!_capacitySet)
            {
                Log.Warn($"[NekoLib.Timer] Capacity has been increased from {oldCap} to {newCap}. " +
                         $"Call NekoLib.TimerConfig.SetCapacity({newCap}) at startup to prevent runtime allocations.");
            }

            Array.Resize(ref _hotSlots, newCap);
            Array.Resize(ref _coldSlots, newCap);
            Array.Resize(ref _free, newCap);
            Array.Resize(ref _active, newCap);

            for (int i = oldCap; i < newCap; i++)
            {
                _hotSlots[i].Version = 1;
                _free[_freeCount++] = newCap - 1 - (i - oldCap);
            }
#if UNITY_EDITOR
            EditorOnCapacityChanged?.Invoke();
#endif
        }

        private static void RegisterActive(int slot)
        {
            if (_activeCount == _active.Length)
            {
                int oldCap = _active.Length;
                int newCap = oldCap * 2;
                if (!_capacitySet)
                    Log.Warn($"[NekoLib.Timer] Active list capacity has been increased from {oldCap} to {newCap}. " +
                             $"Call NekoLib.TimerConfig.SetCapacity({newCap}) at startup to prevent runtime allocations.");
                Array.Resize(ref _active, newCap);
            }

            _active[_activeCount++] = slot;
        }

        private static void KillSlot(int slot)
        {
            ref var h = ref _hotSlots[slot];
            if (h.IsPendingKill) return;
            h.IsPendingKill = true;
        }

        private static void CleanupPending()
        {
            for (int i = _activeCount - 1; i >= 0; i--)
            {
                int slot = _active[i];
                if (!_hotSlots[slot].IsPendingKill) continue;

                _active[i] = _active[--_activeCount];
                FreeSlot(slot);
            }
        }

        private static bool TryGetSlot(TimerHandle handle, out int slot)
        {
            slot = handle.Slot;
            if (_hotSlots == null) return false;
            if ((uint)slot >= (uint)_hotSlots.Length) return false;
            return _hotSlots[slot].Version == handle.Version && _hotSlots[slot].Kind != TimerKind.None;
        }
    }
}
