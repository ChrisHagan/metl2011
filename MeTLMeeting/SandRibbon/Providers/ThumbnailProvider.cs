using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MeTLLib.Providers.Connection;
using SandRibbon.Utils;
using MeTLLib.DataTypes;
using System.Windows.Data;
using MeTLLib;
using System.Net;
using System.Net.Cache;
using Size = System.Windows.Size;

namespace SandRibbon.Providers
{
    public class ThumbnailCaptureHost
    {
        private ClientConnection conn;
        private string conversation;
        public ThumbnailCaptureHost()
        {
            conn = ClientFactory.Connection();
        }
        public ThumbnailCaptureHost(string jid):this()
        {
            if (!Directory.Exists(string.Format("{0}\\thumbs\\", Directory.GetCurrentDirectory())))
                Directory.CreateDirectory(string.Format("{0}\\thumbs\\", Directory.GetCurrentDirectory()));
            conversation = jid;
            if(!Directory.Exists(string.Format("{0}\\thumbs\\{1}\\", Directory.GetCurrentDirectory(), conversation)))
                Directory.CreateDirectory(string.Format("{0}\\thumbs\\{1}\\", Directory.GetCurrentDirectory(), conversation));
            thumbConversation();
        }

        public void thumbConversation() {
            var details = conn.DetailsOf(conversation);
            foreach(var slide in details.Slides.Where(s=>s.type == Slide.TYPE.SLIDE)){
                Dispatcher.CurrentDispatcher.adoptAsync(() =>
                {
                    thumb(slide.id);
                });
            }
        }
        public void thumb(int slideId) { 
            var data = createImage(slideId);
            var stream = new MemoryStream(data);
            
            File.WriteAllBytes(string.Format("{0}\\thumbs\\{1}\\{2}.png",Directory.GetCurrentDirectory(),conversation, slideId), data);
            return;
        }

        int WIDTH = 320;
        int HEIGHT = 240;
        private byte[] createImage(int slide){
            byte[] result = new byte[0];
            Dispatcher.CurrentDispatcher.adoptAsync(delegate
            {
                var provider = conn.getHistoryProvider();
                ManualResetEvent waitHandler = new ManualResetEvent(false);
                var synchrony = new Thread(
                        new ThreadStart(delegate {
                                                   provider.Retrieve <PreParser>
                                                       (
                                                           null,
                                                           null,
                                                           parser => {
                                                               result = parserToInkCanvas(parser);
                                                               waitHandler.Set();
                                                           },
                                                           slide.ToString());
                                                   }));
                                            synchrony.Start();
                                                            waitHandler.WaitOne();
                                                        });

            return result;
        }
        private byte[] parserToInkCanvas(PreParser parser){
            ManualResetEvent waitHandler = new ManualResetEvent(false);
            byte[] result = new byte[0];
            var staThread = new Thread(new ParameterizedThreadStart(delegate
            {
                try
                {
                    var size = new Size(WIDTH, HEIGHT);
                    var canvas = parser.ToVisual();
                    canvas.Measure(size);
                    canvas.Arrange(new Rect(size));
                    canvas.UpdateLayout();
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
                    RenderTargetBitmap targetBitmap =
                       new RenderTargetBitmap(WIDTH, HEIGHT, 96d, 96d, PixelFormats.Pbgra32);
                    targetBitmap.Render(viewBox);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
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

    }
    public class ThumbnailProvider
    {
        
        private static RequestCachePolicy bitmapRetrievePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
        public static SlideToThumbConverter SlideToThumb = new SlideToThumbConverter();
        
        public class SlideToThumbConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var val = ThumbnailProvider.get((Slide)value);
                return val;
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }
        }
        public static ImageBrush get(Slide slide)
        {
            var path = string.Format("http://spacecaps.adm.monash.edu.au:8080/?slide={0}&width={1}&height={2}&server={3}",
                slide.id, 180, 135, ClientFactory.Connection().server.host.Split('.').First());
            var localPath = string.Format("{0}\\thumbs\\{1}\\{2}.png", Directory.GetCurrentDirectory(),Globals.location.activeConversation, slide.id );
            if(!File.Exists(localPath))
                return new ImageBrush();
            App.Now("Loading thumbnail for {0} at {1}", slide.id, localPath);
            BitmapImage bitmap = new BitmapImage();
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