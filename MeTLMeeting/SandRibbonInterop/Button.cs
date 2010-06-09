using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SandRibbonObjects;
using Divelements.SandRibbon;

namespace SandRibbonInterop
{
    public class ThumbnailInformation : INotifyPropertyChanged
    {
        public Slide.TYPE type { get; set; }
        public int slideId { get; set; }
        public int slideNumber { get; set; }
        private ImageBrush thumbnailProperty;
        public bool exposed;
        public ImageBrush thumbnail
        {
            get { return thumbnailProperty; }
            set
            {
                thumbnailProperty = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("thumbnail"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class PowerpointVideo : System.Windows.Controls.MediaElement
    {
        public TextTag tag()
        {
            var newtag = new TextTag();
            return newtag;
        }

        public static readonly DependencyProperty PlayModeProperty =
            DependencyProperty.Register("PlayMode", typeof(System.Windows.Controls.MediaState), typeof(System.Windows.Controls.MediaElement), new UIPropertyMetadata(System.Windows.Controls.MediaState.Manual));
        public System.Windows.Controls.MediaState PlayMode
        {
            get { return (System.Windows.Controls.MediaState)GetValue(PlayModeProperty); }
            set { SetValue(PlayModeProperty, value); }
        }
        public String AnimationTimeLine
        {
            get { return (String)GetValue(AnimationTimeLineProperty); }
            set { SetValue(AnimationTimeLineProperty, value); }
        }
        public static readonly DependencyProperty AnimationTimeLineProperty =
            DependencyProperty.Register("VideoDuration", typeof(String), typeof(System.Windows.Controls.MediaElement), new UIPropertyMetadata(""));
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

        public PowerpointVideo()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
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
            set { SetValue(MediaElementProperty, value); }
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

        /*public System.Windows.Controls.Image Thumbnail
        {
            get { return (System.Windows.Controls.Image)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }
        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(System.Windows.Controls.Image), typeof(Thumb), new UIPropertyMetadata(null));
        */

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
        /*public System.Double RectHeight
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
            DependencyProperty.Register("RectHeight", typeof(System.Double), typeof(System.Windows.Shapes.Rectangle), new UIPropertyMetadata((System.Double)0));
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
            DependencyProperty.Register("RectWidth", typeof(System.Double), typeof(System.Windows.Shapes.Rectangle), new UIPropertyMetadata((System.Double)0));
        */
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
        /*public Brush Background
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
        */

        public RenderedLiveWindow()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }


    public enum ButtonSize
    {
        Never, WhenGroupIsMedium, WhenGroupIsSmall
    }
    public enum InternalButtonSize
    {
        Small, Medium, Large
    }
    public class CheckBox : System.Windows.Controls.CheckBox
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CheckBox), new UIPropertyMetadata("No label"));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(CheckBox), new UIPropertyMetadata(null));

        public ButtonSize CollapseToMedium
        {
            get { return (ButtonSize)GetValue(CollapseToMediumProperty); }
            set { SetValue(CollapseToMediumProperty, value); }
        }
        public static readonly DependencyProperty CollapseToMediumProperty =
            DependencyProperty.Register("CollapseToMedium", typeof(ButtonSize), typeof(CheckBox), new UIPropertyMetadata(ButtonSize.WhenGroupIsMedium));

        public ButtonSize CollapseToSmall
        {
            get { return (ButtonSize)GetValue(CollapseToSmallProperty); }
            set { SetValue(CollapseToSmallProperty, value); }
        }
        public static readonly DependencyProperty CollapseToSmallProperty =
            DependencyProperty.Register("CollapseToSmall", typeof(ButtonSize), typeof(CheckBox), new UIPropertyMetadata(ButtonSize.WhenGroupIsSmall));

        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(RibbonGroupVariant), typeof(CheckBox), new UIPropertyMetadata(RibbonGroupVariant.Large));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(CheckBox), new UIPropertyMetadata(InternalButtonSize.Large));

        public CheckBox()
            : base()
        {
            this.Loaded += (a, b) =>
            {

            };
        }
    }

    public class RibbonPanel : System.Windows.Controls.WrapPanel
    {
        public RibbonPanel()
            : base()
        {
            this.Orientation = System.Windows.Controls.Orientation.Vertical;
            this.Loaded += (a, b) =>
            {
            };
        }
    }

    public class RadioButton : System.Windows.Controls.RadioButton
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RadioButton), new UIPropertyMetadata("No label"));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(RadioButton), new UIPropertyMetadata(null));

        public ButtonSize CollapseToMedium
        {
            get { return (ButtonSize)GetValue(CollapseToMediumProperty); }
            set { SetValue(CollapseToMediumProperty, value); }
        }
        public static readonly DependencyProperty CollapseToMediumProperty =
            DependencyProperty.Register("CollapseToMedium", typeof(ButtonSize), typeof(RadioButton), new UIPropertyMetadata(ButtonSize.WhenGroupIsMedium));

        public ButtonSize CollapseToSmall
        {
            get { return (ButtonSize)GetValue(CollapseToSmallProperty); }
            set { SetValue(CollapseToSmallProperty, value); }
        }
        public static readonly DependencyProperty CollapseToSmallProperty =
            DependencyProperty.Register("CollapseToSmall", typeof(ButtonSize), typeof(RadioButton), new UIPropertyMetadata(ButtonSize.WhenGroupIsSmall));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(RadioButton), new UIPropertyMetadata(InternalButtonSize.Large));

        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(RibbonGroupVariant), typeof(RadioButton), new UIPropertyMetadata(RibbonGroupVariant.Large));

        public RadioButton()
            : base()
        {
            this.Loaded += (a, b) =>
            {
                var tag = this.Parent != null && this.Parent is FrameworkElement ?
                    ((FrameworkElement)this.Parent).Tag :
                    null;
                if (tag != null)
                    this.GroupName = (string)tag;
            };
        }
    }
    public class Button : System.Windows.Controls.Button
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Button), new UIPropertyMetadata("No label"));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(Button), new UIPropertyMetadata(null));

        public ButtonSize CollapseToMedium
        {
            get { return (ButtonSize)GetValue(CollapseToMediumProperty); }
            set { SetValue(CollapseToMediumProperty, value); }
        }
        public static readonly DependencyProperty CollapseToMediumProperty =
            DependencyProperty.Register("CollapseToMedium", typeof(ButtonSize), typeof(Button), new UIPropertyMetadata(ButtonSize.WhenGroupIsMedium));

        public ButtonSize CollapseToSmall
        {
            get { return (ButtonSize)GetValue(CollapseToSmallProperty); }
            set { SetValue(CollapseToSmallProperty, value); }
        }
        public static readonly DependencyProperty CollapseToSmallProperty =
            DependencyProperty.Register("CollapseToSmall", typeof(ButtonSize), typeof(Button), new UIPropertyMetadata(ButtonSize.WhenGroupIsSmall));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(Button), new UIPropertyMetadata(InternalButtonSize.Large));

        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(RibbonGroupVariant), typeof(Button), new UIPropertyMetadata(RibbonGroupVariant.Large));

        public Button()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }

    public class DrawerContent
    {
        public System.Windows.Controls.UserControl content { get; set; }
        public string header { get; set; }
        public Visibility visibility { get; set; }
        public int column { get; set; }
    }
    
    public class NonRibbonButton : System.Windows.Controls.Button
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NonRibbonButton), new UIPropertyMetadata("No label"));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(NonRibbonButton), new UIPropertyMetadata(null));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(NonRibbonButton), new UIPropertyMetadata(InternalButtonSize.Large));

        public NonRibbonButton()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }

    public class DoubleButton : System.Windows.Controls.Button
    {
        public FrameworkElement Popup
        {
            get { return (Popup)GetValue(PopupProperty); }
            set { SetValue(PopupProperty, value); }
        }

        public static readonly DependencyProperty PopupProperty =
            DependencyProperty.Register("Popup", typeof(FrameworkElement), typeof(DoubleButton),
                                        new UIPropertyMetadata(null));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(DoubleButton), new UIPropertyMetadata("No label"));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(DoubleButton), new UIPropertyMetadata(null));

        public ButtonSize CollapseToMedium
        {
            get { return (ButtonSize)GetValue(CollapseToMediumProperty); }
            set { SetValue(CollapseToMediumProperty, value); }
        }
        public static readonly DependencyProperty CollapseToMediumProperty =
            DependencyProperty.Register("CollapseToMedium", typeof(ButtonSize), typeof(DoubleButton), new UIPropertyMetadata(ButtonSize.WhenGroupIsMedium));

        public ButtonSize CollapseToSmall
        {
            get { return (ButtonSize)GetValue(CollapseToSmallProperty); }
            set { SetValue(CollapseToSmallProperty, value); }
        }
        public static readonly DependencyProperty CollapseToSmallProperty =
            DependencyProperty.Register("CollapseToSmall", typeof(ButtonSize), typeof(DoubleButton), new UIPropertyMetadata(ButtonSize.WhenGroupIsSmall));

        public InternalButtonSize InternalButtonSize
        {
            get { return (InternalButtonSize)GetValue(InternalButtonSizeProperty); }
            set { SetValue(InternalButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty InternalButtonSizeProperty =
            DependencyProperty.Register("InternalButtonSize", typeof(InternalButtonSize), typeof(DoubleButton), new UIPropertyMetadata(InternalButtonSize.Large));

        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(RibbonGroupVariant), typeof(DoubleButton), new UIPropertyMetadata(RibbonGroupVariant.Large));

        public DoubleButton()
            : base()
        {
            this.Loaded += (a, b) =>
            {
            };
        }
    }
    public class SlideViewingListBox : System.Windows.Controls.ListBox
    {
        public RibbonGroupVariant ParentActiveVariant
        {
            get { return (Divelements.SandRibbon.RibbonGroupVariant)GetValue(ParentActiveVariantProperty); }
            set { SetValue(ParentActiveVariantProperty, value); }
        }
        public static readonly DependencyProperty ParentActiveVariantProperty =
            DependencyProperty.Register("ParentActiveVariant", typeof(Divelements.SandRibbon.RibbonGroupVariant), typeof(SlideViewingListBox), new UIPropertyMetadata(Divelements.SandRibbon.RibbonGroupVariant.Large));

    }
}
