using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    public static class RouteHelpers
    {
        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        #region Route Data Management (from RouteDataManager.cs)
        // ==========MY NOTES==============
        // Fills the route grid with actual data or shows a message
        // FIXED: Now properly manages loading spinner to prevent conflicts
        public static void LoadRouteData(MainForm mainForm, RouteManager? routeManager, DataGridView routeGrid, SettingsManager settingsManager)
        {
            LoadingHelper.ExecuteWithSpinner(mainForm, () =>
            {
                LoadRouteDataCore(mainForm, routeManager, routeGrid, settingsManager);
            }, "Loading Route Data...");
        }

        // ==========MY NOTES==============
        // Core route loading logic without spinner (for internal use)
        // FIXED: REMOVED all routeGrid.Rows.Add() text - only loads actual route data
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public static void LoadRouteDataCore(MainForm mainForm, RouteManager? routeManager, DataGridView routeGrid, SettingsManager settingsManager)
        {
            if (routeManager != null)
            {
                string selectedGame = GetSelectedGameName(mainForm);

                // Load route data (data-only operation)
                routeManager.LoadRouteDataIntoGrid(routeGrid, selectedGame, mainForm);

                // Check if we have entries loaded
                var entries = routeManager.LoadEntries();
                if (entries.Count > 0)
                {
                    // Store all entries for filtering - NO GRID POPULATION HERE
                    mainForm.allRouteEntries.Clear();
                    mainForm.allRouteEntries.AddRange(entries);

                    PopulateTypeFilter(mainForm);

                    // Calculate completion stats
                    var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                    if (Math.Round(percentage, 2) >= 100.0f)
                        mainForm.completionLabel.Text = "Completion: 100%";
                    else
                        mainForm.completionLabel.Text = $"Completion: {percentage:F2}%";

                    LoggingSystem.LogInfo($"Route loaded successfully: {Path.GetFileName(routeManager.GetRouteFilePath())} with {entries.Count} entries");
                }
                else
                {
                    LoggingSystem.LogError("Failed to load route data: No entries found in route file");
                }
            }
            else
            {
                LoggingSystem.LogError("Failed to load route data: RouteManager is null");
            }
        }

        // ==========MY NOTES==============
        // Called automatically when game stats update to mark entries as complete
        // FIXED: Prevents conflicts with loading operations by checking spinner state
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public static void UpdateRouteCompletionStatus(RouteManager? routeManager, DataGridView routeGrid, GameStatsEventArgs stats, Label completionLabel, SettingsManager settingsManager)
        {
            // FIXED: Don't update if a loading spinner is active to prevent conflicts
            if (IsSpinnerActive(routeGrid.FindForm()))
                return;

            // Suspend layout to prevent scroll jumping during updates
            routeGrid.SuspendLayout();

            try
            {

                bool changed = routeManager?.UpdateCompletionStatus(routeGrid, stats) ?? false;

                // ONLY UPDATE UI IF SOMETHING ACTUALLY CHANGED
                if (changed && routeManager != null)
                {
                    // Update completion label
                    var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                    if (Math.Round(percentage, 2) >= 100.0f)
                        completionLabel.Text = "Completion: 100%";
                    else
                        completionLabel.Text = $"Completion: {percentage:F2}%";

                    UpdateCompletionStatsIfVisible(routeManager);

                    // Apply sorting without triggering additional scroll events
                    /*var currentMode = settingsManager.GetSortingMode();
                    SortingManager.ApplySortingQuiet(routeGrid, currentMode);*/
                }
            }
            finally
            {
                // Always resume layout
                routeGrid.ResumeLayout(true);
            }
        }

        // ==========MY NOTES==============
        // Helper method to check if a loading spinner is currently active
        // Prevents UI operations from interfering with loading operations
        private static bool IsSpinnerActive(Form? form)
        {
            if (form == null) return false;

            // Check if any child control is a LoadingSpinner and is visible
            foreach (Control control in form.Controls)
            {
                if (control is LoadingSpinner spinner && spinner.Visible)
                    return true;
            }
            return false;
        }
        #endregion

        #region Progress Persistence (from RouteManager.cs)
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
        public static void AutoSaveProgress(RouteManager routeManager)
        {
            if (routeManager == null || routeManager.GetRouteEntries().Count == 0)
                return;

            try
            {
                var routeEntries = routeManager.GetRouteEntries();
                string routeFilePath = routeManager.GetRouteFilePath();

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
                string gameAbbreviation = GetGameAbbreviation(routeManager);
                string routeType = GetRouteType(routeManager);

                // Create the cycling autosave with numbered backups
                CreateCyclingAutoSave(saveDir, gameAbbreviation, routeType, entryStatus);

                Debug.WriteLine($"Auto-saved progress with cycling backup system");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error auto-saving progress: {ex.Message}");
            }
        }

        // The core of the cycling backup system
        // Rotates through numbered backups (1-10) and cycles back to 1
        private static void CreateCyclingAutoSave(string saveDir, string gameAbbreviation, string routeType, Dictionary<string, (bool IsCompleted, bool IsSkipped)> entryStatus)
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

        // Figures out which numbered backup to create next
        // Cycles through 1-10 and starts over, giving us rolling backups
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private static int GetNextBackupNumber(string saveDir, string gameAbbreviation, string routeType)
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
        private static string GetGameAbbreviation(RouteManager routeManager)
        {
            string routeFilePath = routeManager.GetRouteFilePath();
            string routeFileName = Path.GetFileNameWithoutExtension(routeFilePath).ToLower();

            // Check route file name for game indicators
            if (routeFileName.Contains("ac4") || (routeFileName.Contains("assassin") && routeFileName.Contains("creed") && routeFileName.Contains("4")))
                return "AC4";
            if (routeFileName.Contains("gow") || (routeFileName.Contains("god") && routeFileName.Contains("war")))
                return "GoW";
            if (routeFileName.Contains("tr13") || (routeFileName.Contains("tomb") && routeFileName.Contains("raider")))
                return "TR13";

            // Try to get from game connection if available
            var gameConnectionManager = routeManager.GetGameConnectionManager();
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
        private static string GetRouteType(RouteManager routeManager)
        {
            string routeFilePath = routeManager.GetRouteFilePath();
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

        // Enhanced autosave loading that can find backups if main file is corrupted
        // Tries the main autosave first, then looks for numbered backups
        public static bool LoadAutoSave(RouteManager routeManager, DataGridView routeGrid)
        {
            var routeEntries = routeManager.GetRouteEntries();
            if (routeEntries == null || routeEntries.Count == 0 || routeGrid == null)
                return false;

            try
            {
                string routeFilePath = routeManager.GetRouteFilePath();
                string saveDir = Path.Combine(
                    Path.GetDirectoryName(routeFilePath) ?? AppDomain.CurrentDomain.BaseDirectory,
                    "SavedProgress");

                if (!Directory.Exists(saveDir))
                    return false;

                // Get file naming info
                string gameAbbreviation = GetGameAbbreviation(routeManager);
                string routeType = GetRouteType(routeManager);
                string baseFileName = $"{gameAbbreviation}AutoSave{routeType}.json";
                string mainAutosaveFile = Path.Combine(saveDir, baseFileName);

                // Try to load main autosave first
                if (File.Exists(mainAutosaveFile))
                {
                    if (TryLoadAutosaveFile(mainAutosaveFile, routeManager, routeGrid))
                    {
                        Debug.WriteLine($"Loaded autosave from {baseFileName}");
                        return true;
                    }
                }

                // If main autosave failed, try numbered backups (newest first)
                return TryLoadFromBackups(saveDir, gameAbbreviation, routeType, routeManager, routeGrid);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading autosave: {ex.Message}");
                return false;
            }
        }

        // Fallback system that looks for working backup files
        // Starts with the most recent backup and works backwards
        private static bool TryLoadFromBackups(string saveDir, string gameAbbreviation, string routeType, RouteManager routeManager, DataGridView routeGrid)
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
                    if (TryLoadAutosaveFile(backupFile, routeManager, routeGrid))
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

        // The actual file loading logic that works with any autosave file
        // Handles different save formats for backwards compatibility
        private static bool TryLoadAutosaveFile(string filePath, RouteManager routeManager, DataGridView routeGrid)
        {
            try
            {
                var routeEntries = routeManager.GetRouteEntries();
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

                // Update UI - handle both virtual and traditional modes
                bool anyChanges = false;

                if (routeGrid.VirtualMode)
                {
                    // For virtual mode, just refresh the display
                    routeGrid.RowCount = routeEntries.Count(e => !e.IsSkipped);
                    routeGrid.Invalidate();
                    anyChanges = true;
                }
                else
                {
                    // Traditional mode - rebuild rows
                    routeGrid.Rows.Clear();
                    foreach (var entry in routeEntries)
                    {
                        if (entry.IsSkipped)
                            continue;

                        string completionMark = entry.IsCompleted ? "X" : "";
                        int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                        routeGrid.Rows[rowIndex].Tag = entry;
                        anyChanges = true;
                    }
                }

                if (anyChanges)
                {
                    RouteManager.SortRouteGridByCompletion(routeGrid);
                    SortingManager.ScrollToFirstIncomplete(routeGrid);
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

        // Detects when the game is loading, in menu, or in active gameplay
        // Preserves progress across these state changes
        public static void HandleGameStateTransition(RouteManager routeManager, DataGridView routeGrid)
        {
            var gameConnectionManager = routeManager.GetGameConnectionManager();

            // Get current states from the game through base class method (works with any game)
            (bool IsLoading, bool IsMainMenu) = gameConnectionManager?.GameStats?.GetGameStatus() ?? (false, false);

            bool isCurrentlyInNonGameplay = IsLoading || IsMainMenu;
            bool wasPreviouslyInNonGameplay = routeManager.GetWasPreviouslyInNonGameplay();

            // Entering loading or menu from gameplay
            if (isCurrentlyInNonGameplay && !wasPreviouslyInNonGameplay)
            {
                string gameName = gameConnectionManager?.GameStats?.GetType().Name ?? "Game";
                Debug.WriteLine($"{gameName}: Entering non-gameplay state (Loading: {IsLoading}, Menu: {IsMainMenu}) - saving progress");
                AutoSaveProgress(routeManager);
            }
            // Returning to gameplay from loading or menu
            else if (!isCurrentlyInNonGameplay && wasPreviouslyInNonGameplay)
            {
                string gameName = gameConnectionManager?.GameStats?.GetType().Name ?? "Game";
                Debug.WriteLine($"{gameName}: Returning to gameplay - loading saved progress");
                LoadAutoSave(routeManager, routeGrid);
            }

            // Update previous state
            routeManager.SetWasPreviouslyInNonGameplay(isCurrentlyInNonGameplay);
        }

        // Saves your progress to any file name you choose
        // Lets you create multiple save files for different segments or runs
        public static void SaveProgressToFile(RouteManager routeManager, Form parentForm)
        {
            var routeEntries = routeManager.GetRouteEntries();
            if (routeEntries == null || routeEntries.Count == 0)
                return;

            try
            {
                string routeFilePath = routeManager.GetRouteFilePath();

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
                    AutoSaveProgress(routeManager);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving progress: {ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Lets you pick any saved file to load progress from
        // Updates the display to show loaded completion status
        // FIXED: Now supports both manual save format and autosave format
        public static bool LoadProgressFromFile(RouteManager routeManager, DataGridView routeGrid, Form parentForm)
        {
            var routeEntries = routeManager.GetRouteEntries();
            if (routeEntries == null || routeEntries.Count == 0 || routeGrid == null)
                return false;

            try
            {
                string routeFilePath = routeManager.GetRouteFilePath();

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

                // Load and apply saved data - handle both formats
                string json = File.ReadAllText(openDialog.FileName);

                bool anyChanges = false;

                try
                {
                    // Try autosave format first (tuple format)
                    var entryStatus = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, (bool IsCompleted, bool IsSkipped)>>(json);

                    if (entryStatus != null && entryStatus.Count > 0)
                    {
                        foreach (var entry in routeEntries)
                        {
                            string key = $"{entry.Id}_{entry.Name}_{entry.Type}_{entry.Condition}";
                            if (entryStatus.TryGetValue(key, out var status))
                            {
                                if (entry.IsCompleted != status.IsCompleted || entry.IsSkipped != status.IsSkipped)
                                {
                                    entry.IsCompleted = status.IsCompleted;
                                    entry.IsSkipped = status.IsSkipped;
                                    anyChanges = true;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback to manual save format (boolean format)
                    var completionStatus = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(json);

                    if (completionStatus != null && completionStatus.Count > 0)
                    {
                        foreach (var entry in routeEntries)
                        {
                            string key = $"{entry.Name}_{entry.Type}_{entry.Condition}";
                            if (completionStatus.TryGetValue(key, out bool isCompleted))
                            {
                                if (entry.IsCompleted != isCompleted)
                                {
                                    entry.IsCompleted = isCompleted;
                                    entry.IsSkipped = false; // Manual saves don't track skipped status
                                    anyChanges = true;
                                }
                            }
                        }
                    }
                }

                // Update UI if needed - handle both virtual and traditional modes
                if (anyChanges)
                {
                    if (routeGrid.VirtualMode)
                    {
                        // For virtual mode, just refresh the display
                        routeGrid.RowCount = routeEntries.Count(e => !e.IsSkipped);
                        routeGrid.Invalidate();
                    }
                    else
                    {
                        // Traditional mode - rebuild rows
                        routeGrid.Rows.Clear();
                        foreach (var entry in routeEntries)
                        {
                            if (entry.IsSkipped)
                                continue;

                            string completionMark = entry.IsCompleted ? "X" : "";
                            int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                            routeGrid.Rows[rowIndex].Tag = entry;
                        }
                    }

                    RouteManager.SortRouteGridByCompletion(routeGrid);
                    SortingManager.ScrollToFirstIncomplete(routeGrid);

                    // After loading manually, update autosave as well
                    AutoSaveProgress(routeManager);
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

        // ==========MY NOTES==============
        // Shows the save location folder and opens it in Explorer
        public static void ShowSaveLocation(RouteManager routeManager, Form parentForm)
        {
            string routeFilePath = routeManager.GetRouteFilePath();
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
        public static void ResetProgressData(RouteManager routeManager, DataGridView routeGrid)
        {
            var routeEntries = routeManager.GetRouteEntries();
            string routeFilePath = routeManager.GetRouteFilePath();

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

            // Try to delete both old and new autosave formats
            string routeName = Path.GetFileNameWithoutExtension(routeFilePath);
            string oldAutosaveFile = Path.Combine(saveDir, $"{routeName}_AutoSave.json");
            if (File.Exists(oldAutosaveFile))
            {
                File.Delete(oldAutosaveFile);
            }

            // Delete new format autosave files
            try
            {
                string gameAbbreviation = GetGameAbbreviationForReset(routeManager);
                string routeType = GetRouteTypeForReset(routeManager);
                string newAutosaveFile = Path.Combine(saveDir, $"{gameAbbreviation}AutoSave{routeType}.json");
                if (File.Exists(newAutosaveFile))
                {
                    File.Delete(newAutosaveFile);
                }

                // Also delete numbered backups
                for (int i = 1; i <= 10; i++)
                {
                    string backupFile = Path.Combine(saveDir, $"{gameAbbreviation}AutoSave{routeType}{i}.json");
                    if (File.Exists(backupFile))
                    {
                        File.Delete(backupFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warning: Could not delete all autosave files: {ex.Message}");
            }

            // Refresh the UI - handle both virtual and traditional modes
            if (routeGrid.VirtualMode)
            {
                // For virtual mode, just refresh the display
                routeGrid.RowCount = routeEntries.Count;
                routeGrid.Invalidate();

                // Apply current sorting and scroll to first incomplete
                if (routeGrid.FindForm() is MainForm mainForm)
                {
                    mainForm.ApplyCurrentSorting();
                }
                else
                {
                    SortingManager.ScrollToFirstIncomplete(routeGrid);
                }
            }
            else
            {
                // Traditional mode - rebuild rows
                routeGrid.Rows.Clear();
                foreach (var entry in routeEntries)
                {
                    int rowIndex = routeGrid.Rows.Add(entry.DisplayText, "");
                    routeGrid.Rows[rowIndex].Tag = entry;
                }
                RouteManager.SortRouteGridByCompletion(routeGrid);
                SortingManager.ScrollToFirstIncomplete(routeGrid);
            }
        }

        // ==========MY NOTES==============
        // Helper methods for reset progress to determine autosave file names
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1847",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "NO")]
        private static string GetGameAbbreviationForReset(RouteManager routeManager)
        {
            string routeFilePath = routeManager.GetRouteFilePath();
            string routeFileName = Path.GetFileNameWithoutExtension(routeFilePath).ToLower();

            // Check route file name for game indicators
            if (routeFileName.Contains("ac4") || (routeFileName.Contains("assassin") && routeFileName.Contains("creed") && routeFileName.Contains("4")))
                return "AC4";
            if (routeFileName.Contains("gow") || (routeFileName.Contains("god") && routeFileName.Contains("war")))
                return "GoW";
            if (routeFileName.Contains("tr13") || (routeFileName.Contains("tomb") && routeFileName.Contains("raider")))
                return "TR13";

            // Try to get from game connection if available
            var gameConnectionManager = routeManager.GetGameConnectionManager();
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

        private static string GetRouteTypeForReset(RouteManager routeManager)
        {
            string routeFilePath = routeManager.GetRouteFilePath();
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
        #endregion

        #region Filtering (from FilterManager.cs)
        public static void UpdateRouteGridWithEntries(MainForm mainForm, List<RouteEntry> entries)
        {
            if (mainForm.GetRouteManager() is RouteManager routeManager)
            {
                var filteredEntries = entries.Where(e => !e.IsSkipped).ToList();

                // Update the route manager's data source
                routeManager.UpdateRouteEntries(filteredEntries);

                // Update virtual grid display
                mainForm.routeGrid.RowCount = filteredEntries.Count;
                mainForm.routeGrid.Invalidate();

                UpdateFilteredCompletionStats(mainForm, entries);
            }
        }

        // ==========MY NOTES==============
        // The main filtering logic that decides which route entries to show
        public static void ApplyFilters(MainForm mainForm)
        {
            if (mainForm.allRouteEntries.Count == 0) return;

            var filteredEntries = mainForm.allRouteEntries.AsEnumerable();

            // Apply search filter
            string searchText = mainForm.searchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredEntries = filteredEntries.Where(entry =>
                    entry.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    entry.Type.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    entry.Coordinates.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            // Apply multi-type filter
            if (mainForm.selectedTypes.Count > 0 && !mainForm.typeFilterCheckedListBox.GetItemChecked(0))
            {
                filteredEntries = filteredEntries.Where(entry =>
                    mainForm.selectedTypes.Contains(entry.Type, StringComparer.OrdinalIgnoreCase));
            }

            // Suspend layout before updating grid
            mainForm.routeGrid.SuspendLayout();
            try
            {
                UpdateRouteGridWithEntries(mainForm, [.. filteredEntries]);
            }
            finally
            {
                mainForm.routeGrid.ResumeLayout(true);
            }
        }

        // ==========MY NOTES==============
        // Clears both search and type filter when you click the Clear button
        public static void ClearFilters(MainForm mainForm)
        {
            // Save current search to history before clearing
            if (!string.IsNullOrEmpty(mainForm.searchTextBox.Text.Trim()))
            {
                string currentSearch = mainForm.searchTextBox.Text.Trim();
                var searchHistoryManager = new SearchHistoryManager();
                _ = searchHistoryManager.AddSearchHistoryAsync(currentSearch);
            }

            mainForm.searchTextBox.Text = "";

            // Clear all type selections and set "All Types" as selected
            mainForm.selectedTypes.Clear();
            for (int i = 0; i < mainForm.typeFilterCheckedListBox.Items.Count; i++)
            {
                mainForm.typeFilterCheckedListBox.SetItemChecked(i, i == 0);
            }
            if (mainForm.typeFilterCheckedListBox.Items.Count > 1)
            {
                for (int i = 1; i < mainForm.typeFilterCheckedListBox.Items.Count; i++)
                {
                    mainForm.selectedTypes.Add(mainForm.typeFilterCheckedListBox.Items[i].ToString() ?? "");
                }
            }

            // Update the label
            var typeFilterPanel = mainForm.typeFilterCheckedListBox.Parent;
            if (typeFilterPanel != null)
            {
                var typeFilterLabel = typeFilterPanel.Controls.OfType<Label>().FirstOrDefault();
                if (typeFilterLabel != null)
                {
                    typeFilterLabel.Text = "All Types";
                }
            }

            // Suspend layout before applying filters
            mainForm.routeGrid.SuspendLayout();
            try
            {
                ApplyFilters(mainForm);
            }
            finally
            {
                mainForm.routeGrid.ResumeLayout(true);
            }
        }

        // ==========MY NOTES==============
        // Fills the type dropdown with all the different types found in the route
        public static void PopulateTypeFilter(MainForm mainForm)
        {
            mainForm.typeFilterCheckedListBox.SuspendLayout();
            try
            {
                var uniqueTypes = mainForm.allRouteEntries
                    .Select(e => e.Type)
                    .Where(type => !string.IsNullOrEmpty(type))
                    .Distinct()
                    .OrderBy(type => type)
                    .ToList();

                mainForm.typeFilterCheckedListBox.Items.Clear();
                mainForm.typeFilterCheckedListBox.Items.Add("All Types");

                foreach (string type in uniqueTypes)
                {
                    mainForm.typeFilterCheckedListBox.Items.Add(type);
                }

                // Set "All Types" as checked and populate selectedTypes
                mainForm.typeFilterCheckedListBox.SetItemChecked(0, true);
                mainForm.selectedTypes.Clear();
                foreach (string type in uniqueTypes)
                {
                    mainForm.selectedTypes.Add(type);
                }

                // Update the label
                var typeFilterPanel = mainForm.typeFilterCheckedListBox.Parent;
                if (typeFilterPanel != null)
                {
                    var typeFilterLabel = typeFilterPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (typeFilterLabel != null)
                    {
                        typeFilterLabel.Text = "All Types";
                    }
                }
            }
            finally
            {
                mainForm.typeFilterCheckedListBox.ResumeLayout(true);
            }
        }

        // ==========MY NOTES==============
        // Updates the completion percentage label to reflect the filtered results
        public static void UpdateFilteredCompletionStats(MainForm mainForm, List<RouteEntry> filteredEntries)
        {
            if (filteredEntries.Count == 0)
            {
                mainForm.completionLabel.Text = "Completion: 0.00%";
                return;
            }

            int totalEntries = filteredEntries.Count(e => !e.IsSkipped);
            int completedEntries = filteredEntries.Count(e => e.IsCompleted && !e.IsSkipped);

            float percentage = totalEntries > 0
                ? (float)Math.Round((float)completedEntries / totalEntries * 100, 2)
                : 0.00f;

            if (filteredEntries.Count != mainForm.allRouteEntries.Count(e => !e.IsSkipped))
            {
                mainForm.completionLabel.Text = $"Completion: {percentage:F2}% ({completedEntries}/{totalEntries} filtered)";
            }
            else
            {
                //mainForm.completionLabel.Text = $"Completion: {percentage:F2}%";
                if (Math.Round(percentage, 2) >= 100.0f)
                    mainForm.completionLabel.Text = "Completion: 100%";
                else
                    mainForm.completionLabel.Text = $"Completion: {percentage:F2}%";
            }
        }
        #endregion

        #region Game Connection (from GameConnectionHelper.cs)
        // ==========MY NOTES==============
        // Gets whatever game is currently selected in the dropdown
        public static string GetSelectedGameName(MainForm mainForm)
        {
            if (mainForm.MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault() is ToolStripComboBox gameDropdown)
            {
                return gameDropdown.SelectedItem?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        // ==========MY NOTES==============
        // Auto-detects running games and updates UI accordingly
        public static void AutoDetectButton_Click(MainForm mainForm, GameConnectionManager gameConnectionManager)
        {
            ToolStripComboBox? gameDropdown = mainForm.MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault();
            ToolStripLabel? connectionLabel = mainForm.MainMenuStrip?.Items.OfType<ToolStripLabel>().FirstOrDefault();

            if (gameDropdown == null || connectionLabel == null)
            {
                MessageBox.Show("Required controls are missing.");
                return;
            }

            string detectedGame = gameConnectionManager.DetectRunningGame();

            if (!string.IsNullOrEmpty(detectedGame))
            {
                gameDropdown.SelectedItem = detectedGame;
                connectionLabel.Text = $"Detected: {detectedGame}";
            }
            else
            {
                connectionLabel.Text = "No supported games detected";
            }
        }

        // ==========MY NOTES==============
        // Old connection button handler - moved to new Connect to Game button
        public static async Task ConnectButton_Click(MainForm mainForm, GameConnectionManager gameConnectionManager, SettingsManager settingsManager)
        {
            ToolStripComboBox? gameDropdown = mainForm.MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault();
            ToolStripLabel? connectionLabel = mainForm.MainMenuStrip?.Items.OfType<ToolStripLabel>().FirstOrDefault();

            if (gameDropdown == null || connectionLabel == null)
            {
                MessageBox.Show("Required controls are missing.");
                return;
            }

            string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(selectedGame))
            {
                selectedGame = gameConnectionManager.DetectRunningGame();
                if (!string.IsNullOrEmpty(selectedGame))
                {
                    gameDropdown.SelectedItem = selectedGame;
                    connectionLabel.Text = $"Auto-detected: {selectedGame}";
                }
                else
                {
                    connectionLabel.Text = "Please select a game.";
                    return;
                }
            }

            string gameDirectory = settingsManager.GetGameDirectory(selectedGame);

            if (string.IsNullOrEmpty(gameDirectory))
            {
                connectionLabel.Text = "Game directory not set.";
                return;
            }

            mainForm.gameDirectoryTextBox.Text = gameDirectory;

            bool connected = await gameConnectionManager.ConnectToGameAsync(selectedGame, mainForm.enableAutoStartMenuItem?.Checked == true);

            if (connected)
            {
                connectionLabel.Text = $"Connected to {selectedGame}";
                string routeFilePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Routes",
                    "AC4 100 % Route - Main Route.tsv");

                mainForm.SetRouteManager(new RouteManager(routeFilePath, gameConnectionManager));
                mainForm.LoadRouteDataPublic();
            }
            else
            {
                connectionLabel.Text = "Error: Cannot connect to process. Make sure the game is running.";
            }
        }

        // ==========MY NOTES==============
        // Legacy method for creating old connection controls - not used anymore
        [SupportedOSPlatform("windows6.1")]
        public static void CreateConnectionControls(MainForm mainForm, MenuStrip menuStrip, GameConnectionManager gameConnectionManager)
        {
            ToolStripLabel connectionLabel = new()
            {
                Text = "Not connected"
            };
            menuStrip.Items.Add(connectionLabel);

            ToolStripComboBox gameDropdown = new();
            gameDropdown.Items.Add("");
            gameDropdown.Items.AddRange([.. SupportedGames.GameList.Values.Select(g => g.DisplayName)]);
            gameDropdown.SelectedIndex = 0;
            menuStrip.Items.Add(gameDropdown);

            ToolStripButton autoDetectButton = new("Auto-Detect");
            autoDetectButton.Click += (s, e) => AutoDetectButton_Click(mainForm, gameConnectionManager);
            menuStrip.Items.Add(autoDetectButton);

            ToolStripButton connectButton = new("Connect to Game");
            connectButton.Click += async (s, e) => await ConnectButton_Click(mainForm, gameConnectionManager, new SettingsManager());
            menuStrip.Items.Add(connectButton);
        }

        // ==========MY NOTES==============
        // This catches the stats when they update automatically and updates the UI
        public static void GameStats_StatsUpdated(MainForm mainForm, GameStatsEventArgs e)
        {

            var gameStats = mainForm.gameConnectionManager.GameStats;

            mainForm.Invoke(() =>
            {
                if (mainForm.routeGrid != null)
                {
                    mainForm.UpdateRouteCompletionStatusPublic(e);
                }

                UpdateStatsWindowIfVisible(e);
            });
        }

        // ==========MY NOTES==============
        // This stops the automatic stat updates when we're done
        public static void CleanupGameStats(GameConnectionManager gameConnectionManager)
        {
            gameConnectionManager?.CleanupGameStats();
        }
        #endregion

        #region Window Management (from WindowManager.cs)
        private static StatsWindow? statsWindow;
        private static CompletionStatsWindow? completionStatsWindow;

        // ==========MY NOTES==============
        // Shows the window with all current game stats when you click Game Stats
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public static void ShowStatsWindow(MainForm mainForm, GameConnectionManager gameConnectionManager)
        {
            if (statsWindow == null || statsWindow.IsDisposed)
                statsWindow = new StatsWindow();

            if (gameConnectionManager.GameStats is IGameStats stats)
            {
                var statsDict = stats.GetStatsAsDictionary();
                string statsText = BuildStatsText(statsDict);
                statsWindow.UpdateStats(statsText);
            }
            else
            {
                statsWindow.UpdateStats("No stats available. Connect to a game first.");
            }

            statsWindow.Show();
            statsWindow.BringToFront();
        }

        // ==========MY NOTES==============
        // Shows the window with route completion stats when you click Route Stats
        public static void ShowCompletionStatsWindow(MainForm mainForm, RouteManager? routeManager)
        {
            if (completionStatsWindow == null || completionStatsWindow.IsDisposed)
            {
                completionStatsWindow = new CompletionStatsWindow
                {
                    Owner = mainForm
                };

                if (mainForm.TopMost)
                {
                    completionStatsWindow.TopMost = true;
                }
            }

            if (routeManager != null)
            {
                var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                completionStatsWindow.UpdateStats(percentage, completed, total);
            }
            else
            {
                completionStatsWindow.UpdateStats(0.00f, 0, 0);
            }

            completionStatsWindow.Show();
            completionStatsWindow.BringToFront();
            completionStatsWindow.Focus();
        }

        // ==========MY NOTES==============
        // Builds the formatted stats text for display in the stats window
        private static string BuildStatsText(Dictionary<string, object> statsDict)
        {
            float exactPercentage = 0f;
            if (statsDict.TryGetValue("Exact Percentage", out var obj))
            {
                if (obj is float f)
                    exactPercentage = f;
                else if (obj is double d)
                    exactPercentage = (float)d;
                else if (obj is string s && float.TryParse(s, out var parsed))
                    exactPercentage = parsed;
                else if (obj is int i)
                    exactPercentage = i;
            }

            string exactPercentageText;
            if (Math.Round(exactPercentage, 2) >= 100.0f)
                exactPercentageText = "100%";
            else
                exactPercentageText = $"{exactPercentage:F2}%";

            return $"Completion Percentage: {statsDict.GetValueOrDefault("Completion Percentage", 0)}%\n" +
                   $"Completion Percentage Exact: {exactPercentageText}\n" +
                   $"Viewpoints Completed: {statsDict.GetValueOrDefault("Viewpoints", 0)}\n" +
                   $"Myan Stones Collected: {statsDict.GetValueOrDefault("Myan Stones", 0)}\n" +
                   $"Buried Treasure Collected: {statsDict.GetValueOrDefault("Buried Treasure", 0)}\n" +
                   $"AnimusFragments Collected: {statsDict.GetValueOrDefault("Animus Fragments", 0)}\n" +
                   $"AssassinContracts Completed: {statsDict.GetValueOrDefault("Assassin Contracts", 0)}\n" +
                   $"NavalContracts Completed: {statsDict.GetValueOrDefault("Naval Contracts", 0)}\n" +
                   $"LetterBottles Collected: {statsDict.GetValueOrDefault("Letter Bottles", 0)}\n" +
                   $"Manuscripts Collected: {statsDict.GetValueOrDefault("Manuscripts", 0)}\n" +
                   $"Music Sheets Collected: {statsDict.GetValueOrDefault("Music Sheets", 0)}\n" +
                   $"Forts Captured: {statsDict.GetValueOrDefault("Forts", 0)}\n" +
                   $"Taverns unlocked: {statsDict.GetValueOrDefault("Taverns", 0)}\n" +
                   $"Total Chests Collected: {statsDict.GetValueOrDefault("Chests", 0)}\n" +
                   $"Story Missions Completed: {statsDict.GetValueOrDefault("Story Missions", 0)}\n" +
                   $"Templar Hunts Completed: {statsDict.GetValueOrDefault("Templar Hunts", 0)}\n" +
                   $"Legendary Ships Defeated: {statsDict.GetValueOrDefault("Legendary Ships", 0)}\n" +
                   $"Treasure Maps Collected: {statsDict.GetValueOrDefault("Treasure Maps", 0)}";
        }

        // ==========MY NOTES==============
        // Updates the completion stats window if it's open and visible
        public static void UpdateCompletionStatsIfVisible(RouteManager? routeManager)
        {
            if (completionStatsWindow != null && !completionStatsWindow.IsDisposed && completionStatsWindow.Visible && routeManager != null)
            {
                var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                completionStatsWindow.UpdateStats(percentage, completed, total);
            }
        }

        // ==========MY NOTES==============
        // Updates the stats window with current game stats if it's visible
        public static void UpdateStatsWindowIfVisible(GameStatsEventArgs e)
        {
            if (statsWindow != null && statsWindow.Visible)
            {
                // Safely extract "Exact Percentage" as float
                float exactPercentage = 0f;
                var obj = e.GetValue<object>("Exact Percentage", 0f);
                if (obj is float f)
                    exactPercentage = f;
                else if (obj is double d)
                    exactPercentage = (float)d;
                else if (obj is string s && float.TryParse(s, out var parsed))
                    exactPercentage = parsed;
                else if (obj is int i)
                    exactPercentage = i;

                string exactPercentageText;
                if (Math.Round(exactPercentage, 2) >= 100.0f)
                    exactPercentageText = "100%";
                else
                    exactPercentageText = $"{exactPercentage:F2}%";

                string statsText =
                    $"Completion Percentage: {e.GetValue<int>("Completion Percentage", 0)}%\n" +
                    $"Completion Percentage Exact: {exactPercentageText}\n" +
                    $"Viewpoints Completed: {e.GetValue<int>("Viewpoints", 0)}\n" +
                    $"Myan Stones Collected: {e.GetValue<int>("Myan Stones", 0)}\n" +
                    $"Buried Treasure Collected: {e.GetValue<int>("Buried Treasure", 0)}\n" +
                    $"AnimusFragments Collected: {e.GetValue<int>("Animus Fragments", 0)}\n" +
                    $"AssassinContracts Completed: {e.GetValue<int>("Assassin Contracts", 0)}\n" +
                    $"NavalContracts Completed: {e.GetValue<int>("Naval Contracts", 0)}\n" +
                    $"LetterBottles Collected: {e.GetValue<int>("Letter Bottles", 0)}\n" +
                    $"Manuscripts Collected: {e.GetValue<int>("Manuscripts", 0)}\n" +
                    $"Music Sheets Collected: {e.GetValue<int>("Music Sheets", 0)}\n" +
                    $"Forts Captured: {e.GetValue<int>("Forts", 0)}\n" +
                    $"Taverns unlocked: {e.GetValue<int>("Taverns", 0)}\n" +
                    $"Total Chests Collected: {e.GetValue<int>("Chests", 0)}\n" +
                    $"Story Missions Completed: {e.GetValue<int>("Story Missions", 0)}\n" +
                    $"Templar Hunts Completed: {e.GetValue<int>("Templar Hunts", 0)}\n" +
                    $"Legendary Ships Defeated: {e.GetValue<int>("Legendary Ships", 0)}\n" +
                    $"Treasure Maps Collected: {e.GetValue<int>("Treasure Maps", 0)}";

                statsWindow.UpdateStats(statsText);
            }
        }
        #endregion

        // ==========MY NOTES==============
        // NEW: Simple auto-connect on startup - tries once, no retries, no multiple game handling
        // If exactly one supported game is running, tries to connect once and that's it
        public static async Task TryAutoConnectOnStartup(MainForm mainForm, GameConnectionManager gameConnectionManager, SettingsManager settingsManager)
        {
            try
            {
                // DetectRunningGame returns the first supported game it finds, or empty string
                string detectedGame = gameConnectionManager.DetectRunningGame();

                // If no supported games detected, skip
                if (string.IsNullOrEmpty(detectedGame))
                {
                    LoggingSystem.LogInfo("No supported games detected running - skipping auto-connect");
                    return;
                }

                // Check if we have multiple games running by checking processes directly
                var supportedGameProcesses = new Dictionary<string, string>
                {
                    { "AC4BFSP", "Assassin's Creed 4" },
                    { "GoW", "God of War 2018" }
                };

                int runningCount = 0;
                foreach (var game in supportedGameProcesses)
                {
                    if (GameConnectionManager.IsProcessRunning(game.Key + ".exe"))
                    {
                        runningCount++;
                    }
                }

                // If multiple supported games are running, don't bother
                if (runningCount > 1)
                {
                    LoggingSystem.LogInfo($"Multiple supported games running ({runningCount}) - skipping auto-connect");
                    return;
                }

                LoggingSystem.LogInfo($"Found exactly 1 supported game running: {detectedGame} - attempting auto-connect");

                // Check if game directory is set
                string gameDirectory = settingsManager.GetGameDirectory(detectedGame);
                if (string.IsNullOrEmpty(gameDirectory))
                {
                    LoggingSystem.LogInfo($"Game directory not set for {detectedGame} - skipping auto-connect");
                    return;
                }

                // Try to connect exactly once - no retries
                bool connected = await gameConnectionManager.ConnectToGameAsync(detectedGame, false);

                if (connected)
                {
                    LoggingSystem.LogInfo($"Successfully auto-connected to {detectedGame} on startup");

                    // Update UI to show connection
                    if (mainForm.MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault() is ToolStripComboBox gameDropdown)
                    {
                        gameDropdown.SelectedItem = detectedGame;
                    }
                }
                else
                {
                    LoggingSystem.LogInfo($"Failed to auto-connect to {detectedGame} on startup - skipping");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError($"Error during startup auto-connect: {ex.Message}", ex);
            }
        }
    }
}