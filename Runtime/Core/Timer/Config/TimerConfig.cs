namespace NekoLib.Timer
{
    /// <summary>Global configuration for NekoLib.Timer.</summary>
    public static class TimerConfig
    {
        /// <summary>Set the initial capacity of the timer world. Default is 32.</summary>
        public static void SetCapacity(int capacity) => TimerWorld.SetCapacity(capacity);
    }
}
