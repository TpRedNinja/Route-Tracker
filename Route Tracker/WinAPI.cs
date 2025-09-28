using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;


namespace Route_Tracker
{
    internal static class WinAPI
    {
        internal const int WH_KEYBOARD_LL = 13;
        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x0101;
        internal const int SW_RESTORE = 9;
        internal const int SW_SHOW = 5;

        internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [SuppressMessage("Style", "CA2101")]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll")]
        internal static extern short GetKeyState(int nVirtKey);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll")]
        internal static extern bool IsIconic(IntPtr hWnd);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll")]
        internal static extern bool BringWindowToTop(IntPtr hWnd);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll")]
        internal static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [SuppressMessage("Style", "SYSLIB1054")]
        [SuppressMessage("Style", "IDE0079")]
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();
    }
}
