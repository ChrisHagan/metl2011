using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Components
{
    public partial class ScrollBar : UserControl
    {
        public ScrollViewer scroll;
        public ScrollBar()
        {
            InitializeComponent();
            scroll = new ScrollViewer();
            scroll.SizeChanged += scrollChanged;
            scroll.ScrollChanged += scroll_ScrollChanged;
            Commands.ExtendCanvasBothWays.RegisterCommand(new DelegateCommand<object>(ExtendBoth));
            VScroll.SmallChange = 3;
            HScroll.SmallChange = 3;
        }
        public void scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            adjustScrollers();
        }
        public void scrollChanged(object sender, SizeChangedEventArgs e)
        {
            adjustScrollers();
        }
        private void ExtendBoth(object _unused)
        {
            doCanvasMoveDown();
            doCanvasMoveRight();
        }
        private void canvasMoveDown(object sender, RoutedEventArgs e)
        {
            doCanvasMoveDown();
        }
        private void canvasMoveRight(object sender, RoutedEventArgs e)
        {
            doCanvasMoveRight();
        }
        private void doCanvasMoveDown()
        {
            ((FrameworkElement)scroll.Content).Height = ((FrameworkElement)scroll.Content).ActualHeight * 1.2;
        }
        private void doCanvasMoveRight()
        {
            ((FrameworkElement)scroll.Content).Width = ((FrameworkElement)scroll.Content).ActualWidth * 1.2;
        }
        private void VScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (scroll.VerticalOffset != VScroll.Value)
            {
                
                scroll.ScrollToVerticalOffset(VScroll.Value);
            }
        }
        private void HScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (scroll.HorizontalOffset != HScroll.Value)
                scroll.ScrollToHorizontalOffset(HScroll.Value);
        }
        public void adjustScrollers()
        {
            if (scroll.VerticalOffset != VScroll.Value)
                VScroll.Value = scroll.VerticalOffset;
            if (scroll.HorizontalOffset != HScroll.Value)
                HScroll.Value = scroll.HorizontalOffset;
            if (scroll.ScrollableHeight != VScroll.Maximum)
                VScroll.Maximum = scroll.ScrollableHeight;
            if (scroll.ScrollableWidth != HScroll.Maximum)
                HScroll.Maximum = scroll.ScrollableWidth;
            HScroll.ViewportSize = scroll.ActualWidth;
            VScroll.ViewportSize = scroll.ActualHeight;
        }
    }
}
