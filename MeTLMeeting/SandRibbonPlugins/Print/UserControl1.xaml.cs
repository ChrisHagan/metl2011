using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace Print
{
    public partial class PluginMain : System.Windows.Controls.UserControl
    {

        public PluginMain()
        {
            InitializeComponent();
        }
        private void Print(object sender, RoutedEventArgs e)
        {
            var sourceWindow = Window.GetWindow(this);
            if (sourceWindow != null)
            {
                PrintDialog printDialog = new PrintDialog();
                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    Convert.ToInt32(printDialog.PrintableAreaHeight), Convert.ToInt32(printDialog.PrintableAreaWidth), 96, 96,
                                                                PixelFormats.Pbgra32);
                rtb.Render(sourceWindow);
                printDialog.ShowDialog();
                printDialog.PrintVisual(sourceWindow, "this Page");
                
            }
        }
    }
}