using System;
using System.Text;
using UnityEngine;
using NekoLib.Services;

namespace NekoLib.Extensions
{
    public static class TimeExtensions
    {
        private const double SecondsPerMinute = 60.0;
        private const double SecondsPerHour = 3600.0;
        private const double SecondsPerDay = 86400.0;

        private static readonly StringBuilder _stringBuilder = new(32);

        #region Time to String Formatting
        /// <summary>
        /// Converts time in seconds to HH:MM:SS format.
        /// </summary>
        public static string ToClock(this float time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Converts time in seconds to HH:MM:SS format.
        /// </summary>
        public static string ToClock(this double time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Converts time in seconds to HH:MM:SS format.
        /// </summary>
        public static string ToClock(this int time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Converts time in seconds to MM:SS format.
        /// </summary>
        public static string ToShortClock(this float time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"mm\:ss");
        }

        /// <summary>
        /// Converts time in seconds to MM:SS format.
        /// </summary>
        public static string ToShortClock(this double time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"mm\:ss");
        }

        /// <summary>
        /// Converts time in seconds to MM:SS format.
        /// </summary>
        public static string ToShortClock(this int time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"mm\:ss");
        }

        /// <summary>
        /// Converts TimeSpan to readable format (e.g., "2d 3h 45m").
        /// </summary>
        public static string ToReadableFormat(this TimeSpan timeSpan, bool useSpacing = false)
        {
            _stringBuilder.Clear();

            var totalDays = (int)timeSpan.TotalDays;
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            if (totalDays >= 1)
            {
                _stringBuilder.Append(totalDays).Append('d');
                if (useSpacing) _stringBuilder.Append(' ');
                _stringBuilder.Append(hours).Append('h');
                if (useSpacing) _stringBuilder.Append(' ');
                _stringBuilder.Append(minutes).Append('m');
            }
            else if (timeSpan.TotalHours >= 1)
            {
                _stringBuilder.Append(hours).Append('h');
                if (useSpacing) _stringBuilder.Append(' ');
                _stringBuilder.Append(minutes).Append('m');
                if (useSpacing) _stringBuilder.Append(' ');
                _stringBuilder.Append(seconds).Append('s');
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                _stringBuilder.Append(minutes).Append('m');
                if (useSpacing) _stringBuilder.Append(' ');
                _stringBuilder.Append(seconds).Append('s');
            }
            else
            {
                _stringBuilder.Append(seconds).Append('s');
            }

            return _stringBuilder.ToString();
        }

        /// <summary>
        /// Converts seconds to readable format.
        /// </summary>
        public static string ToReadableFormat(this double seconds, bool useSpacing = true)
        {
            return TimeSpan.FromSeconds(seconds).ToReadableFormat(useSpacing);
        }

        /// <summary>
        /// Converts seconds to readable format.
        /// </summary>
        public static string ToReadableFormat(this float seconds, bool useSpacing = true)
        {
            return TimeSpan.FromSeconds(seconds).ToReadableFormat(useSpacing);
        }

        #endregion

        #region DateTime Time Calculations

        /// <summary>
        /// Gets the time span from this DateTime to now (for past times).
        /// </summary>
        public static TimeSpan TimeUntilNow(this DateTime time)
        {
            return DateTimeService.Now - time;
        }

        /// <summary>
        /// Gets the time span from this DateTime to now (UTC, for past times).
        /// </summary>
        public static TimeSpan TimeUntilNowUtc(this DateTime time)
        {
            return DateTimeService.UtcNow - time;
        }

        /// <summary>
        /// Gets seconds elapsed from this DateTime to now (for past times).
        /// </summary>
        public static double SecondsUntilNow(this DateTime time)
        {
            var now = DateTimeService.Now;
            var diff = (now - time).TotalSeconds;

            if (diff < 0)
            {
                Debug.LogWarning($"[TimeExtensions] SecondsUntilNow called with future time. Expected past time, got: {time}. Returning 0.");
                return 0;
            }

            return diff;
        }

        /// <summary>
        /// Gets seconds elapsed from this DateTime to now (UTC, for past times).
        /// </summary>
        public static double SecondsUntilNowUtc(this DateTime time)
        {
            var now = DateTimeService.UtcNow;
            var diff = (now - time).TotalSeconds;

            if (diff < 0)
            {
                Debug.LogWarning($"[TimeExtensions] SecondsUntilNowUtc called with future time. Expected past time, got: {time}. Returning 0.");
                return 0;
            }

            return diff;
        }

        /// <summary>
        /// Gets minutes elapsed from this DateTime to now (for past times).
        /// </summary>
        public static double MinutesUntilNow(this DateTime time)
        {
            return SecondsUntilNow(time) / SecondsPerMinute;
        }

        /// <summary>
        /// Gets minutes elapsed from this DateTime to now (UTC, for past times).
        /// </summary>
        public static double MinutesUntilNowUtc(this DateTime time)
        {
            return SecondsUntilNowUtc(time) / SecondsPerMinute;
        }

        /// <summary>
        /// Gets hours elapsed from this DateTime to now (for past times).
        /// </summary>
        public static double HoursUntilNow(this DateTime time)
        {
            return SecondsUntilNow(time) / SecondsPerHour;
        }

        /// <summary>
        /// Gets hours elapsed from this DateTime to now (UTC, for past times).
        /// </summary>
        public static double HoursUntilNowUtc(this DateTime time)
        {
            return SecondsUntilNowUtc(time) / SecondsPerHour;
        }

        /// <summary>
        /// Gets days elapsed from this DateTime to now (for past times).
        /// </summary>
        public static double DaysUntilNow(this DateTime time)
        {
            return SecondsUntilNow(time) / SecondsPerDay;
        }

        /// <summary>
        /// Gets days elapsed from this DateTime to now (UTC, for past times).
        /// </summary>
        public static double DaysUntilNowUtc(this DateTime time)
        {
            return SecondsUntilNowUtc(time) / SecondsPerDay;
        }

        #endregion

        #region DateTime Future Time Calculations

        /// <summary>
        /// Gets the time span from this DateTime to now (for future times).
        /// </summary>
        public static TimeSpan TimeFromNow(this DateTime time)
        {
            return time - DateTimeService.Now;
        }

        /// <summary>
        /// Gets the time span from this DateTime to now (UTC, for past times).
        /// </summary>
        public static TimeSpan TimeFromNowUtc(this DateTime time)
        {
            return time - DateTimeService.UtcNow;
        }

        /// <summary>
        /// Gets seconds from now until this DateTime (for future times).
        /// </summary>
        public static double SecondsFromNow(this DateTime time)
        {
            var now = DateTimeService.Now;
            var diff = (time - now).TotalSeconds;

            if (diff < 0)
            {
                Debug.LogWarning($"[TimeExtensions] SecondsFromNow called with past time. Expected future time, got: {time}. Returning 0.");
                return 0;
            }

            return diff;
        }

        /// <summary>
        /// Gets seconds from now until this DateTime (UTC, for future times).
        /// </summary>
        public static double SecondsFromNowUtc(this DateTime time)
        {
            var now = DateTimeService.UtcNow;
            var diff = (time - now).TotalSeconds;

            if (diff < 0)
            {
                Debug.LogWarning($"[TimeExtensions] SecondsFromNowUtc called with past time. Expected future time, got: {time}. Returning 0.");
                return 0;
            }

            return diff;
        }

        /// <summary>
        /// Gets minutes from now until this DateTime (for future times).
        /// </summary>
        public static double MinutesFromNow(this DateTime time)
        {
            return SecondsFromNow(time) / SecondsPerMinute;
        }

        /// <summary>
        /// Gets minutes from now until this DateTime (UTC, for future times).
        /// </summary>
        public static double MinutesFromNowUtc(this DateTime time)
        {
            return SecondsFromNowUtc(time) / SecondsPerMinute;
        }

        /// <summary>
        /// Gets hours from now until this DateTime (for future times).
        /// </summary>
        public static double HoursFromNow(this DateTime time)
        {
            return SecondsFromNow(time) / SecondsPerHour;
        }

        /// <summary>
        /// Gets hours from now until this DateTime (UTC, for future times).
        /// </summary>
        public static double HoursFromNowUtc(this DateTime time)
        {
            return SecondsFromNowUtc(time) / SecondsPerHour;
        }

        /// <summary>
        /// Gets days from now until this DateTime (for future times).
        /// </summary>
        public static double DaysFromNow(this DateTime time)
        {
            return SecondsFromNow(time) / SecondsPerDay;
        }

        /// <summary>
        /// Gets days from now until this DateTime (UTC, for future times).
        /// </summary>
        public static double DaysFromNowUtc(this DateTime time)
        {
            return SecondsFromNowUtc(time) / SecondsPerDay;
        }

        #endregion

        #region DateTime Raw Time Calculations
        /// <summary>
        /// Gets absolute difference in seconds between the given DateTime and now.
        /// </summary>
        public static double AbsoluteSecondsDifference(this DateTime time)
        {
            var now = DateTimeService.Now;
            return Math.Abs((now - time).TotalSeconds);
        }

        /// <summary>
        /// Gets absolute difference in seconds between the given DateTime and now (UTC).
        /// </summary>
        public static double AbsoluteSecondsDifferenceUtc(this DateTime time)
        {
            var now = DateTimeService.UtcNow;
            return Math.Abs((now - time).TotalSeconds);
        }

        #endregion

        #region DateTime Manipulation

        /// <summary>
        /// Creates a new DateTime with modified date components.
        /// </summary>
        public static DateTime WithDate(this DateTime dt, int? year = null, int? month = null, int? day = null)
        {
            int newYear = year ?? dt.Year;
            int newMonth = month ?? dt.Month;
            int newDay = day ?? dt.Day;

            int daysInMonth = DateTime.DaysInMonth(newYear, newMonth);
            newDay = Math.Min(newDay, daysInMonth);

            return new DateTime(newYear, newMonth, newDay, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }

        /// <summary>
        /// Creates a new DateTime with modified time components.
        /// </summary>
        public static DateTime WithTime(this DateTime dt, int? hour = null, int? minute = null, int? second = null, int? millisecond = null)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day,
                hour ?? dt.Hour,
                minute ?? dt.Minute,
                second ?? dt.Second,
                millisecond ?? dt.Millisecond);
        }

        #endregion

        #region DateTime Period Checks

        /// <summary>
        /// Checks if the DateTime is at the start of the day (00:00:00).
        /// </summary>
        public static bool IsStartOfDay(this DateTime time)
        {
            return time.TimeOfDay == TimeSpan.Zero;
        }

        /// <summary>
        /// Checks if the DateTime is at the start of the week (Monday 00:00:00).
        /// </summary>
        public static bool IsStartOfWeek(this DateTime time)
        {
            return time.DayOfWeek == DayOfWeek.Monday && time.IsStartOfDay();
        }

        /// <summary>
        /// Checks if the DateTime is at the start of the month (1st day 00:00:00).
        /// </summary>
        public static bool IsStartOfMonth(this DateTime time)
        {
            return time.Day == 1 && time.IsStartOfDay();
        }

        /// <summary>
        /// Checks if this DateTime represents today.
        /// </summary>
        public static bool IsToday(this DateTime time)
        {
            return time.Date == DateTimeService.Today;
        }

        /// <summary>
        /// Checks if this DateTime represents today (UTC).
        /// </summary>
        public static bool IsTodayUtc(this DateTime time)
        {
            return time.Date == DateTimeService.TodayUtc;
        }

        #endregion

        #region DateTime Extensions for Periods

        /// <summary>
        /// Gets the next day from this DateTime.
        /// </summary>
        public static DateTime NextDay(this DateTime time)
        {
            return time.AddDays(1);
        }
        #endregion
    }
}
