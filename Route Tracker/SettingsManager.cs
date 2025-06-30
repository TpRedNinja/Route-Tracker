using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Versioning;
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

        // Add this method to save the Always On Top setting
        [SupportedOSPlatform("windows6.1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public void SaveAlwaysOnTop(bool alwaysOnTop)
        {
            Settings.Default.AlwaysOnTop = alwaysOnTop;
            Settings.Default.Save();
        }

        // Add this method to get the Always On Top setting
        [SupportedOSPlatform("windows6.1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public bool GetAlwaysOnTop()
        {
            return Settings.Default.AlwaysOnTop;
        }

        // ==========FORMAL COMMENT=========
        // Saves settings from UI controls to configuration
        // ==========MY NOTES==============
        // Stores current settings so they're remembered next time
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
        }
        #endregion

        #region Game Directory Management
        // ==========FORMAL COMMENT=========
        // Saves game-specific directory path
        // Selects the appropriate setting based on game name
        // ==========MY NOTES==============
        // Stores the game executable location for a specific game
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
        }

        // ==========FORMAL COMMENT=========
        // Retrieves the directory for the specified game
        // ==========MY NOTES==============
        // Gets the saved path for a specific game
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
        // ==========FORMAL COMMENT=========
        // Saves hotkey configuration to application settings
        // Stores both complete and skip hotkey values
        // ==========MY NOTES==============
        // Remembers which keys to use for completing and skipping entries
        [SupportedOSPlatform("windows6.1")]
        public static void SaveHotkeys(Keys completeHotkey, Keys skipHotkey)
        {
            Settings.Default.CompleteHotkey = (int)completeHotkey;
            Settings.Default.SkipHotkey = (int)skipHotkey;
            Settings.Default.Save();
        }

        // ==========FORMAL COMMENT=========
        // Retrieves currently configured hotkeys from settings
        // Returns a tuple containing both hotkey values
        // ==========MY NOTES==============
        // Gets the saved hotkey settings as a pair of values
        [SupportedOSPlatform("windows6.1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public (Keys CompleteHotkey, Keys SkipHotkey) GetHotkeys()
        {
            return ((Keys)Settings.Default.CompleteHotkey, (Keys)Settings.Default.SkipHotkey);
        }

        // ==========FORMAL COMMENT=========
        // Saves the enabled state of hotkeys
        // Controls whether hotkeys are active in the application
        // ==========MY NOTES==============
        // Remembers whether hotkeys should work or be ignored
        [SupportedOSPlatform("windows6.1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks shit")]
        public void SaveHotkeysEnabled(bool enabled)
        {
            Settings.Default.HotkeysEnabled = enabled;
            Settings.Default.Save();
        }

        // ==========FORMAL COMMENT=========
        // Retrieves whether hotkeys are currently enabled
        // Determines if hotkey presses should be processed
        // ==========MY NOTES==============
        // Checks if hotkeys should be active or ignored
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
    }
}
