using System;
using System.Windows.Forms;
using Assassin_s_Creed_Route_Tracker.Properties;

namespace Assassin_s_Creed_Route_Tracker
{
    public partial class GameDirectoryForm : Form
    {
        public GameDirectoryForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private ComboBox gameDropdown;
        private TextBox directoryTextBox;
        private Button browseButton;

        private void InitializeCustomComponents()
        {
            this.Text = "Game Directory Settings";
            this.Size = new System.Drawing.Size(400, 200);

            Label gameLabel = new Label();
            gameLabel.Text = "Select Game:";
            gameLabel.Location = new System.Drawing.Point(20, 20);
            this.Controls.Add(gameLabel);

            gameDropdown = new ComboBox();
            gameDropdown.Items.AddRange(new object[] { "Assassin's Creed 4", "Assassin's Creed Syndicate" });
            gameDropdown.Location = new System.Drawing.Point(120, 20);
            gameDropdown.SelectedIndexChanged += GameDropdown_SelectedIndexChanged;
            this.Controls.Add(gameDropdown);

            Label directoryLabel = new Label();
            directoryLabel.Text = "Game Directory:";
            directoryLabel.Location = new System.Drawing.Point(20, 60);
            this.Controls.Add(directoryLabel);

            directoryTextBox = new TextBox();
            directoryTextBox.Location = new System.Drawing.Point(120, 60);
            directoryTextBox.Width = 200;
            directoryTextBox.ReadOnly = true;
            this.Controls.Add(directoryTextBox);

            browseButton = new Button();
            browseButton.Text = "Browse";
            browseButton.Location = new System.Drawing.Point(330, 60);
            browseButton.Click += BrowseButton_Click;
            this.Controls.Add(browseButton);
        }

        private void GameDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedGame = gameDropdown.SelectedItem.ToString();
            if (selectedGame == "Assassin's Creed 4")
            {
                directoryTextBox.Text = Settings.Default.AC4Directory;
            }
            else if (selectedGame == "Assassin's Creed Syndicate")
            {
                directoryTextBox.Text = Settings.Default.ACSDirectory;
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    directoryTextBox.Text = folderBrowserDialog.SelectedPath;
                    SaveDirectory();
                }
            }
        }

        private void SaveDirectory()
        {
            string selectedGame = gameDropdown.SelectedItem.ToString();
            if (selectedGame == "Assassin's Creed 4")
            {
                Settings.Default.AC4Directory = directoryTextBox.Text;
            }
            else if (selectedGame == "Assassin's Creed Syndicate")
            {
                Settings.Default.ACSDirectory = directoryTextBox.Text;
            }
            Settings.Default.Save();
        }
    }
}
