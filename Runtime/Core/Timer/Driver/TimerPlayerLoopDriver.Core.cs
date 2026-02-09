using System;
using System.Collections.Generic;
using NekoLib.Logger;
using NekoLib.Utilities;
using UnityEngine;
using UnityEngine.LowLevel;

namespace NekoLib.Core
{
    [UnityEngine.Scripting.Preserve]
    /// <summary>Owns and updates timers by injecting an update hook into Unity's PlayerLoop.</summary>
    internal static partial class TimerPlayerLoopDriver
    {
        private const int DefaultActiveCapacity = 128;
        private const int DefaultRemovalCapacity = 32;
        private const int DefaultPoolCapacity = 8;

        private static readonly List<TimerHandlerBase> ActiveTimers = new(DefaultActiveCapacity);
        private static readonly List<TimerHandlerBase> ToRemove = new(DefaultRemovalCapacity);

        private static readonly List<SlotRecord> Slots = new(DefaultActiveCapacity);
        private static readonly Stack<int> FreeSlots = new(DefaultActiveCapacity);

        private static readonly Stack<CountdownHandler> CountdownPool = new(DefaultPoolCapacity);
        private static readonly Stack<StopwatchHandler> StopwatchPool = new(DefaultPoolCapacity);

        private static int _maxCountdownPoolSize = 128;
        private static int _maxStopwatchPoolSize = 128;

        private static bool _installed;
        private static bool _isUpdating;

        private static bool _didDefaultCountdownPrewarm;

        private struct SlotRecord
        {
            public int Id;
            public TimerHandlerBase Timer;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        /// <summary>Resets static state on domain reload and (re)installs the driver into the PlayerLoop.</summary>
        private static void InitDomain()
        {
            ResetStaticState();
            TryInstall();
            EnsureDefaultCountdownPrewarm();
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitPlaySession()
        {
            if (!Application.isPlaying) return;
            if (!Utils.IsReloadDomainDisabled()) return;

            ResetStaticState();
            TryInstall();
            EnsureDefaultCountdownPrewarm();
        }
#endif

        private static void ResetStaticState()
        {
            ActiveTimers.Clear();
            ToRemove.Clear();
            Slots.Clear();
            FreeSlots.Clear();

            CountdownPool.Clear();
            StopwatchPool.Clear();

            _maxCountdownPoolSize = 128;
            _maxStopwatchPoolSize = 128;

            _installed = false;
            _isUpdating = false;

            _didDefaultCountdownPrewarm = false;
        }

        private static void EnsureDefaultCountdownPrewarm()
        {
            if (_didDefaultCountdownPrewarm) return;

            _didDefaultCountdownPrewarm = true;

            // Silent default prewarm at system activation.
            PrewarmCountdown(DefaultPoolCapacity);
        }

        private static void TryInstall()
        {
            if (_installed) return;

            try
            {
                var loop = PlayerLoop.GetCurrentPlayerLoop();
                InjectUpdateFunction(ref loop);
                PlayerLoop.SetPlayerLoop(loop);
                _installed = true;
            }
            catch (Exception ex)
            {
                Log.Error($"[TimerPlayerLoopDriver] Failed to inject into PlayerLoop: {ex}");
                FallbackBehaviour.EnsureExists();
                _installed = true;
            }
        }
    }
}
