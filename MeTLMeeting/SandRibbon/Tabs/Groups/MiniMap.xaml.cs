using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SandRibbon.Tabs.Groups
{
    public partial class MiniMap : Divelements.SandRibbon.RibbonGroup
    {
        public MiniMap()
        {
            InitializeComponent();
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