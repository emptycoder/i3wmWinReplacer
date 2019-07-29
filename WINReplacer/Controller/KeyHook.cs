using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WINReplacer
{
    class KeyHook : IDisposable
    {
        const int WH_KEYBOARD_LL = 13;
        const int WH_KEYDOWN = 256;
        const int WH_KEYUP = 257;

        readonly Dictionary<Keys, startDelegate> keyUp = new Dictionary<Keys, startDelegate>();
        readonly Dictionary<Keys, startDelegate> keyDown = new Dictionary<Keys, startDelegate>();
        readonly Dictionary<Keys, KeyEventHandler> startApps = new Dictionary<Keys, KeyEventHandler>();

        bool win_pushed = false;
        readonly Form form;

        IntPtr _hookID = IntPtr.Zero;
        LowLevelKeyboardProc hookDelegate;

        public event KeyEventHandler LeftArrowEvent;
        public event KeyEventHandler RightArrowEvent;
        public event KeyEventHandler OpenCloseEvent;
        public event KeyEventHandler EnterClickEvent;
        public event KeyClick KeyClickEvent;

        public KeyHook(Form form)
        {
            this.form = form;
            keyDown.Add(Keys.LWin, delegate
            {
                win_pushed = true;
                return false;
            });
            keyUp.Add(Keys.LWin,
                delegate
                {
                    win_pushed = false;
                    return false;
                }
            );
            keyUp.Add(Keys.Left, delegate
            {
                if (form.Visible)
                {
                    LeftArrowEvent?.Invoke();
                    return false;
                }
                return true;
            });
            keyUp.Add(Keys.Right, delegate
            {
                if (form.Visible)
                {
                    RightArrowEvent?.Invoke();
                    return false;
                }
                return true;
            });
            keyUp.Add(Keys.Enter, delegate
            {
                if (form.Visible)
                {
                    EnterClickEvent?.Invoke();
                    return false;
                }
                return true;
            });
            keyUp.Add(Keys.Escape, delegate
            {
                if (form.Visible)
                {
                    OpenCloseEvent?.Invoke();
                    return false;
                }
                return true;
            });
            startApps.Add(Keys.D, delegate
            {
                 OpenCloseEvent?.Invoke();
            });
            startApps.Add(Keys.E, delegate
            {
                 Process.Start("explorer");
            });
            startApps.Add(Keys.I, delegate
            {
                 Process.Start("ms-settings:");
            });
            startApps.Add(Keys.L, delegate
            {
                 Process.Start("rundll32.exe user32.dll,LockWorkStation");
            });

            hookDelegate = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookDelegate,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private IntPtr HookCallback(
           int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return CallNextHookEx(_hookID, nCode, wParam, lParam);

            if ((int)wParam == WH_KEYDOWN)
            {
                Keys key = (Keys)Marshal.ReadInt32(lParam);
                if (keyDown.TryGetValue(key, out startDelegate start))
                {
                    start();
                    return (IntPtr)1;
                }
                if (startApps.ContainsKey(key) && win_pushed
                    || form.Visible)
                {
                    return (IntPtr)1;
                }
            }
            if ((int)wParam == WH_KEYUP)
            {
                Keys key = (Keys)Marshal.ReadInt32(lParam);
                if (keyUp.TryGetValue(key, out startDelegate start) && !start())
                {
                    return (IntPtr)1;
                }
                else if (startApps.TryGetValue(key, out KeyEventHandler app) && win_pushed)
                {
                    app();
                    return (IntPtr)1;
                }
                if (form.Visible)
                {
                    KeyClickEvent?.Invoke(key);
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        void IDisposable.Dispose()
        {
            UnhookWinEvent(_hookID);
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
