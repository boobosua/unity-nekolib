using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using NekoLib.Singleton;
using NekoLib.Utilities;
using NekoLib.Extensions;
using NekoLib.ColorPalette;

namespace NekoLib.Networking
{
    public class NetworkManager : LazySingleton<NetworkManager>
    {
        private const float CheckInterval = 2f;
        private const string URL = "https://google.com";

        public event Action<bool> OnConnectionRefresh;

        public bool HasInternet => Application.internetReachability != NetworkReachability.NotReachable;

        private Coroutine _connectionDiagnosticRoutine = null;

        private bool IsDiagnosticRunning => _connectionDiagnosticRoutine != null;

        public void InitConnectionDiagnostic(Action<bool> onComplete)
        {
            StartCoroutine(ConnectionDiagnosticRoutine(onComplete));
        }

        private IEnumerator ConnectionDiagnosticRoutine(Action<bool> onComplete)
        {
            using var request = UnityWebRequest.Get(URL);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(false);
                Debug.LogWarning($"Failed to connect to {URL.Italic()} via internet.".Colorize(Palette.VibrantRed));
            }
            else
            {
                onComplete?.Invoke(true);
                Debug.Log($"Internet connection successfully established.".Colorize(Palette.PumpkinOrange));
            }
        }

        public void RunConnectionDiagnosticLoop(float interval = CheckInterval, string url = URL)
        {
            _connectionDiagnosticRoutine = StartCoroutine(ConnectionDiagnosticLoopRoutine(interval, url));
        }

        public void StopConnectionDiagnosticLoop()
        {
            if (IsDiagnosticRunning)
            {
                StopCoroutine(_connectionDiagnosticRoutine);
                _connectionDiagnosticRoutine = null;
            }
        }

        private IEnumerator ConnectionDiagnosticLoopRoutine(float interval, string url)
        {
            while (true)
            {
                using var request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    OnConnectionRefresh?.Invoke(false);
                    Debug.LogWarning($"Internet connection lost.".Colorize(Palette.VibrantRed));
                }
                else
                {
                    OnConnectionRefresh?.Invoke(true);
                    Debug.Log($"Internet connection restored.".Colorize(Palette.MintEmerald));
                }

                yield return Utils.GetWaitForSecondsRealtime(interval);
            }
        }
    }
}
