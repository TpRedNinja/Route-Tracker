using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Route_Tracker
{
    public static class SortingManager
    {
        // ==========MY NOTES==============
        // Applies the specified sorting mode to the route grid
        // Handles all different sorting modes and updates the display
        public static void ApplySorting(DataGridView routeGrid, SortingOptionsForm.SortingMode sortingMode)
        {
            if (routeGrid?.FindForm() is not MainForm mainForm)
                return;

            LoadingHelper.ExecuteWithSpinner(mainForm, () =>
            {
                if (routeGrid == null || routeGrid.Rows.Count == 0)
                    return;

                try
                {
                    // Get all entries from the grid
                    List<RouteEntry> entries = [];
                    for (int i = 0; i < routeGrid.Rows.Count; i++)
                    {
                        if (routeGrid.Rows[i].Tag is RouteEntry entry)
                            entries.Add(entry);
                    }

                    if (entries.Count == 0)
                        return;

                    // Apply sorting based on mode
                    List<RouteEntry> sortedEntries = sortingMode switch
                    {
                        SortingOptionsForm.SortingMode.CompletedAtTop => SortCompletedAtTop(entries),
                        SortingOptionsForm.SortingMode.CompletedAtBottom => SortCompletedAtBottom(entries),
                        SortingOptionsForm.SortingMode.HideCompleted => SortHideCompleted(entries),
                        _ => SortCompletedAtTop(entries) // Default fallback
                    };

                    // Update the grid with sorted entries
                    UpdateGridWithSortedEntries(routeGrid, sortedEntries);

                    // Scroll to appropriate position
                    ScrollToAppropriatePosition(routeGrid, sortingMode);

                    Debug.WriteLine($"Applied sorting mode: {sortingMode}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error applying sorting: {ex.Message}");
                    MessageBox.Show($"Error applying sorting: {ex.Message}", "Sorting Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }, "Applying Sorting...");
        }

        // ==========MY NOTES==============
        // Sorts entries with completed ones at the top
        private static List<RouteEntry> SortCompletedAtTop(List<RouteEntry> entries)
        {
            return [.. entries
                .Where(e => !e.IsSkipped) // Don't show skipped entries
                .OrderByDescending(e => e.IsCompleted)
                .ThenBy(e => e.Id)];
        }

        // ==========MY NOTES==============
        // Sorts entries with completed ones at the bottom
        private static List<RouteEntry> SortCompletedAtBottom(List<RouteEntry> entries)
        {
            return [.. entries
                .Where(e => !e.IsSkipped) // Don't show skipped entries
                .OrderBy(e => e.IsCompleted)
                .ThenBy(e => e.Id)];
        }

        // ==========MY NOTES==============
        // Hides completed entries completely
        private static List<RouteEntry> SortHideCompleted(List<RouteEntry> entries)
        {
            return [.. entries
                .Where(e => !e.IsCompleted && !e.IsSkipped)
                .OrderBy(e => e.Id)];
        }

        // ==========MY NOTES==============
        // Updates the grid with the sorted entries
        private static void UpdateGridWithSortedEntries(DataGridView routeGrid, List<RouteEntry> sortedEntries)
        {
            routeGrid.Rows.Clear();
            
            foreach (var entry in sortedEntries)
            {
                string completionMark = entry.IsCompleted ? "X" : "";
                int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                routeGrid.Rows[rowIndex].Tag = entry;
            }
        }

        // ==========MY NOTES==============
        // Scrolls to the appropriate position based on sorting mode
        private static void ScrollToAppropriatePosition(DataGridView routeGrid, SortingOptionsForm.SortingMode sortingMode)
        {
            if (routeGrid.Rows.Count == 0)
                return;

            try
            {
                switch (sortingMode)
                {
                    case SortingOptionsForm.SortingMode.CompletedAtTop:
                        // Scroll to first incomplete entry
                        ScrollToFirstIncomplete(routeGrid);
                        break;
                    case SortingOptionsForm.SortingMode.CompletedAtBottom:
                        // Scroll to top (first incomplete entries)
                        routeGrid.FirstDisplayedScrollingRowIndex = 0;
                        if (routeGrid.Rows.Count > 0)
                        {
                            routeGrid.ClearSelection();
                            routeGrid.Rows[0].Selected = true;
                        }
                        break;
                    case SortingOptionsForm.SortingMode.HideCompleted:
                        // Scroll to top (all entries are incomplete)
                        routeGrid.FirstDisplayedScrollingRowIndex = 0;
                        if (routeGrid.Rows.Count > 0)
                        {
                            routeGrid.ClearSelection();
                            routeGrid.Rows[0].Selected = true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error scrolling to position: {ex.Message}");
            }
        }

        // ==========MY NOTES==============
        // Scrolls to the first incomplete entry (reused from RouteManager)
        public static void ScrollToFirstIncomplete(DataGridView routeGrid)
        {
            if (routeGrid == null || routeGrid.Rows.Count == 0)
                return;

            int firstIncompleteIndex = -1;
            for (int i = 0; i < routeGrid.Rows.Count; i++)
            {
                if (routeGrid.Rows[i].Tag is RouteEntry entry && !entry.IsCompleted)
                {
                    firstIncompleteIndex = i;
                    break;
                }
            }

            if (firstIncompleteIndex >= 0)
            {
                routeGrid.FirstDisplayedScrollingRowIndex = Math.Max(0, firstIncompleteIndex - 2);
                routeGrid.ClearSelection();
                routeGrid.Rows[firstIncompleteIndex].Selected = true;
            }
            else if (routeGrid.Rows.Count > 0)
            {
                routeGrid.FirstDisplayedScrollingRowIndex = 0;
            }
        }

        // ==========MY NOTES==============
        // Cycles through sorting modes in the specified direction
        public static SortingOptionsForm.SortingMode CycleSortingMode(SortingOptionsForm.SortingMode currentMode, bool forward)
        {
            var modes = Enum.GetValues<SortingOptionsForm.SortingMode>();
            int currentIndex = Array.IndexOf(modes, currentMode);

            if (forward)
            {
                currentIndex = (currentIndex + 1) % modes.Length;
            }
            else
            {
                currentIndex = (currentIndex - 1 + modes.Length) % modes.Length;
            }

            return modes[currentIndex];
        }

        // ==========MY NOTES==============
        // Gets a user-friendly name for the sorting mode
        public static string GetSortingModeName(SortingOptionsForm.SortingMode mode)
        {
            return mode switch
            {
                SortingOptionsForm.SortingMode.CompletedAtTop => "Completed at Top",
                SortingOptionsForm.SortingMode.CompletedAtBottom => "Completed at Bottom",
                SortingOptionsForm.SortingMode.HideCompleted => "Hide Completed",
                _ => "Unknown"
            };
        }
    }
}