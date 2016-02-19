using MeTLLib.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SandRibbon.Utils
{
    public class VisualizedPowerpointContent
    {
        public UIElement Visual { get; protected set; }
        public double Width { get; protected set; }
        public double Height { get; protected set; }
        public double X { get; protected set; }
        public double Y { get; protected set; }
        public int Slide { get; protected set; }
        public Privacy Privacy { get; protected set; }
        public string Target { get; protected set; }
        public CanvasContentDescriptor Descriptor { get; protected set; }
        public VisualizedPowerpointContent(int s, double x, double y, double w, double h, Privacy p, string t)
        {
            Slide = s;
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Privacy = p;
            Target = t;
        }
        public VisualizedPowerpointContent(int s, TextDescriptor text) : this(s, text.x, text.y, text.w, text.h,text.privacy,text.target)
        {
            Visual = new TextBlock
            {
                Text = text.content,
                FontFamily = new FontFamily(text.fontFamily),
                FontSize = text.fontSize,
                Foreground = new SolidColorBrush(text.fontColor),
                Width = text.w,
                Height = text.h
            };
            Descriptor = text;
        }
        public VisualizedPowerpointContent(int s, ImageDescriptor image) : this(s, image.x, image.y, image.w, image.h,image.privacy,image.target)
        {
            var source = new BitmapImage();
            try
            {
                source.BeginInit();
                source.StreamSource = new System.IO.MemoryStream(image.imageBytes);
            }
            finally
            {
                source.EndInit();
            }
            Visual = new Image
            {
                Width = image.w,
                Height = image.h,
                Source = source
            };
            Descriptor = image;
        }
    }

    public partial class PowerPointContent : Window
    {
        public PowerPointContent()
        {
            InitializeComponent();
        }
        public void updateProgress(PowerpointImportProgress progress)
        {
            this.Dispatcher.adopt(delegate {
                progressBar.Maximum = progress.totalSlides;
                progressBar.Value = progress.slideId;
                progressLabel.Content = progress.stage.ToString();
                if (progress.stage == PowerpointImportProgress.IMPORT_STAGE.ANALYSED)
                {
                    progressBarContainer.Visibility = Visibility.Collapsed;
                } else {
                    progressBarContainer.Visibility = Visibility.Visible;
                }
            });
        }
        public void updateContent(ConversationDescriptor conversation)
        {
            this.Dispatcher.adopt(delegate
            {
                ContentListing.ItemsSource = conversation.slides.SelectMany(sl => sl.images.Select(i => new VisualizedPowerpointContent(sl.index, i)).Concat(sl.texts.Select(t => new VisualizedPowerpointContent(sl.index, t))));
            });
        }
    }
}
