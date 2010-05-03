using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Components
{
    /// <summary>
    /// Interaction logic for ScrollBar.xaml
    /// </summary>
    public partial class ScrollBar : UserControl
    {
        public ScrollViewer scroll;
        public ScrollBar()
        {
            InitializeComponent();
            scroll = new ScrollViewer();
            scroll.SizeChanged += scrollChanged;
            scroll.ScrollChanged += scroll_ScrollChanged;
        }
        public void scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            adjustScrollers();
        }
        public void scrollChanged(object sender, SizeChangedEventArgs e)
        {
            adjustScrollers();
        }
        private int INCREMENTAL_VALUE = 250;
        private void canvasMoveDown(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)scroll.Content).Height = ((FrameworkElement)scroll.Content).ActualHeight + INCREMENTAL_VALUE;
        }
        private void canvasMoveRight(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)scroll.Content).Width = ((FrameworkElement)scroll.Content).ActualWidth + INCREMENTAL_VALUE;
        }
        private void VScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (scroll.VerticalOffset != VScroll.Value)
                scroll.ScrollToVerticalOffset(VScroll.Value);
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
