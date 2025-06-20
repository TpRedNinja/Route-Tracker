using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
using Assassin_s_Creed_Route_Tracker.Properties;

namespace Assassin_s_Creed_Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Main form class that provides the user interface for the Assassin's Creed Route Tracker
    // Handles connections to game processes, memory reading, and displaying game statistics
    // ==========MY NOTES==============
    // This is the main window of the app - it does everything from connecting to the game
    // to showing the stats and managing settings
    public partial class MainForm : Form
    {
        // ==========FORMAL COMMENT=========
        // Fields for process interaction and game memory access
        // ==========MY NOTES==============
        // These variables help us connect to the game and read its memory
        private IntPtr processHandle;
        private string currentProcess;
        private IntPtr baseAddress;
        private GameStats gameStats;
        private RouteManager routeManager;

        private const int PROCESS_WM_READ = 0x0010;
        private bool isLoadingSettings = false;

        private TabControl tabControl;
        private TabPage statsTabPage;
        private TabPage routeTabPage;

        // ==========FORMAL COMMENT=========
        // Constructor - initializes the form and loads user settings
        // ==========MY NOTES==============
        // This runs when the app starts - sets everything up
        [SupportedOSPlatform("windows6.1")]
        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadSettings();
        }

        private TextBox gameDirectoryTextBox;
        private CheckBox autoStartCheckBox;

        // ==========FORMAL COMMENT=========
        // Creates all custom UI components for the application interface
        // Sets up the menu, tabs, buttons, and other controls
        // ==========MY NOTES==============
        // This builds all the buttons, tabs, and other stuff you see in the app
        [SupportedOSPlatform("windows6.1")]
        private void InitializeCustomComponents()
        {
            this.Text = "Assassin's Creed Route Tracker";
            this.BackColor = System.Drawing.Color.Black;
            this.ForeColor = System.Drawing.Color.White;

            // Create and configure the MenuStrip
            MenuStrip menuStrip = new()
            {
                Dock = DockStyle.Top, // Dock the MenuStrip at the top of the form
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White
            };

            // Create and configure the Settings menu item
            ToolStripMenuItem settingsMenuItem = new("Settings");

            // Create and configure the Auto-Start Game menu item
            ToolStripMenuItem autoStartMenuItem = new("Auto-Start Game")
            {
                CheckOnClick = true
            };
            autoStartMenuItem.CheckedChanged += AutoStartMenuItem_CheckedChanged;

            // Create and configure the Game Directory menu item
            ToolStripMenuItem gameDirectoryMenuItem = new("Game Directory");
            gameDirectoryMenuItem.Click += GameDirectoryMenuItem_Click;

            // Add the Auto-Start Game and Game Directory menu items to the Settings menu item
            settingsMenuItem.DropDownItems.Add(autoStartMenuItem);
            settingsMenuItem.DropDownItems.Add(gameDirectoryMenuItem);

            // Add the Settings menu item to the MenuStrip
            menuStrip.Items.Add(settingsMenuItem);

            // Create and configure the Stats tab button
            ToolStripButton statsTabButton = new("Stats");
            statsTabButton.Click += (sender, e) => tabControl.SelectedTab = statsTabPage;
            menuStrip.Items.Add(statsTabButton);

            // Create and configure the Route tab button
            ToolStripButton routeTabButton = new("Route");
            routeTabButton.Click += (sender, e) => tabControl.SelectedTab = routeTabPage;
            menuStrip.Items.Add(routeTabButton);

            // Create and configure the connection label
            ToolStripLabel connectionLabel = new()
            {
                Text = "Not connected"
            };
            menuStrip.Items.Add(connectionLabel);

            // Create and configure the game dropdown
            ToolStripComboBox gameDropdown = new();
            gameDropdown.Items.AddRange(["", "Assassin's Creed 4", "Assassin's Creed Syndicate"]);
            gameDropdown.SelectedIndex = 0;
            menuStrip.Items.Add(gameDropdown);

            // Create and configure the connect button
            ToolStripButton connectButton = new("Connect to Game");
            connectButton.Click += ConnectButton_Click;
            menuStrip.Items.Add(connectButton);

            // Set the MenuStrip as the main menu strip of the form
            this.MainMenuStrip = menuStrip;

            // Add the MenuStrip to the form's controls
            this.Controls.Add(menuStrip);

            // Create and configure the TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Create and configure the Stats TabPage
            statsTabPage = new TabPage("Stats")
            {
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White
            };

            Button percentageButton = new()
            {
                Text = "Stats",
                Location = new System.Drawing.Point(50, 10)
            };
            percentageButton.Click += PercentageButton_Click;
            statsTabPage.Controls.Add(percentageButton);

            Label percentageLabel = new()
            {
                Name = "percentageLabel",
                Text = "",
                Location = new System.Drawing.Point(50, 50),
                AutoSize = true
            };
            percentageLabel.Font = new Font(percentageLabel.Font.FontFamily, 14); // Set default font size to 14
            statsTabPage.Controls.Add(percentageLabel);

            // Create and configure the Route TabPage
            routeTabPage = new TabPage("Route")
            {
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White
            };

            // Add the TabPages to the TabControl
            tabControl.TabPages.Add(statsTabPage);
            tabControl.TabPages.Add(routeTabPage);

            // Add the TabControl to the form's controls
            this.Controls.Add(tabControl);

            // Initialize gameDirectoryTextBox and autoStartCheckBox
            gameDirectoryTextBox = new TextBox
            {
                Location = new System.Drawing.Point((this.ClientSize.Width - 800) / 2, 100),
                Width = 600,
                ReadOnly = true,
                Visible = false
            };
            statsTabPage.Controls.Add(gameDirectoryTextBox);

            autoStartCheckBox = new CheckBox
            {
                Text = "Auto-Start Game",
                Location = new System.Drawing.Point((this.ClientSize.Width - 800) / 2, 130)
            };
            autoStartCheckBox.CheckedChanged += AutoStartCheckBox_CheckedChanged;
            autoStartCheckBox.Visible = false;
            statsTabPage.Controls.Add(autoStartCheckBox);
        }

        // ==========FORMAL COMMENT=========
        // Loads user settings from application configuration
        // Retrieves saved game directory and auto-start preference
        // ==========MY NOTES==============
        // Gets the saved settings when the app starts up
        // Uses a flag to prevent triggering events while loading
        [SupportedOSPlatform("windows6.1")]
        private void LoadSettings()
        {
            isLoadingSettings = true;
            gameDirectoryTextBox.Text = Settings.Default.GameDirectory;
            autoStartCheckBox.Checked = Settings.Default.AutoStart;
            isLoadingSettings = false;
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
            Settings.Default.GameDirectory = gameDirectoryTextBox.Text;
            Settings.Default.AutoStart = autoStartCheckBox.Checked;
            Settings.Default.Save();
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
            if (isLoadingSettings)
            {
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
            string gameDirectory = string.Empty;

            if (selectedGame == "Assassin's Creed 4")
            {
                gameDirectory = Settings.Default.AC4Directory;
            }
            else if (selectedGame == "Assassin's Creed Syndicate")
            {
                gameDirectory = Settings.Default.ACSDirectory;
            }

            if (string.IsNullOrEmpty(gameDirectory))
            {
                using FolderBrowserDialog folderBrowserDialog = new();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    gameDirectoryTextBox.Text = folderBrowserDialog.SelectedPath;
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
        // Event handler for Connect button clicks
        // Finds and connects to the selected game process
        // Initializes GameStats for memory reading if connection succeeds
        // ==========MY NOTES==============
        // This runs when you click "Connect to Game"
        // Tries to find the game process, starts it if needed, and connects to it
        // Creates the objects needed to read game stats if successful
        private async void ConnectButton_Click(object? sender, EventArgs e)
        {
            ToolStripComboBox? gameDropdown = this.MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault();
            ToolStripLabel? connectionLabel = this.MainMenuStrip?.Items.OfType<ToolStripLabel>().FirstOrDefault();

            if (gameDropdown == null || connectionLabel == null)
            {
                MessageBox.Show("Required controls are missing.");
                return;
            }

            string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;
            string gameDirectory = string.Empty;

            if (selectedGame == "Assassin's Creed 4")
            {
                currentProcess = "AC4BFSP.exe";
                gameDirectory = Settings.Default.AC4Directory;
            }
            else if (selectedGame == "Assassin's Creed Syndicate")
            {
                currentProcess = "ACS.exe";
                gameDirectory = Settings.Default.ACSDirectory;
            }
            else
            {
                connectionLabel.Text = "Please select a game.";
                return;
            }

            if (string.IsNullOrEmpty(gameDirectory))
            {
                connectionLabel.Text = "Game directory not set.";
                return;
            }
            else
            {
                gameDirectoryTextBox.Text = gameDirectory;
            }

            if (autoStartCheckBox.Checked)
            {
                if (selectedGame == "Assassin's Creed Syndicate")
                {
                    MessageBox.Show("Auto-start does not work with Assassin's Creed Syndicate currently.");
                    return;
                }

                if (!IsProcessRunning(currentProcess))
                {
                    StartGame(currentProcess);
                    await WaitForGameToStartAsync();
                }
            }

            Connect();

            if (processHandle != IntPtr.Zero)
            {
                connectionLabel.Text = $"Connected to {selectedGame}";
                gameStats = new GameStats(processHandle, baseAddress);
                routeManager = new RouteManager("path_to_route_file.txt"); // Update with the actual path
            }
            else
            {
                connectionLabel.Text = "Error: Cannot connect to process. Make sure the game is running.";
            }
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Stats button clicks
        // Retrieves and displays all game statistics from memory
        // Handles error conditions and different game types
        // ==========MY NOTES==============
        // Gets all the game stats when you click the button
        // Shows them in the label or displays an error message if something goes wrong
        [SupportedOSPlatform("windows6.1")]
        private void PercentageButton_Click(object? sender, EventArgs e)
        {
            if (statsTabPage.Controls["percentageLabel"] is Label percentageLabel)
            {
                if (processHandle != IntPtr.Zero && currentProcess == "AC4BFSP.exe")
                {
                    try
                    {
                        if (gameStats == null)
                        {
                            percentageLabel.Text = "Error: gameStats is not initialized.";
                            return;
                        }

                        (int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure, int Fragments, int Assassin, int Naval, int Letters, int Manuscripts, int Music, int Forts, int Taverns, int TotalChests) = gameStats.GetStats();

                        percentageLabel.Text = $"Completion Percentage: {Percent}%\n" +
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
                            $"Total Chests Collected: {TotalChests}";
                    }
                    catch (Win32Exception ex)
                    {
                        percentageLabel.Text = $"Error: {ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        percentageLabel.Text = $"Unexpected error: {ex.Message}";
                    }
                }
                else if (processHandle != IntPtr.Zero && currentProcess == "ACS.exe")
                    percentageLabel.Text = "Percentage feature not available for Assassin's Creed Syndicate";
                else
                    percentageLabel.Text = "Not connected to a game";
            }
            else
            {
                // Handle the case where the control is not found
                MessageBox.Show("The percentage label control was not found.");
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
            if (sender is ComboBox settingsGameDropdown && this.Controls["settingsPanel"].Controls["settingsDirectoryTextBox"] is TextBox settingsDirectoryTextBox)
            {
                string selectedGame = settingsGameDropdown.SelectedItem?.ToString() ?? string.Empty;
                if (selectedGame == "Assassin's Creed 4")
                {
                    settingsDirectoryTextBox.Text = Settings.Default.AC4Directory;
                }
                else if (selectedGame == "Assassin's Creed Syndicate")
                {
                    settingsDirectoryTextBox.Text = Settings.Default.ACSDirectory;
                }
            }
        }

        // ==========FORMAL COMMENT=========
        // Event handler for the browse button in settings
        // Opens folder browser dialog and saves selected directory
        // ==========MY NOTES==============
        // Lets you pick a game folder and saves that choice
        [SupportedOSPlatform("windows6.1")]
        private void BrowseButton_Click(object? sender, EventArgs e, TextBox settingsDirectoryTextBox, ComboBox settingsGameDropdown)
        {
            using FolderBrowserDialog folderBrowserDialog = new();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                settingsDirectoryTextBox.Text = folderBrowserDialog.SelectedPath;
                SaveDirectory(settingsGameDropdown.SelectedItem?.ToString() ?? string.Empty, settingsDirectoryTextBox.Text);
            }
        }

        // ==========FORMAL COMMENT=========
        // Saves game-specific directory path to application settings
        // Updates appropriate setting based on selected game
        // ==========MY NOTES==============
        // Saves the game folder path for either AC4 or Syndicate
        [SupportedOSPlatform("windows6.1")]
        private void SaveDirectory(string selectedGame, string directory)
        {
            if (selectedGame == "Assassin's Creed 4")
            {
                Settings.Default.AC4Directory = directory;
            }
            else if (selectedGame == "Assassin's Creed Syndicate")
            {
                Settings.Default.ACSDirectory = directory;
            }
            Settings.Default.Save();
        }

        // ==========FORMAL COMMENT=========
        // Establishes connection to the game process
        // Finds the specified process, gets handle and base address for memory access
        // ==========MY NOTES==============
        // Finds the game in running processes and gets access to its memory
        // Sets up everything needed to read values from the game
        [SupportedOSPlatform("windows6.1")]
        private void Connect()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(currentProcess.Replace(".exe", ""));
                if (processes.Length > 0)
                {
                    Process process = processes[0];
                    processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
                    baseAddress = process.MainModule.BaseAddress;

                    Debug.WriteLine($"Connected to process {currentProcess}");
                    Debug.WriteLine($"Base address: {baseAddress:X}");
                }
                else
                {
                    processHandle = IntPtr.Zero;
                    baseAddress = IntPtr.Zero;
                    Debug.WriteLine($"Process {currentProcess} not found.");
                }
            }
            catch (Exception ex)
            {
                processHandle = IntPtr.Zero;
                baseAddress = IntPtr.Zero;
                Debug.WriteLine($"Error in Connect: {ex.Message}");
            }
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
        // Checks if a specific process is currently running
        // Returns true if the process is found, false otherwise
        // ==========MY NOTES==============
        // Just checks if the game is already running or not
        [SupportedOSPlatform("windows6.1")]
        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName.Replace(".exe", "")).Length > 0;
        }

        // ==========FORMAL COMMENT=========
        // Launches the specified game executable from its directory
        // Handles cases where directory or executable file is not found
        // ==========MY NOTES==============
        // Tries to start the game and shows error messages if it can't
        [SupportedOSPlatform("windows6.1")]
        private void StartGame(string processName)
        {
            try
            {
                string gameDirectory = string.Empty;
                if (currentProcess == "AC4BFSP.exe")
                {
                    gameDirectory = Settings.Default.AC4Directory;
                }
                else if (currentProcess == "ACS.exe")
                {
                    gameDirectory = Settings.Default.ACSDirectory;
                }

                if (string.IsNullOrEmpty(gameDirectory))
                {
                    MessageBox.Show("Please select the game's directory.");
                    return;
                }

                string gamePath = System.IO.Path.Combine(gameDirectory, processName);
                if (!System.IO.File.Exists(gamePath))
                {
                    MessageBox.Show($"The game executable was not found in the selected directory: {gamePath}");
                    return;
                }

                Process.Start(gamePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting the game: {ex.Message}");
            }
        }

        // ==========FORMAL COMMENT=========
        // Waits for the game process to start with a timeout
        // Shows custom dialog with options if game fails to start within time limit
        // ==========MY NOTES==============
        // Waits up to 10 seconds for the game to start
        // If it takes too long, gives you options like retry, wait longer, etc.
        [SupportedOSPlatform("windows6.1")]
        private async Task WaitForGameToStartAsync()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            while (stopwatch.Elapsed.TotalSeconds < 10)
            {
                if (IsProcessRunning(currentProcess))
                {
                    return;
                }

                await Task.Delay(1000);
            }

            stopwatch.Stop();

            using CustomMessageBox customMessageBox = new("The game did not start within 10 seconds. Would you like to try again, wait another 10 seconds, manually start the game, or cancel?");
            if (customMessageBox.ShowDialog() == DialogResult.OK)
            {
                switch (customMessageBox.Result)
                {
                    case CustomMessageBox.CustomDialogResult.TryAgain:
                        StartGame(currentProcess);
                        await WaitForGameToStartAsync();
                        break;
                    case CustomMessageBox.CustomDialogResult.Wait:
                        await WaitForGameToStartAsync();
                        break;
                    case CustomMessageBox.CustomDialogResult.Manually:
                        // Do nothing, user chose to manually start the game
                        break;
                    case CustomMessageBox.CustomDialogResult.Cancel:
                        // Do nothing, user chose to cancel
                        break;
                }
            }
        }

        // ==========FORMAL COMMENT=========
        // Windows API import for opening a process with specific access rights
        // Required to gain read access to the game process memory
        // ==========MY NOTES==============
        // Windows function that lets us connect to the game process
        // Without this, we couldn't read any of the game's memory values
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    }
}


