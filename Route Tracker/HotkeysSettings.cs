using System;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Form for configuring hotkeys to manually complete or skip route entries
    // Allows users to assign custom keys for route entry management
    // ==========MY NOTES==============
    // This lets users set up keyboard shortcuts for marking entries done or skipped
    // Uses key capture to let them press whatever key they want to use
    public partial class HotkeysSettingsForm : Form
    {
        private readonly KeysConverter keysConverter = new();
        private Keys completeHotkey;
        private Keys skipHotkey;

        public HotkeysSettingsForm()
        {
            InitializeComponent();
            LoadHotkeys();

            // Apply app theme
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
        }

        // ==========FORMAL COMMENT=========
        // Initializes form components with consistent styling
        // Creates labeled text boxes and buttons for hotkey configuration
        // ==========MY NOTES==============
        // Sets up the form with text boxes to show the current hotkeys
        // Adds save and cancel buttons to confirm or discard changes
        private void InitializeComponent()
        {
            this.Text = "Configure Hotkeys";
            this.Size = new Size(400, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Create controls
            Label lblComplete = new()
            {
                Text = "Complete Entry Hotkey:",
                AutoSize = true,
                Location = new Point(20, 30)
            };

            TextBox txtCompleteHotkey = new()
            {
                Name = "txtCompleteHotkey",
                Size = new Size(200, 25),
                Location = new Point(180, 30),
                ReadOnly = true
            };
            txtCompleteHotkey.KeyDown += TxtCompleteHotkey_KeyDown;

            Label lblSkip = new()
            {
                Text = "Skip Entry Hotkey:",
                AutoSize = true,
                Location = new Point(20, 70)
            };

            TextBox txtSkipHotkey = new()
            {
                Name = "txtSkipHotkey",
                Size = new Size(200, 25),
                Location = new Point(180, 70),
                ReadOnly = true
            };
            txtSkipHotkey.KeyDown += TxtSkipHotkey_KeyDown;

            Label lblInfo = new()
            {
                Text = "Click in textbox and press desired key",
                AutoSize = true,
                Location = new Point(20, 120),
                Width = 360
            };

            Button btnSave = new()
            {
                Text = "Save",
                Size = new Size(100, 30),
                Location = new Point(140, 170)
            };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new()
            {
                Text = "Cancel",
                Size = new Size(100, 30),
                Location = new Point(260, 170)
            };
            btnCancel.Click += BtnCancel_Click;

            Button btnReset = new()
            {
                Text = "Reset to Default",
                Size = new Size(100, 30),
                Location = new Point(20, 170)
            };
            btnReset.Click += BtnReset_Click;

            // Add controls to form
            this.Controls.Add(lblComplete);
            this.Controls.Add(txtCompleteHotkey);
            this.Controls.Add(lblSkip);
            this.Controls.Add(txtSkipHotkey);
            this.Controls.Add(lblInfo);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnReset);
        }

        // ==========FORMAL COMMENT=========
        // Loads current hotkey settings from application configuration
        // Displays current hotkeys in the appropriate text boxes
        // ==========MY NOTES==============
        // Gets the saved hotkeys from settings
        // Shows them in the text boxes when the form opens
        private void LoadHotkeys()
        {
            // Load from settings
            completeHotkey = (Keys)Settings.Default.CompleteHotkey;
            skipHotkey = (Keys)Settings.Default.SkipHotkey;

            // Display in text boxes
            if (this.Controls["txtCompleteHotkey"] is TextBox txtComplete)
                txtComplete.Text = keysConverter.ConvertToString(completeHotkey);

            if (this.Controls["txtSkipHotkey"] is TextBox txtSkip)
                txtSkip.Text = keysConverter.ConvertToString(skipHotkey);
        }

        // ==========FORMAL COMMENT=========
        // Captures key press in the complete hotkey textbox
        // Updates the displayed hotkey and stores the value
        // ==========MY NOTES==============
        // Catches whatever key the user presses
        // Shows and remembers that key as the new complete hotkey
        private void TxtCompleteHotkey_KeyDown(object? sender, KeyEventArgs e)
        {
            completeHotkey = e.KeyCode;
            if (sender is TextBox txtBox)
                txtBox.Text = keysConverter.ConvertToString(completeHotkey);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        // ==========FORMAL COMMENT=========
        // Captures key press in the skip hotkey textbox
        // Updates the displayed hotkey and stores the value
        // ==========MY NOTES==============
        // Catches whatever key the user presses
        // Shows and remembers that key as the new skip hotkey
        private void TxtSkipHotkey_KeyDown(object? sender, KeyEventArgs e)
        {
            skipHotkey = e.KeyCode;
            if (sender is TextBox txtBox)
                txtBox.Text = keysConverter.ConvertToString(skipHotkey);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        // ==========FORMAL COMMENT=========
        // Saves the configured hotkeys to application settings
        // Closes the form with successful result
        // ==========MY NOTES==============
        // Saves the new hotkeys when the user clicks Save
        // Closes the form and tells the main form everything went OK
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            Settings.Default.CompleteHotkey = (int)completeHotkey;
            Settings.Default.SkipHotkey = (int)skipHotkey;
            Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        // ==========FORMAL COMMENT=========
        // Cancels hotkey configuration without saving
        // Closes the form without changing settings
        // ==========MY NOTES==============
        // Closes the form without saving any changes
        // Used when the user clicks Cancel
        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            // Set your hotkey controls to default values
            Settings.Default.CompleteHotkey = (int)Keys.None; // Code was generated by AI. Had to fix so no errors
            Settings.Default.SkipHotkey = (int)Keys.None;     // Code was generated by AI. Had to fix so no errors

            // Save to settings
            SettingsManager.SaveHotkeys(Keys.None, Keys.None);

            // Update the in-memory Fields and UI
            LoadHotkeys();

            // Optionally, update the UI or show a confirmation
            MessageBox.Show("Hotkeys have been reset to default.", "Reset Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}