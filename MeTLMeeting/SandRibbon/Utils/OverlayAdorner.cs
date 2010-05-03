using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;

namespace SandRibbon.Utils
{
    public class TextOverlayAdorner : Adorner
    {
        public TextBox text;
        private Brush brush = new SolidColorBrush { Opacity = 0.5, Color = Colors.Yellow };
        public TextOverlayAdorner(InkCanvas canvas, TextBox text)
            : base(canvas)
        {
            this.text = text;
        }
        public TextOverlayAdorner(InkCanvas canvas, TextBox text, Color color)
            : base(canvas)
        {
            this.text = text;
            brush = new SolidColorBrush { Opacity = 0.5, Color = color };
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var bounds = new Rect();
            bounds.Y = InkCanvas.GetTop(text);
            bounds.X = InkCanvas.GetLeft(text);
            bounds.Width = text.Text.Count()  * text.FontSize;
            bounds.Height = text.LineCount + text.FontSize;
            if (bounds.Width < 20) bounds.Width = 20;
            if (bounds.Height < 20) bounds.Height = 20;
            drawingContext.DrawRectangle(brush, null, bounds);
        }
    }
    public class ImageOverlayAdorner : Adorner
    {
        public Image image;
        private Brush brush = new SolidColorBrush { Opacity = 0.5, Color = Colors.Yellow };
        public ImageOverlayAdorner(InkCanvas canvas, Image image)
            : base(canvas)
        {
            this.image = image;
        }
        public ImageOverlayAdorner(InkCanvas canvas, Image image, Color color)
            : base(canvas)
        {
            this.image = image;
            brush = new SolidColorBrush { Opacity = 0.5, Color = color };
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var bounds = new Rect();
            bounds.X = InkCanvas.GetLeft(image) + 5;
            bounds.Y = InkCanvas.GetTop(image) + 5;
            bounds.Width = image.Width ;
            bounds.Height = image.Height;
            if (bounds.Width < 20) bounds.Width = 20;
            if (bounds.Height < 20) bounds.Height = 20;
            drawingContext.DrawRectangle(brush, null, bounds);
        }
    }
    public class StrokeOverlayAdorner : Adorner
    {
        public Stroke stroke;
        private Brush brush = new SolidColorBrush { Opacity = 0.5, Color = Colors.Yellow };
        public StrokeOverlayAdorner(InkCanvas canvas, Stroke stroke)
            : base(canvas)
        {
            this.stroke = stroke;
        }
        public StrokeOverlayAdorner(InkCanvas canvas, Stroke stroke, Color color)
            : base(canvas)
        {
            this.stroke = stroke;
            brush = new SolidColorBrush { Opacity = 0.5, Color = color };
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var bounds = stroke.GetBounds();
            if (bounds.Width < 20) bounds.Width = 20;
            if (bounds.Height < 20) bounds.Height = 20;
            drawingContext.DrawRectangle(brush, null, bounds);
        }
    }
}