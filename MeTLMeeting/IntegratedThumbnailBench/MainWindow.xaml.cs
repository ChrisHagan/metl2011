using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MeTLLib;
using System.Collections.ObjectModel;
using System.Threading;
using MeTLLib.Providers.Connection;
using System.IO;
using System.Diagnostics;
using MeTLLib.DataTypes;

namespace Converters {
    public class Converters
    {
        public static DebugConverter debugConverter = new DebugConverter();
        public class DebugConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                Console.WriteLine(value);
                return value;
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }
        }
    }
}
namespace IntegratedThumbnailBench
{
    public partial class MainWindow : Window
    {
        private ClientConnection conn;
        private ObservableCollection<BitmapImage> imageCollection = new ObservableCollection<BitmapImage>();
        public MainWindow()
        {
            InitializeComponent();
            conn = ClientFactory.Connection(MeTLServerAddress.serverMode.STAGING);
            thumbView.ItemsSource = imageCollection;
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e){
            conn.events.StatusChanged += new MeTLLibEventHandlers.StatusChangedEventHandler(events_StatusChanged);
            conn.Connect("eecrole", "m0nash2008");
        }
        void events_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.isConnected) {
                foreach (var conversation in conn.AvailableConversations.Reverse<ConversationDetails>())
                {
                    thumb(conversation.Jid);
                }
            }
        }
        void thumb(string conversationJid) {
            var details = conn.DetailsOf(conversationJid);
            foreach(var slide in details.Slides.Where(s=>s.type == Slide.TYPE.SLIDE)){
                thumb(slide.id);
            }
        }
        void thumb(int slideId) { 
            var data = createImage(slideId);
            var stream = new MemoryStream(data);
            Dispatcher.BeginInvoke((Action)delegate{
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                imageCollection.Add(image);
            });
        }
        int WIDTH = 320;
        int HEIGHT = 240;
        private byte[] createImage(int slide){
            var provider = conn.getHistoryProvider();
            ManualResetEvent waitHandler = new ManualResetEvent(false);
            byte[] result = new byte[0];
            var synchrony = new Thread(new ThreadStart(delegate{
                provider.Retrieve<PreParser>(
                    null, null,
                    parser =>
                    {
                        result = parserToInkCanvas(parser);
                        waitHandler.Set();
                    }
                    , slide.ToString());
            }));
            synchrony.Start();
            waitHandler.WaitOne();
            return result;
        }
        private byte[] parserToInkCanvas(PreParser parser){
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
                    viewBox.Width = WIDTH;
                    viewBox.Height = HEIGHT;
                    var size = new Size(WIDTH, HEIGHT);
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
}
