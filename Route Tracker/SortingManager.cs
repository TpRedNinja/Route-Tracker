using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // Manages different sorting modes for the route grid
    // Provides methods to sort, filter, and display route entries
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
                ApplySortingInternal(routeGrid, sortingMode);
            }, "Applying Sorting...");
        }

        // ==========MY NOTES==============
        // Applies sorting without showing loading spinner (for automatic updates)
        // Prevents multiple spinners and scroll jumping during real-time updates
        public static void ApplySortingQuiet(DataGridView routeGrid, SortingOptionsForm.SortingMode sortingMode)
        {
            ApplySortingInternal(routeGrid, sortingMode);
        }

        // ==========MY NOTES==============
        // Internal sorting logic that does the actual work
        // Used by both ApplySorting and ApplySortingQuiet
        private static void ApplySortingInternal(DataGridView routeGrid, SortingOptionsForm.SortingMode sortingMode)
        {
            if (routeGrid == null || routeGrid.RowCount == 0)
                return;

            try
            {
                // Store current scroll position to minimize jumping
                int currentScrollIndex = routeGrid.FirstDisplayedScrollingRowIndex;
                int selectedRowIndex = routeGrid.CurrentRow?.Index ?? -1;

                // Suspend layout to prevent flickering and scroll jumping
                routeGrid.SuspendLayout();

                try
                {
                    List<RouteEntry> entries = [];

                    if (routeGrid.VirtualMode)
                    {
                        // For virtual mode, get entries from the route manager
                        if (routeGrid.FindForm() is MainForm mainForm && mainForm.GetRouteManager() is RouteManager routeManager)
                        {
                            entries = routeManager.GetRouteEntries();
                        }
                    }
                    else
                    {
                        // For traditional mode, get entries from grid rows
                        for (int i = 0; i < routeGrid.Rows.Count; i++)
                        {
                            if (routeGrid.Rows[i].Tag is RouteEntry entry)
                                entries.Add(entry);
                        }
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

                    // Only scroll automatically for major operations, not during updates
                    if (sortingMode != SortingOptionsForm.SortingMode.CompletedAtTop)
                    {
                        ScrollToAppropriatePosition(routeGrid, sortingMode);
                    }
                    else
                    {
                        // For completed at top, just scroll to first incomplete without selection change
                        ScrollToFirstIncompleteQuiet(routeGrid);
                    }

                    Debug.WriteLine($"Applied sorting mode: {sortingMode}");
                }
                finally
                {
                    routeGrid.ResumeLayout(false); // Don't force layout immediately
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying sorting: {ex.Message}");
                MessageBox.Show($"Error applying sorting: {ex.Message}", "Sorting Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
        // Updates the grid with sorted entries using virtual mode for optimal performance
        private static void UpdateGridWithSortedEntries(DataGridView routeGrid, List<RouteEntry> sortedEntries)
        {
            if (routeGrid.VirtualMode)
            {
                // For virtual mode, update the underlying data source
                if (routeGrid.FindForm() is MainForm mainForm && mainForm.GetRouteManager() is RouteManager routeManager)
                {
                    // Update the route manager's internal entries
                    routeManager.UpdateRouteEntries(sortedEntries);

                    // Update virtual grid display
                    routeGrid.RowCount = sortedEntries.Count;
                    routeGrid.Invalidate();
                }
            }
            else
            {
                // Traditional mode fallback
                routeGrid.Rows.Clear();
                foreach (var entry in sortedEntries)
                {
                    string completionMark = entry.IsCompleted ? "X" : "";
                    int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                    routeGrid.Rows[rowIndex].Tag = entry;
                }
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
                        if (routeGrid.VirtualMode)
                        {
                            routeGrid.FirstDisplayedScrollingRowIndex = 0;
                        }
                        else if (routeGrid.Rows.Count > 0)
                        {
                            routeGrid.FirstDisplayedScrollingRowIndex = 0;
                            routeGrid.ClearSelection();
                            routeGrid.Rows[0].Selected = true;
                        }
                        break;
                    case SortingOptionsForm.SortingMode.HideCompleted:
                        // Scroll to top (all entries are incomplete)
                        if (routeGrid.VirtualMode)
                        {
                            routeGrid.FirstDisplayedScrollingRowIndex = 0;
                        }
                        else if (routeGrid.Rows.Count > 0)
                        {
                            routeGrid.FirstDisplayedScrollingRowIndex = 0;
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
            if (routeGrid == null || routeGrid.RowCount == 0)
                return;

            if (routeGrid.VirtualMode)
            {
                // For virtual mode, need to access the underlying data
                if (routeGrid.FindForm() is MainForm mainForm && mainForm.GetRouteManager() is RouteManager routeManager)
                {
                    var entries = routeManager.GetRouteEntries();
                    int firstIncompleteIndex = -1;

                    for (int i = 0; i < entries.Count; i++)
                    {
                        if (!entries[i].IsCompleted)
                        {
                            firstIncompleteIndex = i;
                            break;
                        }
                    }

                    if (firstIncompleteIndex >= 0)
                    {
                        routeGrid.FirstDisplayedScrollingRowIndex = Math.Max(0, firstIncompleteIndex - 2);
                    }
                    else
                    {
                        routeGrid.FirstDisplayedScrollingRowIndex = 0;
                    }
                }
            }
            else
            {
                // Traditional mode
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
        }

        // ==========MY NOTES==============
        // Scrolls to first incomplete entry without changing selection (quieter version)
        // Used during automatic updates to minimize UI disruption
        private static void ScrollToFirstIncompleteQuiet(DataGridView routeGrid)
        {
            if (routeGrid == null || routeGrid.RowCount == 0)
                return;

            if (routeGrid.VirtualMode)
            {
                // For virtual mode, need to access the underlying data
                if (routeGrid.FindForm() is MainForm mainForm && mainForm.GetRouteManager() is RouteManager routeManager)
                {
                    var entries = routeManager.GetRouteEntries();
                    int firstIncompleteIndex = -1;

                    for (int i = 0; i < entries.Count; i++)
                    {
                        if (!entries[i].IsCompleted)
                        {
                            firstIncompleteIndex = i;
                            break;
                        }
                    }

                    if (firstIncompleteIndex >= 0)
                    {
                        // Only scroll if we're not already close to the target
                        int currentScroll = routeGrid.FirstDisplayedScrollingRowIndex;
                        int targetScroll = Math.Max(0, firstIncompleteIndex - 2);

                        if (Math.Abs(currentScroll - targetScroll) > 3) // Only scroll if significant difference
                        {
                            routeGrid.FirstDisplayedScrollingRowIndex = targetScroll;
                        }
                    }
                }
            }
            else
            {
                // Traditional mode
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
                    // Only scroll if we're not already close to the target
                    int currentScroll = routeGrid.FirstDisplayedScrollingRowIndex;
                    int targetScroll = Math.Max(0, firstIncompleteIndex - 2);

                    if (Math.Abs(currentScroll - targetScroll) > 3) // Only scroll if significant difference
                    {
                        routeGrid.FirstDisplayedScrollingRowIndex = targetScroll;
                    }
                }
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