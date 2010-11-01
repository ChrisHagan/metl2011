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
using System.Net;
using System.ServiceProcess;
using System.Net.Mime;
using MeTLLib.Providers;
using Ninject;
using Ninject.Modules;
using MeTLLib.DataTypes;

namespace ThumbService
{
    struct RequestInfo {//Structs hash against their first field
        public int slide;
        public int width;
        public int height;
        public string server;
    }
    class MadamModule : ProductionModule{
        public override void Load()
        {
            base.Load();
            Unbind<MeTLServerAddress>();
            Bind<MeTLServerAddress>().To<MadamServerAddress>();
        }
    }
    public class ThumbService// : ServiceBase
    {
        private Dictionary<RequestInfo, byte[]> cache = new Dictionary<RequestInfo, byte[]>();
        private HttpHistoryProvider prod = getProd();
        private HttpHistoryProvider staging = getStaging();
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private HttpListener listener;
        /*
        public static void Main(string[] _args)
        {
            ServiceBase.Run(new ServiceBase[]{new ThumbService()});
        }
         */
        private static HttpHistoryProvider getStaging(){
            var kernel = new StandardKernel(new BaseModule(), new MadamModule());
            kernel.Get<JabberWireFactory>().credentials = new MeTLLib.DataTypes.Credentials("foo","bar", new List<AuthorizedGroup>());
            return kernel.Get<HttpHistoryProvider>();
        }
        private static HttpHistoryProvider getProd(){
            var kernel = new StandardKernel(new BaseModule(), new ProductionModule());
            kernel.Get<JabberWireFactory>().credentials = new MeTLLib.DataTypes.Credentials("foo","bar", new List<AuthorizedGroup>());
            return kernel.Get<HttpHistoryProvider>();
        }
        public static void Main(string[] args) {
            Trace.Listeners.Add(new ConsoleTraceListener());
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
            new MeTLStanzasConstructor();
            var server = new ThumbService();
            server.OnStart(args);
            Console.ReadLine();
        }
        //protected override void OnStart(string[] _args){
        protected void OnStart(string[] _args){
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:8080/");
            listener.Start();
            listener.BeginGetContext(Route, listener);
        }
        public void Route(IAsyncResult result) {
            HttpListenerContext context = listener.EndGetContext(result);
            try
            {
                if (q(context, "invalidate") == "true")
                    Forget(context);//Takes write lock
                if (q(context, "store") == "true")
                    Store(context);//Takes write lock
                Thumb(context);//May take write lock
            }
            catch (Exception e)
            {
                try
                {
                    context.Response.StatusCode = 401;
                    var error = Encoding.UTF8.GetBytes(string.Format("Error: {0} {1}", e.Message, e.StackTrace));
                    context.Response.OutputStream.Write(error, 0, error.Count());
                }
                catch (HttpListenerException) { 
                    /*At this point the client has probably closed the connection.  We're more interested in protecting ourselves than him.
                     No further response*/
                }
            }
            finally {
                if (locker.IsReadLockHeld)
                    locker.ExitReadLock();
                if (locker.IsWriteLockHeld)
                    locker.ExitWriteLock();
                try {
                    context.Response.OutputStream.Close();
                }
                catch (Exception){//See above justification of ignoring this exception
                }
                listener.BeginGetContext(Route, listener);
            }
        }
        private string q(HttpListenerContext context, string key){
            return context.Request.QueryString[key];
        }
        private RequestInfo info(HttpListenerContext context) { 
            return new RequestInfo
            {
                slide = Int32.Parse(q(context, "slide")),
                width = Int32.Parse(q(context, "width")),
                height = Int32.Parse(q(context, "height")),
                server = q(context, "server")
            };
        }
        public void Store(HttpListenerContext context) {
            locker.EnterWriteLock();
            var output = new MemoryStream();
            var input = context.Request.InputStream;
            byte[] buffer = new byte[32768];
            while (true)
            {
                int read = input.Read (buffer, 0, buffer.Length);
                if (read <= 0)
                    break;
                output.Write (buffer, 0, read);
            }
            cache[info(context)] = output.ToArray();
        }
        public void Forget(HttpListenerContext context)
        {
            locker.EnterWriteLock();
            int slide = Int32.Parse(q(context, "slide"));
            var memoKeys = cache.Keys.Where(k => k.slide == slide).ToList();
            foreach (var key in memoKeys)
                cache.Remove(key);
        }
        public void Thumb(HttpListenerContext context){
            var requestInfo = info(context); 
            byte[] image;
            if (cache.ContainsKey(requestInfo))
            {
                locker.EnterReadLock();
                image = cache[requestInfo];
            }
            else
            {
                if(!locker.IsWriteLockHeld)
                    locker.EnterWriteLock();
                image = createImage(requestInfo);
                cache[requestInfo] = image;
            }
            context.Response.ContentType = "image/png";
            context.Response.ContentLength64 = image.Count();
            context.Response.OutputStream.Write(image, 0, image.Count());
            context.Response.OutputStream.Close();
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
                catch (Exception e) {
                    Trace.TraceInformation("{0}\n{1}", e.Message, e.StackTrace);
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
        private byte[] createImage(RequestInfo info){
            var provider = info.server == "madam" ? staging : prod;
            ManualResetEvent waitHandler = new ManualResetEvent(false);
            byte[] result = new byte[0];
            var synchrony = new Thread(new ThreadStart(delegate{
                provider.Retrieve<PreParser>(
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