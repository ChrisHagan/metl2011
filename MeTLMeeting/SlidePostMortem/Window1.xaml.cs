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
using SandRibbon.Utils.Connection;
using SandRibbon.Providers;
using System.Collections;
using agsXMPP.Xml;
using agsXMPP.Xml.Dom;
using System.Collections.ObjectModel;
using SandRibbon;
using System.Windows.Threading;
using SandRibbon.Components;
using SandRibbon.Components.Canvas;
using System.Windows.Ink;
using SandRibbonInterop;
using Controls = System.Windows.Controls;

namespace SlidePostMortem
{
    public partial class Window1 : Window
    {
        PageCoroner coroner;
        ObservableCollection<Node> nodes = new ObservableCollection<Node>();
        Dictionary<Point, HashSet<string>> labels = new Dictionary<Point,HashSet<string>>();
        JabberWire wire = new JabberWire();
        public static RoutedCommand Advance = new RoutedCommand();
        private DispatcherTimer timer;
        private static int side = 100;
        private static int location = 767401;
        public Window1()
        {
            InitializeComponent();
            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.ContextIdle, (_state,_context) =>
            {
                if (Advance.CanExecute(null,this))
                    Advance.Execute(null, this);
                labelContents();
            },Dispatcher);
            PreviewMouseMove += new MouseEventHandler(Window1_PreviewMouseMove);
            bounds.Height = side;
            bounds.Width = side;
        }
        void Window1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var square = gridSquareFor(e.GetPosition(stack));
            Canvas.SetLeft(bounds, square.X);
            Canvas.SetTop(bounds, square.Y);
            if (labels.ContainsKey(square))
                currentAuthors.Text = labels[square].Aggregate("Authors in this space:", (acc, item) => acc + " " + item);
            else
                currentAuthors.Text = "No authors in this space";
        }
        void labelContents()
        {
            foreach (var abstractCanvas in stack.stack.Children)
                showAuthorsFor((AbstractCanvas)abstractCanvas);
        }
        void showAuthorsFor(AbstractCanvas canvas)
        {
            foreach (var child in canvas.Children)
                showAuthorOf((UIElement)child);
            foreach (var stroke in canvas.Strokes)
                showAuthorOf(stroke);
        }
        void showAuthorOf(UIElement element)
        {
            string author = "Author unknown";
            if (element is System.Windows.Controls.Image)
                author = ((System.Windows.Controls.Image)element).tag().author;
            else if (element is System.Windows.Controls.TextBox)
                author = ((System.Windows.Controls.TextBox)element).tag().author;
            rememberAuthor(author, new Point
            {
                X = InkCanvas.GetLeft(element),
                Y = InkCanvas.GetTop(element)
            });
        }
        void showAuthorOf(Stroke stroke)
        {
            string author = stroke.tag().author;
            var sp = stroke.StylusPoints[0];
            rememberAuthor(author, new Point { X = sp.X, Y = sp.Y });
        }
        void rememberAuthor(string author, Point location)
        {
            var square = gridSquareFor(location);
            if (labels.ContainsKey(square))
                labels[square].Add(author);
            else
                labels[square] = new HashSet<string> { author };
        }
        Point gridSquareFor(Point point)
        {
            return new Point
            {
                X = point.X - point.X % side,
                Y = point.Y - point.Y % side
            };
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Commands.MoveTo.Execute(location);
            coroner = new PageCoroner(location, this);
        }
        public void LoadNodes(List<Node> nodes)
        {
            foreach(var node in nodes)
                this.nodes.Add(node);
        }
        private void CommandBinding_CanExecute(object _sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = nodes.Count > 0;
        }
        private void CommandBinding_Executed(object _sender, ExecutedRoutedEventArgs _e)
        {
            var next = nodes[0];
            nodes.RemoveAt(0);
            wire.ReceivedMessage(next);
        }
    }
    public class PageCoroner : HttpHistoryProvider
    {
        public List<Node> nodes = new List<Node>();
        private Window1 navigator;
        public PageCoroner(int location, Window1 navigator)
        {
            this.navigator = navigator;
            Retrieve<PreParser>(null, null, (_preParser) => {
                navigator.LoadNodes(nodes);
                CommandManager.InvalidateRequerySuggested();
            }, location.ToString());
        }
        protected override void parseHistoryItem(string item, JabberWire _wire)
        {
            var parser = new StreamParser();
            parser.OnStreamElement += (_sender, node) =>
                 nodes.Add(node);
            var history = item + "</logCollection>";
            parser.Push(Encoding.UTF8.GetBytes(history), 0, history.Length);
        }
    }
}