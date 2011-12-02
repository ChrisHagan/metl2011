using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MeTLLib.DataTypes
{
    public class ThumbnailInformation : DependencyObject
    {
        public Slide.TYPE type { get; set; }
        public int slideId { get; set; }
        public int slideNumber { get; set; }
        public ImageBrush ThumbnailBrush
        {
            get { return (ImageBrush)GetValue(ThumbnailBrushProperty); }
            set { SetValue(ThumbnailBrushProperty, value); }
        }
        public static readonly DependencyProperty ThumbnailBrushProperty =
            DependencyProperty.Register("ThumbnailBrush", typeof(ImageBrush), typeof(ThumbnailInformation), new UIPropertyMetadata(null));
        public Visual Canvas
        {
            get { return (Visual)GetValue(CanvasProperty); }
            set { SetValue(CanvasProperty, value); }
        }
        public static readonly DependencyProperty CanvasProperty =
            DependencyProperty.Register("Canvas", typeof(Visual), typeof(ThumbnailInformation), new UIPropertyMetadata(null));
        public bool Exposed
        {
            get { return (bool)GetValue(ExposedProperty); }
            set { SetValue(ExposedProperty, value); }
        }
        public static readonly DependencyProperty ExposedProperty =
            DependencyProperty.Register("Exposed", typeof(bool), typeof(ThumbnailInformation), new UIPropertyMetadata(false));
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

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static new readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(Thumb), new UIPropertyMetadata(Brushes.Transparent));

        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        public new static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(Thumb), new UIPropertyMetadata(Brushes.Black));

        public System.Double StrokeThickness
        {
            get { return (System.Double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(System.Double), typeof(Thumb), new UIPropertyMetadata((System.Double)1));
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is AutoShape)) return false;
            var foreign = (AutoShape)obj;
            return (((Thumb)this).Equals((Thumb)obj)
                && foreign.PathData.Equals(PathData)
                && foreign.StrokeThickness == StrokeThickness);
        }
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
            public VideoMirrorInformation(string Id, Rectangle Rect)
            {
                id = Id;
                rect = Rect;
            }
            public string id;
            public Rectangle rect;
            public bool ValueEquals(object obj)
            {
                if (obj == null || !(obj is VideoMirrorInformation)) return false;
                var foreign = (VideoMirrorInformation)obj;
                return (foreign.id == id
                    && foreign.rect.Equals(rect));
            }
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is VideoMirror)) return false;
            var foreign = (VideoMirror)obj;
            return (((Thumb)this).Equals((Thumb)obj)
                && foreign.id == id
                && foreign.Rectangle.Equals(Rectangle));
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
    public partial class VideoExtensions
    {
        public static Video clone(this Video video)
        {
            var newVideo = new Video();
            newVideo.MediaElement = video.MediaElement;
            video.tag(video.tag());
            newVideo.VideoSource = video.VideoSource;
            return newVideo;
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
        public static new readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(Thumb), new UIPropertyMetadata((double)0));
        public new double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }
        public new static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Thumb), new UIPropertyMetadata((double)0));
        public new double Height
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
            get
            {
                var dispatcher = this.Dispatcher;
                Uri newUri = null;
                dispatcher.adopt(()=>newUri = (System.Uri)GetValue(VideoSourceProperty));
                return newUri;
            }
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
                Commands.MirrorVideo.Execute(new VideoMirror.VideoMirrorInformation(id, rectToPush));
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
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is RenderedLiveWindow)) return false;
            var foreign = (RenderedLiveWindow)obj;
            return (((Thumb)obj).Equals((Thumb)this)
                && foreign.Rectangle.Equals(Rectangle));
        }
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