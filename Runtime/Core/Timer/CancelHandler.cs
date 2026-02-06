namespace NekoLib.Core
{
    /// <summary>Token that can cancel a timer instance without invoking stop callbacks.</summary>
    public readonly struct CancelHandler
    {
        internal readonly int Slot;
        internal readonly int Id;

        internal CancelHandler(int slot, int id)
        {
            Slot = slot;
            Id = id;
        }

        /// <summary>Returns true while the referenced timer exists.</summary>
        public bool IsAlive => TimerPlayerLoopDriver.IsAlive(Slot, Id);

        /// <summary>Cancels the timer without invoking stop callbacks.</summary>
        public void Cancel() => TimerPlayerLoopDriver.Cancel(Slot, Id);
    }
}
