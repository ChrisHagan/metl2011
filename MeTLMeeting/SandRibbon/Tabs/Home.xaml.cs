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
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Tabs
{
    public partial class Home : Divelements.SandRibbon.RibbonTab
    {
        public Home()
        {
            InitializeComponent();
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(SetLayer));
        }
        private void SetLayer(string layer)
        {
            switch (layer)
            {
                case "Text":
                    ModalToolsGroup.Header = "Text Options";
                    break;
                case "Insert":
                    ModalToolsGroup.Header = "Image Options";
                    break;
                default:
                    ModalToolsGroup.Header = "Ink Options";
                    break;
            }
        }
        private void Viewbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var scroll = (ScrollViewer)DataContext;
            if (scroll == null) return;
            var pos = e.GetPosition(minimap);
            var x = (pos.X / minimap.Width) * scroll.ExtentWidth;
            var y = (pos.Y / minimap.Height) * scroll.ExtentHeight;
            if (new[] { x, y }.Any(i => Double.IsNaN(i))) return;
            var viewBoxXOffset = scroll.ViewportWidth / 2;
            var viewBoxYOffset = scroll.ViewportHeight / 2;
            var finalX = x - viewBoxXOffset;
            if (!(finalX > 0))
                finalX = 0;
            scroll.ScrollToHorizontalOffset(finalX);
            var finalY = y - viewBoxYOffset;
            if (!(finalY > 0))
                finalY = 0;
            scroll.ScrollToVerticalOffset(finalY);
        }
        private void Viewbox_MouseMove(object sender, MouseEventArgs e)
        {
            var scroll = (ScrollViewer)((FrameworkElement)sender).DataContext;
            if (scroll == null) return;
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(minimap);
                var x = (pos.X / minimap.Width) * scroll.ExtentWidth;
                var y = (pos.Y / minimap.Height) * scroll.ExtentHeight;
                if (new[] { x, y }.Any(i => Double.IsNaN(i))) return;
                var viewBoxXOffset = scroll.ViewportWidth / 2;
                var viewBoxYOffset = scroll.ViewportHeight / 2;
                var finalX = x - viewBoxXOffset;
                if (!(finalX > 0))
                    finalX = 0;
                scroll.ScrollToHorizontalOffset(finalX);
                var finalY = y - viewBoxYOffset;
                if (!(finalY > 0))
                    finalY = 0;
                scroll.ScrollToVerticalOffset(finalY);
            }
        }
    }
}
