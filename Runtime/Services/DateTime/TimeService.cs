using System;
using System.Collections;
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
    public static class TimeService
    {
        // When defined, TimeService behaves like System.DateTime for easier local debugging.
        // Suggested Scripting Define Symbol: NEKO_TIME_DEBUG
        private static bool UseSystemTime
        {
            get
            {
#if NEKO_TIME_DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        private const string PrimaryUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";
        private const int TimeoutSeconds = 5;

        private static readonly string[] HeaderUrls =
        {
            "https://www.google.com",
            "https://www.cloudflare.com",
            "https://www.microsoft.com",
        };

        private static DateTime _syncedUtcTime;
        private static double _syncedAtRealtime;
        private static bool _hasSynced;
        private static bool _isFetching;
        private static bool _hasLoggedUnsyncedWarning;

        private static double GetRealtimeSinceStartup()
        {
#if UNITY_2020_2_OR_NEWER
            return Time.realtimeSinceStartupAsDouble;
#else
            return Time.realtimeSinceStartup;
#endif
        }

        /// <summary>
        /// Fetches the current time from the server (Coroutine version).
        /// </summary>
        public static IEnumerator FetchTimeFromServerCoroutine(Action<bool> onDone)
        {
            if (UseSystemTime)
            {
                onDone?.Invoke(true);
                yield break;
            }

            if (_hasSynced)
            {
                onDone?.Invoke(true);
                yield break;
            }

            if (_isFetching)
            {
                while (_isFetching)
                {
                    yield return null;
                }

                onDone?.Invoke(_hasSynced);
                yield break;
            }

            _isFetching = true;
            Log.Info("[TimeService] Starting time sync (coroutine)...");

            try
            {
                _syncedUtcTime = DateTime.UtcNow;

                var success = false;
                var utcTime = default(DateTime);

                yield return TryFetchTimeFromHttpHeaderCoroutine((ok, time) =>
                {
                    success = ok;
                    utcTime = time;
                });

                if (success)
                {
                    MarkSynced(utcTime, "HTTP Date header");
                    onDone?.Invoke(true);
                    yield break;
                }

                yield return TryFetchTimeFromTimeApiCoroutine((ok, time) =>
                {
                    success = ok;
                    utcTime = time;
                });

                if (success)
                {
                    MarkSynced(utcTime, "TimeAPI.io");
                    onDone?.Invoke(true);
                    yield break;
                }

                _hasSynced = false;
                Log.Warn("[TimeService] Failed to sync from all sources. Time remains unsynced.");
                onDone?.Invoke(false);
            }
            finally
            {
                _isFetching = false;
            }
        }

        /// <summary>
        /// Fetches the current time from the server.
        /// Returns true if the service successfully synced from an online source.
        /// </summary>
        public static async Task<bool> FetchTimeFromServerAsync(CancellationToken token = default)
        {
            if (UseSystemTime)
                return true;

            if (_hasSynced)
                return true;

            if (_isFetching)
            {
                while (_isFetching)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                return _hasSynced;
            }

            _isFetching = true;
            Log.Info("[TimeService] Starting time sync (async)...");

            try
            {
                _syncedUtcTime = DateTime.UtcNow;

                if (await TryFetchTimeFromHttpHeaderAsync(token))
                {
                    MarkSynced("HTTP Date header");
                    return true;
                }

                if (await TryFetchTimeFromTimeApiAsync(token))
                {
                    MarkSynced("TimeAPI.io");
                    return true;
                }

                _hasSynced = false;
                Log.Warn("[TimeService] Failed to sync from all sources. Time remains unsynced.");
                return false;
            }
            catch (OperationCanceledException)
            {
                Log.Warn("[TimeService] Time sync was cancelled.");
                return false;
            }
            finally
            {
                _isFetching = false;
            }
        }

        /// <summary>
        /// Fetches the current time from the TimeAPI.io service (Coroutine version).
        /// </summary>
        private static IEnumerator TryFetchTimeFromTimeApiCoroutine(Action<bool, DateTime> onDone)
        {
            using var request = UnityWebRequest.Get(PrimaryUrl);
            request.timeout = TimeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();

            var startRealtime = GetRealtimeSinceStartup();

            yield return request.SendWebRequest();

            var endRealtime = GetRealtimeSinceStartup();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log.Warn($"[TimeService] TimeAPI.io error: {request.error.Colorize(Swatch.GA)}.");
                onDone?.Invoke(false, default);
                yield break;
            }

            var json = request.downloadHandler.text;
            var result = JsonUtility.FromJson<TimeApiResponse>(json);

            if (result == null || string.IsNullOrEmpty(result.dateTime))
            {
                Log.Warn("[TimeService] TimeAPI.io response missing 'dateTime'.");
                onDone?.Invoke(false, default);
                yield break;
            }

            // Parse as UTC to avoid timezone issues
            if (DateTime.TryParse(result.dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedTime))
            {
                var adjustedTime = ApplyLatencyCompensation(parsedTime, startRealtime, endRealtime);
                onDone?.Invoke(true, adjustedTime);
                yield break;
            }

            Log.Warn($"[TimeService] Failed to parse TimeAPI.io dateTime: {result.dateTime.Colorize(Swatch.GA)}.");
            onDone?.Invoke(false, default);
        }

        /// <summary>
        /// Tries to fetch the current time from the TimeAPI.io service.
        /// </summary>
        private static async Task<bool> TryFetchTimeFromTimeApiAsync(CancellationToken token = default)
        {
            using var request = UnityWebRequest.Get(PrimaryUrl);
            request.timeout = TimeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                var startRealtime = GetRealtimeSinceStartup();
                var operation = request.SendWebRequest();

                // Wait for completion using cancellation token
                while (!operation.isDone)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                var endRealtime = GetRealtimeSinceStartup();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log.Warn($"[TimeService] TimeAPI.io error: {request.error.Colorize(Swatch.GA)}.");
                    return false;
                }

                var json = request.downloadHandler.text;
                var result = JsonUtility.FromJson<TimeApiResponse>(json);

                if (result == null || string.IsNullOrEmpty(result.dateTime))
                {
                    Log.Warn("[TimeService] TimeAPI.io response missing 'dateTime'.");
                    return false;
                }

                // Parse as UTC to avoid timezone issues
                if (DateTime.TryParse(result.dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedTime))
                {
                    _syncedUtcTime = ApplyLatencyCompensation(parsedTime, startRealtime, endRealtime);
                    return true;
                }

                Log.Warn($"[TimeService] Failed to parse TimeAPI.io dateTime: {result.dateTime.Colorize(Swatch.GA)}.");
                return false;
            }
            catch (OperationCanceledException)
            {
                request.Abort();
                Log.Warn("[TimeService] TimeAPI.io request was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                Log.Warn($"[TimeService] Failed to parse TimeAPI.io response: {e.Message.Colorize(Swatch.GA)}.");
                return false;
            }
        }

        /// <summary>
        /// Fetches the current time from the HTTP header (Coroutine version).
        /// </summary>
        private static IEnumerator TryFetchTimeFromHttpHeaderCoroutine(Action<bool, DateTime> onDone)
        {
            for (var i = 0; i < HeaderUrls.Length; i++)
            {
                var completed = false;
                var success = false;
                var utcTime = default(DateTime);

                yield return TryFetchTimeFromHttpHeaderCoroutine(HeaderUrls[i], (ok, time) =>
                {
                    success = ok;
                    utcTime = time;
                    completed = true;
                });

                if (completed && success)
                {
                    onDone?.Invoke(true, utcTime);
                    yield break;
                }
            }

            onDone?.Invoke(false, default);
        }

        private static IEnumerator TryFetchTimeFromHttpHeaderCoroutine(string url, Action<bool, DateTime> onDone)
        {
            using var request = UnityWebRequest.Head(url);
            request.timeout = TimeoutSeconds;

            var startRealtime = GetRealtimeSinceStartup();

            yield return request.SendWebRequest();

            var endRealtime = GetRealtimeSinceStartup();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log.Warn($"[TimeService] HTTP header request error ({url.Colorize(Swatch.GA)}): {request.error.Colorize(Swatch.GA)}.");
                onDone?.Invoke(false, default);
                yield break;
            }

            var header = request.GetResponseHeader("Date");
            if (string.IsNullOrEmpty(header))
            {
                Log.Warn($"[TimeService] 'Date' header missing from response ({url.Colorize(Swatch.GA)}).");
                onDone?.Invoke(false, default);
                yield break;
            }

            if (DateTime.TryParseExact(header, "r", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTime))
            {
                var adjustedTime = ApplyLatencyCompensation(dateTime, startRealtime, endRealtime);
                onDone?.Invoke(true, adjustedTime);
                yield break;
            }

            Log.Warn($"[TimeService] Failed to parse 'Date' header ({url.Colorize(Swatch.GA)}): {header.Colorize(Swatch.GA)}.");
            onDone?.Invoke(false, default);
        }

        /// <summary>
        /// Tries to fetch the current time from the HTTP header.
        /// </summary>
        private static async Task<bool> TryFetchTimeFromHttpHeaderAsync(CancellationToken token = default)
        {
            for (var i = 0; i < HeaderUrls.Length; i++)
            {
                if (await TryFetchTimeFromHttpHeaderAsync(HeaderUrls[i], token))
                    return true;
            }

            return false;
        }

        private static async Task<bool> TryFetchTimeFromHttpHeaderAsync(string url, CancellationToken token)
        {
            using var request = UnityWebRequest.Head(url);
            request.timeout = TimeoutSeconds;

            try
            {
                var startRealtime = GetRealtimeSinceStartup();
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                var endRealtime = GetRealtimeSinceStartup();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log.Warn($"[TimeService] HTTP header request error ({url.Colorize(Swatch.GA)}): {request.error.Colorize(Swatch.GA)}.");
                    return false;
                }

                var header = request.GetResponseHeader("Date");
                if (string.IsNullOrEmpty(header))
                {
                    Log.Warn($"[TimeService] 'Date' header missing from response ({url.Colorize(Swatch.GA)}).");
                    return false;
                }

                if (DateTime.TryParseExact(header, "r", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTime))
                {
                    _syncedUtcTime = ApplyLatencyCompensation(dateTime, startRealtime, endRealtime);
                    return true;
                }

                Log.Warn($"[TimeService] Failed to parse 'Date' header ({url.Colorize(Swatch.GA)}): {header.Colorize(Swatch.GA)}.");
                return false;
            }
            catch (OperationCanceledException)
            {
                request.Abort();
                Log.Warn($"[TimeService] HTTP header request was cancelled ({url.Colorize(Swatch.GA)}).");
                return false;
            }
            catch (Exception e)
            {
                Log.Warn($"[TimeService] HTTP header request failed ({url.Colorize(Swatch.GA)}): {e.Message.Colorize(Swatch.GA)}.");
                return false;
            }
        }

        /// <summary>
        /// Applies latency compensation to the given server UTC time.
        /// </summary>
        private static DateTime ApplyLatencyCompensation(DateTime serverUtcTime, double requestStartRealtime, double requestEndRealtime)
        {
            var rttSeconds = Math.Max(0d, requestEndRealtime - requestStartRealtime);
            return serverUtcTime.AddSeconds(rttSeconds * 0.5d);
        }

        /// <summary>
        /// Marks the service as synced with the given UTC time and source.
        /// </summary>
        private static void MarkSynced(DateTime utcTime, string source)
        {
            _syncedUtcTime = utcTime;
            MarkSynced(source);
        }

        /// <summary>
        /// Marks the service as synced using the current value of <see cref="_syncedUtcTime"/>.
        /// </summary>
        private static void MarkSynced(string source)
        {
            _hasSynced = true;
            _syncedAtRealtime = GetRealtimeSinceStartup();
            Log.Info($"[TimeService] Synced from {source}: {_syncedUtcTime.ToString().Colorize(Swatch.DE)}.");
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
                if (UseSystemTime)
                {
                    return DateTime.UtcNow;
                }

                if (!_hasSynced)
                {
                    if (!_hasLoggedUnsyncedWarning)
                    {
                        _hasLoggedUnsyncedWarning = true;
                        Log.Warn($"[TimeService] Using unsynced time (falling back to {nameof(DateTime.UtcNow).Colorize(Swatch.GA)}). Call {nameof(FetchTimeFromServerAsync).Colorize(Swatch.GA)} first for server time.");
                    }
                    return DateTime.UtcNow;
                }

                var drift = GetRealtimeSinceStartup() - _syncedAtRealtime;
                return _syncedUtcTime.AddSeconds(drift);
            }
        }

        /// <summary>
        /// Gets the current server time in local timezone.
        /// </summary>
        public static DateTime Now
        {
            get
            {
                if (UseSystemTime)
                {
                    return DateTime.Now;
                }

                if (!_hasSynced)
                {
                    if (!_hasLoggedUnsyncedWarning)
                    {
                        _hasLoggedUnsyncedWarning = true;
                        Log.Warn($"[TimeService] Using unsynced time (falling back to {nameof(DateTime.Now).Colorize(Swatch.GA)}). Call {nameof(FetchTimeFromServerAsync).Colorize(Swatch.GA)} first for server time.");
                    }
                    return DateTime.Now;
                }

                return UtcNow.ToLocalTime();
            }
        }

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