using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Interop;
using System.Windows.Media;
using SandRibbon;
using SandRibbonInterop;
using System.Windows.Input;
using SBSDKComWrapperLib;
using MessageBox = System.Windows.MessageBox;

namespace SandRibbon.Components
{
    public partial class SMARTboardControl : System.Windows.Controls.UserControl
    {

        private ISBSDKBaseClass2 Sbsdk;
        private _ISBSDKBaseClass2Events_Event SbsdkEvents;

        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessageA([MarshalAs(UnmanagedType.LPStr)] string lpString);
        private int SBSDKMessageID = RegisterWindowMessageA("SBSDK_NEW_MESSAGE");
        public static RoutedCommand ConnectToSmartboard = new RoutedCommand();
        public static RoutedCommand DisconnectFromSmartboard = new RoutedCommand();
        private bool isConnected = false;
        public IntPtr mainMeTLWindowPtr;
        public HwndSource mainMeTLWindowSrc;

        public SMARTboardControl()
        {
            InitializeComponent();
        }

        private void SMARTboardConsole(string message)
        {
            SMARTboardDiagnosticOutput.Items.Insert(0, SandRibbonObjects.DateTimeFactory.Now() + ": " + message);
        }

        private void canConnectToSmartboard(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isConnected == false && SMARTboardDiagnosticOutput != null;
            e.Handled = true;
        }
        private void ConnectToSmartboardExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            connectToSmartboard();
        }
        private void connectToSmartboard()
        {
            try
            {
                Sbsdk = new SBSDKBaseClass2();
            }
            catch (Exception e)
            {
                MessageBox.Show("A SMARTboard cannot be found.  Please check your connection to the SMARTboard.");
                return;
                //Logger.log("Exception in Main: " + e.Message);
            }
            if (Sbsdk != null)
            {
                SbsdkEvents = (_ISBSDKBaseClass2Events_Event)Sbsdk;
            }
            if (SbsdkEvents != null)
            {
                SbsdkEvents.OnEraser += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnEraserEventHandler(this.OnEraser);
                SbsdkEvents.OnNoTool += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNoToolEventHandler(this.OnNoTool);
                SbsdkEvents.OnPen += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPenEventHandler(this.OnPen);
                SbsdkEvents.OnBoardStatusChange += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnBoardStatusChangeEventHandler(this.OnBoardStatusChange);
            }

            if (Sbsdk != null)
            {
                Sbsdk.SBSDKAttachWithMsgWnd(mainMeTLWindowPtr.ToInt32(), false, mainMeTLWindowPtr.ToInt32());
                Sbsdk.SBSDKSetSendMouseEvents(mainMeTLWindowPtr.ToInt32(), _SBCSDK_MOUSE_EVENT_FLAG.SBCME_ALWAYS, -1);
            }
            isConnected = true;
            SMARTboardConsole("Connected to SMARTboard");
        }
        private void canDisconnectFromSmartboard(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isConnected == true;
            e.Handled = true;
        }
        private void DisconnectFromSmartboardExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            disconnectFromSmartboard();
        }
        private void disconnectFromSmartboard()
        {
            if (Sbsdk != null)
                Sbsdk.SBSDKDetach(mainMeTLWindowPtr.ToInt32());
            if (SbsdkEvents != null)
            {
                SbsdkEvents.OnEraser -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnEraserEventHandler(this.OnEraser);
                SbsdkEvents.OnNoTool -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNoToolEventHandler(this.OnNoTool);
                SbsdkEvents.OnPen -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPenEventHandler(this.OnPen);
                SbsdkEvents.OnBoardStatusChange -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnBoardStatusChangeEventHandler(this.OnBoardStatusChange);
            }
            Sbsdk = null;
            isConnected = false;
            SMARTboardConsole("Disconnected from SMARTboard");
        }

        void Main_Loaded(object sender, RoutedEventArgs e)
        {
            mainMeTLWindowPtr = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            mainMeTLWindowSrc = HwndSource.FromHwnd(mainMeTLWindowPtr);
            if (mainMeTLWindowSrc != null)
            {
                HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(Window.GetWindow(this)).Handle);
                source.AddHook(new HwndSourceHook(WndProc));
            }
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            OnNotifyMessage(msg);
            return IntPtr.Zero;
        }
        private void OnNotifyMessage(int m)
        {
            if (m.Equals(SBSDKMessageID) && Sbsdk != null)
            {
                Sbsdk.SBSDKProcessData();
            }
        }
        private void OnPen(int iPointerID)
        {
            var penRed = new int();
            var penGreen = new int();
            var penBlue = new int();
            int penAlpha = 255;
            Sbsdk.SBSDKGetToolColor(iPointerID, out penRed, out penGreen, out penBlue);
            Color SMARTboardPenColor = new Color();
            SMARTboardPenColor.R = Convert.ToByte(penRed);
            SMARTboardPenColor.G = Convert.ToByte(penGreen);
            SMARTboardPenColor.B = Convert.ToByte(penBlue);
            SMARTboardPenColor.A = Convert.ToByte(penAlpha);
            SMARTboardConsole("SMARTboard pen: R" + penRed + ", G" + penGreen + ", B" + penBlue + ", A" + penAlpha);
            var color = new Color { A = SMARTboardPenColor.A, R = SMARTboardPenColor.R, G = SMARTboardPenColor.G, B = SMARTboardPenColor.B };
            var drawingAttributes = new DrawingAttributes() { Color = color, IsHighlighter = false, Height = 3, Width = 3 };
            SandRibbon.Commands.SetDrawingAttributes.Execute(drawingAttributes);
            SandRibbon.Commands.SetInkCanvasMode.Execute("Ink");
        }

        private void OnNoTool(int iPointerID)
        {
            SMARTboardConsole("SMARTboard No Active Tool");
        }

        private void OnEraser(int iPointerID)
        {
            SandRibbon.Commands.SetInkCanvasMode.Execute("EraseByStroke");
            SMARTboardConsole("SMARTboard Eraser");
        }

        private void OnBoardStatusChange()
        {
            SMARTboardConsole("SMARTboard boardStatusChange");
        }
    }
}