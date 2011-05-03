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
        public static RoutedCommand disconnectFromSmartboard = new RoutedCommand();
        private static SmartboardConnector smartboard;
        private static AutomationElement metl;
        public static void AttachToProcess()
        {
            if (metl == null)
                try
                {
                    var rootMetl = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "ribbonWindow"))[0];
                    metl = rootMetl.Descendant("commandBridge");
                }
                catch (Exception)
                {
                    //MessageBox.Show("Could not find a process named MeTL.  Have you started an instance (it can be clickonce)");
                }
        }
        public static void initializeSmartboard(DependencyObject userControl)
        {
            smartboard = new SmartboardConnector(userControl);
            AttachToProcess();
        }
        public static void SetDrawingAttributes(DrawingAttributes attributes)
        {
            if (metl == null)
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
            if (metl == null)
                AttachToProcess();
            if (metl != null)
            {
                metl.Value(String.Format("SetInkCanvasMode:{0}", mode));
            }
        }
        public static void SetLayer(string mode)
        {
            if (metl == null)
                AttachToProcess();
            if (metl != null)
            {
                metl.Value(String.Format("SetLayer:{0}", mode));
            }
        }
        public static void ConnectToSmartboard()
        {
            if (smartboard != null && !isConnectedToSmartboard)
                smartboard.connectToSmartboard(null);
        }
        public static void DisconnectFromSmartboard()
        {
            if (isConnectedToSmartboard)
                smartboard.disconnectFromSmartboard(null);
        }
        public static bool isConnectedToSmartboard
        {
            get
            {
                if (smartboard != null && smartboard.isConnected)
                    return true;
                else return false;
            }
        }
    }
}
