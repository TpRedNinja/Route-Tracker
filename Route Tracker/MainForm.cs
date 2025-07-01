using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
using Route_Tracker.Properties;
using System.Net.Http;
using System.Text.Json;
using System.IO.Compression;

namespace Route_Tracker
{
    // static class so version number can be displayed and so we can get updates
    public static class AppInfo
    {
        public const string Version = "v0.4-Beta";
        public const string GitHubRepo = "TpRedNinja/Route-Tracker"; // e.g. "myuser/RouteTracker"
    }

    // Helper extension method for safe dictionary lookups
    public static class DictionaryExtensions
    {
        public static T GetValueOrDefault<T>(this Dictionary<string, object> dict, string key, T defaultValue)
        {
            if (dict.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }
    }

    // ==========FORMAL COMMENT=========
    // Main form class that provides the user interface for the Assassin's Creed Route Tracker
    // Handles connections to game processes, memory reading, and displaying game statistics
    // ==========MY NOTES==============
    // This is the main window of the app - it does everything from connecting to the game
    // to showing the stats and managing settings
    public partial class MainForm : Form
    {
        // ==========FORMAL COMMENT=========
        // Static utility class providing consistent visual styling throughout the application
        // Defines colors, fonts, spacing, and methods for applying the theme to controls
        // ==========MY NOTES==============
        // This makes everything look consistent with the dark theme
        // Keeps all styling in one place so it's easy to change the look later
        private static class AppTheme
        {
            // Colors
            public static readonly Color BackgroundColor = Color.Black;
            public static readonly Color TextColor = Color.White;
            public static readonly Color AccentColor = Color.DarkRed; // Good for highlights/buttons

            // Fonts
            public static readonly Font DefaultFont = new("Segoe UI", 9f);
            public static readonly Font HeaderFont = new("Segoe UI", 12f, FontStyle.Bold);
            public static readonly Font StatsFont = new("Segoe UI", 14f);

            // Spacing
            public const int StandardPadding = 10;
            public const int StandardMargin = 5;

            // Apply theme to a control
            public static void ApplyTo(Control control)
            {
                control.BackColor = BackgroundColor;
                control.ForeColor = TextColor;
                control.Font = DefaultFont;
            }
        }

        // ==========FORMAL COMMENT=========
        // Fields for process interaction and game memory access
        // ==========MY NOTES==============
        // These variables help us connect to the game and read its memory
        private string currentProcess = string.Empty; // Initialize with empty string
        private RouteManager? routeManager; // Mark as nullable since it's set after connection
        private readonly GameConnectionManager gameConnectionManager;
        private readonly SettingsManager settingsManager;
        private StatsWindow? statsWindow;
        private Button showStatsButton = null!;
        private Button saveButton = null!;
        private Button loadButton = null!;
        private Button resetButton = null!;
        private readonly string lastSelectedGame = string.Empty; //tbh idk if i should remove this keeping here for now
        private DataGridView routeGrid = null!;
        private TextBox gameDirectoryTextBox = null!; // Same approach
        private ToolStripComboBox? autoStartGameComboBox;
        private ToolStripMenuItem? enableAutoStartMenuItem;
        private bool isHotkeysEnabled = false;

        private Label completionLabel = null!;
        private Button showCompletionButton = null!;
        private CompletionStatsWindow? completionStatsWindow;

        // ==========FORMAL COMMENT=========
        // Constructor - initializes the form and loads user settings
        // ==========MY NOTES==============
        // This runs when the app starts - sets everything up
        [SupportedOSPlatform("windows6.1")]
        public MainForm()
        {
            InitializeComponent();

            // Initialize managers FIRST
            gameConnectionManager = new GameConnectionManager();
            gameConnectionManager.StatsUpdated += GameStats_StatsUpdated;

            settingsManager = new SettingsManager(); // Move this up before UI initialization

            // Now initialize UI components that need settingsManager
            InitializeCustomComponents();

            // Auto-detect on startup
            AutoDetectGameOnStartup();

            LoadSettings();
            this.FormClosing += MainForm_FormClosing;
            // In MainForm.cs constructor or Load event
            this.Text = $"Route Tracker {AppInfo.Version}";
        }

        #region UI Initialization
        private void AutoDetectGameOnStartup()
        {
            ToolStripComboBox? gameDropdown = this.MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault();
            if (gameDropdown != null)
            {
                string detectedGame = gameConnectionManager.DetectRunningGame();
                if (!string.IsNullOrEmpty(detectedGame))
                {
                    gameDropdown.SelectedItem = detectedGame;
                }
            }
        }

        // ==========FORMAL COMMENT=========
        // Initializes all custom UI components and sets up the form appearance
        // Configures the menu, tabs, and controls with proper theming
        // ==========MY NOTES==============
        // Sets up the entire UI from scratch since we're not using the designer
        // Makes sure everything has the right colors, sizes, and event handlers
        [SupportedOSPlatform("windows6.1")]
        private void InitializeCustomComponents()
        {
            this.Text = "Route Tracker";
            AppTheme.ApplyTo(this);

            //minimum window size
            this.MinimumSize = new Size(400, 200);

            InitializeHiddenControls();

            // Create a TableLayoutPanel to control layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.BackgroundColor,
                ColumnCount = 1,
                RowCount = 3, // Now using 3 rows
                AutoSize = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            // Row 0: Top bar with buttons (auto size)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // Row 1: Completion label row (auto size)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // Row 2: Route grid (fills remaining space)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // --- Top bar for menu and buttons ---
            var topBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = AppTheme.BackgroundColor,
                Padding = new Padding(0, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 0)
            };

            // MenuStrip (settings only)
            var menuStrip = new MenuStrip
            {
                BackColor = AppTheme.BackgroundColor,
                ForeColor = AppTheme.TextColor,
                Dock = DockStyle.None
            };
            CreateSettingsMenu(menuStrip);
            topBar.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            // Progress buttons
            saveButton = new Button
            {
                Text = "Save Progress",
                MinimumSize = new Size(100, 25),
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(5, 2, 0, 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            saveButton.Click += SaveButton_Click;
            topBar.Controls.Add(saveButton);

            loadButton = new Button
            {
                Text = "Load Progress",
                MinimumSize = new Size(100, 25),
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(5, 2, 0, 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            loadButton.Click += LoadButton_Click;
            topBar.Controls.Add(loadButton);

            resetButton = new Button
            {
                Text = "Reset Progress",
                MinimumSize = new Size(100, 25),
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(5, 2, 0, 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            resetButton.Click += ResetProgressButton_Click;
            topBar.Controls.Add(resetButton);

            // Connect to Game button (opens ConnectionWindow)
            var connectWindowButton = new Button
            {
                Text = "Connect to Game",
                MinimumSize = new Size(100, 25),
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(5, 2, 0, 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            connectWindowButton.Click += (s, e) =>
            {
                using var connectionWindow = new ConnectionWindow(gameConnectionManager, settingsManager);
                if (connectionWindow.ShowDialog(this) == DialogResult.OK)
                {
                    // Always set routeManager after connecting
                    string selectedGame = connectionWindow.SelectedGame;
                    string routeFilePath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Routes",
                        "AC4 100 % Route - Main Route.tsv");

                    routeManager = new RouteManager(routeFilePath, gameConnectionManager);
                    LoadRouteData(routeGrid);
                }
            };
            topBar.Controls.Add(connectWindowButton);

            // Show Stats button (now in top bar)
            showStatsButton = new Button
            {
                Text = "Game Stats",
                MinimumSize = new Size(80, 25),
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(5, 2, 0, 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            showStatsButton.Click += ShowStatsMenuItem_Click;
            topBar.Controls.Add(showStatsButton);

            // Completion stats button
            showCompletionButton = new Button
            {
                Text = "Route Stats",
                MinimumSize = new Size(80, 25),
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(5, 2, 0, 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            showCompletionButton.Click += ShowCompletionStatsButton_Click;
            topBar.Controls.Add(showCompletionButton);

            // Add topBar to the TableLayoutPanel
            mainLayout.Controls.Add(topBar, 0, 0);

            // --- Completion Label Row (second row) ---
            var labelPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.BackgroundColor,
                Padding = new Padding(10, 0, 0, 0) // Left padding to align with left edge
            };

            // Then add the completion label to this panel
            completionLabel = new Label
            {
                Text = "Completion: 0.00%",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppTheme.TextColor,
                Font = new Font(AppTheme.DefaultFont.FontFamily, AppTheme.DefaultFont.Size + 2),  // Slightly larger font
                Location = new Point(10, 0),  // Position at the left edge
                Padding = new Padding(0, 3, 0, 3)
            };
            labelPanel.Controls.Add(completionLabel);

            // Add the label panel to the second row
            mainLayout.Controls.Add(labelPanel, 0, 1);

            // --- RouteGrid Panel (third row, fills remaining area) ---
            routeGrid = CreateRouteGridView();
            routeGrid.Dock = DockStyle.Fill;

            var routeGridPanel = new Panel
            {
                Name = "routeGridPanel",
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 10, 10),
                BackColor = AppTheme.BackgroundColor
            };
            routeGridPanel.Controls.Add(routeGrid);

            // Add routeGridPanel to the TableLayoutPanel (now in row 2)
            mainLayout.Controls.Add(routeGridPanel, 0, 2);

            // Add the TableLayoutPanel to the form
            this.Controls.Add(mainLayout);
        }

        private void ShowCompletionStatsButton_Click(object? sender, EventArgs e)
        {
            if (completionStatsWindow == null || completionStatsWindow.IsDisposed)
                completionStatsWindow = new CompletionStatsWindow();

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
        }

        private void ShowStatsMenuItem_Click(object? sender, EventArgs e)
        {
            if (statsWindow == null || statsWindow.IsDisposed)
                statsWindow = new StatsWindow();

            // Try to get stats from the current game connection
            if (gameConnectionManager.GameStats is IGameStats stats)
            {
                var statsDict = stats.GetStatsAsDictionary();

                string statsText =
                    $"Completion Percentage: {statsDict.GetValueOrDefault("Completion Percentage", 0)}%\n" +
                    $"Completion Percentage Exact: {statsDict.GetValueOrDefault("Exact Percentage", 0):F2}%\n" +
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

                statsWindow.UpdateStats(statsText);
            }
            else
            {
                statsWindow.UpdateStats("No stats available. Connect to a game first.");
            }

            statsWindow.Show();
            statsWindow.BringToFront();
        }

        // ==========FORMAL COMMENT=========
        // Creates the settings menu with sub-items for application configuration
        // Adds auto-start and game directory menu items with event handlers
        // ==========MY NOTES==============
        // Builds the Settings dropdown menu with its options
        // Connects the menu items to their respective event handlers
        [SupportedOSPlatform("windows6.1")]
        private void CreateSettingsMenu(MenuStrip menuStrip)
        {
            ToolStripMenuItem settingsMenuItem = new("Settings");

            AddUpdateCheckMenuItem(settingsMenuItem);
            AddDevModeMenuItem(settingsMenuItem);

            // --- New Auto-Start Game UI ---
            // ComboBox for selecting the game to auto-start
            autoStartGameComboBox = new ToolStripComboBox
            {
                Name = "autoStartGameComboBox",
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 180
            };
            // Populate with games that have directories set
            var gamesWithDirs = settingsManager.GetGamesWithDirectoriesSet();
            autoStartGameComboBox.Items.AddRange([.. gamesWithDirs]);
            autoStartGameComboBox.SelectedIndex = -1;

            // Set selected item if already set in settings
            string savedAutoStart = Settings.Default.AutoStart;
            if (!string.IsNullOrEmpty(savedAutoStart) && gamesWithDirs.Contains(savedAutoStart))
            {
                autoStartGameComboBox.SelectedItem = savedAutoStart;
            }

            autoStartGameComboBox.SelectedIndexChanged += AutoStartGameComboBox_SelectedIndexChanged;
            settingsMenuItem.DropDownItems.Add(new ToolStripLabel("Auto-Start Game:"));
            settingsMenuItem.DropDownItems.Add(autoStartGameComboBox);

            // Checkbox to enable/disable auto-start
            enableAutoStartMenuItem = new ToolStripMenuItem("Enable Auto-Start")
            {
                CheckOnClick = true,
                Checked = !string.IsNullOrEmpty(savedAutoStart),
                Enabled = autoStartGameComboBox.SelectedItem != null
            };
            enableAutoStartMenuItem.CheckedChanged += EnableAutoStartMenuItem_CheckedChanged;
            settingsMenuItem.DropDownItems.Add(enableAutoStartMenuItem);

            // --- End New Auto-Start Game UI ---

            // Game Directory menu item
            ToolStripMenuItem gameDirectoryMenuItem = new("Game Directory");
            gameDirectoryMenuItem.Click += GameDirectoryMenuItem_Click;

            // Always On Top menu item
            ToolStripMenuItem alwaysOnTopMenuItem = new("Always On Top")
            {
                CheckOnClick = true,
                Checked = this.TopMost
            };
            alwaysOnTopMenuItem.CheckedChanged += AlwaysOnTopMenuItem_CheckedChanged;

            // Add the rest of the menu items
            settingsMenuItem.DropDownItems.Add(gameDirectoryMenuItem);
            settingsMenuItem.DropDownItems.Add(alwaysOnTopMenuItem);

            menuStrip.Items.Add(settingsMenuItem);

            settingsMenuItem.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem hotkeysMenuItem = new("Configure Hotkeys");
            hotkeysMenuItem.Click += HotkeysMenuItem_Click;
            settingsMenuItem.DropDownItems.Add(hotkeysMenuItem);

            ToolStripMenuItem enableHotkeysMenuItem = new("Enable Hotkeys")
            {
                CheckOnClick = true,
                Checked = settingsManager.GetHotkeysEnabled()
            };
            enableHotkeysMenuItem.CheckedChanged += EnableHotkeysMenuItem_CheckedChanged;
            settingsMenuItem.DropDownItems.Add(enableHotkeysMenuItem);

            isHotkeysEnabled = settingsManager.GetHotkeysEnabled();

            ToolStripMenuItem showSaveLocationMenuItem = new("Show Save Location");
            showSaveLocationMenuItem.Click += ShowSaveLocationMenuItem_Click;
            settingsMenuItem.DropDownItems.Add(showSaveLocationMenuItem);
        }

        private void AutoStartGameComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (autoStartGameComboBox == null || enableAutoStartMenuItem == null)
                return;

            // Enable the checkbox only if a game is selected
            enableAutoStartMenuItem.Enabled = autoStartGameComboBox.SelectedItem != null;

            // If a game is selected and the checkbox is checked, update the setting
            if (enableAutoStartMenuItem.Checked && autoStartGameComboBox.SelectedItem is string selectedGame)
            {
                Settings.Default.AutoStart = selectedGame;
                Settings.Default.Save();
            }
        }

        private void EnableAutoStartMenuItem_CheckedChanged(object? sender, EventArgs e)
        {
            if (autoStartGameComboBox == null || enableAutoStartMenuItem == null)
                return;

            if (enableAutoStartMenuItem.Checked && autoStartGameComboBox.SelectedItem is string selectedGame)
            {
                Settings.Default.AutoStart = selectedGame;
            }
            else
            {
                Settings.Default.AutoStart = string.Empty;
            }
            Settings.Default.Save();
        }

        private void ShowSaveLocationMenuItem_Click(object? sender, EventArgs e)
        {
            // Get the route file path from the RouteManager if available
            string routeFilePath = routeManager?.GetRouteFilePath() ??
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes", "AC4 100 % Route - Main Route.tsv");

            // Calculate the save directory path
            string saveDir = Path.Combine(
                Path.GetDirectoryName(routeFilePath) ?? AppDomain.CurrentDomain.BaseDirectory,
                "SavedProgress");

            // Show the path in a message box
            MessageBox.Show($"Autosave location: {saveDir}",
                "Save File Location",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Try to open the folder for the user
            try
            {
                Process.Start("explorer.exe", saveDir);
            }
            catch
            {
                // If opening the folder fails, show additional instructions
                MessageBox.Show($"Could not open the folder automatically.\n\n" +
                    $"Please navigate to this location manually:\n{saveDir}",
                    "Open Folder Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void HotkeysMenuItem_Click(object? sender, EventArgs e)
        {
            // Create the form
            HotkeysSettingsForm hotkeysForm = new()
            {
                // Ensure form appears in front of main window
                StartPosition = FormStartPosition.CenterParent
            };

            // If TopMost is set on main form, we need to temporarily adjust
            bool wasTopMost = this.TopMost;
            if (wasTopMost)
                this.TopMost = false;

            // Show dialog as modal with this form as owner
            hotkeysForm.ShowDialog(this);

            // Restore TopMost setting if needed
            if (wasTopMost)
                this.TopMost = true;
        }

        private void EnableHotkeysMenuItem_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                isHotkeysEnabled = menuItem.Checked;
                settingsManager.SaveHotkeysEnabled(isHotkeysEnabled);
            }
        }

        // Override ProcessCmdKey to handle hotkeys
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Only process hotkeys if enabled and on route tab
            if (isHotkeysEnabled)
            {
                (Keys CompleteHotkey, Keys SkipHotkey) = settingsManager.GetHotkeys();

                if (keyData == CompleteHotkey)
                {
                    CompleteSelectedEntry();
                    return true;
                }
                else if (keyData == SkipHotkey)
                {
                    SkipSelectedEntry();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Methods to handle entry actions
        private void CompleteSelectedEntry()
        {
            if (routeGrid.CurrentRow != null && routeGrid.CurrentRow.Tag is RouteEntry 
                selectedEntry)
            {
                // Mark as complete
                selectedEntry.IsCompleted = true;
                routeGrid.CurrentRow.Cells[1].Value = "X";

                // Update grid
                RouteManager.SortRouteGridByCompletion(routeGrid);
                RouteManager.ScrollToFirstIncomplete(routeGrid);

                // Auto-save progress
                routeManager?.AutoSaveProgress();

                // Update completion percentage display
                if (routeManager != null)
                {
                    var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                    completionLabel.Text = $"Completion: {percentage:F2}%";

                    // Also update the stats window if it's open
                    if (completionStatsWindow != null && !completionStatsWindow.IsDisposed && completionStatsWindow.Visible)
                    {
                        completionStatsWindow.UpdateStats(percentage, completed, total);
                    }
                }
            }
        }

        private void SkipSelectedEntry()
        {
            if (routeGrid.CurrentRow != null && routeGrid.CurrentRow.Tag is RouteEntry selectedEntry)
            {
                // Mark as skipped
                selectedEntry.IsSkipped = true;

                // Remove from display
                routeGrid.Rows.Remove(routeGrid.CurrentRow);

                // Auto-save progress
                routeManager?.AutoSaveProgress();

                if (routeManager != null)
                {
                    var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                    completionLabel.Text = $"Completion: {percentage:F2}%";

                    // Also update the stats window if it's open
                    if (completionStatsWindow != null && !completionStatsWindow.IsDisposed && completionStatsWindow.Visible)
                    {
                        completionStatsWindow.UpdateStats(percentage, completed, total);
                    }
                }
            }
        }

        // ==========FORMAL COMMENT=========
        // Creates connection controls in the main menu for game selection and connection
        // Adds connection status label, game dropdown, and connect button
        // ==========MY NOTES==============
        // Sets up the game selection dropdown and connect button
        // Shows the connection status so users know what's happening
        [SupportedOSPlatform("windows6.1")]
        private void CreateConnectionControls(MenuStrip menuStrip)
        {
            // Create and configure the connection label
            ToolStripLabel connectionLabel = new()
            {
                Text = "Not connected"
            };
            menuStrip.Items.Add(connectionLabel);

            // Create and configure the game dropdown
            ToolStripComboBox gameDropdown = new();
            gameDropdown.Items.AddRange(["", "Assassin's Creed 4", "God of War 2018"]);
            gameDropdown.SelectedIndex = 0;
            menuStrip.Items.Add(gameDropdown);

            // Add auto-detect button
            ToolStripButton autoDetectButton = new("Auto-Detect");
            autoDetectButton.Click += AutoDetectButton_Click;
            menuStrip.Items.Add(autoDetectButton);

            // Create and configure the connect button
            ToolStripButton connectButton = new("Connect to Game");
            connectButton.Click += ConnectButton_Click;
            menuStrip.Items.Add(connectButton);
        }

        // ==========FORMAL COMMENT=========
        // Initializes hidden controls used for settings that aren't directly visible
        // Sets up event handlers and default values for these controls
        // ==========MY NOTES==============
        // These are controls we need for functionality but don't want to show
        // They store values that get saved but aren't part of the main UI
        [SupportedOSPlatform("windows6.1")]
        private void InitializeHiddenControls()
        {
            // Initialize gameDirectoryTextBox and autoStartCheckBox
            gameDirectoryTextBox = new TextBox
            {
                ReadOnly = true,
                Visible = false,
                Width = 600,
                Dock = DockStyle.Top
            };
            this.Controls.Add(gameDirectoryTextBox);
        }

        private void AutoDetectButton_Click(object? sender, EventArgs e)
        {
            ToolStripComboBox? gameDropdown = this.MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault();
            ToolStripLabel? connectionLabel = this.MainMenuStrip?.Items.OfType<ToolStripLabel>().FirstOrDefault();

            if (gameDropdown == null || connectionLabel == null)
            {
                MessageBox.Show("Required controls are missing.");
                return;
            }

            // Try to detect running game
            string detectedGame = gameConnectionManager.DetectRunningGame();

            if (!string.IsNullOrEmpty(detectedGame))
            {
                // Found a game - select it in the dropdown
                gameDropdown.SelectedItem = detectedGame;
                connectionLabel.Text = $"Detected: {detectedGame}";
            }
            else
            {
                connectionLabel.Text = "No supported games detected";
            }
        }
        #endregion

        #region Route
        // ==========FORMAL COMMENT=========
        // Event handler for Save Progress button clicks
        // Delegates to the RouteManager to handle the file dialog and saving process
        // ==========MY NOTES==============
        // Runs when the user clicks Save Progress
        // Lets routeManager handle all the details of saving the data
        private void SaveButton_Click(object? sender, EventArgs e)
        {
            routeManager?.SaveProgress(this);
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Load Progress button clicks
        // Delegates to the RouteManager to handle file selection and loading process
        // Provides user feedback when loading completes successfully
        // ==========MY NOTES==============
        // Runs when the user clicks Load Progress
        // Has RouteManager do the actual loading and shows a message if it worked
        private void LoadButton_Click(object? sender, EventArgs e)
        {
            // Always ensure routeManager exists
            routeManager ??= new RouteManager(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes", "AC4 100 % Route - Main Route.tsv"),
                new GameConnectionManager()
            );

            // Always ensure route entries are loaded before loading progress
            if (routeManager.LoadEntries().Count == 0)
            {
                MessageBox.Show("No route entries found. Make sure the route file exists.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Open file dialog and load progress
            if (routeManager.LoadProgress(routeGrid, this))
            {
                MessageBox.Show("Progress loaded successfully.", "Load Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ResetProgressButton_Click(object? sender, EventArgs e)
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

        // ==========FORMAL COMMENT=========
        // Creates and configures the DataGridView for route entries
        // Sets up appearance, behavior, and performance settings
        // ==========MY NOTES==============
        // Sets up the grid that shows all route items and their completion status
        // Configures it with our dark theme and proper display settings
        private static DataGridView CreateRouteGridView()
        {
            // Create a DataGridView for displaying route entries
            DataGridView routeGrid = new()
            {
                Name = "routeGrid",
                Dock = DockStyle.Fill,
                BackgroundColor = Color.Black,
                ForeColor = Color.White,
                GridColor = Color.FromArgb(80, 80, 80),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Font = new Font("Segoe UI", 11f),
                RowTemplate = { Height = 30 },
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            };

            ConfigureRouteGridColumns(routeGrid);
            ApplyRouteGridStyling(routeGrid);

            return routeGrid;
        }

        // ==========FORMAL COMMENT=========
        // Configures columns for the route grid with proper styling
        // Sets up item description and completion status columns
        // ==========MY NOTES==============
        // Creates the two columns - one for the description and one for the checkmark
        // Styles them to match our dark theme and make them easy to read
        private static void ConfigureRouteGridColumns(DataGridView grid)
        {
            // Configure columns - with item first, completion status second
            DataGridViewTextBoxColumn itemColumn = new()
            {
                Name = "Item",
                HeaderText = "", // No header text
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Padding = new Padding(10, 5, 5, 5),
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    WrapMode = DataGridViewTriState.True // Enable text wrapping
                }
            };

            // Use text column for completion status
            DataGridViewTextBoxColumn completedColumn = new()
            {
                Name = "Completed",
                HeaderText = "",
                Width = 50,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    BackColor = Color.Black,
                    ForeColor = Color.White
                }
            };

            // Add columns in correct order
            grid.Columns.Add(itemColumn);
            grid.Columns.Add(completedColumn);
        }

        // ==========FORMAL COMMENT=========
        // Applies consistent styling to the route grid for better appearance
        // Configures colors, fonts, and visual elements to match theme
        // ==========MY NOTES==============
        // Makes the grid look good with our dark theme
        // Removes headers, sets colors, and configures the selection style
        private static void ApplyRouteGridStyling(DataGridView grid)
        {
            // Apply theme colors to the grid
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.RowHeadersDefaultCellStyle.BackColor = Color.Black;
            grid.RowsDefaultCellStyle.BackColor = Color.Black;
            grid.RowsDefaultCellStyle.ForeColor = Color.White;

            // No alternating row colors
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.Black;

            // Make the row selection very subtle
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 30, 35);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;

            // Make the header row minimal height or invisible
            grid.ColumnHeadersHeight = 4;
            grid.ColumnHeadersVisible = false;

            // Explicitly set scrollbar visibility
            grid.ScrollBars = ScrollBars.Vertical;
        }

        // ==========FORMAL COMMENT=========
        // Loads route data into the grid from the RouteManager
        // Shows diagnostic messages if no route data is available
        // ==========MY NOTES==============
        // Fills the route grid with actual data or shows a message
        // Handles cases where we're not connected yet
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059", 
        Justification = "Values needed for clarity and completion tracking")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079", 
        Justification = "Suppression required for code consistency")]
        private void LoadRouteData(DataGridView routeGrid)
        {
            if (routeManager != null)
            {
                string selectedGame = GetSelectedGameName();
                routeManager.LoadRouteDataIntoGrid(routeGrid, selectedGame);

                // Update completion percentage after loading route data
                var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                completionLabel.Text = $"Completion: {percentage:F2}%";
            }
            else
            {
                var (completeHotkey, skipHotkey) = settingsManager.GetHotkeys();
                bool hotkeysSet = completeHotkey != Keys.None || skipHotkey != Keys.None;

                if (hotkeysSet)
                {
                    // This should not happen if you set up routeManager above, but just in case
                    routeGrid.Rows.Add("Hotkeys are set, but route manager is not initialized.", "");
                }
                else
                {
                    routeGrid.Rows.Add("Connect to a game or set up hotkeys to load route tracking data", "");
                }
            }
        }

        // ==========FORMAL COMMENT=========
        // Updates route entry completion status based on current game statistics
        // Delegates completion checking to the RouteManager
        // ==========MY NOTES==============
        // This tells the RouteManager to check if any route items are complete
        // Gets called automatically whenever the game stats update
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style","IDE0059", 
        Justification = "Values needed for clarity and completion tracking")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style","IDE0079", 
        Justification = "Suppression required for code consistency")]
        private void UpdateRouteCompletionStatus(DataGridView routeGrid, GameStatsEventArgs stats)
        {
            // Delegate all the route checking logic to the RouteManager
            bool changed = routeManager?.UpdateCompletionStatus(routeGrid, stats) ?? false;

            // Update the completion percentage label
            if (routeManager != null)
            {
                var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                completionLabel.Text = $"Completion: {percentage:F2}%";

                // Also update the stats window if it's open
                if (completionStatsWindow != null && !completionStatsWindow.IsDisposed && completionStatsWindow.Visible)
                {
                    completionStatsWindow.UpdateStats(percentage, completed, total);
                }
            }
        }

        #endregion

        #region Game Connection
        // ==========FORMAL COMMENT=========
        // Event handler for Connect button clicks
        // Handles UI interaction and delegates game connection to GameConnectionManager
        // ==========MY NOTES==============
        // This runs when you click "Connect to Game"
        // Manages the UI and uses GameConnectionManager for the actual connection work
        private async void ConnectButton_Click(object? sender, EventArgs e)
        {
            ToolStripComboBox? gameDropdown = this.MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault();
            ToolStripLabel? connectionLabel = this.MainMenuStrip?.Items.OfType<ToolStripLabel>().FirstOrDefault();

            if (gameDropdown == null || connectionLabel == null)
            {
                MessageBox.Show("Required controls are missing.");
                return;
            }

            // Get selected game, or try auto-detect if nothing selected
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

            gameDirectoryTextBox.Text = gameDirectory;

            // Remember process name for later reference
            currentProcess = selectedGame switch
            {
                "Assassin's Creed 4" => "AC4BFSP.exe",
                "God of War 2018" => "GoW.exe",
                _ => string.Empty
            };

            // Use GameConnectionManager to handle the connection
            bool connected = await gameConnectionManager.ConnectToGameAsync(selectedGame, enableAutoStartMenuItem?.Checked == true);

            // Use actual route file path when initializing RouteManager
            if (connected)
            {
                connectionLabel.Text = $"Connected to {selectedGame}";
                string routeFilePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Routes",
                    "AC4 100 % Route - Main Route.tsv"); // Use the exact filename you have

                // Pass the gameConnectionManager when creating the RouteManager
                routeManager = new RouteManager(routeFilePath, gameConnectionManager);

                // Reload route data if we're on the route tab
                LoadRouteData(routeGrid);
            }
            else
            {
                connectionLabel.Text = "Error: Cannot connect to process. Make sure the game is running.";
            }
        }

        // ==========FORMAL COMMENT=========
        // Retrieves the currently selected game name from the UI dropdown
        // Handles null cases and returns empty string when no selection
        // ==========MY NOTES==============
        // Gets whatever game is currently selected in the dropdown
        // Handles cases where nothing is selected or the dropdown is missing
        private string GetSelectedGameName()
        {
            if (MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault() is ToolStripComboBox gameDropdown)
            {
                return gameDropdown.SelectedItem?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        //for method below
        private DateTime _lastUIUpdateTime = DateTime.MinValue;
        private readonly TimeSpan _minimumUIUpdateInterval = TimeSpan.FromMilliseconds(100);

        // ==========FORMAL COMMENT=========
        // Event handler for GameStats statistics update events
        // Receives updated game metrics and refreshes the UI with the latest values
        // Uses thread-safe Invoke to update UI controls from the background thread
        // ==========MY NOTES==============
        // This catches the stats when they update automatically
        // Updates the UI safely across threads to show the new numbers
        // The real workhorse that keeps the display current without clicking buttons
        private void GameStats_StatsUpdated(object? sender, GameStatsEventArgs e)
        {
            // Throttle UI updates to prevent excessive redraws
            if (DateTime.Now - _lastUIUpdateTime < _minimumUIUpdateInterval)
                return;

            _lastUIUpdateTime = DateTime.Now;

            this.Invoke(() =>
            {
                // Update route completion status
                if (routeGrid != null)
                {
                    UpdateRouteCompletionStatus(routeGrid, e);
                }

                // Update stats window if open
                if (statsWindow != null && statsWindow.Visible)
                {
                    var stats = e.Stats;

                    // Build the stats text using the dictionary approach
                    string statsText =
                        $"Completion Percentage: {e.GetValue<int>("Completion Percentage", 0)}%\n" +
                        $"Completion Percentage Exact: {e.GetValue<float>("Exact Percentage", 0f):F2}%\n" +
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
            });
        }

        // ==========FORMAL COMMENT=========
        // Performs cleanup operations for GameStats resources
        // Unsubscribes from events and stops background updates to prevent memory leaks
        // ==========MY NOTES==============
        // This stops the automatic stat updates when we're done
        // Properly disconnects everything to avoid crashes and memory leaks
        // Important housekeeping to keep things tidy
        private void CleanupGameStats()
        {
            gameConnectionManager?.CleanupGameStats();
        }
        #endregion

        #region Settings Management
        // ==========FORMAL COMMENT=========
        // Loads user settings from application configuration
        // Retrieves saved game directory and auto-start preference
        // ==========MY NOTES==============
        // Gets the saved settings when the app starts up
        // Uses a flag to prevent triggering events while loading
        [SupportedOSPlatform("windows6.1")]
        private void LoadSettings()
        {
            settingsManager.LoadSettings(gameDirectoryTextBox);
            this.TopMost = settingsManager.GetAlwaysOnTop();
        }

        // ==========FORMAL COMMENT=========
        // Saves current application settings to the configuration file
        // Persists game directory and auto-start preferences for future sessions
        // ==========MY NOTES==============
        // Writes all settings to disk so they're remembered next time
        // Simple wrapper around the Settings.Save() functionality
        [SupportedOSPlatform("windows6.1")]
        private void SaveSettings()
        {
            settingsManager.SaveSettings(gameDirectoryTextBox.Text, Settings.Default.AutoStart);
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Game Directory menu item
        // Opens the game directory settings form
        // ==========MY NOTES==============
        // Shows a window where you can set game directories
        [SupportedOSPlatform("windows6.1")]
        private void GameDirectoryMenuItem_Click(object? sender, EventArgs e)
        {
            GameDirectoryForm gameDirectoryForm = new();
            gameDirectoryForm.ShowDialog();
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Settings menu item click
        // Toggles the visibility of the settings panel in the UI
        // ==========MY NOTES==============
        // Shows or hides the settings panel when you click the menu item
        [SupportedOSPlatform("windows6.1")]
        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            // Assumes you have a Panel named "settingsPanel" already created and added to the form
            if (this.Controls["settingsPanel"] is Panel settingsPanel)
            {
                settingsPanel.Visible = !settingsPanel.Visible;
                settingsPanel.BringToFront();
            }
        }

        // ==========FORMAL COMMENT=========
        // Event handler for game selection change in settings
        // Updates directory textbox based on the selected game
        // ==========MY NOTES==============
        // When you pick a different game in settings, this shows the right game folder
        [SupportedOSPlatform("windows6.1")]
        private void SettingsGameDropdown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (sender is not ComboBox settingsGameDropdown)
                return;

            // Add null check for settingsPanel
            if (this.Controls["settingsPanel"] is not Panel settingsPanel)
                return;

            // Add null check for settingsDirectoryTextBox
            if (settingsPanel.Controls["settingsDirectoryTextBox"] is not TextBox settingsDirectoryTextBox)
                return;

            string selectedGame = settingsGameDropdown.SelectedItem?.ToString() ?? string.Empty;
            settingsDirectoryTextBox.Text = settingsManager.GetGameDirectory(selectedGame);
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Always On Top menu item state changes
        // Applies the setting immediately and persists it to application settings
        // Maintains UI consistency with application behavior
        // ==========MY NOTES==============
        // Makes the window stay on top of other windows when checked
        // Updates both the visual display and saves the setting for next time
        private void AlwaysOnTopMenuItem_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem alwaysOnTopMenuItem)
            {
                // Apply the setting immediately
                this.TopMost = alwaysOnTopMenuItem.Checked;

                // Save the setting
                settingsManager.SaveAlwaysOnTop(alwaysOnTopMenuItem.Checked);
            }
        }
        #endregion

        #region Update Management
        // Call this in your constructor or OnLoad (after UI is ready)
        [SupportedOSPlatform("windows6.1")]
        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Auto-start logic
            string autoStartGame = Settings.Default.AutoStart;
            if (!string.IsNullOrEmpty(autoStartGame) && !string.IsNullOrEmpty(settingsManager.GetGameDirectory(autoStartGame)))
            {
                bool connected = await gameConnectionManager.ConnectToGameAsync(autoStartGame, true);
                if (connected)
                {
                    // Load route data for the auto-started game
                    string routeFilePath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Routes",
                        $"{autoStartGame} 100 % Route - Main Route.tsv"); // Adjust this pattern for your game/route file naming
                    routeManager = new RouteManager(routeFilePath, gameConnectionManager);
                    LoadRouteData(routeGrid);
                }
            }

            await CheckForUpdatesAsync();
        }

        // Checks GitHub for a new release and prompts the user to update if one is available.
        // Doesnt run if the user has disabled update checks in settings.
        // Also if dev passcode is is entered in the settings, it will not check for updates.
        private static async Task CheckForUpdatesAsync()
        {
            if (Properties.Settings.Default.DevMode)
                return;

            if (!Properties.Settings.Default.CheckForUpdateOnStartup)
                return;

            string apiUrl = $"https://api.github.com/repos/{AppInfo.GitHubRepo}/releases";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RouteTrackerUpdater");

            try
            {
                var response = await client.GetStringAsync(apiUrl);
                using var doc = JsonDocument.Parse(response);

                // Find the latest release (including pre-releases)
                JsonElement? latestRelease = null;
                DateTime latestDate = DateTime.MinValue;

                foreach (var release in doc.RootElement.EnumerateArray())
                {
                    // Skip drafts
                    if (release.GetProperty("draft").GetBoolean())
                        continue;

                    // Get published date
                    if (release.TryGetProperty("published_at", out var publishedAtProp) &&
                        DateTime.TryParse(publishedAtProp.GetString(), out var publishedAt))
                    {
                        if (publishedAt > latestDate)
                        {
                            latestDate = publishedAt;
                            latestRelease = release;
                        }
                    }
                }

                if (latestRelease == null)
                {
                    MessageBox.Show("Could not find any releases on GitHub.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string? latestVersion = latestRelease.Value.GetProperty("tag_name").GetString();
                if (string.IsNullOrEmpty(latestVersion))
                {
                    MessageBox.Show("Could not determine the latest version from GitHub.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!string.Equals(latestVersion, AppInfo.Version, StringComparison.OrdinalIgnoreCase))
                {
                    var result = MessageBox.Show(
                        $"A new version is available!\n\nCurrent: {AppInfo.Version}\nLatest: {latestVersion}\n\nDo you want to download and install it?",
                        "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        var assets = latestRelease.Value.GetProperty("assets");
                        if (assets.GetArrayLength() > 0)
                        {
                            string? zipUrl = assets[0].GetProperty("browser_download_url").GetString();
                            if (string.IsNullOrEmpty(zipUrl))
                            {
                                MessageBox.Show("No download URL found for the latest release.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            string tempZip = Path.GetTempFileName();
                            using (var zipStream = await client.GetStreamAsync(zipUrl))
                            using (var fileStream = File.Create(tempZip))
                                await zipStream.CopyToAsync(fileStream);

                            ZipFile.ExtractToDirectory(tempZip, AppDomain.CurrentDomain.BaseDirectory, true);

                            MessageBox.Show("Update complete. Please restart the application.", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No downloadable asset found in the latest release.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
            }
        }

        private static void AddUpdateCheckMenuItem(ToolStripMenuItem settingsMenuItem)
        {
            ToolStripMenuItem checkForUpdatesMenuItem = new("Check for Updates on Startup")
            {
                CheckOnClick = true,
                Checked = Properties.Settings.Default.CheckForUpdateOnStartup
            };
            checkForUpdatesMenuItem.CheckedChanged += (s, e) =>
            {
                Properties.Settings.Default.CheckForUpdateOnStartup = checkForUpdatesMenuItem.Checked;
                Properties.Settings.Default.Save();
            };
            settingsMenuItem.DropDownItems.Add(checkForUpdatesMenuItem);

        }

        private static void AddDevModeMenuItem(ToolStripMenuItem settingsMenuItem)
        {
            var devModeMenuItem = new ToolStripMenuItem("Enable Dev Mode")
            {
                CheckOnClick = true,
                Checked = Properties.Settings.Default.DevMode
            };
            devModeMenuItem.CheckedChanged += (s, e) =>
            {
                if (devModeMenuItem.Checked)
                {
                    using var passForm = new DevPasscodeForm();
                    if (passForm.ShowDialog() == DialogResult.OK)
                    {
                        if (passForm.Passcode == "1289")
                        {
                            Properties.Settings.Default.DevMode = true;
                            Properties.Settings.Default.Save();
                            MessageBox.Show("Dev Mode enabled. Update checks will be skipped.", "Dev Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            devModeMenuItem.Checked = false;
                            MessageBox.Show("Incorrect passcode.", "Dev Mode", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        devModeMenuItem.Checked = false;
                    }
                }
                else
                {
                    Properties.Settings.Default.DevMode = false;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Dev Mode disabled.", "Dev Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            settingsMenuItem.DropDownItems.Add(devModeMenuItem);
        }
        #endregion

        #region Form Events
        // ==========FORMAL COMMENT=========
        // Form closing event handler that ensures proper resource cleanup
        // Triggers cleanup operations before the form is destroyed
        // ==========MY NOTES==============
        // Runs when you close the application
        // Makes sure we clean up everything properly before exiting
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Clean up any resources before closing
            CleanupGameStats();
        }
        #endregion
    }
}
