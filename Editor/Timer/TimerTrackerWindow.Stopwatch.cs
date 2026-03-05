#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib.Timer
{
    public partial class TimerTrackerWindow
    {
        private void DrawStopwatchTable(PaginationEditor.Slice slice)
        {
            DrawStopwatchHeader();
            for (int i = slice.Start; i < slice.End; i++)
                DrawStopwatchRow(_stopwatches[i], i - slice.Start);
        }

        private static void DrawStopwatchHeader()
        {
            var rect = GUILayoutUtility.GetRect(GetStopwatchTableMinWidth(), HeaderHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, ColHeaderBg);
            DrawTableBorders(rect);

            var style = MakeHeaderStyle();
            float x = rect.x;

            DrawHeaderColumn(new Rect(x, rect.y, SwSlotW, rect.height), "Slot", style);
            x += SwSlotW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, SwVerW, rect.height), "Ver", style);
            x += SwVerW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, SwOwnerW, rect.height), "Owner", style);
            x += SwOwnerW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, SwElapsedW, rect.height), "Elapsed", style);
            x += SwElapsedW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, SwStatusW, rect.height), "Status", style);
        }

        private void DrawStopwatchRow(in TimerWorld.TimerDebugEntry entry, int index)
        {
            var rowRect = GUILayoutUtility.GetRect(GetStopwatchTableMinWidth(), RowHeight, GUILayout.ExpandWidth(true));

            if (index % 2 == 1)
                EditorGUI.DrawRect(rowRect, ColZebraStripe);
            DrawTableBorders(rowRect);

            var dimmed = DimmedText();
            var (statusLabel, statusColor) = GetStatus(entry.IsRunning, entry.IsPendingKill, entry.UseUnscaledTime);

            float x = rowRect.x;

            DrawCenteredText(new Rect(x, rowRect.y, SwSlotW, rowRect.height), entry.Slot.ToString(), dimmed, 10);
            x += SwSlotW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawCenteredText(new Rect(x, rowRect.y, SwVerW, rowRect.height), entry.Version.ToString(), dimmed, 10);
            x += SwVerW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawOwnerColumn(new Rect(x, rowRect.y, SwOwnerW, rowRect.height), entry.Owner, dimmed);
            x += SwOwnerW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawCenteredText(new Rect(x, rowRect.y, SwElapsedW, rowRect.height), FormatTimeExact(entry.StopwatchElapsed), dimmed, 10);
            x += SwElapsedW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawCenteredText(new Rect(x, rowRect.y, SwStatusW, rowRect.height), statusLabel, statusColor, 9);
        }
    }
}
#endif
