using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // This is the main connection manager that bridges the UI and game memory
    // It handles finding, starting, and connecting to games
    // Takes care of the entire connection lifecycle from launch to cleanup
    public class GameConnectionManager
    {
        private IntPtr processHandle;
        private IntPtr baseAddress;
        private string currentProcess = string.Empty;
        private GameStatsBase? gameStats;

        private const int PROCESS_WM_READ = 0x0010;

        // Forward the GameStatsEventArgs event from GameStats
        public event EventHandler<GameStatsEventArgs>? StatsUpdated;

        public bool IsConnected => processHandle != IntPtr.Zero && baseAddress != IntPtr.Zero;
        public GameStatsBase? GameStats => gameStats;
        public IntPtr ProcessHandle => processHandle;
        public IntPtr BaseAddress => baseAddress;

        #region Process Management
        // Finds the game in running processes and gets access to its memory
        // Sets up everything needed to read values from the game
        [SupportedOSPlatform("windows6.1")]
        public void Connect()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(currentProcess.Replace(".exe", ""));
                if (processes.Length > 0)
                {
                    Process process = processes[0];
                    processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
                    
                    // Add null check for MainModule
                    if (process.MainModule != null)
                    {
                        baseAddress = process.MainModule.BaseAddress;
                        Debug.WriteLine($"Connected to process {currentProcess}");
                        Debug.WriteLine($"Base address: {baseAddress:X}");
                    }
                    else
                    {
                        processHandle = IntPtr.Zero;
                        baseAddress = IntPtr.Zero;
                        Debug.WriteLine($"Cannot access MainModule for process {currentProcess}. This may be due to insufficient permissions.");
                    }
                }
                else
                {
                    processHandle = IntPtr.Zero;
                    baseAddress = IntPtr.Zero;
                    Debug.WriteLine($"Process {currentProcess} not found.");
                }
            }
            catch (Exception ex)
            {
                processHandle = IntPtr.Zero;
                baseAddress = IntPtr.Zero;
                Debug.WriteLine($"Error in Connect: {ex.Message}");
            }
        }

        // ==========MY NOTES==============
        // Just checks if the game is already running or not
        [SupportedOSPlatform("windows6.1")]
        public static bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName.Replace(".exe", "")).Length > 0;
        }

        public void SetCurrentProcess(string processName)
        {
            currentProcess = processName;
        }
        #endregion

        #region Game Launching
        // ==========MY NOTES==============
        // Tries to start the game and shows error messages if it can't
        [SupportedOSPlatform("windows6.1")]
        public void StartGame(string processName)
        {
            try
            {
                string gameDirectory = string.Empty;
                if (currentProcess == "AC4BFSP.exe")
                {
                    gameDirectory = Settings.Default?.AC4Directory ?? string.Empty;
                }
                else if (currentProcess == "GoW.exe")
                {
                    gameDirectory = Settings.Default?.Gow2018Directory ?? string.Empty;
                }

                if (string.IsNullOrEmpty(gameDirectory))
                {
                    MessageBox.Show("Please select the game's directory.");
                    return;
                }

                string gamePath = System.IO.Path.Combine(gameDirectory, processName);
                if (!System.IO.File.Exists(gamePath))
                {
                    MessageBox.Show($"The game executable was not found in the selected directory: {gamePath}");
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = gamePath,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting the game: {ex.Message}");
            }
        }

        // ==========MY NOTES==============
        // Waits up to 10 seconds for the game to start
        // If it takes too long, gives you options like retry, wait longer, etc.
        [SupportedOSPlatform("windows6.1")]
        public async Task WaitForGameToStartAsync(int recursionCount = 0)
        {
            // Add recursion limit to prevent stack overflow
            if (recursionCount > 3)
            {
                MessageBox.Show("Maximum retry count reached. Please start the game manually.");
                return;
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();

            while (stopwatch.Elapsed.TotalSeconds < 10)
            {
                if (IsProcessRunning(currentProcess))
                {
                    return;
                }

                await Task.Delay(1000);
            }

            stopwatch.Stop();

            using CustomMessageBox customMessageBox = new("The game did not start within 10 seconds. Would you like to try again, wait another 10 seconds, manually start the game, or cancel?");
            if (customMessageBox.ShowDialog() == DialogResult.OK)
            {
                switch (customMessageBox.Result)
                {
                    case CustomMessageBox.CustomDialogResult.TryAgain:
                        StartGame(currentProcess);
                        await WaitForGameToStartAsync(recursionCount + 1);
                        break;
                    case CustomMessageBox.CustomDialogResult.Wait:
                        await WaitForGameToStartAsync();
                        break;
                    case CustomMessageBox.CustomDialogResult.Manually:
                        // Do nothing, user chose to manually start the game
                        break;
                    case CustomMessageBox.CustomDialogResult.Cancel:
                        // Do nothing, user chose to cancel
                        break;
                }
            }
        }

        [SupportedOSPlatform("windows6.1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1822:",
        Justification = "it breaks shit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079:R",
        Justification = "it breaks shit")]
        public string DetectRunningGame()
        {
            // Check for running processes that match our supported games
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    // Check if this process name matches any of our supported games
                    foreach (var game in SupportedGames.GameList)
                    {
                        if (process.ProcessName.Contains(game.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            return game.Value.DisplayName; // Return the friendly game name
                        }
                    }
                }
                catch
                {
                    // Skip any processes we can't access
                    continue;
                }
            }

            return string.Empty; // No matching game found
        }
        #endregion

        #region Game Statistics
        // ==========MY NOTES==============
        // Creates the right stats reader for the specific game we're connected to
        // Sets up event handling so stats updates get passed to the UI
        // Starts the automatic refresh system to keep stats current
        public void InitializeGameStats(string gameName)
        {
            if (processHandle != IntPtr.Zero && baseAddress != IntPtr.Zero)
            {
                // Create the correct stats object based on game
                gameStats = gameName switch
                {
                    "Assassin's Creed 4" => new AC4GameStats(processHandle, baseAddress),
                    "God of War 2018" => new GoW2018GameStats(processHandle, baseAddress),
                    _ => throw new NotSupportedException($"Game {gameName} is not supported")
                };

                gameStats.StatsUpdated += OnGameStatsUpdated;
                gameStats.StartUpdating();
            }
        }

        // ==========MY NOTES==============
        // Passes stats updates from the game reader to whoever's listening
        // Simple relay that forwards events without changing them
        // Uses ?. to avoid null reference exceptions if there are no listeners
        private void OnGameStatsUpdated(object? sender, GameStatsEventArgs e)
        {
            StatsUpdated?.Invoke(this, e);
        }

        // ==========MY NOTES==============
        // Properly disconnects and shuts down the stats system
        // Makes sure we don't leave any timers or events running
        // Important for clean shutdown and reconnection
        public void CleanupGameStats()
        {
            if (gameStats != null)
            {
                gameStats.StopUpdating();
                if (gameStats is IDisposable disposable)
                    disposable.Dispose();
                gameStats = null;
            }
        }
        #endregion

        #region Connection Orchestration
        // ==========MY NOTES==============
        // One-stop method for connecting to a game - handles all the connection steps
        // Returns success/failure so the UI can update accordingly
        [SupportedOSPlatform("windows6.1")]
        public async Task<bool> ConnectToGameAsync(string gameName, bool autoStart = false)
        {
            try
            {
                LoggingSystem.LogInfo($"Attempting to connect to game: {gameName} (AutoStart: {autoStart})");

                // Get exe name from display name using SupportedGames dictionary
                string? exeName = SupportedGames.GameList
                    .FirstOrDefault(kvp => kvp.Value.DisplayName == gameName).Key;

                if (string.IsNullOrEmpty(exeName))
                {
                    LoggingSystem.LogError($"Invalid game selection: {gameName}");
                    return false; // Invalid game selection
                }

                // Set the correct process name based on game selection
                currentProcess = $"{exeName}.exe";

                // Auto-start the game if requested and not already running
                if (autoStart)
                {
                    if (gameName == "God of War 2018")
                    {
                        LoggingSystem.LogWarning("Auto-start not supported for God of War 2018");
                        return false; // Auto-start not supported for GOW
                    }

                    if (!IsProcessRunning(currentProcess))
                    {
                        LoggingSystem.LogInfo($"Starting game process: {currentProcess}");
                        StartGame(currentProcess);
                        await WaitForGameToStartAsync();
                    }
                    else
                    {
                        LoggingSystem.LogInfo($"Game already running: {currentProcess}");
                    }
                }

                // Attempt to connect to the game process
                Connect();

                // Initialize game stats if connection was successful
                if (processHandle != IntPtr.Zero)
                {
                    InitializeGameStats(gameName);
                    LoggingSystem.LogInfo($"Successfully connected to {gameName}");
                    return true;
                }
                else
                {
                    LoggingSystem.LogWarning($"Failed to get process handle for {gameName}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError($"Error connecting to game {gameName}", ex);
                return false;
            }
        }
        #endregion

        #region external API Imports
        [System.Diagnostics.CodeAnalysis.SuppressMessage("style", "SYSLIB1054:",
        Justification = "because i said so")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079:",
        Justification = "because i said so")]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        #endregion
    }
}
