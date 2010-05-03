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

namespace ThoughtIdeas
{
    public partial class Thought : UserControl
    {
        public Thought()
        {
            InitializeComponent();
        }
        private void pushpinClicked(object sender, RoutedEventArgs e)
        {
            thoughtControls.Visibility = thoughtControls.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    public class ThoughtAdorner : Adorner
    {
        private Thought adorner;
        private UIElement adornee;
        private double x;
        private double y;
        public ThoughtAdorner(double x, double y, UIElement adornee)
            : base(adornee)
        {
            this.adorner = new Thought();
            this.x = x + 20;
            this.y = y - 20;
            AddVisualChild(adorner);
        }
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            adorner.Arrange(new Rect(new Point(x,y), adorner.DesiredSize));
            return finalSize;
        }
        protected override Visual GetVisualChild(int index)
        {
            return adorner;
        }
    }
}