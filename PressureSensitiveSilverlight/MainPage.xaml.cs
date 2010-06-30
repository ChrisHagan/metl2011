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
    public partial class MainPage : UserControl
    {
        private System.Windows.Ink.Stroke newStroke;
        private double PressureMultiplier = 4;
        private Color[] colors = new Color[] { Colors.Black, 
                Colors.White, Colors.Red, Colors.Blue, Colors.Green,
                Colors.Yellow, Colors.Gray};
        private Color currentColor = Colors.Black;
        private Byte currentAlpha;
        private double lastPressure;
        private enum inkCanvasModes { Draw, Select, Erase };
        private inkCanvasModes inkCanvasMode = inkCanvasModes.Draw;
        private StrokeCollection selectedStrokes = new StrokeCollection();
        private bool mouseDown = false;

        public MainPage()
        {
            InitializeComponent();
            setupColourPicker();
        }
        private void setupColourPicker()
        {
            foreach (Color color in colors)
            {
                ColourPicker.Items.Add(new SolidColorBrush(color));
            }
        }
        private void Erase(object sender, RoutedEventArgs e)
        {
            inkCanvasMode = inkCanvasModes.Erase;
        }
        private void Select(object sender, RoutedEventArgs e)
        {
            inkCanvasMode = inkCanvasModes.Select;
        }
        private void ChangeColour(object sender, RoutedEventArgs e)
        {
            var colour = ((Button)sender).Background;
            var itemNumber = ColourPicker.Items.IndexOf(colour);
            currentColor = colors[itemNumber];
            currentColor.A = currentAlpha;
            inkCanvasMode = inkCanvasModes.Draw;
        }
        private void InkPresenter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            inkcanvas.CaptureMouse();
            PenControls.Opacity = 0.2;
            mouseDown = true;
            if (e.StylusDevice.Inverted)
                inkCanvasMode = inkCanvasModes.Erase;

            switch (inkCanvasMode)
            {
                case inkCanvasModes.Draw:
                    addNewStroke(e.StylusDevice.GetStylusPoints(inkcanvas));
                    break;
                case inkCanvasModes.Erase:
                    break;
                case inkCanvasModes.Select:
                    selectedStrokes.Clear();
                    adornerLayer.Children.Clear();
                    break;
            }
        }
        private void InkPresenter_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            switch (inkCanvasMode)
            {
                case inkCanvasModes.Draw:
                    removeStylingFromStrokes();
                    if (newStroke == null) return;
                    if (nearEnough(e.StylusDevice.GetStylusPoints(inkcanvas)[0].PressureFactor, newStroke.StylusPoints[0].PressureFactor, 0.001))
                    {
                        continueStroke(e.StylusDevice.GetStylusPoints(inkcanvas));
                    }
                    else
                        addNewStroke(e.StylusDevice.GetStylusPoints(inkcanvas));
                    break;
                case inkCanvasModes.Erase:
                    tryEraseStroke(e.StylusDevice.GetStylusPoints(inkcanvas));
                    break;
                case inkCanvasModes.Select:
                    trySelectStroke(e.StylusDevice.GetStylusPoints(inkcanvas));
                    break;
            }
        }
        private void InkPresenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!mouseDown) return;
            switch (inkCanvasMode)
            {
                case inkCanvasModes.Draw:
                    completeStroke();
                    break;
                case inkCanvasModes.Erase:
                    break;
                case inkCanvasModes.Select:
                    break;
            }
            mouseDown = false;
            inkcanvas.ReleaseMouseCapture();
            PenControls.Opacity = 1;
        }
        private void addStylingToStroke(Stroke stroke)
        {
            var invertedColor = stroke.DrawingAttributes.Color;
            invertedColor.R = Convert.ToByte(255 - (int)invertedColor.R);
            invertedColor.G = Convert.ToByte(255 - (int)invertedColor.G);
            invertedColor.B = Convert.ToByte(255 - (int)invertedColor.B);
            stroke.DrawingAttributes.OutlineColor = invertedColor;
        }
        private void removeStylingFromStrokes()
        {
            foreach (Stroke stroke in selectedStrokes)
            {
                stroke.DrawingAttributes.OutlineColor = Colors.Transparent;
            }
            selectedStrokes.Clear();
        }
        private void createAdorner(StrokeCollection selectedStrokes)
        {
            adornerLayer.Children.Clear();
                   var selectionAdorner = new SelectionAdorner(selectedStrokes, inkcanvas);
                adornerLayer.Children.Add(selectionAdorner);
        }
        private void trySelectStroke(StylusPointCollection spc)
        {
            foreach (Stroke stroke in inkcanvas.Strokes)
            {
                if (stroke.HitTest(spc))
                {
                    if (!selectedStrokes.Contains(stroke))
                    selectedStrokes.Add(stroke);
                    createAdorner(selectedStrokes);
                }
            }
            foreach (Stroke stroke in selectedStrokes)
            {
                addStylingToStroke(stroke);
            }
        }
        private void tryEraseStroke(StylusPointCollection spc)
        {
            removeStylingFromStrokes();
            StrokeCollection strokesToDelete = new StrokeCollection();
            foreach (Stroke stroke in inkcanvas.Strokes)
            {
                if (stroke.HitTest(spc))
                    strokesToDelete.Add(stroke);
            }
            foreach (Stroke stroke in strokesToDelete)
            {
                inkcanvas.Strokes.Remove(stroke);
            }
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
            inkcanvas.Strokes.Add(newStroke);
            newStroke.StylusPoints.Add(spc);
        }
        private void continueStroke(StylusPointCollection spc)
        {
            setDrawingAttributes(newStroke);
            newStroke.StylusPoints.Add(spc);
        }
        private void setDrawingAttributes(Stroke newStroke)
        {
            newStroke.DrawingAttributes.Color = currentColor;
            if (newStroke.StylusPoints.Count > 0)
            {
                lastPressure = newStroke.StylusPoints.Average(s => s.PressureFactor) * PressureMultiplier;
            }
            newStroke.DrawingAttributes.Height = lastPressure;
            newStroke.DrawingAttributes.Width = lastPressure;
        }
        private void completeStroke()
        {
            if (newStroke != null)
            {
                setDrawingAttributes(newStroke);
                newStroke = null;
            }
        }
        private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            currentAlpha = Convert.ToByte(e.NewValue);
            var newColor = currentColor;
            newColor.A = currentAlpha;
            currentColor = newColor;
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PressureMultiplier = e.NewValue;
        }
    }
}
