using System;
using System.Windows;
using System.Windows.Documents;
using WpfCanvas = System.Windows.Controls.Canvas;
using System.Windows.Media;

namespace SandRibbon.Components.Utility
{
    public class UIAdorner : Adorner
    {
        private FrameworkElement adornee;
        public FrameworkElement content;
        public Type contentType
        {
            get { return content.GetType(); }
        }
        public static UIAdorner InCanvas(FrameworkElement adornee, FrameworkElement content, Point initialPosition) 
        {
            var canvas = new WpfCanvas {};
            var adorner = new UIAdorner(adornee, canvas);
            canvas.Children.Add(content);
            WpfCanvas.SetTop(content, initialPosition.Y);
            WpfCanvas.SetLeft(content, initialPosition.X);
            return adorner;
        }
        public UIAdorner(FrameworkElement adornee, FrameworkElement content)
            : base(adornee)
        {
            this.adornee = adornee;
            this.content = content;
            AddVisualChild(this.content);
        }
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            content.Arrange(new Rect { Height = adornee.ActualHeight, Width = adornee.ActualWidth });
            return finalSize;
        }
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            return content;
        }
    }
}
