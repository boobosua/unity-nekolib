using TMPro;

namespace NekoLib.Extensions
{
    public static class TMPTextExtensions
    {
        private const int HoursPerDay = 24;
        private const int MinutesPerHour = 60;

        /// <summary>Sets text to "HH:MM:SS" from seconds.</summary>
        public static void SetClockHHMMSS(this TMP_Text text, int totalSeconds)
        {
            if (text == null) return;

            if (totalSeconds < 0) totalSeconds = 0;

            int totalMinutes = totalSeconds / MinutesPerHour;
            int seconds = totalSeconds - (totalMinutes * MinutesPerHour);

            int totalHours = totalMinutes / MinutesPerHour;
            int minutes = totalMinutes - (totalHours * MinutesPerHour);

            int hours = totalHours % HoursPerDay;

            text.SetText("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }

        /// <summary>Sets text to "HH:MM:SS" from seconds.</summary>
        public static void SetClockHHMMSS(this TMP_Text text, float totalSeconds)
        {
            SetClockHHMMSS(text, (int)totalSeconds);
        }

        /// <summary>Sets text to readable duration like "2d 3h 45m" (spacing optional).</summary>
        public static void SetReadableTime(this TMP_Text text, int totalSeconds, bool useSpacing = true)
        {
            if (text == null) return;

            if (totalSeconds < 0) totalSeconds = 0;

            int totalMinutes = totalSeconds / MinutesPerHour;
            int seconds = totalSeconds - (totalMinutes * MinutesPerHour);

            int totalHours = totalMinutes / MinutesPerHour;
            int minutes = totalMinutes - (totalHours * MinutesPerHour);

            int totalDays = totalHours / HoursPerDay;
            int hours = totalHours - (totalDays * HoursPerDay);

            if (totalDays >= 1)
            {
                text.SetText(useSpacing ? "{0}d {1}h {2}m" : "{0}d{1}h{2}m", totalDays, hours, minutes);
                return;
            }

            if (totalHours >= 1)
            {
                text.SetText(useSpacing ? "{0}h {1}m {2}s" : "{0}h{1}m{2}s", hours, minutes, seconds);
                return;
            }

            if (totalMinutes >= 1)
            {
                text.SetText(useSpacing ? "{0}m {1}s" : "{0}m{1}s", minutes, seconds);
                return;
            }

            text.SetText("{0}s", seconds);
        }

        /// <summary>Sets text to readable duration like "2d 3h 45m" (spacing optional).</summary>
        public static void SetReadableTime(this TMP_Text text, float totalSeconds, bool useSpacing = true)
        {
            SetReadableTime(text, (int)totalSeconds, useSpacing);
        }
    }
}
