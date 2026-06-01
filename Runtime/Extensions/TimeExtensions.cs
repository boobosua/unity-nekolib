using System;
using System.Text;
using UnityEngine;

namespace TRnK.Extensions
{
    public static class TimeExtensions
    {
        private static readonly StringBuilder s_stringBuilder = new(32);

        #region Time to String Formatting

        /// <summary>Converts time in seconds to HH:MM:SS format.</summary>
        public static string ToClock(this float time, bool useCeiling = true)
        {
            var totalSeconds = useCeiling ? Mathf.CeilToInt(time) : Mathf.FloorToInt(time);
            return TimeSpan.FromSeconds(totalSeconds).ToString(@"hh\:mm\:ss");
        }

        /// <summary>Converts time in seconds to HH:MM:SS format.</summary>
        public static string ToClock(this double time, bool useCeiling = true)
        {
            var totalSeconds = useCeiling ? Math.Ceiling(time) : Math.Floor(time);
            return TimeSpan.FromSeconds(totalSeconds).ToString(@"hh\:mm\:ss");
        }

        /// <summary>Converts time in seconds to HH:MM:SS format.</summary>
        public static string ToClock(this int time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");
        }

        /// <summary>Converts time in seconds to MM:SS format.</summary>
        public static string ToShortClock(this float time, bool useCeiling = true)
        {
            var totalSeconds = useCeiling ? Mathf.CeilToInt(time) : Mathf.FloorToInt(time);
            return TimeSpan.FromSeconds(totalSeconds).ToString(@"mm\:ss");
        }

        /// <summary>Converts time in seconds to MM:SS format.</summary>
        public static string ToShortClock(this double time, bool useCeiling = true)
        {
            var totalSeconds = useCeiling ? Math.Ceiling(time) : Math.Floor(time);
            return TimeSpan.FromSeconds(totalSeconds).ToString(@"mm\:ss");
        }

        /// <summary>Converts time in seconds to MM:SS format.</summary>
        public static string ToShortClock(this int time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"mm\:ss");
        }

        /// <summary>Converts TimeSpan to readable format (e.g., "2d 3h 45m").</summary>
        public static string ToReadableFormat(this TimeSpan timeSpan, bool useSpacing = false)
        {
            s_stringBuilder.Clear();

            var totalDays = (int)timeSpan.TotalDays;
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            if (totalDays >= 1)
            {
                s_stringBuilder.Append(totalDays).Append('d');
                if (useSpacing) s_stringBuilder.Append(' ');
                s_stringBuilder.Append(hours).Append('h');
                if (useSpacing) s_stringBuilder.Append(' ');
                s_stringBuilder.Append(minutes).Append('m');
            }
            else if (timeSpan.TotalHours >= 1)
            {
                s_stringBuilder.Append(hours).Append('h');
                if (useSpacing) s_stringBuilder.Append(' ');
                s_stringBuilder.Append(minutes).Append('m');
                if (useSpacing) s_stringBuilder.Append(' ');
                s_stringBuilder.Append(seconds).Append('s');
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                s_stringBuilder.Append(minutes).Append('m');
                if (useSpacing) s_stringBuilder.Append(' ');
                s_stringBuilder.Append(seconds).Append('s');
            }
            else
            {
                s_stringBuilder.Append(seconds).Append('s');
            }

            return s_stringBuilder.ToString();
        }

        /// <summary>Converts seconds to readable format.</summary>
        public static string ToReadableFormat(this double seconds, bool useSpacing = true, bool useCeiling = true)
        {
            var totalSeconds = useCeiling ? Math.Ceiling(seconds) : Math.Floor(seconds);
            return TimeSpan.FromSeconds(totalSeconds).ToReadableFormat(useSpacing);
        }

        /// <summary>Converts seconds to readable format.</summary>
        public static string ToReadableFormat(this float seconds, bool useSpacing = true, bool useCeiling = true)
        {
            var totalSeconds = useCeiling ? Mathf.CeilToInt(seconds) : Mathf.FloorToInt(seconds);
            return TimeSpan.FromSeconds(totalSeconds).ToReadableFormat(useSpacing);
        }

        /// <summary>Converts seconds to readable format.</summary>
        public static string ToReadableFormat(this int seconds, bool useSpacing = true)
        {
            return TimeSpan.FromSeconds(seconds).ToReadableFormat(useSpacing);
        }

        #endregion

        #region DateTime Manipulation

        /// <summary>Returns a new DateTime with modified date components (day clamps to the new month's max).</summary>
        public static DateTime WithDate(this DateTime dt, int? year = null, int? month = null, int? day = null)
        {
            int newYear = year ?? dt.Year;
            int newMonth = month ?? dt.Month;
            int newDay = day ?? dt.Day;

            int daysInMonth = DateTime.DaysInMonth(newYear, newMonth);
            newDay = Math.Min(newDay, daysInMonth);

            return new DateTime(newYear, newMonth, newDay, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }

        /// <summary>Creates a new DateTime with modified time components.</summary>
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

        /// <summary>Checks if the DateTime is at the start of the day (00:00:00).</summary>
        public static bool IsStartOfDay(this DateTime time)
        {
            return time.TimeOfDay == TimeSpan.Zero;
        }

        /// <summary>Checks if the DateTime is at the start of the week (00:00:00 on the given first day; defaults to Monday).</summary>
        public static bool IsStartOfWeek(this DateTime time, DayOfWeek firstDay = DayOfWeek.Monday)
        {
            return time.DayOfWeek == firstDay && time.IsStartOfDay();
        }

        /// <summary>Checks if the DateTime is at the start of the month (1st day 00:00:00).</summary>
        public static bool IsStartOfMonth(this DateTime time)
        {
            return time.Day == 1 && time.IsStartOfDay();
        }

        #endregion
    }
}
