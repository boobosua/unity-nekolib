using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using NekoLib.Utilities;
using NekoLib.Extensions;

namespace NekoLib.Services
{
    public sealed class NetworkManager : LazySingleton<NetworkManager>
    {
        private const float CheckIntervalSeconds = 5f;
        private const string PingUrl = "https://google.com";
        private const int TimeoutSeconds = 5;

        public event Action<bool> OnInternetRefresh;

        public bool HasInternet { get; private set; } = false;

        private CancellationTokenSource _monitoringCts;

        /// <summary>
        /// Checks internet connection asynchronously using Task.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation</param>
        public async Task<bool> CheckInternetConnectionAsync(CancellationToken token = default)
        {
            using var request = UnityWebRequest.Get(PingUrl);
            request.timeout = TimeoutSeconds; // Add timeout to prevent hanging

            var cancellationToken = token == default
                ? destroyCancellationToken
                : CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, token).Token;

            try
            {
                var operation = request.SendWebRequest();

                // Wait for completion using cancellation token
                while (!operation.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
            }
            catch (OperationCanceledException)
            {
                HasInternet = false;
                return false;
            }

            bool isConnected = request.result == UnityWebRequest.Result.Success;
            HasInternet = isConnected;

            if (isConnected)
            {
                Debug.Log("[NetworkManager] Internet connection verified.".Colorize(Palette.MintEmerald));
            }
            else
            {
                Debug.LogWarning($"[NetworkManager] Failed to connect to {PingUrl.Italic()}".Colorize(Palette.VibrantRed));
            }

            OnInternetRefresh?.Invoke(isConnected);
            return isConnected;
        }

        public void StartMonitoring(CancellationToken token = default)
        {
            // Prevent multiple monitoring instances
            if (_monitoringCts != null && !_monitoringCts.IsCancellationRequested)
            {
                Debug.LogWarning("[NetworkManager] Monitoring is already running.".Colorize(Palette.PumpkinOrange));
                return;
            }

            _ = StartMonitoringAsync(token);
        }

        /// <summary>
        /// Starts monitoring internet connection with Task.
        /// </summary>
        /// <param name="token">Cancellation token to stop monitoring</param>
        private async Task StartMonitoringAsync(CancellationToken token = default)
        {
            // Stop any existing monitoring
            StopMonitoring();

            // Create linked token that combines destroyCancellationToken with external token
            _monitoringCts = token == default
                ? CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken)
                : CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, token);

            Debug.Log("[NetworkManager] Init internet monitoring.".Colorize(Palette.PumpkinOrange));

            try
            {
                while (!_monitoringCts.IsCancellationRequested)
                {
                    // Pass the combined token directly - no double linking
                    await CheckInternetConnectionAsync(_monitoringCts.Token);
                    await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), _monitoringCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[NetworkManager] Internet monitoring cancelled.".Colorize(Palette.VibrantRed));
            }
        }

        /// <summary>
        /// Stops monitoring internet connection.
        /// </summary>
        public void StopMonitoring()
        {
            _monitoringCts?.Cancel();
            _monitoringCts?.Dispose();
            _monitoringCts = null;
        }

        private void OnDestroy()
        {
            StopMonitoring();
        }
    }
}
