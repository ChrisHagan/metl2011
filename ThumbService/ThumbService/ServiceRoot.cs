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

namespace ThumbService
{
    struct RequestInfo { 
        public int slide;
        public int width;
        public int height;
    }
    public class ServiceRoot : KayakService
    {
        private static Dictionary<RequestInfo, byte[]> cache = new Dictionary<RequestInfo, byte[]>();
        private static ClientConnection client = ClientFactory.Connection(MeTLServerAddress.serverMode.STAGING);
        public static void Main(string[] _args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            var server = new KayakServer();
            server.UseFramework();
            server.Start(); 
            client.events.StatusChanged += (sender, args) => Trace.TraceInformation("Status changed: {0}", args.isConnected);
            client.Connect("eecrole", "m0nash2008");
            Trace.TraceInformation("Don't press anything or I'll die.  I'm serious.  I'll just die."); 
            Console.ReadLine();
            Trace.TraceInformation("Oh god why");
            server.Stop(); 
        }
        [Path("/thumb/{slide}")]
        public void Thumb(int slide){
            Trace.TraceInformation("Request for slide {0}", slide);
            var width = 320;
            var height = 240;
            var requestInfo = new RequestInfo { 
                slide=slide,
                width=width,
                height=height
            };
            var image = cache.ContainsKey(requestInfo) ?
                cache[requestInfo] : createImage(requestInfo);
            Response.Headers.Add("mime-type", "image/png");
            Response.OutputStream.Write(image, 0, image.Count());
        }
        private byte[] createImage(RequestInfo info){
            Trace.TraceInformation(info.ToString());
            byte[] result = new byte[0];
            var synchrony = new Thread(new ParameterizedThreadStart(delegate{
                List<PreParser> parserList = new List<PreParser>();
                MeTLLib.MeTLLibEventHandlers.PreParserAvailableEventHandler parserReceiver = (sender, parserAvailableArgs)=>{
                    parserList.Add(parserAvailableArgs.parser);
                };
                client.events.PreParserAvailable += parserReceiver;
                var staThread = new Thread(new ParameterizedThreadStart(delegate{
                    while (parserList.Count() < 3) { }
                    var canvas = new InkCanvas();
                    foreach (var preParser in parserList.ToList())
                        preParser.Populate(canvas);
                    var viewBox = new Viewbox();
                    viewBox.Stretch = Stretch.Uniform;
                    viewBox.Child = canvas;
                    viewBox.Width = info.width;
                    viewBox.Height = info.height;
                    var size = new Size(info.width, info.height);
                    viewBox.Measure(size);
                    viewBox.Arrange(new Rect(size));
                    viewBox.UpdateLayout();
                    viewBox.Dispatcher.Invoke(DispatcherPriority.Background,new ThreadStart(delegate { }));
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
                    client.events.PreParserAvailable -= parserReceiver;
                }));
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                client.AsyncRetrieveHistoryOf(info.slide);
                staThread.Join();
            }));
            synchrony.Start();
            synchrony.Join();
            cache[info] = result;
            return result;
        }
    }
}