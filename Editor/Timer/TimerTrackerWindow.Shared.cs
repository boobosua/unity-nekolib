#if UNITY_EDITOR
using System.Globalization;
using NekoLib.Extensions;
using UnityEditor;
using UnityEngine;

namespace NekoLib.Timer
{
    public partial class TimerTrackerWindow
    {
        // ─── Layout constants ────────────────────────────────────────────────────────
        private const int ItemsPerPage = 20;
        private const float RowHeight = 30f;
        private const float HeaderHeight = 26f;
        private const float SmoothSpeed = 5f;

        // Countdown column widths
        private const float CdSlotW = 50f;
        private const float CdVerW = 50f;
        private const float CdOwnerW = 150f;
        private const float CdTotalW = 90f;
        private const float CdRemainingW = 90f;
        private const float CdLoopW = 80f;
        private const float CdProgressW = 210f;
        private const float CdStatusW = 140f;

        // Stopwatch column widths
        private const float SwSlotW = 50f;
        private const float SwVerW = 50f;
        private const float SwOwnerW = 150f;
        private const float SwElapsedW = 110f;
        private const float SwStatusW = 190f;

        // ─── Colors ──────────────────────────────────────────────────────────────────
        private static readonly Color ColRunningBg = new(0.2f, 0.6f, 0.9f, 0.30f);
        private static readonly Color ColPausedBg = new(0.9f, 0.8f, 0.2f, 0.20f);
        private static readonly Color ColZebraStripe = new(0f, 0f, 0f, 0.05f);
        private static readonly Color ColHeaderBg = new(0.2f, 0.2f, 0.2f, 0.40f);
        private static readonly Color ColBorder = new(0.5f, 0.5f, 0.5f, 0.30f);
        private static readonly Color ColRunningText = new(0.1f, 0.4f, 0.8f, 1f);

        // ─── Stats bar ───────────────────────────────────────────────────────────────
        private void DrawStatsBar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            try
            {
                var textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

                DrawStatCard("IsAlive", TimerWorld.EditorActiveCount, textColor, new Color(ColRunningText.r, ColRunningText.g, ColRunningText.b, 0.15f));
                DrawStatCard("Capacity", TimerWorld.EditorCapacity, textColor, ColZebraStripe);
                DrawStatCard("Free Slots", TimerWorld.EditorFreeCount, textColor, ColZebraStripe);
                DrawStatCard("Slot Memory", FormatBytes(TimerWorld.EditorSlotMemoryBytes), textColor, ColZebraStripe);
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawStatCard(string label, int count, Color textColor, Color bgColor)
            => DrawStatCard(label, count.ToString(), textColor, bgColor);

        private void DrawStatCard(string label, string value, Color textColor, Color bgColor)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(54));
            var cardRect = EditorGUILayout.GetControlRect(false, 50);
            EditorGUI.DrawRect(cardRect, bgColor);
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, cardRect.width, 1), ColBorder);
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.yMax - 1, cardRect.width, 1), ColBorder);
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, 1, cardRect.height), ColBorder);
            EditorGUI.DrawRect(new Rect(cardRect.xMax - 1, cardRect.y, 1, cardRect.height), ColBorder);

            var countStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };
            countStyle.normal.textColor = textColor;
            GUI.Label(new Rect(cardRect.x, cardRect.y + 10, cardRect.width, 18), value, countStyle);

            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };
            labelStyle.normal.textColor = Color.Lerp(textColor, Color.gray, 0.3f);
            GUI.Label(new Rect(cardRect.x, cardRect.y + 28, cardRect.width, 14), label, labelStyle);

            EditorGUILayout.EndVertical();
        }

        // ─── Table drawing primitives ────────────────────────────────────────────────
        private static float GetCountdownTableMinWidth() =>
            CdSlotW + CdVerW + CdOwnerW + CdTotalW + CdRemainingW + CdLoopW + CdProgressW + CdStatusW;

        private static float GetStopwatchTableMinWidth() =>
            SwSlotW + SwVerW + SwOwnerW + SwElapsedW + SwStatusW;

        private static void DrawTableBorders(Rect rect)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), ColBorder);
        }

        private static void DrawVerticalDivider(float x, float y, float height)
        {
            EditorGUI.DrawRect(new Rect(x - 1, y, 1, height), ColBorder);
        }

        private static void DrawHeaderColumn(Rect rect, string text, GUIStyle style)
        {
            GUI.Label(rect, text, style);
        }

        private static void DrawCenteredText(Rect rect, string text, Color textColor, int fontSize)
        {
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };
            style.normal.textColor = textColor;
            GUI.Label(new Rect(rect.x, rect.y - 1, rect.width, rect.height + 2), text, style);
        }

        private static void DrawOwnerColumn(Rect rect, Object owner, Color textColor)
        {
            var inner = new Rect(rect.x + 8, rect.y, rect.width - 16, rect.height);
            var mb = owner as MonoBehaviour;
            var go = mb != null ? mb.gameObject : null;

            string goName = go != null ? go.name : (owner != null ? owner.name : "NULL");
            string compName = mb != null ? mb.GetType().Name : "—";

            var goStyle = new GUIStyle(EditorStyles.linkLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
            };
            goStyle.normal.textColor = textColor;

            var goRect = new Rect(inner.x, inner.y + 3, inner.width, 14);
            if (go != null)
            {
                if (GUI.Button(goRect, goName, goStyle))
                {
                    EditorGUIUtility.PingObject(go);
                    Selection.activeGameObject = go;
                }
            }
            else
            {
                GUI.Label(goRect, goName, goStyle);
            }

            var compStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Italic,
            };
            compStyle.normal.textColor = Color.Lerp(textColor, Color.gray, 0.4f);
            GUI.Label(new Rect(inner.x, inner.y + 17, inner.width, 11), compName, compStyle);
        }

        private float GetSmoothFill(int slot, int version, float targetFill, bool isRunning)
        {
            var key = SmoothKey(slot, version);
            if (!_smoothProgress.TryGetValue(key, out float current))
                current = targetFill;

            float result = isRunning
                ? Mathf.Lerp(current, targetFill, Time.unscaledDeltaTime * SmoothSpeed)
                : targetFill;

            _smoothProgress[key] = result;
            return result;
        }

        private void DrawProgressColumn(Rect rect, int slot, int version, float fill, bool isRunning, bool isPaused)
        {
            var bar = new Rect(rect.x + 6, rect.y + 4, rect.width - 12, 16);
            float displayFill = GetSmoothFill(slot, version, fill, isRunning);

            var bgColor = EditorGUIUtility.isProSkin
                ? new Color(0.2f, 0.2f, 0.2f, 0.8f)
                : new Color(0.8f, 0.8f, 0.8f, 0.8f);
            EditorGUI.DrawRect(bar, bgColor);

            if (displayFill > 0f)
            {
                var fillRect = new Rect(bar.x, bar.y, bar.width * displayFill, bar.height);
                var fillColor = isPaused ? ColPausedBg : isRunning ? ColRunningBg : Color.gray;
                EditorGUI.DrawRect(fillRect, fillColor);
            }

            var textStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter,
            };
            textStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            GUI.Label(new Rect(bar.x, bar.y - 1, bar.width, bar.height + 2), fill.AsPercent(), textStyle);
        }

        // ─── Formatting helpers ──────────────────────────────────────────────────────
        private static string FormatBytes(int bytes)
        {
            if (bytes >= 1024 * 1024) return $"{bytes / (1024f * 1024f):0.#} MB";
            if (bytes >= 1024) return $"{bytes / 1024f:0.#} KB";
            return $"{bytes} B";
        }

        private static string FormatTime(float t)
        {
            float rounded = Mathf.Round(t / 0.25f) * 0.25f;
            return rounded.ToString("0.##", CultureInfo.InvariantCulture) + "s";
        }

        private static string FormatTimeExact(float t)
        {
            return Mathf.Max(0f, t).ToString("0.00", CultureInfo.InvariantCulture) + "s";
        }

        private static string FormatLoops(int iteration, int loopCount)
        {
            if (loopCount == 0) return "—";
            string total = loopCount == -1 ? "∞" : loopCount.ToString();
            return $"{iteration + 1} / {total}";
        }

        private static (string label, Color color) GetStatus(bool isRunning, bool isPendingKill, bool useUnscaledTime)
        {
            var col = DimmedText();
            if (isPendingKill) return ("KILLING", col);
            if (!isRunning) return ("PAUSED", col);
            string label = useUnscaledTime ? "RUNNING | UNSCALED" : "RUNNING";
            return (label, col);
        }

        private static GUIStyle MakeHeaderStyle()
        {
            var s = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
            };
            s.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            return s;
        }

        private static Color DimmedText()
        {
            var baseColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            return Color.Lerp(baseColor, Color.gray, 0.3f);
        }

        private static long SmoothKey(int slot, int version) => ((long)version << 32) | (uint)slot;
    }
}
#endif
