using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Ink;

namespace SilverlightApplication1
{
    public partial class SelectionAdorner : UserControl
    {
        private StrokeCollection referencedStrokes = new StrokeCollection();
        private StrokeCollection replacementStrokes = new StrokeCollection();
        private InkPresenter referencedCanvas;

        public SelectionAdorner(StrokeCollection selectedStrokes, InkPresenter inkcanvas)
        {
            InitializeComponent();
            referencedStrokes = selectedStrokes;
            referencedCanvas = inkcanvas;
            setupSelectionAdorner();
        }
        private void setupSelectionAdorner()
        {
            var selectedBound = new Rect();
            bool firstShape = true;
            foreach (Stroke stroke in referencedStrokes)
            {
                var bounds = stroke.GetBounds();
                if (firstShape)
                {
                    selectedBound.X = bounds.Left;
                    selectedBound.Y = bounds.Top;
                    firstShape = false;
                }
                var points = new Point[] { new Point(bounds.Left, bounds.Top), new Point(bounds.Right, bounds.Bottom) };
                foreach (Point point in points)
                    selectedBound.Union(point);
            }
            this.Width = selectedBound.Width;
            this.Height = selectedBound.Height;
            Canvas.SetLeft(this, selectedBound.X);
            Canvas.SetTop(this, selectedBound.Y);
        }
        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush blue = new SolidColorBrush(Colors.Blue);

        private void setColor(Rectangle sender)
        {
            if (sender == CenterBorder)
                return;
            sender.Fill = blue;
        }
        private void resetColors()
        {
            foreach (Rectangle border in new FrameworkElement[] { UpBorder, DownBorder, RightBorder, LeftBorder })
            {
                border.Fill = red;
            }
        }

        private enum resizingMode { up, left, down, right, move, none };
        private resizingMode currentResizingMode = resizingMode.none;
        private Point origin;
        private Point destination;
        private void calculateMove()
        {
            var offset = new Point(destination.X - origin.X, destination.Y - origin.Y);
            var oldSelection = new Rect(Canvas.GetLeft(this), Canvas.GetTop(this), this.Width, this.Height);
            var newSelection = new Rect(oldSelection.X,oldSelection.Y,oldSelection.Width,oldSelection.Height);
            switch (currentResizingMode)
            {
                case resizingMode.left:
                    newSelection.Width -= offset.X;
                    newSelection.X += offset.X;
                    break;
                case resizingMode.up:
                    newSelection.Height -= offset.Y;
                    newSelection.Y += offset.Y;
                    break;
                case resizingMode.right:
                    newSelection.Width += offset.X;
                    break;
                case resizingMode.down:
                    newSelection.Height += offset.Y;
                    break;
                case resizingMode.move:
                    newSelection.X += offset.X;
                    newSelection.Y += offset.Y;
                    break;
            }
            resizeSelectedObjects(oldSelection, newSelection);
        }
        private void resizeSelectedObjects(Rect oldSelection, Rect newSelection)
        {
            var removingStrokes = new StrokeCollection();
            replacementStrokes.Clear();
            double WidthFactor = 1;
            double HeightFactor = 1;
            if (oldSelection.Width != newSelection.Width)
                WidthFactor = newSelection.Width / oldSelection.Width;
            if (oldSelection.Height != newSelection.Height)
                HeightFactor = newSelection.Height / oldSelection.Height;
            foreach (Stroke stroke in referencedStrokes)
            {
                var newSpc = new StylusPointCollection();
                foreach (StylusPoint sp in stroke.StylusPoints)
                {
                    var newSp = new StylusPoint();
                    newSp.X = ((sp.X - oldSelection.X) * WidthFactor) + newSelection.X;
                    newSp.Y = ((sp.Y - oldSelection.Y) * HeightFactor) + newSelection.Y;
                    newSp.PressureFactor = sp.PressureFactor;
                    newSpc.Add(newSp);
                }
                var newStroke = new Stroke(newSpc);
                newStroke.DrawingAttributes.Height = stroke.DrawingAttributes.Height;
                newStroke.DrawingAttributes.Width = stroke.DrawingAttributes.Width;
                removingStrokes.Add(stroke);
                replacementStrokes.Add(newStroke);
                referencedCanvas.Strokes.Remove(stroke);
                referencedCanvas.Strokes.Add(newStroke);
            }
            referencedStrokes.Clear();
            foreach (Stroke stroke in replacementStrokes)
                referencedStrokes.Add(stroke);
            setupSelectionAdorner();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((FrameworkElement)sender).CaptureMouse();
            currentResizingMode = (resizingMode)Enum.Parse(((Type)typeof(resizingMode)), ((FrameworkElement)sender).Tag.ToString(), true);
            setColor((Rectangle)sender);
            origin = new Point(e.StylusDevice.GetStylusPoints((UIElement)this.Parent)[0].X,
                    e.StylusDevice.GetStylusPoints((UIElement)this.Parent)[0].Y);
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentResizingMode != resizingMode.none)
            {
                var currentPoint = new Point(e.StylusDevice.GetStylusPoints((UIElement)this.Parent)[0].X,
                    e.StylusDevice.GetStylusPoints((UIElement)this.Parent)[0].Y);
                destination = currentPoint;
                calculateMove();
                origin = currentPoint;
            }
        }
        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((FrameworkElement)sender).ReleaseMouseCapture();
            resetColors();
            currentResizingMode = resizingMode.none;
        }
    }
}
