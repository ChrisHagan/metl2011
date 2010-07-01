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
    #region eventHandlers
    public delegate void StrokeCollectedEventHandler(object sender, StrokeAddedEventArgs e);
    public class StrokeAddedEventArgs : EventArgs
    {
        public Stroke stroke;
        public StrokeAddedEventArgs(Stroke newStroke)
        {
            stroke = newStroke;
        }
    }
    public delegate void SelectedStrokesChangedEventHandler(object sender, StrokesChangedEventArgs e);
    public delegate void StrokesChangedEventHandler(object sender, StrokesChangedEventArgs e);
    public class StrokesChangedEventArgs : EventArgs
    {
        public StrokeCollection addedStrokes;
        public StrokeCollection removedStrokes;
        public StrokesChangedEventArgs(StrokeCollection newlyRemovedStrokes, StrokeCollection newlyAddedStrokes)
        {
            addedStrokes = newlyAddedStrokes;
            removedStrokes = newlyRemovedStrokes;
        }
    }
    #endregion
    public partial class InkCanvas : UserControl
    {
        public InkCanvas()
        {
            InitializeComponent();
        }
        public StrokeCollection Strokes
        {
            get { return inkLayer.Strokes; }
            set { inkLayer.Strokes = value; }
        }
        public UIElementCollection Children
        {
            get { return inkLayer.Children; }
            set
            {
                inkLayer.Children.Clear();
                foreach (UIElement UIE in value)
                {
                    inkLayer.Children.Add(UIE);
                }
            }
        }
        #region inkCanvasEvents
        public event StrokesChangedEventHandler StrokesChanged;
        public event StrokeCollectedEventHandler StrokeCollected;
        public event SelectedStrokesChangedEventHandler SelectedStrokesChanged;

        protected virtual void OnSelectedStrokesChanged(StrokesChangedEventArgs e)
        {
            SelectedStrokesChanged(this, e);
        }
        protected virtual void OnStrokesChanged(StrokesChangedEventArgs e)
        {
            StrokesChanged(this, e);
        }
        protected virtual void OnStrokeCollected(StrokeAddedEventArgs e)
        {
            StrokeCollected(this, e);
        }
        public void eventHandler_ReplaceSelectedStrokes(StrokeCollection oldStrokes, StrokeCollection newStrokes)
        {
            OnSelectedStrokesChanged(new StrokesChangedEventArgs(oldStrokes,newStrokes));
        }
        public void eventHandler_ReplaceStrokes(StrokeCollection oldStrokes, StrokeCollection newStrokes)
        {
            OnStrokesChanged(new StrokesChangedEventArgs(oldStrokes, newStrokes));
        }
        public void eventHandler_AddStroke(Stroke newStroke)
        {
            OnStrokeCollected(new StrokeAddedEventArgs(newStroke));
        }
        #endregion
        private System.Windows.Ink.Stroke newStroke;
        private Color[] colors = new Color[] { Colors.Black, 
                Colors.White, Colors.Red, Colors.Blue, Colors.Green,
                Colors.Yellow, Colors.Gray};
        private double lastPressure;
        public enum inkCanvasModes { Draw, Select, Erase, None };
        public inkCanvasModes activeEditingMode = inkCanvasModes.Draw;
        private StrokeCollection selectedStrokes = new StrokeCollection();
        private bool mouseDown = false;
        public DrawingAttributes defaultDrawingAttributes = new DrawingAttributes() { OutlineColor = Colors.Transparent, Color = Colors.Black, Height = 2, Width = 2 };
        private SelectionAdorner currentSelectionAdorner;

        private Polygon currentSelectionShape = new Polygon()
        {
            StrokeDashArray = new DoubleCollection { 2, 2 },
            Fill = new SolidColorBrush(new Color { A = (byte)20, R = (byte)128, G = (byte)0, B = (byte)0 }),
            Stroke = new SolidColorBrush(new Color { A = (byte)180, R = (byte)255, G = (byte)0, B = (byte)0 })
        };

        private void InkPresenter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            inkLayer.CaptureMouse();
            selectedStrokes.Clear();
            clearAdorners();
            mouseDown = true;
            if (e.StylusDevice.Inverted)
                activeEditingMode = inkCanvasModes.Erase;

            switch (activeEditingMode)
            {
                case inkCanvasModes.Draw:
                    addNewStroke(e.StylusDevice.GetStylusPoints(inkLayer));
                    break;
                case inkCanvasModes.Erase:
                    break;
                case inkCanvasModes.Select:
                    adornerLayer.Children.Add(currentSelectionShape);
                    break;
            }
        }
        private void InkPresenter_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            switch (activeEditingMode)
            {
                case inkCanvasModes.Draw:
                    if (newStroke == null) return;
                    if (nearEnough(e.StylusDevice.GetStylusPoints(inkLayer)[0].PressureFactor, newStroke.StylusPoints[0].PressureFactor, 0.001))
                    {
                        continueStroke(e.StylusDevice.GetStylusPoints(inkLayer));
                    }
                    else
                        addNewStroke(e.StylusDevice.GetStylusPoints(inkLayer));
                    break;
                case inkCanvasModes.Erase:
                    tryEraseStroke(e.StylusDevice.GetStylusPoints(inkLayer));
                    break;
                case inkCanvasModes.Select:
                    trySelectStroke(e.StylusDevice.GetStylusPoints(inkLayer));
                    break;
            }
        }
        private void InkPresenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!mouseDown) return;
            switch (activeEditingMode)
            {
                case inkCanvasModes.Draw:
                    completeStroke();
                    break;
                case inkCanvasModes.Erase:
                    break;
                case inkCanvasModes.Select:
                    if (currentSelectionShape != null)
                        adornerLayer.Children.Remove(currentSelectionShape);
                    currentSelectionShape.Points.Clear();
                    break;
            }
            mouseDown = false;
            inkLayer.ReleaseMouseCapture();
        }
        private void createAdorner(StrokeCollection selectedStrokes)
        {
            clearAdorners();
            currentSelectionAdorner = null;
            currentSelectionAdorner = new SelectionAdorner(selectedStrokes, this);
            adornerLayer.Children.Add(currentSelectionAdorner);
        }
        private void clearAdorners()
        {
            adornerLayer.Children.Clear();
            if (currentSelectionAdorner != null)
                currentSelectionAdorner.removeStylingFromStrokes();
            if (currentSelectionShape.Points.Count > 0)
                adornerLayer.Children.Add(currentSelectionShape);
        }
        private bool polyHitTest(Point[] vertices, Point testPoint)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = vertices.Count() - 1; i < vertices.Count(); j = i++)
            {
                if (((vertices[i].Y > testPoint.Y) != (vertices[j].Y > testPoint.Y)) &&
                 (testPoint.X < (vertices[j].X - vertices[i].X) * (testPoint.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                    c = !c;
            }
            return c;
        }
        private void trySelectStrokeByIntercept(StylusPointCollection spc)
        {
            foreach (Stroke stroke in inkLayer.Strokes)
            {
                if (stroke.HitTest(spc))
                {
                    if (!selectedStrokes.Contains(stroke))
                        selectedStrokes.Add(stroke);
                    createAdorner(selectedStrokes);
                }
            }
        }
        //These are just for selection optimization
        private int tempIndex = 0;
        private int skipFactor = 5;

        private void trySelectStroke(StylusPointCollection spc)
        {
            StrokeCollection currentSelectedStrokes = selectedStrokes;
            tempIndex++;
            if (tempIndex != skipFactor) return;
            tempIndex = 0;

            foreach (StylusPoint sp in spc)
                currentSelectionShape.Points.Add(new Point(sp.X, sp.Y));
            var currentSelectionShapeLeft = currentSelectionShape.Points.Aggregate((a, b) => { if (a.X < b.X)return a; else return b; }).X;
            var currentSelectionShapeTop = currentSelectionShape.Points.Aggregate((a, b) => { if (a.Y < b.Y)return a; else return b; }).Y;
            var currentSelectionShapeRight = currentSelectionShape.Points.Aggregate((a, b) => { if (a.X > b.X) return a; else return b; }).X;
            var currentSelectionShapeBottom = currentSelectionShape.Points.Aggregate((a, b) => { if (a.Y > b.Y) return a; else return b; }).Y;
            var currentShapeBounds = new Rect(new Point(currentSelectionShapeLeft, currentSelectionShapeTop), new Point(currentSelectionShapeRight, currentSelectionShapeBottom));
            foreach (Stroke stroke in inkLayer.Strokes)
            {
                if (!selectedStrokes.Contains(stroke))
                {
                    if (isStrokeContainedByPolygon(stroke, currentShapeBounds))
                    {
                        selectedStrokes.Add(stroke);
                    }
                }
                else
                {
                    if (!isStrokeContainedByPolygon(stroke, currentShapeBounds))
                        selectedStrokes.Remove(stroke);
                }
                if (selectedStrokes.Count > 0)
                    createAdorner(selectedStrokes);
                else
                    clearAdorners();
                if (!currentSelectedStrokes.All(t => selectedStrokes.Contains(t)) && selectedStrokes.All(t => currentSelectedStrokes.Contains(t)))
                    eventHandler_ReplaceSelectedStrokes(currentSelectedStrokes, selectedStrokes);
            }
        }
        private bool isStrokeContainedByPolygon(Stroke stroke, Rect currentShapeBounds)
        {
            bool isContained = false;
            bool strokeContainedinBounds = false;
            var sBounds = stroke.GetBounds();
            foreach (Point p in new[]{
                    new Point(sBounds.Left,sBounds.Top),
                    new Point(sBounds.Right,sBounds.Top),
                    new Point(sBounds.Left,sBounds.Bottom),
                    new Point(sBounds.Right,sBounds.Bottom)})
                if (!strokeContainedinBounds && currentShapeBounds.Contains(p))
                {
                    strokeContainedinBounds = true;
                }
            if (strokeContainedinBounds)
            {
                var hitPointCount = 0;
                int speedFactor = 1;
                int tempIndex = 0;
                var hitPointThreshold = (stroke.StylusPoints.Count() / speedFactor) * .75;
                foreach (StylusPoint sp in stroke.StylusPoints)
                {
                    tempIndex++;
                    if (tempIndex == speedFactor)
                    {
                        tempIndex = 0;
                        var spPoint = new Point(sp.X, sp.Y);
                        if (polyHitTest(currentSelectionShape.Points.ToArray(), spPoint))
                        {
                            hitPointCount++;
                        }
                    }
                }
                if (hitPointCount > hitPointThreshold)
                {
                    isContained = true;
                }
            }
            return isContained;
        }
        private void tryEraseStroke(StylusPointCollection spc)
        {
            StrokeCollection strokesToDelete = new StrokeCollection();
            foreach (Stroke stroke in inkLayer.Strokes)
            {
                if (stroke.HitTest(spc))
                    strokesToDelete.Add(stroke);
            }
            foreach (Stroke stroke in strokesToDelete)
            {
                inkLayer.Strokes.Remove(stroke);
            }
            eventHandler_ReplaceStrokes(strokesToDelete, null);
        }
        private bool nearEnough(double a, double b, double threshold)
        {
            double max = Math.Max(a, b);
            double min;
            if (max == a)
                min = b;
            else
                min = a;
            if ((max - min) < threshold)
                return true;
            return false;
        }
        private void addNewStroke(StylusPointCollection spc)
        {
            if (newStroke != null)
                newStroke.StylusPoints.Add(spc);
            completeStroke();
            newStroke = new System.Windows.Ink.Stroke();
            setDrawingAttributes(newStroke);
            inkLayer.Strokes.Add(newStroke);
            newStroke.StylusPoints.Add(spc);
        }
        private void continueStroke(StylusPointCollection spc)
        {
            setDrawingAttributes(newStroke);
            newStroke.StylusPoints.Add(spc);
        }
        private void setDrawingAttributes(Stroke newStroke)
        {
            newStroke.DrawingAttributes.Color = defaultDrawingAttributes.Color;
            if (newStroke.StylusPoints.Count > 0)
            {
                lastPressure = newStroke.StylusPoints.Average(s => s.PressureFactor) * defaultDrawingAttributes.Height;
            }
            newStroke.DrawingAttributes.Height = lastPressure;
            newStroke.DrawingAttributes.Width = lastPressure;
        }
        private void completeStroke()
        {
            if (newStroke != null)
            {
                eventHandler_AddStroke(newStroke);
                setDrawingAttributes(newStroke);
                newStroke = null;
            }
        }
    }
}
