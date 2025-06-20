using System;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Settings form that allows users to configure application preferences
    // Manages auto-start behavior and game directory settings
    // ==========MY NOTES==============
    // This is the settings window that lets users change app options
    [SupportedOSPlatform("windows6.1")]
    public partial class SettingsForm : Form
    {
        // ==========FORMAL COMMENT=========
        // Initializes the settings form and loads current preferences
        // Sets up the UI and populates fields with saved settings
        // ==========MY NOTES==============
        // Creates the settings window and fills it with current settings
        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        // ==========FORMAL COMMENT=========
        // Loads user settings from application configuration
        // Populates UI controls with saved preferences
        // ==========MY NOTES==============
        // Gets saved settings and shows them in the form
        private void LoadSettings()
        {
            autoStartCheckBox.Checked = Settings.Default.AutoStart;
            gameDirectoryTextBox.Text = Settings.Default.GameDirectory;
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Save button clicks
        // Persists user selections to application settings and closes the form
        // ==========MY NOTES==============
        // Saves settings when the user clicks Save and closes the window
        private void SaveButton_Click(object sender, EventArgs e)
        {
            Settings.Default.AutoStart = autoStartCheckBox.Checked;
            Settings.Default.GameDirectory = gameDirectoryTextBox.Text;
            Settings.Default.Save();
            this.Close();
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Browse button clicks
        // Opens folder selection dialog and updates directory path field
        // ==========MY NOTES==============
        // Lets the user pick a folder for the game directory
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog folderDialog = new();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                gameDirectoryTextBox.Text = folderDialog.SelectedPath;
            }
        }
    }
}
