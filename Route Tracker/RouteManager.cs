using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace Route_Tracker
{
    public class RouteManager
    {
        private readonly string routeFilePath;
        private readonly GameConnectionManager gameConnectionManager;
        private List<RouteEntry> routeEntries = [];

        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        // last know folder path for Routes folder to avoid repeated searches
        private static string? lastFoundRoutesFolder;

        // Add these fields to the RouteManager class
        private bool isAt100Percent = false;
        private System.Threading.Timer? completionCheckTimer = null;
        private bool wasPreviouslyInNonGameplay = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public RouteManager(string routeFilePath, GameConnectionManager gameConnectionManager)
        {
            this.routeFilePath = routeFilePath;
            this.gameConnectionManager = gameConnectionManager;
        }

        #region Route Loading
        // Reads the route file and creates route entries from it
        // Simple wrapper around RouteLoader that handles the actual parsing
        // Returns whatever entries it finds or an empty list if there's a problem
        public List<RouteEntry> LoadEntries()
        {
            RouteLoader routeLoader = new();
            if (File.Exists(routeFilePath))
            {
                string filename = Path.GetFileName(routeFilePath);
                routeEntries = routeLoader.LoadRoute(filename);

                // Assign IDs and set up prerequisites
                for (int i = 0; i < routeEntries.Count; i++)
                {
                    routeEntries[i].Id = i + 1;
                    routeEntries[i].Prerequisite = i > 0 ? routeEntries[i - 1] : null;
                }

                LoggingSystem.LogInfo($"LoadEntries: Successfully loaded {routeEntries.Count} entries from {filename}");
            }
            else
            {
                LoggingSystem.LogError($"LoadEntries: Route file not found: {routeFilePath}");
            }
            return routeEntries;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:",
        Justification = "Required for interface compatibility")]
        public void LoadRouteDataIntoGrid(DataGridView routeGrid, string gameName, MainForm mainForm)
        {
            try
            {
                // First try to find any Routes folder
                string? routesFolder = FindRoutesFolderAnywhere();
                if (routesFolder == null)
                {
                    return; // Let UI layer handle error display
                }

                // Find any TSV file that might be a route file
                string[] tsvFiles = Directory.GetFiles(routesFolder, "*.tsv");
                if (tsvFiles.Length == 0)
                {
                    return; // Let UI layer handle error display
                }

                // Pick the first file that might be relevant
                string? routeFile = null;
                foreach (var file in tsvFiles)
                {
                    string filename = Path.GetFileName(file).ToLower();
                    // Try to find a file that seems related to AC4 routes
                    if (filename.Contains("ac4") || filename.Contains("assassin") ||
                        filename.Contains("route") || filename.Contains("main"))
                    {
                        routeFile = file;
                        break;
                    }
                }

                // If no specific match, just take the first TSV file
                routeFile ??= tsvFiles[0];

                // Load the route entries
                List<RouteEntry> entries = LoadRouteFromPath(routeFile);
                if (entries.Count == 0)
                {
                    return; // Let UI layer handle error display
                }

                // Store the entries for future reference
                routeEntries = entries;

                // Set up virtual mode grid for optimal performance
                SetupVirtualModeGrid(routeGrid, entries);

                // apply current sorting mode
                mainForm.ApplyCurrentSorting();

            }
            catch (Exception ex)
            {
                LoggingSystem.LogError($"Error loading route data: {ex.Message}", ex);
                // Let UI layer handle error display
            }
        }

        // Reads the actual TSV file and creates route entries from each line
        // Skips blank lines or lines that don't have all required parts
        // Returns a list of all valid route entries from the file
        private static List<RouteEntry> LoadRouteFromPath(string fullPath)
        {
            List<RouteEntry> entries = [];

            foreach (string line in File.ReadAllLines(fullPath))
            {
                // Skip blank lines or lines with insufficient data
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('\t');
                if (parts.Length >= 6)
                {
                    string displayText = parts[0].Trim();

                    // Skip entries with blank display text
                    if (string.IsNullOrWhiteSpace(displayText))
                        continue;

                    string conditionType = parts[1].Trim();

                    bool conditionParsed = int.TryParse(parts[2].Trim(), out int conditionValue);
                    if (!conditionParsed)
                    {
                        LoggingSystem.LogWarning($"Invalid condition value in route entry: {line}");
                        continue;
                    }

                    string coordinates = parts[3].Trim();
                    string location = parts[4].Trim();

                    bool locationConditionParsed = int.TryParse(parts[5].Trim(), out int locationCondition);
                    if (!locationConditionParsed)
                    {
                        LoggingSystem.LogWarning($"Invalid location condition value in route entry: {line}");
                        continue;
                    }

                    RouteEntry entry = new(displayText, conditionType, conditionValue, location, locationCondition);

                    // Add coordinates from the fourth column if available
                    if (!string.IsNullOrWhiteSpace(coordinates))
                    {
                        entry.Coordinates = coordinates;
                    }

                    entries.Add(entry);
                }
            }

            return entries;
        }

        // Sets up the route grid for virtual mode operation with optimized performance
        // Virtual mode only creates visible rows, dramatically improving performance for large datasets
        public void SetupVirtualModeGrid(DataGridView routeGrid, List<RouteEntry> entries)
        {
            try
            {
                // Suspend all layout operations during setup
                routeGrid.SuspendLayout();

                // Enable virtual mode for optimal performance with large datasets
                routeGrid.VirtualMode = true;
                routeGrid.RowCount = entries.Count;

                // Set up event handler for virtual mode data requests
                routeGrid.CellValueNeeded -= RouteGrid_CellValueNeeded; // Remove existing handler
                routeGrid.CellValueNeeded += RouteGrid_CellValueNeeded;

                // Store entries for virtual mode access
                routeEntries = entries;

                Debug.WriteLine($"Virtual mode grid setup complete with {entries.Count} entries");
            }
            finally
            {
                routeGrid.ResumeLayout(true);
            }
        }

        // ==========MY NOTES==============
        // Virtual mode event handler that supplies cell data on-demand
        // Only called for visible cells, dramatically reducing memory usage and load time
        private void RouteGrid_CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= routeEntries.Count)
                return;

            var entry = routeEntries[e.RowIndex];

            if (e.ColumnIndex == 0) // Display text column
            {
                e.Value = entry.DisplayText;
            }
            else if (e.ColumnIndex == 1) // Completion column
            {
                e.Value = entry.IsCompleted ? "X" : "";
            }
        }

        // Searches everywhere on your PC for a Routes folder
        // Tries common places first, then does deeper searches if needed
        // Remembers where it found the folder to be faster next time
        private static string? FindRoutesFolderAnywhere()
        {
            // First check the persistent setting
            string? savedPath = Route_Tracker.Properties.Settings.Default.RoutesFolderPath;
            if (!string.IsNullOrEmpty(savedPath) && Directory.Exists(savedPath))
            {
                lastFoundRoutesFolder = savedPath;
                return savedPath;
            }

            // Places to check in order of likelihood
            List<string> possibleLocations =
            [
                AppDomain.CurrentDomain.BaseDirectory,
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."),
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..")
            ];

            // First check direct Routes folders in common locations
            foreach (var location in possibleLocations)
            {
                try
                {
                    string potentialPath = Path.Combine(location, "Routes");
                    if (Directory.Exists(potentialPath))
                    {
                        lastFoundRoutesFolder = potentialPath;
                        Route_Tracker.Properties.Settings.Default.RoutesFolderPath = potentialPath;
                        Route_Tracker.Properties.Settings.Default.Save();
                        return potentialPath;
                    }
                }
                catch { /* Skip any locations we can't access */ }
            }

            // Next, try to find a Routes folder in any children of common locations
            foreach (var location in possibleLocations)
            {
                try
                {
                    if (!Directory.Exists(location))
                        continue;

                    foreach (var dir in Directory.GetDirectories(location))
                    {
                        try
                        {
                            string potentialPath = Path.Combine(dir, "Routes");
                            if (Directory.Exists(potentialPath))
                            {
                                lastFoundRoutesFolder = potentialPath;
                                Route_Tracker.Properties.Settings.Default.RoutesFolderPath = potentialPath;
                                Route_Tracker.Properties.Settings.Default.Save();
                                return potentialPath;
                            }

                            foreach (var subdir in Directory.GetDirectories(dir))
                            {
                                try
                                {
                                    string subPath = Path.Combine(subdir, "Routes");
                                    if (Directory.Exists(subPath))
                                    {
                                        lastFoundRoutesFolder = subPath;
                                        Route_Tracker.Properties.Settings.Default.RoutesFolderPath = subPath;
                                        Route_Tracker.Properties.Settings.Default.Save();
                                        return subPath;
                                    }
                                }
                                catch { /* Skip any we can't access */ }
                            }
                        }
                        catch { /* Skip any we can't access */ }
                    }
                }
                catch { /* Skip any locations we can't access */ }
            }

            // Last resort: check if any folder named "Routes" exists in user profile recursively
            try
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string? foundPath = FindRoutesFolderRecursive(userProfile, 3);
                if (foundPath != null)
                {
                    lastFoundRoutesFolder = foundPath;
                    Route_Tracker.Properties.Settings.Default.RoutesFolderPath = foundPath;
                    Route_Tracker.Properties.Settings.Default.Save();
                    return foundPath;
                }
            }
            catch { /* Skip if we can't access */ }

            return null; // No Routes folder found anywhere
        }

        // Digs through folders looking for a Routes folder
        // Doesn't go too deep to avoid taking forever
        // Skips Windows and Program Files to avoid permission errors
        private static string? FindRoutesFolderRecursive(string startPath, int maxDepth)
        {
            if (maxDepth <= 0) return null;

            try
            {
                // Check this directory itself
                string potentialPath = Path.Combine(startPath, "Routes");
                if (Directory.Exists(potentialPath))
                {
                    lastFoundRoutesFolder = potentialPath; // Cache it here too
                    return potentialPath;
                }

                // Check subdirectories
                foreach (var dir in Directory.GetDirectories(startPath))
                {
                    try
                    {
                        // Skip system folders to speed things up and avoid permission issues
                        if (dir.EndsWith("Windows") || dir.EndsWith("Program Files") ||
                            dir.EndsWith("Program Files (x86)") || dir.EndsWith("$Recycle.Bin"))
                            continue;

                        string? result = FindRoutesFolderRecursive(dir, maxDepth - 1);
                        if (result != null)
                            return result;
                    }
                    catch { /* Skip any we can't access */ }
                }
            }
            catch { /* Skip if we can't access this directory */ }

            return null;
        }
        #endregion

        #region Route Processing
        // Checks all route entries to see if they're complete based on game stats
        // Marks entries as complete or incomplete with checkmarks
        // Returns true if anything changed so we can update the display
        public bool UpdateCompletionStatus(DataGridView routeGrid, GameStatsEventArgs stats)
        {
            bool anyChanges = false;

            // Only check for 100% completion in AC4
            bool isAC4Game = gameConnectionManager?.GameStats is AC4GameStats;
            bool isGameCompleted = false;

            if (isAC4Game)
            {
                // Round percentage to 2 decimal places for 100% detection - AC4 specific
                isGameCompleted = Math.Round(stats.GetValue<float>("Exact Percentage", 0f), 2) >= 100.0f;

                // Handle the case when dropping below 100% after being at 100%
                if (isAt100Percent && !isGameCompleted)
                {
                    // If we dropped below 100%, stop the timer and reset the flag
                    isAt100Percent = false;
                    completionCheckTimer?.Dispose();
                    completionCheckTimer = null;
                    Debug.WriteLine("Game dropped below 100% completion - resuming normal updates");
                }

                // If we're at 100% and already marked as such, skip the update to prevent scrolling issues
                if (isAt100Percent && isGameCompleted)
                {
                    // We're already at 100% and still at 100%, no need to update the UI
                    return false;
                }

                // If game just reached 100% complete, mark all entries as complete
                if (isGameCompleted && !isAt100Percent)
                {
                    // Mark all entries as complete in memory
                    foreach (var entry in routeEntries)
                    {
                        entry.IsCompleted = true;
                    }

                    // Refresh virtual grid to show changes
                    if (routeGrid.VirtualMode)
                    {
                        routeGrid.Invalidate();

                        // Apply current sorting mode and scroll to first incomplete
                        if (routeGrid.FindForm() is MainForm mainForm)
                        {
                            mainForm.ApplyCurrentSorting();
                        }
                        SortingManager.ScrollToFirstIncomplete(routeGrid);
                    }

                    Debug.WriteLine("Game at 100% completion - marked all route entries as complete");

                    // Set the 100% flag and start the timer
                    isAt100Percent = true;
                    completionCheckTimer?.Dispose();
                    completionCheckTimer = new System.Threading.Timer(
                        _ => isAt100Percent = isGameCompleted,
                        null,
                        10000,
                        10000);

                    // Auto-save when reaching 100%
                    AutoSaveProgress();
                    return true;
                }
            }

            // Regular processing for all games - work directly with routeEntries
            foreach (var entry in routeEntries)
            {
                // Check if this entry should be completed based on its conditions
                bool shouldBeCompleted = CheckCompletion(entry, stats);

                // Only update if completion status has changed
                if (shouldBeCompleted != entry.IsCompleted)
                {
                    entry.IsCompleted = shouldBeCompleted;
                    anyChanges = true;
                    Debug.WriteLine($"Completion status changed for '{entry.Name}': Now {(shouldBeCompleted ? "Completed" : "Incomplete")}");
                }
            }

            // If changes were made, refresh virtual grid, apply sorting, and scroll to first incomplete
            if (anyChanges)
            {
                if (routeGrid.VirtualMode)
                {
                    routeGrid.Invalidate(); // Refresh virtual mode display

                    // Apply current sorting mode and scroll to first incomplete
                    if (routeGrid.FindForm() is MainForm mainForm)
                    {
                        mainForm.ApplyCurrentSorting();
                    }
                    else
                    {
                        // Fallback to default sorting
                        SortRouteGridByCompletion(routeGrid);
                    }

                    // Always scroll to first incomplete after automatic completion
                    SortingManager.ScrollToFirstIncomplete(routeGrid);
                }
                AutoSaveProgress();
            }

            return anyChanges;
        }

        // does cleanup?
        public void Dispose()
        {
            completionCheckTimer?.Dispose();
            completionCheckTimer = null;
        }

        // call completionmanager checkcompletion so this file doesn't get too bloated
        private bool CheckCompletion(RouteEntry entry, GameStatsEventArgs stats)
        {
            // Delegate to the completion manager for game-specific logic
            return CompletionManager.CheckEntryCompletion(entry, stats, gameConnectionManager);
        }
        #endregion

        #region UI Management
        // main coding logic moved to SortingManager so just calling it in here in this method to avoid replacing tons of code
        public static void SortRouteGridByCompletion(DataGridView routeGrid)
        {
            SortingManager.ApplySortingQuiet(routeGrid, SortingOptionsForm.SortingMode.CompletedAtTop);
        }

        // Public method for completing any entry - used by hotkeys and manual actions
        // Handles UI updates and autosave automatically
        public void CompleteEntry(DataGridView routeGrid, RouteEntry entry)
        {
            if (entry == null) return;

            entry.IsCompleted = true;

            if (routeGrid.VirtualMode)
            {
                // For virtual mode, just refresh the display
                routeGrid.Invalidate();
                SortRouteGridByCompletion(routeGrid);
                SortingManager.ScrollToFirstIncomplete(routeGrid);
            }
            else
            {
                // Traditional mode - update grid rows
                foreach (DataGridViewRow row in routeGrid.Rows)
                {
                    if (row.Tag == entry)
                    {
                        row.Cells[1].Value = "X";
                        break;
                    }
                }
                SortRouteGridByCompletion(routeGrid);
                SortingManager.ScrollToFirstIncomplete(routeGrid);
            }

            AutoSaveProgress();
        }

        // Public method for skipping any entry - used by hotkeys and manual actions
        // Removes the entry from display and handles autosave
        public void SkipEntry(DataGridView routeGrid, RouteEntry entry)
        {
            if (entry == null) return;

            entry.IsSkipped = true;

            if (routeGrid.VirtualMode)
            {
                // For virtual mode, update row count and refresh
                routeGrid.RowCount = routeEntries.Count(e => !e.IsSkipped);
                routeGrid.Invalidate();
            }
            else
            {
                // Traditional mode - remove from grid display
                for (int i = routeGrid.Rows.Count - 1; i >= 0; i--)
                {
                    if (routeGrid.Rows[i].Tag == entry)
                    {
                        routeGrid.Rows.RemoveAt(i);
                        break;
                    }
                }
            }

            AutoSaveProgress();
        }

        // New undo feature that works on any completed or skipped entry
        // Brings back skipped entries and marks completed ones as incomplete
        public void UndoEntry(DataGridView routeGrid, RouteEntry entry)
        {
            if (entry == null) return;

            bool wasSkipped = entry.IsSkipped;
            entry.IsCompleted = false;
            entry.IsSkipped = false;

            if (routeGrid.VirtualMode)
            {
                // For virtual mode, update row count and refresh
                if (wasSkipped)
                {
                    routeGrid.RowCount = routeEntries.Count(e => !e.IsSkipped);
                }
                routeGrid.Invalidate();
                SortRouteGridByCompletion(routeGrid);
                SortingManager.ScrollToFirstIncomplete(routeGrid);
            }
            else
            {
                // Traditional mode
                if (wasSkipped)
                {
                    // Re-add to grid if it was skipped
                    string completionMark = entry.IsCompleted ? "X" : "";
                    int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                    routeGrid.Rows[rowIndex].Tag = entry;
                }
                else
                {
                    // Update existing row
                    foreach (DataGridViewRow row in routeGrid.Rows)
                    {
                        if (row.Tag == entry)
                        {
                            row.Cells[1].Value = "";
                            break;
                        }
                    }
                }
                SortRouteGridByCompletion(routeGrid);
                SortingManager.ScrollToFirstIncomplete(routeGrid);
            }

            AutoSaveProgress();
        }

        // Helper method to get the selected entry for advanced hotkey mode
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public RouteEntry? GetSelectedEntry(DataGridView routeGrid)
        {
            if (routeGrid.VirtualMode)
            {
                // For virtual mode, use the current row index
                if (routeGrid.CurrentRow != null && routeGrid.CurrentRow.Index >= 0)
                {
                    return GetEntryByIndex(routeGrid.CurrentRow.Index);
                }
            }
            else
            {
                // Traditional mode
                if (routeGrid.CurrentRow?.Tag is RouteEntry selectedEntry)
                    return selectedEntry;
            }
            return null;
        }

        // Helper method to get the first incomplete entry for normal hotkey mode
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public RouteEntry? GetFirstIncompleteEntry(DataGridView routeGrid)
        {
            if (routeGrid.VirtualMode)
            {
                // For virtual mode, search through the entries list
                foreach (var entry in routeEntries)
                {
                    if (!entry.IsCompleted && !entry.IsSkipped)
                        return entry;
                }
            }
            else
            {
                // Traditional mode
                foreach (DataGridViewRow row in routeGrid.Rows)
                {
                    if (row.Tag is RouteEntry entry && !entry.IsCompleted)
                        return entry;
                }
            }
            return null;
        }

        // ==========MY NOTES==============
        // Gets a route entry by index for virtual mode operations
        // Used by various UI operations that need to access entries by grid row index
        public RouteEntry? GetEntryByIndex(int index)
        {
            if (index >= 0 && index < routeEntries.Count)
            {
                return routeEntries[index];
            }
            return null;
        }

        // ==========MY NOTES==============
        // Forces a refresh of the virtual mode grid display
        // Call this after making changes to route entries to update the UI
        public void RefreshVirtualGrid(DataGridView routeGrid)
        {
            if (routeGrid.VirtualMode)
            {
                routeGrid.RowCount = routeEntries.Count(e => !e.IsSkipped);
                routeGrid.Invalidate();
            }
        }

        #endregion

        #region Progress Persistence
        // Enhanced autosave that creates multiple backup files
        // Gives users automatic protection with rotating save files
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1869:",
        Justification = "Performance optimizations are minimal")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079:",
        Justification = "because i said so")]
        public void AutoSaveProgress()
        {
            RouteHelpers.AutoSaveProgress(this);
        }

        // Detects when the game is loading, in menu, or in active gameplay
        // Preserves progress across these state changes
        public void HandleGameStateTransition(DataGridView routeGrid)
        {
            RouteHelpers.HandleGameStateTransition(this, routeGrid);
        }

        // Saves your progress to any file name you choose
        // Lets you create multiple save files for different segments or runs
        public void SaveProgress(Form parentForm)
        {
            RouteHelpers.SaveProgressToFile(this, parentForm);
        }

        // Lets you pick any saved file to load progress from
        // Updates the display to show loaded completion status
        public bool LoadProgress(DataGridView routeGrid, Form parentForm)
        {
            return RouteHelpers.LoadProgressFromFile(this, routeGrid, parentForm);
        }

        public string GetRouteFilePath()
        {
            return routeFilePath;
        }

        // show and open the save location
        public void ShowSaveLocation(Form parentForm)
        {
            RouteHelpers.ShowSaveLocation(this, parentForm);
        }

        // Sets all entries to not completed/skipped, deletes autosave, and refreshes the grid
        public void ResetProgress(DataGridView routeGrid)
        {
            RouteHelpers.ResetProgressData(this, routeGrid);
        }
        #endregion

        #region Labels
        public (float CompletionPercentage, int CompletedCount, int TotalCount) CalculateCompletionStats()
        {
            if (routeEntries == null || routeEntries.Count == 0)
                return (0.00f, 0, 0);

            int totalEntries = routeEntries.Count(e => !e.IsSkipped);
            int completedEntries = routeEntries.Count(e => e.IsCompleted && !e.IsSkipped);

            float percentage = totalEntries > 0
                ? (float)Math.Round((float)completedEntries / totalEntries * 100, 2)
                : 0.00f;

            return (percentage, completedEntries, totalEntries);
        }

        public string GetCurrentLocation()
        {
            var entry = routeEntries.FirstOrDefault(e => !e.IsCompleted && !e.IsSkipped);
            return entry?.Location ?? string.Empty;
        }
        #endregion

        #region Public Data Access Methods for RouteHelpers
        // ==========MY NOTES==============
        // Updates the internal route entries list for virtual mode operations
        // Used by filtering and sorting systems to modify the displayed data
        public void UpdateRouteEntries(List<RouteEntry> newEntries)
        {
            routeEntries = newEntries;
        }

        // ==========MY NOTES==============
        // Public accessor methods for RouteHelpers to access internal data
        public List<RouteEntry> GetRouteEntries()
        {
            return routeEntries;
        }

        public GameConnectionManager GetGameConnectionManager()
        {
            return gameConnectionManager;
        }

        public bool GetWasPreviouslyInNonGameplay()
        {
            return wasPreviouslyInNonGameplay;
        }

        public void SetWasPreviouslyInNonGameplay(bool value)
        {
            wasPreviouslyInNonGameplay = value;
        }

        // ==========MY NOTES==============
        // Gets the complete original route entries including completed and skipped ones
        // Used when switching sorting modes to ensure all entries are available
        public List<RouteEntry> GetAllOriginalEntries()
        {
            return routeEntries;
        }
        #endregion
    }
}