using System;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Form for configuring game directory locations
    // Allows users to select and set paths for different Assassin's Creed games
    // ==========MY NOTES==============
    // This window lets the user pick where each game is installed
    // Used for the auto-start feature to know where to find the games
    public partial class GameDirectoryForm : Form
    {
        public event EventHandler? DirectoryChanged;

        // ==========FORMAL COMMENT=========
        // Initializes a new game directory configuration form
        // Sets up the UI and prepares the form for user interaction
        // ==========MY NOTES==============
        // Creates the game directory settings window
        public GameDirectoryForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        // Using null-forgiving operator
        private ComboBox gameDropdown = null!;
        private TextBox directoryTextBox = null!;
        private Button browseButton = null!;

        // ==========FORMAL COMMENT=========
        // UI control fields for the game directory form
        // Stores references to key interactive elements
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
            gameDropdown.Items.AddRange(["Assassin's Creed 4", "God of War 2018"]);
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

        // ==========FORMAL COMMENT=========
        // Creates and configures all UI components for the form
        // Sets up labels, dropdowns, textboxes, and buttons with event handlers
        // ==========MY NOTES==============
        // Builds all the UI elements for selecting and setting game directories
        private void GameDropdown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;
            if (selectedGame == "Assassin's Creed 4")
            {
                directoryTextBox.Text = Settings.Default.AC4Directory;
            }
            else if (selectedGame == "God of War 2018")
            {
                directoryTextBox.Text = Settings.Default.Gow2018Directory;
            }
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Browse button clicks
        // Opens a folder browser dialog and saves the selected path
        // ==========MY NOTES==============
        // Lets the user choose a folder for the currently selected game
        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                directoryTextBox.Text = folderBrowserDialog.SelectedPath;
                SaveDirectory();
            }
        }

        // ==========FORMAL COMMENT=========
        // Saves the current directory path to application settings
        // Updates the appropriate setting based on which game is selected
        // ==========MY NOTES==============
        // Saves the game folder location to the app settings
        private void SaveDirectory()
        {
            string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;
            if (selectedGame == "Assassin's Creed 4")
            {
                Settings.Default.AC4Directory = directoryTextBox.Text;
            }
            else if (selectedGame == "God of War 2018")
            {
                Settings.Default.Gow2018Directory = directoryTextBox.Text;
            }
            Settings.Default.Save();

            DirectoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
