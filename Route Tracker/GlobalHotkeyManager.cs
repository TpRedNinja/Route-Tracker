using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // Low-level global hotkey manager that works even when games have focus
    // Uses Windows keyboard hooks instead of RegisterHotKey to bypass game interference
    public static class GlobalHotkeyManager
    {
        private static LowLevelKeyboardProc? _proc = null;
        private static IntPtr _hookID = IntPtr.Zero;
        private static readonly Dictionary<Keys, Action> _registeredHotkeys = [];
        private static readonly HashSet<Keys> _pressedKeys = [];
        private static Form? _mainForm = null; // Store reference to main form

        // Windows API constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        // Constants for ShowWindow
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        // Windows API imports
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA2101",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "SYSLIB1054",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // Delegate for the hook procedure
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // ==========MY NOTES==============
        // Installs the low-level keyboard hook and stores main form reference
        public static void Install(Form? mainForm = null)
        {
            if (_hookID != IntPtr.Zero) return; // Already installed

            _mainForm = mainForm; // Store reference to main form
            _proc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
                    GetModuleHandle(curModule?.ModuleName ?? ""), 0);
            }

            if (_hookID == IntPtr.Zero)
            {
                LoggingSystem.LogError("Failed to install global hotkey hook");
            }
            else
            {
                LoggingSystem.LogInfo("Global hotkey hook installed successfully");
            }
        }

        // ==========MY NOTES==============
        // Uninstalls the keyboard hook
        public static void Uninstall()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
                _registeredHotkeys.Clear();
                _pressedKeys.Clear();
                _mainForm = null;
                LoggingSystem.LogInfo("Global hotkey hook uninstalled");
            }
        }

        // ==========MY NOTES==============
        // Registers a hotkey combination with an action
        public static void RegisterHotkey(Keys keys, Action action)
        {
            _registeredHotkeys[keys] = action;
        }

        // ==========MY NOTES==============
        // Brings the Route Tracker window to the foreground
        private static void BringApplicationToForeground()
        {
            if (_mainForm == null) return;

            try
            {
                IntPtr handle = _mainForm.Handle;

                // If the window is minimized, restore it
                if (IsIconic(handle))
                {
                    ShowWindow(handle, SW_RESTORE);
                }
                else
                {
                    ShowWindow(handle, SW_SHOW);
                }

                // Bring window to top and set foreground
                BringWindowToTop(handle);
                SetForegroundWindow(handle);

                // Force the window to be active and on top
                if (_mainForm.InvokeRequired)
                {
                    _mainForm.Invoke(new Action(() =>
                    {
                        _mainForm.Activate();
                        _mainForm.BringToFront();
                        _mainForm.Focus();
                    }));
                }
                else
                {
                    _mainForm.Activate();
                    _mainForm.BringToFront();
                    _mainForm.Focus();
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError($"Error bringing application to foreground: {ex.Message}");
            }
        }

        // ==========MY NOTES==============
        // The hook callback that processes all keyboard input
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                try
                {
                    if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        int vkCode = Marshal.ReadInt32(lParam);
                        Keys key = (Keys)vkCode;

                        // Track pressed keys
                        _pressedKeys.Add(key);

                        // Build current key combination
                        Keys combination = key;

                        if (IsKeyPressed(Keys.ControlKey) || IsKeyPressed(Keys.LControlKey) || IsKeyPressed(Keys.RControlKey))
                            combination |= Keys.Control;

                        if (IsKeyPressed(Keys.ShiftKey) || IsKeyPressed(Keys.LShiftKey) || IsKeyPressed(Keys.RShiftKey))
                            combination |= Keys.Shift;

                        if (IsKeyPressed(Keys.Menu) || IsKeyPressed(Keys.LMenu) || IsKeyPressed(Keys.RMenu))
                            combination |= Keys.Alt;

                        // Check if this combination is registered
                        if (_registeredHotkeys.TryGetValue(combination, out Action? action))
                        {
                            // Execute the action on the UI thread
                            try
                            {
                                // Bring application to foreground BEFORE executing the action
                                BringApplicationToForeground();

                                // Execute the action
                                action?.Invoke();

                                return (IntPtr)1; // Suppress the key
                            }
                            catch (Exception ex)
                            {
                                LoggingSystem.LogError($"Error executing global hotkey action: {ex.Message}");
                            }
                        }
                    }
                    else if (wParam == (IntPtr)WM_KEYUP)
                    {
                        int vkCode = Marshal.ReadInt32(lParam);
                        Keys key = (Keys)vkCode;
                        _pressedKeys.Remove(key);
                    }
                }
                catch (Exception ex)
                {
                    LoggingSystem.LogError($"Error in global hotkey hook: {ex.Message}");
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        // ==========MY NOTES==============
        // Checks if a specific key is currently pressed
        private static bool IsKeyPressed(Keys key)
        {
            return (GetKeyState((int)key) & 0x8000) != 0;
        }
    }
}