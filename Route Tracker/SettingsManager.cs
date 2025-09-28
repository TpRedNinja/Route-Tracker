using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    public class SettingsManager
    {
        public bool IsLoadingSettings = false;

        private static readonly string BackupFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RouteTracker",
            "Settings"
        );
        private static readonly string BackupFilePath = Path.Combine(BackupFolder, "settings_backup.json");
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public class SettingsBackup
        {
            public int CompleteHotkey { get; set; } = 0;
            public int SkipHotkey { get; set; } = 0;
            public int UndoHotkey { get; set; } = 0;
            public bool HotkeysEnabled { get; set; } = false;
            public bool AlwaysOnTop { get; set; } = false;
            public bool CheckForUpdateOnStartup { get; set; } = true;
            public bool DevMode { get; set; } = false;
            public DateTime BackupTimestamp { get; set; } = DateTime.Now;
            public string AppVersion { get; set; } = string.Empty;
            public string LayoutMode { get; set; } = "Normal";
            public int ShortLoad { get; set; } = (int)(Keys.Control | Keys.O);
            public int ShortSave { get; set; } = (int)(Keys.Control | Keys.S);
            public int ShortLoadP { get; set; } = (int)(Keys.Control | Keys.L);
            public int ShortResetP { get; set; } = (int)(Keys.Control | Keys.R);
            public int ShortRefresh { get; set; } = (int)Keys.F5;
            public int ShortHelp { get; set; } = (int)Keys.F1;
            public int ShortFilterC { get; set; } = (int)Keys.Escape;
            public int ShortConnect { get; set; } = (int)(Keys.Shift | Keys.C);
            public int ShortGameStats { get; set; } = (int)(Keys.Shift | Keys.S);
            public int ShortRouteStats { get; set; } = (int)(Keys.Shift | Keys.R);
            public int ShortLayoutUp { get; set; } = (int)(Keys.Alt | Keys.M);
            public int ShortLayoutDown { get; set; } = (int)(Keys.Shift | Keys.M);
            public int ShortBackFold { get; set; } = (int)(Keys.Control | Keys.B);
            public int ShortBackNow { get; set; } = (int)(Keys.Shift | Keys.B);
            public int ShortRestore { get; set; } = (int)(Keys.Control | Keys.Shift | Keys.B);
            public int ShortSetFold { get; set; } = (int)(Keys.Control | Keys.Shift | Keys.S);
            public int AutoTog { get; set; } = (int)(Keys.Control | Keys.A);
            public int TopTog { get; set; } = (int)(Keys.Control | Keys.T);
            public int AdvTog { get; set; } = (int)(Keys.Shift | Keys.A);
            public int GlobalTog { get; set; } = (int)(Keys.Control | Keys.G);
            public int SortingMode { get; set; } = 0;
            public int SortingUp { get; set; } = (int)(Keys.Alt | Keys.D);
            public int SortingDown { get; set; } = (int)(Keys.Shift | Keys.D);
            public int GameDirect { get; set; } = (int)(Keys.Control | Keys.D);
        }

        #region Backup/Restore Settings
        public void BackupSettings()
        {
            try
            {
                if (!Directory.Exists(BackupFolder))
                    Directory.CreateDirectory(BackupFolder);

                SettingsBackup backup = new()
                {
                    CompleteHotkey = Settings.Default.CompleteHotkey,
                    SkipHotkey = Settings.Default.SkipHotkey,
                    UndoHotkey = Settings.Default.UndoHotkey,
                    HotkeysEnabled = Settings.Default.HotkeysEnabled,
                    AlwaysOnTop = Settings.Default.AlwaysOnTop,
                    CheckForUpdateOnStartup = Settings.Default.CheckForUpdateOnStartup,
                    DevMode = Settings.Default.DevMode,
                    BackupTimestamp = DateTime.Now,
                    AppVersion = AppTheme.Version,
                    LayoutMode = Settings.Default.LayoutMode,
                    ShortLoad = Settings.Default.ShortLoad,
                    ShortSave = Settings.Default.ShortSave,
                    ShortLoadP = Settings.Default.ShortLoadP,
                    ShortResetP = Settings.Default.ShortResetP,
                    ShortRefresh = Settings.Default.ShortRefresh,
                    ShortHelp = Settings.Default.ShortHelp,
                    ShortFilterC = Settings.Default.ShortFilterC,
                    ShortConnect = Settings.Default.ShortConnect,
                    ShortGameStats = Settings.Default.ShortGameStats,
                    ShortRouteStats = Settings.Default.ShortRouteStats,
                    ShortLayoutUp = Settings.Default.ShortLayoutUp,
                    ShortLayoutDown = Settings.Default.ShortLayoutDown,
                    ShortBackFold = Settings.Default.ShortBackFold,
                    ShortBackNow = Settings.Default.ShortBackNow,
                    ShortRestore = Settings.Default.ShortRestore,
                    ShortSetFold = Settings.Default.ShortSetFold,
                    AutoTog = Settings.Default.AutoTog,
                    TopTog = Settings.Default.TopTog,
                    AdvTog = Settings.Default.AdvTog,
                    GlobalTog = Settings.Default.GlobalTog,
                    SortingMode = Settings.Default.SortingMode,
                    SortingUp = Settings.Default.SortingUp,
                    SortingDown = Settings.Default.SortingDown,
                    GameDirect = Settings.Default.GameDirect
                };

                string json = JsonSerializer.Serialize(backup, JsonOptions);
                File.WriteAllText(BackupFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error backing up settings: {ex.Message}");
            }
        }

        public bool RestoreFromBackup()
        {
            try
            {
                if (!File.Exists(BackupFilePath))
                    return false;

                string json = File.ReadAllText(BackupFilePath);
                var backup = System.Text.Json.JsonSerializer.Deserialize<SettingsBackup>(json);

                if (backup == null)
                    return false;

                Settings.Default.CompleteHotkey = backup.CompleteHotkey;
                Settings.Default.SkipHotkey = backup.SkipHotkey;
                Settings.Default.UndoHotkey = backup.UndoHotkey;
                Settings.Default.HotkeysEnabled = backup.HotkeysEnabled;
                Settings.Default.AlwaysOnTop = backup.AlwaysOnTop;
                Settings.Default.CheckForUpdateOnStartup = backup.CheckForUpdateOnStartup;
                Settings.Default.DevMode = backup.DevMode;
                Settings.Default.LayoutMode = backup.LayoutMode;
                Settings.Default.ShortLoad = backup.ShortLoad;
                Settings.Default.ShortSave = backup.ShortSave;
                Settings.Default.ShortLoadP = backup.ShortLoadP;
                Settings.Default.ShortResetP = backup.ShortResetP;
                Settings.Default.ShortRefresh = backup.ShortRefresh;
                Settings.Default.ShortHelp = backup.ShortHelp;
                Settings.Default.ShortFilterC = backup.ShortFilterC;
                Settings.Default.ShortConnect = backup.ShortConnect;
                Settings.Default.ShortGameStats = backup.ShortGameStats;
                Settings.Default.ShortRouteStats = backup.ShortRouteStats;
                Settings.Default.ShortLayoutUp = backup.ShortLayoutUp;
                Settings.Default.ShortLayoutDown = backup.ShortLayoutDown;
                Settings.Default.ShortBackFold = backup.ShortBackFold;
                Settings.Default.ShortBackNow = backup.ShortBackNow;
                Settings.Default.ShortRestore = backup.ShortRestore;
                Settings.Default.ShortSetFold = backup.ShortSetFold;
                Settings.Default.AutoTog = backup.AutoTog;
                Settings.Default.TopTog = backup.TopTog;
                Settings.Default.AdvTog = backup.AdvTog;
                Settings.Default.GlobalTog = backup.GlobalTog;
                Settings.Default.SortingMode = backup.SortingMode;
                Settings.Default.SortingUp = backup.SortingUp;
                Settings.Default.SortingDown = backup.SortingDown;

                Settings.Default.Save();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error restoring settings: {ex.Message}");
                return false;
            }
        }

        // ==========MY NOTES==============
        // Gets backup info without restoring
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public (bool HasBackup, DateTime BackupDate, string BackupVersion) GetBackupInfo()
        {
            try
            {
                if (!File.Exists(BackupFilePath))
                    return (false, DateTime.MinValue, string.Empty);

                string json = File.ReadAllText(BackupFilePath);
                var backup = JsonSerializer.Deserialize<SettingsBackup>(json);

                if (backup == null)
                    return (false, DateTime.MinValue, string.Empty);

                return (true, backup.BackupTimestamp, backup.AppVersion);
            }
            catch
            {
                return (false, DateTime.MinValue, string.Empty);
            }
        }

        // ==========MY NOTES==============
        // Opens the backup folder in Windows Explorer
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public void OpenBackupFolder()
        {
            try
            {
                if (Directory.Exists(BackupFolder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", BackupFolder);
                }
                else
                {
                    // Create folder first, then open
                    Directory.CreateDirectory(BackupFolder);
                    System.Diagnostics.Process.Start("explorer.exe", BackupFolder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open backup folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ==========MY NOTES==============
        // Checks if this is first run and offers to restore from backup
        public void CheckFirstRun()
        {
            // If current settings are empty but backup exists, offer restore
            bool settingsEmpty = string.IsNullOrEmpty(Settings.Default.GameDirectory) &&
                                string.IsNullOrEmpty(Settings.Default.AC4Directory) &&
                                string.IsNullOrEmpty(Settings.Default.Gow2018Directory);

            var (hasBackup, backupDate, backupVersion) = GetBackupInfo();

            if (settingsEmpty && hasBackup)
            {
                var result = MessageBox.Show(
                    $"Found settings backup from {backupDate:yyyy-MM-dd HH:mm} (version {backupVersion})\n\n" +
                    "Would you like to restore your previous settings?",
                    "Restore Settings",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (RestoreFromBackup())
                    {
                        MessageBox.Show("Settings restored successfully!", "Settings Restored",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to restore settings from backup.", "Restore Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
        #endregion

        #region Settings Loading/Saving
        public void LoadSettings(TextBox gameDirectoryTextBox)
        {
            IsLoadingSettings = true;
            gameDirectoryTextBox.Text = Settings.Default.GameDirectory;
            IsLoadingSettings = false;
        }

        // ==========MY NOTES==============
        // Enhanced save method that also creates backup
        [SupportedOSPlatform("windows6.1")]
        public void SaveAlwaysOnTop(bool alwaysOnTop)
        {
            Settings.Default.AlwaysOnTop = alwaysOnTop;
            Settings.Default.Save();
            BackupSettings();
        }

        [SupportedOSPlatform("windows6.1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public bool GetAlwaysOnTop()
        {
            return Settings.Default.AlwaysOnTop;
        }

        // ==========MY NOTES==============
        // Enhanced save method that also creates backup
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        [SupportedOSPlatform("windows6.1")]
        public void SaveSettings(string gameDirectory, string autoStartGame)
        {
            Settings.Default.GameDirectory = gameDirectory;
            Settings.Default.AutoStart = autoStartGame;
            Settings.Default.Save();
            BackupSettings(); // Auto-backup after saving
        }
        #endregion

        #region Game Directory Management
        // ==========MY NOTES==============
        // Enhanced save method that also creates backup
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        [SupportedOSPlatform("windows6.1")]
        public void SaveDirectory(string selectedGame, string directory)
        {
            // Map display name to exe name to determine which setting to use
            string? exeName = SupportedGames.GameList
                .FirstOrDefault(kvp => kvp.Value.DisplayName == selectedGame).Key;

            switch (exeName)
            {
                case "AC4BFSP":
                    Settings.Default.AC4Directory = directory;
                    break;
                case "GoW":
                    Settings.Default.Gow2018Directory = directory;
                    break;
            }

            Settings.Default.Save();

            // Enhanced: Automatically create backup after saving
            BackupSettings();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        [SupportedOSPlatform("windows6.1")]
        public string GetGameDirectory(string game)
        {
            // Map display name to exe name to determine which setting to use
            string? exeName = SupportedGames.GameList
                .FirstOrDefault(kvp => kvp.Value.DisplayName == game).Key;

            return exeName switch
            {
                "AC4BFSP" => Settings.Default.AC4Directory,
                "GoW" => Settings.Default.Gow2018Directory,
                _ => string.Empty
            };
        }
        #endregion

        #region Hotkey Settings
        public void SaveHotkeys(Keys completeHotkey, Keys skipHotkey)
        {
            Settings.Default.CompleteHotkey = (int)completeHotkey;
            Settings.Default.SkipHotkey = (int)skipHotkey;
            Settings.Default.Save();
            BackupSettings(); // Auto-backup after saving
        }

        public (Keys CompleteHotkey, Keys SkipHotkey) GetHotkeys()
        {
            return ((Keys)Settings.Default.CompleteHotkey, (Keys)Settings.Default.SkipHotkey);
        }

        // ==========MY NOTES==============
        // Enhanced save method that also creates backup
        [SupportedOSPlatform("windows6.1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public void SaveHotkeysEnabled(bool enabled)
        {
            Settings.Default.HotkeysEnabled = enabled;
            Settings.Default.Save();
            BackupSettings(); // Auto-backup after saving
        }

        [SupportedOSPlatform("windows6.1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public bool GetHotkeysEnabled()
        {
            return Settings.Default.HotkeysEnabled;
        }

        public void SaveHotkeySettings(Keys undoHotkey, bool globalHotkeys, bool advancedHotkeys)
        {
            Settings.Default.UndoHotkey = (int)undoHotkey;
            Settings.Default.GlobalHotkeys = globalHotkeys;
            Settings.Default.AdvancedHotkeys = advancedHotkeys;
            Settings.Default.Save();
            BackupSettings();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public (Keys CompleteHotkey, Keys SkipHotkey, Keys UndoHotkey, bool GlobalHotkeys, bool AdvancedHotkeys) GetAllHotkeySettings()
        {
            return (
                (Keys)Settings.Default.CompleteHotkey,
                (Keys)Settings.Default.SkipHotkey,
                (Keys)Settings.Default.UndoHotkey,
                Settings.Default.GlobalHotkeys,
                Settings.Default.AdvancedHotkeys
            );
        }
        #endregion

        #region shortcut settings
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public (Keys Load, Keys Save, Keys LoadProgress, Keys ResetProgress,
            Keys Refresh, Keys Help, Keys FilterClear, Keys Connect, Keys GameStats,
            Keys RouteStats, Keys LayoutUp, Keys LayoutDown, Keys BackupFolder,
            Keys BackupNow, Keys Restore, Keys SetFolder, Keys AutoTog, Keys TopTog,
            Keys AdvTog, Keys GlobalTog, Keys SortingUp, Keys SortingDown,
            Keys GameDirect) GetShortcuts()
        {
            return (
                (Keys)Settings.Default.ShortLoad,
                (Keys)Settings.Default.ShortSave,
                (Keys)Settings.Default.ShortLoadP,
                (Keys)Settings.Default.ShortResetP,
                (Keys)Settings.Default.ShortRefresh,
                (Keys)Settings.Default.ShortHelp,
                (Keys)Settings.Default.ShortFilterC,
                (Keys)Settings.Default.ShortConnect,
                (Keys)Settings.Default.ShortGameStats,
                (Keys)Settings.Default.ShortRouteStats,
                (Keys)Settings.Default.ShortLayoutUp,
                (Keys)Settings.Default.ShortLayoutDown,
                (Keys)Settings.Default.ShortBackFold,
                (Keys)Settings.Default.ShortBackNow,
                (Keys)Settings.Default.ShortRestore,
                (Keys)Settings.Default.ShortSetFold,
                (Keys)Settings.Default.AutoTog,
                (Keys)Settings.Default.TopTog,
                (Keys)Settings.Default.AdvTog,
                (Keys)Settings.Default.GlobalTog,
                (Keys)Settings.Default.SortingUp,
                (Keys)Settings.Default.SortingDown,
                (Keys)Settings.Default.GameDirect
            );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public void SaveShortcuts(Keys load, Keys save, Keys loadProgress,
            Keys resetProgress, Keys refresh, Keys help, Keys filterClear, Keys connect,
            Keys gameStats, Keys routeStats, Keys layoutUp, Keys layoutDown,
            Keys backupFolder, Keys backupNow, Keys restore, Keys setFolder,
            Keys autoTog, Keys topTog, Keys advTog, Keys globalTog,
            Keys sortingUp, Keys sortingDown, Keys gameDirect)
        {
            Settings.Default.ShortLoad = (int)load;
            Settings.Default.ShortSave = (int)save;
            Settings.Default.ShortLoadP = (int)loadProgress;
            Settings.Default.ShortResetP = (int)resetProgress;
            Settings.Default.ShortRefresh = (int)refresh;
            Settings.Default.ShortHelp = (int)help;
            Settings.Default.ShortFilterC = (int)filterClear;
            Settings.Default.ShortConnect = (int)connect;
            Settings.Default.ShortGameStats = (int)gameStats;
            Settings.Default.ShortRouteStats = (int)routeStats;
            Settings.Default.ShortLayoutUp = (int)layoutUp;
            Settings.Default.ShortLayoutDown = (int)layoutDown;
            Settings.Default.ShortBackFold = (int)backupFolder;
            Settings.Default.ShortBackNow = (int)backupNow;
            Settings.Default.ShortRestore = (int)restore;
            Settings.Default.ShortSetFold = (int)setFolder;
            Settings.Default.AutoTog = (int)autoTog;
            Settings.Default.TopTog = (int)topTog;
            Settings.Default.AdvTog = (int)advTog;
            Settings.Default.GlobalTog = (int)globalTog;
            Settings.Default.SortingUp = (int)sortingUp;
            Settings.Default.SortingDown = (int)sortingDown;
            Settings.Default.GameDirect = (int)gameDirect;
            Settings.Default.Save();
            BackupSettings();
        }
        #endregion

        #region Misc settings
        public List<string> GetGamesWithDirectoriesSet()
        {
            var games = new List<string>();

            // Check all supported games from our centralized list
            foreach (var game in SupportedGames.GameList)
            {
                if (!string.IsNullOrEmpty(GetGameDirectory(game.Value.DisplayName)))
                {
                    games.Add(game.Value.DisplayName);
                }
            }

            return games;
        }

        public void SaveLayoutMode(LayoutSettingsForm.LayoutMode layoutMode)
        {
            Settings.Default.LayoutMode = layoutMode.ToString();
            Settings.Default.Save();
            BackupSettings();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public LayoutSettingsForm.LayoutMode GetLayoutMode()
        {
            return Enum.TryParse<LayoutSettingsForm.LayoutMode>(Settings.Default.LayoutMode, out var mode)
                ? mode
                : LayoutSettingsForm.LayoutMode.Normal;
        }

        public void SaveLayoutSettings(LayoutSettingsForm.LayoutMode layoutMode)
        {
            Settings.Default.LayoutMode = layoutMode.ToString();
            Settings.Default.Save();
            BackupSettings();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public LayoutSettingsForm.LayoutMode GetLayoutSettings()
        {
            return Enum.TryParse<LayoutSettingsForm.LayoutMode>(Settings.Default.LayoutMode, out var mode)
                ? mode
                : LayoutSettingsForm.LayoutMode.Normal;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public void OpenSettingsFolder()
        {
            try
            {
                // Get the folder where the app's config files are stored
                string settingsFolder = Path.GetDirectoryName(Application.UserAppDataPath) ??
                                       Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RouteTracker");

                if (Directory.Exists(settingsFolder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", settingsFolder);
                }
                else
                {
                    // Fallback to app directory if user settings folder doesn't exist
                    string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                    System.Diagnostics.Process.Start("explorer.exe", appFolder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open settings folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SaveSortingMode(SortingOptionsForm.SortingMode sortingMode)
        {
            Settings.Default.SortingMode = (int)sortingMode;
            Settings.Default.Save();
            BackupSettings();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public SortingOptionsForm.SortingMode GetSortingMode()
        {
            return (SortingOptionsForm.SortingMode)Settings.Default.SortingMode;
        }
        #endregion
    }
}
