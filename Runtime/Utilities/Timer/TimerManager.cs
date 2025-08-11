using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NekoLib.Utilities
{
    public class TimerManager : LazySingleton<TimerManager>
    {
        private readonly List<TimerBase> _activeTimers = new();
        private readonly List<TimerBase> _timersToRemove = new();

        private void Update()
        {
            // Update all active timers
            for (int i = _activeTimers.Count - 1; i >= 0; i--)
            {
                var timer = _activeTimers[i];

                // Check if the owner GameObject/Component is destroyed
                if (!timer.IsOwnerValid)
                {
                    // GameObject or component is destroyed, mark timer for removal
                    _timersToRemove.Add(timer);
                    continue;
                }

                // Only update the timer if the owner is active and enabled
                if (timer.IsOwnerActiveAndEnabled)
                {
                    // Use unscaled or scaled time based on timer preference
                    float deltaTime = timer.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    timer.Tick(deltaTime);
                }
                // If disabled, the timer stays in the list but doesn't update
                // This preserves the timer state when re-enabled
            }

            // Remove invalid timers
            if (_timersToRemove.Count > 0)
            {
                foreach (var timer in _timersToRemove)
                {
                    _activeTimers.Remove(timer);
                }
                _timersToRemove.Clear();
            }
        }

        /// <summary>
        /// Register a timer to be managed by the TimerManager.
        /// </summary>
        public void RegisterTimer(TimerBase timer)
        {
            if (timer == null)
            {
                Debug.LogWarning("Attempted to register a null timer");
                return;
            }

            if (timer.Owner == null)
            {
                Debug.LogWarning("Attempted to register a timer with null owner");
                return;
            }

            if (!_activeTimers.Contains(timer))
            {
                _activeTimers.Add(timer);
            }
        }

        /// <summary>
        /// Unregister a timer from the TimerManager.
        /// </summary>
        public void UnregisterTimer(TimerBase timer)
        {
            if (timer != null)
            {
                _activeTimers.Remove(timer);
            }
        }

        /// <summary>
        /// Get all timers owned by a specific GameObject.
        /// </summary>
        public IEnumerable<TimerBase> GetTimersForObject(GameObject owner)
        {
            if (owner == null)
                return Enumerable.Empty<TimerBase>();

            return _activeTimers.Where(timer => timer.Owner == owner);
        }

        /// <summary>
        /// Get all timers owned by a specific MonoBehaviour component.
        /// </summary>
        public IEnumerable<TimerBase> GetTimersForComponent(MonoBehaviour component)
        {
            if (component == null)
                return Enumerable.Empty<TimerBase>();

            return _activeTimers.Where(timer => timer.OwnerComponent == component);
        }

        /// <summary>
        /// Stop and remove all timers owned by a specific GameObject.
        /// </summary>
        public void CleanupTimersForObject(GameObject owner)
        {
            if (owner == null) return;

            var timersToCleanup = GetTimersForObject(owner).ToList();
            foreach (var timer in timersToCleanup)
            {
                timer.Stop();
                UnregisterTimer(timer);
            }
        }

        /// <summary>
        /// Stop and remove all timers owned by a specific MonoBehaviour component.
        /// </summary>
        public void CleanupTimersForComponent(MonoBehaviour component)
        {
            if (component == null) return;

            var timersToCleanup = GetTimersForComponent(component).ToList();
            foreach (var timer in timersToCleanup)
            {
                timer.Stop();
                UnregisterTimer(timer);
            }
        }

        /// <summary>
        /// Get the total number of active timers.
        /// </summary>
        public int ActiveTimerCount => _activeTimers.Count;

#if UNITY_EDITOR
        /// <summary>
        /// Get debug information about all active timers.
        /// </summary>
        public string GetDebugInfo()
        {
            var info = $"Active Timers: {ActiveTimerCount}\n";
            for (int i = 0; i < _activeTimers.Count; i++)
            {
                var timer = _activeTimers[i];
                var ownerName = timer.Owner != null ? timer.Owner.name : "NULL";
                var componentInfo = timer.OwnerComponent != null ?
                    $" | Component: {timer.OwnerComponent.GetType().Name} (Enabled: {timer.OwnerComponent.enabled})" :
                    " | Component: None";

                var gameObjectActive = timer.Owner != null ? timer.Owner.activeInHierarchy : false;
                var timeInfo = timer.UseUnscaledTime ? "UnscaledTime" : "ScaledTime";
                var statusInfo = $" | Running: {timer.IsRunning} | Valid: {timer.IsOwnerValid} | Active&Enabled: {timer.IsOwnerActiveAndEnabled} | GameObject Active: {gameObjectActive} | Time: {timeInfo}";

                info += $"  [{i}] {timer.GetType().Name} - Owner: {ownerName}{componentInfo}{statusInfo}\n";
            }
            return info;
        }
#endif
    }
}
