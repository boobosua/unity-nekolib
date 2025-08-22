using System.Collections.Generic;
using UnityEngine;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
        // Dictionary caching all WaitForSeconds to avoid unnecessary new memory allocations.
        private static readonly Dictionary<float, WaitForSeconds> s_waitForSecondsDictionary = new();

        /// <summary>
        /// Retrieve a cached WaitForSeconds stored in a dictionary to avoid new allocation of memory.
        /// </summary>
        public static WaitForSeconds GetWaitForSeconds(float waitSeconds)
        {
            if (s_waitForSecondsDictionary.TryGetValue(waitSeconds, out var wait))
            {
                return wait;
            }

            s_waitForSecondsDictionary.Add(waitSeconds, new WaitForSeconds(waitSeconds));

            return s_waitForSecondsDictionary[waitSeconds];
        }

        private static readonly Dictionary<float, WaitForSecondsRealtime> s_waitForSecondsRealtimeDictionary = new();

        /// <summary>
        /// Retrieve a cached WaitForSecondsRealtime stored in a dictionary to avoid new allocation of memory.
        /// </summary>
        public static WaitForSecondsRealtime GetWaitForSecondsRealtime(float waitSeconds)
        {
            if (s_waitForSecondsRealtimeDictionary.TryGetValue(waitSeconds, out var wait))
            {
                return wait;
            }

            s_waitForSecondsRealtimeDictionary.Add(waitSeconds, new WaitForSecondsRealtime(waitSeconds));

            return s_waitForSecondsRealtimeDictionary[waitSeconds];
        }
    }
}
