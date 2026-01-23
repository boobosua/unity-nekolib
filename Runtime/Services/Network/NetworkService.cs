using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEngine.Networking;

namespace NekoLib.Services
{
    public static class NetworkService
    {
        private const float ConnectionMonitorIntervalSeconds = 5f;
        private const int RequestTimeoutSeconds = 5;

        private static readonly string[] PingUrls =
        {
            "https://www.google.com",
            "https://www.cloudflare.com",
            "https://www.microsoft.com",
        };

        public static event Action<bool> OnConnectionUpdate;

        public static bool IsOnline { get; private set; } = false;

        private static CancellationTokenSource _connectionMonitoringCts;

        /// <summary>
        /// Checks internet connection using Coroutine.
        /// </summary>
        public static IEnumerator FetchInternetConnectionCoroutine(Action<bool> onDone)
        {
            var isOnline = false;

            for (var i = 0; i < PingUrls.Length; i++)
            {
                using var request = UnityWebRequest.Head(PingUrls[i]);
                request.timeout = RequestTimeoutSeconds;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    isOnline = true;
                    break;
                }
            }

            SetIsOnline(isOnline);
            onDone?.Invoke(isOnline);
        }

        /// <summary>
        /// Checks internet connection asynchronously using Task.
        /// </summary>
        public static async Task<bool> FetchInternetConnectionAsync(CancellationToken token = default)
        {
            var isOnline = false;

            for (var i = 0; i < PingUrls.Length; i++)
            {
                using var request = UnityWebRequest.Head(PingUrls[i]);
                request.timeout = RequestTimeoutSeconds;

                try
                {
                    var operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.Delay(10, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    request.Abort();
                    SetIsOnline(false);
                    return false;
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    isOnline = true;
                    break;
                }
            }

            SetIsOnline(isOnline);
            return isOnline;
        }

        public static void StartMonitoring(CancellationToken token = default)
        {
            // Prevent multiple monitoring instances.
            if (_connectionMonitoringCts != null && !_connectionMonitoringCts.IsCancellationRequested)
            {
                Log.Warn("[NetworkService] Monitoring is already running.");
                return;
            }

            StartMonitoringAsync(token).Forget();
        }

        /// <summary>
        /// Starts monitoring internet connection with Task.
        /// </summary>
        private static async Task StartMonitoringAsync(CancellationToken token = default)
        {
            // Stop any existing monitoring.
            StopMonitoring();

            // Create combined cancellation token source
            _connectionMonitoringCts = token == default
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(token);

            Log.Info("[NetworkService] Init internet monitoring.");

            try
            {
                while (!_connectionMonitoringCts.IsCancellationRequested)
                {
                    // Pass the combined token directly - no double linking
                    await FetchInternetConnectionAsync(_connectionMonitoringCts.Token);
                    await Task.Delay(TimeSpan.FromSeconds(ConnectionMonitorIntervalSeconds), _connectionMonitoringCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("[NetworkService] Internet monitoring cancelled.");
            }
        }

        private static void SetIsOnline(bool isOnline)
        {
            if (IsOnline == isOnline) return;

            IsOnline = isOnline;
            OnConnectionUpdate?.Invoke(isOnline);
        }

        /// <summary>
        /// Stops monitoring internet connection.
        /// </summary>
        public static void StopMonitoring()
        {
            _connectionMonitoringCts?.Cancel();
            _connectionMonitoringCts?.Dispose();
            _connectionMonitoringCts = null;
        }

        /// <summary>
        /// Disposes the NetworkService, stopping monitoring and clearing listeners.
        /// </summary>
        public static void Dispose()
        {
            StopMonitoring();
            OnConnectionUpdate = null;
        }
    }
}

