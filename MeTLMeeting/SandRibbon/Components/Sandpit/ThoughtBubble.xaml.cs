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
using SandRibbonInterop.MeTLStanzas;
using System.Windows.Ink;

namespace SandRibbon.Components.Sandpit
{
    public partial class ThoughtBubble : UserControl
    {
        public Point position = new Point(0,0);
        public int parent;
        public List<Stroke> strokeContext;
        public List<FrameworkElement> childContext;
        public ThoughtBubble()
        {
            InitializeComponent();
            strokeContext = new List<Stroke>();
            childContext = new List<FrameworkElement>();
        }
        public ThoughtBubble relocate() 
        {
            var bounds = getBounds();
            position = new Point(bounds.X, bounds.Y);
            return this;
        }
        private Rect getBounds()
        {
            var listX = new List<Double>();
            var listY = new List<Double>();
            foreach (Stroke stroke in strokeContext)
            {
                listX.Add(stroke.GetBounds().Left);
                listY.Add(stroke.GetBounds().Top);
            }
            foreach (var child in childContext)
            {
                listX.Add(InkCanvas.GetLeft(child));
                listY.Add(InkCanvas.GetTop(child));
            }
            if (listX.Count > 0)
                return new Rect
                {
                    X = listX.Min(),
                    Y = listY.Min(),
                    Width = listX.Max() - listX.Min(),
                    Height = listY.Max() - listY.Min()
                };
            else
                return new Rect();
        }
    }
}