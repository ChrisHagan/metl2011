using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MeTLLib.DataTypes
{
    public class WormMove
    {
        public string conversation;
        public string direction;
    }

    public class AutoShape : System.Windows.Controls.Primitives.Thumb
    {
        public PathGeometry PathData
        {
            get { return (PathGeometry)GetValue(PathDataProperty); }
            set { SetValue(PathDataProperty, value); }
        }
        public static readonly DependencyProperty PathDataProperty =
            DependencyProperty.Register("PathData", typeof(PathGeometry), typeof(Thumb), new UIPropertyMetadata(null));

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(Thumb), new UIPropertyMetadata(Brushes.Transparent));

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(Thumb), new UIPropertyMetadata(Brushes.Black));

        public System.Double StrokeThickness
        {
            get { return (System.Double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(System.Double), typeof(Thumb), new UIPropertyMetadata((System.Double)1));

        public AutoShape()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }
    public class VideoMirror : System.Windows.Controls.Primitives.Thumb
    {
        public class VideoMirrorInformation
        {
            public string id;
            public Rectangle rect;
        }

        public string id;
        public void UpdateMirror(VideoMirrorInformation info)
        {
            if (info.id == id)
            {
                this.Rectangle = (Rectangle)info.rect;
            }
        }
        public void RequestNewRectangle()
        {
            Commands.VideoMirrorRefreshRectangle.Execute(id);
        }

        public System.Windows.Shapes.Rectangle Rectangle
        {
            get { return (System.Windows.Shapes.Rectangle)GetValue(RectangleProperty); }
            set
            {
                SetValue(RectangleProperty, value);
                SetValue(RectHeightProperty, ((System.Windows.Shapes.Rectangle)value).Height);
                SetValue(RectWidthProperty, ((System.Windows.Shapes.Rectangle)value).Width);
                SetValue(RectFillProperty, ((System.Windows.Shapes.Rectangle)value).Fill);
            }
        }
        public static readonly DependencyProperty RectangleProperty =
            DependencyProperty.Register("Rectangle", typeof(System.Windows.Shapes.Rectangle), typeof(VideoMirror), new UIPropertyMetadata(null));
        public System.Double RectHeight
        {
            get { return (System.Double)((System.Windows.Shapes.Rectangle)GetValue(RectangleProperty)).Height; }
            set
            {
                if (value == ((System.Windows.Shapes.Rectangle)GetValue(RectangleProperty)).Height)
                    return;
                var newRect = (System.Windows.Shapes.Rectangle)GetValue(RectangleProperty);
                newRect.Height = value;
                SetValue(RectangleProperty, newRect);
            }
        }
        public static readonly DependencyProperty RectHeightProperty =
            DependencyProperty.Register("RectHeight", typeof(System.Double), typeof(VideoMirror), new UIPropertyMetadata((System.Double)0));
        public System.Double RectWidth
        {
            get { return (System.Double)((System.Windows.Shapes.Rectangle)GetValue(RectangleProperty)).Width; }
            set
            {
                if (value == ((System.Windows.Shapes.Rectangle)GetValue(RectangleProperty)).Width)
                    return;
                var newRect = (System.Windows.Shapes.Rectangle)GetValue(RectangleProperty);
                newRect.Width = value;
                SetValue(RectangleProperty, newRect);
            }
        }
        public static readonly DependencyProperty RectWidthProperty =
            DependencyProperty.Register("RectWidth", typeof(System.Double), typeof(VideoMirror), new UIPropertyMetadata((System.Double)0));

        public Brush RectFill
        {
            get { return (Brush)((System.Windows.Shapes.Rectangle)GetValue(RectangleProperty)).Fill; }
            set
            {
                if (value == ((System.Windows.Shapes.Rectangle)GetValue(RectangleProperty)).Fill)
                    return;
                var newRect = (System.Windows.Shapes.Rectangle)GetValue(RectangleProperty);
                newRect.Fill = value;
                SetValue(RectangleProperty, newRect);
            }
        }
        public static readonly DependencyProperty RectFillProperty =
            DependencyProperty.Register("RectFill", typeof(Brush), typeof(VideoMirror), new UIPropertyMetadata(Brushes.Transparent));
        public VideoMirror()
            : base()
        {
            this.Loaded += (a, b) =>
            {
                RequestNewRectangle();
            };
        }
    }

    public class Video : System.Windows.Controls.Primitives.Thumb
    {
        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(Thumb), new UIPropertyMetadata(null));

        public TimeSpan Position
        {
            get { return (TimeSpan)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(Thumb), new UIPropertyMetadata((double)0));
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(Thumb), new UIPropertyMetadata((double)0));
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(Thumb), new UIPropertyMetadata((double)0));
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Thumb), new UIPropertyMetadata((double)0));
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(TimeSpan), typeof(Thumb), new UIPropertyMetadata(null));

        public System.Windows.Controls.MediaElement MediaElement
        {
            get { return (System.Windows.Controls.MediaElement)GetValue(MediaElementProperty); }
            set
            {
                SetValue(MediaElementProperty, value);
            }
        }
        public static readonly DependencyProperty MediaElementProperty =
            DependencyProperty.Register("MediaElement", typeof(System.Windows.Controls.MediaElement), typeof(Thumb), new UIPropertyMetadata(null));
        public System.Uri VideoSource
        {
            get { return (System.Uri)GetValue(VideoSourceProperty); }
            set
            {
                SetValue(VideoSourceProperty, value);
            }
        }
        public static readonly DependencyProperty VideoSourceProperty =
            DependencyProperty.Register("VideoSource", typeof(System.Uri), typeof(Thumb), new UIPropertyMetadata(null));

        public TimeSpan VideoDuration
        {
            get { return (TimeSpan)GetValue(VideoDurationProperty); }
            set { SetValue(VideoDurationProperty, value); }
        }
        public static readonly DependencyProperty VideoDurationProperty =
            DependencyProperty.Register("VideoDuration", typeof(TimeSpan), typeof(Thumb), new UIPropertyMetadata(null));
        public System.Double VideoHeight
        {
            get { return (System.Double)GetValue(VideoHeightProperty); }
            set { SetValue(VideoHeightProperty, value); }
        }
        public static readonly DependencyProperty VideoHeightProperty =
            DependencyProperty.Register("VideoHeight", typeof(System.Double), typeof(Thumb), new UIPropertyMetadata((System.Double)Double.NaN));
        public System.Double VideoWidth
        {
            get { return (System.Double)GetValue(VideoWidthProperty); }
            set { SetValue(VideoWidthProperty, value); }
        }
        public static readonly DependencyProperty VideoWidthProperty =
            DependencyProperty.Register("VideoWidth", typeof(System.Double), typeof(Thumb), new UIPropertyMetadata((System.Double)Double.NaN));

        public void UpdateMirror(string id)
        {
            if (id == this.tag().id)
            {
                var rectToPush = new Rectangle();
                rectToPush.Fill = new VisualBrush(MediaElement);
                rectToPush.Stroke = Brushes.Red;
                rectToPush.Height = MediaElement.ActualHeight;
                rectToPush.Width = MediaElement.ActualWidth;
                Commands.MirrorVideo.Execute(new VideoMirror.VideoMirrorInformation { id = id, rect = rectToPush });
            }
        }
        public Video()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }

    public class RenderedLiveWindow : System.Windows.Controls.Primitives.Thumb
    {
        public System.Windows.Shapes.Rectangle Rectangle
        {
            get { return (System.Windows.Shapes.Rectangle)GetValue(RectangleProperty); }
            set
            {
                SetValue(RectangleProperty, value);
                SetValue(HeightProperty, ((System.Windows.Shapes.Rectangle)value).Height);
                SetValue(WidthProperty, ((System.Windows.Shapes.Rectangle)value).Width);
                SetValue(RectFillProperty, ((System.Windows.Shapes.Rectangle)value).Fill);
            }
        }
        public static readonly DependencyProperty RectangleProperty =
            DependencyProperty.Register("Rectangle", typeof(System.Windows.Shapes.Rectangle), typeof(Thumb), new UIPropertyMetadata(null));
        public Brush RectFill
        {
            get { return (Brush)((System.Windows.Shapes.Rectangle)GetValue(RectangleProperty)).Fill; }
            set
            {
                if (value == ((System.Windows.Shapes.Rectangle)GetValue(RectangleProperty)).Fill)
                    return;
                var newRect = (System.Windows.Shapes.Rectangle)GetValue(RectangleProperty);
                newRect.Fill = value;
                SetValue(RectangleProperty, newRect);
            }
        }
        public static readonly DependencyProperty RectFillProperty =
            DependencyProperty.Register("RectFill", typeof(Brush), typeof(Thumb), new UIPropertyMetadata(Brushes.Transparent));
        public RenderedLiveWindow()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }
}