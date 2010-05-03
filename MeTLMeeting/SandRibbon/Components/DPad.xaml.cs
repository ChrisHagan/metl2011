using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SandRibbon.Components
{
    public partial class DPad : UserControl
    {
        public static PointCollection POINTS = new PointCollection().With(0, 100, 50, 0, 100, 100);
        public delegate void MoveRequestedHandler(Arrow direction);
        public event MoveRequestedHandler MoveRequested;
        public DPad()
        {
            InitializeComponent();
        }
        private void directionSelected(object sender, RoutedEventArgs e)
        {
            if (MoveRequested != null)
                MoveRequested(new Arrow { angle = Int32.Parse((string)((FrameworkElement)sender).DataContext )});
        }
    }
    public static class PointCollectionExtension
    {
        public static PointCollection With(this PointCollection collection, params int[] points)
        {
            return new PointCollection(points
                        .Select((value, index) => new { PairNum = index / 2, value })
                        .GroupBy(pair => pair.PairNum)
                        .Select(grp => grp.Select(g => g.value).ToArray())
                        .Select(a=>new Point(a[0],a[1])));
        }
    }
    public class Arrow : DependencyObject
    {
        public int angle
        {
            get { return (int)GetValue(angleProperty); }
            set { SetValue(angleProperty, value); }
        }
        public static readonly DependencyProperty angleProperty =
            DependencyProperty.Register("angle", typeof(int), typeof(Arrow), new UIPropertyMetadata(0));
        public Visibility visible
        {
            get { return (Visibility)GetValue(visibleProperty); }
            set { SetValue(visibleProperty, value); }
        }
        public static readonly DependencyProperty visibleProperty =
            DependencyProperty.Register("visible", typeof(Visibility), typeof(Arrow), new UIPropertyMetadata(Visibility.Collapsed));
    }
}
