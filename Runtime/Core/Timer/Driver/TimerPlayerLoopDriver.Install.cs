using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace NekoLib.Core
{
    internal static partial class TimerPlayerLoopDriver
    {
        // Injects UpdateTimers into Unity's PlayerLoop Update phase.
        private static void InjectUpdateFunction(ref PlayerLoopSystem root)
        {
            var systems = root.subSystemList;
            if (systems == null) return;

            for (int i = 0; i < systems.Length; i++)
            {
                ref var sys = ref systems[i];
                if (sys.type != typeof(Update)) continue;

                var sub = sys.subSystemList;
                if (sub == null) sub = Array.Empty<PlayerLoopSystem>();

                var list = new List<PlayerLoopSystem>(sub.Length + 1);
                list.AddRange(sub);

                list.Insert(0, new PlayerLoopSystem
                {
                    type = typeof(TimerPlayerLoopDriver),
                    updateDelegate = UpdateTimers
                });

                sys.subSystemList = list.ToArray();
                root.subSystemList = systems;
                return;
            }
        }

        private static class FallbackBehaviour
        {
            private static GameObject _go;

            internal static void EnsureExists()
            {
                if (_go != null) return;

                _go = new GameObject("__TimerFallbackDriver")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                UnityEngine.Object.DontDestroyOnLoad(_go);
                _go.AddComponent<FallbackComponent>();
            }

            private sealed class FallbackComponent : MonoBehaviour
            {
                private void Update() => UpdateTimers();
                private void OnDestroy() => _go = null;
            }
        }
    }
}
