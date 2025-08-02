using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    public static class RouteHelpers
    {
        #region Route Data Management (from RouteDataManager.cs)
        // ==========MY NOTES==============
        // Runs when the user clicks Save Progress (legacy - moved to context menu)
        public static void SaveProgress(RouteManager? routeManager, MainForm mainForm)
        {
            routeManager?.SaveProgress(mainForm);
        }

        // ==========MY NOTES==============
        // Runs when the user clicks Load Progress (legacy - moved to context menu)
        public static void LoadProgress(ref RouteManager? routeManager, DataGridView routeGrid, MainForm mainForm)
        {
            routeManager ??= new RouteManager(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes", "AC4 100 % Route - Main Route.tsv"),
                new GameConnectionManager()
            );

            if (routeManager.LoadEntries().Count == 0)
            {
                MessageBox.Show("No route entries found. Make sure the route file exists.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (routeManager.LoadProgress(routeGrid, mainForm))
            {
                MessageBox.Show("Progress loaded successfully.", "Load Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ==========MY NOTES==============
        // Resets all progress when you click Reset Progress (legacy - moved to context menu)
        public static void ResetProgress(RouteManager? routeManager, DataGridView? routeGrid)
        {
            if (routeManager == null || routeGrid == null)
                return;

            var result = MessageBox.Show(
                "Are you sure you want to reset your progress?\n\nThis will delete your autosave and cannot be undone.",
                "Reset Progress",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                routeManager.ResetProgress(routeGrid);
            }
        }

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
                routeManager.LoadRouteDataIntoGrid(routeGrid, selectedGame);

                // Check if we have entries loaded
                var entries = routeManager.LoadEntries();
                if (entries.Count > 0)
                {
                    // Suspend layout before modifying routeGrid
                    routeGrid.SuspendLayout();

                    // Clear and show actual entries
                    routeGrid.Rows.Clear();

                    foreach (var entry in entries)
                    {
                        string completionMark = entry.IsCompleted ? "X" : "";
                        int rowIndex = routeGrid.Rows.Add(entry.DisplayText, completionMark);
                        routeGrid.Rows[rowIndex].Tag = entry;
                    }

                    // Store all entries for filtering
                    mainForm.allRouteEntries.Clear();
                    foreach (DataGridViewRow row in routeGrid.Rows)
                    {
                        if (row.Tag is RouteEntry entry)
                        {
                            mainForm.allRouteEntries.Add(entry);
                        }
                    }

                    PopulateTypeFilter(mainForm);

                    // Calculate completion stats
                    var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                    if (Math.Round(percentage, 2) >= 100.0f)
                        mainForm.completionLabel.Text = "Completion: 100%";
                    else
                        mainForm.completionLabel.Text = $"Completion: {percentage:F2}%";


                    // Check main menu state
                    var gameStats = mainForm.gameConnectionManager.GameStats;

                    // Resume layout after all modifications
                    routeGrid.ResumeLayout(true);

                    // SUCCESS: Show popup notification
                    string routeFileName = System.IO.Path.GetFileName(routeManager.GetRouteFilePath());
                    MessageBox.Show($"Route loaded successfully!\n\nFile: {routeFileName}",
                        "Route Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoggingSystem.LogInfo($"Route loaded successfully: {routeFileName} with {entries.Count} entries");
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

        #region Filtering (from FilterManager.cs)
        public static void UpdateRouteGridWithEntries(MainForm mainForm, List<RouteEntry> entries)
        {
            // Suspend layout to prevent scroll jumping
            mainForm.routeGrid.SuspendLayout();

            try
            {
                mainForm.routeGrid.Rows.Clear();

                foreach (var entry in entries)
                {
                    if (entry.IsSkipped) continue;

                    string completionMark = entry.IsCompleted ? "X" : "";
                    int rowIndex = mainForm.routeGrid.Rows.Add(entry.DisplayText, completionMark);
                    mainForm.routeGrid.Rows[rowIndex].Tag = entry;
                }

                RouteManager.SortRouteGridByCompletion(mainForm.routeGrid);
                SortingManager.ScrollToFirstIncomplete(mainForm.routeGrid);
                UpdateFilteredCompletionStats(mainForm, entries);
            }
            finally
            {
                mainForm.routeGrid.ResumeLayout(true);
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
            gameDropdown.Items.AddRange(["", "Assassin's Creed 4", "God of War 2018"]);
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
    }
}