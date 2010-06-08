using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;

namespace SandRibbon.Components
{
    public partial class PrintingHost : UserControl
    {
        private int slideId;
        private static int ThumbnailSideSize = 96;
        public PrintingHost()
        {
            InitializeComponent();
            Commands.ThumbnailGenerated.RegisterCommand(new DelegateCommand<UnscaledThumbnailData>(ThumbnailGenerated));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
        }
        private void ThumbnailGenerated(UnscaledThumbnailData thumbData)
        {
            saveScaledBitmapToDisk(ThumbnailPath(thumbData.id), ThumbnailSideSize, ThumbnailSideSize, thumbData.data);
            Commands.ThumbnailAvailable.Execute(thumbData.id);
        }
        private void MoveTo(int slideId)
        {
            this.slideId = slideId;
        }
        public string ThumbnailPath(int id)
        {
            if (!Directory.Exists("thumbs"))
                Directory.CreateDirectory("thumbs");
            var fullPath = string.Format("thumbs/{0}", Globals.me);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            var path = string.Format("{0}/{1}.png", fullPath, id);
            return path;
        }
        public void saveCanvasToDisk(FrameworkElement content, string path, int sourceWidth, int sourceHeight, int desiredWidth, int desiredHeight)
        {
            var bitmap = new RenderTargetBitmap(sourceWidth, sourceHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(content);
            saveScaledBitmapToDisk(path, desiredWidth, desiredHeight, bitmap);
        }
        private static void saveScaledBitmapToDisk(string path, int width, int height, RenderTargetBitmap bitmap)
        {
            var dominantSide = bitmap.Height;
            if (bitmap.Width > bitmap.Height)
                dominantSide = bitmap.Width;
            var scalingRatio = width / dominantSide;
            using (var scaledBitmap = Scale(bitmap, (float)scalingRatio, (float)scalingRatio))
            {
                using (var stream = File.Create(path))
                    scaledBitmap.Save((Stream)stream, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        public static Bitmap Scale(RenderTargetBitmap bitmap, float ScaleFactorX, float ScaleFactorY)
        {
            int scaleWidth = (int)Math.Max(bitmap.Width * ScaleFactorX, 1.0f);
            int scaleHeight = (int)Math.Max(bitmap.Height * ScaleFactorY, 1.0f);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            var stream = new MemoryStream();
            encoder.Save(stream);
            var frombitmap = new Bitmap(stream);
            stream.Close();
            stream.Dispose();

            var newbitmap = (Bitmap)frombitmap;
            Bitmap scaledBitmap = new Bitmap(scaleWidth, scaleHeight);

            using (Graphics gr = Graphics.FromImage(scaledBitmap))
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                gr.DrawImage(newbitmap, new System.Drawing.Rectangle(0, 0, scaleWidth, scaleHeight), new System.Drawing.Rectangle(0, 0, (int)bitmap.Width, (int)bitmap.Height), GraphicsUnit.Pixel);
            }
            return scaledBitmap;
        }
        private class ThumbBox : Viewbox
        {
            private string filename;
            public ThumbBox(UIElement child, string filename, int sideSize)
                : base()
            {
                this.Child = child;
                this.filename = filename;
                this.Width = sideSize;
                this.Height = sideSize;
            }
            public void Save()
            {
                this.UpdateLayout();
                var bitmap = new RenderTargetBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(this);
                BitmapFrame frame = BitmapFrame.Create(bitmap);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(frame);
                using (var stream = File.Create(filename))
                    encoder.Save(stream);
            }
        }
    }
}