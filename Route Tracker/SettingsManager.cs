using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Manages application settings and game directories
    // Handles loading, saving, and providing access to configuration values
    // ==========MY NOTES==============
    // This centralizes all settings-related code in one place
    // Makes it easier to access and update settings from anywhere
    public class SettingsManager
    {
        private bool isLoadingSettings = false;
        public bool IsLoadingSettings => isLoadingSettings;

        // ==========MY NOTES==============
        // Settings backup location that survives version updates and uninstalls
        private static readonly string BackupFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RouteTracker",
            "Settings"
        );
        private static readonly string BackupFilePath = Path.Combine(BackupFolder, "settings_backup.json");

        // ==========MY NOTES==============
        // Cached JsonSerializerOptions to avoid recreating on every backup
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        // ==========MY NOTES==============
        // Data model for all settings we want to backup
        public class SettingsBackup
        {
            public string GameDirectory { get; set; } = string.Empty;
            public string AutoStart { get; set; } = string.Empty;
            public string AC4Directory { get; set; } = string.Empty;
            public string Gow2018Directory { get; set; } = string.Empty;
            public int CompleteHotkey { get; set; } = 0;
            public int SkipHotkey { get; set; } = 0;
            public bool HotkeysEnabled { get; set; } = false;
            public bool AlwaysOnTop { get; set; } = false;
            public bool CheckForUpdateOnStartup { get; set; } = true;
            public bool DevMode { get; set; } = false;
            public DateTime BackupTimestamp { get; set; } = DateTime.Now;
            public string AppVersion { get; set; } = string.Empty;
            public string LayoutMode { get; set; } = "Normal";
            public bool TransparentBackground { get; set; } = false;
        }

        #region Backup/Restore Settings
        // ==========MY NOTES==============
        // Creates a backup of all current settings to AppData
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public void BackupSettings()
        {
            try
            {
                // Ensure backup directory exists
                Directory.CreateDirectory(BackupFolder);

                // Create backup object with current settings
                var backup = new SettingsBackup
                {
                    GameDirectory = Settings.Default.GameDirectory,
                    AutoStart = Settings.Default.AutoStart,
                    AC4Directory = Settings.Default.AC4Directory,
                    Gow2018Directory = Settings.Default.Gow2018Directory,
                    CompleteHotkey = Settings.Default.CompleteHotkey,
                    SkipHotkey = Settings.Default.SkipHotkey,
                    HotkeysEnabled = Settings.Default.HotkeysEnabled,
                    AlwaysOnTop = Settings.Default.AlwaysOnTop,
                    CheckForUpdateOnStartup = Settings.Default.CheckForUpdateOnStartup,
                    DevMode = Settings.Default.DevMode,
                    LayoutMode = Settings.Default.LayoutMode,           // Add this
                    TransparentBackground = Settings.Default.TransparentBackground, // Add this
                    BackupTimestamp = DateTime.Now,
                    AppVersion = AppTheme.Version
                };

                // Use cached JsonSerializerOptions
                string json = JsonSerializer.Serialize(backup, JsonOptions);
                File.WriteAllText(BackupFilePath, json);

                System.Diagnostics.Debug.WriteLine($"Settings backed up to: {BackupFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to backup settings: {ex.Message}");
            }
        }

        // ==========MY NOTES==============
        // Restores settings from backup if it exists
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public bool RestoreFromBackup()
        {
            try
            {
                if (!File.Exists(BackupFilePath))
                    return false;

                string json = File.ReadAllText(BackupFilePath);
                var backup = JsonSerializer.Deserialize<SettingsBackup>(json);

                if (backup == null)
                    return false;

                // Restore all settings
                Settings.Default.GameDirectory = backup.GameDirectory;
                Settings.Default.AutoStart = backup.AutoStart;
                Settings.Default.AC4Directory = backup.AC4Directory;
                Settings.Default.Gow2018Directory = backup.Gow2018Directory;
                Settings.Default.CompleteHotkey = backup.CompleteHotkey;
                Settings.Default.SkipHotkey = backup.SkipHotkey;
                Settings.Default.HotkeysEnabled = backup.HotkeysEnabled;
                Settings.Default.AlwaysOnTop = backup.AlwaysOnTop;
                Settings.Default.CheckForUpdateOnStartup = backup.CheckForUpdateOnStartup;
                Settings.Default.DevMode = backup.DevMode;
                Settings.Default.LayoutMode = backup.LayoutMode;               // Add this
                Settings.Default.TransparentBackground = backup.TransparentBackground; // Add this

                Settings.Default.Save();

                System.Diagnostics.Debug.WriteLine($"Settings restored from backup dated: {backup.BackupTimestamp}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to restore settings: {ex.Message}");
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
        // ==========FORMAL COMMENT=========
        // Loads user settings into UI controls
        // Uses a flag to prevent change events during loading
        // ==========MY NOTES==============
        // Fills UI controls with saved settings when app starts
        [SupportedOSPlatform("windows6.1")]
        public void LoadSettings(TextBox gameDirectoryTextBox)
        {
            isLoadingSettings = true;
            gameDirectoryTextBox.Text = Settings.Default.GameDirectory;
            isLoadingSettings = false;
        }

        // ==========MY NOTES==============
        // Enhanced save method that also creates backup
        [SupportedOSPlatform("windows6.1")]
        public void SaveAlwaysOnTop(bool alwaysOnTop)
        {
            Settings.Default.AlwaysOnTop = alwaysOnTop;
            Settings.Default.Save();
            BackupSettings(); // Auto-backup after saving
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
            if (selectedGame == "Assassin's Creed 4")
            {
                Settings.Default.AC4Directory = directory;
            }
            else if (selectedGame == "God of War 2018")
            {
                Settings.Default.Gow2018Directory = directory;
            }
            Settings.Default.Save();
            BackupSettings(); // Auto-backup after saving
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        [SupportedOSPlatform("windows6.1")]
        public string GetGameDirectory(string game)
        {
            if (game == "Assassin's Creed 4")
                return Settings.Default.AC4Directory;
            else if (game == "God of War 2018")
                return Settings.Default.Gow2018Directory;
            return string.Empty;
        }
        #endregion

        #region Hotkey Settings
        // ==========MY NOTES==============
        // Enhanced save method that also creates backup
        [SupportedOSPlatform("windows6.1")]
        public void SaveHotkeys(Keys completeHotkey, Keys skipHotkey)
        {
            Settings.Default.CompleteHotkey = (int)completeHotkey;
            Settings.Default.SkipHotkey = (int)skipHotkey;
            Settings.Default.Save();
            BackupSettings(); // Auto-backup after saving
        }

        [SupportedOSPlatform("windows6.1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
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
        #endregion

        #region misc settings
        public List<string> GetGamesWithDirectoriesSet()
        {
            var games = new List<string>();
            var supportedGames = new[] { "Assassin's Creed 4", "God of War 2018" };
            foreach (var game in supportedGames)
            {
                if (!string.IsNullOrEmpty(GetGameDirectory(game)))
                    games.Add(game);
            }
            return games;
        }

        public void SaveLayoutMode(LayoutSettingsForm.LayoutMode layoutMode)
        {
            Settings.Default.LayoutMode = layoutMode.ToString();
            Settings.Default.Save();
            BackupSettings();
        }

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

        public LayoutSettingsForm.LayoutMode GetLayoutSettings()
        {
            return Enum.TryParse<LayoutSettingsForm.LayoutMode>(Settings.Default.LayoutMode, out var mode)
                ? mode
                : LayoutSettingsForm.LayoutMode.Normal;
        }
        #endregion
    }
}
