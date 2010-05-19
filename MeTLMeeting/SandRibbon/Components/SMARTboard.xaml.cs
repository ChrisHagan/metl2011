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
            SMARTboardDiagnosticOutput.Items.Insert(0, DateTime.Now + ": " + message);
        }

        private void canConnectToSmartboard(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isConnected == false && SMARTboardDiagnosticOutput != null;
            e.Handled = true;
        }
        private void ConnectToSmartboardExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            isConnected = true;
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
                MessageBox.Show("Exception in Main: " + e.Message);
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
                SbsdkEvents.OnXYDown += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYDownEventHandler(this.OnXYDown);
                SbsdkEvents.OnXYMove += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYMoveEventHandler(this.OnXYMove);
                SbsdkEvents.OnXYUp += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYUpEventHandler(this.OnXYUp);
                SbsdkEvents.OnCircle += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnCircleEventHandler(this.OnCircle);
                SbsdkEvents.OnClear += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnClearEventHandler(this.OnClear);
                SbsdkEvents.OnLine += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnLineEventHandler(this.OnLine);
                SbsdkEvents.OnNext += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNextEventHandler(this.OnNext);
                SbsdkEvents.OnPrevious += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPreviousEventHandler(this.OnPrevious);
                SbsdkEvents.OnPrint += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPrintEventHandler(this.OnPrint);
                SbsdkEvents.OnRectangle += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnRectangleEventHandler(this.OnRectangle);
                SbsdkEvents.OnXMLAnnotation += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXMLAnnotationEventHandler(this.OnXMLAnnotation);
                SbsdkEvents.OnXYNonProjectedDown += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedDownEventHandler(this.OnXYNonProjectedDown);
                SbsdkEvents.OnXYNonProjectedMove += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedMoveEventHandler(this.OnXYNonProjectedMove);
                SbsdkEvents.OnXYNonProjectedUp += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedUpEventHandler(this.OnXYNonProjectedUp);
            }

            if (Sbsdk != null)
            {
                Sbsdk.SBSDKAttachWithMsgWnd(mainMeTLWindowPtr.ToInt32(), false, mainMeTLWindowPtr.ToInt32());
            }

            SMARTboardConsole("Connected to SMARTboard");
        }
        private void canDisconnectFromSmartboard(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isConnected == true;
            e.Handled = true;
        }
        private void DisconnectFromSmartboardExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            isConnected = false;
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
                SbsdkEvents.OnXYDown -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYDownEventHandler(this.OnXYDown);
                SbsdkEvents.OnXYMove -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYMoveEventHandler(this.OnXYMove);
                SbsdkEvents.OnXYUp -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYUpEventHandler(this.OnXYUp);
                SbsdkEvents.OnCircle -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnCircleEventHandler(this.OnCircle);
                SbsdkEvents.OnClear -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnClearEventHandler(this.OnClear);
                SbsdkEvents.OnLine -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnLineEventHandler(this.OnLine);
                SbsdkEvents.OnNext -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNextEventHandler(this.OnNext);
                SbsdkEvents.OnPrevious -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPreviousEventHandler(this.OnPrevious);
                SbsdkEvents.OnPrint -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPrintEventHandler(this.OnPrint);
                SbsdkEvents.OnRectangle -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnRectangleEventHandler(this.OnRectangle);
                SbsdkEvents.OnXMLAnnotation -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXMLAnnotationEventHandler(this.OnXMLAnnotation);
                SbsdkEvents.OnXYNonProjectedDown -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedDownEventHandler(this.OnXYNonProjectedDown);
                SbsdkEvents.OnXYNonProjectedMove -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedMoveEventHandler(this.OnXYNonProjectedMove);
                SbsdkEvents.OnXYNonProjectedUp -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedUpEventHandler(this.OnXYNonProjectedUp);
            }
            Sbsdk = null;
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
            MessageBox.Show(SMARTboardPenColor.ToString());
            SandRibbon.Commands.SetInkCanvasMode.Execute(InkCanvasEditingMode.Ink);
        }

        private void OnNoTool(int iPointerID)
        {
            Color emptyColor = new Color();
            emptyColor.R = (byte)0;
            emptyColor.G = (byte)0;
            emptyColor.B = (byte)0;
            emptyColor.A = (byte)255;
            SMARTboardConsole("SMARTboard No Active Tool");
            SandRibbon.Commands.SetInkCanvasMode.Execute(InkCanvasEditingMode.None);
        }

        private void OnEraser(int iPointerID)
        {
            // It'd be nice to have a command like this:
            // Commands.SetEraseMode.Execute(size, (IInputElement)Parent);
            SandRibbon.Commands.SetInkCanvasMode.Execute(InkCanvasEditingMode.EraseByPoint);
            SMARTboardConsole("SMARTboard Eraser");
        }

        private void OnBoardStatusChange()
        {
            //This fires when the board connects or disconnects while the SDK is connected,
            //or when the SDK connects or disconnects.
            SMARTboardConsole("SMARTboard boardStatusChange");
        }

        private void OnXYDown(int x, int y, int z, int iPointerID)
        {
            SMARTboardConsole("PenDown: X" + x + ", Y" + y + ", Z" + z);
            
            float floatZ = new float();
            if (z == 0)
                floatZ = 0.5f;
            CreateStroke(x, y, floatZ, iPointerID);
        }

        private void OnXYMove(int x, int y, int z, int iPointerID)
        {
            SMARTboardConsole("PenMove: X" + x + ", Y" + y + ", Z" + z);
            
            float floatZ = new float();
            if (z == 0)
                floatZ = 0.5f;
            AddToStroke(x, y, floatZ, iPointerID);
        }

        private void OnXYUp(int x, int y, int z, int iPointerID)
        {
            SMARTboardConsole("PenUp: X" + x + ", Y" + y + ", Z" + z);
            
            float floatZ = new float();
            if (z == 0)
                floatZ = 0.5f;
            CompleteStroke(x, y, floatZ, iPointerID);
        }
        private void OnMouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var x = (int)e.GetPosition(this).X;
            var y = (int)e.GetPosition(this).Y;
            OnXYDown(x, y, 0, 0);
            SMARTboardConsole("MouseDown: X" + x + ", Y" + y);
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var x = (int)e.GetPosition(this).X;
            var y = (int)e.GetPosition(this).Y;
            OnXYMove(x, y, 0, 0);
            SMARTboardConsole("MouseMove: X" + x + ", Y" + y);
        }

        private void OnMouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var x = (int)e.GetPosition(this).X;
            var y = (int)e.GetPosition(this).Y;
            OnXYUp(x, y, 0, 0);
            SMARTboardConsole("MouseUp: X" + x + ", Y" + y);
        }

        private void OnXYHover(int x, int y, int z, int iPointerID)
        {
            SMARTboardConsole("Hover: X" + x + ", Y" + y + ", Z" + z + ", iPointerID" + iPointerID);
        }

        private void OnCircle(int iPointerID)
        {
            SMARTboardConsole("Circle: " + iPointerID);
        }

        private void OnLine(int iPointerID)
        {
            SMARTboardConsole("Line: " + iPointerID);
        }

        private void OnRectangle(int iPointerID)
        {
            SMARTboardConsole("Rectangle: " + iPointerID);
        }

        private void OnClear(int iPointerID)
        {
            SMARTboardConsole("Clear: " + iPointerID);
        }

        private void OnNext(int iPointerID)
        {
            SMARTboardConsole("Next: " + iPointerID);
        }

        private void OnPrevious(int iPointerID)
        {
            SMARTboardConsole("Previous: " + iPointerID);
        }

        private void OnPrint(int iPointerID)
        {
            SMARTboardConsole("Print: " + iPointerID);
        }

        private void OnXMLAnnotation(string xml)
        {
            SMARTboardConsole("XMLAnnotation: " + xml);
        }

        private void OnXYNonProjectedDown(int x, int y, int z, int iPointerID)
        {
            SMARTboardConsole("XYNonProjectedDown: X" + x + ", Y" + y + ", Z" + z + ", iPointerID" + iPointerID);
        }

        private void OnXYNonProjectedMove(int x, int y, int z, int iPointerID)
        {
            SMARTboardConsole("XYNonProjectedMove: X" + x + ", Y" + y + ", Z" + z + ", iPointerID" + iPointerID);
        }

        private void OnXYNonProjectedUp(int x, int y, int z, int iPointerID)
        {
            SMARTboardConsole("XYNonProjectedUp: X" + x + ", Y" + y + ", Z" + z + ", iPointerID" + iPointerID);
        }
        //This section is for creating strokes from the SMARTboard's pen and/or mouse

        private Stroke CurrentStroke;

        private void CreateStroke(int x, int y, float z, int iPointerID)
        {
            if (CurrentStroke == null)
                CurrentStroke = new Stroke(new StylusPointCollection());
            if (CurrentStroke != null)
                MessageBox.Show(CurrentStroke.ToString());
            StylusPoint Pt = new StylusPoint(x, y, z);
            CurrentStroke.StylusPoints.Add(Pt);

            if (CurrentStroke.StylusPoints.Count == 1)
            {

                var CurrentPenWidth = 3;
                int iRed = 0, iGreen = 0, iBlue = 0;
                var CurrentPenColor = Color.FromArgb((byte)iRed, (byte)iGreen, (byte)iBlue, (byte)255);
                if (Sbsdk != null)
                {
                    Sbsdk.SBSDKGetToolColor(iPointerID, out iRed, out iGreen, out iBlue);
                    CurrentPenWidth = Sbsdk.SBSDKGetToolWidth(iPointerID);
                }
                CurrentStroke.DrawingAttributes.Color = CurrentPenColor;
                CurrentStroke.DrawingAttributes.Width = CurrentPenWidth;
            }
        }
        private void AddToStroke(int x, int y, float z, int iPointerID)
        {
            if (CurrentStroke.StylusPoints.Count != 0)
            {
                StylusPoint Pt = new StylusPoint(x, y, z);
                CurrentStroke.StylusPoints.Add(Pt);
            }
            else
            {
                CreateStroke(x, y, z, iPointerID);
            }
        }
        private void CompleteStroke(int x, int y, float z, int iPointerID)
        {
            if (CurrentStroke.StylusPoints.Count != 0)
            {
                AddToStroke(x, y, z, iPointerID);
                SMARTboardConsole("Points: " + CurrentStroke.StylusPoints.Count + ", Colour: " + CurrentStroke.DrawingAttributes.Color);
                Commands.ReceiveStroke.Execute(CurrentStroke.Clone()); 
                CurrentStroke.StylusPoints.Clear();
            }
            else
            {
                CreateStroke(x, y, z, iPointerID);
                CompleteStroke(x, y, z, iPointerID);
            }
        }
    }
}