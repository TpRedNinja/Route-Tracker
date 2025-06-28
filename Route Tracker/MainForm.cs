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
        public const string Version = "v0.2.5-Beta";
        public const string GitHubRepo = "TpRedNinja/Route-Tracker"; // e.g. "myuser/RouteTracker"
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
        

        private DataGridView routeGrid = null!;

        private TextBox gameDirectoryTextBox = null!; // Same approach
        private CheckBox autoStartCheckBox = null!; // Same approach

        private bool isHotkeysEnabled = false;

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

            CreateMainMenu();
            InitializeHiddenControls();

            // Progress buttons (manual position)
            int buttonWidth = 100;
            int buttonHeight = 25;
            int spacing = 10;
            int menuHeight = this.MainMenuStrip?.Height ?? 24;

            saveButton = new Button
            {
                Text = "Save Progress",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = spacing,
                Top = menuHeight + spacing,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont
            };
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            loadButton = new Button
            {
                Text = "Load Progress",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = saveButton.Right + spacing,
                Top = menuHeight + spacing,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont
            };
            loadButton.Click += LoadButton_Click;
            this.Controls.Add(loadButton);

            resetButton = new Button
            {
                Text = "Reset Progress",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = loadButton.Right + spacing,
                Top = menuHeight + spacing,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont
            };
            resetButton.Click += ResetProgressButton_Click;
            this.Controls.Add(resetButton);

            // Keep buttons in place on resize (optional, if you want them to stay at the top left)
            this.Resize += (s, e) =>
            {
                int y = (this.MainMenuStrip?.Height ?? 24) + spacing;
                saveButton.Top = y;
                loadButton.Top = y;
                resetButton.Top = y;
                loadButton.Left = saveButton.Right + spacing;
                resetButton.Left = loadButton.Right + spacing;
            };

            // Add the Show Stats button at the bottom-right (as before)
            showStatsButton = new Button
            {
                Text = "Show Stats",
                Width = 90,
                Height = 25,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont
            };
            showStatsButton.Click += ShowStatsMenuItem_Click;
            this.Controls.Add(showStatsButton);
            showStatsButton.Left = this.ClientSize.Width - showStatsButton.Width - 20;
            showStatsButton.Top = this.ClientSize.Height - showStatsButton.Height - 20;
            this.Resize += (s, e) =>
            {
                showStatsButton.Left = this.ClientSize.Width - showStatsButton.Width - 20;
                showStatsButton.Top = this.ClientSize.Height - showStatsButton.Height - 20;
            };

            routeGrid = CreateRouteGridView();
            this.Controls.Add(routeGrid);
        }

        // ==========FORMAL COMMENT=========
        // Creates and configures the main menu bar at the top of the form
        // Adds settings, tab selection buttons, and connection controls
        // ==========MY NOTES==============
        // Builds the top menu bar with all its buttons and dropdown menus
        // Sets up the styling and attaches event handlers
        [SupportedOSPlatform("windows6.1")]
        private void CreateMainMenu()
        {
            // Create and configure the MenuStrip
            MenuStrip menuStrip = new()
            {
                Dock = DockStyle.Top,
                BackColor = AppTheme.BackgroundColor,
                ForeColor = AppTheme.TextColor
            };

            // Create settings menu with sub-items
            CreateSettingsMenu(menuStrip);

            // Create connection controls
            CreateConnectionControls(menuStrip);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void ShowStatsMenuItem_Click(object? sender, EventArgs e)
        {
            if (statsWindow == null || statsWindow.IsDisposed)
                statsWindow = new StatsWindow();

            // Try to get stats from the current game connection
            if (gameConnectionManager.GameStats is IGameStats stats)
            {
                var (Percent, PercentFloat, Viewpoints, Myan, Treasure, Fragments, Assassin, Naval, Letters, Manuscripts, Music, Forts, Taverns, TotalChests) = stats.GetStats();
                var (StoryMissions, TemplarHunts, LegendaryShips, TreasureMaps) = stats.GetSpecialActivityCounts();

                string statsText =
                    $"Completion Percentage: {Percent}%\n" +
                    $"Completion Percentage Exact: {Math.Round(PercentFloat, 2)}%\n" +
                    $"Viewpoints Completed: {Viewpoints}\n" +
                    $"Myan Stones Collected: {Myan}\n" +
                    $"Buried Treasure Collected: {Treasure}\n" +
                    $"AnimusFragments Collected: {Fragments}\n" +
                    $"AssassinContracts Completed: {Assassin}\n" +
                    $"NavalContracts Completed: {Naval}\n" +
                    $"LetterBottles Collected: {Letters}\n" +
                    $"Manuscripts Collected: {Manuscripts}\n" +
                    $"Music Sheets Collected: {Music}\n" +
                    $"Forts Captured: {Forts}\n" +
                    $"Taverns unlocked: {Taverns}\n" +
                    $"Total Chests Collected: {TotalChests}\n" +
                    $"Story Missions Completed: {StoryMissions}\n" +
                    $"Templar Hunts Completed: {TemplarHunts}\n" +
                    $"Legendary Ships Defeated: {LegendaryShips}\n" +
                    $"Treasure Maps Collected: {TreasureMaps}";

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
            // Create and configure the Settings menu item
            ToolStripMenuItem settingsMenuItem = new("Settings");

            AddUpdateCheckMenuItem(settingsMenuItem);

            AddDevModeMenuItem(settingsMenuItem);

            // Create and configure the Auto-Start Game menu item
            ToolStripMenuItem autoStartMenuItem = new("Auto-Start Game")
            {
                CheckOnClick = true
            };
            autoStartMenuItem.CheckedChanged += AutoStartMenuItem_CheckedChanged;

            // Create and configure the Game Directory menu item
            ToolStripMenuItem gameDirectoryMenuItem = new("Game Directory");
            gameDirectoryMenuItem.Click += GameDirectoryMenuItem_Click;

            // Create and configure the Always On Top menu item
            ToolStripMenuItem alwaysOnTopMenuItem = new("Always On Top")
            {
                CheckOnClick = true,
                Checked = this.TopMost // Initialize with current state
            };
            alwaysOnTopMenuItem.CheckedChanged += AlwaysOnTopMenuItem_CheckedChanged;

            // Add the menu items to the Settings menu item
            settingsMenuItem.DropDownItems.Add(autoStartMenuItem);
            settingsMenuItem.DropDownItems.Add(gameDirectoryMenuItem);
            settingsMenuItem.DropDownItems.Add(alwaysOnTopMenuItem);

            // Add the Settings menu item to the MenuStrip
            menuStrip.Items.Add(settingsMenuItem);

            // Add separator
            settingsMenuItem.DropDownItems.Add(new ToolStripSeparator());

            // Add hotkeys menu items
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

            // Initialize hotkey state
            isHotkeysEnabled = settingsManager.GetHotkeysEnabled();

            ToolStripMenuItem showSaveLocationMenuItem = new("Show Save Location");
            showSaveLocationMenuItem.Click += ShowSaveLocationMenuItem_Click;
            settingsMenuItem.DropDownItems.Add(showSaveLocationMenuItem);
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
                RouteManager.SortAndScrollToFirstIncomplete(routeGrid);

                // Auto-save progress
                routeManager?.AutoSaveProgress();
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

            autoStartCheckBox = new CheckBox
            {
                Text = "Auto-Start Game",
                Visible = false,
                Dock = DockStyle.Top
            };
            autoStartCheckBox.CheckedChanged += AutoStartCheckBox_CheckedChanged;
            this.Controls.Add(autoStartCheckBox);
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
        private void LoadRouteData(DataGridView routeGrid)
        {
            if (routeManager != null)
            {
                string selectedGame = GetSelectedGameName();
                routeManager.LoadRouteDataIntoGrid(routeGrid, selectedGame);
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
        private void UpdateRouteCompletionStatus(DataGridView routeGrid, StatsUpdatedEventArgs stats)
        {
            // Delegate all the route checking logic to the RouteManager
            routeManager?.UpdateCompletionStatus(routeGrid, stats);
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
            bool connected = await gameConnectionManager.ConnectToGameAsync(selectedGame, autoStartCheckBox.Checked);

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
        private void GameStats_StatsUpdated(object? sender, StatsUpdatedEventArgs e)
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
                    var (storyMissions, templarHunts, legendaryShips, treasureMaps) =
                        gameConnectionManager.GameStats?.GetSpecialActivityCounts() ?? (0, 0, 0, 0);

                    string statsText =
                        $"Completion Percentage: {e.Percent}%\n" +
                        $"Completion Percentage Exact: {Math.Round(e.PercentFloat, 2)}%\n" +
                        $"Viewpoints Completed: {e.Viewpoints}\n" +
                        $"Myan Stones Collected: {e.Myan}\n" +
                        $"Buried Treasure Collected: {e.Treasure}\n" +
                        $"AnimusFragments Collected: {e.Fragments}\n" +
                        $"AssassinContracts Completed: {e.Assassin}\n" +
                        $"NavalContracts Completed: {e.Naval}\n" +
                        $"LetterBottles Collected: {e.Letters}\n" +
                        $"Manuscripts Collected: {e.Manuscripts}\n" +
                        $"Music Sheets Collected: {e.Music}\n" +
                        $"Forts Captured: {e.Forts}\n" +
                        $"Taverns unlocked: {e.Taverns}\n" +
                        $"Total Chests Collected: {e.TotalChests}\n" +
                        $"Story Missions Completed: {storyMissions}\n" +
                        $"Templar Hunts Completed: {templarHunts}\n" +
                        $"Legendary Ships Defeated: {legendaryShips}\n" +
                        $"Treasure Maps Collected: {treasureMaps}";

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
            settingsManager.LoadSettings(gameDirectoryTextBox, autoStartCheckBox);

            // Apply the Always On Top setting
            this.TopMost = settingsManager.GetAlwaysOnTop();

            // Update menu item if it exists
            if (MainMenuStrip?.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Text == "Settings") is ToolStripMenuItem settingsMenuItem)
            {
                if (settingsMenuItem.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Text == "Always On Top") is ToolStripMenuItem alwaysOnTopMenuItem)
                {
                    alwaysOnTopMenuItem.Checked = this.TopMost;
                }
            }
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
            settingsManager.SaveSettings(gameDirectoryTextBox.Text, autoStartCheckBox.Checked);
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
        // Event handler for auto-start menu item state changes
        // Synchronizes checkbox with menu item and saves settings
        // ==========MY NOTES==============
        // Makes sure the checkbox matches the menu item when you click it
        [SupportedOSPlatform("windows6.1")]
        private void AutoStartMenuItem_CheckedChanged(object? sender, EventArgs e)
        {
            ToolStripMenuItem? autoStartMenuItem = sender as ToolStripMenuItem;
            autoStartCheckBox.Checked = autoStartMenuItem?.Checked ?? false;
            SaveSettings();
        }

        // ==========FORMAL COMMENT=========
        // Event handler for auto-start checkbox state changes
        // Updates settings and prompts for game directory if needed
        // ==========MY NOTES==============
        // Runs when the auto-start checkbox is checked or unchecked
        // Asks for the game folder if we don't know where it is yet
        [SupportedOSPlatform("windows6.1")]
        private void AutoStartCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (settingsManager.IsLoadingSettings)
            {
                return;
            }

            // Add null check for MainMenuStrip
            if (this.MainMenuStrip == null)
            {
                MessageBox.Show("Menu strip not found.");
                autoStartCheckBox.Checked = false;
                return;
            }
            
            ToolStripComboBox? gameDropdown = this.MainMenuStrip.Items.OfType<ToolStripComboBox>().FirstOrDefault();
            if (gameDropdown == null)
            {
                MessageBox.Show("Game dropdown not found.");
                autoStartCheckBox.Checked = false;
                return;
            }

            string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;
            string gameDirectory = settingsManager.GetGameDirectory(selectedGame);

            if (string.IsNullOrEmpty(gameDirectory))
            {
                using FolderBrowserDialog folderBrowserDialog = new();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    gameDirectoryTextBox.Text = folderBrowserDialog.SelectedPath;
                    // Save the directory to the settings manager
                    settingsManager.SaveDirectory(selectedGame, folderBrowserDialog.SelectedPath);
                    SaveSettings();
                }
                else
                {
                    autoStartCheckBox.Checked = false;
                }
            }
            else
            {
                gameDirectoryTextBox.Text = gameDirectory;
                SaveSettings();
            }
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Settings menu item click
        // Toggles the visibility of the settings panel in the UI
        // ==========MY NOTES==============
        // Shows or hides the settings panel when you click the menu item
        [SupportedOSPlatform("windows6.1")]
        private void SettingsMenuItem_Click(object? sender, EventArgs e)
        {
            if (this.Controls["settingsPanel"] is Panel settingsPanel)
            {
                settingsPanel.Visible = !settingsPanel.Visible;
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
        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
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

            string apiUrl = $"https://api.github.com/repos/{AppInfo.GitHubRepo}/releases/latest";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RouteTrackerUpdater");

            try
            {
                var response = await client.GetStringAsync(apiUrl);
                using var doc = JsonDocument.Parse(response);
                string? latestVersion = doc.RootElement.GetProperty("tag_name").GetString();
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
                        var assets = doc.RootElement.GetProperty("assets");
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
