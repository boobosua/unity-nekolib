using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NekoLib.Core;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEngine;
using UnityEngine.Networking;

namespace NekoLib.Services
{
    public static class DateTimeService
    {
        private const string PrimaryUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";
        private const string HeaderUrl = "https://www.google.com";
        private const int TimeoutSeconds = 5;

        private static DateTime _syncedUtcTime;
        private static float _syncedAtRealtime;
        private static bool _hasSynced;

        /// <summary>
        /// Fetches the current time from the server.
        /// </summary>
        public static async Task FetchTimeFromServerAsync(CancellationToken token = default)
        {
            _syncedUtcTime = DateTime.UtcNow;

            if (await TryFetchTimeFromTimeApi(token))
            {
                _hasSynced = true;
                _syncedAtRealtime = Time.realtimeSinceStartup;
                Log.Info($"[DateTimeService] Synced from TimeAPI.io: {_syncedUtcTime.ToString().Colorize(Swatch.DE)}.");
                return;
            }

            if (await TryFetchTimeFromHttpHeader(token))
            {
                _hasSynced = true;
                _syncedAtRealtime = Time.realtimeSinceStartup;
                Log.Info($"[DateTimeService] Synced from Google header: {_syncedUtcTime.ToString().Colorize(Swatch.DE)}.");
                return;
            }

            _hasSynced = false;
            Log.Warn($"[DateTimeService] Failed to sync from all sources. Using fallback {nameof(DateTime.UtcNow).Colorize(Swatch.GA)}.");
        }

        /// <summary>
        /// Tries to fetch the current time from the TimeAPI.io service.
        /// </summary>
        private static async Task<bool> TryFetchTimeFromTimeApi(CancellationToken token = default)
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
                    Log.Warn($"[DateTimeService] TimeAPI.io error: {request.error.Colorize(Swatch.GA)}.");
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

                Log.Warn($"[DateTimeService] Failed to parse TimeAPI.io dateTime: {result.dateTime.Colorize(Swatch.GA)}.");
                return false;
            }
            catch (OperationCanceledException)
            {
                Log.Warn("[DateTimeService] TimeAPI.io request was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                Log.Warn($"[DateTimeService] Failed to parse TimeAPI.io response: {e.Message.Colorize(Swatch.GA)}.");
                return false;
            }
        }

        /// <summary>
        /// Tries to fetch the current time from the HTTP header.
        /// </summary>
        private static async Task<bool> TryFetchTimeFromHttpHeader(CancellationToken token = default)
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
                    Log.Warn($"[DateTimeService] Google request error: {request.error.Colorize(Swatch.GA)}.");
                    return false;
                }

                var header = request.GetResponseHeader("Date");
                if (string.IsNullOrEmpty(header))
                {
                    Log.Warn("[DateTimeService] 'Date' header missing from Google response.");
                    return false;
                }

                // Parse as UTC to avoid timezone issues
                if (DateTime.TryParse(header, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTime))
                {
                    _syncedUtcTime = dateTime;
                    return true;
                }

                Log.Warn($"[DateTimeService] Failed to parse 'Date' header: {header.Colorize(Swatch.GA)}.");
                return false;
            }
            catch (OperationCanceledException)
            {
                Log.Warn("[DateTimeService] Google request was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                Log.Warn($"[DateTimeService] Google request failed: {e.Message.Colorize(Swatch.GA)}.");
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
        public static DateTime UtcNow
        {
            get
            {
                if (!_hasSynced)
                {
                    // Log.Warn("[DateTimeService] Getting time before server sync. Using System.DateTime.".Colorize(Swatch.VR));
                    return DateTime.UtcNow;
                }

                var drift = Time.realtimeSinceStartup - _syncedAtRealtime;
                return _syncedUtcTime.AddSeconds(drift);
            }
        }

        /// <summary>
        /// Gets the current server time in local timezone.
        /// </summary>
        public static DateTime Now => UtcNow.ToLocalTime();

        /// <summary>
        /// Gets today's date (midnight) in server time.
        /// </summary>
        public static DateTime TodayUtc => UtcNow.Date;

        /// <summary>
        /// Gets today's date (midnight) in local timezone.
        /// </summary>
        public static DateTime Today => Now.Date;

        /// <summary>
        /// Checks if today (local time) is the start of the week (Monday by default).
        /// </summary>
        public static bool IsTodayStartOfWeek => Today.DayOfWeek == DayOfWeek.Monday;

        /// <summary>
        /// Checks if today (UTC) is the start of the week (Monday by default).
        /// </summary>
        public static bool IsTodayStartOfWeekUtc => TodayUtc.DayOfWeek == DayOfWeek.Monday;

        /// <summary>
        /// Checks if today (local time) is the start of the month (1st day).
        /// </summary>
        public static bool IsTodayStartOfMonth => Today.Day == 1;

        /// <summary>
        /// Checks if today (UTC) is the start of the month (1st day).
        /// </summary>
        public static bool IsTodayStartOfMonthUtc => TodayUtc.Day == 1;

        /// <summary>
        /// Gets tomorrow (00:00:00) from current local time.
        /// </summary>
        public static DateTime NextDay => Today.AddDays(1);

        /// <summary>
        /// Gets tomorrow (00:00:00) from current UTC time.
        /// </summary>
        public static DateTime NextDayUtc => TodayUtc.AddDays(1);

        /// <summary>
        /// Gets whether the service has successfully synced with a time server.
        /// </summary>
        public static bool HasSynced => _hasSynced;
    }
}