using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using NekoLib.Singleton;
using NekoLib.Extensions;
using NekoLib.ColorPalette;

namespace NekoLib.Networking
{
    public class WorldTimeAPI : LazySingleton<WorldTimeAPI>
    {
        [Serializable]
        private struct TimeData
        {
            // The name datetime must match the param from the JSON.
            public string datetime;
        }

        private const string API_URL = "https://worldtimeapi.org/api/ip";

        public bool IsTimeLoaded { get; private set; } = false;

        private DateTime _currentDateTime = DateTime.Now;

        public void InitWorldTimeAPI(Action<DateTime, bool> onComplete)
        {
            StartCoroutine(GetRealDateTimeFromAPI(onComplete));
        }

        public DateTime GetCurrentDateTime()
        {
            return _currentDateTime.AddSeconds(Time.realtimeSinceStartup);
        }

        private IEnumerator GetRealDateTimeFromAPI(Action<DateTime, bool> onComplete)
        {
            var webRequest = UnityWebRequest.Get(API_URL);

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                _currentDateTime = DateTime.Now;
                onComplete?.Invoke(_currentDateTime, false);

                Debug.LogWarningFormat(webRequest.error.Colorize(Palette.VibrantRed));
            }
            else
            {
                // Success.
                var timeData = webRequest.downloadHandler.text.Deserialize<TimeData>();
                // timeData.DateTime value is: 2024-11-19T13:48:59.810136+07:00.

                _currentDateTime = ParseDateTime(timeData.datetime);
                IsTimeLoaded = true;

                onComplete?.Invoke(_currentDateTime, true);

                Debug.Log($"Success getting time from the WorldAPI".Colorize(Palette.PumpkinOrange));
            }
        }

        private DateTime ParseDateTime(string dateTime)
        {
            var date = Regex.Match(dateTime, @"^\d{4}-\d{2}-\d{2}").Value;
            var time = Regex.Match(dateTime, @"\d{2}:\d{2}:\d{2}").Value;

            return DateTime.Parse(string.Format("{0} {1}", date, time));
        }
    }
}
