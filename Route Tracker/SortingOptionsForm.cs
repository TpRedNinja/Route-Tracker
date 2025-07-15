using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Versioning;

namespace Route_Tracker
{
    public partial class SortingOptionsForm : Form
    {
        public enum SortingMode
        {
            CompletedAtTop,     // Default - completed at top, then incomplete
            CompletedAtBottom,  // Completed at bottom, incomplete at top
            HideCompleted       // Hide completed entirely
        }

        private SortingMode selectedSortingMode;
        private readonly SettingsManager settingsManager;

        public SortingMode SelectedSortingMode => selectedSortingMode;

        public SortingOptionsForm(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Sorting Options";
            this.Size = new Size(400, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = AppTheme.TextColor;

            var font = new Font(AppTheme.DefaultFont.FontFamily, 9f);

            // Main panel
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = AppTheme.BackgroundColor
            };

            // Title label
            var titleLabel = new Label
            {
                Text = "Route Grid Sorting Options",
                Font = new Font(font.FontFamily, 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // Description label
            var descriptionLabel = new Label
            {
                Text = "Choose how completed and incomplete route entries are displayed:",
                Font = font,
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 30),
                MaximumSize = new Size(340, 0)
            };

            // Radio buttons
            var completedTopRadio = new RadioButton
            {
                Text = "Completed entries at top",
                Font = font,
                ForeColor = AppTheme.TextColor,
                AutoSize = true,
                Location = new Point(0, 65),
                Tag = SortingMode.CompletedAtTop
            };

            var completedBottomRadio = new RadioButton
            {
                Text = "Completed entries at bottom",
                Font = font,
                ForeColor = AppTheme.TextColor,
                AutoSize = true,
                Location = new Point(0, 95),
                Tag = SortingMode.CompletedAtBottom
            };

            var hideCompletedRadio = new RadioButton
            {
                Text = "Hide completed entries",
                Font = font,
                ForeColor = AppTheme.TextColor,
                AutoSize = true,
                Location = new Point(0, 125),
                Tag = SortingMode.HideCompleted
            };

            // Tooltips
            var toolTip = new ToolTip();
            toolTip.SetToolTip(completedTopRadio, "Show completed entries first, then incomplete entries (default)");
            toolTip.SetToolTip(completedBottomRadio, "Show incomplete entries first, then completed entries at the bottom");
            toolTip.SetToolTip(hideCompletedRadio, "Hide completed entries from view (they are still saved in progress)");

            // Buttons
            var okButton = new Button
            {
                Text = "OK",
                Size = new Size(75, 23),
                Location = new Point(185, 170),
                DialogResult = DialogResult.OK,
                Font = font
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(75, 23),
                Location = new Point(270, 170),
                DialogResult = DialogResult.Cancel,
                Font = font
            };

            // Event handlers for radio buttons
            completedTopRadio.CheckedChanged += (s, e) => { if (completedTopRadio.Checked) selectedSortingMode = SortingMode.CompletedAtTop; };
            completedBottomRadio.CheckedChanged += (s, e) => { if (completedBottomRadio.Checked) selectedSortingMode = SortingMode.CompletedAtBottom; };
            hideCompletedRadio.CheckedChanged += (s, e) => { if (hideCompletedRadio.Checked) selectedSortingMode = SortingMode.HideCompleted; };

            // Add controls to main panel
            mainPanel.Controls.AddRange([
                titleLabel, descriptionLabel, completedTopRadio, completedBottomRadio, 
                hideCompletedRadio, okButton, cancelButton
            ]);

            // Apply theme
            AppTheme.ApplyToButton(okButton);
            AppTheme.ApplyToButton(cancelButton);

            this.Controls.Add(mainPanel);
        }

        private void LoadCurrentSettings()
        {
            selectedSortingMode = settingsManager.GetSortingMode();
            
            // Set the appropriate radio button
            foreach (Control control in this.Controls[0].Controls)
            {
                if (control is RadioButton radio && radio.Tag is SortingMode mode)
                {
                    radio.Checked = (mode == selectedSortingMode);
                }
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            // Save the selected sorting mode
            settingsManager.SaveSortingMode(selectedSortingMode);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}