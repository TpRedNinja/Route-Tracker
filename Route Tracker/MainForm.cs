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
    // ==========MY NOTES==============
    // This is the main window of the app - it does everything from connecting to the game
    // to showing the stats and managing settings
    public partial class MainForm : Form
    {

        #region Fields and Properties
        // ==========MY NOTES==============
        // All the important variables that keep track of the app's state

        // Game connection and management
        private readonly string currentProcess = string.Empty;
        private RouteManager? routeManager;
        private readonly GameConnectionManager gameConnectionManager;
        private readonly SettingsManager settingsManager;
        private LayoutSettingsForm.LayoutMode currentLayoutMode = LayoutSettingsForm.LayoutMode.Normal;

        public RouteManager? GetRouteManager() => routeManager;
        public void SetRouteManager(RouteManager manager) => routeManager = manager;
        public GameConnectionManager GameConnectionManager => gameConnectionManager;
        public void LoadRouteDataPublic() => RouteHelpers.LoadRouteData(this, routeManager, routeGrid, settingsManager);
        public RouteManager CreateDefaultRouteManager() => new(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes", "AC4 100 % Route - Main Route.tsv"),
            gameConnectionManager);
        public LayoutSettingsForm.LayoutMode GetCurrentLayoutMode() => currentLayoutMode;
        public void SetHotkeysEnabled(bool enabled) => isHotkeysEnabled = enabled;
        public void SetCurrentLayoutMode(LayoutSettingsForm.LayoutMode mode) => currentLayoutMode = mode;
        public void RefreshAutoStartDropdownPublic() => SettingsMenuManager.RefreshAutoStartDropdown(this, settingsManager);
        public DateTime GetLastUIUpdateTime() => _lastUIUpdateTime;
        public TimeSpan GetMinimumUIUpdateInterval() => _minimumUIUpdateInterval;
        public void SetLastUIUpdateTime(DateTime time) => _lastUIUpdateTime = time;
        public void UpdateRouteCompletionStatusPublic(GameStatsEventArgs stats) =>
    RouteHelpers.UpdateRouteCompletionStatus(routeManager, routeGrid, stats, completionLabel);
        public void LoadRouteDataPublicManager() => RouteHelpers.LoadRouteData(this, routeManager, routeGrid, settingsManager);
        public bool IsHotkeysEnabled => isHotkeysEnabled;

        // Main UI controls
        public Button showStatsButton = null!;
        public Button showCompletionButton = null!;
        public DataGridView routeGrid = null!;
        public Label completionLabel = null!;
        public TextBox gameDirectoryTextBox = null!;

        // Menu and settings controls
        public ToolStripComboBox? autoStartGameComboBox;
        public ToolStripMenuItem? enableAutoStartMenuItem;

        // Filtering and search controls
        public ComboBox typeFilterComboBox = null!;
        public TextBox searchTextBox = null!;
        public Button clearFiltersButton = null!;
        public List<RouteEntry> allRouteEntries = [];

        // Application state
        private bool isHotkeysEnabled = false;
        private readonly string lastSelectedGame = string.Empty;

        // UI update throttling
        private DateTime _lastUIUpdateTime = DateTime.MinValue;
        private readonly TimeSpan _minimumUIUpdateInterval = TimeSpan.FromMilliseconds(100);
        #endregion

        #region Constructor and Initialization
        // ==========MY NOTES==============
        // This runs when the app starts - sets everything up in the right order
        [SupportedOSPlatform("windows6.1")]
        public MainForm()
        {
            InitializeComponent();

            // Initialize managers FIRST
            gameConnectionManager = new GameConnectionManager();
            gameConnectionManager.StatsUpdated += (s, e) => RouteHelpers.GameStats_StatsUpdated(this, e);
            settingsManager = new SettingsManager();

            // Now initialize UI components that need settingsManager
            InitializeCustomComponents();

            // Auto-detect on startup
            AutoDetectGameOnStartup();

            SettingsLifecycleManager.LoadSettings(this, settingsManager, gameDirectoryTextBox);
            settingsManager.CheckFirstRun();
            SettingsMenuManager.RefreshAutoStartDropdown(this, settingsManager);
            this.FormClosing += MainForm_FormClosing;
            this.Text = $"Route Tracker {AppTheme.Version}";
        }

        // ==========MY NOTES==============
        // Looks for games that are already running when the app starts
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
        #endregion

        #region UI Layout and Component Creation
        // ==========MY NOTES==============
        // Sets up the entire UI from scratch since we're not using the designer
        [SupportedOSPlatform("windows6.1")]
        private void InitializeCustomComponents()
        {
            this.Text = "Route Tracker";

            this.MinimumSize = new Size(600, 200);

            InitializeHiddenControls();

            // Create main layout structure
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.BackgroundColor,
                ColumnCount = 1,
                RowCount = 3,
                AutoSize = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Create each row
            mainLayout.Controls.Add(CreateTopBarRow(), 0, 0);
            mainLayout.Controls.Add(CreateCompletionLabelRow(), 0, 1);
            mainLayout.Controls.Add(CreateRouteGridRow(), 0, 2);

            this.Controls.Add(mainLayout);
            AppTheme.ApplyTo(this);
        }

        // ==========MY NOTES==============
        // Creates the top row with menu, buttons, and filter controls
        private FlowLayoutPanel CreateTopBarRow()
        {
            var topBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = AppTheme.BackgroundColor,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Settings menu
            var menuStrip = new MenuStrip
            {
                BackColor = AppTheme.BackgroundColor,
                ForeColor = AppTheme.TextColor,
                Dock = DockStyle.None
            };
            SettingsMenuManager.CreateSettingsMenu(this, menuStrip, settingsManager);
            topBar.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            // Connect button
            var connectButton = new Button
            {
                Text = "Connect to Game",
                MinimumSize = new Size(100, 25),
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(5, 2, 5, 2)
            };
            connectButton.Click += async (s, e) =>
            {
                using var connectionWindow = new ConnectionWindow(gameConnectionManager, settingsManager);
                if (connectionWindow.ShowDialog(this) == DialogResult.OK)
                {
                    string selectedGame = connectionWindow.SelectedGame;
                    bool shouldAutoStart = enableAutoStartMenuItem?.Checked == true &&
                                          !string.IsNullOrEmpty(Settings.Default.AutoStart) &&
                                          Settings.Default.AutoStart == selectedGame;

                    bool connected = await gameConnectionManager.ConnectToGameAsync(selectedGame, shouldAutoStart);

                    if (connected)
                    {
                        string routeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes", "AC4 100 % Route - Main Route.tsv");
                        routeManager = new RouteManager(routeFilePath, gameConnectionManager);
                        RouteHelpers.LoadRouteData(this, routeManager, routeGrid, settingsManager);
                    }
                }
            };
            topBar.Controls.Add(connectButton);

            // Game Stats button
            showStatsButton = new Button
            {
                Text = "Game Stats",
                MinimumSize = new Size(80, 25),
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(5, 2, 5, 2)
            };
            showStatsButton.Click += (s, e) => RouteHelpers.ShowStatsWindow(this, gameConnectionManager);
            topBar.Controls.Add(showStatsButton);

            // Route Stats button
            showCompletionButton = new Button
            {
                Text = "Route Stats",
                MinimumSize = new Size(80, 25),
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(5, 2, 15, 2)
            };
            showCompletionButton.Click += (s, e) => RouteHelpers.ShowCompletionStatsWindow(this, routeManager);
            topBar.Controls.Add(showCompletionButton);

            // Search box
            searchTextBox = new TextBox
            {
                PlaceholderText = "Search...",
                Width = 120,
                Height = 23,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10, 2, 5, 2)
            };
            searchTextBox.TextChanged += (s, e) => RouteHelpers.ApplyFilters(this);
            topBar.Controls.Add(searchTextBox);

            // Type filter dropdown
            typeFilterComboBox = new ComboBox
            {
                Width = 100,
                Height = 23,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 2, 5, 2)
            };
            typeFilterComboBox.Items.Add("All Types");
            typeFilterComboBox.SelectedIndex = 0;
            typeFilterComboBox.SelectedIndexChanged += (s, e) => RouteHelpers.ApplyFilters(this);
            topBar.Controls.Add(typeFilterComboBox);

            // Clear filters button
            clearFiltersButton = new Button
            {
                Text = "Clear",
                Width = 50,
                Height = 23,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(0, 2, 5, 2)
            };
            clearFiltersButton.Click += (s, e) => RouteHelpers.ClearFilters(this);
            topBar.Controls.Add(clearFiltersButton);

            AppTheme.ApplyToButton(connectButton);
            AppTheme.ApplyToButton(showStatsButton);
            AppTheme.ApplyToButton(showCompletionButton);
            AppTheme.ApplyToButton(clearFiltersButton);

            return topBar;
        }

        // ==========MY NOTES==============
        // Creates the completion percentage label row
        private Panel CreateCompletionLabelRow()
        {
            var labelPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = AppTheme.BackgroundColor,
                Padding = new Padding(10, 0, 0, 0)
            };

            completionLabel = new Label
            {
                Text = "Completion: 0.00%",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppTheme.TextColor,
                Font = new Font(AppTheme.DefaultFont.FontFamily, AppTheme.DefaultFont.Size + 2),
                Location = new Point(10, 0),
                Padding = new Padding(0, 3, 0, 3)
            };

            labelPanel.Controls.Add(completionLabel);
            return labelPanel;
        }

        // ==========MY NOTES==============
        // Creates the main route grid row
        private Panel CreateRouteGridRow()
        {
            routeGrid = MainFormHelpers.CreateRouteGridView();
            routeGrid.MouseClick += (s, e) => MainFormHelpers.HandleRouteGridMouseClick(this, routeManager, e);
            routeGrid.Dock = DockStyle.Fill;

            var routeGridPanel = new Panel
            {
                Name = "routeGridPanel",
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 10, 10),
                BackColor = AppTheme.BackgroundColor
            };
            routeGridPanel.Controls.Add(routeGrid);

            return routeGridPanel;
        }

        // ==========MY NOTES==============
        // Sets up invisible controls for settings storage
        [SupportedOSPlatform("windows6.1")]
        private void InitializeHiddenControls()
        {
            gameDirectoryTextBox = new TextBox
            {
                ReadOnly = true,
                Visible = false,
                Width = 600,
                Dock = DockStyle.Top
            };
            this.Controls.Add(gameDirectoryTextBox);
        }
        #endregion

        #region Hotkey Management
        // ==========MY NOTES==============
        // Catches key presses and runs hotkey actions if hotkeys are enabled
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool handled = MainFormHelpers.ProcessCmdKey(this, settingsManager, routeManager, ref msg, keyData);
            return handled || base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Application Lifecycle
        // ==========MY NOTES==============
        // Handles app startup - auto-start logic and update checks
        [SupportedOSPlatform("windows6.1")]
        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await SettingsLifecycleManager.HandleApplicationStartup(this, settingsManager, gameConnectionManager);
        }

        // ==========MY NOTES==============
        // Handles app shutdown - cleans up resources properly
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            RouteHelpers.CleanupGameStats(gameConnectionManager);
        }
        #endregion
    }
}