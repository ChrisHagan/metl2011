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
        private Color[] colors = new Color[] { Colors.Black, 
                Colors.White, Colors.Red, Colors.Blue, Colors.Green,
                Colors.Yellow, Colors.Gray};

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
            inkcanvas.activeEditingMode = InkCanvas.inkCanvasModes.Erase;
        }
        private void Select(object sender, RoutedEventArgs e)
        {
            inkcanvas.activeEditingMode = InkCanvas.inkCanvasModes.Select;
        }
        private void ChangeColour(object sender, RoutedEventArgs e)
        {
            if (inkcanvas != null)
            {
                var colour = ((Button)sender).Background;
                var itemNumber = ColourPicker.Items.IndexOf(colour);
                var currentColor = colors[itemNumber];
                currentColor.A = inkcanvas.defaultDrawingAttributes.Color.A;
                inkcanvas.defaultDrawingAttributes.Color = currentColor;
                inkcanvas.activeEditingMode = InkCanvas.inkCanvasModes.Draw;
            }
        }
        private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (inkcanvas != null)
            {
                var currentAlpha = Convert.ToByte(e.NewValue);
                var newColor = inkcanvas.defaultDrawingAttributes.Color;
                newColor.A = currentAlpha;
                inkcanvas.defaultDrawingAttributes.Color = newColor;
            }
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (inkcanvas != null)
            {
                inkcanvas.defaultDrawingAttributes.Height = e.NewValue;
                inkcanvas.defaultDrawingAttributes.Width = e.NewValue;
            }
        }
        private void inkcanvas_strokesReplaced(object sender, StrokesChangedEventArgs e)
        {
            /* if (e.addedStrokes == null) return;
               string stringMessage = "strokes collected:";
               foreach (Stroke stroke in e.addedStrokes)
                   {
                   stringMessage += " stroke:
                   foreach (StylusPoint sp in stroke.StylusPoints)
                       stringMessage += "(" + sp.X + "," + sp.Y + "," + sp.PressureFactor + "),";
               }
               MessageBox.Show(stringMessage);
           */
        }
        private void inkcanvas_strokeCollected(object sender, StrokeAddedEventArgs e)
        {
            /*   if (e.stroke == null) return;
               string stringMessage = "stroke collected:";
                   foreach (StylusPoint sp in e.stroke.StylusPoints)
                       stringMessage += "(" + sp.X + "," + sp.Y + "," + sp.PressureFactor + "),";
               MessageBox.Show(stringMessage);
           */
        }
        private void inkcanvas_selectedStrokesChanged(object sender, StrokesChangedEventArgs e)
        {
            /*   if (e.addedStrokes == null) return;
               string stringMessage = "strokes collected:";
               foreach (Stroke stroke in e.addedStrokes)
                   {
                   stringMessage += " stroke:
                   foreach (StylusPoint sp in stroke.StylusPoints)
                       stringMessage += "(" + sp.X + "," + sp.Y + "," + sp.PressureFactor + "),";
               }
               MessageBox.Show(stringMessage);
           */
        }
    }
}
