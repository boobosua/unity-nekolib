using System;
using System.Collections.Generic;
using NekoLib.Logger;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace NekoLib.Timer
{
    internal static partial class TimerWorld
    {
        private static bool _installed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitDomain()
        {
            _installed = false;

            ResetForDomain();
            EnsureInitialized();
            TryInstallPlayerLoop();
        }

        private static void TryInstallPlayerLoop()
        {
            if (_installed) return;

            try
            {
                var loop = PlayerLoop.GetCurrentPlayerLoop();
                if (!InjectIntoUpdateRecursive(ref loop))
                {
                    Log.Error("[NekoLib.Timer] Failed to locate Update loop in PlayerLoop.");
                    FallbackBehaviour.EnsureExists();
                    _installed = true;
                    return;
                }

                PlayerLoop.SetPlayerLoop(loop);
                _installed = true;
            }
            catch (Exception ex)
            {
                Log.Error($"[NekoLib.Timer] Failed to inject into PlayerLoop: {ex}");
                FallbackBehaviour.EnsureExists();
                _installed = true;
            }
        }

        private static bool InjectIntoUpdateRecursive(ref PlayerLoopSystem root)
        {
            var list = root.subSystemList;
            if (list == null) return false;

            for (int i = 0; i < list.Length; i++)
            {
                ref var sys = ref list[i];

                if (sys.type == typeof(Update))
                {
                    InjectIntoUpdateNode(ref sys);
                    root.subSystemList = list;
                    return true;
                }

                if (InjectIntoUpdateRecursive(ref sys))
                {
                    list[i] = sys;
                    root.subSystemList = list;
                    return true;
                }
            }

            return false;
        }

        private static void InjectIntoUpdateNode(ref PlayerLoopSystem updateNode)
        {
            var sub = updateNode.subSystemList ?? Array.Empty<PlayerLoopSystem>();

            var filtered = new List<PlayerLoopSystem>(sub.Length + 1);
            for (int i = 0; i < sub.Length; i++)
            {
                var s = sub[i];
                if (s.type == typeof(TimerWorld)) continue;
                if (s.updateDelegate == TickPlayerLoop) continue;
                filtered.Add(s);
            }

            filtered.Insert(0, new PlayerLoopSystem
            {
                type = typeof(TimerWorld),
                updateDelegate = TickPlayerLoop
            });

            updateNode.subSystemList = filtered.ToArray();
        }

        private static class FallbackBehaviour
        {
            private static GameObject _go;

            internal static void EnsureExists()
            {
                if (_go != null) return;

                _go = new GameObject("__NekoLib.TimerFallbackDriver")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                UnityEngine.Object.DontDestroyOnLoad(_go);
                _go.AddComponent<FallbackComponent>();
            }

            private sealed class FallbackComponent : MonoBehaviour
            {
                private void Update() => TickPlayerLoop();
                private void OnDestroy() => _go = null;
            }
        }
    }
}
