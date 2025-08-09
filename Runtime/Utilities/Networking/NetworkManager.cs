using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using NekoLib.Singleton;
using NekoLib.Extensions;
using NekoLib.ColorPalette;

namespace NekoLib.Networking
{
    public sealed class NetworkManager : LazySingleton<NetworkManager>
    {
        private const float CheckIntervalSeconds = 5f;
        private const string PingUrl = "https://google.com";
        private const int TimeoutSeconds = 10;

        public event Action<bool> OnInternetRefresh;

        public bool HasInternet { get; private set; } = false;

        private CancellationTokenSource _monitoringCts;

        /// <summary>
        /// Checks internet connection asynchronously using UniTask.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation</param>
        public async UniTask<bool> CheckInternetConnectionAsync(CancellationToken token = default)
        {
            using var request = UnityWebRequest.Get(PingUrl);
            request.timeout = TimeoutSeconds; // Add timeout to prevent hanging

            // Use the provided token, or destroyCancellationToken if none provided.
            var effectiveToken = token == default ? destroyCancellationToken : token;

            try
            {
                await request.SendWebRequest().WithCancellation(effectiveToken);
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
                Debug.Log("Internet connection verified.".Colorize(Palette.MintEmerald));
            }
            else
            {
                Debug.LogWarning($"Failed to connect to {PingUrl.Italic()}".Colorize(Palette.VibrantRed));
            }

            OnInternetRefresh?.Invoke(isConnected);
            return isConnected;
        }

        /// <summary>
        /// Starts monitoring internet connection with UniTask.
        /// </summary>
        /// <param name="token">Cancellation token to stop monitoring</param>
        public async UniTaskVoid StartMonitoringAsync(CancellationToken token = default)
        {
            // Stop any existing monitoring
            StopMonitoring();

            // Create linked token that combines destroyCancellationToken with external token
            _monitoringCts = token == default
                ? CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken)
                : CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, token);

            Debug.Log("Init internet monitoring.".Colorize(Palette.PumpkinOrange));

            try
            {
                while (!_monitoringCts.IsCancellationRequested)
                {
                    // Pass the combined token directly - no double linking
                    await CheckInternetConnectionAsync(_monitoringCts.Token);
                    await UniTask.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), cancellationToken: _monitoringCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Internet monitoring cancelled.".Colorize(Palette.VibrantRed));
            }
            finally
            {
                _monitoringCts?.Dispose();
                _monitoringCts = null;
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
