using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Central manager for route data handling, file operations, and route completion tracking
    // Connects game statistics with route entries and updates their completion status
    // Responsible for loading, displaying, and real-time updating of route information
    // ==========MY NOTES==============
    // This is the brain of the route tracking system
    // Handles finding route files, loading them, and marking items as complete
    // Updates the route display as you play the game
    public class RouteManager
    {
        // ==========FORMAL COMMENT=========
        // Key fields for route management functionality
        // Tracks file paths, route entries, and maintains connection to game stats
        // Caches folder locations and tracking state between operations
        // ==========MY NOTES==============
        // These are the important variables that keep track of everything
        // Remembers where files are so we don't have to search every time
        // Keeps the last percentage value to detect changes
        private readonly string routeFilePath;
        private readonly GameConnectionManager gameConnectionManager;
        private List<RouteEntry> routeEntries = [];

        // last know folder path for Routes folder to avoid repeated searches
        private static string? lastFoundRoutesFolder;

        // Add these fields to the RouteManager class
        private bool isAt100Percent = false;
        private System.Threading.Timer? completionCheckTimer = null;
        private bool wasPreviouslyInNonGameplay = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0290: Use primary constructure",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "because i said so")]
        public RouteManager(string routeFilePath, GameConnectionManager gameConnectionManager)
        {
            this.routeFilePath = routeFilePath;
            this.gameConnectionManager = gameConnectionManager;
        }

        #region Route Loading

        // ==========FORMAL COMMENT=========
        // Loads route entries from the specified TSV file path
        // Uses RouteLoader to parse raw file data into structured route entries
        // Returns the loaded entries or empty list if file cannot be found
        // ==========MY NOTES==============
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

                // Assign sequential IDs to entries
                for (int i = 0; i < routeEntries.Count; i++)
                {
                    routeEntries[i].Id = i + 1;
                }
            }
            return routeEntries;
        }

        // ==========FORMAL COMMENT=========
        // Populates the route grid with data from appropriate route files
        // Performs extensive file searching and provides diagnostic feedback
        // Shows route entries with completion status in the DataGridView
        // ==========MY NOTES==============
        // Fills the route grid with entries from the best matching file
        // Shows progress messages while it searches for files
        // Shows errors if files can't be found or loaded properly
        public void LoadRouteDataIntoGrid(DataGridView routeGrid,
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for interface compatibility")]
    string gameName)
        {
            routeGrid.Rows.Clear();
            routeGrid.Rows.Add("Searching everywhere for route files...", "");

            try
            {
                // First try to find any Routes folder
                string? routesFolder = FindRoutesFolderAnywhere();
                if (routesFolder == null)
                {
                    routeGrid.Rows.Add("ERROR: Could not find any Routes folder anywhere!", "");
                    routeGrid.Rows.Add("Please create a 'Routes' folder somewhere on your system", "");
                    return;
                }

                routeGrid.Rows.Add($"Found Routes folder at: {routesFolder}", "");

                // Find any TSV file that might be a route file
                string[] tsvFiles = Directory.GetFiles(routesFolder, "*.tsv");
                if (tsvFiles.Length == 0)
                {
                    routeGrid.Rows.Add("ERROR: No TSV files found in Routes folder!", "");
                    return;
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

                routeGrid.Rows.Add($"Using route file: {routeFile}", "");

                // Load the route entries
                List<RouteEntry> entries = LoadRouteFromPath(routeFile);
                if (entries.Count == 0)
                {
                    routeGrid.Rows.Add("ERROR: No valid entries found in the route file", "");
                    return;
                }

                // Clear diagnostics and show actual entries
                routeGrid.Rows.Clear();
                foreach (var entry in entries)
                {
                    string completionMark = entry.IsCompleted ? "X" : "";
                    int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                    routeGrid.Rows[rowIndex].Tag = entry;
                }

                // Store the entries for future reference
                routeEntries = entries;

                // Sort and scroll
                SortAndScrollToFirstIncomplete(routeGrid);
            }
            catch (Exception ex)
            {
                routeGrid.Rows.Add($"Error: {ex.Message}", "");
                routeGrid.Rows.Add($"Stack trace: {ex.StackTrace}", "");
            }
        }

        // ==========FORMAL COMMENT=========
        // Parses a TSV file into a collection of RouteEntry objects
        // Reads each line, extracting display text, condition type, and condition value
        // Filters out invalid or incomplete entries during processing
        // ==========MY NOTES==============
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
                if (parts.Length >= 3)
                {
                    string displayText = parts[0].Trim();

                    // Skip entries with blank display text
                    if (string.IsNullOrWhiteSpace(displayText))
                        continue;

                    string conditionType = parts[1].Trim();

                    if (int.TryParse(parts[2].Trim(), out int conditionValue))
                    {
                        RouteEntry entry = new(displayText, conditionType, conditionValue);

                        // Add coordinates from the fourth column if available
                        if (parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]))
                        {
                            entry.Coordinates = parts[3].Trim();
                        }

                        entries.Add(entry);
                    }
                }
            }

            return entries;
        }

        // ==========FORMAL COMMENT=========
        // Comprehensive search algorithm for locating any Routes folder
        // Checks multiple possible locations in order of likelihood
        // Uses caching to optimize repeated searches and recursive searching as fallback
        // ==========MY NOTES==============
        // Searches everywhere on your PC for a Routes folder
        // Tries common places first, then does deeper searches if needed
        // Remembers where it found the folder to be faster next time
        private static string? FindRoutesFolderAnywhere()
        {
            // First check the cached location from previous searches
            if (lastFoundRoutesFolder != null && Directory.Exists(lastFoundRoutesFolder))
            {
                return lastFoundRoutesFolder; // Use cached path if it still exists
            }

            // Places to check in order of likelihood
            List<string> possibleLocations =
    [
        // App folder and nearby
        AppDomain.CurrentDomain.BaseDirectory,
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."),
        
        // User folders
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop"),
        
        // Project folders (when running from IDE)
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..")
    ];

            // First check direct Routes folders in common locations
            foreach (var location in possibleLocations)
            {
                try
                {
                    string potentialPath = Path.Combine(location, "Routes");
                    if (Directory.Exists(potentialPath))
                        return potentialPath;
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
                            // Check for Routes folder in this directory
                            string potentialPath = Path.Combine(dir, "Routes");
                            if (Directory.Exists(potentialPath))
                            {
                                lastFoundRoutesFolder = potentialPath;
                                return potentialPath;
                            }

                            // Also check children directories one level deeper
                            foreach (var subdir in Directory.GetDirectories(dir))
                            {
                                try
                                {
                                    string subPath = Path.Combine(subdir, "Routes");
                                    if (Directory.Exists(potentialPath))
                                    {
                                        lastFoundRoutesFolder = potentialPath;
                                        return potentialPath;
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
                }
                return foundPath;
            }
            catch { /* Skip if we can't access */ }

            return null; // No Routes folder found anywhere
        }

        // ==========FORMAL COMMENT=========
        // Recursively searches for a Routes folder starting from a given path
        // Implements depth limiting to prevent excessive searching
        // Skips system folders to improve performance and avoid permission issues
        // ==========MY NOTES==============
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
        // ==========FORMAL COMMENT=========
        // Updates completion status of all route entries based on current game statistics
        // Processes each entry individually through CheckCompletion to determine state
        // Returns whether any entries changed state during this update
        // ==========MY NOTES==============
        // Checks all route entries to see if they're complete based on game stats
        // Marks entries as complete or incomplete with checkmarks
        // Returns true if anything changed so we can update the display
        public bool UpdateCompletionStatus(DataGridView routeGrid, StatsUpdatedEventArgs stats)
        {
            bool anyChanges = false;

            // Round percentage to 2 decimal places for 100% detection
            bool isGameCompleted = Math.Round(stats.PercentFloat, 2) >= 100.0f;

            // Log stats for debugging
            string statsInfo = $"Stats: Percent={stats.Percent}%, PercentFloat={stats.PercentFloat}%, " +
                 $"Viewpoints={stats.Viewpoints}, Myan={stats.Myan}, " +
                 $"Treasure={stats.Treasure}, Fragments={stats.Fragments}, " +
                 $"Assassin={stats.Assassin}, Naval={stats.Naval}";
            Debug.WriteLine(statsInfo);

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
                // Clear the grid and re-add all entries as completed
                List<RouteEntry> entries = [];

                foreach (DataGridViewRow row in routeGrid.Rows)
                {
                    if (row.Tag is RouteEntry entry)
                    {
                        entry.IsCompleted = true;  // Mark all entries as complete
                        entries.Add(entry);
                    }
                }

                // Clear and rebuild the grid with all entries as completed
                routeGrid.Rows.Clear();
                foreach (var entry in entries)
                {
                    int rowIndex = routeGrid.Rows.Add(entry.DisplayText, "X");
                    routeGrid.Rows[rowIndex].Tag = entry;
                }

                // Scroll to top since all entries are complete
                if (routeGrid.Rows.Count > 0)
                    routeGrid.FirstDisplayedScrollingRowIndex = 0;

                Debug.WriteLine("Game at 100% completion - marked all route entries as complete");

                // Set the 100% flag and start the timer
                isAt100Percent = true;
                completionCheckTimer?.Dispose();  // Dispose any existing timer
                completionCheckTimer = new System.Threading.Timer(
                    _ => isAt100Percent = isGameCompleted,  // Keep the flag aligned with game state
                    null,
                    10000,  // Check after 10 seconds
                    10000); // And continue checking every 10 seconds

                // Auto-save when reaching 100%
                AutoSaveProgress();
                return true;
            }

            // Normal flow for non-100% completion state
            if (!isGameCompleted)
            {
                // Regular individual checking
                foreach (DataGridViewRow row in routeGrid.Rows)
                {
                    if (row.Tag is RouteEntry entry)
                    {
                        // Check if this entry should be completed based on its conditions
                        bool shouldBeCompleted = CheckCompletion(entry, stats);

                        // Only update if completion status has changed
                        if (shouldBeCompleted != entry.IsCompleted)
                        {
                            entry.IsCompleted = shouldBeCompleted;
                            row.Cells[1].Value = shouldBeCompleted ? "X" : "";
                            anyChanges = true;
                        }
                    }
                }

                // If changes were made, sort and scroll
                if (anyChanges)
                {
                    SortAndScrollToFirstIncomplete(routeGrid);

                    // Auto-save whenever any entry is marked as completed
                    AutoSaveProgress();
                }
            }

            return anyChanges;
        }

        // Add this to the RouteManager class to handle cleanup
        public void Dispose()
        {
            completionCheckTimer?.Dispose();
            completionCheckTimer = null;
        }

        // ==========FORMAL COMMENT=========
        // Determines if a specific route entry should be marked as completed
        // Uses type-based matching to compare game stats against completion conditions
        // Handles special cases like story missions and supports multiple entry types
        // ==========MY NOTES==============
        // Decides if a specific route entry is complete or not
        // Looks at the entry type (viewpoint, chest, etc.) and checks the right stat
        // Handles special types like story missions differently from collectibles
        private bool CheckCompletion(RouteEntry entry, StatsUpdatedEventArgs stats)
        {
            if (string.IsNullOrEmpty(entry.Type))
                return false;

            // Normalize for comparison (trim and ignore case)
            string normalizedType = entry.Type.Trim();

            // Get the special activity counts from the game stats
            var specialCounts = gameConnectionManager?.GameStats is AC4GameStats ac4GameStats
                ? ac4GameStats.GetSpecialActivityCounts()
                : (0, 0, 0, 0);

            // Use your exact names for matching
            return normalizedType switch
            {
                "Story" or "story" => specialCounts.Item1 >= entry.Condition,
                "Viewpoint" or "viewpoint" => stats.Viewpoints >= entry.Condition,
                "Chest" or "chest" => stats.TotalChests >= entry.Condition,
                "Animus Fragment" or "animus fragment" => stats.Fragments >= entry.Condition,
                "Myan Stones" or "myan stones" => stats.Myan >= entry.Condition,
                "Buried Treasure" or "buried treasure" => stats.Treasure >= entry.Condition,
                "Assassin Contracts" or "assassin contracts" => stats.Assassin >= entry.Condition,
                "Naval Contracts" or "naval contracts" => stats.Naval >= entry.Condition,
                "Letters" or "letters" => stats.Letters >= entry.Condition,
                "Manuscripts" or "manuscripts" => stats.Manuscripts >= entry.Condition,
                "Shanty" or "shanty" => stats.Music >= entry.Condition,
                "Forts" or "forts" => stats.Forts >= entry.Condition,
                "Taverns" or "taverns" => stats.Taverns >= entry.Condition,
                "Upgrades" or "upgrades" => gameConnectionManager?.GameStats is AC4GameStats upgradeStats &&
                entry.Condition > 0 && entry.Condition <= upgradeStats.GetPurchasedUpgrades().Length &&
                upgradeStats.GetPurchasedUpgrades()[entry.Condition - 1],
                "Legendary Ships" or "legendary ships" => specialCounts.Item3 >= entry.Condition,
                "Templar Hunts" or "templar hunts" => specialCounts.Item2 >= entry.Condition,
                "Treasure Map" or "treasure map" => specialCounts.Item4 >= entry.Condition,
                _ => false,
            };
        }
        #endregion

        #region UI Management
        // ==========FORMAL COMMENT=========
        // Sorts the route grid to show completed entries first, followed by incomplete entries
        // Automatically scrolls to the first incomplete entry to focus user attention
        // Preserves original ordering within completion groups for logical progression
        // ==========MY NOTES==============
        // Sorts the route grid so completed items move to the top
        // Automatically jumps to the first thing you still need to do
        // Makes it easy to see what's done and what's next at a glance
        public static void SortAndScrollToFirstIncomplete(DataGridView routeGrid)
        {
            if (routeGrid == null || routeGrid.Rows.Count == 0)
                return;

            // Get all entries from the grid
            List<RouteEntry> entries = [];
            for (int i = 0; i < routeGrid.Rows.Count; i++)
            {
                if (routeGrid.Rows[i].Tag is RouteEntry entry)
                {
                    entries.Add(entry);
                }
            }

            // Sort: completed first (by Id), then incomplete (by Id)
            var sortedEntries = entries
                .OrderByDescending(e => e.IsCompleted)
                .ThenBy(e => e.Id)
                .ToList();

            // Clear and re-add rows in sorted order
            routeGrid.Rows.Clear();
            int firstIncompleteIndex = -1;

            for (int i = 0; i < sortedEntries.Count; i++)
            {
                var entry = sortedEntries[i];
                string completionMark = entry.IsCompleted ? "X" : "";
                int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                routeGrid.Rows[rowIndex].Tag = entry;

                // Track first non-completed entry
                if (firstIncompleteIndex == -1 && !entry.IsCompleted)
                    firstIncompleteIndex = rowIndex;
            }

            // Scroll to first non-completed entry
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
        #endregion

        #region Progress Persistence
        // ==========FORMAL COMMENT=========
        // Automatically saves current progress to a predefined autosave file
        // Creates consistent file name in same folder as route file for easy tracking
        // ==========MY NOTES==============
        // Silently saves progress whenever route entries change
        // Used for preserving progress between game sessions and crashes
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1869:",
        Justification = "Performance optimizations are minimal")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:",
        Justification = "because i said so")]
        public void AutoSaveProgress()
        {
            if (routeEntries == null || routeEntries.Count == 0)
                return;

            try
            {
                // Create save data with completion and skip status
                Dictionary<string, (bool IsCompleted, bool IsSkipped)> entryStatus = [];
                foreach (var entry in routeEntries)
                {
                    string key = $"{entry.Id}_{entry.Name}_{entry.Type}_{entry.Condition}";
                    entryStatus[key] = (entry.IsCompleted, entry.IsSkipped);
                }

                // Create directory for saves if it doesn't exist
                string saveDir = Path.Combine(
                    Path.GetDirectoryName(routeFilePath) ?? AppDomain.CurrentDomain.BaseDirectory,
                    "SavedProgress");

                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);

                // Use a fixed name for the autosave
                string routeName = Path.GetFileNameWithoutExtension(routeFilePath);
                string autosaveFile = Path.Combine(saveDir, $"{routeName}_AutoSave.json");

                // Save the data - completely overwrite the file
                System.Text.Json.JsonSerializerOptions options = new() { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(entryStatus, options);
                File.WriteAllText(autosaveFile, json);

                Debug.WriteLine($"Auto-saved progress to {autosaveFile}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error auto-saving progress: {ex.Message}");
            }
        }

        // ==========FORMAL COMMENT=========
        // Loads autosaved route completion status if available
        // Silently loads from predefined autosave location without user interaction
        // ==========MY NOTES==============
        // Automatically loads saved progress when re-entering gameplay
        // Preserves completion status between sessions
        private bool LoadAutoSave(DataGridView routeGrid)
        {
            if (routeEntries == null || routeEntries.Count == 0 || routeGrid == null)
                return false;

            try
            {
                // Determine autosave file path
                string saveDir = Path.Combine(
                    Path.GetDirectoryName(routeFilePath) ?? AppDomain.CurrentDomain.BaseDirectory,
                    "SavedProgress");

                string routeName = Path.GetFileNameWithoutExtension(routeFilePath);
                string autosaveFile = Path.Combine(saveDir, $"{routeName}_AutoSave.json");

                // Check if file exists
                if (!File.Exists(autosaveFile))
                    return false;

                // Load and apply saved data
                string json = File.ReadAllText(autosaveFile);

                try
                {
                    // First, try to deserialize as the new tuple format
                    var entryStatus = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, (bool IsCompleted, bool IsSkipped)>>(json);

                    // Apply status to entries
                    foreach (var entry in routeEntries)
                    {
                        string key = $"{entry.Id}_{entry.Name}_{entry.Type}_{entry.Condition}";
                        if (entryStatus != null && entryStatus.TryGetValue(key, out var status))
                        {
                            entry.IsCompleted = status.IsCompleted;
                            entry.IsSkipped = status.IsSkipped;
                        }
                    }
                }
                catch
                {
                    // Fallback: try the old format (for backwards compatibility)
                    var completionStatus = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(json);

                    // Apply completion status only
                    foreach (var entry in routeEntries)
                    {
                        string key = $"{entry.Name}_{entry.Type}_{entry.Condition}";
                        if (completionStatus != null && completionStatus.TryGetValue(key, out bool isCompleted))
                        {
                            entry.IsCompleted = isCompleted;
                        }
                    }
                }

                // Update UI
                routeGrid.Rows.Clear();
                bool anyChanges = false;

                foreach (var entry in routeEntries)
                {
                    // Skip entries marked as skipped
                    if (entry.IsSkipped)
                        continue;

                    string completionMark = entry.IsCompleted ? "X" : "";
                    int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                    routeGrid.Rows[rowIndex].Tag = entry;
                    anyChanges = true;
                }

                if (anyChanges)
                {
                    SortAndScrollToFirstIncomplete(routeGrid);
                    Debug.WriteLine($"Loaded autosave from {autosaveFile}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading autosave: {ex.Message}");
                return false;
            }
        }

        // ==========FORMAL COMMENT=========
        // Handles game state transitions including loading and main menu
        // Automatically saves/loads progress based on state changes
        // ==========MY NOTES==============
        // Detects when the game is loading, in menu, or in active gameplay
        // Preserves progress across these state changes
        public void HandleGameStateTransition(DataGridView routeGrid)
        {
            // Get current states from the game
            (bool IsLoading, bool IsMainMenu) = (gameConnectionManager?.GameStats as AC4GameStats)?.GetGameStatus() ?? (false, false);
            bool isLoading = IsLoading;
            bool isMainMenu = IsMainMenu;

            bool isCurrentlyInNonGameplay = isLoading || isMainMenu;

            // Entering loading or menu from gameplay
            if (isCurrentlyInNonGameplay && !wasPreviouslyInNonGameplay)
            {
                Debug.WriteLine($"AC4: Entering non-gameplay state (Loading: {isLoading}, Menu: {isMainMenu}) - saving progress");
                AutoSaveProgress();
            }
            // Returning to gameplay from loading or menu
            else if (!isCurrentlyInNonGameplay && wasPreviouslyInNonGameplay)
            {
                Debug.WriteLine($"AC4: Returning to gameplay - loading saved progress");
                LoadAutoSave(routeGrid);
            }

            // Update previous state
            wasPreviouslyInNonGameplay = isCurrentlyInNonGameplay;
        }

        // ==========FORMAL COMMENT=========
        // Saves the current route completion status to a user-specified file
        // Displays a SaveFileDialog to allow custom naming of save files
        // ==========MY NOTES==============
        // Saves your progress to any file name you choose
        // Lets you create multiple save files for different segments or runs
        public void SaveProgress(Form parentForm)
        {
            if (routeEntries == null || routeEntries.Count == 0)
                return;

            try
            {
                // Create save data with completion status
                Dictionary<string, bool> completionStatus = [];
                foreach (var entry in routeEntries)
                {
                    string key = $"{entry.Name}_{entry.Type}_{entry.Condition}";
                    completionStatus[key] = entry.IsCompleted;
                }

                // Setup save dialog
                using SaveFileDialog saveDialog = new()
                {
                    Filter = "Progress Files|*.json|All Files|*.*",
                    Title = "Save Route Progress",
                    DefaultExt = "json",
                    FileName = $"RouteProgress_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                // Try to use a logical starting folder
                string initialDir = Path.GetDirectoryName(routeFilePath) ?? string.Empty;
                string saveDir = Path.Combine(initialDir, "SavedProgress");
                if (Directory.Exists(saveDir))
                    saveDialog.InitialDirectory = saveDir;
                else if (Directory.Exists(initialDir))
                    saveDialog.InitialDirectory = initialDir;

                // Show dialog and save if confirmed
                if (saveDialog.ShowDialog(parentForm) == DialogResult.OK)
                {
                    string json = System.Text.Json.JsonSerializer.Serialize(completionStatus);
                    File.WriteAllText(saveDialog.FileName, json);

                    // Also update autosave when manually saving
                    AutoSaveProgress();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving progress: {ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========FORMAL COMMENT=========
        // Loads route completion status from a user-selected file
        // Uses OpenFileDialog to allow selecting from multiple save files
        // ==========MY NOTES==============
        // Lets you pick any saved file to load progress from
        // Updates the display to show loaded completion status
        public bool LoadProgress(DataGridView routeGrid, Form parentForm)
        {
            if (routeEntries == null || routeEntries.Count == 0 || routeGrid == null)
                return false;

            try
            {
                // Setup open file dialog
                using OpenFileDialog openDialog = new()
                {
                    Filter = "Progress Files|*.json|All Files|*.*",
                    Title = "Load Route Progress",
                    CheckFileExists = true
                };

                // Try to use a logical starting folder
                string initialDir = Path.GetDirectoryName(routeFilePath) ?? string.Empty;
                string saveDir = Path.Combine(initialDir, "SavedProgress");
                if (Directory.Exists(saveDir))
                    openDialog.InitialDirectory = saveDir;
                else if (Directory.Exists(initialDir))
                    openDialog.InitialDirectory = initialDir;

                // Show dialog and load if confirmed
                if (openDialog.ShowDialog(parentForm) != DialogResult.OK)
                    return false;

                // Load and apply saved data
                string json = File.ReadAllText(openDialog.FileName);
                var completionStatus = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(json);

                if (completionStatus == null || completionStatus.Count == 0)
                    return false;

                // Apply saved completion status
                bool anyChanges = false;
                foreach (var entry in routeEntries)
                {
                    string key = $"{entry.Name}_{entry.Type}_{entry.Condition}";
                    if (completionStatus.TryGetValue(key, out bool isCompleted))
                    {
                        if (entry.IsCompleted != isCompleted)
                        {
                            entry.IsCompleted = isCompleted;
                            anyChanges = true;
                        }
                    }
                }

                // Update UI if needed
                if (anyChanges)
                {
                    routeGrid.Rows.Clear();
                    foreach (var entry in routeEntries)
                    {
                        string completionMark = entry.IsCompleted ? "X" : "";
                        int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                        routeGrid.Rows[rowIndex].Tag = entry;
                    }

                    SortAndScrollToFirstIncomplete(routeGrid);

                    // After loading manually, update autosave as well
                    AutoSaveProgress();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading progress: {ex.Message}", "Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public string GetRouteFilePath()
        {
            return routeFilePath;
        }

        // And add this method to show and open the save location
        public void ShowSaveLocation(Form parentForm)
        {
            string saveDir = Path.Combine(
                Path.GetDirectoryName(routeFilePath) ?? AppDomain.CurrentDomain.BaseDirectory,
                "SavedProgress");

            MessageBox.Show(parentForm,
                $"Autosave files are stored in:\n{saveDir}",
                "Autosave Location",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Try to open the folder
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = saveDir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open save directory: {ex.Message}");
            }
        }

        // ==========FORMAL COMMENT=========
        // Resets all route progress, clears autosave, and updates the UI
        // ==========MY NOTES==============
        // Sets all entries to not completed/skipped, deletes autosave, and refreshes the grid
        public void ResetProgress(DataGridView routeGrid)
        {
            // Clear completion/skipped state in memory
            foreach (var entry in routeEntries)
            {
                entry.IsCompleted = false;
                entry.IsSkipped = false;
            }

            // Delete autosave file if it exists
            string saveDir = Path.Combine(
                Path.GetDirectoryName(routeFilePath) ?? AppDomain.CurrentDomain.BaseDirectory,
                "SavedProgress");
            string routeName = Path.GetFileNameWithoutExtension(routeFilePath);
            string autosaveFile = Path.Combine(saveDir, $"{routeName}_AutoSave.json");
            if (File.Exists(autosaveFile))
                File.Delete(autosaveFile);

            // Refresh the UI
            routeGrid.Rows.Clear();
            foreach (var entry in routeEntries)
            {
                int rowIndex = routeGrid.Rows.Add(entry.DisplayText, "");
                routeGrid.Rows[rowIndex].Tag = entry;
            }
            SortAndScrollToFirstIncomplete(routeGrid);
        }
        #endregion
    }
}