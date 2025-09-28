using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Route_Tracker
{
    public static class GlobalHotkeyManager
    {
        private static WinAPI.LowLevelKeyboardProc? _proc = null;
        private static IntPtr _hookID = IntPtr.Zero;
        private static readonly Dictionary<Keys, Action> _registeredHotkeys = [];
        private static readonly HashSet<Keys> _pressedKeys = [];
        private static Form? _mainForm = null;

        public static void Install(Form? mainForm = null)
        {
            if (_hookID != IntPtr.Zero) return;

            _mainForm = mainForm;
            _proc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
                _hookID = WinAPI.SetWindowsHookEx(WinAPI.WH_KEYBOARD_LL, _proc, WinAPI.GetModuleHandle(curModule?.ModuleName ?? ""), 0);

            if (_hookID == IntPtr.Zero) LoggingSystem.LogError("Failed to install global hotkey hook");
            else LoggingSystem.LogInfo("Global hotkey hook installed successfully");
        }

        public static void Uninstall()
        {
            if (_hookID == IntPtr.Zero) return;
            WinAPI.UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
            _registeredHotkeys.Clear();
            _pressedKeys.Clear();
            _mainForm = null;
            LoggingSystem.LogInfo("Global hotkey hook uninstalled");
        }

        public static void RegisterHotkey(Keys keys, Action action)
        {
            _registeredHotkeys[keys] = action;
        }

        private static void BringApplicationToForeground()
        {
            if (_mainForm == null) return;

            try
            {
                IntPtr handle = _mainForm.Handle;

                // If the window is minimized, restore it
                if (WinAPI.IsIconic(handle)) WinAPI.ShowWindow(handle, WinAPI.SW_RESTORE);
                else WinAPI.ShowWindow(handle, WinAPI.SW_SHOW);

                // Bring window to top and set foreground
                WinAPI.BringWindowToTop(handle);
                WinAPI.SetForegroundWindow(handle);

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

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                try
                {
                    if (wParam == WinAPI.WM_KEYDOWN)
                    {
                        int vkCode = Marshal.ReadInt32(lParam);
                        Keys key = (Keys)vkCode;
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
                            try
                            {
                                BringApplicationToForeground();
                                action?.Invoke();
                                // Suppress the key
                                return 1;
                            }
                            catch (Exception ex)
                            {
                                LoggingSystem.LogError($"Error executing global hotkey action: {ex.Message}");
                            }
                        }
                    }
                    else if (wParam == WinAPI.WM_KEYUP)
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

            return WinAPI.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static bool IsKeyPressed(Keys key)
        {
            return (WinAPI.GetKeyState((int)key) & 0x8000) != 0;
        }
    }
}