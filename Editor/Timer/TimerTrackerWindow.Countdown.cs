#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib.Timer
{
    public partial class TimerTrackerWindow
    {
        private void DrawCountdownTable(PaginationEditor.Slice slice)
        {
            DrawCountdownHeader();
            for (int i = slice.Start; i < slice.End; i++)
                DrawCountdownRow(_countdowns[i], i - slice.Start);
        }

        private static void DrawCountdownHeader()
        {
            var rect = GUILayoutUtility.GetRect(GetCountdownTableMinWidth(), HeaderHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, ColHeaderBg);
            DrawTableBorders(rect);

            var style = MakeHeaderStyle();
            float x = rect.x;

            DrawHeaderColumn(new Rect(x, rect.y, CdSlotW, rect.height), "Slot", style);
            x += CdSlotW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, CdVerW, rect.height), "Ver", style);
            x += CdVerW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, CdOwnerW, rect.height), "Owner", style);
            x += CdOwnerW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, CdTotalW, rect.height), "Total", style);
            x += CdTotalW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, CdRemainingW, rect.height), "Remaining", style);
            x += CdRemainingW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, CdLoopW, rect.height), "Loops", style);
            x += CdLoopW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, CdProgressW, rect.height), "Progress", style);
            x += CdProgressW;
            DrawVerticalDivider(x, rect.y, rect.height);

            DrawHeaderColumn(new Rect(x, rect.y, CdStatusW, rect.height), "Status", style);
        }

        private void DrawCountdownRow(in TimerWorld.TimerDebugEntry entry, int index)
        {
            var rowRect = GUILayoutUtility.GetRect(GetCountdownTableMinWidth(), RowHeight, GUILayout.ExpandWidth(true));

            if (index % 2 == 1)
                EditorGUI.DrawRect(rowRect, ColZebraStripe);
            DrawTableBorders(rowRect);

            float total = entry.CountdownTotal;
            float remaining = entry.CountdownRemaining;
            float fill = total <= 0f ? 0f : Mathf.Clamp01(remaining / total); // 1 = full, 0 = empty

            bool isPaused = !entry.IsRunning && !entry.IsPendingKill;
            var dimmed = DimmedText();
            var (statusLabel, statusColor) = GetStatus(entry.IsRunning, entry.IsPendingKill, entry.UseUnscaledTime);

            float x = rowRect.x;

            DrawCenteredText(new Rect(x, rowRect.y, CdSlotW, rowRect.height), entry.Slot.ToString(), dimmed, 10);
            x += CdSlotW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawCenteredText(new Rect(x, rowRect.y, CdVerW, rowRect.height), entry.Version.ToString(), dimmed, 10);
            x += CdVerW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawOwnerColumn(new Rect(x, rowRect.y, CdOwnerW, rowRect.height), entry.Owner, dimmed);
            x += CdOwnerW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawCenteredText(new Rect(x, rowRect.y, CdTotalW, rowRect.height), FormatTime(total), dimmed, 10);
            x += CdTotalW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawCenteredText(new Rect(x, rowRect.y, CdRemainingW, rowRect.height), FormatTimeExact(remaining), dimmed, 10);
            x += CdRemainingW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawCenteredText(new Rect(x, rowRect.y, CdLoopW, rowRect.height), FormatLoops(entry.LoopIteration, entry.LoopCount), dimmed, 10);
            x += CdLoopW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawProgressColumn(new Rect(x, rowRect.y, CdProgressW, rowRect.height), entry.Slot, entry.Version, fill, entry.IsRunning, isPaused);
            x += CdProgressW;
            DrawVerticalDivider(x, rowRect.y, rowRect.height);

            DrawCenteredText(new Rect(x, rowRect.y, CdStatusW, rowRect.height), statusLabel, statusColor, 9);
        }
    }
}
#endif
