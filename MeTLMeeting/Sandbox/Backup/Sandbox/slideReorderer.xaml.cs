using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbon;
using SandRibbon.Providers.Structure;
using SandRibbonObjects;
using Path=System.Windows.Shapes.Path;

namespace Sandbox
{
    /// <summary>
    /// Interaction logic for slideReorderer.xaml
    /// </summary>
    public partial class slideReorderer : UserControl
    {
        private int X = 0;
        private int Y = 100;
        private const int SIZE = 40;
        public slideReorderer()
        {
            InitializeComponent();
            var title = "97400";
            var details = ConversationDetailsProviderFactory.Provider.DetailsOf(title);
            int position = 0;
            foreach(var slide in details.Slides)
            {

                //need to thumbnail, cannot easily do outside metl
                var ellipse = new Ellipse();
                var thumb = new MetLThumb 
                        {
                            startX = X,
                            startY = Y,
                            slideId = slide.id,
                            Background = Brushes.Blue,
                            Width = SIZE,
                            Height = SIZE,
                            position = position
                        };
                position++;
                thumb.ToolTip = slide.id.ToString();
                
                thumb.DragStarted += dragStarted;
                thumb.DragCompleted += dragCompleted;
                thumb.DragDelta += dragDelta;
                Canvas.SetLeft(thumb, X);
                Canvas.SetTop(thumb, Y);
                X += 80;
                slides.Children.Add(thumb);
            }
        }

        private void dragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (MetLThumb) sender;
            Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange );
            Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange );
        }

        private void dragCompleted(object sender, DragCompletedEventArgs e)
        {
            var currentThumb = (MetLThumb) sender;
            moveSlide(currentThumb);
            currentThumb.Background = Brushes.Blue;
        }

        private void moveSlide(MetLThumb currentThumb)
        {
            var currentX = Canvas.GetLeft(currentThumb);
            var currentY = Canvas.GetTop(currentThumb);
            var thumbRect = new Rect(new Point(currentX, currentY), new Size(SIZE, SIZE));
            foreach(var element in slides.Children)
            {
                if(element.GetType() == typeof(MetLThumb))
                {
                    var thumb = (MetLThumb) element;
                    var x = Canvas.GetLeft(thumb);
                    var y = Canvas.GetTop(thumb);
                    var rect = new Rect(new Point(x, y), new Size(SIZE, SIZE));
                    if(rect.IntersectsWith(thumbRect) && (thumb.slideId != currentThumb.slideId))
                    {
                        var point1 = new Point();
                        var currentIndex = currentThumb.position;
                        var thumbIndex = thumb.position;
                        Canvas.SetLeft(currentThumb, x);
                        point1.X = x + (SIZE/2);
                        currentX = currentThumb.startX;
                        currentThumb.startX = x;
                        Canvas.SetTop(currentThumb, y);
                        point1.Y = y;
                        currentY = currentThumb.startY;
                        currentThumb.startY = y;
                        currentThumb.position = thumbIndex;

                        var point2 = new Point();
                        Canvas.SetLeft(thumb, currentX);
                        point2.X = currentX+ (SIZE/2);
                        thumb.startX = currentX;
                        Canvas.SetRight(thumb, currentY);
                        point2.Y = currentY;
                        thumb.startY = currentY;
                        thumb.position = currentIndex;

                        double maxX, minX;
                        if(point1.X > point2.X)
                        {
                            maxX = point1.X;
                            minX = point2.X;
                        }
                        else
                        {
                            maxX = point2.X;
                            minX = point1.X;
                        }
                        Path myPath = createArc(point1,0, minX, maxX, SweepDirection.Clockwise);


                        slides.Children.Add(myPath);
                        slides.Children.Add(new Arrow
                                                {
                                                    X1 = maxX,
                                                    Y1 = point1.Y,
                                                    Direction = "DOWN",
                                                    Stroke = Brushes.Black,
                                                    StrokeThickness = 2,
                                                    HeadHeight = 5,
                                                    HeadWidth = 5
                                                });


                        myPath = createArc(point1,SIZE, minX, maxX, SweepDirection.Counterclockwise);
                        slides.Children.Add(myPath);
                        slides.Children.Add(new Arrow
                                                {
                                                    X1 = minX,
                                                    Y1 = point1.Y + SIZE,
                                                    Direction = "UP",
                                                    Stroke = Brushes.Black,
                                                    StrokeThickness = 2,
                                                    HeadHeight = 5,
                                                    HeadWidth = 5
                                                });

                        return;
                    }
                }
            }
            Canvas.SetLeft(currentThumb, currentThumb.startX);
            Canvas.SetTop(currentThumb, currentThumb.startY);
        }

        private Path createArc(Point point1, int offset, double minX, double maxX, SweepDirection direction)
        {
            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = new Point(minX, point1.Y + offset);
            myPathFigure.Segments.Add(
                new ArcSegment(new Point(maxX, point1.Y + offset), new Size(10, 10),0, true, direction, true )
                );
            // Create a PathGeometry to contain the figure.
            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures.Add(myPathFigure);

            // Display the PathGeometry. 
            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;
            myPath.Data = myPathGeometry;
            return myPath;
        }

        private void dragStarted(object sender, DragStartedEventArgs e)
        {

                ((MetLThumb)sender).Background = Brushes.Orange;
        }
    }
    public class MetLThumb: Thumb
    {
        public double startX;
        public double startY;
        public int slideId;
        public int position;

    }

    public sealed class Arrow : Shape
	{
		#region Dependency Properties

		public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty HeadWidthProperty = DependencyProperty.Register("HeadWidth", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty HeadHeightProperty = DependencyProperty.Register("HeadHeight", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure)); 
		public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(String), typeof(Arrow), new PropertyMetadata("UP")); 

		#endregion

		#region CLR Properties

		[TypeConverter(typeof(LengthConverter))]
		public double X1
		{
			get { return (double)base.GetValue(X1Property); }
			set { base.SetValue(X1Property, value); }
		}
        [TypeConverter(typeof(LengthConverter))]
        public double Y1
        {
            get { return (double)base.GetValue(Y1Property); }
            set { base.SetValue(Y1Property, value); }
        }
		[TypeConverter(typeof(String))]
		public string Direction 
		{
			get { return (String)base.GetValue(DirectionProperty); }
			set { base.SetValue(DirectionProperty, value); }
		}
		[TypeConverter(typeof(LengthConverter))]
		public double HeadWidth
		{
			get { return (double)base.GetValue(HeadWidthProperty); }
			set { base.SetValue(HeadWidthProperty, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double HeadHeight
		{
			get { return (double)base.GetValue(HeadHeightProperty); }
			set { base.SetValue(HeadHeightProperty, value); }
		}

		#endregion

		#region Overrides

		protected override Geometry DefiningGeometry
		{
			get
			{
				// Create a StreamGeometry for describing the shape
				StreamGeometry geometry = new StreamGeometry();
				geometry.FillRule = FillRule.EvenOdd;

				using (StreamGeometryContext context = geometry.Open())
				{
					InternalDrawArrowGeometry(context);
				}

				// Freeze the geometry for performance benefits
				geometry.Freeze();

				return geometry;
			}
		}		

		#endregion

		#region Privates

		private void InternalDrawArrowGeometry(StreamGeometryContext context)
		{
			Point pt1 = new Point(X1, this.Y1);
		    Point pt3, pt2;
            if (Direction == "UP")
            {
                pt3 = new Point(
                    X1 + (HeadWidth),
                    Y1 + (HeadHeight));

                pt2 = new Point(
                    X1 - (HeadWidth),
                    Y1 + (HeadHeight));
            }
            else
            {
                pt3 = new Point(
                    X1 + (HeadWidth),
                    Y1 - (HeadHeight));

                pt2 = new Point(
                    X1 - (HeadWidth),
                    Y1 - (HeadHeight));
            }

		    context.BeginFigure(pt1, true, false);
			context.LineTo(pt2, true, true);
			context.LineTo(pt3, true, true);
			context.LineTo(pt1, true, true);

		}
		
		#endregion
	}
}
