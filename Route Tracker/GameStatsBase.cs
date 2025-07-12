using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Route_Tracker
{
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
        #region current. and old. things
        private readonly Dictionary<string, (object Old, object Current)> _statHistory = [];

        protected void RegisterStat(string name, object value)
        {
            if (_statHistory.TryGetValue(name, out var entry))
                _statHistory[name] = (entry.Current, value);
            else
                _statHistory[name] = (value, value);
        }

        public dynamic Current => new StatAccessor(_statHistory, true);
        public dynamic Old => new StatAccessor(_statHistory, false);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0290: Use primary constructure",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "because i said so")]
        private class StatAccessor : System.Dynamic.DynamicObject
        {
            private readonly Dictionary<string, (object Old, object Current)> _dict;
            private readonly bool _isCurrent;
            public StatAccessor(Dictionary<string, (object Old, object Current)> dict, bool isCurrent)
            {
                _dict = dict;
                _isCurrent = isCurrent;
            }
            public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
            {
                if (_dict.TryGetValue(binder.Name, out var entry))
                {
                    result = _isCurrent ? entry.Current : entry.Old;
                    return true;
                }
                result = null!;
                return false;
            }
        }
        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0290: Use primary constructure",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "because i said so")]
        public GameStatsBase(IntPtr processHandle, IntPtr baseAddress)
        {
            this.processHandle = processHandle;
            this.baseAddress = baseAddress;
        }

        protected readonly IntPtr processHandle;
        protected readonly IntPtr baseAddress;

        // Timer-related fields for automatic stat updates
        private System.Threading.CancellationTokenSource? _updateCancellationTokenSource;
        private bool _isUpdating = false;
        private readonly int _updateIntervalMs = 1000; // Default update interval of 1 second
        private System.Threading.Timer? _updateTimer; // Timer that controls the periodic stat updates

        // Memory cache system
        private readonly Dictionary<string, (DateTime Timestamp, object Value)> _memoryCache = [];
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMilliseconds(500); // Half-second cache

        // Performance optimization fields
        private readonly int _activeUpdateIntervalMs = 500;  // Fast updates during active gameplay
        private readonly int _idleUpdateIntervalMs = 1000;   // Slower updates when idle
        private DateTime _lastActivityTime = DateTime.Now;
        private bool _inActiveGameplay = true;
        private Dictionary<string, object> _previousStats = [];

        // Event that fires whenever game statistics change
        public event EventHandler<GameStatsEventArgs>? StatsUpdated;

        public virtual (bool IsLoading, bool IsMainMenu) GetGameStatus()
        {
            return (false, false);
        }

        // ==========FORMAL COMMENT=========
        // Generic method to read values from game memory
        // Follows chains of pointers using the provided offsets
        // ==========MY NOTES==============
        // This is the low-level function that actually reads from memory
        // It follows the trail of addresses to find the specific value we want
        protected unsafe T Read<T>(nint baseAddress, int[] offsets) where T : unmanaged
        {
            nint address = baseAddress;

            int pointer_size = 4; // Always use 32-bit
            foreach (int offset in offsets)
            {
                if (!ReadProcessMemory(processHandle, address, &address, pointer_size, out nint bytesReads) ||
                    bytesReads != pointer_size || address == IntPtr.Zero)
                {
                    return default;
                }
                address += offset;
            }

            T value;
            int size = sizeof(T);
            if (!ReadProcessMemory(processHandle, address, &value, size, out nint bytesRead) ||
                bytesRead != size)
            {
                return default;
            }
            return value;
        }

        #region Stats Retrieval
        // Only required method - each game implements this to provide its stats
        public abstract Dictionary<string, object> GetStatsAsDictionary();
        #endregion

        #region Update System
        // ==========FORMAL COMMENT=========
        // Timer-based auto-update system for game statistics
        // Periodically reads memory and notifies listeners when values change
        // ==========MY NOTES==============
        // Automatically refreshes stats every second without manual button clicks
        public void StartUpdating()
        {
            if (_isUpdating) return;
            _updateCancellationTokenSource = new System.Threading.CancellationTokenSource();
            _isUpdating = true;
            _updateTimer = new System.Threading.Timer(UpdateCallback, null, 0, _updateIntervalMs);
        }

        // ==========FORMAL COMMENT=========
        // Detects changes in game statistics compared to previous readings
        // ==========MY NOTES==============
        // Compares new stats with previous ones to see if anything changed
        private bool HasStatsChanged(Dictionary<string, object> currentStats)
        {
            bool changed = false;

            // Check if the number of items changed
            if (_previousStats.Count != currentStats.Count)
            {
                changed = true;
            }
            else
            {
                // Check if any values changed
                foreach (var kvp in currentStats)
                {
                    if (!_previousStats.TryGetValue(kvp.Key, out var prevValue) ||
                        !Equals(prevValue, kvp.Value))
                    {
                        changed = true;
                        break;
                    }
                }
            }

            // Store current stats for next comparison
            _previousStats = new Dictionary<string, object>(currentStats);

            return changed;
        }

        // ==========FORMAL COMMENT=========
        // Adjusts update frequency based on recent player activity
        // ==========MY NOTES==============
        // Smart timer that speeds up when the player is active
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
        // ==========MY NOTES==============
        // This runs every second to check for new values
        private void UpdateCallback(object? state)
        {
            try
            {
                if (!_isUpdating || _updateTimer == null)
                    return;

                // Get stats using the dictionary method
                var stats = GetStatsAsDictionary();

                // Check for changes
                if (HasStatsChanged(stats))
                {
                    _lastActivityTime = DateTime.Now;
                }

                // Update timer frequency
                UpdateAdaptiveTimer();

                // Raise event with new event args type
                StatsUpdated?.Invoke(this, new GameStatsEventArgs(stats));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in update timer callback: {ex.Message}");
            }
        }

        // ==========FORMAL COMMENT=========
        // Stops the automatic updates and releases timer resources
        // ==========MY NOTES==============
        // Shuts down the auto-update system cleanly
        public void StopUpdating()
        {
            if (!_isUpdating) return;

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
        // ==========MY NOTES==============
        // This is like Read<T> but faster since it avoids repeated reads
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
        // ==========MY NOTES==============
        // Quick way to check if we already have the value cached
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
        // ==========MY NOTES==============
        // Force a value into the cache
        protected void StoreInCache(string cacheKey, object value)
        {
            _memoryCache[cacheKey] = (DateTime.Now, value);
        }

        // ==========FORMAL COMMENT=========
        // Invalidates all cached memory values
        // ==========MY NOTES==============
        // Wipes out all cached values
        public void ClearCache()
        {
            _memoryCache.Clear();
        }
        #endregion

        // ==========FORMAL COMMENT=========
        // Windows API import for reading data from the memory of another process
        // ==========MY NOTES==============
        // This is the Windows function that lets us peek into the game's memory
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
}