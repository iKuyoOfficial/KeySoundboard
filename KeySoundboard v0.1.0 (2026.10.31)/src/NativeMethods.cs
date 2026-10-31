using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal static class NativeMethods
    {
        public const string MainWindowTitle = "音效键盘";
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;
        public const int WM_APP = 0x8000;
        public const int WM_SHOW_MAIN = WM_APP + 1;
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetProcessDPIAware();

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private static readonly uint CurrentProcessId = (uint)Process.GetCurrentProcess().Id;

        public static bool IsCurrentProcessForeground()
        {
            IntPtr foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero) return false;
            uint pid;
            GetWindowThreadProcessId(foreground, out pid);
            return pid == CurrentProcessId;
        }

        public static bool ActivateExistingWindow()
        {
            IntPtr hwnd = FindWindow(null, MainWindowTitle);
            if (hwnd == IntPtr.Zero) return false;

            PostMessage(hwnd, WM_SHOW_MAIN, IntPtr.Zero, IntPtr.Zero);
            if (IsIconic(hwnd)) ShowWindow(hwnd, SW_RESTORE);
            else ShowWindow(hwnd, SW_SHOW);
            SetForegroundWindow(hwnd);
            return true;
        }

        public static void EnableDarkTitleBar(IntPtr hwnd)
        {
            try
            {
                int enabled = 1;
                DwmSetWindowAttribute(hwnd, 20, ref enabled, sizeof(int));
                DwmSetWindowAttribute(hwnd, 19, ref enabled, sizeof(int));
            }
            catch
            {
                // Older Windows versions do not expose the immersive dark mode attribute.
            }
        }

        public static void ApplyDarkScrollbar(IntPtr hwnd)
        {
            try
            {
                SetWindowTheme(hwnd, "DarkMode_Explorer", null);
            }
            catch
            {
            }
        }

        public static void EnsureDpiAwareness()
        {
            try
            {
                SetProcessDPIAware();
            }
            catch
            {
            }
        }
    }
}
