using System.Runtime.Versioning;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    public static class SettingsLifecycleManager
    {
        // ==========MY NOTES==============
        // Gets all the saved settings when the app starts up
        [SupportedOSPlatform("windows6.1")]
        public static void LoadSettings(MainForm mainForm, SettingsManager settingsManager, TextBox gameDirectoryTextBox)
        {
            settingsManager.LoadSettings(gameDirectoryTextBox);
            mainForm.TopMost = settingsManager.GetAlwaysOnTop();

            // Load layout settings - just the layout mode
            var currentLayoutMode = settingsManager.GetLayoutMode();

            // Apply layout settings after UI is fully initialized
            LayoutManager.ApplyLayoutSettings(mainForm, currentLayoutMode);
        }

        // ==========MY NOTES==============
        // Writes all current settings to disk so they're remembered next time
        [SupportedOSPlatform("windows6.1")]
        public static void SaveSettings(SettingsManager settingsManager, string gameDirectoryText, string autoStart)
        {
            settingsManager.SaveSettings(gameDirectoryText, autoStart);
        }

        // ==========MY NOTES==============
        // Handles app startup logic - auto-start and update checks
        [SupportedOSPlatform("windows6.1")]
        public static async Task HandleApplicationStartup(MainForm mainForm, SettingsManager settingsManager, GameConnectionManager gameConnectionManager)
        {
            // Auto-start logic
            string autoStartGame = Settings.Default.AutoStart;
            if (!string.IsNullOrEmpty(autoStartGame) && !string.IsNullOrEmpty(settingsManager.GetGameDirectory(autoStartGame)))
            {
                bool connected = await gameConnectionManager.ConnectToGameAsync(autoStartGame, true);
                if (connected)
                {
                    string routeFilePath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Routes",
                        $"{autoStartGame} 100 % Route - Main Route.tsv");
                    mainForm.SetRouteManager(new RouteManager(routeFilePath, gameConnectionManager));
                    RouteHelpers.LoadRouteData(mainForm, mainForm.GetRouteManager(), mainForm.routeGrid, settingsManager);
                }
            }
            await UpdateManager.CheckForUpdatesAsync();
        }
    }
}