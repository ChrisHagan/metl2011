using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading;
using System.Windows.Ink;
using System.Windows.Input;
using MeTLLib;
using System.Diagnostics;
using MeTLLib.Providers.Connection;
using System.Windows;
using System.Windows.Threading;
using Kayak;
using Kayak.Framework;
using System.Net;
using System.ServiceProcess;

namespace ThumbService
{
    struct RequestInfo { 
        public int slide;
        public int width;
        public int height;
    }
    public class ThumbService : KayakService
    {
        public static KayakServer server;
        private static Dictionary<RequestInfo, byte[]> cache = new Dictionary<RequestInfo, byte[]>();
        private static ClientConnection client = ClientFactory.Connection(MeTLServerAddress.serverMode.STAGING);
        public static void Main(string[] _args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            server = new KayakServer();
            server.UseFramework();
            server.Start(); 
            client.events.StatusChanged += (sender, args) => Trace.TraceInformation("Status changed: {0}", args.isConnected);
            client.Connect("eecrole", "m0nash2008");
            Console.ReadLine();
        }
        [Path("/thumb/{slide}")]
        public void Thumb(int slide, int width, int height){
            var requestInfo = new RequestInfo
            {
                slide = slide,
                width = width,
                height = height
            };
            var image = cache.ContainsKey(requestInfo) ?
                cache[requestInfo] : createImage(requestInfo);
            Response.Headers.Add("mime-type", "image/png");
            Response.OutputStream.Write(image, 0, image.Count());
        }
        private byte[] parserToInkCanvas(PreParser parser, RequestInfo info) { 
            ManualResetEvent waitHandler = new ManualResetEvent(false);
            byte[] result = new byte[0];
            var staThread = new Thread(new ParameterizedThreadStart(delegate
            {
                try
                {
                    var canvas = new InkCanvas();
                    parser.Populate(canvas);
                    var viewBox = new Viewbox();
                    viewBox.Stretch = Stretch.Uniform;
                    viewBox.Child = canvas;
                    viewBox.Width = info.width;
                    viewBox.Height = info.height;
                    var size = new Size(info.width, info.height);
                    viewBox.Measure(size);
                    viewBox.Arrange(new Rect(size));
                    viewBox.UpdateLayout();
                    RenderTargetBitmap targetBitmap =
                       new RenderTargetBitmap(info.width, info.height, 96d, 96d, PixelFormats.Pbgra32);
                    targetBitmap.Render(viewBox);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(targetBitmap));
                    using (var stream = new MemoryStream())
                    {
                        encoder.Save(stream);
                        result = stream.ToArray();
                    }
                }
                catch (Exception) {}
                finally { 
                    waitHandler.Set();
                }
            }));
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            waitHandler.WaitOne();
            return result;
        }
        private byte[] createImage(RequestInfo info){
            Trace.TraceInformation(info.ToString());
            ManualResetEvent waitHandler = new ManualResetEvent(false);
            byte[] result = new byte[0];
            var synchrony = new Thread(new ThreadStart(delegate{
                client.getHistoryProvider().Retrieve<PreParser>(
                    null, null,
                    parser =>
                    {
                        result = parserToInkCanvas(parser, info);
                        waitHandler.Set();
                    }
                    , info.slide.ToString());
            }));
            synchrony.Start();
            waitHandler.WaitOne();
            cache[info] = result;
            return result;
        }
    }
}