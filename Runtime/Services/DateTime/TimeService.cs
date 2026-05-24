using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using TRnK.Logger;
using UnityEngine;
using UnityEngine.Networking;

namespace TRnK.Services
{
    /// <summary>
    /// Provides server-synced UTC time. Fetches once via HTTP Date headers (Google/Cloudflare/Microsoft)
    /// or TimeAPI.io, then extrapolates locally using <see cref="Time.realtimeSinceStartupAsDouble"/>.
    /// Falls back to <see cref="DateTime.UtcNow"/> if no sync has been performed.
    /// </summary>
    public static class TimeService
    {
        private const string TimeApiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";
        private const int TimeoutSeconds = 5;

        private static readonly string[] s_headerUrls =
        {
            "https://www.google.com",
            "https://www.cloudflare.com",
            "https://www.microsoft.com",
        };

        private static DateTime s_syncedUtcTime;
        private static double s_syncedAtRealtime;
        private static bool s_hasSynced;
        private static bool s_isFetching;
        private static bool s_hasLoggedUnsyncedWarning;

        // ─── Public API ──────────────────────────────────────────────────────────

        /// <summary>Whether the service has successfully synced with a time server.</summary>
        public static bool HasSynced => s_hasSynced;

        /// <summary>
        /// Current UTC time. Server-synced with drift compensation when synced,
        /// <see cref="DateTime.UtcNow"/> with a one-time warning otherwise.
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                if (!s_hasSynced)
                {
                    WarnIfUnsynced();
                    return DateTime.UtcNow;
                }

                double drift = Time.realtimeSinceStartupAsDouble - s_syncedAtRealtime;
                return s_syncedUtcTime.AddSeconds(drift);
            }
        }

        /// <summary>Current time in local timezone, derived from <see cref="UtcNow"/>.</summary>
        public static DateTime Now
        {
            get
            {
                if (!s_hasSynced)
                {
                    WarnIfUnsynced();
                    return DateTime.Now;
                }

                return UtcNow.ToLocalTime();
            }
        }

        /// <summary>Today's date (midnight) in UTC.</summary>
        public static DateTime TodayUtc => UtcNow.Date;

        /// <summary>Today's date (midnight) in local timezone.</summary>
        public static DateTime Today => Now.Date;

        /// <summary>Whether today (local) is Monday.</summary>
        public static bool IsTodayStartOfWeek => Today.DayOfWeek == DayOfWeek.Monday;

        /// <summary>Whether today (UTC) is Monday.</summary>
        public static bool IsTodayStartOfWeekUtc => TodayUtc.DayOfWeek == DayOfWeek.Monday;

        /// <summary>Whether today (local) is the 1st of the month.</summary>
        public static bool IsTodayStartOfMonth => Today.Day == 1;

        /// <summary>Whether today (UTC) is the 1st of the month.</summary>
        public static bool IsTodayStartOfMonthUtc => TodayUtc.Day == 1;

        /// <summary>Tomorrow at midnight, local timezone.</summary>
        public static DateTime NextDay => Today.AddDays(1);

        /// <summary>Tomorrow at midnight, UTC.</summary>
        public static DateTime NextDayUtc => TodayUtc.AddDays(1);

        // ─── Fetch ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Syncs with a time server. Tries HTTP Date headers first, then TimeAPI.io.
        /// Returns true if sync succeeded or was already synced. Safe to call concurrently.
        /// </summary>
        public static async Task<bool> FetchTimeFromServerAsync(CancellationToken token = default)
        {
            if (s_hasSynced)
                return true;

            if (s_isFetching)
            {
                while (s_isFetching)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
                return s_hasSynced;
            }

            s_isFetching = true;
            Log.Info("[TimeService] Starting time sync...");

            try
            {
                DateTime? time = await TryFetchFromHttpHeadersAsync(token);
                if (time.HasValue)
                {
                    MarkSynced(time.Value, "HTTP Date header");
                    return true;
                }

                time = await TryFetchFromTimeApiAsync(token);
                if (time.HasValue)
                {
                    MarkSynced(time.Value, "TimeAPI.io");
                    return true;
                }

                Log.Warn("[TimeService] Failed to sync from all sources.");
                return false;
            }
            catch (OperationCanceledException)
            {
                Log.Warn("[TimeService] Time sync was cancelled.");
                return false;
            }
            finally
            {
                s_isFetching = false;
            }
        }

        /// <summary>Coroutine wrapper for <see cref="FetchTimeFromServerAsync"/>.</summary>
        public static IEnumerator FetchTimeFromServerCoroutine(Action<bool> onDone = null)
        {
            Task<bool> task = FetchTimeFromServerAsync();
            while (!task.IsCompleted)
                yield return null;
            onDone?.Invoke(task.IsCompletedSuccessfully && task.Result);
        }

        // ─── HTTP Date Header ────────────────────────────────────────────────────

        private static async Task<DateTime?> TryFetchFromHttpHeadersAsync(CancellationToken token)
        {
            foreach (string url in s_headerUrls)
            {
                DateTime? result = await TryFetchFromHttpHeaderAsync(url, token);
                if (result.HasValue)
                    return result;
            }
            return null;
        }

        private static async Task<DateTime?> TryFetchFromHttpHeaderAsync(string url, CancellationToken token)
        {
            using var request = UnityWebRequest.Head(url);
            request.timeout = TimeoutSeconds;

            try
            {
                double startRealtime = Time.realtimeSinceStartupAsDouble;
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                double endRealtime = Time.realtimeSinceStartupAsDouble;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log.Warn($"[TimeService] HTTP header error ({url}): {request.error}");
                    return null;
                }

                string header = request.GetResponseHeader("Date");
                if (string.IsNullOrEmpty(header))
                {
                    Log.Warn($"[TimeService] Date header missing ({url}).");
                    return null;
                }

                if (DateTime.TryParseExact(header, "r", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out DateTime parsed))
                {
                    return ApplyLatencyCompensation(parsed, startRealtime, endRealtime);
                }

                Log.Warn($"[TimeService] Failed to parse Date header ({url}): {header}");
                return null;
            }
            catch (OperationCanceledException)
            {
                request.Abort();
                throw;
            }
            catch (Exception e)
            {
                Log.Warn($"[TimeService] HTTP header request failed ({url}): {e.Message}");
                return null;
            }
        }

        // ─── TimeAPI.io ──────────────────────────────────────────────────────────

        private static async Task<DateTime?> TryFetchFromTimeApiAsync(CancellationToken token)
        {
            using var request = UnityWebRequest.Get(TimeApiUrl);
            request.timeout = TimeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                double startRealtime = Time.realtimeSinceStartupAsDouble;
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                double endRealtime = Time.realtimeSinceStartupAsDouble;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log.Warn($"[TimeService] TimeAPI.io error: {request.error}");
                    return null;
                }

                string json = request.downloadHandler.text;
                var response = JsonUtility.FromJson<TimeApiResponse>(json);

                if (response == null || string.IsNullOrEmpty(response.dateTime))
                {
                    Log.Warn("[TimeService] TimeAPI.io response missing 'dateTime'.");
                    return null;
                }

                if (DateTime.TryParse(response.dateTime, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out DateTime parsed))
                {
                    return ApplyLatencyCompensation(parsed, startRealtime, endRealtime);
                }

                Log.Warn($"[TimeService] Failed to parse TimeAPI.io dateTime: {response.dateTime}");
                return null;
            }
            catch (OperationCanceledException)
            {
                request.Abort();
                throw;
            }
            catch (Exception e)
            {
                Log.Warn($"[TimeService] TimeAPI.io request failed: {e.Message}");
                return null;
            }
        }

        // ─── Internal ────────────────────────────────────────────────────────────

        private static DateTime ApplyLatencyCompensation(DateTime serverUtcTime, double startRealtime, double endRealtime)
        {
            // AddSeconds rounds to the nearest millisecond, which loses sub-ms RTT compensation.
            // Convert to ticks (100 ns) directly, with rounding to avoid systematic truncation bias.
            double rtt = Math.Max(0d, endRealtime - startRealtime);
            long compensationTicks = (long)Math.Round(rtt * 0.5d * TimeSpan.TicksPerSecond);
            return serverUtcTime.AddTicks(compensationTicks);
        }

        private static void MarkSynced(DateTime utcTime, string source)
        {
            s_syncedUtcTime = utcTime;
            s_syncedAtRealtime = Time.realtimeSinceStartupAsDouble;
            s_hasSynced = true;
            Log.Info($"[TimeService] Synced via {source}: {utcTime:O}");
        }

        private static void WarnIfUnsynced()
        {
            if (s_hasLoggedUnsyncedWarning) return;
            s_hasLoggedUnsyncedWarning = true;
            Log.Warn("[TimeService] Time not synced — using system clock. Call FetchTimeFromServerAsync first.");
        }

        [Serializable]
        private class TimeApiResponse
        {
            public string dateTime;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            if (!UnityEditor.EditorSettings.enterPlayModeOptionsEnabled ||
                !UnityEditor.EditorSettings.enterPlayModeOptions.HasFlag(
                    UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                return;

            s_syncedUtcTime = default;
            s_syncedAtRealtime = default;
            s_hasSynced = false;
            s_isFetching = false;
            s_hasLoggedUnsyncedWarning = false;
        }
#endif
    }
}
