using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Event args class that carries game statistics data to event subscribers
    // Contains properties for all tracked game metrics to provide a complete snapshot
    // Used when notifying UI components about updated game statistics
    // ==========MY NOTES==============
    // This package contains all the stats in one neat bundle
    // When stats update, this gets sent to anyone listening for changes
    // Makes it easy to update the UI with all new values at once
    public class StatsUpdatedEventArgs(
    int percent,
    float percentFloat,
    int viewpoints,
    int myan,
    int treasure,
    int fragments,
    int assassin,
    int naval,
    int letters,
    int manuscripts,
    int music,
    int forts,
    int taverns,
    int totalChests) : EventArgs
    {
        public int Percent { get; } = percent;
        public float PercentFloat { get; } = percentFloat;
        public int Viewpoints { get; } = viewpoints;
        public int Myan { get; } = myan;
        public int Treasure { get; } = treasure;
        public int Fragments { get; } = fragments;
        public int Assassin { get; } = assassin;
        public int Naval { get; } = naval;
        public int Letters { get; } = letters;
        public int Manuscripts { get; } = manuscripts;
        public int Music { get; } = music;
        public int Forts { get; } = forts;
        public int Taverns { get; } = taverns;
        public int TotalChests { get; } = totalChests;
    }

    // ==========FORMAL COMMENT=========
    // Class that interfaces with game memory to read player statistics
    // Handles memory calculation, data extraction, and real-time updates for all tracked stats
    // Supports both individual collectibles and range-based collectible counting with auto-refreshing
    // ==========MY NOTES==============
    // This reads all the game stats from memory using addresses and offsets
    // It updates automatically every second to keep the UI in sync with the game
    // The core class that powers the entire tracking functionality
    public abstract unsafe class GameStatsBase : IGameStats
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0290: Use primary constructure",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "because i said so")]
        // Update constructor to accept architecture info
        public GameStatsBase(IntPtr processHandle, IntPtr baseAddress)
        {
            this.processHandle = processHandle;
            this.baseAddress = baseAddress;
        }

        // Change private fields to protected so derived classes can access them
        protected readonly IntPtr processHandle;
        protected readonly IntPtr baseAddress;

        // Timer-related fields for automatic stat updates
        private System.Threading.CancellationTokenSource? _updateCancellationTokenSource;
        private bool _isUpdating = false;
        private readonly int _updateIntervalMs = 1000; // Default update interval of 1 second
        private System.Threading.Timer? _updateTimer; // Timer that controls the periodic stat updates
        
        // for updating stats?
        private readonly Dictionary<string, (DateTime Timestamp, object Value)> _memoryCache = [];
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMilliseconds(500); // Half-second cache

        //for performance optimizations
        private readonly int _activeUpdateIntervalMs = 500;  // Fast updates during active gameplay
        private readonly int _idleUpdateIntervalMs = 1000;   // Slower updates when idle
        private DateTime _lastActivityTime = DateTime.Now;
        private bool _inActiveGameplay = true;
        private (int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure,
            int Fragments, int Assassin, int Naval, int Letters, int Manuscripts, int Music,
            int Forts, int Taverns, int TotalChests) _previousStats;


        // Event that fires whenever game statistics change
        // UI components can subscribe to this to get real-time updates
        public event EventHandler<StatsUpdatedEventArgs>? StatsUpdated;

        // ==========FORMAL COMMENT=========
        // Generic method to read values from game memory
        // Follows chains of pointers using the provided offsets
        // ==========MY NOTES==============
        // This is the low-level function that actually reads from memory
        // It follows the trail of addresses to find the specific value we want
        // Simplify Read to just use the original method
        protected unsafe T Read<T>(nint baseAddress, int[] offsets) where T : unmanaged
        {
            nint address = baseAddress;

            int pointer_size = 4; // Always use 32-bit
            foreach (int offset in offsets)
            {
                if (!ReadProcessMemory(processHandle, address, &address, pointer_size, out nint bytesReads) ||
                    bytesReads != pointer_size || address == IntPtr.Zero)
                {
                    //Debug.WriteLine($"Failed to read memory address at offset {offset:X}");
                    return default;
                }
                address += offset;
            }

            T value;
            int size = sizeof(T);
            if (!ReadProcessMemory(processHandle, address, &value, size, out nint bytesRead) ||
                bytesRead != size)
            {
                //Debug.WriteLine($"Failed to read value from memory at address {address:X}");
                return default;
            }
            return value;
        }

        #region Stats Retrieval
        // Make GetStats() abstract so each game must implement it
        public abstract (int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure,
            int Fragments, int Assassin, int Naval, int Letters, int Manuscripts, int Music,
            int Forts, int Taverns, int TotalChests) GetStats();

        // make GetSpecialActivityCounts() virtual so it can be overridden if needed
        public virtual (int StoryMissions, int TemplarHunts, int LegendaryShips, int TreasureMaps) GetSpecialActivityCounts()
        {
            // Default implementation for games that don't support these stats
            return (0, 0, 0, 0);
        }

        // Add the new interface method
        public virtual Dictionary<string, object> GetStatsAsDictionary()
        {
            (int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure, int Fragments, int Assassin, int Naval, int Letters, int Manuscripts, int Music, int Forts, int Taverns, int TotalChests) = GetStats();

            return new Dictionary<string, object>
            {
                ["Completion Percentage"] = Percent,
                ["Exact Percentage"] = Math.Round(PercentFloat, 2),
                ["Viewpoints Completed"] = Viewpoints,
                ["Myan Stones Collected"] = Myan,
                ["Buried Treasure Collected"] = Treasure,
                ["AnimusFragments Collected"] = Fragments,
                ["AssassinContracts Completed"] = Assassin,
                ["NavalContracts Completed"] = Naval,
                ["LetterBottles Collected"] = Letters,
                ["Manuscripts Collected"] = Manuscripts,
                ["Music Sheets Collected"] = Music,
                ["Forts Captured"] = Forts,
                ["Taverns unlocked"] = Taverns,
                ["Total Chests Collected"] = TotalChests
            };
        }
        #endregion

        #region Update System
        // ==========FORMAL COMMENT=========
        // Timer-based auto-update system for game statistics
        // Periodically reads memory and notifies listeners when values change
        // Includes proper resource management and thread safety
        // ==========MY NOTES==============
        // Automatically refreshes stats every second without manual button clicks
        // Uses a timer instead of async/await to work properly in unsafe contexts
        // Makes sure everything gets cleaned up properly when we disconnect
        public void StartUpdating()
        {
            //dont start if already updating
            if (_isUpdating) return;

            // create cancellation token
            _updateCancellationTokenSource = new System.Threading.CancellationTokenSource();

            // mark as updating
            _isUpdating = true;

            // create and start timer
            _updateTimer = new System.Threading.Timer(UpdateCallback, null, 0, _updateIntervalMs);
        }

        // ==========FORMAL COMMENT=========
        // Detects changes in game statistics compared to previous readings
        // Updates the stored previous state and returns whether changes were detected
        // Used to identify meaningful stat changes that indicate active gameplay
        // ==========MY NOTES==============
        // Compares new stats with previous ones to see if anything changed
        // Stores the current stats for next time
        // Returns true if at least one value changed
        private bool HasStatsChanged((int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure,
    int Fragments, int Assassin, int Naval, int Letters, int Manuscripts, int Music,
    int Forts, int Taverns, int TotalChests) currentStats)
        {
            bool changed = currentStats != _previousStats;
            _previousStats = currentStats;
            return changed;
        }

        // ==========FORMAL COMMENT=========
        // Adjusts update frequency based on recent player activity
        // Increases polling rate during active gameplay and reduces it during idle periods
        // Optimizes resource usage while maintaining responsiveness
        // ==========MY NOTES==============
        // Smart timer that speeds up when the player is active
        // Checks if anything has changed in the last 10 seconds
        // Saves resources when nothing is happening in the game
        private void UpdateAdaptiveTimer()
        {
            bool shouldBeActive = (DateTime.Now - _lastActivityTime).TotalSeconds < 10;

            if (shouldBeActive != _inActiveGameplay)
            {
                _inActiveGameplay = shouldBeActive;
                _updateTimer?.Change(0, _inActiveGameplay ? _activeUpdateIntervalMs : _idleUpdateIntervalMs);
            }
        }

        // ==========FORMAL COMMENT=========
        // Callback method invoked by the update timer at regular intervals
        // Reads current game statistics and raises the StatsUpdated event
        // Includes error handling to prevent timer disruption
        // ==========MY NOTES==============
        // This runs every second to check for new values
        // It gets fresh stats and tells listeners about the changes
        // The try-catch makes sure timer keeps working even if something fails
        private void UpdateCallback(object? state)
        {
            try
            {
                if (!_isUpdating || _updateTimer == null)
                    return;

                var stats = GetStats();

                // If we detect changes, reset activity timer
                if (HasStatsChanged(stats))
                {
                    _lastActivityTime = DateTime.Now;
                }

                // Update timer frequency based on activity
                UpdateAdaptiveTimer();

                StatsUpdated?.Invoke(this, new StatsUpdatedEventArgs(
                    stats.Percent, stats.PercentFloat, stats.Viewpoints, stats.Myan,
                    stats.Treasure, stats.Fragments, stats.Assassin, stats.Naval,
                    stats.Letters, stats.Manuscripts, stats.Music, stats.Forts,
                    stats.Taverns, stats.TotalChests));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in update timer callback: {ex.Message}");
            }
        }

        // ==========FORMAL COMMENT=========
        // Stops the automatic updates and releases timer resources
        // Safely handles cleanup of the timer to prevent memory leaks
        // ==========MY NOTES==============
        // Shuts down the auto-update system cleanly
        // Makes sure we don't leave timers running or resources locked
        // Called when disconnecting or closing the app
        public void StopUpdating()
        {
            if(!_isUpdating) return;

            _isUpdating = false;

            // stop and dispose timer
            if (_updateTimer != null)
            {
                _updateTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                _updateTimer.Dispose();
                _updateTimer = null;
            }
        }
        #endregion

        #region Memory Cache Management
        // ==========FORMAL COMMENT=========
        // Reads memory values with caching to reduce redundant memory access
        // Implements a short-term cache for frequently accessed memory addresses
        // Returns cached values when available to improve performance
        // ==========MY NOTES==============
        // This is like Read<T> but faster since it avoids repeated reads
        // Stores values for half a second before getting fresh ones
        // Great for reducing overhead when checking stats rapidly
        protected T ReadWithCache<T>(string cacheKey, nint baseAddress, int[] offsets) where T : unmanaged
        {
            // Check if we have a recent cached value
            if (_memoryCache.TryGetValue(cacheKey, out var cachedData) &&
                DateTime.Now - cachedData.Timestamp < _cacheDuration &&
                cachedData.Value is T value)
            {
                return value;
            }

            // Read fresh value from memory
            T result = Read<T>(baseAddress, offsets);
            _memoryCache[cacheKey] = (DateTime.Now, result);
            return result;
        }

        // ==========FORMAL COMMENT=========
        // Attempts to retrieve a cached value without accessing memory
        // Returns true and outputs the value if a valid cache entry exists
        // Prevents unnecessary memory reads when values haven't changed
        // ==========MY NOTES==============
        // Quick way to check if we already have the value cached
        // Returns false if cache miss, true if hit
        // Avoids memory reads completely when we have recent data
        protected bool TryGetCachedValue<T>(string cacheKey, out T value) where T : unmanaged
        {
            if (_memoryCache.TryGetValue(cacheKey, out var cachedData) &&
                DateTime.Now - cachedData.Timestamp < _cacheDuration &&
                cachedData.Value is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        // ==========FORMAL COMMENT=========
        // Manually stores a value in the memory cache
        // Associates the value with a timestamp for cache expiration
        // Useful for pre-populating cache with known values
        // ==========MY NOTES==============
        // Force a value into the cache
        // Useful when I already have a value and don't want to re-read it
        // Helps avoid redundant memory access
        protected void StoreInCache(string cacheKey, object value)
        {
            _memoryCache[cacheKey] = (DateTime.Now, value);
        }

        // ==========FORMAL COMMENT=========
        // Invalidates all cached memory values
        // Forces fresh reads from game memory on next access
        // Useful when game state changes significantly
        // ==========MY NOTES==============
        // Wipes out all cached values
        // Use when connecting to a new game or when cache might be wrong
        // Forces everything to be re-read from memory
        public void ClearCache()
        {
            _memoryCache.Clear();
        }
        #endregion

        #region whatever the heck this is
        // ==========FORMAL COMMENT=========
        // Windows API import for reading data from the memory of another process
        // Required for accessing the game's memory space
        // ==========MY NOTES==============
        // This is the Windows function that lets us peek into the game's memory
        // Without this, we couldn't read any stats
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time",
        Justification = "Using ReadProcessMemory for direct memory reading in unsafe context")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression", 
        Justification = "Required for unsafe memory operations")]
        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            nint lpBaseAddress,
            void* lpBuffer,
            nint nSize,
            out nint lpNumberOfBytesRead);
    }
    #endregion

}

