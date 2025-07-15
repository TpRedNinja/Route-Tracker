using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Form for importing route files from URLs
    // Provides UI for URL input, filename selection, and download progress
    // Supports loading route after download and maintains download history
    // ==========MY NOTES==============
    // The main dialog for downloading routes from URLs
    // Handles all the UI interaction and coordinates with the download manager
    public partial class ImportRouteForm : Form
    {
        private readonly MainForm parentForm;
        private readonly RouteDownloadManager downloadManager;
        private readonly RouteHistoryManager historyManager;
        private bool downloadCompleted = false;
        private string? downloadedFilePath = null;
        private Panel bottomPanel = null!;

        public ImportRouteForm(MainForm parent)
        {
            parentForm = parent;
            downloadManager = new RouteDownloadManager();
            historyManager = new RouteHistoryManager();
            InitializeComponent();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            this.Text = "Import Route from URL";
            this.Size = new System.Drawing.Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = AppTheme.TextColor;

            // Main layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                Padding = new Padding(20),
                BackColor = AppTheme.BackgroundColor
            };

            // Configure row styles
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Instructions
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // URL label
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // URL textbox
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Filename label
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Filename textbox
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Checkbox and button
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Progress bar
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Spacer and buttons

            // Instructions label
            var instructionsLabel = new Label
            {
                Text = "Import route files from direct/raw URLs:\n\n" +
                       "✓ GitHub raw links (https://raw.githubusercontent.com/...)\n" +
                       "✓ Gist raw links (https://gist.githubusercontent.com/...)\n" +
                       "✓ Pastebin raw links (https://pastebin.com/raw/...)\n" +
                       "✓ Any direct link to a TSV file\n\n" +
                       "Note: The URL must point directly to the file content, not a web page.",
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(0, 0, 0, 10)
            };

            // URL label
            var urlLabel = new Label
            {
                Text = "URL:",
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(0, 0, 0, 5)
            };

            // URL textbox
            var urlTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "https://raw.githubusercontent.com/user/repo/main/route.tsv",
                BackColor = AppTheme.InputBackgroundColor,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Height = 23,
                Margin = new Padding(0, 0, 0, 10)
            };

            // Filename label
            var filenameLabel = new Label
            {
                Text = "Save as:",
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Margin = new Padding(0, 0, 0, 5)
            };

            // Filename textbox
            var filenameTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "MyRoute.tsv",
                BackColor = AppTheme.InputBackgroundColor,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Height = 23,
                Margin = new Padding(0, 0, 0, 10)
            };

            // Checkbox and download button panel
            var checkboxButtonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 10)
            };

            var loadAfterDownloadCheckBox = new CheckBox
            {
                Text = "Load route after downloading",
                Checked = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                AutoSize = true,
                Margin = new Padding(0, 0, 20, 0)
            };

            var downloadButton = new Button
            {
                Text = "Download",
                Width = 100,
                Height = 30,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Enabled = false
            };

            checkboxButtonPanel.Controls.Add(loadAfterDownloadCheckBox);
            checkboxButtonPanel.Controls.Add(downloadButton);

            // Progress bar
            var progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Height = 20,
                Minimum = 0,
                Maximum = 100,
                Visible = false,
                Margin = new Padding(0, 0, 0, 10)
            };

            // Bottom panel with status and continue button
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.BackgroundColor
            };

            this.bottomPanel = bottomPanel;

            var statusLabel = new Label
            {
                Text = "Ready to download",
                AutoSize = true,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Location = new System.Drawing.Point(0, 0)
            };

            var continueButton = new Button
            {
                Text = "Continue",
                Width = 100,
                Height = 30,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK,
                Enabled = false
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                Width = 100,
                Height = 30,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel,
                Margin = new Padding(0, 0, 10, 0)
            };

            // Position buttons
            continueButton.Location = new System.Drawing.Point(
                bottomPanel.Width - continueButton.Width - 10, 
                bottomPanel.Height - continueButton.Height - 10);
            cancelButton.Location = new System.Drawing.Point(
                continueButton.Left - cancelButton.Width - 10, 
                bottomPanel.Height - cancelButton.Height - 10);

            bottomPanel.Controls.Add(statusLabel);
            bottomPanel.Controls.Add(continueButton);
            bottomPanel.Controls.Add(cancelButton);

            // Add all controls to main layout
            mainLayout.Controls.Add(instructionsLabel, 0, 0);
            mainLayout.Controls.Add(urlLabel, 0, 1);
            mainLayout.Controls.Add(urlTextBox, 0, 2);
            mainLayout.Controls.Add(filenameLabel, 0, 3);
            mainLayout.Controls.Add(filenameTextBox, 0, 4);
            mainLayout.Controls.Add(checkboxButtonPanel, 0, 5);
            mainLayout.Controls.Add(progressBar, 0, 6);
            mainLayout.Controls.Add(bottomPanel, 0, 7);

            this.Controls.Add(mainLayout);

            // Apply theme
            AppTheme.ApplyToTextBox(urlTextBox);
            AppTheme.ApplyToTextBox(filenameTextBox);
            AppTheme.ApplyToButton(downloadButton);
            AppTheme.ApplyToButton(continueButton);
            AppTheme.ApplyToButton(cancelButton);

            // Store references for event handlers
            this.urlTextBox = urlTextBox;
            this.filenameTextBox = filenameTextBox;
            this.loadAfterDownloadCheckBox = loadAfterDownloadCheckBox;
            this.downloadButton = downloadButton;
            this.progressBar = progressBar;
            this.statusLabel = statusLabel;
            this.continueButton = continueButton;
            this.cancelButton = cancelButton;

            // Set up tooltips
            var toolTip = new ToolTip();
            toolTip.SetToolTip(urlTextBox, "Enter the direct URL to a TSV route file");
            toolTip.SetToolTip(filenameTextBox, "Enter the filename to save the route as (must end with .tsv)");
            toolTip.SetToolTip(loadAfterDownloadCheckBox, "Automatically load the route after downloading");
            toolTip.SetToolTip(downloadButton, "Start downloading the route file");
            toolTip.SetToolTip(continueButton, "Close this dialog and load the route if selected");
        }

        private TextBox urlTextBox = null!;
        private TextBox filenameTextBox = null!;
        private CheckBox loadAfterDownloadCheckBox = null!;
        private Button downloadButton = null!;
        private ProgressBar progressBar = null!;
        private Label statusLabel = null!;
        private Button continueButton = null!;
        private Button cancelButton = null!;
        

        private void SetupEventHandlers()
        {
            // Enable download button when both fields have content
            urlTextBox.TextChanged += (s, e) => UpdateDownloadButtonState();
            filenameTextBox.TextChanged += (s, e) => UpdateDownloadButtonState();

            // Auto-add .tsv extension if not present
            filenameTextBox.Leave += (s, e) =>
            {
                if (!string.IsNullOrEmpty(filenameTextBox.Text) && !filenameTextBox.Text.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase))
                {
                    filenameTextBox.Text += ".tsv";
                }
            };

            // Download button click
            downloadButton.Click += async (s, e) => await StartDownload();

            // Continue button click
            continueButton.Click += (s, e) => HandleContinue();

            // Form closing validation
            this.FormClosing += (s, e) =>
            {
                if (DialogResult == DialogResult.OK && !downloadCompleted)
                {
                    e.Cancel = true;
                    MessageBox.Show("Please complete the download before continuing.", "Download Not Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // Resize event to position buttons correctly
            this.Resize += (s, e) =>
            {
                if (bottomPanel != null)
                {
                    continueButton.Location = new System.Drawing.Point(
                        bottomPanel.Width - continueButton.Width - 10,
                        bottomPanel.Height - continueButton.Height - 10);
                    cancelButton.Location = new System.Drawing.Point(
                        continueButton.Left - cancelButton.Width - 10,
                        bottomPanel.Height - cancelButton.Height - 10);
                }
            };
        }

        private void UpdateDownloadButtonState()
        {
            downloadButton.Enabled = !string.IsNullOrWhiteSpace(urlTextBox.Text) && 
                                    !string.IsNullOrWhiteSpace(filenameTextBox.Text);
        }

        private async Task StartDownload()
        {
            string url = urlTextBox.Text.Trim();
            string filename = filenameTextBox.Text.Trim();

            // Validate inputs
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Please enter a URL.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                MessageBox.Show("Please enter a filename.", "Invalid Filename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!filename.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".tsv";
                filenameTextBox.Text = filename;
            }

            // Check if file already exists
            string downloadPath = historyManager.GetDownloadPath(filename);
            if (File.Exists(downloadPath))
            {
                var result = MessageBox.Show($"A route file named '{filename}' already exists. Do you want to overwrite it?", 
                    "File Already Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;
            }

            // Start download
            downloadButton.Enabled = false;
            urlTextBox.Enabled = false;
            filenameTextBox.Enabled = false;
            progressBar.Visible = true;
            statusLabel.Text = "Downloading...";

            try
            {
                var progress = new Progress<int>(value =>
                {
                    progressBar.Value = value;
                    statusLabel.Text = $"Downloading... {value}%";
                });

                downloadedFilePath = await downloadManager.DownloadRouteAsync(url, filename, progress);
                
                // Save to history
                historyManager.AddDownloadHistory(url, filename, downloadedFilePath);
                
                downloadCompleted = true;
                statusLabel.Text = "Download completed successfully!";
                continueButton.Enabled = true;
                
                if (loadAfterDownloadCheckBox.Checked)
                {
                    statusLabel.Text = "Download completed! Click Continue to load the route.";
                }
                else
                {
                    statusLabel.Text = "Download completed successfully!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Download failed: {ex.Message}", "Download Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Reset UI
                downloadButton.Enabled = true;
                urlTextBox.Enabled = true;
                filenameTextBox.Enabled = true;
                progressBar.Visible = false;
                statusLabel.Text = "Download failed. Please try again.";
            }
        }

        private void HandleContinue()
        {
            if (downloadCompleted && loadAfterDownloadCheckBox.Checked && !string.IsNullOrEmpty(downloadedFilePath))
            {
                try
                {
                    // Load the route in the main form
                    var routeManager = new RouteManager(downloadedFilePath, parentForm.GameConnectionManager);
                    parentForm.SetRouteManager(routeManager);
                    parentForm.LoadRouteDataPublic();
                    
                    string fileName = Path.GetFileName(downloadedFilePath);
                    MessageBox.Show($"Route loaded successfully: {fileName}", "Route Loaded", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load route: {ex.Message}", "Load Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (downloadCompleted)
            {
                MessageBox.Show("Route downloaded successfully!", "Download Complete", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void ShowImportDialog(MainForm parentForm)
        {
            using var importForm = new ImportRouteForm(parentForm);
            importForm.ShowDialog(parentForm);
        }
    }
}