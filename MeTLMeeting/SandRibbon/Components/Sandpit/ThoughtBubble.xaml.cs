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
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers.Structure;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonInterop.MeTLStanzas;
using System.Windows.Ink;
using SandRibbonObjects;
using SandRibbon.Quizzing;

namespace SandRibbon.Components.Sandpit
{
    public partial class ThoughtBubble : UserControl
    {
        public Point position = new Point(0,0);
        public int parent;
        public string conversation;
        public List<Stroke> strokeContext;
        private bool opened = false;
        public int room;
        public string me;
        public List<FrameworkElement> childContext;

        public ThoughtBubble()
        {
            InitializeComponent();
            strokeContext = new List<Stroke>();
            childContext = new List<FrameworkElement>();
            thought.stack.handwriting.target = "thoughtBubble";
            thought.stack.text.target = "thoughtBubble";
            thought.stack.images.target = "thoughtBubble";
            thought.stack.handwriting.actualPrivacy = "public";
            thought.stack.text.actualPrivacy = "public";
            thought.stack.images.actualPrivacy = "public";
            thought.stack.handwriting.defaultPrivacy = "public";
            thought.stack.text.defaultPrivacy = "public";
            thought.stack.images.defaultPrivacy = "public";
            Commands.ThoughtLiveWindow.RegisterCommand(new DelegateCommand<Rectangle>(mainSlideLiveWindow));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(PreParserAvailable));
        }
        private void PreParserAvailable(PreParser parser)
        {
            thought.stack.handwriting.ReceiveStrokes(parser.ink);
            thought.stack.images.ReceiveImages(parser.images.Values);
            foreach (var text in parser.text.Values)
                thought.stack.text.doText(text);
            Worm.heart.Interval = TimeSpan.FromMilliseconds(1500);
        }
        public void setIdentities()
        {
            thought.stack.handwriting.me = me;
            thought.stack.text.me = me;
            thought.stack.images.me = me;
            thought.stack.handwriting.currentSlideId = room;
            thought.stack.text.currentSlideId = room;
            thought.stack.images.currentSlideId = room;
        }
        private void mainSlideLiveWindow(Rectangle liveWindow)
        {
            Dispatcher.Invoke((Action) delegate
                                           {
                                               var RLW = new RenderedLiveWindow()
                                               {
                                                   Rectangle = liveWindow,
                                                   Height = liveWindow.Height,
                                                   Width = liveWindow.Width
                                               };
                                               RLWViewBox.Child = RLW;
                                               RLW.MouseUp += ThoughtBubbleSpace_MouseDown;
                                               System.Windows.Controls.Canvas.SetLeft(RLWViewBox, 0);
                                               System.Windows.Controls.Canvas.SetTop(RLWViewBox, 0);
                                           });
        }

        public ThoughtBubble relocate() 
        {
            var bounds = getBounds();
            position = new Point(bounds.X + bounds.Width, bounds.Y - bounds.Height/2);
            return this;
        }
        public void enterBubble()
        {
            Commands.SneakInto.Execute(room.ToString());
        }
        private Rect getBounds()
        {
            var listX = new List<Double>();
            var listY = new List<Double>();
            var strokes = new StrokeCollection(strokeContext);
            listX.Add(strokes.GetBounds().X); 
            listX.Add(strokes.GetBounds().X + strokes.GetBounds().Width); 
            listY.Add(strokes.GetBounds().Y); 
            listY.Add(strokes.GetBounds().Y + strokes.GetBounds().Height); 
            foreach (var child in childContext)
            {
                listX.Add(System.Windows.Controls.Canvas.GetLeft(child));
                listX.Add(System.Windows.Controls.Canvas.GetLeft(child) + child.Width);
                listY.Add(System.Windows.Controls.Canvas.GetTop(child));
                listY.Add(System.Windows.Controls.Canvas.GetTop(child) + child.Height);
            }
            if (listX.Count > 0)
                return new Rect
                {
                    X = listX.Min(),
                    Y = listY.Min(),
                    Width = listX.Max() - listX.Min(),
                    Height = listY.Max() - listY.Min()
                };
            else
                return new Rect();
        }
        private void move(Point point)
        {
            System.Windows.Controls.Canvas.SetLeft(this, point.X);
            System.Windows.Controls.Canvas.SetTop(this, point.Y);
        }
        private void setThoughtAccess(bool access)
        {
            thought.stack.handwriting.SetCanEdit(access);
            thought.stack.text.SetCanEdit(access);
            thought.stack.images.SetCanEdit(access);
        }
        private void ThoughtBubbleSpace_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(!opened)
            {
                position = new Point(0, 0);
                thoughtView.Width = ((System.Windows.Controls.Canvas)Parent).ActualWidth;
                thoughtView.Height = ((System.Windows.Controls.Canvas)Parent).ActualHeight;
                thought.MouseLeftButtonUp -= ThoughtBubbleSpace_MouseDown;
                RLWViewBox.Visibility = Visibility.Visible;
                setIdentities();
                Dispatcher.BeginInvoke((Action) delegate
                {
                    setThoughtAccess(true);
                });
                Commands.ExploreBubble.Execute(this);
            }
            else
            {
                relocate();
                thought.MouseLeftButtonUp += ThoughtBubbleSpace_MouseDown;
                RLWViewBox.Visibility = Visibility.Collapsed;
                thoughtView.Width = 40;
                thoughtView.Height = 40;
                setThoughtAccess(false);
            
            }
            move(position);
            opened = !opened;
        }
    }
}