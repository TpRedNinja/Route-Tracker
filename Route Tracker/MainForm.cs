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
        public RouteManager? routeManager;
        public readonly GameConnectionManager gameConnectionManager;
        public readonly SettingsManager settingsManager;
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
        public void LoadRouteDataPublicManager() => RouteHelpers.LoadRouteData(this, routeManager, routeGrid, settingsManager);
        public bool IsHotkeysEnabled => isHotkeysEnabled;

        // Main UI controls
        public Button showStatsButton = null!;
        public Button showCompletionButton = null!;
        public DataGridView routeGrid = null!;
        public Label completionLabel = null!;
        public Label currentLocationLabel = null!;
        public TextBox gameDirectoryTextBox = null!;
        private Label helpShortcutLabel = null!;

        // Menu and settings controls
        public ToolStripComboBox? autoStartGameComboBox;
        public ToolStripMenuItem? enableAutoStartMenuItem;

        // Filtering and search controls
        public CheckedListBox typeFilterCheckedListBox = null!;
        public TextBox searchTextBox = null!;
        public Button clearFiltersButton = null!;
        public List<RouteEntry> allRouteEntries = [];
        public HashSet<string> selectedTypes = [];

        //search history controls
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044",
        Justification = "NO")]
        private SearchHistoryManager searchHistoryManager = null!;
        private ListBox searchHistoryListBox = null!;
        private bool isSearchHistoryVisible = false;
        private string lastSearchTerm = string.Empty;

        // Application state
        private bool isHotkeysEnabled = false;
        private readonly string lastSelectedGame = string.Empty;

        // global hotkey support
        private bool globalHotkeysRegistered = false;

        #endregion

        #region Constructor and Initialization
        // ==========MY NOTES==============
        // This runs when the app starts - sets everything up in the right order
        [SupportedOSPlatform("windows6.1")]
        public MainForm()
        {
            InitializeComponent();
            this.KeyPreview = true;

            // Initialize managers FIRST
            gameConnectionManager = new GameConnectionManager();
            gameConnectionManager.StatsUpdated += (s, e) => RouteHelpers.GameStats_StatsUpdated(this, e);
            settingsManager = new SettingsManager();
            searchHistoryManager = new SearchHistoryManager();

            // Now initialize UI components that need settingsManager
            InitializeCustomComponents();

            // load icon on start with fallback image
            try
            {
                var iconPath = "ProjectIcon_Main.ico";
                this.Icon = new Icon(iconPath);
                LoggingSystem.LogInfo($"Main icon loaded successfully: {iconPath}");
            }
            catch (Exception exMain)
            {
                LoggingSystem.LogWarning($"Failed to load main icon: {exMain.Message}");
                try
                {
                    var altIconPath = "ProjectIcon_Alt.ico";
                    this.Icon = new Icon(altIconPath);
                    LoggingSystem.LogInfo($"Fallback icon loaded successfully: {altIconPath}");
                }
                catch (Exception exAlt)
                {
                    LoggingSystem.LogError("Failed to load both main and fallback icons", exAlt);
                }
            }

            // Auto-detect on startup
            AutoDetectGameOnStartup();

            SettingsLifecycleManager.LoadSettings(this, settingsManager, gameDirectoryTextBox);
            settingsManager.CheckFirstRun();
            SettingsMenuManager.RefreshAutoStartDropdown(this, settingsManager);

            // register global hotkeys if enabled
            UpdateGlobalHotkeys();

            // Load last search term
            LoadLastSearchTerm();

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0019",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0039",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
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
            var connectButton = UIControlFactory.CreateThemedButton("Connect to Game", 100, 25);
            connectButton.MinimumSize = new Size(100, 25);
            connectButton.AutoSize = true;
            connectButton.Margin = new Padding(5, 2, 5, 2);

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
                        routeGrid.SuspendLayout();
                        RouteHelpers.LoadRouteData(this, routeManager, routeGrid, settingsManager);
                        UpdateCurrentLocationLabel();
                        ApplyCurrentSorting();
                        routeGrid.ResumeLayout(true);
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

            // Search box with history support
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

            // Handle text changes for filtering
            searchTextBox.TextChanged += (s, e) => RouteHelpers.ApplyFilters(this);

            // Handle focus events for search history
            searchTextBox.Enter += (s, e) => ShowSearchHistoryDropdown();
            searchTextBox.Leave += (s, e) =>
            {
                // Save search to history when leaving the textbox
                SaveCurrentSearchToHistory();

                // Hide dropdown after a small delay to allow clicking on items
                System.Windows.Forms.Timer hideTimer = new() { Interval = 150 };
                hideTimer.Tick += (sender, args) =>
                {
                    hideTimer.Stop();
                    hideTimer.Dispose();
                    HideSearchHistoryDropdown();
                };
                hideTimer.Start();
            };

            // Handle click to show dropdown even if already focused
            searchTextBox.Click += (s, e) => ShowSearchHistoryDropdown();
            topBar.Controls.Add(searchTextBox);

            // Type filter dropdown
            var typeFilterPanel = new Panel
            {
                Width = 120,
                Height = 23,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(40, 40, 40),
                Margin = new Padding(0, 2, 5, 2)
            };

            var typeFilterLabel = new Label
            {
                Text = "All Types",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                BackColor = Color.FromArgb(40, 40, 40),
                Padding = new Padding(5, 0, 20, 0),
                Cursor = Cursors.Hand
            };

            var dropDownButton = new Button
            {
                Text = "▼",
                Width = 20,
                Height = 21,
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = AppTheme.TextColor,
                Font = new Font(AppTheme.DefaultFont.FontFamily, 8),
                Cursor = Cursors.Hand
            };
            dropDownButton.FlatAppearance.BorderSize = 0;

            typeFilterCheckedListBox = new CheckedListBox
            {
                Width = 120,
                Height = 150,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true,
                Visible = false
            };

            typeFilterPanel.Controls.Add(typeFilterLabel);
            typeFilterPanel.Controls.Add(dropDownButton);
            typeFilterPanel.Controls.Add(typeFilterCheckedListBox);

            // Event handlers for dropdown functionality
            EventHandler toggleDropdown = (s, e) =>
            {
                if (!typeFilterCheckedListBox.Visible)
                {
                    // Show dropdown below the panel, on top of everything
                    var panel = typeFilterCheckedListBox.Parent as Panel;
                    if (panel != null)
                    {
                        var screenPoint = panel.PointToScreen(new Point(0, panel.Height));
                        var formPoint = this.PointToClient(screenPoint);
                        typeFilterCheckedListBox.Location = formPoint;
                        typeFilterCheckedListBox.Parent = this;
                        typeFilterCheckedListBox.BringToFront();
                    }
                }
                typeFilterCheckedListBox.Visible = !typeFilterCheckedListBox.Visible;
            };
            typeFilterLabel.Click += toggleDropdown;
            dropDownButton.Click += toggleDropdown;

            // Handle item check changes
            typeFilterCheckedListBox.ItemCheck += (s, e) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    string item = typeFilterCheckedListBox.Items[e.Index].ToString() ?? "";

                    if (item == "All Types")
                    {
                        if (e.NewValue == CheckState.Checked)
                        {
                            // Select all types
                            for (int i = 1; i < typeFilterCheckedListBox.Items.Count; i++)
                            {
                                typeFilterCheckedListBox.SetItemChecked(i, true);
                            }
                            selectedTypes.Clear();
                            for (int i = 1; i < typeFilterCheckedListBox.Items.Count; i++)
                            {
                                selectedTypes.Add(typeFilterCheckedListBox.Items[i].ToString() ?? "");
                            }
                        }
                        else
                        {
                            // Unselect all types
                            for (int i = 1; i < typeFilterCheckedListBox.Items.Count; i++)
                            {
                                typeFilterCheckedListBox.SetItemChecked(i, false);
                            }
                            selectedTypes.Clear();
                        }
                    }
                    else
                    {
                        if (e.NewValue == CheckState.Checked)
                        {
                            selectedTypes.Add(item);
                        }
                        else
                        {
                            selectedTypes.Remove(item);
                            // Uncheck "All Types" if any individual type is unchecked
                            typeFilterCheckedListBox.SetItemChecked(0, false);
                        }

                        // Check "All Types" if all individual types are selected
                        if (selectedTypes.Count == typeFilterCheckedListBox.Items.Count - 1)
                        {
                            typeFilterCheckedListBox.SetItemChecked(0, true);
                        }
                    }

                    UpdateTypeFilterLabel(typeFilterLabel);
                    RouteHelpers.ApplyFilters(this);
                }));
            };

            // Handle clicking outside to close dropdown
            this.Click += (s, e) =>
            {
                if (typeFilterCheckedListBox.Visible)
                {
                    typeFilterCheckedListBox.Visible = false;
                }
            };

            topBar.Controls.Add(typeFilterPanel);

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

            // Help shortcut label
            helpShortcutLabel = new Label
            {
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                BackColor = AppTheme.BackgroundColor,
                Font = new Font(AppTheme.DefaultFont.FontFamily, AppTheme.DefaultFont.Size, FontStyle.Italic),
                Margin = new Padding(10, 2, 5, 2),
                TextAlign = ContentAlignment.MiddleLeft
            };
            UpdateHelpShortcutLabel();
            topBar.Controls.Add(helpShortcutLabel);

            AppTheme.ApplyToControls(showStatsButton, showCompletionButton, clearFiltersButton);

            var toolTip = new ToolTip();
            toolTip.SetToolTip(connectButton, "Connect to the selected game. If not running, you can auto-start it in the settings panel.");
            toolTip.SetToolTip(showStatsButton, "View the values of certain in game values like percentage, total viewpoints done, etc etc...");
            toolTip.SetToolTip(showCompletionButton, "View your progress and completion statistics for the current route");
            toolTip.SetToolTip(searchTextBox, "Type to search route entries by name, type, or coordinates");
            toolTip.SetToolTip(typeFilterCheckedListBox, "Filter route entries by type.");
            toolTip.SetToolTip(typeFilterPanel, "Select multiple route entry types to filter. Click to open dropdown with checkboxes.");
            toolTip.SetToolTip(clearFiltersButton, "Clear all filters and show all route entries.");

            return topBar;
        }

        public void UpdateRouteCompletionStatusPublic(GameStatsEventArgs stats)
        {
            RouteHelpers.UpdateRouteCompletionStatus(routeManager, routeGrid, stats, completionLabel, settingsManager);
            UpdateCurrentLocationLabel();
        }

        private void UpdateTypeFilterLabel(Label typeFilterLabel)
        {
            if (selectedTypes.Count == 0)
            {
                typeFilterLabel.Text = "None";
            }
            else if (typeFilterCheckedListBox.GetItemChecked(0)) // "All Types" is checked
            {
                typeFilterLabel.Text = "All Types";
            }
            else if (selectedTypes.Count == 1)
            {
                typeFilterLabel.Text = selectedTypes.First();
            }
            else
            {
                typeFilterLabel.Text = "Multiple Types";
            }
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

            // Add the new label for current location
            currentLocationLabel = new Label
            {
                Text = "Current Location: ",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppTheme.TextColor,
                Font = new Font(AppTheme.DefaultFont.FontFamily, AppTheme.DefaultFont.Size + 2),
                Location = new Point(completionLabel.Right + 30, 0),
                Padding = new Padding(20, 3, 0, 3)
            };

            labelPanel.Controls.Add(completionLabel);
            labelPanel.Controls.Add(currentLocationLabel);

            var tooltip = new ToolTip();
            tooltip.SetToolTip(completionLabel, "Shows the current completion percentage based on completed route entries.");
            tooltip.SetToolTip(currentLocationLabel, "Shows the location for the next uncompleted route entry.");

            return labelPanel;
        }

        public void UpdateCurrentLocationLabel()
        {
            if (routeManager != null)
                currentLocationLabel.Text = $"Current Location: {routeManager.GetCurrentLocation()}";
            else
                currentLocationLabel.Text = "Current Location: ";
        }

        // update help shortcut label based on current hotkey settings
        private void UpdateHelpShortcutLabel()
        {
            var shortcuts = settingsManager.GetShortcuts();
            var keysConverter = new KeysConverter();
            string helpKey = keysConverter.ConvertToString(shortcuts.Help) ?? "None";
            helpShortcutLabel.Text = $"Click ({helpKey}) for help";
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
            // Handle hotkeys through the registry system
            if (HotkeyActionRegistry.HandleHotkey(this, keyData))
                return true;

            // Handle remaining hotkeys that need special processing
            bool handled = MainFormHelpers.ProcessCmdKey(this, settingsManager, routeManager, ref msg, keyData);
            return handled || base.ProcessCmdKey(ref msg, keyData);
        }

        // ==========MY NOTES==============
        // Cycles through sorting modes and applies them
        public void CycleSorting(bool forward)
        {
            var currentMode = settingsManager.GetSortingMode();
            var newMode = SortingManager.CycleSortingMode(currentMode, forward);

            settingsManager.SaveSortingMode(newMode);
            SortingManager.ApplySorting(routeGrid, newMode);

            // Show a brief message about the new sorting mode
            string modeName = SortingManager.GetSortingModeName(newMode);
            MessageBox.Show($"Sorting mode changed to: {modeName}", "Sorting Changed",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ==========MY NOTES==============
        // Opens the game directory window
        public void OpenGameDirectory()
        {
            bool wasTopMost = this.TopMost;
            if (wasTopMost)
                this.TopMost = false;

            try
            {
                GameDirectoryForm gameDirectoryForm = new()
                {
                    Owner = this,
                    StartPosition = FormStartPosition.CenterParent
                };

                gameDirectoryForm.DirectoryChanged += (s, args) => RefreshAutoStartDropdownPublic();
                gameDirectoryForm.ShowDialog(this);
            }
            finally
            {
                if (wasTopMost)
                    this.TopMost = true;
            }
        }

        // ==========MY NOTES==============
        // Public method to apply sorting (for use by other classes)
        public void ApplyCurrentSorting()
        {
            var currentMode = settingsManager.GetSortingMode();
            SortingManager.ApplySortingQuiet(routeGrid, currentMode);
        }

        public void CycleLayout(bool forward)
        {
            var modes = Enum.GetValues<LayoutSettingsForm.LayoutMode>();
            int currentIndex = Array.IndexOf(modes, currentLayoutMode);

            if (forward)
            {
                currentIndex = (currentIndex + 1) % modes.Length;
            }
            else
            {
                currentIndex = (currentIndex - 1 + modes.Length) % modes.Length;
            }

            currentLayoutMode = modes[currentIndex];
            settingsManager.SaveLayoutMode(currentLayoutMode);
            LayoutManager.ApplyLayoutMode(this, currentLayoutMode);
        }

        // ==========MY NOTES==============
        // Handles global hotkey messages and window procedure overrides
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            if (m.Msg == WM_HOTKEY)
            {
                var (CompleteHotkey, SkipHotkey, UndoHotkey, GlobalHotkeys, AdvancedHotkeys) = settingsManager.GetAllHotkeySettings();
                if (GlobalHotkeys)
                {
                    int hotkeyId = m.WParam.ToInt32();

                    // Handle main hotkeys (Complete, Skip, Undo)
                    Keys keyPressed = hotkeyId switch
                    {
                        1 => CompleteHotkey,   // HOTKEY_ID_COMPLETE
                        2 => SkipHotkey,       // HOTKEY_ID_SKIP  
                        3 => UndoHotkey,       // HOTKEY_ID_UNDO
                        _ => Keys.None
                    };

                    if (keyPressed != Keys.None)
                    {
                        MainFormHelpers.ProcessGlobalHotkey(this, settingsManager, routeManager, keyPressed);
                        base.WndProc(ref m);
                        return;
                    }

                    // Handle shortcuts (ID 100+)
                    if (hotkeyId >= 100)
                    {
                        var shortcuts = settingsManager.GetShortcuts();
                        var shortcutKeys = new Keys[]
                        {
                    shortcuts.Load, shortcuts.Save, shortcuts.LoadProgress, shortcuts.ResetProgress,
                    shortcuts.Refresh, shortcuts.Help, shortcuts.FilterClear, shortcuts.Connect,
                    shortcuts.GameStats, shortcuts.RouteStats, shortcuts.LayoutUp, shortcuts.LayoutDown,
                    shortcuts.BackupFolder, shortcuts.BackupNow, shortcuts.Restore, shortcuts.SetFolder,
                    shortcuts.AutoTog, shortcuts.TopTog, shortcuts.AdvTog, shortcuts.GlobalTog,
                    shortcuts.SortingUp, shortcuts.SortingDown, shortcuts.GameDirect
                        };

                        int shortcutIndex = hotkeyId - 100;
                        if (shortcutIndex >= 0 && shortcutIndex < shortcutKeys.Length)
                        {
                            Keys shortcutKey = shortcutKeys[shortcutIndex];
                            if (shortcutKey != Keys.None)
                            {
                                // Use HotkeyActionRegistry to handle the shortcut
                                HotkeyActionRegistry.HandleHotkey(this, shortcutKey);
                            }
                        }
                    }
                }
            }

            base.WndProc(ref m);
        }

        // ==========MY NOTES==============
        // Registers or unregisters global hotkeys based on settings
        public void UpdateGlobalHotkeys()
        {
            var hotkeySettings = settingsManager.GetAllHotkeySettings();

            if (hotkeySettings.GlobalHotkeys && !globalHotkeysRegistered)
            {
                // Use the new low-level hook system and pass the main form reference
                GlobalHotkeyManager.Install(this);
                RegisterLowLevelHotkeys(hotkeySettings);
                globalHotkeysRegistered = true;
            }
            else if (!hotkeySettings.GlobalHotkeys && globalHotkeysRegistered)
            {
                GlobalHotkeyManager.Uninstall();
                globalHotkeysRegistered = false;
            }
        }

        // ==========MY NOTES==============
        // Registers hotkeys using the low-level hook system
        private void RegisterLowLevelHotkeys((Keys CompleteHotkey, Keys SkipHotkey, Keys UndoHotkey, bool GlobalHotkeys, bool AdvancedHotkeys) settings)
        {
            // Register main hotkeys
            if (settings.CompleteHotkey != Keys.None)
            {
                GlobalHotkeyManager.RegisterHotkey(settings.CompleteHotkey, () =>
                    MainFormHelpers.ProcessGlobalHotkey(this, settingsManager, routeManager, settings.CompleteHotkey));
            }

            if (settings.SkipHotkey != Keys.None)
            {
                GlobalHotkeyManager.RegisterHotkey(settings.SkipHotkey, () =>
                    MainFormHelpers.ProcessGlobalHotkey(this, settingsManager, routeManager, settings.SkipHotkey));
            }

            if (settings.UndoHotkey != Keys.None)
            {
                GlobalHotkeyManager.RegisterHotkey(settings.UndoHotkey, () =>
                    MainFormHelpers.ProcessGlobalHotkey(this, settingsManager, routeManager, settings.UndoHotkey));
            }

            // Register all shortcuts
            var shortcuts = settingsManager.GetShortcuts();
            var shortcutKeys = new Keys[]
            {
        shortcuts.Load, shortcuts.Save, shortcuts.LoadProgress, shortcuts.ResetProgress,
        shortcuts.Refresh, shortcuts.Help, shortcuts.FilterClear, shortcuts.Connect,
        shortcuts.GameStats, shortcuts.RouteStats, shortcuts.LayoutUp, shortcuts.LayoutDown,
        shortcuts.BackupFolder, shortcuts.BackupNow, shortcuts.Restore, shortcuts.SetFolder,
        shortcuts.AutoTog, shortcuts.TopTog, shortcuts.AdvTog, shortcuts.GlobalTog,
        shortcuts.SortingUp, shortcuts.SortingDown, shortcuts.GameDirect
            };

            foreach (var shortcutKey in shortcutKeys)
            {
                if (shortcutKey != Keys.None)
                {
                    Keys capturedKey = shortcutKey; // Capture for closure
                    GlobalHotkeyManager.RegisterHotkey(capturedKey, () =>
                        HotkeyActionRegistry.HandleHotkey(this, capturedKey));
                }
            }
        }

        //so other files can use this function
        public void RefreshHelpShortcutLabel()
        {
            UpdateHelpShortcutLabel();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void RegisterGlobalHotkeys((Keys CompleteHotkey, Keys SkipHotkey, Keys UndoHotkey, bool GlobalHotkeys, bool AdvancedHotkeys) settings)
        {
            const int HOTKEY_ID_COMPLETE = 1;
            const int HOTKEY_ID_SKIP = 2;
            const int HOTKEY_ID_UNDO = 3;

            // Start IDs for shortcuts at 100 to avoid conflicts
            int shortcutIdBase = 100;

            // Helper method to convert Keys to modifier and virtual key
            static (int modifiers, int vkey) ConvertKeys(Keys keys)
            {
                int modifiers = 0;
                int vkey = (int)(keys & Keys.KeyCode);

                if ((keys & Keys.Control) == Keys.Control)
                    modifiers |= 0x0002; // MOD_CONTROL
                if ((keys & Keys.Alt) == Keys.Alt)
                    modifiers |= 0x0001; // MOD_ALT
                if ((keys & Keys.Shift) == Keys.Shift)
                    modifiers |= 0x0004; // MOD_SHIFT

                return (modifiers, vkey);
            }

            // Register the main hotkeys
            if (settings.CompleteHotkey != Keys.None)
            {
                var (modifiers, vkey) = ConvertKeys(settings.CompleteHotkey);
                RegisterHotKey(this.Handle, HOTKEY_ID_COMPLETE, modifiers, vkey);
            }

            if (settings.SkipHotkey != Keys.None)
            {
                var (modifiers, vkey) = ConvertKeys(settings.SkipHotkey);
                RegisterHotKey(this.Handle, HOTKEY_ID_SKIP, modifiers, vkey);
            }

            if (settings.UndoHotkey != Keys.None)
            {
                var (modifiers, vkey) = ConvertKeys(settings.UndoHotkey);
                RegisterHotKey(this.Handle, HOTKEY_ID_UNDO, modifiers, vkey);
            }

            // Register ALL shortcuts globally
            var shortcuts = settingsManager.GetShortcuts();

            // Create array of all shortcut keys for easier processing
            var shortcutKeys = new Keys[]
            {
        shortcuts.Load, shortcuts.Save, shortcuts.LoadProgress, shortcuts.ResetProgress,
        shortcuts.Refresh, shortcuts.Help, shortcuts.FilterClear, shortcuts.Connect,
        shortcuts.GameStats, shortcuts.RouteStats, shortcuts.LayoutUp, shortcuts.LayoutDown,
        shortcuts.BackupFolder, shortcuts.BackupNow, shortcuts.Restore, shortcuts.SetFolder,
        shortcuts.AutoTog, shortcuts.TopTog, shortcuts.AdvTog, shortcuts.GlobalTog,
        shortcuts.SortingUp, shortcuts.SortingDown, shortcuts.GameDirect
            };

            // Register each shortcut with a unique ID
            for (int i = 0; i < shortcutKeys.Length; i++)
            {
                if (shortcutKeys[i] != Keys.None)
                {
                    var (modifiers, vkey) = ConvertKeys(shortcutKeys[i]);
                    RegisterHotKey(this.Handle, shortcutIdBase + i, modifiers, vkey);
                }
            }
        }

        private void UnregisterGlobalHotkeys()
        {
            const int HOTKEY_ID_COMPLETE = 1;
            const int HOTKEY_ID_SKIP = 2;
            const int HOTKEY_ID_UNDO = 3;

            // Unregister main hotkeys
            UnregisterHotKey(this.Handle, HOTKEY_ID_COMPLETE);
            UnregisterHotKey(this.Handle, HOTKEY_ID_SKIP);
            UnregisterHotKey(this.Handle, HOTKEY_ID_UNDO);

            // Unregister all shortcuts (IDs 100-122)
            for (int i = 0; i < 23; i++) // 23 shortcuts total
            {
                UnregisterHotKey(this.Handle, 100 + i);
            }
        }
        #endregion

        #region Search History Management
        // ==========MY NOTES==============
        // Loads the last search term when the app starts
        private void LoadLastSearchTerm()
        {
            searchTextBox.Text = "";
        }

        // ==========MY NOTES==============
        // Saves the current search term to history when user clicks away
        private void SaveCurrentSearchToHistory()
        {
            string currentSearch = searchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(currentSearch) && currentSearch != lastSearchTerm)
            {
                _ = searchHistoryManager.AddSearchHistoryAsync(currentSearch);
                lastSearchTerm = currentSearch;
            }
        }

        // ==========MY NOTES==============
        // Shows the search history dropdown when clicking on the search box
        private void ShowSearchHistoryDropdown()
        {
            var history = searchHistoryManager.LoadSearchHistory();
            if (history.Count == 0)
                return;

            if (searchHistoryListBox == null)
            {
                CreateSearchHistoryListBox();
            }

            // Clear and populate the dropdown
            searchHistoryListBox!.Items.Clear();
            foreach (string term in history.Take(10)) // Show max 10 items
            {
                searchHistoryListBox.Items.Add(term);
            }

            if (searchHistoryListBox.Items.Count > 0)
            {
                // Position the dropdown below the search box
                var searchBoxLocation = searchTextBox.PointToScreen(Point.Empty);
                var formLocation = this.PointToClient(searchBoxLocation);

                searchHistoryListBox.Location = new Point(formLocation.X, formLocation.Y + searchTextBox.Height);
                searchHistoryListBox.Width = searchTextBox.Width;

                // Calculate height based on items (max 10 items visible)
                int itemHeight = searchHistoryListBox.ItemHeight;
                int visibleItems = Math.Min(searchHistoryListBox.Items.Count, 10);
                searchHistoryListBox.Height = visibleItems * itemHeight + 4; // +4 for borders

                searchHistoryListBox.Visible = true;
                searchHistoryListBox.BringToFront();
                isSearchHistoryVisible = true;
            }
        }

        // ==========MY NOTES==============
        // Creates the search history dropdown listbox
        private void CreateSearchHistoryListBox()
        {
            searchHistoryListBox = new ListBox
            {
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                IntegralHeight = false // Allow custom height
            };

            // Handle item selection
            searchHistoryListBox.Click += (s, e) =>
            {
                if (searchHistoryListBox.SelectedItem != null)
                {
                    string selectedTerm = searchHistoryListBox.SelectedItem.ToString() ?? "";
                    searchTextBox.Text = selectedTerm;
                    HideSearchHistoryDropdown();

                    // Apply the filter immediately
                    RouteHelpers.ApplyFilters(this);

                    // Focus back to search box
                    searchTextBox.Focus();
                }
            };

            // Handle mouse leave to hide dropdown
            searchHistoryListBox.MouseLeave += (s, e) =>
            {
                // Small delay to allow clicking on items
                System.Windows.Forms.Timer hideTimer = new() { Interval = 100 };
                hideTimer.Tick += (sender, args) =>
                {
                    hideTimer.Stop();
                    hideTimer.Dispose();
                    if (!searchHistoryListBox.ClientRectangle.Contains(searchHistoryListBox.PointToClient(Cursor.Position)))
                    {
                        HideSearchHistoryDropdown();
                    }
                };
                hideTimer.Start();
            };

            this.Controls.Add(searchHistoryListBox);
        }

        // ==========MY NOTES==============
        // Hides the search history dropdown
        private void HideSearchHistoryDropdown()
        {
            if (searchHistoryListBox != null && isSearchHistoryVisible)
            {
                searchHistoryListBox.Visible = false;
                isSearchHistoryVisible = false;
            }
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
        private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Save current search term to history before closing
            SaveCurrentSearchToHistory();

            // Cleanup global hotkeys
            if (globalHotkeysRegistered)
            {
                GlobalHotkeyManager.Uninstall();
            }

            RouteHelpers.CleanupGameStats(gameConnectionManager);

            SettingsLifecycleManager.StopBackgroundAutoConnection();

            // Cleanup layout manager cache to prevent memory leaks
            LayoutManager.DisposeCache();

            // Handle logging system exit process
            try
            {
                // Cancel the closing temporarily to handle async email sending
                if (!e.Cancel)
                {
                    e.Cancel = true;

                    // Handle logging system exit in background
                    await LoggingSystem.HandleApplicationExit();

                    // Now actually close the application without showing designer errors
                    this.FormClosing -= MainForm_FormClosing; // Prevent recursion

                    // Force immediate exit to prevent IUIService errors
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError("Error during application exit", ex);
                // Force exit even if logging fails to prevent IUIService errors
                Environment.Exit(0);
            }
        }
        #endregion
    }
}