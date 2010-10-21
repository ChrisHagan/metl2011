using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yield = System.Collections.Generic.IEnumerator<MindTouch.Tasking.IYield>;
using MindTouch.Dream;
using MindTouch.Tasking;
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
using MindTouch.Xml;

namespace ThumbService
{
    struct RequestInfo { 
        public int slide;
        public int width;
        public int height;
    }
    [DreamService("Dream Tutorial 8-Ball", "Copyright (c) 2006, 2007 MindTouch, Inc.",
        Info = "http://doc.opengarden.org/Dream/Samples /8-Ball",
        SID = new string[] { "http://services.mindtouch.com/dream/tutorial/2007/03/8ball" })]
    public class ServiceRoot : DreamService
    {
        private static Dictionary<RequestInfo, byte[]> cache = new Dictionary<RequestInfo, byte[]>();
        private static ClientConnection client = ClientFactory.Connection();
        Plug _fs;
        protected override Yield Start(XDoc config, Result result) {
            Result res;
            yield return res = Coroutine.Invoke(base.Start, config, new Result());
            res.Confirm();
            var fs = new Result<Plug>();
            CreateService("mount", "http://services.mindtouch.com/dream/draft/2006/11/mount", 
                new XDoc("config").Start("mount").Attr("to", "files").Value("%DreamHost%").End(), fs);
            joinMeTL();
            _fs = fs.Value;
            result.Return();
        }
        private void joinMeTL()
        {
            client.events.StatusChanged += (sender, args) => Console.WriteLine("Status changed: {0}", args.isConnected);
            client.Connect("eecrole", "m0nash2008");
            Trace.Listeners.Add(new ConsoleTraceListener());
        }
        [DreamFeature("GET:", "Returns a thumbnail")]
        public Yield thumb(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var slide = context.GetParam<int>("slide");
            var width = context.GetParam<int>("width");
            var height = context.GetParam<int>("height");
            var requestInfo = new RequestInfo { 
                slide=slide,
                width=width,
                height=height
            };
            var image = cache.ContainsKey(requestInfo) ?
                cache[requestInfo] : createImage(requestInfo);
            response.Return(DreamMessage.Ok(MimeType.JPEG, image));
            yield break;
        }
        [DreamFeature("GET:/home", "Provides a minimal view")]
        public Yield home(DreamContext context) {
            yield return context.Relay(_fs.At("metl.html"), "GET:", null, null);
        }
        private byte[] createImage(RequestInfo info){
            byte[] result = new byte[0];
            var synchrony = new Thread(new ParameterizedThreadStart(delegate{
                List<PreParser> parserList = new List<PreParser>();
                MeTLLib.MeTLLibEventHandlers.PreParserAvailableEventHandler parserReceiver = (sender, parserAvailableArgs)=>{
                    parserList.Add(parserAvailableArgs.parser);
                };
                client.events.PreParserAvailable += parserReceiver;
                var staThread = new Thread(new ParameterizedThreadStart(delegate{
                    while (parserList.Count() < 3)
                    { }
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