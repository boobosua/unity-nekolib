using System;

namespace NekoLib.Services
{
    public static class TimeSystem
    {
        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        public static DateTime UtcNow()
        {
            return DateTimeManager.Instance.UtcNow();
        }

        /// <summary>
        /// Gets the current local time.
        /// </summary>
        public static DateTime Now()
        {
            return DateTimeManager.Instance.Now();
        }

        /// <summary>
        /// Gets the current UTC date.
        /// </summary>
        public static DateTime TodayUtc()
        {
            return DateTimeManager.Instance.TodayUtc();
        }

        /// <summary>
        /// Gets the current local date.
        /// </summary>
        public static DateTime Today()
        {
            return DateTimeManager.Instance.Today();
        }

        /// <summary>
        /// Checks if today is the start of the week.
        /// </summary>
        public static bool IsTodayStartOfWeek()
        {
            return DateTimeManager.Instance.IsTodayStartOfWeek();
        }

        /// <summary>
        /// Checks if today is the start of the week in UTC.
        /// </summary>
        public static bool IsTodayStartOfWeekUtc()
        {
            return DateTimeManager.Instance.IsTodayStartOfWeekUtc();
        }

        /// <summary>
        /// Checks if today is the start of the month.
        /// </summary>
        public static bool IsTodayStartOfMonth()
        {
            return DateTimeManager.Instance.IsTodayStartOfMonth();
        }

        /// <summary>
        /// Checks if today is the start of the month in UTC.
        /// </summary>
        public static bool IsTodayStartOfMonthUtc()
        {
            return DateTimeManager.Instance.IsTodayStartOfMonthUtc();
        }

        /// <summary>
        /// Gets the next day.
        /// </summary>
        public static DateTime NextDay()
        {
            return DateTimeManager.Instance.NextDay();
        }

        /// <summary>
        /// Gets the next day in UTC.
        /// </summary>
        public static DateTime NextDayUtc()
        {
            return DateTimeManager.Instance.NextDayUtc();
        }
    }
}
