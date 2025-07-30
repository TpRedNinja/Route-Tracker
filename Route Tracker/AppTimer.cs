using System;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // Universal timer class that handles all timer needs in the app
    // Prevents timer-related errors and provides consistent timer functionality
    // Works for both UI and background operations
    public class AppTimer : IDisposable
    {
        private System.Windows.Forms.Timer? _winFormsTimer;
        private System.Threading.Timer? _threadingTimer;
        private readonly bool _isUITimer;
        private bool _disposed;

        // ==========MY NOTES==============
        // Creates a UI timer (for animations, UI updates)
        public static AppTimer CreateUITimer(int intervalMs, EventHandler tickHandler)
        {
            var timer = new AppTimer(true)
            {
                _winFormsTimer = new System.Windows.Forms.Timer
                {
                    Interval = intervalMs
                }
            };
            timer._winFormsTimer.Tick += tickHandler;
            return timer;
        }

        // ==========MY NOTES==============
        // Creates a background timer (for background operations)
        public static AppTimer CreateBackgroundTimer(int intervalMs, TimerCallback callback)
        {
            var timer = new AppTimer(false)
            {
                _threadingTimer = new System.Threading.Timer(callback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite),
                IntervalMs = intervalMs
            };
            return timer;
        }

        // ==========MY NOTES==============
        // Creates a one-shot delay timer for staggered operations
        public static AppTimer CreateDelayTimer(int delayMs, Action callback)
        {
            var timer = new AppTimer(true);
            timer._winFormsTimer = new System.Windows.Forms.Timer
            {
                Interval = delayMs
            };
            timer._winFormsTimer.Tick += (s, e) =>
            {
                timer.Stop();
                callback();
                timer.Dispose();
            };
            return timer;
        }

        private AppTimer(bool isUITimer)
        {
            _isUITimer = isUITimer;
        }

        public int IntervalMs { get; private set; }

        // ==========MY NOTES==============
        // Starts the timer
        public void Start()
        {
            if (_disposed) return;

            if (_isUITimer && _winFormsTimer != null)
            {
                _winFormsTimer.Start();
            }
            else if (!_isUITimer && _threadingTimer != null)
            {
                _threadingTimer.Change(0, IntervalMs);
            }
        }

        // ==========MY NOTES==============
        // Stops the timer
        public void Stop()
        {
            if (_disposed) return;

            if (_isUITimer && _winFormsTimer != null)
            {
                _winFormsTimer.Stop();
            }
            else if (!_isUITimer && _threadingTimer != null)
            {
                _threadingTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
        }

        // ==========MY NOTES==============
        // Changes the timer interval
        public void ChangeInterval(int newIntervalMs)
        {
            if (_disposed) return;

            IntervalMs = newIntervalMs;

            if (_isUITimer && _winFormsTimer != null)
            {
                bool wasRunning = _winFormsTimer.Enabled;
                _winFormsTimer.Stop();
                _winFormsTimer.Interval = newIntervalMs;
                if (wasRunning)
                    _winFormsTimer.Start();
            }
            else if (!_isUITimer && _threadingTimer != null)
            {
                _threadingTimer.Change(0, newIntervalMs);
            }
        }

        // ==========MY NOTES==============
        // Checks if the timer is currently running
        public bool IsRunning
        {
            get
            {
                if (_disposed) return false;

                if (_isUITimer && _winFormsTimer != null)
                    return _winFormsTimer.Enabled;

                // For threading timer, we can't directly check if it's running
                // so we track it based on whether it was started
                return !_disposed;
            }
        }

        // ==========MY NOTES==============
        // Proper disposal pattern that satisfies CA1816
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    Stop();
                    _winFormsTimer?.Dispose();
                    _threadingTimer?.Dispose();
                }

                // Clean up unmanaged resources (none in this case)
                _winFormsTimer = null;
                _threadingTimer = null;
                _disposed = true;
            }
        }
    }
}