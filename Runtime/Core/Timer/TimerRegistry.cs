using System.Collections.Generic;
using UnityEngine;

namespace NekoLib.Core
{
    [DisallowMultipleComponent]
    public class TimerRegistry : MonoBehaviour
    {
        private readonly List<TimerBase> _activeTimers = new();
        private readonly List<TimerBase> _timersToRemove = new();

        /// <summary>
        /// Gets the current count of active timers.
        /// </summary>
        public int ActiveTimerCount => _activeTimers.Count;

        private void Update()
        {
            // Update all active timers.
            for (int i = _activeTimers.Count - 1; i >= 0; i--)
            {
                var timer = _activeTimers[i];

                // Check if the owner GameObject/Component is destroyed.
                if (!timer.IsOwnerValid)
                {
                    // GameObject or component is destroyed, mark timer for removal.
                    _timersToRemove.Add(timer);
                    continue;
                }

                // Only update the timer if the owner is active and enabled.
                if (timer.IsOwnerActiveAndEnabled)
                {
                    // Use unscaled or scaled time based on timer preference.
                    float deltaTime = timer.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    timer?.Tick(deltaTime);
                }
            }

            // Remove invalid timers.
            if (_timersToRemove.Count > 0)
            {
                foreach (var timer in _timersToRemove)
                {
                    timer?.Dispose();
                    _activeTimers.Remove(timer);
                }
                _timersToRemove.Clear();
            }
        }

        /// <summary>
        /// Register a timer to be managed by this TimerRegistry.
        /// </summary>
        public void RegisterTimer(TimerBase timer)
        {
            if (timer == null)
            {
                Debug.LogWarning("Attempted to register a null timer.");
                return;
            }

            if (timer.Owner == null)
            {
                Debug.LogWarning("Attempted to register a timer with null owner.");
                return;
            }

            if (!_activeTimers.Contains(timer))
            {
                _activeTimers.Add(timer);
            }
        }

        /// <summary>
        /// Unregister a timer from this TimerRegistry.
        /// </summary>
        public void UnregisterTimer(TimerBase timer)
        {
            if (timer != null)
            {
                timer.Dispose();
                _activeTimers.Remove(timer);
            }
        }

        /// <summary>
        /// Clean up all timers owned by a specific MonoBehaviour component.
        /// </summary>
        public void CleanUpForComponent(MonoBehaviour ownerComponent)
        {
            if (ownerComponent == null)
                return;

            for (int i = _activeTimers.Count - 1; i >= 0; i--)
            {
                var timer = _activeTimers[i];
                if (timer.OwnerComponent == ownerComponent)
                {
                    timer.Dispose();
                    _activeTimers.RemoveAt(i);
                }
            }

            // Also clean up any in the removal list
            for (int i = _timersToRemove.Count - 1; i >= 0; i--)
            {
                var timer = _timersToRemove[i];
                if (timer.OwnerComponent == ownerComponent)
                {
                    timer.Dispose();
                    _timersToRemove.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Clean up all timers owned by a specific GameObject.
        /// </summary>
        public void CleanUpForObject(GameObject owner)
        {
            if (owner == null)
                return;

            for (int i = _activeTimers.Count - 1; i >= 0; i--)
            {
                var timer = _activeTimers[i];
                if (timer.Owner == owner)
                {
                    timer.Dispose();
                    _activeTimers.RemoveAt(i);
                }
            }

            // Also clean up any in the removal list
            for (int i = _timersToRemove.Count - 1; i >= 0; i--)
            {
                var timer = _timersToRemove[i];
                if (timer.Owner == owner)
                {
                    timer.Dispose();
                    _timersToRemove.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Cleans up all timers, disposing of them and clearing the registry.
        /// </summary>
        public void CleanupTimers()
        {
            // Dispose all active timers
            foreach (var timer in _activeTimers)
            {
                timer?.Dispose();
            }
            _activeTimers.Clear();

            // Dispose all timers in removal list
            foreach (var timer in _timersToRemove)
            {
                timer?.Dispose();
            }
            _timersToRemove.Clear();
        }

        private void OnDestroy()
        {
            CleanupTimers();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Get all active timers (for debugging/editor purposes).
        /// </summary>
        public IReadOnlyList<TimerBase> GetActiveTimers()
        {
            return _activeTimers.AsReadOnly();
        }
#endif
    }
}
