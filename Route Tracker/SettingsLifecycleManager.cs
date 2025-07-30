using System.Runtime.Versioning;
using System.Threading.Tasks;
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
        // FIXED: Staggered startup with delays to prevent UI freezing
        // Each operation happens with a 2.5 second delay to allow UI to render properly
        [SupportedOSPlatform("windows6.1")]
        public static async Task HandleApplicationStartup(MainForm mainForm, SettingsManager settingsManager, GameConnectionManager gameConnectionManager)
        {
            // Check if we need to do auto-start operations
            string autoStartGame = Settings.Default.AutoStart;
            bool hasAutoStart = !string.IsNullOrEmpty(autoStartGame) && !string.IsNullOrEmpty(settingsManager.GetGameDirectory(autoStartGame));

            if (hasAutoStart)
            {
                // Step 1: Wait 2.5 seconds for main form to fully load and render
                LoggingSystem.LogInfo("Starting staggered startup operations...");

                // Step 2: Connect to game
                bool connected = false;
                try
                {
                    connected = await gameConnectionManager.ConnectToGameAsync(autoStartGame, true);
                    LoggingSystem.LogInfo($"Game connection result: {connected}");
                }
                catch (Exception ex)
                {
                    LoggingSystem.LogError($"Error connecting to game: {ex.Message}", ex);
                }

                if (connected)
                {
                    LoggingSystem.LogInfo("Creating route manager...");

                    // Create route manager on UI thread
                    string routeFilePath = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Routes",
                        "AC4 100 % Route - Main Route.tsv");

                    var routeManager = new RouteManager(routeFilePath, gameConnectionManager);
                    mainForm.SetRouteManager(routeManager);

                    // Step 4: Wait another 2.5 seconds before loading route data
                    await Task.Delay(500);
                    LoggingSystem.LogInfo("Loading route data...");

                    // Load route data on UI thread
                    mainForm.Invoke(() =>
                    {
                        RouteHelpers.LoadRouteDataCore(mainForm, routeManager, mainForm.routeGrid, settingsManager);
                    });
                }
            }

            // Check for updates separately (doesn't need delay)
            await UpdateManager.CheckForUpdatesAsync();
        }
    }
}