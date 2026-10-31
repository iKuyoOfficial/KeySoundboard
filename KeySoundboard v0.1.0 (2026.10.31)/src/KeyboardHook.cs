using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class KeyboardHook : IDisposable
    {
        private readonly NativeMethods.LowLevelKeyboardProc callback;
        private readonly HashSet<int> heldKeys = new HashSet<int>();
        private IntPtr hookHandle;
        private bool disposed;

        public event Action<int> KeyDown;
        public event Action<int> KeyUp;

        public bool Enabled { get; set; }
        public bool Suspended { get; set; }

        public KeyboardHook()
        {
            callback = HookCallback;
            Enabled = true;
        }

        public bool IsInstalled
        {
            get { return hookHandle != IntPtr.Zero; }
        }

        public void Install()
        {
            if (hookHandle != IntPtr.Zero) return;
            IntPtr module = NativeMethods.GetModuleHandle(null);
            hookHandle = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                callback,
                module,
                0);

            if (hookHandle == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "无法安装全局键盘钩子。");
            }
        }

        public void Uninstall()
        {
            if (hookHandle == IntPtr.Zero) return;
            NativeMethods.UnhookWindowsHookEx(hookHandle);
            hookHandle = IntPtr.Zero;
            heldKeys.Clear();
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && !disposed)
            {
                int message = wParam.ToInt32();
                bool isDown = message == NativeMethods.WM_KEYDOWN || message == NativeMethods.WM_SYSKEYDOWN;
                bool isUp = message == NativeMethods.WM_KEYUP || message == NativeMethods.WM_SYSKEYUP;

                if (isDown || isUp)
                {
                    var data = (NativeMethods.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(
                        lParam, typeof(NativeMethods.KBDLLHOOKSTRUCT));
                    int key = data.vkCode;

                    if (isDown)
                    {
                        bool firstPress = heldKeys.Add(key);
                        if (firstPress && Enabled && !Suspended && !NativeMethods.IsCurrentProcessForeground())
                        {
                            RaiseKeyDown(key);
                        }
                    }
                    else
                    {
                        heldKeys.Remove(key);
                        if (Enabled && !Suspended)
                        {
                            RaiseKeyUp(key);
                        }
                    }
                }
            }

            return NativeMethods.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        private void RaiseKeyDown(int key)
        {
            Action<int> handler = KeyDown;
            if (handler == null) return;
            try
            {
                handler(key);
            }
            catch
            {
            }
        }

        private void RaiseKeyUp(int key)
        {
            Action<int> handler = KeyUp;
            if (handler == null) return;
            try
            {
                handler(key);
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Uninstall();
        }
    }
}
