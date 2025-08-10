using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using NekoLib.Singleton;
using NekoLib.Extensions;
using NekoLib.ColorPalette;

namespace NekoLib.DateTimeServices
{
    public class DateTimeManager : LazySingleton<DateTimeManager>
    {
        private const string PrimaryUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";
        private const string HeaderUrl = "https://www.google.com";
        private const int TimeoutSeconds = 5;

        private DateTime _syncedTime;
        private float _syncedAtRealtime;
        private bool _hasSynced;

        public async UniTask FetchTimeFromServerAsync()
        {
            _syncedTime = DateTime.UtcNow;

            if (await TryFetchTimeFromTimeApi())
            {
                _hasSynced = true;
                _syncedAtRealtime = Time.realtimeSinceStartup;
                Debug.Log($"[DateTimeManager] Synced from TimeAPI.io: {_syncedTime}".Colorize(Palette.MintEmerald));
                return;
            }

            if (await TryFetchTimeFromHttpHeader())
            {
                _hasSynced = true;
                _syncedAtRealtime = Time.realtimeSinceStartup;
                Debug.Log($"[DateTimeManager] Synced from Google header: {_syncedTime}".Colorize(Palette.MintEmerald));
                return;
            }

            _hasSynced = false;
            Debug.LogWarning("[DateTimeManager] Failed to sync from all sources. Using fallback DateTime.UtcNow.".Colorize(Palette.VibrantRed));
        }

        /// <summary>
        /// Gets the current UTC time
        /// </summary>
        public DateTime UtcNow()
        {
            if (!_hasSynced)
            {
                Debug.LogWarning("[DateTimeManager] GetUtcNow() used before sync. Using DateTime.UtcNow.".Colorize(Palette.VibrantRed));
                return DateTime.UtcNow;
            }

            var drift = Time.realtimeSinceStartup - _syncedAtRealtime;
            return _syncedTime.AddSeconds(drift);
        }

        /// <summary>
        /// Gets the current server time in local timezone
        /// </summary>
        public DateTime Now()
        {
            return UtcNow().ToLocalTime();
        }

        /// <summary>
        /// Gets today's date (midnight) in server time
        /// </summary>
        public DateTime Today()
        {
            return UtcNow().Date;
        }

        /// <summary>
        /// Gets today's date (midnight) in local timezone
        /// </summary>
        public DateTime TodayLocal()
        {
            return Now().Date;
        }

        private async UniTask<bool> TryFetchTimeFromTimeApi()
        {
            using var request = UnityWebRequest.Get(PrimaryUrl);
            request.timeout = TimeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                await request.SendWebRequest().WithCancellation(destroyCancellationToken);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[DateTimeManager] TimeAPI.io error: {request.error}".Colorize(Palette.VibrantRed));
                    return false;
                }

                var json = request.downloadHandler.text;
                var result = JsonUtility.FromJson<TimeApiResponse>(json);

                // Parse as UTC to avoid timezone issues
                if (DateTime.TryParse(result.dateTime, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedTime))
                {
                    _syncedTime = parsedTime;
                    return true;
                }

                Debug.LogWarning($"[DateTimeManager] Failed to parse TimeAPI.io dateTime: {result.dateTime}".Colorize(Palette.VibrantRed));
                return false;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[DateTimeManager] TimeAPI.io request was cancelled".Colorize(Palette.VibrantRed));
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DateTimeManager] Failed to parse TimeAPI.io response: {e.Message}".Colorize(Palette.VibrantRed));
                return false;
            }
        }

        private async UniTask<bool> TryFetchTimeFromHttpHeader()
        {
            using var request = UnityWebRequest.Get(HeaderUrl);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = TimeoutSeconds;

            try
            {
                await request.SendWebRequest().WithCancellation(destroyCancellationToken);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[DateTimeManager] Google request error: {request.error}".Colorize(Palette.VibrantRed));
                    return false;
                }

                var header = request.GetResponseHeader("Date");
                if (string.IsNullOrEmpty(header))
                {
                    Debug.LogWarning("[DateTimeManager] 'Date' header missing from Google response.".Colorize(Palette.VibrantRed));
                    return false;
                }

                // Parse as UTC to avoid timezone issues
                if (DateTime.TryParse(header, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTime))
                {
                    _syncedTime = dateTime;
                    return true;
                }

                Debug.LogWarning($"[DateTimeManager] Failed to parse 'Date' header: {header}".Colorize(Palette.VibrantRed));
                return false;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[DateTimeManager] Google request was cancelled".Colorize(Palette.VibrantRed));
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DateTimeManager] Google request failed: {e.Message}".Colorize(Palette.VibrantRed));
                return false;
            }
        }

        [Serializable]
        private class TimeApiResponse
        {
            public string dateTime;
        }
    }
}
