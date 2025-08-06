using System.Runtime.Versioning;
using System.Threading.Tasks;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    public static class SettingsLifecycleManager
    {
        // ==========MY NOTES==============
        // Background auto-connection manager - static so it persists for the app lifetime
        private static AutoConnectionManager? autoConnectionManager;

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
            else
            {
                // if not auto-starting try connecting once on startup
                await RouteHelpers.TryAutoConnectOnStartup(mainForm, gameConnectionManager, settingsManager);
            }

            // START BACKGROUND AUTO-CONNECTION SYSTEM
            // This runs on a separate thread and won't slow down the main app
            StartBackgroundAutoConnection(mainForm, gameConnectionManager, settingsManager);

            // Check for updates separately (doesn't need delay)
            await UpdateManager.CheckForUpdatesAsync();
        }

        // ==========MY NOTES==============
        // NEW: Starts the background auto-connection system
        // Runs on a separate thread every 10 seconds looking for supported games
        [SupportedOSPlatform("windows6.1")]
        private static void StartBackgroundAutoConnection(MainForm mainForm, GameConnectionManager gameConnectionManager, SettingsManager settingsManager)
        {
            try
            {
                // Create and start the background auto-connection manager
                autoConnectionManager = new AutoConnectionManager(mainForm, gameConnectionManager, settingsManager);
                autoConnectionManager.StartAutoConnection();

                LoggingSystem.LogInfo("Background auto-connection system started successfully");
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError($"Failed to start background auto-connection system: {ex.Message}", ex);
            }
        }

        // ==========MY NOTES==============
        // NEW: Stops the background auto-connection system (called on app shutdown)
        public static void StopBackgroundAutoConnection()
        {
            try
            {
                autoConnectionManager?.Dispose();
                autoConnectionManager = null;
                LoggingSystem.LogInfo("Background auto-connection system stopped");
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError($"Error stopping background auto-connection system: {ex.Message}", ex);
            }
        }

        // ==========MY NOTES==============
        // NEW: Gets the status of the background auto-connection system
        public static bool IsBackgroundAutoConnectionRunning => autoConnectionManager?.IsRunning ?? false;
    }
}