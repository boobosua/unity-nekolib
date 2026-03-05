using System;

namespace NekoLib.Timer
{
    /// <summary>Lightweight handle that references an internal timer slot.</summary>
    public readonly struct TimerHandle : IEquatable<TimerHandle>
    {
        internal readonly int Slot;
        internal readonly int Version;

        internal TimerHandle(int slot, int version)
        {
            Slot = slot;
            Version = version;
        }

        /// <summary>Returns true if this handle references a currently alive timer.</summary>
        public bool IsAlive => TimerWorld.IsAlive(this);

        public bool Equals(TimerHandle other) => Slot == other.Slot && Version == other.Version;
        public override bool Equals(object obj) => obj is TimerHandle other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Slot, Version);
        public static bool operator ==(TimerHandle a, TimerHandle b) => a.Equals(b);
        public static bool operator !=(TimerHandle a, TimerHandle b) => !a.Equals(b);
    }
}
