using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // Background auto-connection system that runs on a separate thread
    // Tries to connect to any supported game every 10 seconds without slowing down the main app
    public class AutoConnectionManager : IDisposable
    {
        private readonly MainForm mainForm;
        private readonly GameConnectionManager gameConnectionManager;
        private readonly SettingsManager settingsManager;
        private AppTimer? backgroundTimer;
        private bool isRunning = false;
        private bool disposed = false;

        // ==========MY NOTES==============
        // List of all supported games - keep this in sync with GameConnectionManager
        private readonly string[] supportedGames = ["Assassin's Creed 4", "God of War 2018"];
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290",
        Justification = "NO")]
        public AutoConnectionManager(MainForm mainForm, GameConnectionManager gameConnectionManager, SettingsManager settingsManager)
        {
            this.mainForm = mainForm;
            this.gameConnectionManager = gameConnectionManager;
            this.settingsManager = settingsManager;
        }

        // ==========MY NOTES==============
        // Starts the background auto-connection system
        // Runs on a separate thread every 10 seconds
        public void StartAutoConnection()
        {
            if (isRunning || disposed)
                return;

            LoggingSystem.LogInfo("Starting background auto-connection system (10-second intervals)");

            // Create background timer that runs on separate thread
            backgroundTimer = AppTimer.CreateBackgroundTimer(10000, BackgroundConnectionAttempt);
            backgroundTimer.Start();
            isRunning = true;
        }

        // ==========MY NOTES==============
        // Stops the background auto-connection system
        public void StopAutoConnection()
        {
            if (!isRunning || disposed)
                return;

            LoggingSystem.LogInfo("Stopping background auto-connection system");

            backgroundTimer?.Stop();
            backgroundTimer?.Dispose();
            backgroundTimer = null;
            isRunning = false;
        }

        // ==========MY NOTES==============
        // Background callback that runs every 10 seconds on a separate thread
        // ENHANCED: Now properly detects connection loss and attempts reconnection
        private async void BackgroundConnectionAttempt(object? state)
        {
            try
            {
                // Check if we think we're connected but the process is actually dead
                if (gameConnectionManager.IsConnected)
                {
                    // Verify the connection is actually still valid
                    if (IsConnectionActuallyAlive())
                    {
                        return; // Still connected and process is alive, skip
                    }
                    else
                    {
                        LoggingSystem.LogInfo("Background auto-connect: Detected lost connection, attempting to reconnect");
                        // Clear the dead connection
                        gameConnectionManager.CleanupGameStats();
                    }
                }

                // Use the same logic as the startup auto-connect but in background
                string detectedGame = gameConnectionManager.DetectRunningGame();

                if (string.IsNullOrEmpty(detectedGame))
                    return; // No games detected, skip quietly

                // Check for multiple games running (same logic as startup)
                var supportedGameProcesses = new Dictionary<string, string>
                {
                    { "AC4BFSP", "Assassin's Creed 4" },
                    { "GoW", "God of War 2018" }
                };

                int runningCount = 0;
                foreach (var game in supportedGameProcesses)
                {
                    if (GameConnectionManager.IsProcessRunning(game.Key + ".exe"))
                    {
                        runningCount++;
                    }
                }

                // Skip if multiple games running
                if (runningCount > 1)
                {
                    LoggingSystem.LogInfo($"Background auto-connect: Multiple games running ({runningCount}) - skipping");
                    return;
                }

                // Check if game directory is set
                string gameDirectory = settingsManager.GetGameDirectory(detectedGame);
                if (string.IsNullOrEmpty(gameDirectory))
                {
                    LoggingSystem.LogInfo($"Background auto-connect: No directory set for {detectedGame} - skipping");
                    return;
                }

                LoggingSystem.LogInfo($"Background auto-connect: Attempting to connect to {detectedGame}");

                // Try to connect (this runs on background thread)
                bool connected = await gameConnectionManager.ConnectToGameAsync(detectedGame, false);

                if (connected)
                {
                    LoggingSystem.LogInfo($"Background auto-connect: Successfully connected to {detectedGame}");

                    // Update UI on main thread
                    mainForm.Invoke(() =>
                    {
                        // Update game dropdown if it exists
                        if (mainForm.MainMenuStrip?.Items.OfType<ToolStripComboBox>().FirstOrDefault() is ToolStripComboBox gameDropdown)
                        {
                            gameDropdown.SelectedItem = detectedGame;
                        }

                        // Optional: Show a subtle notification (you can remove this if you find it annoying)
                        LoggingSystem.LogInfo($"Auto-connected to {detectedGame} in background");
                    });
                }
                else
                {
                    LoggingSystem.LogInfo($"Background auto-connect: Failed to connect to {detectedGame}");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError($"Error in background auto-connection: {ex.Message}", ex);
            }
        }

        // ==========MY NOTES==============
        // NEW: Checks if the current connection is actually still alive
        // Verifies that the connected process is still running
        private bool IsConnectionActuallyAlive()
        {
            try
            {
                if (!gameConnectionManager.IsConnected)
                    return false;

                // Try to detect what game should be running based on current connection
                string detectedGame = gameConnectionManager.DetectRunningGame();

                // If we can't detect any supported game, the connection is dead
                if (string.IsNullOrEmpty(detectedGame))
                {
                    LoggingSystem.LogInfo("Background auto-connect: No supported games detected, connection appears dead");
                    return false;
                }

                // Additional check: verify the process handle is still valid by trying to use it
                // This is a lightweight check that doesn't require full reconnection
                try
                {
                    var stats = gameConnectionManager.GameStats?.GetStatsAsDictionary();
                    // If this throws an exception, the connection is dead
                    return stats != null;
                }
                catch
                {
                    LoggingSystem.LogInfo("Background auto-connect: Stats reading failed, connection appears dead");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError($"Error checking connection health: {ex.Message}", ex);
                return false; // Assume dead on error
            }
        }

        // ==========MY NOTES==============
        // Checks if the auto-connection system is currently running
        public bool IsRunning => isRunning && !disposed;

        // ==========MY NOTES==============
        // Proper cleanup when disposing - follows CA1816 disposal pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ==========MY NOTES==============
        // Protected disposal method for proper disposal pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    StopAutoConnection();
                }

                // Clean up any unmanaged resources here (none in this case)
                disposed = true;
            }
        }
    }
}