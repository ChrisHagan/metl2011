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
using System.Diagnostics;
using MeTLLib.Providers;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Ink;

namespace LibTester
{
    public partial class MainWindow : Window
    {
        private List<ConversationDetails> ConversationListingSource;
        private ClientConnection client;
        public static SlidesConverter slideConverter = new SlidesConverter();
        public static SlideIdConverter slideIdConverter = new SlideIdConverter();
        public static SlideIndexConverter slideIndexConverter = new SlideIndexConverter();
        public class SlidesConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var slides = new ObservableCollection<Slide>();
                if (value is ConversationDetails)
                {
                    var cd = (ConversationDetails)value;
                    foreach (Slide slide in cd.Slides)
                    {
                        slides.Add(slide);
                    }
                }
                return slides;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        public class SlideIdConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is Slide)
                {
                    return ((Slide)value).id.ToString();
                }
                return "";
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        public class SlideIndexConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is Slide)
                {
                    return ((Slide)value).index.ToString();
                }
                return "";
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            client = ClientFactory.Connection();
            client.events.StatusChanged += (sender, args) => { Dispatcher.adoptAsync(() => { if (args.isConnected)setup(); }); };
            client.events.StrokeAvailable += (sender, args) => { Dispatcher.adoptAsync(() => inkCanvas.Strokes.Add(args.stroke.stroke)); };
            client.events.DirtyStrokeAvailable += (sender, args) =>
            {
                Dispatcher.adoptAsync(() =>
                {
                    var strokesToRemove = new StrokeCollection();
                    for (int i = 0; i < inkCanvas.Strokes.Count; i++)
                    {
                        var child = inkCanvas.Strokes[i];
                        if (child is Stroke && ((Stroke)child).startingSum().ToString() == args.dirtyElement.identifier)
                            strokesToRemove.Add(inkCanvas.Strokes[i]);
                    }
                    foreach (Stroke removedStroke in strokesToRemove)
                        inkCanvas.Strokes.Remove(removedStroke);
                    ;
                });
            };
            client.events.TextBoxAvailable += (sender, args) =>
            {
                Dispatcher.adoptAsync(() =>
                {
                    var box = args.textBox.box;
                    box.Background = Brushes.Transparent;
                    box.BorderBrush = Brushes.Transparent;
                    box.BorderThickness = new Thickness(0);
                    inkCanvas.Children.Add(box);
                });
            };
            client.events.DirtyTextBoxAvailable += (sender, args) =>
                {
                    Dispatcher.adoptAsync(() =>
                    {
                        for (int i = 0; i < inkCanvas.Children.Count; i++)
                        {
                            var child = inkCanvas.Children[i];
                            if (child is TextBox && ((TextBox)child).tag().id == args.dirtyElement.identifier)
                                inkCanvas.Children.Remove(inkCanvas.Children[i]);
                        }
                        ;
                    });
                };
            client.events.ImageAvailable += (sender, args) => { Dispatcher.adoptAsync(() => inkCanvas.Children.Add(args.image.image)); };
            client.events.DirtyImageAvailable += (sender, args) =>
            {
                Dispatcher.adoptAsync(() =>
                {
                    for (int i = 0; i < inkCanvas.Children.Count; i++)
                    {
                        var child = inkCanvas.Children[i];
                        if (child is Image && ((Image)child).tag().id == args.dirtyElement.identifier)
                            inkCanvas.Children.Remove(inkCanvas.Children[i]);
                    }
                    ;
                });
            };
            client.events.VideoAvailable += (sender, args) =>
            {
                Dispatcher.adoptAsync(() =>
                {
                    var me = args.video.video.MediaElement;
                    me.LoadedBehavior = MediaState.Play;
                    Canvas.SetLeft(me, args.video.X);
                    Canvas.SetTop(me, args.video.Y);
                    me.Width = args.video.Width;
                    me.Height = args.video.Height;
                    inkCanvas.Children.Add(me);
                });
            };
            client.events.PreParserAvailable += (sender, args) =>
            {
                Dispatcher.adoptAsync(() =>
                    {
                        var parser = ((PreParser)args.parser);
                        foreach (TargettedVideo video in parser.videos.Values)
                        {
                            var me = video.video.MediaElement;
                            me.LoadedBehavior = MediaState.Play;
                            Canvas.SetLeft(me, video.video.X);
                            Canvas.SetTop(me, video.video.Y);
                            me.Width = video.video.Width;
                            me.Height = video.video.Height;
                            inkCanvas.Children.Add(me);
                        }
                        foreach (TargettedImage image in parser.images.Values)
                            inkCanvas.Children.Add(image.image);
                        foreach (TargettedTextBox textBox in parser.text.Values)
                            inkCanvas.Children.Add(textBox.box);
                        foreach (TargettedStroke stroke in parser.ink)
                            inkCanvas.Strokes.Add(stroke.stroke);
                    });
            };
            client.events.FileAvailable += (sender, args) =>
            {
                var a = sender;
                var b = args;
                MessageBox.Show(a + ":" + b.file.name + ":" + b.file.url);
            };
            username.Text = "eecrole";
            password.Password = "m0nash2008";
            attemptToAuthenticate(this, new RoutedEventArgs());
        }
        private void attemptToAuthenticate(object sender, RoutedEventArgs e)
        {
            client.Connect(username.Text, password.Password);
        }
        private void setup()
        {
            ConversationListingSource = getAllConversations();
            ConversationListing.ItemsSource = ConversationListingSource;
            doMoveTo(101);
        }
        private void getConversations(object sender, RoutedEventArgs e)
        {
            var conversationList = "";
            foreach (ConversationDetails details in getAllConversations())
                conversationList += details.Jid + ":" + details.Title + "\r\n";
            MessageBox.Show(conversationList);
        }
        private List<ConversationDetails> getAllConversations()
        {
            if (client != null && client.isConnected)
            {
                return client.AvailableConversations;
            }
            else return null;
        }
        private void moveTo(object sender, RoutedEventArgs e)
        {
            doMoveTo(Int32.Parse(location.Text));
        }
        private void joinConversationByButton(object sender, RoutedEventArgs e)
        {
            doJoinConversation(((Button)sender).Tag.ToString());
        }
        private void moveToByButton(object sender, RoutedEventArgs e)
        {
            doMoveTo(Int32.Parse(((Button)sender).Tag.ToString()));
        }
        private void doMoveTo(int where)
        {
            if (client == null) return;
            inkCanvas.Children.Clear();
            inkCanvas.Strokes.Clear();
            client.MoveTo(where);
        }
        private void doJoinConversation(string where)
        {
            if (client == null) return;
            inkCanvas.Children.Clear();
            inkCanvas.Strokes.Clear();
            client.JoinConversation(where);
        }
        private void getHistory(object sender, RoutedEventArgs e)
        {
            if (client == null) return;
            var parser = client.RetrieveHistoryOfMUC(location.Text);
            //foreach (PreParser parser in parsers)
                MessageBox.Show(describeParser(parser));
        }
        private void getDisco(object sender, RoutedEventArgs e)
        {
            if (client == null) return;
            var conversationList = "";
            foreach (ConversationDetails details in client.CurrentConversations)
                conversationList += details.Jid + ":" + details.Title + "\r\n";
            MessageBox.Show(conversationList);
        }
        private void setInkMode(object sender, RoutedEventArgs e)
        {
            string tag = ((FrameworkElement)sender).Tag.ToString();
            switch (tag)
            {
                case "Ink":
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    break;
                case "Select":
                    inkCanvas.EditingMode = InkCanvasEditingMode.Select;
                    break;
                case "Erase":
                    inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    break;
            }
        }
        private void StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (client != null)
            {
                var internalStroke = e.Stroke;
                var newStroke = new TargettedStroke(client.location.currentSlide, client.username, "presentationSpace", "public", internalStroke);
                client.SendStroke(newStroke);
            }
        }
        private void NewTextBox(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                var internalTextBox = new TextBox();
                internalTextBox.Text = "this is a new textbox from MeTLLib";
                Canvas.SetLeft(internalTextBox, 100);
                Canvas.SetTop(internalTextBox, 100);
                var newTargettedTextBox = new TargettedTextBox(client.location.currentSlide, client.username, "presentationSpace", "public", internalTextBox);
                client.SendTextBox(newTargettedTextBox);
            }
        }
        private void NewImage(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                var ofdg = new Microsoft.Win32.OpenFileDialog();
                ofdg.Multiselect = false;
                ofdg.ShowDialog();
                if (!String.IsNullOrEmpty(ofdg.FileName))
                {
                    var internalImage = new Image();
                    internalImage.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ofdg.FileName);
                    Canvas.SetLeft(internalImage, 100);
                    Canvas.SetTop(internalImage, 100);
                    client.UploadAndSendImage(new MeTLLib.DataTypes.MeTLStanzas.LocalImageInformation
                    (client.location.currentSlide, client.username, "presentationSpace", "public", internalImage, ofdg.FileName, false));
                }
            }
        }
        private void NewVideo(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                var ofdg = new Microsoft.Win32.OpenFileDialog();
                ofdg.Multiselect = false;
                ofdg.ShowDialog();
                if (!String.IsNullOrEmpty(ofdg.FileName))
                {
                    var internalVideo = new Video();
                    internalVideo.VideoSource = new Uri("file://" + ofdg.FileName);
                    internalVideo.VideoHeight = 320;
                    internalVideo.VideoWidth = 240;
                    internalVideo.Height = 480;
                    internalVideo.Width = 640;
                    Canvas.SetLeft(internalVideo, 100);
                    Canvas.SetTop(internalVideo, 100);
                    client.UploadAndSendVideo(new MeTLLib.DataTypes.MeTLStanzas.LocalVideoInformation
                    (client.location.currentSlide, client.username, "presentationSpace", "public", internalVideo, ofdg.FileName, false));
                }
            }
        }
        private void NewFile(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                var ofdg = new Microsoft.Win32.OpenFileDialog();
                ofdg.Multiselect = false;
                ofdg.ShowDialog();
                if (!String.IsNullOrEmpty(ofdg.FileName))
                {
                    var directoryPath = ofdg.FileName.TrimEnd(ofdg.SafeFileName.ToCharArray());
                    var directoryInfo = new System.IO.DirectoryInfo(directoryPath);
                    var fileLength = directoryInfo.GetFiles(ofdg.SafeFileName).ElementAt(0).Length;
                    client.UploadAndSendFile(new MeTLLib.DataTypes.MeTLStanzas.LocalFileInformation
                    (client.location.currentSlide, client.username, "presentationSpace", "public", ofdg.FileName, ofdg.SafeFileName, false, fileLength, DateTime.Now.ToString()));
                }
            }
        }
        private string describeParser(PreParser pp)
        {
            return "Location:" + pp.location.currentSlide.ToString() +
            " Ink:" + pp.ink.Count.ToString() +
            " Images:" + pp.images.Count.ToString() +
            " Text:" + pp.text.Count.ToString() +
            " Videos:" + pp.videos.Count.ToString() +
            " Submissions:" + pp.submissions.Count.ToString() +
            " Quizzes:" + pp.quizzes.Count.ToString() +
            " QuizAnswers:" + pp.quizAnswers.Count.ToString();

        }
    }
    public class TraceLevelFilter : TraceFilter
    {
        private TraceEventType traceLevel;
        public TraceLevelFilter(TraceEventType level)
            : base()
        {
            traceLevel = level;
        }
        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            if (eventType == traceLevel)
                return true;
            else
                return false;
        }
    }
    public class TraceLoggerItemsControl : ItemsControl
    {
        delegate void LogDelegate(string message, category level);
        public TraceEventType traceLevel
        {
            get { return (TraceEventType)GetValue(traceLevelProperty); }
            set { SetValue(traceLevelProperty, value); }
        }
        public static readonly DependencyProperty traceLevelProperty =
            DependencyProperty.Register("traceLevel", typeof(TraceEventType), typeof(TraceLoggerItemsControl), new UIPropertyMetadata(null));

        internal ObservableCollection<Label> traceLogStore = new ObservableCollection<Label>();
        private enum category { WARN, ERROR, INFO, UNKNOWN }
        public TraceLoggerItemsControl()
            : base()
        {
            this.Loaded += new RoutedEventHandler(TraceLoggerItemsControl_Loaded);
            this.ItemsSource = traceLogStore;
        }
        private void TraceLoggerItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            Trace.Listeners.Add(new TextBlockTraceLogger(doAffineLog) { Name = traceLevel.ToString(), Filter = new TraceLevelFilter(traceLevel) });
        }
        private void doAffineLog(string message, category cat)
        {
            Dispatcher.BeginInvoke(new LogDelegate(logMessage), DispatcherPriority.Normal, new object[] { message, cat });
        }
        private void logMessage(string message, category cat)
        {
            //Action del = delegate
            //{
            traceLogStore.Add(new Label { Content = message, ToolTip = cat });
            //};
            /*if (Thread.CurrentThread == Dispatcher.Thread)
                del();
            else
                Dispatcher.BeginInvoke(del);
            */
        }
        class TextBlockTraceLogger : TraceListener
        {
            private Action<string, category> log;
            public TextBlockTraceLogger(Action<string, category> logOutput)
                : base()
            {
                this.Name = "textBlockTraceLogger";
                log = logOutput;
            }
            public override void Write(string message)
            {
                if (log != null) AddMessage(message, category.UNKNOWN);
            }
            public override void WriteLine(string message)
            {
                if (log != null) AddMessage(message, category.UNKNOWN);
            }
            public override void WriteLine(string message, string inputCategory)
            {
                var cat = category.UNKNOWN;
                switch (inputCategory)
                {
                    case "warning":
                        cat = category.WARN;
                        break;
                    case "information":
                        cat = category.INFO;
                        break;
                    case "error":
                        cat = category.ERROR;
                        break;
                    default:
                        cat = category.UNKNOWN;
                        break;
                }
                AddMessage(message, cat);
                base.WriteLine(message, inputCategory);
            }
            private void AddMessage(string message, category cat)
            {
                message = message.Insert(0, DateTime.Now.ToString() + ": ");
                log(message, cat);
            }
        }
    }
}
