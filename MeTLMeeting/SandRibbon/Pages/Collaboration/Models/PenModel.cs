using SandRibbon.Components;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SandRibbon.Pages.Collaboration
{
    public class ImageSources
    {/*Only a page can push this in - raw models are no good*/
        public ImageSource eraserImage { get; protected set; }
        public ImageSource penImage { get; protected set; }
        public ImageSource highlighterImage { get; protected set; }
        public Brush selectedBrush { get; set; }
        public ImageSources(ImageSource _eraserImage, ImageSource _penImage, ImageSource _highlighterImage)
        {
            eraserImage = _eraserImage;
            penImage = _penImage;
            highlighterImage = _highlighterImage;            
        }
    }
    public class PenAttributes : DependencyObject
    {
        public DrawingAttributes attributes { get; protected set; }
        protected DrawingAttributes originalAttributes;
        protected InkCanvasEditingMode originalMode;
        public int id { get; protected set; }
        protected bool ready = false;
        protected ImageSources images;
        public PenAttributes(int _id, InkCanvasEditingMode _mode, DrawingAttributes _attributes, ImageSources _images)
        {
            id = _id;
            images = _images;
            attributes = _attributes;
            mode = _mode;
            attributes.StylusTip = StylusTip.Ellipse;
            width = attributes.Width;
            color = attributes.Color;
            originalAttributes = _attributes.Clone();
            originalMode = _mode;
            IsSelectedPen = false;
            ready = true;
            icon = generateImageSource();
        }
        public void replaceAttributes(PenAttributes newAttributes)
        {
            mode = newAttributes.mode;
            color = newAttributes.color;
            width = newAttributes.width;
            isHighlighter = newAttributes.isHighlighter;
        }
        public void resetAttributes()
        {
            replaceAttributes(new PenAttributes(id, originalMode, originalAttributes, images));
        }
        protected static readonly Point centerBottom = new Point(128, 256);
        protected static readonly Point centerTop = new Point(128, 0);
        protected void regenerateVisual()
        {
            backgroundBrush = generateBackgroundBrush();
            icon = generateImageSource();
            description = generateDescription();
        }
        protected string generateDescription()
        {
            return ColorHelpers.describe(this);
        }
        protected Brush generateBackgroundBrush()
        {
            return IsSelectedPen ? images.selectedBrush : Brushes.Transparent;
        }
        protected ImageSource generateImageSource()
        {
            DrawingVisual visual = new DrawingVisual();
            DrawingContext dc = visual.RenderOpen();
            if (mode == InkCanvasEditingMode.EraseByStroke || mode == InkCanvasEditingMode.EraseByPoint)
            {
                dc.DrawImage(images.eraserImage, new Rect(0, 0, 256, 256));
            }
            else
            {
                var colorBrush = new SolidColorBrush(attributes.Color);
                dc.DrawEllipse(colorBrush, new Pen(colorBrush, 0), centerBottom, attributes.Width + 25, attributes.Width + 25); //adjusting size to scale them and make them more visible, so that the radius will be between 25 and 125 out of a maximum diameter of 256
                if (attributes.IsHighlighter)
                {
                    dc.DrawImage(images.highlighterImage, new Rect(0, 0, 256, 256));
                }
                else
                {
                    dc.DrawImage(images.penImage, new Rect(0, 0, 256, 256));
                }
            }
            dc.Close();
            var bmp = new RenderTargetBitmap(256, 256, 96.0, 96.0, PixelFormats.Pbgra32);
            bmp.Render(visual);
            return bmp;
        }
        public double width
        {
            get { return (double)GetValue(widthProperty); }
            set
            {
                attributes.Width = value;
                attributes.Height = value;
                var changed = ((double)GetValue(widthProperty)) != value;
                if (ready && changed)
                    regenerateVisual();
                SetValue(widthProperty, value);
            }
        }

        public static readonly DependencyProperty widthProperty = DependencyProperty.Register("width", typeof(double), typeof(PenAttributes), new PropertyMetadata(1.0));
        public bool isHighlighter
        {
            get { return (bool)GetValue(isHighlighterProperty); }
            set
            {
                attributes.IsHighlighter = value;
                var changed = ((bool)GetValue(isHighlighterProperty)) != value;
                if (ready && changed)
                    regenerateVisual();
                SetValue(isHighlighterProperty, value);
            }
        }

        public static readonly DependencyProperty isHighlighterProperty = DependencyProperty.Register("isHighlighter", typeof(bool), typeof(PenAttributes), new PropertyMetadata(false));
        public Color color
        {
            get { return (Color)GetValue(colorProperty); }
            set
            {
                attributes.Color = value;
                var changed = ((Color)GetValue(colorProperty)) != value;
                if (ready && changed)
                    regenerateVisual();
                SetValue(colorProperty, value);
            }
        }

        public static readonly DependencyProperty colorProperty = DependencyProperty.Register("color", typeof(Color), typeof(PenAttributes), new PropertyMetadata(Colors.Black));
        public InkCanvasEditingMode mode
        {
            get { return (InkCanvasEditingMode)GetValue(modeProperty); }
            set
            {
                var changed = ((InkCanvasEditingMode)GetValue(modeProperty)) != value;
                if (ready && changed)
                    regenerateVisual();
                SetValue(modeProperty, value);
            }
        }

        public static readonly DependencyProperty modeProperty = DependencyProperty.Register("mode", typeof(InkCanvasEditingMode), typeof(PenAttributes), new PropertyMetadata(InkCanvasEditingMode.None));
        public Brush backgroundBrush
        {
            get { return (Brush)GetValue(backgroundBrushProperty); }
            set
            {
                SetValue(backgroundBrushProperty, value);
            }
        }

        public static readonly DependencyProperty backgroundBrushProperty = DependencyProperty.Register("backgroundBrush", typeof(Brush), typeof(PenAttributes), new PropertyMetadata(Brushes.Transparent));
        public string description
        {
            get { return (string)GetValue(descriptionProperty); }
            set
            {
                SetValue(descriptionProperty, value);
            }
        }

        public static readonly DependencyProperty descriptionProperty = DependencyProperty.Register("description", typeof(string), typeof(PenAttributes), new PropertyMetadata(""));

        public ImageSource icon
        {
            get { return (ImageSource)GetValue(iconProperty); }
            protected set { SetValue(iconProperty, value); }
        }

        public static ImageSource emptyImage = new BitmapImage();
        public static readonly DependencyProperty iconProperty = DependencyProperty.Register("icon", typeof(ImageSource), typeof(PenAttributes), new PropertyMetadata(emptyImage));
        public bool IsSelectedPen
        {
            get { return (bool)GetValue(isSelectedPenProperty); }
            set
            {
                if (ready)
                    regenerateVisual();
                SetValue(isSelectedPenProperty, value);
            }
        }

        public static readonly DependencyProperty isSelectedPenProperty = DependencyProperty.Register("IsSelectedPen", typeof(bool), typeof(PenAttributes), new PropertyMetadata(false));
    }
}
