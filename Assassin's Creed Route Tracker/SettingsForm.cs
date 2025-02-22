using System;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Assassin_s_Creed_Route_Tracker.Properties;

namespace Assassin_s_Creed_Route_Tracker
{
    [SupportedOSPlatform("windows6.1")]
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            autoStartCheckBox.Checked = Settings.Default.AutoStart;
            gameDirectoryTextBox.Text = Settings.Default.GameDirectory;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Settings.Default.AutoStart = autoStartCheckBox.Checked;
            Settings.Default.GameDirectory = gameDirectoryTextBox.Text;
            Settings.Default.Save();
            this.Close();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    gameDirectoryTextBox.Text = folderDialog.SelectedPath;
                }
            }
        }
    }
}
