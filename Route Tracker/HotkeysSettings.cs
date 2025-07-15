using System;
using System.Windows.Forms;
using Route_Tracker.Properties;
using System.Runtime.InteropServices;

namespace Route_Tracker
{
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
        private Keys shortResetP;
        private Keys shortRefresh;
        private Keys shortHelp;
        private Keys shortFilterC;
        private Keys shortConnect;
        private Keys shortGameStats;
        private Keys shortRouteStats;
        private Keys shortLayoutUp;
        private Keys shortLayoutDown;
        private Keys shortBackFold;
        private Keys shortBackNow;
        private Keys shortRestore;
        private Keys shortSetFold;
        private Keys shortAutoTog;
        private Keys shortTopTog;
        private Keys shortAdvTog;
        private Keys shortGlobalTog;
        private Keys shortImportRoute;
        private Keys shortSortingUp;
        private Keys shortSortingDown;
        private Keys shortGameDirect;
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

        private void InitializeComponent()
        {
            this.Text = "Configure Hotkeys";
            this.Size = new Size(400, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = AppTheme.TextColor;
            this.AutoScroll = true;

            var font = new Font(AppTheme.DefaultFont.FontFamily, 7.5f);

            int y = 20;
            int spacing = 25;

            Label lblComplete = new()
            {
                Text = "Complete Entry:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtCompleteHotkey = new()
            {
                Name = "txtCompleteHotkey",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                ReadOnly = true,
                Font = font
            };
            txtCompleteHotkey.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblSkip = new()
            {
                Text = "Skip Entry:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtSkipHotkey = new()
            {
                Name = "txtSkipHotkey",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                ReadOnly = true,
                Font = font
            };
            txtSkipHotkey.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblUndo = new()
            {
                Text = "Undo Entry:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtUndoHotkey = new()
            {
                Name = "txtUndoHotkey",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                ReadOnly = true,
                Font = font
            };
            txtUndoHotkey.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortLoad = new()
            {
                Text = "Load Route:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortLoad = new()
            {
                Name = "txtShortLoad",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortLoad.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortSave = new()
            {
                Text = "Save Progress:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortSave = new()
            {
                Name = "txtShortSave",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortSave.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortLoadP = new()
            {
                Text = "Load Progress:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortLoadP = new()
            {
                Name = "txtShortLoadP",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortLoadP.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortResetP = new()
            {
                Text = "Reset Progress:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortResetP = new()
            {
                Name = "txtShortResetP",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortResetP.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortRefresh = new()
            {
                Text = "Refresh:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortRefresh = new()
            {
                Name = "txtShortRefresh",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortRefresh.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortHelp = new()
            {
                Text = "Help:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortHelp = new()
            {
                Name = "txtShortHelp",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortHelp.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortFilterC = new()
            {
                Text = "Clear Filters:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortFilterC = new()
            {
                Name = "txtShortFilterC",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortFilterC.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortConnect = new()
            {
                Text = "Connect to Game:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortConnect = new()
            {
                Name = "txtShortConnect",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortConnect.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortGameStats = new()
            {
                Text = "Game Stats:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortGameStats = new()
            {
                Name = "txtShortGameStats",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortGameStats.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortRouteStats = new()
            {
                Text = "Route Stats:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortRouteStats = new()
            {
                Name = "txtShortRouteStats",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortRouteStats.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortLayoutUp = new()
            {
                Text = "Layout Next:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortLayoutUp = new()
            {
                Name = "txtShortLayoutUp",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortLayoutUp.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortLayoutDown = new()
            {
                Text = "Layout Previous:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortLayoutDown = new()
            {
                Name = "txtShortLayoutDown",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortLayoutDown.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortBackFold = new()
            {
                Text = "Open Backup Folder:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortBackFold = new()
            {
                Name = "txtShortBackFold",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortBackFold.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortBackNow = new()
            {
                Text = "Backup Now:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortBackNow = new()
            {
                Name = "txtShortBackNow",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortBackNow.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortRestore = new()
            {
                Text = "Restore Settings:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortRestore = new()
            {
                Name = "txtShortRestore",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortRestore.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortSetFold = new()
            {
                Text = "Open settings folder:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortSetFold = new()
            {
                Name = "txtShortSetFold",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortSetFold.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortAutoTog = new()
            {
                Text = "Toggle Auto-Start Game:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortAutoTog = new()
            {
                Name = "txtShortAutoTog",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortAutoTog.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortTopTog = new()
            {
                Text = "Toggle Always On Top:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortTopTog = new()
            {
                Name = "txtShortTopTog",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortTopTog.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortAdvTog = new()
            {
                Text = "Toggle Advanced Hotkeys:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortAdvTog = new()
            {
                Name = "txtShortAdvTog",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortAdvTog.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortGlobalTog = new()
            {
                Text = "Toggle Global Hotkeys:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortGlobalTog = new()
            {
                Name = "txtShortGlobalTog",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortGlobalTog.KeyDown += TextBoxes_KeysDown;
            y += spacing;
            Label lblShortImportRoute = new()
            {
                Text = "Import Route:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortImportRoute = new()
            {
                Name = "txtShortImportRoute",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortImportRoute.KeyDown += TextBoxes_KeysDown;
            y += spacing;
            Label lblShortSortingUp = new()
            {
                Text = "Sorting Mode Next:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortSortingUp = new()
            {
                Name = "txtShortSortingUp",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortSortingUp.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortSortingDown = new()
            {
                Text = "Sorting Mode Previous:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortSortingDown = new()
            {
                Name = "txtShortSortingDown",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortSortingDown.KeyDown += TextBoxes_KeysDown;

            y += spacing;
            Label lblShortGameDirect = new()
            {
                Text = "Game Directory:",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            TextBox txtShortGameDirect = new()
            {
                Name = "txtShortGameDirect",
                Size = new Size(110, 18),
                Location = new Point(160, y),
                Font = font
            };
            txtShortGameDirect.KeyDown += TextBoxes_KeysDown;

            y += spacing + 8;
            Label lblInfo = new()
            {
                Text = "Click in textbox and press desired key combination",
                AutoSize = true,
                Location = new Point(20, y),
                Width = 350,
                Font = font,
                ForeColor = Color.Gray
            };

            y += spacing;
            CheckBox chkGlobalHotkeys = new()
            {
                Name = "chkGlobalHotkeys",
                Text = "Global Hotkeys",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            CheckBox chkAdvancedHotkeys = new()
            {
                Name = "chkAdvancedHotkeys",
                Text = "Advanced Hotkeys",
                AutoSize = true,
                Location = new Point(140, y),
                Font = font
            };
            ToolTip toolTip = new();
            toolTip.SetToolTip(chkAdvancedHotkeys, "Allows hotkey actions to apply to any selected entry, not just the first incomplete entry.");

            y += spacing;
            Button btnSave = new()
            {
                Text = "Save",
                AutoSize = true,
                Location = new Point(20, y),
                Font = font
            };
            btnSave.Click += BtnSave_Click;
            AppTheme.ApplyToButton(btnSave);

            Button btnCancel = new()
            {
                Text = "Cancel",
                AutoSize = true,
                Location = new Point(140, y),
                Font = font
            };
            btnCancel.Click += BtnCancel_Click;
            AppTheme.ApplyToButton(btnCancel);

            Button btnReset = new()
            {
                Text = "Reset to default",
                AutoSize = true,
                Location = new Point(260, y),
                Font = font
            };
            btnReset.Click += BtnReset_Click;
            AppTheme.ApplyToButton(btnReset);

            this.Controls.AddRange([
                lblComplete, txtCompleteHotkey,
                lblSkip, txtSkipHotkey,
                lblUndo, txtUndoHotkey,
                lblShortLoad, txtShortLoad,
                lblShortSave, txtShortSave,
                lblShortLoadP, txtShortLoadP,
                lblShortResetP, txtShortResetP,
                lblShortRefresh, txtShortRefresh,
                lblShortHelp, txtShortHelp,
                lblShortFilterC, txtShortFilterC,
                lblShortConnect, txtShortConnect,
                lblShortGameStats, txtShortGameStats,
                lblShortRouteStats, txtShortRouteStats,
                lblShortLayoutUp, txtShortLayoutUp,
                lblShortLayoutDown, txtShortLayoutDown,
                lblShortBackFold, txtShortBackFold,
                lblShortBackNow, txtShortBackNow,
                lblShortRestore, txtShortRestore,
                lblShortSetFold, txtShortSetFold,
                lblShortAutoTog, txtShortAutoTog,
                lblShortTopTog, txtShortTopTog,
                lblShortAdvTog, txtShortAdvTog,
                lblShortGlobalTog, txtShortGlobalTog,
                lblShortImportRoute, txtShortImportRoute,
                lblShortSortingUp, txtShortSortingUp,
                lblShortSortingDown, txtShortSortingDown,
                lblShortGameDirect, txtShortGameDirect,
                lblInfo, chkGlobalHotkeys, chkAdvancedHotkeys,
                btnSave, btnCancel, btnReset
            ]);

            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = AppTheme.TextColor;
            AppTheme.ApplyTo(this);
        }

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
            shortResetP = (Keys)Settings.Default.ShortResetP;
            shortRefresh = (Keys)Settings.Default.ShortRefresh;
            shortHelp = (Keys)Settings.Default.ShortHelp;
            shortFilterC = (Keys)Settings.Default.ShortFilterC;
            shortConnect = (Keys)Settings.Default.ShortConnect;
            shortGameStats = (Keys)Settings.Default.ShortGameStats;
            shortRouteStats = (Keys)Settings.Default.ShortRouteStats;
            shortLayoutUp = (Keys)Settings.Default.ShortLayoutUp;
            shortLayoutDown = (Keys)Settings.Default.ShortLayoutDown;
            shortBackFold = (Keys)Settings.Default.ShortBackFold;
            shortBackNow = (Keys)Settings.Default.ShortBackNow;
            shortRestore = (Keys)Settings.Default.ShortRestore;
            shortSetFold = (Keys)Settings.Default.ShortSetFold;
            shortImportRoute = (Keys)Settings.Default.ShortImportRoute;
            shortSortingUp = (Keys)Settings.Default.SortingUp;
            shortSortingDown = (Keys)Settings.Default.SortingDown;
            shortGameDirect = (Keys)Settings.Default.GameDirect;

            if (this.Controls["txtShortLoad"] is TextBox txtLoad) txtLoad.Text = keysConverter.ConvertToString(shortLoad);
            if (this.Controls["txtShortSave"] is TextBox txtSave) txtSave.Text = keysConverter.ConvertToString(shortSave);
            if (this.Controls["txtShortLoadP"] is TextBox txtLoadP) txtLoadP.Text = keysConverter.ConvertToString(shortLoadP);
            if (this.Controls["txtShortResetP"] is TextBox txtResetP) txtResetP.Text = keysConverter.ConvertToString(shortResetP);
            if (this.Controls["txtShortRefresh"] is TextBox txtRefresh) txtRefresh.Text = keysConverter.ConvertToString(shortRefresh);
            if (this.Controls["txtShortHelp"] is TextBox txtHelp) txtHelp.Text = keysConverter.ConvertToString(shortHelp);
            if (this.Controls["txtShortFilterC"] is TextBox txtFilterC) txtFilterC.Text = keysConverter.ConvertToString(shortFilterC);
            if (this.Controls["txtShortConnect"] is TextBox txtConnect) txtConnect.Text = keysConverter.ConvertToString(shortConnect);
            if (this.Controls["txtShortGameStats"] is TextBox txtGameStats) txtGameStats.Text = keysConverter.ConvertToString(shortGameStats);
            if (this.Controls["txtShortRouteStats"] is TextBox txtRouteStats) txtRouteStats.Text = keysConverter.ConvertToString(shortRouteStats);
            if (this.Controls["txtShortLayoutUp"] is TextBox txtLayoutUp) txtLayoutUp.Text = keysConverter.ConvertToString(shortLayoutUp);
            if (this.Controls["txtShortLayoutDown"] is TextBox txtLayoutDown) txtLayoutDown.Text = keysConverter.ConvertToString(shortLayoutDown);
            if (this.Controls["txtShortBackFold"] is TextBox txtBackFold) txtBackFold.Text = keysConverter.ConvertToString(shortBackFold);
            if (this.Controls["txtShortBackNow"] is TextBox txtBackNow) txtBackNow.Text = keysConverter.ConvertToString(shortBackNow);
            if (this.Controls["txtShortRestore"] is TextBox txtRestore) txtRestore.Text = keysConverter.ConvertToString(shortRestore);
            if (this.Controls["txtShortSetFold"] is TextBox txtSetFold) txtSetFold.Text = keysConverter.ConvertToString(shortSetFold);
            if (this.Controls["txtShortImportRoute"] is TextBox txtImportRoute) txtImportRoute.Text = keysConverter.ConvertToString(shortImportRoute);
            if (this.Controls["txtShortSortingUp"] is TextBox txtSortingUp) txtSortingUp.Text = keysConverter.ConvertToString(shortSortingUp);
            if (this.Controls["txtShortSortingDown"] is TextBox txtSortingDown) txtSortingDown.Text = keysConverter.ConvertToString(shortSortingDown);
            if (this.Controls["txtShortGameDirect"] is TextBox txtGameDirect) txtGameDirect.Text = keysConverter.ConvertToString(shortGameDirect);

            //toggle shortcuts
            shortAutoTog = (Keys)Settings.Default.AutoTog;
            shortTopTog = (Keys)Settings.Default.TopTog;
            shortAdvTog = (Keys)Settings.Default.AdvTog;
            shortGlobalTog = (Keys)Settings.Default.GlobalTog;

            if (this.Controls["txtShortAutoTog"] is TextBox txtAutoTog) txtAutoTog.Text = keysConverter.ConvertToString(shortAutoTog);
            if (this.Controls["txtShortTopTog"] is TextBox txtTopTog) txtTopTog.Text = keysConverter.ConvertToString(shortTopTog);
            if (this.Controls["txtShortAdvTog"] is TextBox txtAdvTog) txtAdvTog.Text = keysConverter.ConvertToString(shortAdvTog);
            if (this.Controls["txtShortGlobalTog"] is TextBox txtGlobalTog) txtGlobalTog.Text = keysConverter.ConvertToString(shortGlobalTog);

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
                case "txtShortResetP":
                    shortResetP = value;
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
                case "txtShortConnect":
                    shortConnect = value;
                    break;
                case "txtShortGameStats":
                    shortGameStats = value;
                    break;
                case "txtShortRouteStats":
                    shortRouteStats = value;
                    break;
                case "txtShortLayoutUp":
                    shortLayoutUp = value;
                    break;
                case "txtShortLayoutDown":
                    shortLayoutDown = value;
                    break;
                case "txtShortBackFold":
                    shortBackFold = value;
                    break;
                case "txtShortBackNow":
                    shortBackNow = value;
                    break;
                case "txtShortRestore":
                    shortRestore = value;
                    break;
                case "txtShortSetFold":
                    shortSetFold = value;
                    break;
                case "txtShortAutoTog":
                    shortAutoTog = value;
                    break;
                case "txtShortTopTog":
                    shortTopTog = value;
                    break;
                case "txtShortAdvTog":
                    shortAdvTog = value;
                    break;
                case "txtShortGlobalTog":
                    shortGlobalTog = value;
                    break;
                case "txtShortImportRoute":
                    shortImportRoute = value;
                    break;
                case "txtShortSortingUp":
                    shortSortingUp = value;
                    break;
                case "txtShortSortingDown":
                    shortSortingDown = value;
                    break;
                case "txtShortGameDirect":
                    shortGameDirect = value;
                    break;
            }

            txtBox.Text = keysConverter.ConvertToString(value);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (this.Controls["chkGlobalHotkeys"] is CheckBox chkGlobal)
                globalHotkeys = chkGlobal.Checked;

            if (this.Controls["chkAdvancedHotkeys"] is CheckBox chkAdvanced)
                advancedHotkeys = chkAdvanced.Checked;

            Settings.Default.CompleteHotkey = (int)completeHotkey;
            Settings.Default.SkipHotkey = (int)skipHotkey;
            Settings.Default.UndoHotkey = (int)undoHotkey;
            Settings.Default.GlobalHotkeys = globalHotkeys;
            Settings.Default.AdvancedHotkeys = advancedHotkeys;
            Settings.Default.ShortLoad = (int)shortLoad;
            Settings.Default.ShortSave = (int)shortSave;
            Settings.Default.ShortLoadP = (int)shortLoadP;
            Settings.Default.ShortResetP = (int)shortResetP;
            Settings.Default.ShortRefresh = (int)shortRefresh;
            Settings.Default.ShortHelp = (int)shortHelp;
            Settings.Default.ShortFilterC = (int)shortFilterC;
            Settings.Default.ShortConnect = (int)shortConnect;
            Settings.Default.ShortGameStats = (int)shortGameStats;
            Settings.Default.ShortRouteStats = (int)shortRouteStats;
            Settings.Default.ShortLayoutUp = (int)shortLayoutUp;
            Settings.Default.ShortLayoutDown = (int)shortLayoutDown;
            Settings.Default.ShortBackFold = (int)shortBackFold;
            Settings.Default.ShortBackNow = (int)shortBackNow;
            Settings.Default.ShortRestore = (int)shortRestore;
            Settings.Default.ShortSetFold = (int)shortSetFold;
            Settings.Default.AutoTog = (int)shortAutoTog;
            Settings.Default.TopTog = (int)shortTopTog;
            Settings.Default.AdvTog = (int)shortAdvTog;
            Settings.Default.GlobalTog = (int)shortGlobalTog;
            Settings.Default.ShortImportRoute = (int)shortImportRoute;
            Settings.Default.SortingUp = (int)shortSortingUp;
            Settings.Default.SortingDown = (int)shortSortingDown;
            Settings.Default.GameDirect = (int)shortGameDirect;
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
            shortResetP = Keys.Control | Keys.R;
            shortRefresh = Keys.F5;
            shortHelp = Keys.F1;
            shortFilterC = Keys.Escape;
            shortConnect = Keys.Shift | Keys.C;
            shortGameStats = Keys.Shift | Keys.S;
            shortRouteStats = Keys.Shift | Keys.R;
            shortLayoutUp = Keys.Alt | Keys.M;
            shortLayoutDown = Keys.Shift | Keys.M;
            shortBackFold = Keys.Control | Keys.B;
            shortBackNow = Keys.Shift | Keys.B;
            shortRestore = Keys.Control | Keys.Shift | Keys.B;
            shortSetFold = Keys.Control | Keys.Shift | Keys.S;
            shortAutoTog = Keys.Control | Keys.A;
            shortTopTog = Keys.Control | Keys.T;
            shortAdvTog = Keys.Shift | Keys.A;
            shortGlobalTog = Keys.Control | Keys.G;
            shortImportRoute = Keys.Control | Keys.U;
            shortSortingUp = Keys.Alt | Keys.D;
            shortSortingDown = Keys.Shift | Keys.D;
            shortGameDirect = Keys.Control | Keys.D;

            if (settingsManager != null)
            {
                settingsManager.SaveHotkeys(Keys.None, Keys.None);
                settingsManager.SaveHotkeySettings(Keys.None, false, false);
                settingsManager.SaveShortcuts(shortLoad, shortSave, shortLoadP,
                    shortResetP, shortRefresh, shortHelp, shortFilterC, shortConnect,
                    shortGameStats, shortRouteStats, shortLayoutUp, shortLayoutDown,
                    shortBackFold, shortBackNow, shortRestore, shortSetFold,
                    shortAutoTog, shortTopTog, shortAdvTog, shortGlobalTog, shortImportRoute,
                    shortSortingUp, shortSortingDown, shortGameDirect);
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
                Settings.Default.ShortResetP = (int)shortResetP;
                Settings.Default.ShortRefresh = (int)shortRefresh;
                Settings.Default.ShortHelp = (int)shortHelp;
                Settings.Default.ShortFilterC = (int)shortFilterC;
                Settings.Default.ShortConnect = (int)shortConnect;
                Settings.Default.ShortGameStats = (int)shortGameStats;
                Settings.Default.ShortRouteStats = (int)shortRouteStats;
                Settings.Default.ShortLayoutUp = (int)shortLayoutUp;
                Settings.Default.ShortLayoutDown = (int)shortLayoutDown;
                Settings.Default.ShortBackFold = (int)shortBackFold;
                Settings.Default.ShortBackNow = (int)shortBackNow;
                Settings.Default.ShortRestore = (int)shortRestore;
                Settings.Default.ShortSetFold = (int)shortSetFold;
                Settings.Default.AutoTog = (int)shortAutoTog;
                Settings.Default.TopTog = (int)shortTopTog;
                Settings.Default.AdvTog = (int)shortAdvTog;
                Settings.Default.GlobalTog = (int)shortGlobalTog;
                Settings.Default.ShortImportRoute = (int)shortImportRoute;
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