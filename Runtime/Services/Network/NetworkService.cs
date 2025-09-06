using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using NekoLib.Extensions;

namespace NekoLib.Services
{
    public static class NetworkService
    {
        private const float ConnectionMonitorIntervalSeconds = 5f;
        private const string PingUrl = "https://google.com";
        private const int RequestTimeoutSeconds = 5;

        public static event Action<bool> OnConnectionUpdate;

        public static bool IsOnline { get; private set; } = false;

        private static CancellationTokenSource _connectionMonitoringCts;

        /// <summary>
        /// Checks internet connection asynchronously using Task.
        /// </summary>
        public static async Task<bool> FetchInternetConnectionAsync(CancellationToken token = default)
        {
            using var request = UnityWebRequest.Get(PingUrl);
            request.timeout = RequestTimeoutSeconds; // Add timeout to prevent hanging

            try
            {
                var operation = request.SendWebRequest();

                // Wait for completion using cancellation token with proper delay.
                while (!operation.isDone)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(10, token);
                }
            }
            catch (OperationCanceledException)
            {
                IsOnline = false;
                return false;
            }

            bool isOnline = request.result == UnityWebRequest.Result.Success;
            IsOnline = isOnline;

            OnConnectionUpdate?.Invoke(isOnline);
            return isOnline;
        }

        public static void StartMonitoring(CancellationToken token = default)
        {
            // Prevent multiple monitoring instances.
            if (_connectionMonitoringCts != null && !_connectionMonitoringCts.IsCancellationRequested)
            {
                Debug.LogWarning("[NetworkService] Monitoring is already running.");
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

            Debug.Log("[NetworkService] Init internet monitoring.");

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
                Debug.Log("[NetworkService] Internet monitoring cancelled.");
            }
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
