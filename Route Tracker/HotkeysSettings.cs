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
        private Keys shortLoad;
        private Keys shortSave;
        private Keys shortLoadP;
        private Keys shortRefresh;
        private Keys shortHelp;
        private Keys shortFilterC;
        public readonly SettingsManager? settingsManager;

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
            this.Size = new Size(450, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = AppTheme.TextColor;

            // Hotkey labels and textboxes
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
            txtCompleteHotkey.KeyDown += TextBoxes_KeysDown;

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
            txtSkipHotkey.KeyDown += TextBoxes_KeysDown;

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
            txtUndoHotkey.KeyDown += TextBoxes_KeysDown;

            // Shortcut labels and textboxes
            Label lblShortLoad = new()
            {
                Text = "Shortcut: Load Route",
                AutoSize = true,
                Location = new Point(20, 150)
            };
            TextBox txtShortLoad = new()
            {
                Name = "txtShortLoad",
                Size = new Size(200, 25),
                Location = new Point(200, 150)
            };
            txtShortLoad.KeyDown += TextBoxes_KeysDown;

            Label lblShortSave = new()
            {
                Text = "Shortcut: Save Progress",
                AutoSize = true,
                Location = new Point(20, 190)
            };
            TextBox txtShortSave = new()
            {
                Name = "txtShortSave",
                Size = new Size(200, 25),
                Location = new Point(200, 190)
            };
            txtShortSave.KeyDown += TextBoxes_KeysDown;

            Label lblShortLoadP = new()
            {
                Text = "Shortcut: Load Progress",
                AutoSize = true,
                Location = new Point(20, 230)
            };
            TextBox txtShortLoadP = new()
            {
                Name = "txtShortLoadP",
                Size = new Size(200, 25),
                Location = new Point(200, 230)
            };
            txtShortLoadP.KeyDown += TextBoxes_KeysDown;

            Label lblShortRefresh = new()
            {
                Text = "Shortcut: Refresh",
                AutoSize = true,
                Location = new Point(20, 270)
            };
            TextBox txtShortRefresh = new()
            {
                Name = "txtShortRefresh",
                Size = new Size(200, 25),
                Location = new Point(200, 270)
            };
            txtShortRefresh.KeyDown += TextBoxes_KeysDown;

            Label lblShortHelp = new()
            {
                Text = "Shortcut: Help",
                AutoSize = true,
                Location = new Point(20, 310)
            };
            TextBox txtShortHelp = new()
            {
                Name = "txtShortHelp",
                Size = new Size(200, 25),
                Location = new Point(200, 310)
            };
            txtShortHelp.KeyDown += TextBoxes_KeysDown;

            Label lblShortFilterC = new()
            {
                Text = "Shortcut: Clear Filters",
                AutoSize = true,
                Location = new Point(20, 350)
            };
            TextBox txtShortFilterC = new()
            {
                Name = "txtShortFilterC",
                Size = new Size(200, 25),
                Location = new Point(200, 350)
            };
            txtShortFilterC.KeyDown += TextBoxes_KeysDown;

            // Info label
            Label lblInfo = new()
            {
                Text = "Click in textbox and press desired key",
                AutoSize = true,
                Location = new Point(20, 390),
                Width = 400
            };

            // Checkboxes (moved below all textboxes)
            CheckBox chkGlobalHotkeys = new()
            {
                Name = "chkGlobalHotkeys",
                Text = "Global Hotkeys",
                AutoSize = true,
                Location = new Point(20, 430)
            };
            CheckBox chkAdvancedHotkeys = new()
            {
                Name = "chkAdvancedHotkeys",
                Text = "Advanced Hotkeys",
                AutoSize = true,
                Location = new Point(20, 460)
            };
            ToolTip toolTip = new();
            toolTip.SetToolTip(chkAdvancedHotkeys, "Allows hotkey actions to apply to any selected entry, not just the first incomplete entry.");

            // Buttons (moved below checkboxes)
            Button btnSave = new()
            {
                Text = "Save",
                Size = new Size(100, 30),
                Location = new Point(150, 510)
            };
            btnSave.Click += BtnSave_Click;
            AppTheme.ApplyToButton(btnSave);

            Button btnCancel = new()
            {
                Text = "Cancel",
                Size = new Size(100, 30),
                Location = new Point(270, 510)
            };
            btnCancel.Click += BtnCancel_Click;
            AppTheme.ApplyToButton(btnCancel);

            Button btnReset = new()
            {
                Text = "Reset to Default",
                Size = new Size(120, 30),
                Location = new Point(20, 510)
            };
            btnReset.Click += BtnReset_Click;
            AppTheme.ApplyToButton(btnReset);

            // Add all controls
            this.Controls.AddRange(
            [
                lblComplete, txtCompleteHotkey,
                lblSkip, txtSkipHotkey,
                lblUndo, txtUndoHotkey,
                lblShortLoad, txtShortLoad,
                lblShortSave, txtShortSave,
                lblShortLoadP, txtShortLoadP,
                lblShortRefresh, txtShortRefresh,
                lblShortHelp, txtShortHelp,
                lblShortFilterC, txtShortFilterC,
                lblInfo,
                chkGlobalHotkeys, chkAdvancedHotkeys,
                btnSave, btnCancel, btnReset
            ]);

            // Add this at the end of InitializeComponent()
            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = AppTheme.TextColor;
            AppTheme.ApplyTo(this);
            AppTheme.ApplyToButton(btnSave);
            AppTheme.ApplyToButton(btnCancel);
            AppTheme.ApplyToButton(btnReset);
        }

        // ==========FORMAL COMMENT=========
        // Loads current hotkey settings including new undo and option settings
        // ==========MY NOTES==============
        // Enhanced loading that includes all the new settings
        private void LoadHotkeys()
        {
            // hotkey stuff
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

            // shortcut stuff
            shortLoad = (Keys)Settings.Default.ShortLoad;
            shortSave = (Keys)Settings.Default.ShortSave;
            shortLoadP = (Keys)Settings.Default.ShortLoadP;
            shortRefresh = (Keys)Settings.Default.ShortRefresh;
            shortHelp = (Keys)Settings.Default.ShortHelp;
            shortFilterC = (Keys)Settings.Default.ShortFilterC;

            if (this.Controls["txtShortLoad"] is TextBox txtLoad) txtLoad.Text = keysConverter.ConvertToString(shortLoad);
            if (this.Controls["txtShortSave"] is TextBox txtSave) txtSave.Text = keysConverter.ConvertToString(shortSave);
            if (this.Controls["txtShortLoadP"] is TextBox txtLoadP) txtLoadP.Text = keysConverter.ConvertToString(shortLoadP);
            if (this.Controls["txtShortRefresh"] is TextBox txtRefresh) txtRefresh.Text = keysConverter.ConvertToString(shortRefresh);
            if (this.Controls["txtShortHelp"] is TextBox txtHelp) txtHelp.Text = keysConverter.ConvertToString(shortHelp);
            if (this.Controls["txtShortFilterC"] is TextBox txtFilterC) txtFilterC.Text = keysConverter.ConvertToString(shortFilterC);

        }

        private void TextBoxes_KeysDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox txtBox)
                return;

            Keys value = e.KeyCode | e.Modifiers;

            switch (txtBox.Name)
            {
                case "txtCompleteHotkey":
                    completeHotkey = value;
                    break;
                case "txtSkipHotkey":
                    skipHotkey = value;
                    break;
                case "txtUndoHotkey":
                    undoHotkey = value;
                    break;
                case "txtShortLoad":
                    shortLoad = value;
                    break;
                case "txtShortSave":
                    shortSave = value;
                    break;
                case "txtShortLoadP":
                    shortLoadP = value;
                    break;
                case "txtShortRefresh":
                    shortRefresh = value;
                    break;
                case "txtShortHelp":
                    shortHelp = value;
                    break;
                case "txtShortFilterC":
                    shortFilterC = value;
                    break;
            }

            txtBox.Text = keysConverter.ConvertToString(value);
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
            Settings.Default.ShortLoad = (int)shortLoad;
            Settings.Default.ShortSave = (int)shortSave;
            Settings.Default.ShortLoadP = (int)shortLoadP;
            Settings.Default.ShortRefresh = (int)shortRefresh;
            Settings.Default.ShortHelp = (int)shortHelp;
            Settings.Default.ShortFilterC = (int)shortFilterC;
            Settings.Default.Save();

            if (this.Owner is MainForm mainForm)
            {
                mainForm.RefreshHelpShortcutLabel();
            }

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
            shortLoad = Keys.Control | Keys.O;
            shortSave = Keys.Control | Keys.S;
            shortLoadP = Keys.Control | Keys.L;
            shortRefresh = Keys.F5;
            shortHelp = Keys.F1;
            shortFilterC = Keys.Escape;

            if (settingsManager != null)
            {
                settingsManager.SaveHotkeys(Keys.None, Keys.None);
                settingsManager.SaveHotkeySettings(Keys.None, false, false);
                settingsManager.SaveShortcuts(shortLoad, shortSave, shortLoadP, shortRefresh, shortHelp, shortFilterC);
            }
            else
            {
                Settings.Default.CompleteHotkey = (int)Keys.None;
                Settings.Default.SkipHotkey = (int)Keys.None;
                Settings.Default.UndoHotkey = (int)Keys.None;
                Settings.Default.GlobalHotkeys = false;
                Settings.Default.AdvancedHotkeys = false;
                Settings.Default.ShortLoad = (int)shortLoad;
                Settings.Default.ShortSave = (int)shortSave;
                Settings.Default.ShortLoadP = (int)shortLoadP;
                Settings.Default.ShortRefresh = (int)shortRefresh;
                Settings.Default.ShortHelp = (int)shortHelp;
                Settings.Default.ShortFilterC = (int)shortFilterC;
                Settings.Default.Save();
            }

            LoadHotkeys();

            if (this.Owner is MainForm mainForm)
            {
                mainForm.RefreshHelpShortcutLabel();
            }

            MessageBox.Show("Hotkeys have been reset to default.", "Reset Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}