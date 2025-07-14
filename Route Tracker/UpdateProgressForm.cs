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
            this.Text = "Updating Route Tracker";
            this.Size = new System.Drawing.Size(500, 280);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = AppTheme.TextColor;

            // Status label
            StatusLabel = new Label
            {
                Text = "Ready to download update...",
                AutoSize = false,
                Width = 450,
                Height = 24,
                Location = new System.Drawing.Point(20, 20),
                ForeColor = AppTheme.TextColor
            };

            // Download progress bar
            DownloadProgressBar = new ProgressBar
            {
                Width = 450,
                Height = 20,
                Location = new System.Drawing.Point(20, 50),
                Minimum = 0,
                Maximum = 100
            };

            // Download path
            var downloadLabel = new Label
            {
                Text = "Download to:",
                Location = new System.Drawing.Point(20, 80),
                AutoSize = true,
                ForeColor = AppTheme.TextColor
            };
            DownloadPathBox = new TextBox
            {
                Width = 350,
                Location = new System.Drawing.Point(20, 100),
                Text = "",
                BackColor = AppTheme.InputBackgroundColor,
                ForeColor = AppTheme.TextColor
            };
            BrowseDownloadButton = new Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(380, 98),
                Width = 80,
                Height = 23
            };

            // Extract progress bar
            ExtractProgressBar = new ProgressBar
            {
                Width = 450,
                Height = 20,
                Location = new System.Drawing.Point(20, 130),
                Minimum = 0,
                Maximum = 100
            };

            // Extract path
            var extractLabel = new Label
            {
                Text = "Extract to:",
                Location = new System.Drawing.Point(20, 160),
                AutoSize = true,
                ForeColor = AppTheme.TextColor
            };
            ExtractPathBox = new TextBox
            {
                Width = 350,
                Location = new System.Drawing.Point(20, 180),
                Text = "",
                BackColor = AppTheme.InputBackgroundColor,
                ForeColor = AppTheme.TextColor
            };
            BrowseExtractButton = new Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(380, 178),
                Width = 80,
                Height = 23
            };

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
            ContinueButton = new Button
            {
                Text = "Continue",
                Width = 100,
                Height = 28,
                Location = new System.Drawing.Point(370, 210),
                DialogResult = DialogResult.OK,
                Enabled = false
            };

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

            // Apply theme
            AppTheme.ApplyToButton(BrowseDownloadButton);
            AppTheme.ApplyToButton(BrowseExtractButton);
            AppTheme.ApplyToButton(ContinueButton);
            AppTheme.ApplyToTextBox(DownloadPathBox);
            AppTheme.ApplyToTextBox(ExtractPathBox);

            // Wire up events
            BrowseDownloadButton.Click += (s, e) =>
            {
                using var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                    DownloadPathBox.Text = fbd.SelectedPath;
            };

            BrowseExtractButton.Click += (s, e) =>
            {
                using var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                    ExtractPathBox.Text = fbd.SelectedPath;
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