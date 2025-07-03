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
                    // Set prerequisite to previous entry (null for first)
                    routeEntries[i].Prerequisite = i > 0 ? routeEntries[i - 1] : null;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:",
        Justification = "Required for interface compatibility")]
        public void LoadRouteDataIntoGrid(DataGridView routeGrid, string gameName)
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
                SortRouteGridByCompletion(routeGrid);
                ScrollToFirstIncomplete(routeGrid);
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

                // Log stats for debugging
                string statsInfo = $"Stats: Percent={stats.GetValue<int>("Completion Percentage", 0)}%, " +
                    $"PercentFloat={stats.GetValue<float>("Exact Percentage", 0f):F2}%, " +
                    $"Viewpoints={stats.GetValue<int>("Viewpoints", 0)}, Myan={stats.GetValue<int>("Myan Stones", 0)}, " +
                    $"Treasure={stats.GetValue<int>("Buried Treasure", 0)}, Fragments={stats.GetValue<int>("Animus Fragments", 0)}, " +
                    $"Assassin={stats.GetValue<int>("Assassin Contracts", 0)}, Naval={stats.GetValue<int>("Naval Contracts", 0)}";
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
            }

            // Regular processing for all games
            // Check individual entries regardless of completion percentage
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
                SortRouteGridByCompletion(routeGrid);
                ScrollToFirstIncomplete(routeGrid);

                // Auto-save whenever any entry is marked as completed
                AutoSaveProgress();
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
        private bool CheckCompletion(RouteEntry entry, GameStatsEventArgs stats)
        {
            if (string.IsNullOrEmpty(entry.Type))
                return false;

            // Enforce prerequisite for ALL entries (not just upgrades)
            if (entry.Prerequisite != null && !entry.Prerequisite.IsCompleted)
                return false;

            // Normalize for comparison (trim and ignore case)
            string normalizedType = entry.Type.Trim();

            // Get the special activity counts from the game stats
            // NOTE: This code is currently unused but kept for possible future use
            /*
            var specialCounts = new Dictionary<string, int>();
            if (stats.Stats.TryGetValue("Special Activities", out var specialActivitiesObj) &&
                specialActivitiesObj is Dictionary<string, int> specialActivities)
            {
                specialCounts = specialActivities;
            }
            */

            // Use your exact names for matching with dictionary-based approach
            return normalizedType switch
            {
                "Story" or "story" => stats.GetValue<int>("Story Missions", 0) >= entry.Condition,
                "Viewpoint" or "viewpoint" => stats.GetValue<int>("Viewpoints", 0) >= entry.Condition,
                "Chest" or "chest" => stats.GetValue<int>("Chests", 0) >= entry.Condition,
                "Animus Fragment" or "animus fragment" => stats.GetValue<int>("Animus Fragments", 0) >= entry.Condition,
                "Myan Stones" or "myan stones" => stats.GetValue<int>("Myan Stones", 0) >= entry.Condition,
                "Burired Treasure" or "burired treasure" => stats.GetValue<int>("Buried Treasure", 0) >= entry.Condition,
                "Assassin Contracts" or "assassin contracts" => stats.GetValue<int>("Assassin Contracts", 0) >= entry.Condition,
                "Naval Contracts" or "naval contracts" => stats.GetValue<int>("Naval Contracts", 0) >= entry.Condition,
                "Letters" or "letters" => stats.GetValue<int>("Letter Bottles", 0) >= entry.Condition,
                "Manuscripts" or "manuscripts" => stats.GetValue<int>("Manuscripts", 0) >= entry.Condition,
                "Shanty" or "shanty" => stats.GetValue<int>("Music Sheets", 0) >= entry.Condition,
                "Forts" or "forts" => stats.GetValue<int>("Forts", 0) >= entry.Condition,
                "Taverns" or "taverns" => stats.GetValue<int>("Taverns", 0) >= entry.Condition,
                "Legendary Ships" or "legendary ships" => stats.GetValue<int>("Legendary Ships", 0) >= entry.Condition,
                "Templar Hunts" or "templar hunts" => stats.GetValue<int>("Templar Hunts", 0) >= entry.Condition,
                "Treasure Map" or "treasure map" => stats.GetValue<int>("Treasure Maps", 0) >= entry.Condition,
                "Modern Day" or "modern day" => stats.GetValue<int>("Modern Day Missions", 0) >= entry.Condition,
                // Special case for upgrades
                "Upgrades" or "upgrades" => gameConnectionManager?.GameStats is AC4GameStats upgradeStats &&
                    entry.Condition > 0 && entry.Condition <= upgradeStats.GetPurchasedUpgrades().Length &&
                    upgradeStats.GetPurchasedUpgrades()[entry.Condition - 1],
                _ => false,
            };
        }
        #endregion

        #region UI Management
        // ==========FORMAL COMMENT=========
        // Sorts the route grid so completed entries are at the top (by Id), followed by incomplete entries (by Id).
        // Does not change the current scroll position or selection in the grid.
        // Maintains original route order within completed and incomplete groups.
        // ==========MY NOTES==============
        // Just puts all the checked-off stuff at the top, but doesn't scroll or highlight anything.
        // Keeps everything in the order from the route file so it's easy to see what's done and what's left.
        public static void SortRouteGridByCompletion(DataGridView routeGrid)
        {
            if (routeGrid == null || routeGrid.Rows.Count == 0)
                return;

            List<RouteEntry> entries = [];
            for (int i = 0; i < routeGrid.Rows.Count; i++)
            {
                if (routeGrid.Rows[i].Tag is RouteEntry entry)
                    entries.Add(entry);
            }

            var sortedEntries = entries
                .OrderByDescending(e => e.IsCompleted)
                .ThenBy(e => e.Id)
                .ToList();

            routeGrid.Rows.Clear();
            foreach (var entry in sortedEntries)
            {
                string completionMark = entry.IsCompleted ? "X" : "";
                int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                routeGrid.Rows[rowIndex].Tag = entry;
            }
        }

        // ==========FORMAL COMMENT=========
        // Scrolls the route grid to the first incomplete entry and selects it for user attention.
        // Does not change the order of entries in the grid.
        // If all entries are complete, scrolls to the top of the grid.
        // ==========MY NOTES==============
        // Jumps the list to the first thing you haven't finished yet and highlights it.
        // Doesn't move anything around, just helps you find what to do next.
        // If everything is done, scrolls to the very top.
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
        #endregion

        #region Progress Persistence
        // Replace the existing AutoSaveProgress method with this enhanced version:
        // ==========FORMAL COMMENT=========
        // Automatically saves current progress with cycling backup system
        // Creates numbered backup files and cycles through them for redundancy
        // ==========MY NOTES==============
        // Enhanced autosave that creates multiple backup files
        // Gives users automatic protection with rotating save files
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1869:",
        Justification = "Performance optimizations are minimal")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079:",
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

                // Determine game abbreviation and route type
                string gameAbbreviation = GetGameAbbreviation();
                string routeType = GetRouteType();

                // Create the cycling autosave with numbered backups
                CreateCyclingAutoSave(saveDir, gameAbbreviation, routeType, entryStatus);

                Debug.WriteLine($"Auto-saved progress with cycling backup system");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error auto-saving progress: {ex.Message}");
            }
        }

        // ==========FORMAL COMMENT=========
        // Creates cycling autosave files with numbered backups
        // Maintains main autosave and up to 10 numbered backup files
        // ==========MY NOTES==============
        // The core of the cycling backup system
        // Rotates through numbered backups (1-10) and cycles back to 1
        private void CreateCyclingAutoSave(string saveDir, string gameAbbreviation, string routeType, Dictionary<string, (bool IsCompleted, bool IsSkipped)> entryStatus)
        {
            // Create file names
            string baseFileName = $"{gameAbbreviation}AutoSave{routeType}.json";
            string mainAutosaveFile = Path.Combine(saveDir, baseFileName);

            string json = System.Text.Json.JsonSerializer.Serialize(entryStatus, JsonOptions);


            // If main autosave exists, we need to cycle it into numbered backups
            if (File.Exists(mainAutosaveFile))
            {
                // Find the next backup number to use (1-10, cycling)
                int nextBackupNumber = GetNextBackupNumber(saveDir, gameAbbreviation, routeType);

                // Move current main autosave to numbered backup
                string backupFileName = $"{gameAbbreviation}AutoSave{routeType}{nextBackupNumber}.json";
                string backupFilePath = Path.Combine(saveDir, backupFileName);

                try
                {
                    File.Copy(mainAutosaveFile, backupFilePath, overwrite: true);
                    Debug.WriteLine($"Backed up previous autosave to {backupFileName}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Warning: Could not create backup {backupFileName}: {ex.Message}");
                }
            }

            // Save new main autosave
            File.WriteAllText(mainAutosaveFile, json);
            Debug.WriteLine($"Created new autosave: {baseFileName}");
        }

        // ==========FORMAL COMMENT=========
        // Determines the next backup number in the cycle (1-10)
        // Cycles back to 1 after reaching 10 for continuous rotation
        // ==========MY NOTES==============
        // Figures out which numbered backup to create next
        // Cycles through 1-10 and starts over, giving us rolling backups
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private int GetNextBackupNumber(string saveDir, string gameAbbreviation, string routeType)
        {
            // Find the highest existing backup number
            int highestBackup = 0;
            string searchPattern = $"{gameAbbreviation}AutoSave{routeType}*.json";

            try
            {
                var backupFiles = Directory.GetFiles(saveDir, searchPattern);
                foreach (string file in backupFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string expectedPrefix = $"{gameAbbreviation}AutoSave{routeType}";

                    if (fileName.StartsWith(expectedPrefix) && fileName != expectedPrefix)
                    {
                        string numberPart = fileName[expectedPrefix.Length..];
                        if (int.TryParse(numberPart, out int backupNumber) && backupNumber > 0 && backupNumber <= 10)
                        {
                            highestBackup = Math.Max(highestBackup, backupNumber);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warning: Error checking backup files: {ex.Message}");
            }

            // Return next number in cycle (1-10)
            return highestBackup >= 10 ? 1 : highestBackup + 1;
        }

        // ==========FORMAL COMMENT=========
        // Determines game abbreviation based on route file or game connection
        // Returns standard abbreviations like AC4, GoW, TR13, etc.
        // ==========MY NOTES==============
        // Creates short game names for the autosave files
        // Uses common abbreviations that gamers would recognize
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1847",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private string GetGameAbbreviation()
        {
            string routeFileName = Path.GetFileNameWithoutExtension(routeFilePath).ToLower();

            // Check route file name for game indicators
            if (routeFileName.Contains("ac4") || (routeFileName.Contains("assassin") && routeFileName.Contains("creed") && routeFileName.Contains("4")))
                return "AC4";
            if (routeFileName.Contains("gow") || (routeFileName.Contains("god") && routeFileName.Contains("war")))
                return "GoW";
            if (routeFileName.Contains("tr13") || (routeFileName.Contains("tomb") && routeFileName.Contains("raider")))
                return "TR13";

            // Try to get from game connection if available
            if (gameConnectionManager?.GameStats != null)
            {
                var statsType = gameConnectionManager.GameStats.GetType().Name;
                if (statsType.Contains("AC4"))
                    return "AC4";
                if (statsType.Contains("GoW"))
                    return "GoW";
            }

            // Default fallback - use first 3-4 characters of route name
            string safeName = new([.. routeFileName.Take(4).Where(char.IsLetterOrDigit)]);
            return string.IsNullOrEmpty(safeName) ? "Game" : safeName.ToUpper();
        }

        // ==========FORMAL COMMENT=========
        // Determines if this is a user-created route or official route
        // Returns "User" for custom routes, empty string for official routes
        // ==========MY NOTES==============
        // Distinguishes between official game routes and user-made custom routes
        // Adds "User" to filename for custom routes so they don't conflict
        private string GetRouteType()
        {
            string routeFileName = Path.GetFileNameWithoutExtension(routeFilePath).ToLower();

            // Check for indicators of user-created routes
            if (routeFileName.Contains("user") ||
                routeFileName.Contains("custom") ||
                routeFileName.Contains("personal") ||
                routeFileName.Contains("test") ||
                (!routeFileName.Contains("100") && !routeFileName.Contains("main")))
            {
                return "User";
            }

            return ""; // Official route
        }

        // Replace the existing LoadAutoSave method with this enhanced version:
        // ==========FORMAL COMMENT=========
        // Loads autosaved route completion status from cycling backup system
        // Attempts to load from main autosave, falls back to most recent backup
        // ==========MY NOTES==============
        // Enhanced autosave loading that can find backups if main file is corrupted
        // Tries the main autosave first, then looks for numbered backups
        private bool LoadAutoSave(DataGridView routeGrid)
        {
            if (routeEntries == null || routeEntries.Count == 0 || routeGrid == null)
                return false;

            try
            {
                string saveDir = Path.Combine(
                    Path.GetDirectoryName(routeFilePath) ?? AppDomain.CurrentDomain.BaseDirectory,
                    "SavedProgress");

                if (!Directory.Exists(saveDir))
                    return false;

                // Get file naming info
                string gameAbbreviation = GetGameAbbreviation();
                string routeType = GetRouteType();
                string baseFileName = $"{gameAbbreviation}AutoSave{routeType}.json";
                string mainAutosaveFile = Path.Combine(saveDir, baseFileName);

                // Try to load main autosave first
                if (File.Exists(mainAutosaveFile))
                {
                    if (TryLoadAutosaveFile(mainAutosaveFile, routeGrid))
                    {
                        Debug.WriteLine($"Loaded autosave from {baseFileName}");
                        return true;
                    }
                }

                // If main autosave failed, try numbered backups (newest first)
                return TryLoadFromBackups(saveDir, gameAbbreviation, routeType, routeGrid);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading autosave: {ex.Message}");
                return false;
            }
        }

        // ==========FORMAL COMMENT=========
        // Attempts to load autosave from numbered backup files
        // Tries backups in reverse chronological order (newest first)
        // ==========MY NOTES==============
        // Fallback system that looks for working backup files
        // Starts with the most recent backup and works backwards
        private bool TryLoadFromBackups(string saveDir, string gameAbbreviation, string routeType, DataGridView routeGrid)
        {
            try
            {
                // Find all backup files for this game/route
                string searchPattern = $"{gameAbbreviation}AutoSave{routeType}*.json";
                var backupFiles = Directory.GetFiles(saveDir, searchPattern)
                    .Where(f =>
                    {
                        string fileName = Path.GetFileNameWithoutExtension(f);
                        string expectedPrefix = $"{gameAbbreviation}AutoSave{routeType}";
                        return fileName.StartsWith(expectedPrefix) && fileName != expectedPrefix;
                    })
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToList();

                foreach (string backupFile in backupFiles)
                {
                    if (TryLoadAutosaveFile(backupFile, routeGrid))
                    {
                        Debug.WriteLine($"Loaded autosave from backup: {Path.GetFileName(backupFile)}");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading from backups: {ex.Message}");
                return false;
            }
        }

        // ==========FORMAL COMMENT=========
        // Attempts to load and apply autosave data from a specific file
        // Handles both new tuple format and legacy format for compatibility
        // ==========MY NOTES==============
        // The actual file loading logic that works with any autosave file
        // Handles different save formats for backwards compatibility
        private bool TryLoadAutosaveFile(string filePath, DataGridView routeGrid)
        {
            try
            {
                string json = File.ReadAllText(filePath);

                try
                {
                    // Try new tuple format first
                    var entryStatus = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, (bool IsCompleted, bool IsSkipped)>>(json);

                    foreach (var entry in routeEntries!)
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
                    // Fallback to legacy format
                    var completionStatus = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(json);

                    foreach (var entry in routeEntries!)
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
                    if (entry.IsSkipped)
                        continue;

                    string completionMark = entry.IsCompleted ? "X" : "";
                    int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                    routeGrid.Rows[rowIndex].Tag = entry;
                    anyChanges = true;
                }

                if (anyChanges)
                {
                    SortRouteGridByCompletion(routeGrid);
                    ScrollToFirstIncomplete(routeGrid);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading autosave file {filePath}: {ex.Message}");
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
            // Get current states from the game through base class method (works with any game)
            (bool IsLoading, bool IsMainMenu) = gameConnectionManager?.GameStats?.GetGameStatus() ?? (false, false);

            bool isCurrentlyInNonGameplay = IsLoading || IsMainMenu;

            // Entering loading or menu from gameplay
            if (isCurrentlyInNonGameplay && !wasPreviouslyInNonGameplay)
            {
                string gameName = gameConnectionManager?.GameStats?.GetType().Name ?? "Game";
                Debug.WriteLine($"{gameName}: Entering non-gameplay state (Loading: {IsLoading}, Menu: {IsMainMenu}) - saving progress");
                AutoSaveProgress();
            }
            // Returning to gameplay from loading or menu
            else if (!isCurrentlyInNonGameplay && wasPreviouslyInNonGameplay)
            {
                string gameName = gameConnectionManager?.GameStats?.GetType().Name ?? "Game";
                Debug.WriteLine($"{gameName}: Returning to gameplay - loading saved progress");
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

                    SortRouteGridByCompletion(routeGrid);
                    ScrollToFirstIncomplete(routeGrid);

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
            SortRouteGridByCompletion(routeGrid);
            ScrollToFirstIncomplete(routeGrid);
        }
        #endregion

        #region completion percentage
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
        #endregion
    }
}