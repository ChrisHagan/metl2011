using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;
using System.Windows.Data;
using MeTLLib;
using Size = System.Windows.Size;

namespace SandRibbon.Providers
{
    public class ThumbnailCaptureHost
    {
        private readonly ClientConnection conn;
        private string conversation;
        public ThumbnailCaptureHost()
        {
            conn = ClientFactory.Connection();
        }
        public ThumbnailCaptureHost(string jid):this()
        {
            conversation = jid;
            createThumbnailFileStructure(jid);
        }
        public void thumbConversation() {
            var details = conn.DetailsOf(conversation);
            foreach(var slide in details.Slides.Where(s=>s.type == Slide.TYPE.SLIDE))
            {
                var localSlide = slide;
                Dispatcher.CurrentDispatcher.adoptAsync(() => thumb(localSlide.id));
            }
        }
        public void thumb(string jid, int slideId)
        {
            conversation = jid;
            createThumbnailFileStructure(jid);
            thumb(slideId);
        }
        public void thumb(int[] ids)
        {
            foreach(var id in ids)
                thumb(id);
        }
        public void thumb(int slideId) { 
            var data = createImage(slideId);
            File.WriteAllBytes(ThumbnailPath(conversation, slideId), data);
            return;
        }
        private const int WIDTH = 320;
        private const int HEIGHT = 240;
        private byte[] createImage(int slide){
            var result = new byte[0];
            var provider = conn.getHistoryProvider();
            var waitHandler = new ManualResetEvent(false);
            var synchrony = new Thread(() => provider.Retrieve<PreParser>(
                                             null,
                                             null,
                                             parser =>
                                             {
                                                 result = parserToInkCanvas(parser);
                                                 waitHandler.Set();
                                             },
                                             slide.ToString()));
            synchrony.Start();
            waitHandler.WaitOne();
            return result;
        }
        private static byte[] parserToInkCanvas(PreParser parser){
            var waitHandler = new ManualResetEvent(false);
            var result = new byte[0];
            var staThread = new Thread(new ParameterizedThreadStart(delegate
            {
                try
                {
                    var canvas = new InkCanvas();
                    parser.Populate(canvas);
                    var size = new Size(WIDTH, HEIGHT);
                    var viewBox = new Viewbox
                          {
                              Stretch = Stretch.Uniform,
                              Child = canvas,
                              Width = WIDTH,
                              Height = HEIGHT
                          };
                    viewBox.Measure(size);
                    viewBox.Arrange(new Rect(size));
                    viewBox.UpdateLayout();
                    var targetBitmap = new RenderTargetBitmap(WIDTH, HEIGHT, 96d, 96d, PixelFormats.Pbgra32);
                    targetBitmap.Render(viewBox);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(targetBitmap));
                    using (var stream = new MemoryStream())
                    {
                        encoder.Save(stream);
                        result = stream.ToArray();
                    }
                }
                finally { 
                    waitHandler.Set();
                }
            }));
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            waitHandler.WaitOne();
            return result;
        }
        public string ThumbnailPath(string jid, int id)
        {
            string fullPath = createThumbnailFileStructure(jid);
            var path = string.Format("{0}\\{1}.png", fullPath, id);
            return path;
        }
        private static string createThumbnailFileStructure(string jid)
        {
            if (!Directory.Exists(string.Format("{0}\\thumbs\\", Directory.GetCurrentDirectory())))
                Directory.CreateDirectory(string.Format("{0}\\thumbs\\", Directory.GetCurrentDirectory()));
            if(!Directory.Exists(string.Format("{0}\\thumbs\\{1}\\", Directory.GetCurrentDirectory(), Globals.me)))
                Directory.CreateDirectory(string.Format("{0}\\thumbs\\{1}\\", Directory.GetCurrentDirectory(), Globals.me));
            var fullPath = string.Format("{0}\\thumbs\\{1}\\{2}\\", Directory.GetCurrentDirectory(), Globals.me, jid);
            if(!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            return fullPath;
        }
    }
    public class ThumbnailProvider
    {
        public static SlideToThumbConverter SlideToThumb = new SlideToThumbConverter();
        public class SlideToThumbConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var val = get((Slide)value);
                return val;
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }
        }
        public static ImageBrush get(Slide slide)
        {
            var localPath = new ThumbnailCaptureHost().ThumbnailPath(Globals.conversationDetails.Jid, slide.id); 
            if(!File.Exists(localPath))
                return new ImageBrush();
            App.Now("Loading thumbnail for {0} at {1}", slide.id, localPath);
            var bitmap = new BitmapImage();
            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(localPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            catch (Exception)
            {
                App.Now("Error in loading a thumbnail. boourns");
            }
            var image = new ImageBrush(bitmap);
            return image;
        }
        
    }
}