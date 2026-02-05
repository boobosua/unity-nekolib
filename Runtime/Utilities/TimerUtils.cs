using NekoLib.Core;

namespace NekoLib.Utilities
{
    public static class TimerUtils
    {
        /// <summary>Prewarms the countdown timer pool with the specified number of instances.</summary>
        public static void PrewarmCountdown(int count)
        {
            TimerPlayerLoopDriver.PrewarmCountdown(count);
        }

        /// <summary>Prewarms the stopwatch timer pool with the specified number of instances.</summary>
        public static void PrewarmStopwatch(int count)
        {
            TimerPlayerLoopDriver.PrewarmStopwatch(count);
        }
    }
}
