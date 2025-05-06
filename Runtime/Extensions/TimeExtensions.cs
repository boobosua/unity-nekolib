using System;

namespace NekoLib.Extensions
{
    public static class TimeExtensions
    {
        public static string ToClock(this float time)
        {
            return TimeSpan.FromSeconds(time).ToString("hh':'mm':'ss");
        }

        public static string ToClock(this double time)
        {
            return TimeSpan.FromSeconds(time).ToString("hh':'mm':'ss");
        }

        public static string ClockFromNow(this DateTime time)
        {
            return SecondsFromNow(time).ToClock();
        }

        public static double SecondsFromNow(this DateTime time)
        {
            return (DateTime.Now - time).TotalSeconds;
        }

        public static double MinutesFromNow(this DateTime time)
        {
            return (DateTime.Now - time).TotalMinutes;
        }

        public static DateTime WithDate(this DateTime dt, int? year = null, int? month = null, int? day = null)
        {
            int newYear = year ?? dt.Year;
            int newMonth = month ?? dt.Month;
            int newDay = day ?? dt.Day;

            // Ensure the new date is valid by clamping day if necessary
            int daysInMonth = DateTime.DaysInMonth(newYear, newMonth);
            newDay = Math.Min(newDay, daysInMonth);

            return new DateTime(newYear, newMonth, newDay, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }
    }
}

