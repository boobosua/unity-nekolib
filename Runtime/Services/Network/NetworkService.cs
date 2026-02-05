using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NekoLib.Extensions;
using NekoLib.Logger;
using NekoLib.Utilities;
using UnityEngine;
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

        public static event Action<ConnectionStatus> OnConnectionUpdate;

        public static ConnectionStatus Status { get; private set; } = ConnectionStatus.Unknown;

        public static bool IsOnline => Status == ConnectionStatus.Online;

        private static CancellationTokenSource _connectionMonitoringCts;

        /// <summary>Checks internet connection (coroutine).</summary>
        public static IEnumerator FetchInternetConnectionCoroutine(Action<ConnectionStatus> onDone)
        {
            var status = ConnectionStatus.Offline;

            for (var i = 0; i < PingUrls.Length; i++)
            {
                using var request = UnityWebRequest.Head(PingUrls[i]);
                request.timeout = RequestTimeoutSeconds;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    status = ConnectionStatus.Online;
                    break;
                }
            }

            SetStatus(status);
            onDone?.Invoke(status);
        }

        /// <summary>Checks internet connection (async).</summary>
        public static async Task<ConnectionStatus> FetchInternetConnectionAsync(CancellationToken token = default)
        {
            var status = ConnectionStatus.Offline;

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
                    SetStatus(ConnectionStatus.Unknown);
                    return ConnectionStatus.Unknown;
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    status = ConnectionStatus.Online;
                    break;
                }
            }

            SetStatus(status);
            return status;
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

        /// <summary>Starts monitoring internet connection (async loop).</summary>
        private static async Task StartMonitoringAsync(CancellationToken token = default)
        {
            // Stop any existing monitoring.
            StopMonitoring();

            // Create combined cancellation token source
            var monitoringCts = token == default
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(token);

            _connectionMonitoringCts = monitoringCts;

            Log.Info("[NetworkService] Init internet monitoring.");

            try
            {
                while (!monitoringCts.IsCancellationRequested)
                {
                    // Pass the combined token directly - no double linking
                    await FetchInternetConnectionAsync(monitoringCts.Token);
                    await Task.Delay(TimeSpan.FromSeconds(ConnectionMonitorIntervalSeconds), monitoringCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("[NetworkService] Internet monitoring cancelled.");
            }
            finally
            {
                if (ReferenceEquals(_connectionMonitoringCts, monitoringCts))
                    _connectionMonitoringCts = null;

                monitoringCts.Dispose();
            }
        }

        private static void SetStatus(ConnectionStatus status)
        {
            if (Status == status) return;

            Status = status;
            OnConnectionUpdate?.Invoke(status);
        }

        /// <summary>Stops monitoring internet connection.</summary>
        public static void StopMonitoring()
        {
            var monitoringCts = _connectionMonitoringCts;
            _connectionMonitoringCts = null;

            if (monitoringCts == null)
                return;

            try
            {
                monitoringCts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Ignored.
            }
        }

        /// <summary>Stops monitoring and clears listeners.</summary>
        public static void Dispose()
        {
            StopMonitoring();
            OnConnectionUpdate = null;
            SetStatus(ConnectionStatus.Unknown);
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void EditorSubsystemRegistrationReset()
        {
            if (!Utils.IsReloadDomainDisabled())
                return;

            StopMonitoring();
            OnConnectionUpdate = null;
            Status = ConnectionStatus.Unknown;
        }
#endif
    }
}

