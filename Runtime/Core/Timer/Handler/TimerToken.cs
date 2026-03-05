namespace NekoLib.Timer
{
    /// <summary>A restricted handle returned by fire-and-forget timer helpers.</summary>
    public readonly struct TimerToken
    {
        private readonly TimerHandle _handle;

        internal TimerToken(TimerHandle handle) => _handle = handle;

        /// <summary>Returns true while the underlying timer is still alive.</summary>
        public bool IsAlive => TimerWorld.IsAlive(_handle);

        /// <summary>Cancels the timer without invoking stop callbacks.</summary>
        public void Cancel() => TimerWorld.Cancel(_handle);
    }
}
