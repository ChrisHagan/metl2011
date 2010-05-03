using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Components.SimpleImpl
{
    public partial class SimpleToolBox : UserControl
    {
        public SimpleToolBox()
        {
            InitializeComponent();
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(updateToolBox));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(setDefaults));
            Commands.SetDrawingAttributes.RegisterCommand(new DelegateCommand<object>(setDefaults));
        }
        private void setDefaults(object obj)
        {
            Draw.IsChecked = true;
            Commands.SetInkCanvasMode.Execute("Ink");
        }
        private void CreateAutoShape(object sender, RoutedEventArgs e)
        {
            PathGeometry sendingPath = new PathGeometry(); 
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(47.7778, 48.6667);
            pathFigure.IsClosed = true;
            LineSegment lineSegment1 = new LineSegment();
            lineSegment1.Point = new Point(198, 48.6667);
            pathFigure.Segments.Add(lineSegment1);
            LineSegment lineSegment2 = new LineSegment();
            lineSegment2.Point = new Point(198, 102);
            pathFigure.Segments.Add(lineSegment2);
            BezierSegment bezierSegment1 = new BezierSegment();
            bezierSegment1.Point1 = new Point(174.889, 91.3334);
            bezierSegment1.Point2 = new Point(157.111, 79.7778);
            bezierSegment1.Point3 = new Point(110.889, 114.444);
            pathFigure.Segments.Add(bezierSegment1);
            BezierSegment bezierSegment2 = new BezierSegment();
            bezierSegment2.Point1 = new Point(64.667, 149.111);
            bezierSegment2.Point2 = new Point(58.4444, 130.444);
            bezierSegment2.Point3 = new Point(47.7778, 118.889);
            pathFigure.Segments.Add(bezierSegment2);
            sendingPath.Figures.Add(pathFigure);

            PathGeometry pathGeometry = new PathGeometry();
            if (sendingPath.FillRule != null)
                pathGeometry.FillRule = sendingPath.FillRule;
            else pathGeometry.FillRule = FillRule.Nonzero;
            pathGeometry.Figures = sendingPath.Figures;

            var newThumb = new SandRibbonInterop.AutoShape();
            var safePathGeometry = (Geometry)(new GeometryConverter().ConvertFromString(pathGeometry.ToString()));
            var safePathData = new PathGeometry();
            safePathData.AddGeometry(safePathGeometry);
            newThumb.PathData = safePathData;
            newThumb.Background = Brushes.LightGreen;
            newThumb.Foreground = Brushes.Green;
            newThumb.StrokeThickness = 5;
            newThumb.Height = 100;
            newThumb.Width = 100;
            
            Commands.AddAutoShape.Execute(newThumb);
        }
        private void updateToolBox(string layer)
        {
            hideAll();
            if (layer == "Text")
                TextOptions.Visibility = Visibility.Visible;
            else if(layer == "Insert")
                ImageOptions.Visibility = Visibility.Visible;
            else
                InkOptions.Visibility = Visibility.Visible;
        }
        private void hideAll()
        {
            InkOptions.Visibility = Visibility.Collapsed;
            TextOptions.Visibility = Visibility.Collapsed;
            ImageOptions.Visibility = Visibility.Collapsed;
        }
    }
}
