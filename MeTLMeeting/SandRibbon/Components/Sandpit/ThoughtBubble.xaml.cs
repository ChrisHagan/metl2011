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
using SandRibbon.Providers.Structure;
using SandRibbon.Utils.Connection;
using SandRibbonInterop.MeTLStanzas;
using System.Windows.Ink;
using SandRibbonObjects;

namespace SandRibbon.Components.Sandpit
{
    public partial class ThoughtBubble : UserControl
    {
        public Point position = new Point(0,0);
        public int parent;
        public string conversation;
        public List<Stroke> strokeContext;
        private bool opened = false;
        public int room;
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
            position = new Point(bounds.X + bounds.Width, bounds.Y - bounds.Height/2);
            return this;
        }
        public void enterBubble()
        {
            Commands.SneakInto.Execute(room.ToString());
        }
        private Rect getBounds()
        {
            var listX = new List<Double>();
            var listY = new List<Double>();
            var strokes = new StrokeCollection(strokeContext);
            listX.Add(strokes.GetBounds().X); 
            listX.Add(strokes.GetBounds().X + strokes.GetBounds().Width); 
            listY.Add(strokes.GetBounds().Y); 
            listY.Add(strokes.GetBounds().Y + strokes.GetBounds().Height); 
            foreach (var child in childContext)
            {
                listX.Add(InkCanvas.GetLeft(child));
                listX.Add(InkCanvas.GetLeft(child) + child.Width);
                listY.Add(InkCanvas.GetTop(child));
                listY.Add(InkCanvas.GetTop(child) + child.Height);
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
        private void move(Point point)
        {
            InkCanvas.SetLeft(this, point.X);
            InkCanvas.SetTop(this, point.Y);
        }
        private void ThoughtBubbleSpace_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(!opened)
            {
                position = new Point(0, 0);
                Width = Window.GetWindow(this).ActualWidth;
                Height = Window.GetWindow(this).ActualHeight; 
            }
            else
            {
                relocate();
                Width = 40;
                Height = 40;
            }
            move(position);
            opened = !opened;
        }
    }
}