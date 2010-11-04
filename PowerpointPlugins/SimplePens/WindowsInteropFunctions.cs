using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PowerpointJabber
{
    class WindowsInteropFunctions
    {
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]

        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("User32.dll", EntryPoint = "ShowWindowAsync")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        private const int WS_SHOWNORMAL = 1;
        private const int WS_SHOWMAXIMIZED = 3;
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        private static IntPtr FindWindowByCaption(string lpWindowName)
        {
            return FindWindowByCaption(IntPtr.Zero, lpWindowName);
        }
        public static bool presenterActive
        {
            get
            {
                if ((int)presenterWindow > 0)
                    return true;
                else return false;
            }
        }
        public static IntPtr presenterWindow
        {
            get
            {
                return FindWindowByCaption("PowerPoint Presenter View - [" + ThisAddIn.instance.Application.ActivePresentation.Windows[1].Caption + "]");
            }
        }
        public static void BringAppropriateViewToFront()
        {
            if (presenterActive)
                BringWindowToFront(presenterWindow);
            else
                BringWindowToFront((IntPtr)ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.HWND);
        }

        public static void BringWindowToFront(IntPtr windowHandle)
        {
            SystemParametersInfo((uint)0x2001, 0, 0, 0x0002 | 0x0001);
            ShowWindowAsync(windowHandle, WS_SHOWMAXIMIZED);
            SetForegroundWindow(windowHandle);
            SystemParametersInfo((uint)0x2001, 200000, 200000, 0x0002 | 0x0001);
        }
    }
}
