using System;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // This window lets the user pick where each game is installed
    // Used for the auto-start feature to know where to find the games
    public partial class GameDirectoryForm : Form
    {
        public event EventHandler? DirectoryChanged;
        private readonly SettingsManager settingsManager;

        public GameDirectoryForm(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
            InitializeComponent();
            InitializeCustomComponents();
        }

        // Using null-forgiving operator
        private ComboBox gameDropdown = null!;
        private TextBox directoryTextBox = null!;
        private Button browseButton = null!;

        public SettingsManager? SettingsManager => settingsManager;

        // ==========MY NOTES==============
        // These are the main controls we need to access in our code
        private void InitializeCustomComponents()
        {
            this.Text = "Game Directory Settings";
            this.Size = new System.Drawing.Size(400, 200);
            AppTheme.ApplyToSettingsForm(this);

            Label gameLabel = new()
            {
                Text = "Select Game:",
                Location = new System.Drawing.Point(20, 20)
            };
            this.Controls.Add(gameLabel);

            gameDropdown = new ComboBox();
            gameDropdown.Items.Add("");
            gameDropdown.Items.AddRange([.. SupportedGames.GameList.Values.Select(g => g.DisplayName)]);
            gameDropdown.Location = new System.Drawing.Point(120, 20);
            gameDropdown.SelectedIndexChanged += GameDropdown_SelectedIndexChanged;
            this.Controls.Add(gameDropdown);

            Label directoryLabel = new()
            {
                Text = "Game Directory:",
                Location = new System.Drawing.Point(20, 60)
            };
            this.Controls.Add(directoryLabel);

            directoryTextBox = new TextBox
            {
                Location = new System.Drawing.Point(120, 60),
                Width = 200,
                ReadOnly = true
            };
            this.Controls.Add(directoryTextBox);

            browseButton = new Button
            {
                Text = "Browse",
                Location = new System.Drawing.Point(330, 60)
            };
            browseButton.Click += BrowseButton_Click;
            this.Controls.Add(browseButton);

            AppTheme.ApplyToButton(browseButton);
            AppTheme.ApplyToComboBox(gameDropdown);
            AppTheme.ApplyToTextBox(directoryTextBox);

        }

        // ==========MY NOTES==============
        // Builds all the UI elements for selecting and setting game directories
        private void GameDropdown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;

            // Use SettingsManager to get the directory (it handles the mapping internally)
            directoryTextBox.Text = SettingsManager?.GetGameDirectory(selectedGame);
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Browse button clicks
        // Opens a folder browser dialog and saves the selected path
        // ==========MY NOTES==============
        // Lets the user choose a folder for the currently selected game
        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            try
            {
                using FolderBrowserDialog folderBrowserDialog = new();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    directoryTextBox.Text = folderBrowserDialog.SelectedPath;
                    SaveDirectory();

                    string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;
                    LoggingSystem.LogInfo($"Game directory set for {selectedGame}: {folderBrowserDialog.SelectedPath}");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError("Error selecting game directory", ex);
                MessageBox.Show($"Error selecting directory: {ex.Message}", "Directory Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========MY NOTES==============
        // Saves the game folder location to the app settings
        private void SaveDirectory()
        {
            try
            {
                string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;

                // Use SettingsManager to save the directory (it handles the mapping internally)
                SettingsManager?.SaveDirectory(selectedGame, directoryTextBox.Text);

                DirectoryChanged?.Invoke(this, EventArgs.Empty);

                LoggingSystem.LogInfo($"Game directory set for {selectedGame}: {directoryTextBox.Text}");
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError("Error saving game directory", ex);
            }
        }
    }
}
