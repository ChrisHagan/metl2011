using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;

namespace SmartboardController
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Connect_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !App.isConnectedToSmartboard;
            e.Handled = true;
        }
        private void Connect_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            App.ConnectToSmartboard();
        }
        private void Disconnect_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = App.isConnectedToSmartboard;
            e.Handled = true;
        }
        private void Disconnect_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            App.DisconnectFromSmartboard();
        }
        private void CommandBridgeTest(object sender, RoutedEventArgs e)
        {
            var buttonTag = ((Button)sender).Content.ToString();
            App.SetLayer("Sketch");
            if (buttonTag == "eraser")
                App.SetInkCanvasMode("EraseByStroke");
            else
            {
                var da = new DrawingAttributes { Color = Colors.Black, Height = 5, Width = 5, IsHighlighter = false };
                switch (buttonTag)
                {
                    case "red":
                        da.Color = Colors.Red;
                        break;
                    case "blue":
                        da.Color = Colors.Blue;
                        break;
                    case "green":
                        da.Color = Colors.Green;
                        break;
                }
                App.SetInkCanvasMode("Ink");
                App.SetDrawingAttributes(da);
            }
        }
    }
}
