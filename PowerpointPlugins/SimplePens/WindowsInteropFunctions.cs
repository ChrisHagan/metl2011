using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace PowerpointJabber
{
    using LONG = System.Int32;
    using UINT = System.UInt32;

    class WindowsInteropFunctions
    {
        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError =
        true)]
        private static extern bool _EnumDesktopWindows(IntPtr
        hDesktop,
        EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError =
        true)]
        private static extern int _GetWindowText(IntPtr hWnd,
        StringBuilder lpWindowText, int nMaxCount);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        public const int SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef hwnd, out RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public LONG x;
            public LONG y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public UINT length;
            public UINT flags;
            public UINT showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        public static bool IsWindowShown(IntPtr hWnd)
        {
            bool res = IsWindowVisible(hWnd);
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            if (res && GetWindowPlacement(hWnd, ref placement))
            {
                switch (placement.showCmd)
                {
                    case SW_RESTORE:
                    case SW_SHOW:
                    case SW_SHOWMAXIMIZED:
                    case SW_SHOWNA:
                    case SW_SHOWNORMAL:
                        res = true;
                        break;
                    default:
                        res = false;
                        break;
                }
            }
            return res;
        }
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

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
        [DllImport("User32.dll", EntryPoint = "SetActiveWindow")]
        private static extern void SetActiveWindow(int hWnd);

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
        public static string GetWindowText(IntPtr hWnd)
        {
            StringBuilder title = new StringBuilder(255);
            int titleLength = _GetWindowText(hWnd, title,
            title.Capacity + 1);
            title.Length = titleLength;
            return title.ToString();
        }
        public static void switchToMeTL()
        {
            Dictionary<IntPtr, string> mTitlesList = new Dictionary<IntPtr, string>();
            EnumDelegate enumfunc = new EnumDelegate((EnumDelegate)delegate(IntPtr hWnd, int lParam)
            {
                string title = GetWindowText(hWnd);
                mTitlesList.Add(hWnd, title);
                return true;
            });
            IntPtr hDesktop = IntPtr.Zero; // current desktop
            bool success = _EnumDesktopWindows(hDesktop, enumfunc, IntPtr.Zero);
            int successFrequency = 0;
            if (success)
            {
                foreach (var KV in mTitlesList)
                {
                    if (KV.Value.StartsWith("MeTL") || KV.Value.EndsWith("- MeTL"))
                    {
                        BringWindowToFront(KV.Key);
                        successFrequency++;
                    }
                }
                if (successFrequency == 0)
                    System.Diagnostics.Process.Start("iexplore.exe", "-extoff http://metl.adm.monash.edu.au/MeTL2011/MeTL%20Presenter.application");
            }
            else
                System.Diagnostics.Process.Start("iexplore.exe", "-extoff http://metl.adm.monash.edu.au/MeTL2011/MeTL%20Presenter.application");
        }
        public static IntPtr presenterWindow
        {
            get
            {
                try
                {
                    return FindWindowByCaption("PowerPoint Presenter View - [" + ThisAddIn.instance.Application.ActivePresentation.Windows[1].Caption + "]");
                }
                catch (Exception ex)
                {
                    return (IntPtr)0;
                }
            }
        }
        public struct WindowStateData
        {
            public bool isVisible;
            public double X;
            public double Y;
        }
        private static IntPtr currentWindow()
        {
            try
            {
                return presenterActive ? presenterWindow : (IntPtr)ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.HWND;
            }
            catch (Exception) { return (IntPtr)0; }
        }
        public static WindowStateData getAppropriateViewData()
        {
            var window = currentWindow();
            var stateData = new WindowStateData();
            stateData.isVisible = (isWindowFocused(window) || (ThisAddIn.instance != null && ThisAddIn.instance.SSSW != null && ThisAddIn.instance.SSSW.HWND != null && isWindowFocused(ThisAddIn.instance.SSSW.HWND)));
            stateData.X = windowTopLeft(window).X;
            stateData.Y = windowTopLeft(window).Y;
            return stateData;
        }
        public static void BringAppropriateViewToFront()
        {

            if (presenterActive)
                BringWindowToFront(presenterWindow);
            else
                BringWindowToFront((IntPtr)ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.HWND);
        }
        private static void BringWindowToFront(IntPtr windowHandle)
        {
            SystemParametersInfo((uint)0x2001, 0, 0, 0x0002 | 0x0001);
            //ShowWindowAsync(windowHandle, WS_SHOWMAXIMIZED);
            SetForegroundWindow(windowHandle);
            //SystemParametersInfo((uint)0x2001, 200000, 200000, 0x0002 | 0x0001);
            SetActiveWindow((int)windowHandle);
        }
        private static Point windowTopLeft(IntPtr windowHandle)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            GetWindowPlacement(windowHandle, ref placement);
            Point res = new Point();
            switch (placement.showCmd)
            {
                case SW_SHOWMAXIMIZED:
                    res = new Point(0, 0);
                    //res = new Point(placement.ptMaxPosition.x, placement.ptMaxPosition.y);
                    break;
                case SW_RESTORE:
                case SW_SHOW:
                case SW_SHOWNA:
                case SW_SHOWNORMAL:
                    res = new Point(placement.rcNormalPosition.Left, placement.rcNormalPosition.Top);
                    break;
                default:
                    res = new Point();
                    break;
            }
            return res;
        }
        private static bool isWindowFocused(IntPtr windowHandle)
        {
            return (GetForegroundWindow() == windowHandle);
        }
    }
}
