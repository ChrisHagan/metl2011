using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Components
{
    public partial class ScrollBar : UserControl
    {
        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ScrollBar));

        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set 
            { 
                SetValue(ScrollViewerProperty, value);

                if (value != null)
                {
                    ScrollViewer.SizeChanged += sizeChanged;
                    ScrollViewer.ScrollChanged += scrollChanged;
                }
            }
        }

        public ScrollBar()
        {
            InitializeComponent();
            Commands.ExtendCanvasBothWays.RegisterCommand(new DelegateCommand<object>(ExtendBoth));

            updateScrollBarButtonDistances();
            VScroll.SmallChange = 10;
            HScroll.SmallChange = 10;
        }

        private void scrollChanged(object sender, ScrollChangedEventArgs e)
        {
            adjustScrollers();
        }

        private void sizeChanged(object sender, SizeChangedEventArgs e)
        {
            adjustScrollers();
        }

        private void ExtendBoth(object _unused)
        {
            var canvas = (FrameworkElement)ScrollViewer.Content;
            Commands.ExtendCanvasBySize.Execute(new Size(canvas.ActualWidth * 1.2, canvas.ActualHeight * 1.2));
        }

        private void VScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ScrollViewer.VerticalOffset != VScroll.Value)
            {
                ScrollViewer.ScrollToVerticalOffset(VScroll.Value);
            }
        }

        private void HScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ScrollViewer.HorizontalOffset != HScroll.Value)
                ScrollViewer.ScrollToHorizontalOffset(HScroll.Value);
        }

        private void adjustScrollers()
        {
            if (ScrollViewer.VerticalOffset != VScroll.Value)
                VScroll.Value = ScrollViewer.VerticalOffset;
            if (ScrollViewer.HorizontalOffset != HScroll.Value)
                HScroll.Value = ScrollViewer.HorizontalOffset;
            if (ScrollViewer.ScrollableHeight != VScroll.Maximum)
                VScroll.Maximum = ScrollViewer.ScrollableHeight;
            if (ScrollViewer.ScrollableWidth != HScroll.Maximum)
                HScroll.Maximum = ScrollViewer.ScrollableWidth;
            HScroll.ViewportSize = ScrollViewer.ActualWidth;
            VScroll.ViewportSize = ScrollViewer.ActualHeight;

            updateScrollBarButtonDistances();
        }

        private void updateScrollBarButtonDistances()
        {
            if (ScrollViewer != null)
            {
                HScroll.LargeChange = ScrollViewer.ActualWidth;
                VScroll.LargeChange = ScrollViewer.ActualHeight;
            }
        }
    }
}
