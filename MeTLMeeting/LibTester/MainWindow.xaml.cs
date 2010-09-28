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

namespace LibTester
{
    public partial class MainWindow : Window
    {
        private ClientConnection client;
        public MainWindow()
        {
            InitializeComponent();
            client = ClientFactory.Connection();
            client.StrokeAvailable += (sender, args) => { inkCanvas.Strokes.Add(args.stroke.stroke); };
            client.TextBoxAvailable += (sender, args) => { inkCanvas.Children.Add(args.textBox.box); };
            client.ImageAvailable += (sender, args) => { inkCanvas.Children.Add(args.image.image); };
            client.VideoAvailable += (sender, args) =>
            {
                var me = args.video.video.MediaElement;
                me.LoadedBehavior = MediaState.Play;
                Canvas.SetLeft(me, args.video.X);
                Canvas.SetTop(me, args.video.Y);
                me.Width = args.video.Width;
                me.Height = args.video.Height;
                inkCanvas.Children.Add(me);

            };
            client.PreParserAvailable += (sender, args) =>
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
            };
            username.Text = "eecrole";
            password.Password = "m0nash2008";
            attemptToAuthenticate(this, new RoutedEventArgs());
        }
        private void attemptToAuthenticate(object sender, RoutedEventArgs e)
        {
            client.Connect(username.Text, password.Password);
            checkConversations();
        }
        private void checkConversations()
        {
            if (client != null && client.isConnected)
            {
                foreach (ConversationDetails details in client.AvailableConversations)
                {
                    Console.WriteLine(details.Jid + " : " + details.Title);
                }
            }
        }
        private void moveTo(object sender, RoutedEventArgs e)
        {
            if (client == null) return;
            inkCanvas.Children.Clear();
            inkCanvas.Strokes.Clear();
            client.MoveTo(Int32.Parse(location.Text));
        }
        private void getHistory(object sender, RoutedEventArgs e)
        {
            if (client == null) return;
            var parser = client.RetrieveHistoryOf(location.Text);
            MessageBox.Show(describeParser(parser));
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
                var newStroke = new TargettedStroke
                {
                    stroke = internalStroke,
                    privacy = "public",
                    author = client.username,
                    slide = client.location.currentSlide,
                    target = "presentationSpace",
                };
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
                var newTargettedTextBox = new TargettedTextBox
                {
                    privacy = "public",
                    author = client.username,
                    slide = client.location.currentSlide,
                    target = "presentationSpace",
                    box = internalTextBox,
                };
                client.SendTextBox(newTargettedTextBox);
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
        private void doAffineLog(string message, category cat) {
            Dispatcher.BeginInvoke(new LogDelegate(logMessage), DispatcherPriority.Normal, new object[]{message,cat});
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
