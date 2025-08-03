using System;
using System.Windows.Forms;

namespace Route_Tracker
{
    public partial class UpdateProgressForm : Form
    {
        public ProgressBar DownloadProgressBar { get; }
        public ProgressBar ExtractProgressBar { get; }
        public Label StatusLabel { get; }
        public TextBox DownloadPathBox { get; }
        public TextBox ExtractPathBox { get; }
        public Button BrowseDownloadButton { get; }
        public Button BrowseExtractButton { get; }
        public CheckBox LaunchNewVersionCheckBox { get; }
        public Button ContinueButton { get; }

        public UpdateProgressForm()
        {
            this.SetupAsSettingsForm("Updating Route Tracker");
            this.Size = new System.Drawing.Size(500, 280);

            // Status label
            StatusLabel = UIControlFactory.CreateThemedLabel("Ready to download update...", false);
            StatusLabel.Width = 450;
            StatusLabel.Height = 24;
            StatusLabel.Location = new System.Drawing.Point(20, 20);

            // Download progress bar
            DownloadProgressBar = new ProgressBar
            {
                Width = 450,
                Height = 20,
                Location = new System.Drawing.Point(20, 50),
                Minimum = 0,
                Maximum = 100
            };

            // Download path section
            var downloadLabel = UIControlFactory.CreateThemedLabel("Download to:");
            downloadLabel.Location = new System.Drawing.Point(20, 80);

            DownloadPathBox = UIControlFactory.CreateThemedTextBox(350);
            DownloadPathBox.Location = new System.Drawing.Point(20, 100);

            BrowseDownloadButton = UIControlFactory.CreateThemedButton("Browse...", 80, 23);
            BrowseDownloadButton.Location = new System.Drawing.Point(380, 98);

            // Extract progress bar
            ExtractProgressBar = new ProgressBar
            {
                Width = 450,
                Height = 20,
                Location = new System.Drawing.Point(20, 130),
                Minimum = 0,
                Maximum = 100
            };

            // Extract path section
            var extractLabel = UIControlFactory.CreateThemedLabel("Extract to:");
            extractLabel.Location = new System.Drawing.Point(20, 160);

            ExtractPathBox = UIControlFactory.CreateThemedTextBox(350);
            ExtractPathBox.Location = new System.Drawing.Point(20, 180);

            BrowseExtractButton = UIControlFactory.CreateThemedButton("Browse...", 80, 23);
            BrowseExtractButton.Location = new System.Drawing.Point(380, 178);

            // Launch checkbox
            LaunchNewVersionCheckBox = new CheckBox
            {
                Text = "Launch new version after update",
                Checked = true,
                Location = new System.Drawing.Point(20, 210),
                Width = 300,
                Enabled = false,
                ForeColor = AppTheme.TextColor
            };

            // Continue button
            ContinueButton = UIControlFactory.CreateThemedButton("Continue", 100, 28);
            ContinueButton.Location = new System.Drawing.Point(370, 210);
            ContinueButton.DialogResult = DialogResult.OK;
            ContinueButton.Enabled = false;

            // Add controls
            this.Controls.Add(StatusLabel);
            this.Controls.Add(DownloadProgressBar);
            this.Controls.Add(downloadLabel);
            this.Controls.Add(DownloadPathBox);
            this.Controls.Add(BrowseDownloadButton);
            this.Controls.Add(ExtractProgressBar);
            this.Controls.Add(extractLabel);
            this.Controls.Add(ExtractPathBox);
            this.Controls.Add(BrowseExtractButton);
            this.Controls.Add(LaunchNewVersionCheckBox);
            this.Controls.Add(ContinueButton);

            // Wire up events
            BrowseDownloadButton.Click += (s, e) =>
            {
                using var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    DownloadPathBox.Text = fbd.SelectedPath;
                }
            };

            BrowseExtractButton.Click += (s, e) =>
            {
                using var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    ExtractPathBox.Text = fbd.SelectedPath;
                }
            };

            this.AcceptButton = ContinueButton;

            // Remove all auto-close logic: The form will only close when the user clicks Continue.
            ContinueButton.Click += (s, e) => this.Close();
            this.FormClosing += (s, e) =>
            {
                // Prevent closing by any means other than ContinueButton
                if (!ContinueButton.Enabled)
                {
                    e.Cancel = true;
                }
            };
        }
    }
}