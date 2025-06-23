using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Manages connection to game processes and coordinates game statistics access
    // Provides high-level operations for game launching, connection, and memory reading
    // Handles initialization and cleanup of game-specific statistics objects
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

        // Forward the StatsUpdated event from GameStats
        public event EventHandler<StatsUpdatedEventArgs>? StatsUpdated;

        public bool IsConnected => processHandle != IntPtr.Zero && baseAddress != IntPtr.Zero;
        public GameStatsBase? GameStats => gameStats;
        public IntPtr ProcessHandle => processHandle;
        public IntPtr BaseAddress => baseAddress;

        #region Process Management
        // ==========FORMAL COMMENT=========
        // Establishes connection to the game process
        // Finds the specified process, gets handle and base address for memory access
        // ==========MY NOTES==============
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

        // ==========FORMAL COMMENT=========
        // Checks if a specific process is currently running
        // Returns true if the process is found, false otherwise
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
        // ==========FORMAL COMMENT=========
        // Launches the specified game executable from its directory
        // Handles cases where directory or executable file is not found
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
                    gameDirectory = Settings.Default?.GoW2018Directory ?? string.Empty;
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

        // ==========FORMAL COMMENT=========
        // Waits for the game process to start with a timeout
        // Shows custom dialog with options if game fails to start within time limit
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
        #endregion

        #region Game Statistics
        // ==========FORMAL COMMENT=========
        // Creates the appropriate game statistics handler based on the connected game
        // Initializes and configures the statistics object with process handle and address
        // Sets up event forwarding and begins automatic stats updates
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

        // ==========FORMAL COMMENT=========
        // Event handler that forwards statistics updates from the game-specific handler
        // Acts as a bridge between game stats objects and UI subscribers
        // Ensures proper event propagation with null-conditional operator
        // ==========MY NOTES==============
        // Passes stats updates from the game reader to whoever's listening
        // Simple relay that forwards events without changing them
        // Uses ?. to avoid null reference exceptions if there are no listeners
        private void OnGameStatsUpdated(object? sender, StatsUpdatedEventArgs e)
        {
            StatsUpdated?.Invoke(this, e);
        }

        // ==========FORMAL COMMENT=========
        // Performs cleanup operations for game statistics resources
        // Unsubscribes from events and stops background updates to prevent memory leaks
        // Called when disconnecting or closing the application
        // ==========MY NOTES==============
        // Properly disconnects and shuts down the stats system
        // Makes sure we don't leave any timers or events running
        // Important for clean shutdown and reconnection
        public void CleanupGameStats()
        {
            if (gameStats != null)
            {
                gameStats.StatsUpdated -= OnGameStatsUpdated;
                gameStats.StopUpdating();
            }
        }
        #endregion

        // ==========FORMAL COMMENT=========
        // High-level method that orchestrates the game connection process
        // Handles game selection, auto-starting if needed, and statistics initialization
        // ==========MY NOTES==============
        // One-stop method for connecting to a game - handles all the connection steps
        // Returns success/failure so the UI can update accordingly
        [SupportedOSPlatform("windows6.1")]
        public async Task<bool> ConnectToGameAsync(string gameName, bool autoStart = false)
        {
            // Set the correct process name based on game selection
            if (gameName == "Assassin's Creed 4")
                currentProcess = "AC4BFSP.exe";
            else if (gameName == "God of War 2018")
                currentProcess = "GoW.exe";
            else
                return false; // Invalid game selection

            // Auto-start the game if requested and not already running
            if (autoStart)
            {
                if (gameName == "God of War 2018")
                    return false; // Auto-start not supported for Syndicate

                if (!IsProcessRunning(currentProcess))
                {
                    StartGame(currentProcess);
                    await WaitForGameToStartAsync();
                }
            }

            // Attempt to connect to the game process
            Connect();

            // Initialize game stats if connection was successful
            if (processHandle != IntPtr.Zero)
            {
                InitializeGameStats(gameName);
                return true;
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "SYSLIB1054:",
        Justification = "Using ReadProcessMemory for direct memory reading in unsafe context")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "Required for unsafe memory operations")]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    }
}
