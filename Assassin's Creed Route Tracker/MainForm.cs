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
    public partial class MainForm : Form
    {
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

        [SupportedOSPlatform("windows6.1")]
        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadSettings();
        }

        private TextBox gameDirectoryTextBox;
        private CheckBox autoStartCheckBox;

        [SupportedOSPlatform("windows6.1")]
        private void InitializeCustomComponents()
        {
            this.Text = "Assassin's Creed Route Tracker";
            this.BackColor = System.Drawing.Color.Black;
            this.ForeColor = System.Drawing.Color.White;

            // Create and configure the MenuStrip
            MenuStrip menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Top; // Dock the MenuStrip at the top of the form
            menuStrip.BackColor = System.Drawing.Color.Black;
            menuStrip.ForeColor = System.Drawing.Color.White;

            // Create and configure the Settings menu item
            ToolStripMenuItem settingsMenuItem = new ToolStripMenuItem("Settings");

            // Create and configure the Auto-Start Game menu item
            ToolStripMenuItem autoStartMenuItem = new ToolStripMenuItem("Auto-Start Game");
            autoStartMenuItem.CheckOnClick = true;
            autoStartMenuItem.CheckedChanged += AutoStartMenuItem_CheckedChanged;

            // Create and configure the Game Directory menu item
            ToolStripMenuItem gameDirectoryMenuItem = new ToolStripMenuItem("Game Directory");
            gameDirectoryMenuItem.Click += GameDirectoryMenuItem_Click;

            // Add the Auto-Start Game and Game Directory menu items to the Settings menu item
            settingsMenuItem.DropDownItems.Add(autoStartMenuItem);
            settingsMenuItem.DropDownItems.Add(gameDirectoryMenuItem);

            // Add the Settings menu item to the MenuStrip
            menuStrip.Items.Add(settingsMenuItem);

            // Create and configure the Stats tab button
            ToolStripButton statsTabButton = new ToolStripButton("Stats");
            statsTabButton.Click += (sender, e) => tabControl.SelectedTab = statsTabPage;
            menuStrip.Items.Add(statsTabButton);

            // Create and configure the Route tab button
            ToolStripButton routeTabButton = new ToolStripButton("Route");
            routeTabButton.Click += (sender, e) => tabControl.SelectedTab = routeTabPage;
            menuStrip.Items.Add(routeTabButton);

            // Create and configure the connection label
            ToolStripLabel connectionLabel = new ToolStripLabel();
            connectionLabel.Text = "Not connected";
            menuStrip.Items.Add(connectionLabel);

            // Create and configure the game dropdown
            ToolStripComboBox gameDropdown = new ToolStripComboBox();
            gameDropdown.Items.AddRange(new object[] { "", "Assassin's Creed 4", "Assassin's Creed Syndicate" });
            gameDropdown.SelectedIndex = 0;
            menuStrip.Items.Add(gameDropdown);

            // Create and configure the connect button
            ToolStripButton connectButton = new ToolStripButton("Connect to Game");
            connectButton.Click += ConnectButton_Click;
            menuStrip.Items.Add(connectButton);

            // Set the MenuStrip as the main menu strip of the form
            this.MainMenuStrip = menuStrip;

            // Add the MenuStrip to the form's controls
            this.Controls.Add(menuStrip);

            // Create and configure the TabControl
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // Create and configure the Stats TabPage
            statsTabPage = new TabPage("Stats");
            statsTabPage.BackColor = System.Drawing.Color.Black;
            statsTabPage.ForeColor = System.Drawing.Color.White;

            Button percentageButton = new Button();
            percentageButton.Text = "Stats";
            percentageButton.Location = new System.Drawing.Point(50, 10);
            percentageButton.Click += PercentageButton_Click;
            statsTabPage.Controls.Add(percentageButton);

            Label percentageLabel = new Label();
            percentageLabel.Name = "percentageLabel";
            percentageLabel.Text = "";
            percentageLabel.Location = new System.Drawing.Point(50, 50);
            percentageLabel.AutoSize = true;
            percentageLabel.Font = new Font(percentageLabel.Font.FontFamily, 14); // Set default font size to 14
            statsTabPage.Controls.Add(percentageLabel);

            // Create and configure the Route TabPage
            routeTabPage = new TabPage("Route");
            routeTabPage.BackColor = System.Drawing.Color.Black;
            routeTabPage.ForeColor = System.Drawing.Color.White;

            // Add the TabPages to the TabControl
            tabControl.TabPages.Add(statsTabPage);
            tabControl.TabPages.Add(routeTabPage);

            // Add the TabControl to the form's controls
            this.Controls.Add(tabControl);

            // Initialize gameDirectoryTextBox and autoStartCheckBox
            gameDirectoryTextBox = new TextBox();
            gameDirectoryTextBox.Location = new System.Drawing.Point((this.ClientSize.Width - 800) / 2, 100);
            gameDirectoryTextBox.Width = 600;
            gameDirectoryTextBox.ReadOnly = true;
            gameDirectoryTextBox.Visible = false;
            statsTabPage.Controls.Add(gameDirectoryTextBox);

            autoStartCheckBox = new CheckBox();
            autoStartCheckBox.Text = "Auto-Start Game";
            autoStartCheckBox.Location = new System.Drawing.Point((this.ClientSize.Width - 800) / 2, 130);
            autoStartCheckBox.CheckedChanged += AutoStartCheckBox_CheckedChanged;
            autoStartCheckBox.Visible = false;
            statsTabPage.Controls.Add(autoStartCheckBox);
        }

        [SupportedOSPlatform("windows6.1")]
        private void LoadSettings()
        {
            isLoadingSettings = true;
            gameDirectoryTextBox.Text = Settings.Default.GameDirectory;
            autoStartCheckBox.Checked = Settings.Default.AutoStart;
            isLoadingSettings = false;
        }

        [SupportedOSPlatform("windows6.1")]
        private void SaveSettings()
        {
            Settings.Default.GameDirectory = gameDirectoryTextBox.Text;
            Settings.Default.AutoStart = autoStartCheckBox.Checked;
            Settings.Default.Save();
        }

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
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
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
            }
            else
            {
                gameDirectoryTextBox.Text = gameDirectory;
                SaveSettings();
            }
        }

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

        [SupportedOSPlatform("windows6.1")]
        private void PercentageButton_Click(object? sender, EventArgs e)
        {
            Label? percentageLabel = statsTabPage.Controls["percentageLabel"] as Label;

            if (percentageLabel != null)
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

                        var stats = gameStats.GetStats();

                        percentageLabel.Text = $"Completion Percentage: {stats.Percent}%\n" +
                            $"Completion Percentage Exact: {Math.Round(stats.PercentFloat, 2)}%\n" +
                            $"Viewpoints Completed: {stats.Viewpoints}\n" +
                            $"Myan Stones Collected: {stats.Myan}\n" +
                            $"Buried Treasure Collected: {stats.Treasure}\n" +
                            $"AnimusFragments Collected: {stats.Fragments}\n" +
                            $"AssassinContracts Completed: {stats.Assassin}\n" +
                            $"NavalContracts Completed: {stats.Naval}\n" +
                            $"LetterBottles Collected: {stats.Letters}\n" +
                            $"Manuscripts Collected: {stats.Manuscripts}\n" +
                            $"Music Sheets Collected: {stats.Music}\n" +
                            $"Forts Captured: {stats.Forts}\n" +
                            $"Taverns unlocked: {stats.Taverns}\n" +
                            $"Total Chests Collected: {stats.TotalChests}";
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


        [SupportedOSPlatform("windows6.1")]
        private void SettingsMenuItem_Click(object? sender, EventArgs e)
        {
            Panel? settingsPanel = this.Controls["settingsPanel"] as Panel;
            if (settingsPanel != null)
            {
                settingsPanel.Visible = !settingsPanel.Visible;
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void SettingsGameDropdown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ComboBox? settingsGameDropdown = sender as ComboBox;
            TextBox? settingsDirectoryTextBox = this.Controls["settingsPanel"].Controls["settingsDirectoryTextBox"] as TextBox;

            if (settingsGameDropdown != null && settingsDirectoryTextBox != null)
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

        [SupportedOSPlatform("windows6.1")]
        private void BrowseButton_Click(object? sender, EventArgs e, TextBox settingsDirectoryTextBox, ComboBox settingsGameDropdown)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    settingsDirectoryTextBox.Text = folderBrowserDialog.SelectedPath;
                    SaveDirectory(settingsGameDropdown.SelectedItem?.ToString() ?? string.Empty, settingsDirectoryTextBox.Text);
                }
            }
        }

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
                    Debug.WriteLine($"Base address: {baseAddress.ToString("X")}");
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

        [SupportedOSPlatform("windows6.1")]
        private void GameDirectoryMenuItem_Click(object? sender, EventArgs e)
        {
            GameDirectoryForm gameDirectoryForm = new GameDirectoryForm();
            gameDirectoryForm.ShowDialog();
        }

        [SupportedOSPlatform("windows6.1")]
        private void AutoStartMenuItem_CheckedChanged(object? sender, EventArgs e)
        {
            ToolStripMenuItem? autoStartMenuItem = sender as ToolStripMenuItem;
            autoStartCheckBox.Checked = autoStartMenuItem?.Checked ?? false;
            SaveSettings();
        }

        [SupportedOSPlatform("windows6.1")]
        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName.Replace(".exe", "")).Length > 0;
        }

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

        [SupportedOSPlatform("windows6.1")]
        private async Task WaitForGameToStartAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
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

            using (var customMessageBox = new CustomMessageBox("The game did not start within 10 seconds. Would you like to try again, wait another 10 seconds, manually start the game, or cancel?"))
            {
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
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    }
}


