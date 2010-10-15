using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using SandRibbon.Components.Canvas;
using SandRibbon.Providers.Structure;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonInterop.MeTLStanzas;
using System.Windows.Ink;
using SandRibbonObjects;
using SandRibbon.Quizzing;
using Image=SandRibbon.Components.Canvas.Image;
using MeTLLib.DataTypes;

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
        public List<FrameworkElement> childContext;
        private ImageBrush thoughtMask;
        public ThoughtBubble()
        {
            InitializeComponent();
            strokeContext = new List<Stroke>();
            childContext = new List<FrameworkElement>();
            Commands.ThoughtLiveWindow.RegisterCommand(new DelegateCommand<ThoughtBubbleLiveWindow>(mainSlideLiveWindow));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(PreParserAvailable));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(mainWindowMove));
            var uri = new Uri(@"..\..\Resources\thoughtOutline.png", UriKind.RelativeOrAbsolute);
            var image = new BitmapImage(uri);
            thoughtMask = new ImageBrush
                              {
                                  ImageSource = image 
                              };
            thoughtBorder.OpacityMask = thoughtMask;
            thought.Opacity = 0.3;
        }
        private void mainWindowMove(int newSlide)
        {
            if (newSlide != parent)
                Commands.SneakOutOf.Execute(room.ToString());
        }
        private void PreParserAvailable(PreParser parser)
        {
            thought.stack.handwriting.ReceiveStrokes(parser.ink);
            thought.stack.images.ReceiveImages(parser.images.Values);
            foreach (var text in parser.text.Values)
                thought.stack.text.doText(text);
            Worm.heart.Interval = TimeSpan.FromMilliseconds(1500);
        }
        private void mainSlideLiveWindow(ThoughtBubbleLiveWindow thoughtBubbleLiveWindow)
        {
            if(thoughtBubbleLiveWindow.Bubble.room != room)return;
            Dispatcher.adopt((Action) delegate
                                           {
                                               var RLW = new RenderedLiveWindow()
                                               {
                                                   Rectangle = thoughtBubbleLiveWindow.LiveWindow,
                                                   Height = thoughtBubbleLiveWindow.LiveWindow.Height,
                                                   Width = thoughtBubbleLiveWindow.LiveWindow.Width
                                               };
                                               RLWViewBox.Child = RLW;
                                               RLW.PreviewMouseLeftButtonUp += toggleThoughtBubble;
                                               System.Windows.Controls.Canvas.SetLeft(RLWViewBox, 0);
                                               System.Windows.Controls.Canvas.SetTop(RLWViewBox, 0);
                                           });
        }

        public ThoughtBubble relocate() 
        {
            var bounds = getBounds();
            position = new Point(Math.Abs(bounds.X + 40), Math.Abs(bounds.Y - 80));
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
            if (strokes.Count > 0)
            {
                listX.Add(strokes.GetBounds().X);
                listY.Add(strokes.GetBounds().Y);
            }
            foreach (var child in childContext)
            {
                listX.Add(InkCanvas.GetLeft(child));
                listY.Add(InkCanvas.GetTop(child));
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
        private void toggleThoughtBubble(object sender, MouseButtonEventArgs e)
        {
            if(!opened)
            {
                
                thoughtBorder.OpacityMask = null;
                thought.Opacity = 1;
                position = new Point(0, 0);
                thought.IsHitTestVisible = true;
                thoughtView.MouseLeftButtonUp-= toggleThoughtBubble;
                thoughtView.Width = ((System.Windows.Controls.Canvas)Parent).ActualWidth;
                thoughtView.Height = ((System.Windows.Controls.Canvas)Parent).ActualHeight;
                RLWViewBox.Visibility = Visibility.Visible;
                Dispatcher.adoptAsync(() => setThoughtAccess(true));
                Commands.ExploreBubble.Execute(this);
            }
            else
            {
                relocate();

                thoughtBorder.OpacityMask = thoughtMask;
                thought.Opacity = 0.3;
                thoughtView.MouseLeftButtonUp +=new MouseButtonEventHandler(toggleThoughtBubble);
                RLWViewBox.Visibility = Visibility.Collapsed;
                thoughtView.Width = 80;
                thoughtView.Height = 80;
                setThoughtAccess(false);
            
            }
            move(position);
            opened = !opened;
        }
        private void thought_MouseEnter(object sender, MouseEventArgs e)
        {
            if(!opened)
               thought.Opacity = 0.7;
            foreach(var stroke in strokeContext)
            {
                var bounds = stroke.GetBounds();
                var verticies = new[] {bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft};
                Commands.Highlight.Execute(new HighlightParameters
                                               {
                                                   color = Colors.Blue,
                                                   verticies = verticies
                                               });
            }
            foreach(var child in childContext)
            {
                IEnumerable<Point>  points;
                if (child.GetType() == typeof(TextBox))
                    points = Text.getTextPoints((TextBox) child);
                else
                    points = Image.getImagePoints((System.Windows.Controls.Image) child);
                if(points != null)
                    Commands.Highlight.Execute(new HighlightParameters
                                                   {
                                                       color = Colors.Blue,
                                                       verticies =points 
                                                   });
            }
        }
        private void thought_MouseLeave(object sender, MouseEventArgs e)
        {
            if(!opened)
                thought.Opacity = 0.3;
            foreach (var stroke in strokeContext)
            {
                var bounds = stroke.GetBounds();
                var verticies = new[] {bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft};
                Commands.RemoveHighlight.Execute(new HighlightParameters
                                                     {
                                                         color = Colors.Blue,
                                                         verticies = verticies
                                                     });
            }
            foreach (var child in childContext)
            {
                IEnumerable<Point> points;
                if (child.GetType() == typeof (TextBox))
                    points = Text.getTextPoints((TextBox) child);
                else
                    points = Image.getImagePoints((System.Windows.Controls.Image) child);
                if (points != null)
                    Commands.RemoveHighlight.Execute(new HighlightParameters
                                                         {
                                                             color = Colors.Blue,
                                                             verticies = points
                                                         });

            }
        }

        public void overrideCanvasDefaults()
        {
            thought.stack.handwriting.currentSlide = room;
            thought.stack.text.currentSlide = room;
            thought.stack.images.currentSlide = room;
        }
    }

    public class ThoughtBubbleLiveWindow
    {
        public Rectangle LiveWindow;
        public ThoughtBubble Bubble;
    }
}