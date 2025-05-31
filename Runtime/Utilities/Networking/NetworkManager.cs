using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using NekoLib.Singleton;
using NekoLib.Utilities;
using NekoLib.Extensions;

namespace NekoLib.Networking
{
    public class NetworkManager : LazySingleton<NetworkManager>
    {
        private const float CheckInterval = 2f;
        private const string URL = "https://google.com";

        /// <summary>
        /// Called when the internet connection status changes.
        /// </summary>
        public event Action<bool> OnConnectionRefresh;

        private bool _hasInternet = false;
        public bool HasInternet
        {
            get
            {
                // if (!IsDiagnosticRunning)
                // {
                //     RunConnectionDiagnostic();
                //     return Application.internetReachability != NetworkReachability.NotReachable;
                // }

                return _hasInternet;
            }
        }

        public bool IsNetworkReachable => Application.internetReachability != NetworkReachability.NotReachable;

        private Coroutine _connectionDiagnosticRoutine = null;
        private bool IsDiagnosticRunning => _connectionDiagnosticRoutine != null;

        /// <summary>
        /// Run an internet connection diagnostic.
        /// </summary>
        /// <param name="interval"> Time delay between each diagnostic. </param>
        /// <param name="url"> URL of a website to ping. </param>
        public void RunConnectionDiagnostic(float interval = CheckInterval, string url = URL)
        {
            _connectionDiagnosticRoutine = StartCoroutine(ConnectionDiagnosticRoutine(interval, url));
        }

        public void StopConnectionDiagnostic()
        {
            if (_connectionDiagnosticRoutine != null)
            {
                StopCoroutine(_connectionDiagnosticRoutine);
                _connectionDiagnosticRoutine = null;
            }
        }

        private IEnumerator ConnectionDiagnosticRoutine(float interval, string url)
        {
            while (true)
            {
                using var request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    if (_hasInternet)
                    {
                        _hasInternet = false;

                        OnConnectionRefresh?.Invoke(_hasInternet);
                        Debug.LogWarning($"Internet connection lost.".Colorize(Color.red));
                    }
                }
                else
                {
                    if (!_hasInternet)
                    {
                        _hasInternet = true;

                        OnConnectionRefresh?.Invoke(_hasInternet);
                        Debug.Log($"Internet connection restored.".Colorize(Color.green));
                    }
                }

                yield return Utils.GetWaitForSecondsRealtime(interval);
            }
        }
    }
}
