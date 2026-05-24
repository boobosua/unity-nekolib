using System;
using System.Threading;
using System.Threading.Tasks;
using TRnK.Extensions;
using TRnK.Logger;
using TRnK.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace TRnK.Services
{
    public static class NetworkService
    {
        private const float PollIntervalSeconds = 5f;
        private const int RequestTimeoutSeconds = 5;

        private static readonly string[] PingUrls =
        {
            "https://www.google.com",
            "https://www.cloudflare.com",
            "https://www.microsoft.com",
        };

        public static event Action<bool> OnConnectionChanged;

        public static bool IsOnline { get; private set; }

        private static CancellationTokenSource _cts;

        public static void StartMonitoring(CancellationToken externalToken = default)
        {
            StopMonitoring();

            _cts = externalToken == default
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(externalToken);

            MonitorAsync(_cts).Forget();
        }

        public static void StopMonitoring()
        {
            var cts = _cts;
            _cts = null;

            if (cts == null) return;

            try { cts.Cancel(); }
            catch (ObjectDisposedException) { }
        }

        public static void Dispose()
        {
            StopMonitoring();
            OnConnectionChanged = null;
        }

        private static async Task MonitorAsync(CancellationTokenSource cts)
        {
            Log.Info("[NetworkService] Monitoring started.");

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    var isOnline = await CheckConnectionAsync(cts.Token);
                    SetOnline(isOnline);
                    await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("[NetworkService] Monitoring stopped.");
            }
            finally
            {
                // Guard against nulling out a CTS from a newer StartMonitoring call.
                if (ReferenceEquals(_cts, cts))
                    _cts = null;

                cts.Dispose();
            }
        }

        private static async Task<bool> CheckConnectionAsync(CancellationToken token)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return false;

            foreach (var url in PingUrls)
            {
                using var request = UnityWebRequest.Head(url);
                request.timeout = RequestTimeoutSeconds;

                try
                {
                    var operation = request.SendWebRequest();

                    while (!operation.isDone)
                        await Task.Delay(100, token);

                    // ProtocolError means we reached the server (HTTP 4xx/5xx) — still online.
                    if (request.result is UnityWebRequest.Result.Success
                                       or UnityWebRequest.Result.ProtocolError)
                        return true;
                }
                catch (OperationCanceledException)
                {
                    request.Abort();
                    throw;
                }
            }

            return false;
        }

        private static void SetOnline(bool isOnline)
        {
            if (IsOnline == isOnline) return;

            IsOnline = isOnline;
            OnConnectionChanged?.Invoke(isOnline);
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            if (!Utils.IsReloadDomainDisabled())
                return;

            StopMonitoring();
            OnConnectionChanged = null;
            IsOnline = false;
        }
#endif
    }
}
