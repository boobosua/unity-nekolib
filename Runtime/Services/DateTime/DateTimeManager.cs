using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using NekoLib.Utilities;
using NekoLib.Extensions;

namespace NekoLib.Services
{
    public sealed class DateTimeManager : PersistentSingleton<DateTimeManager>
    {
        private const string PrimaryUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";
        private const string HeaderUrl = "https://www.google.com";
        private const int TimeoutSeconds = 5;

        private DateTime _syncedUtcTime;
        private float _syncedAtRealtime;
        private bool _hasSynced;

        /// <summary>
        /// Fetches the current time from the server.
        /// </summary>
        public async Task FetchTimeFromServerAsync(CancellationToken token = default)
        {
            var effectiveToken = token == default
                ? destroyCancellationToken
                : CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, token).Token;

            _syncedUtcTime = DateTime.UtcNow;

            if (await TryFetchTimeFromTimeApi(effectiveToken))
            {
                _hasSynced = true;
                _syncedAtRealtime = Time.realtimeSinceStartup;
                Debug.Log($"[DateTimeManager] Synced from TimeAPI.io: {_syncedUtcTime}".Colorize(Swatch.ME));
                return;
            }

            if (await TryFetchTimeFromHttpHeader(effectiveToken))
            {
                _hasSynced = true;
                _syncedAtRealtime = Time.realtimeSinceStartup;
                Debug.Log($"[DateTimeManager] Synced from Google header: {_syncedUtcTime}".Colorize(Swatch.ME));
                return;
            }

            _hasSynced = false;
            Debug.LogWarning("[DateTimeManager] Failed to sync from all sources. Using fallback DateTime.UtcNow.".Colorize(Swatch.VR));
        }

        /// <summary>
        /// Tries to fetch the current time from the TimeAPI.io service.
        /// </summary>
        private async Task<bool> TryFetchTimeFromTimeApi(CancellationToken token = default)
        {
            using var request = UnityWebRequest.Get(PrimaryUrl);
            request.timeout = TimeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                var operation = request.SendWebRequest();

                // Wait for completion using cancellation token
                while (!operation.isDone)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[DateTimeManager] TimeAPI.io error: {request.error}".Colorize(Swatch.VR));
                    return false;
                }

                var json = request.downloadHandler.text;
                var result = JsonUtility.FromJson<TimeApiResponse>(json);

                // Parse as UTC to avoid timezone issues
                if (DateTime.TryParse(result.dateTime, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedTime))
                {
                    _syncedUtcTime = parsedTime;
                    return true;
                }

                Debug.LogWarning($"[DateTimeManager] Failed to parse TimeAPI.io dateTime: {result.dateTime}".Colorize(Swatch.VR));
                return false;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[DateTimeManager] TimeAPI.io request was cancelled".Colorize(Swatch.VR));
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DateTimeManager] Failed to parse TimeAPI.io response: {e.Message}".Colorize(Swatch.VR));
                return false;
            }
        }

        /// <summary>
        /// Tries to fetch the current time from the HTTP header.
        /// </summary>
        private async Task<bool> TryFetchTimeFromHttpHeader(CancellationToken token = default)
        {
            using var request = UnityWebRequest.Get(HeaderUrl);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = TimeoutSeconds;

            try
            {
                var operation = request.SendWebRequest();

                // Wait for completion using cancellation token
                while (!operation.isDone)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[DateTimeManager] Google request error: {request.error}".Colorize(Swatch.VR));
                    return false;
                }

                var header = request.GetResponseHeader("Date");
                if (string.IsNullOrEmpty(header))
                {
                    Debug.LogWarning("[DateTimeManager] 'Date' header missing from Google response.".Colorize(Swatch.VR));
                    return false;
                }

                // Parse as UTC to avoid timezone issues
                if (DateTime.TryParse(header, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTime))
                {
                    _syncedUtcTime = dateTime;
                    return true;
                }

                Debug.LogWarning($"[DateTimeManager] Failed to parse 'Date' header: {header}".Colorize(Swatch.VR));
                return false;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[DateTimeManager] Google request was cancelled".Colorize(Swatch.VR));
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DateTimeManager] Google request failed: {e.Message}".Colorize(Swatch.VR));
                return false;
            }
        }

        [Serializable]
        private class TimeApiResponse
        {
            public string dateTime;
        }

        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        public DateTime UtcNow()
        {
            if (!_hasSynced)
            {
                Debug.LogWarning("[DateTimeManager] Getting time before server sync. Using System.DateTime.".Colorize(Swatch.VR));
                return DateTime.UtcNow;
            }

            var drift = Time.realtimeSinceStartup - _syncedAtRealtime;
            return _syncedUtcTime.AddSeconds(drift);
        }

        /// <summary>
        /// Gets the current server time in local timezone.
        /// </summary>
        public DateTime Now()
        {
            return UtcNow().ToLocalTime();
        }

        /// <summary>
        /// Gets today's date (midnight) in server time.
        /// </summary>
        public DateTime TodayUtc()
        {
            return UtcNow().Date;
        }

        /// <summary>
        /// Gets today's date (midnight) in local timezone.
        /// </summary>
        public DateTime Today()
        {
            return Now().Date;
        }

        /// <summary>
        /// Checks if today (local time) is the start of the week (Monday by default).
        /// </summary>
        public bool IsTodayStartOfWeek()
        {
            return Today().DayOfWeek == DayOfWeek.Monday;
        }

        /// <summary>
        /// Checks if today (UTC) is the start of the week (Monday by default).
        /// </summary>
        public bool IsTodayStartOfWeekUtc()
        {
            return TodayUtc().DayOfWeek == DayOfWeek.Monday;
        }

        /// <summary>
        /// Checks if today (local time) is the start of the month (1st day).
        /// </summary>
        public bool IsTodayStartOfMonth()
        {
            return Today().Day == 1;
        }

        /// <summary>
        /// Checks if today (UTC) is the start of the month (1st day).
        /// </summary>
        public bool IsTodayStartOfMonthUtc()
        {
            return TodayUtc().Day == 1;
        }

        /// <summary>
        /// Gets tomorrow (00:00:00) from current local time.
        /// </summary>
        public DateTime NextDay()
        {
            return Today().AddDays(1);
        }

        /// <summary>
        /// Gets tomorrow (00:00:00) from current UTC time.
        /// </summary>
        public DateTime NextDayUtc()
        {
            return TodayUtc().AddDays(1);
        }
    }
}
