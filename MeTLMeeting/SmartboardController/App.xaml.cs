using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Automation;

namespace SmartboardController
{
    public partial class App : Application
    {
        public static RoutedCommand connectToSmartboard = new RoutedCommand();
        public static RoutedCommand reconnectToSmartboard = new RoutedCommand();
        public static RoutedCommand disconnectFromSmartboard = new RoutedCommand();
        private static AutomationElement metl;
        private static int metlPtr;
        public static int appPtr;
        private static SmartboardConnector smartboard = new SmartboardConnector();
        public static void AttachToProcess()
        {
            try
            {
                var candidate = WindowsInteropFunctions.fuzzyFindWindow("MeTL");
                if (metlPtr == 0 || metlPtr != candidate.ToInt32())
                {
                    metlPtr = (int)candidate;
                    if (metlPtr > 0)
                    {
                        var rootMeTLWindow = AutomationElement.FromHandle((IntPtr)metlPtr);
                        metl = rootMeTLWindow.Descendant("commandBridge");
                        smartboard.ReAssertConnection(metlPtr);
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("Could not find a process named MeTL.  Have you started an instance (it can be clickonce)");
            }
        }
        public static void SetDrawingAttributes(DrawingAttributes attributes)
        {
            AttachToProcess();
            if (metl != null)
            {
                var color = String.Format("{0} {1} {2} {3}", attributes.Color.A.ToString(), attributes.Color.R.ToString(), attributes.Color.G.ToString(), attributes.Color.B.ToString());
                var size = attributes.Height.ToString();
                var isHighlighter = attributes.IsHighlighter.ToString();
                metl.Value(String.Format("SetDrawingAttributes:{0}:{1}:{2}", color, size, isHighlighter));
            }
        }
        public static void SetInkCanvasMode(string mode)
        {
            AttachToProcess();
            if (metl != null)
            {
                metl.Value(String.Format("SetInkCanvasMode:{0}", mode));
            }
        }
        public static void SetLayer(string mode)
        {
            AttachToProcess();
            if (metl != null)
            {
                metl.Value(String.Format("SetLayer:{0}", mode));
            }
        }
        public static void InitialConnectToSmartboard()
        {
            AttachToProcess();
            if (metlPtr == 0)
                metlPtr = appPtr;
            if (metlPtr > 0 && appPtr > 0 && !isConnectedToSmartboard)
                smartboard.connectToSmartboard((IntPtr)metlPtr);
        }
        public static void ConnectToSmartboard()
        {
            AttachToProcess();
            if (metlPtr > 0 && appPtr > 0 && !isConnectedToSmartboard)
                smartboard.connectToSmartboard((IntPtr)metlPtr);
        }
        public static void DisconnectFromSmartboard()
        {
            AttachToProcess();
            if (metlPtr > 0 && appPtr > 0 && isConnectedToSmartboard)
                smartboard.disconnectFromSmartboard();
        }
        public static bool isConnectedToSmartboard
        {
            get
            {
                if (smartboard.isConnected)
                    return true;
                else return false;
            }
        }
    }
}
