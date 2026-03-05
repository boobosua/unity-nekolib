namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        internal static void ResetForDomain()
        {
            _hotSlots = null;
            _coldSlots = null;
            _free = null;
            _freeCount = 0;
            _active = null;
            _activeCount = 0;
            _capacitySet = false;
        }
    }
}
