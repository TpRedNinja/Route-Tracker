using System;
using System.Windows.Forms;
using Route_Tracker.Properties;
using System.Runtime.InteropServices;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Form for configuring hotkeys to manually complete or skip route entries
    // Allows users to assign custom keys for route entry management
    // Supports global hotkeys and advanced hotkey behaviors
    // ==========MY NOTES==============
    // Enhanced hotkey settings with global hotkeys and advanced mode
    // Now includes undo functionality and selection-based actions
    public partial class HotkeysSettingsForm : Form
    {
        private readonly KeysConverter keysConverter = new();
        private Keys completeHotkey;
        private Keys skipHotkey;
        private Keys undoHotkey;
        private bool globalHotkeys;
        private bool advancedHotkeys;
        private readonly SettingsManager? settingsManager;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID_COMPLETE = 1;
        private const int HOTKEY_ID_SKIP = 2;
        private const int HOTKEY_ID_UNDO = 3;

        public HotkeysSettingsForm(SettingsManager? settingsManager = null)
        {
            this.settingsManager = settingsManager;
            InitializeComponent();
            LoadHotkeys();
        }

        // ==========FORMAL COMMENT=========
        // Initializes form components with enhanced styling and new options
        // Creates all controls including new undo hotkey and setting checkboxes
        // ==========MY NOTES==============
        // Enhanced form with all the new controls for global and advanced hotkeys
        private void InitializeComponent()
        {
            this.Text = "Configure Hotkeys";
            this.Size = new Size(450, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Complete hotkey
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
                Location = new Point(200, 30),
                ReadOnly = true
            };
            txtCompleteHotkey.KeyDown += TxtCompleteHotkey_KeyDown;

            // Skip hotkey
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
                Location = new Point(200, 70),
                ReadOnly = true
            };
            txtSkipHotkey.KeyDown += TxtSkipHotkey_KeyDown;

            // Undo hotkey
            Label lblUndo = new()
            {
                Text = "Undo Entry Hotkey:",
                AutoSize = true,
                Location = new Point(20, 110)
            };

            TextBox txtUndoHotkey = new()
            {
                Name = "txtUndoHotkey",
                Size = new Size(200, 25),
                Location = new Point(200, 110),
                ReadOnly = true
            };
            txtUndoHotkey.KeyDown += TxtUndoHotkey_KeyDown;

            // Global hotkeys checkbox
            CheckBox chkGlobalHotkeys = new()
            {
                Name = "chkGlobalHotkeys",
                Text = "Global Hotkeys",
                AutoSize = true,
                Location = new Point(20, 160)
            };

            // Advanced hotkeys checkbox with tooltip
            CheckBox chkAdvancedHotkeys = new()
            {
                Name = "chkAdvancedHotkeys",
                Text = "Advanced Hotkeys",
                AutoSize = true,
                Location = new Point(20, 190)
            };

            // Tooltip for advanced hotkeys
            ToolTip toolTip = new();
            toolTip.SetToolTip(chkAdvancedHotkeys, "Allows hotkey actions to apply to any selected entry, not just the first incomplete entry.");

            // Info label
            Label lblInfo = new()
            {
                Text = "Click in textbox and press desired key",
                AutoSize = true,
                Location = new Point(20, 230),
                Width = 400
            };

            // Buttons
            Button btnSave = new()
            {
                Text = "Save",
                Size = new Size(100, 30),
                Location = new Point(150, 320)
            };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new()
            {
                Text = "Cancel",
                Size = new Size(100, 30),
                Location = new Point(270, 320)
            };
            btnCancel.Click += BtnCancel_Click;

            Button btnReset = new()
            {
                Text = "Reset to Default",
                Size = new Size(120, 30),
                Location = new Point(20, 320)
            };
            btnReset.Click += BtnReset_Click;

            // Add all controls
            this.Controls.AddRange([
                lblComplete, txtCompleteHotkey,
                lblSkip, txtSkipHotkey,
                lblUndo, txtUndoHotkey,
                chkGlobalHotkeys, chkAdvancedHotkeys,
                lblInfo, btnSave, btnCancel, btnReset
            ]);
        }

        // ==========FORMAL COMMENT=========
        // Loads current hotkey settings including new undo and option settings
        // ==========MY NOTES==============
        // Enhanced loading that includes all the new settings
        private void LoadHotkeys()
        {
            completeHotkey = (Keys)Settings.Default.CompleteHotkey;
            skipHotkey = (Keys)Settings.Default.SkipHotkey;
            undoHotkey = (Keys)Settings.Default.UndoHotkey;
            globalHotkeys = Settings.Default.GlobalHotkeys;
            advancedHotkeys = Settings.Default.AdvancedHotkeys;

            if (this.Controls["txtCompleteHotkey"] is TextBox txtComplete)
                txtComplete.Text = keysConverter.ConvertToString(completeHotkey);

            if (this.Controls["txtSkipHotkey"] is TextBox txtSkip)
                txtSkip.Text = keysConverter.ConvertToString(skipHotkey);

            if (this.Controls["txtUndoHotkey"] is TextBox txtUndo)
                txtUndo.Text = keysConverter.ConvertToString(undoHotkey);

            if (this.Controls["chkGlobalHotkeys"] is CheckBox chkGlobal)
                chkGlobal.Checked = globalHotkeys;

            if (this.Controls["chkAdvancedHotkeys"] is CheckBox chkAdvanced)
                chkAdvanced.Checked = advancedHotkeys;
        }

        private void TxtCompleteHotkey_KeyDown(object? sender, KeyEventArgs e)
        {
            completeHotkey = e.KeyCode;
            if (sender is TextBox txtBox)
                txtBox.Text = keysConverter.ConvertToString(completeHotkey);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void TxtSkipHotkey_KeyDown(object? sender, KeyEventArgs e)
        {
            skipHotkey = e.KeyCode;
            if (sender is TextBox txtBox)
                txtBox.Text = keysConverter.ConvertToString(skipHotkey);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void TxtUndoHotkey_KeyDown(object? sender, KeyEventArgs e)
        {
            undoHotkey = e.KeyCode;
            if (sender is TextBox txtBox)
                txtBox.Text = keysConverter.ConvertToString(undoHotkey);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        // ==========FORMAL COMMENT=========
        // Saves all hotkey settings including new undo and option settings
        // ==========MY NOTES==============
        // Enhanced saving that includes all the new settings
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Get checkbox states
            if (this.Controls["chkGlobalHotkeys"] is CheckBox chkGlobal)
                globalHotkeys = chkGlobal.Checked;

            if (this.Controls["chkAdvancedHotkeys"] is CheckBox chkAdvanced)
                advancedHotkeys = chkAdvanced.Checked;

            // Save all settings
            Settings.Default.CompleteHotkey = (int)completeHotkey;
            Settings.Default.SkipHotkey = (int)skipHotkey;
            Settings.Default.UndoHotkey = (int)undoHotkey;
            Settings.Default.GlobalHotkeys = globalHotkeys;
            Settings.Default.AdvancedHotkeys = advancedHotkeys;
            Settings.Default.Save();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            completeHotkey = Keys.None;
            skipHotkey = Keys.None;
            undoHotkey = Keys.None;
            globalHotkeys = false;
            advancedHotkeys = false;

            if (settingsManager != null)
            {
                settingsManager.SaveHotkeys(Keys.None, Keys.None);
                settingsManager.SaveHotkeySettings(Keys.None, false, false);
            }
            else
            {
                Settings.Default.CompleteHotkey = (int)Keys.None;
                Settings.Default.SkipHotkey = (int)Keys.None;
                Settings.Default.UndoHotkey = (int)Keys.None;
                Settings.Default.GlobalHotkeys = false;
                Settings.Default.AdvancedHotkeys = false;
                Settings.Default.Save();
            }

            LoadHotkeys();
            MessageBox.Show("Hotkeys have been reset to default.", "Reset Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}