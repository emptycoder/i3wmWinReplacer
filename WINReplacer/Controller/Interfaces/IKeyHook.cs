using System;
using System.Windows.Forms;

namespace WINReplacer
{
    delegate void KeyEventHandler();
    delegate void KeyClick(Keys key);
    delegate bool startDelegate();
    delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    interface IKeyHook
    {
        event KeyEventHandler LeftArrowEvent;
        event KeyEventHandler RightArrowEvent;
        event KeyEventHandler OpenCloseEvent;
        event KeyEventHandler EnterClickEvent;
        event KeyClick KeyClickEvent;
    }
}
