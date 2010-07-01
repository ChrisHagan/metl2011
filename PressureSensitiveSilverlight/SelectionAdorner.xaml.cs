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
        private InkCanvas referencedCanvas;

        public SelectionAdorner(StrokeCollection selectedStrokes, InkCanvas inkcanvas)
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
                addStylingToStroke(stroke);
                var bounds = stroke.GetBounds();
                if (firstShape)
                {
                    selectedBound.X = bounds.Left - 5;
                    selectedBound.Y = bounds.Top - 5;
                    firstShape = false;
                }
                var points = new Point[] { new Point(bounds.Left, bounds.Top), new Point(bounds.Right, bounds.Bottom) };
                foreach (Point point in points)
                    selectedBound.Union(point);
            }
            this.Width = selectedBound.Width + 10;
            this.Height = selectedBound.Height + 10;
            Canvas.SetLeft(this, selectedBound.X);
            Canvas.SetTop(this, selectedBound.Y);
        }
        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush blue = new SolidColorBrush(Colors.Blue);

        private void setColor(Rectangle sender)
        {
            if(((FrameworkElement)sender).Tag.ToString() == "move")
                return;
            sender.Fill = blue;
        }
        private void resetColors()
        {
            foreach (Rectangle border in new FrameworkElement[] { NBorder, NEBorder, EBorder, SEBorder, SBorder, SWBorder, WBorder, NWBorder })
            {
                border.Fill = red;
            }
        }

        private enum resizingMode { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, move, none };
        private resizingMode currentResizingMode = resizingMode.none;
        private Point origin;
        private Point destination;
        private void calculateMove()
        {
            var offset = new Point(destination.X - origin.X, destination.Y - origin.Y);
            var oldSelection = new Rect(Canvas.GetLeft(this), Canvas.GetTop(this), this.Width, this.Height);
            var newSelection = new Rect(oldSelection.X, oldSelection.Y, oldSelection.Width, oldSelection.Height);
            switch (currentResizingMode)
            {
                case resizingMode.North:
                    newSelection.Height -= offset.Y;
                    newSelection.Y += offset.Y;
                    break;
                case resizingMode.NorthEast:
                    newSelection.Height -= offset.Y;
                    newSelection.Y += offset.Y;
                    newSelection.Width += offset.X;
                    break;
                case resizingMode.East:
                    newSelection.Width += offset.X;
                    break;
                case resizingMode.SouthEast:
                    newSelection.Height += offset.Y;
                    newSelection.Width += offset.X;
                    break;
                case resizingMode.South:
                    newSelection.Height += offset.Y;
                    break;
                case resizingMode.SouthWest:
                    newSelection.Height += offset.Y;
                    newSelection.Width -= offset.X;
                    newSelection.X += offset.X;
                    break;
                case resizingMode.West:
                    newSelection.Width -= offset.X;
                    newSelection.X += offset.X;
                    break;
                case resizingMode.NorthWest:
                    newSelection.Height -= offset.Y;
                    newSelection.Y += offset.Y;
                    newSelection.Width -= offset.X;
                    newSelection.X += offset.X;
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
            removeStylingFromStrokes();
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
                newStroke.DrawingAttributes.Color = stroke.DrawingAttributes.Color;
                newStroke.DrawingAttributes.OutlineColor = stroke.DrawingAttributes.OutlineColor;
                newStroke.DrawingAttributes.Height = stroke.DrawingAttributes.Height;
                newStroke.DrawingAttributes.Width = stroke.DrawingAttributes.Width;
                removingStrokes.Add(stroke);
                replacementStrokes.Add(newStroke);
                referencedCanvas.Strokes.Remove(stroke);
                referencedCanvas.Strokes.Add(newStroke);
            }
            referencedCanvas.eventHandler_ReplaceStrokes(removingStrokes, replacementStrokes);
            referencedCanvas.eventHandler_ReplaceSelectedStrokes(removingStrokes, replacementStrokes);
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
        private void addStylingToStroke(Stroke stroke)
        {
            /*
            if (stroke.DrawingAttributes.OutlineColor == Colors.Transparent)
            {
                stroke.DrawingAttributes.OutlineColor = stroke.DrawingAttributes.Color;
                var invertedColor = stroke.DrawingAttributes.Color;
                invertedColor.R = Convert.ToByte(255 - (int)invertedColor.R);
                invertedColor.G = Convert.ToByte(255 - (int)invertedColor.G);
                invertedColor.B = Convert.ToByte(255 - (int)invertedColor.B);
                stroke.DrawingAttributes.Color = invertedColor;
            }
             */
        }
        public void removeStylingFromStrokes()
        {
            /*
            foreach (Stroke stroke in referencedStrokes)
            {
                if (stroke.DrawingAttributes.OutlineColor != Colors.Transparent)
                {
                    stroke.DrawingAttributes.Color = stroke.DrawingAttributes.OutlineColor;
                    stroke.DrawingAttributes.OutlineColor = Colors.Transparent;
                }
            }
            */
        }

    }
}
