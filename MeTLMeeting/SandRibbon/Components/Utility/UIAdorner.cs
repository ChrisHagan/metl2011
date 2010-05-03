using System;
using System.Windows;
using System.Windows.Documents;

namespace SandRibbon.Components.Utility
{
    public class UIAdorner : Adorner
    {
        private FrameworkElement adornee;
        private FrameworkElement content;
        public Type contentType
        {
            get { return content.GetType(); }
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
