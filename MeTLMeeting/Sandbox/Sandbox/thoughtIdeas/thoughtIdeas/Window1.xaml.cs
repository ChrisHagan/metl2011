using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon;
using SandRibbon.Connection;
using SandRibbon.Utils.Connection;
using System.Windows.Media.Animation;

namespace ThoughtIdeas
{
    public partial class Window1 : Window
    {
        public static RoutedCommand addThought = new RoutedCommand();
        public static Window1 window;
        public static Interpreter.UserInformation userInformation = new Interpreter.UserInformation();
        public Window1()
        {
            InitializeComponent();
            window = this;
            canvas.Width = window.Width;
            canvas.Height = window.Height;
            Loaded += (sender, args) =>
            {
                connect();
            };
        }
        public delegate void StrokeCollectedHandler(Stroke stroke);
        public StrokeCollectedHandler strokedCollectedHandled;
        public delegate void NavigationRequester();
        public event NavigationRequester NavigationRequested;
        private int room = 100000;
        private JabberWire wire;
        private string username;
        private void connect()
        {
            username = System.Environment.MachineName;
            //username = new Random().Next().ToString();
            userInformation.credentials = new Interpreter.Credentials {name = username, password = "aPassword"};
            userInformation.location = new Interpreter.Location { currentSlide = room };
            wire = new JabberWire(username, null, userInformation.credentials);
            strokedCollectedHandled += (stroke) => wire.Send(stroke);
            NavigationRequested += () => wire.SneakOutOf((room+1).ToString());
            Commands.GotoThread.RegisterCommand(
                new DelegateCommand<string>((location) =>
                {
                    Return.Visibility = Visibility.Visible;
                    ((Storyboard)FindResource("recede")).Begin();
                    canvasBorder.BorderBrush = Brushes.Black;
                    view.Width = 100;
                    wire.SneakIntoWithHistory(location);
                    if(threadCanvassesById.Keys.Contains(location))
                        canvasses.Children.Add(threadCanvassesById[location]);
                    if(thoughtCanvassesById.Keys.Contains(location))
                    {
                        Promote.Visibility = Visibility.Visible;
                        Promote.Tag = location;
                        canvasses.Children.Add(thoughtCanvassesById[location]);
                    }
                }
            ));            
            wire.Login( new Solder(), userInformation.location);
            Commands.AddForeignStroke.RegisterCommand(
                new DelegateCommand<Interpreter.ForeignStroke>(addForeignStroke));
            Commands.AddForeignThread.RegisterCommand(
               new DelegateCommand<Interpreter.ConversationThread>(addConversationThread) 
           );
        }
        private Dictionary<string, InkCanvas> threadCanvassesById = new Dictionary<string,InkCanvas>();
        private Dictionary<string, InkCanvas> thoughtCanvassesById = new Dictionary<string,InkCanvas>();
        private void addConversationThread(Interpreter.ConversationThread obj)
        {
            Dispatcher.Invoke((Action)delegate
            {
                var newCanvas = new InkCanvas { Height = this.Height, Width = this.Width };
                newCanvas.Tag = obj.slideId;
                newCanvas.StrokeCollected += threadStrokeCollected;
                
                if (obj.thread)
                {
                    if (thoughtCanvassesById.Keys.Contains(obj.slideId))
                        removeThought(obj);
                    AdornerLayer.GetAdornerLayer(canvas).Add(new ThreadAdorner(obj.x, obj.y, canvas, obj, newCanvas, true));
                    threadCanvassesById.Add(obj.slideId.ToString(), newCanvas);
                }
                else if(!obj.thread && obj.author == username)
                {
                    AdornerLayer.GetAdornerLayer(canvas).Add(new ThreadAdorner(obj.x, obj.y, canvas, obj, newCanvas, false));
                    if(!thoughtCanvassesById.ContainsKey(obj.slideId.ToString()))
                        thoughtCanvassesById.Add(obj.slideId.ToString(), newCanvas);
                }
            });
        }
        private void removeThought(Interpreter.ConversationThread obj)
        {
            var adornerLayers = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayers != null)
            {
                var adorners = adornerLayers.GetAdorners(canvas);
                if (adorners != null)
                {
                    for (var i = 0; i < adorners.Count(); i++)
                    {
                        var element = adorners[i];
                        if (element.GetType().Name == "ThreadAdorner")
                        {
                            var threadAdorner = ((ThreadAdorner) element);
                            if (threadAdorner.id == obj.slideId && !threadAdorner.thread)
                                adornerLayers.Remove(element);
                        }
                    }
                }
            }
            thoughtCanvassesById.Remove(obj.slideId);
        }

        private void threadStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var threadRoom = ((InkCanvas) sender).Tag.ToString();
            wire.SendToRoom(threadRoom, e.Stroke);
        }
        private void addForeignStroke(Interpreter.ForeignStroke foreignStroke)
        {
            Dispatcher.Invoke((Action) (() =>
                                            {
                                                if (threadCanvassesById.Keys.Contains(foreignStroke.location))
                                                    threadCanvassesById[foreignStroke.location].Strokes.Add(foreignStroke.stroke);
                                                else if(thoughtCanvassesById.Keys.Contains(foreignStroke.location))
                                                    thoughtCanvassesById[foreignStroke.location].Strokes.Add(foreignStroke.stroke);
                                                else
                                                    canvas.Strokes.Add(foreignStroke.stroke);
                                            }));
        }
        private void SelectMode(object sender, RoutedEventArgs e)
        {
            canvas.EditingMode = InkCanvasEditingMode.Select;
        }
        private void InkMode(object sender, RoutedEventArgs e)
        {
            canvas.EditingMode = InkCanvasEditingMode.Ink;
        }
        private void AddThought(object sender, RoutedEventArgs e)
        {
            if(canvas.ActiveEditingMode == InkCanvasEditingMode.Select && canvas.GetSelectedStrokes().Count() > 0)
            {
                var name = (room + 1 + thoughtCanvassesById.Count +threadCanvassesById.Count).ToString();
                var X = canvas.GetSelectionBounds().TopRight.X;
                var Y = canvas.GetSelectionBounds().TopRight.Y;
                var context = new Interpreter.ConversationThread
                                  {
                                      author = username,
                                      thread = false,
                                      slideId = name,
                                      title = name,
                                      x = X,
                                      y = Y,
                                  };
                var myThread = new Thread(context);
                InkCanvas.SetLeft(myThread, X + 20);
                InkCanvas.SetTop(myThread, Y - 20);
                wire.SendThread(context, room);           
            }
        }
        private void createThread(object sender, RoutedEventArgs e)
        {
            if (canvas.ActiveEditingMode == InkCanvasEditingMode.Select)
            {
                var name = (room + 1 + thoughtCanvassesById.Count + threadCanvassesById.Count).ToString();
                var X = canvas.GetSelectionBounds().TopRight.X;
                var Y = canvas.GetSelectionBounds().TopRight.Y;
                var context = new Interpreter.ConversationThread
                                  {
                                      author = username,
                                      thread = true,
                                      slideId = name,
                                      title = name,
                                      x = X,
                                      y = Y,
                                  };
                var myThread = new Thread(context);
                InkCanvas.SetLeft(myThread, X + 20);
                InkCanvas.SetTop(myThread, Y - 20);
                wire.SendThread(context, room);
            }
        }
        private void strokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (strokedCollectedHandled != null)
                strokedCollectedHandled(e.Stroke);
        }
        private void returnToConversation(object sender, RoutedEventArgs e)
        {
            if(NavigationRequested != null)
                NavigationRequested();

            foreach (var canvas in thoughtCanvassesById.Values)
                canvasses.Children.Remove(canvas);
            foreach (var canvas in threadCanvassesById.Values)
                canvasses.Children.Remove(canvas);
            Return.Visibility = Visibility.Collapsed;
            Promote.Visibility = Visibility.Collapsed;
            ((Storyboard)FindResource("return")).Begin();
            canvasBorder.BorderBrush = Brushes.Transparent;

        }


        private void Navigation(object sender, RoutedEventArgs e)
        {
            wire.MoveTo(room);
            join.Visibility = Visibility.Collapsed;
        }

        private void PromoteThought(object sender, RoutedEventArgs e)
        {
            var location = ((Button) sender).Tag.ToString();
            double x = 0;
            double y = 0;
            var adornerLayers = AdornerLayer.GetAdornerLayer(canvas);
            if(adornerLayers != null)
            {
                var adorners = adornerLayers.GetAdorners(canvas); 
                if(adorners != null)
                {
                    foreach(var element in adorners)
                    {
                        if(element.GetType().Name == "ThreadAdorner")
                        {
                            var threadAdorner = ((ThreadAdorner) element);
                           if(threadAdorner.id == location)
                           {
                               x = threadAdorner.x;
                               y = threadAdorner.y;
                           }
                        }
                    }
                }
            }
            var context = new Interpreter.ConversationThread
                              {
                                  author = username,
                                  slideId = location,
                                  thread = true,
                                  title = location,
                                  x =x,
                                  y =y

                              };
            wire.SendThread(context, room);
            returnToConversation(null, null);
        }
    }
}
