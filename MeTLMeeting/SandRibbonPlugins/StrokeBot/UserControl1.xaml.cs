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
using SandRibbon;
using System.Threading;
using System.Windows.Ink;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Markup;
using System.Xml.Linq;
using SandRibbonObjects;

namespace StrokeBot
{
    public static class ColorExtensions
    {
        public static Color Next(this Color color)
        {
            var random = new Random();
            var limit = 255;
            var increment = 50;
            return new Color
            {
                A = color.A,
                R = (byte)((color.R + random.Next(increment)) % limit),
                G = (byte)((color.G + random.Next(increment)) % limit),
                B = (byte)((color.B + random.Next(increment)) % limit)
            };
        }
    }
    public partial class PluginMain : UserControl
    {
        public static RoutedCommand StartStroking = new RoutedCommand();
        public static RoutedCommand StopStroking = new RoutedCommand();
        private bool isStroking = false;
        private int strokeCount = 0;
        private int strokesBeforeSlideChange = new Random().Next(60);
        private int currentSlide = 0;
        private Timer timer;
        private Stroke STROKE = new Stroke(new StylusPointCollection(Enumerable.Range(0, 100).Select((i) => new StylusPoint(i, i))));
        private StrokeCollection myNameInLights;
        private ConversationDetails details;
        private string username;
        private int location;
        public PluginMain()
        {
            InitializeComponent();
            Commands.LoggedIn.RegisterCommand(new DelegateCommand<string>((username)=>
            {
                try
                {
                    this.username = username;
                    generateHandwritingFor(username);
                    beginStroking();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>((location)=>this.location = location));
            Commands.ChangeConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(
                (details)=>this.details = details));
        }
        private void generateHandwritingFor(string content)
        {
            Dispatcher.Invoke((Action)delegate
            {
                var x = 0;
                var lettersAsStrokes = new StrokeCollection();
                var letters = XDocument.Load("strokes.xml");
                for (int i = 0; i < content.Length; i++)
                {
                    var letter = content.Substring(i, 1).ToLower();
                    var letterStrokes = letters.Descendants("stroke")
                        .Where(s => letter.Equals(s.Attribute("meaning").Value))
                        .Single().Attribute("content").Value;
                    var letterAsStrokes = new StrokeCollection(((InkCanvas)XamlReader.Parse(letterStrokes)).Strokes
                        .Select(s => new Stroke(new StylusPointCollection(s.StylusPoints.Select((p) => new StylusPoint(p.X + x, p.Y))))));
                    lettersAsStrokes.Add(letterAsStrokes);
                    x += 60;
                }
                myNameInLights = lettersAsStrokes;
            });
        }
        private void beginStroking()
        {
            if (timer != null)
                timer = null;
            timer = new Timer((_state) =>
            {
                Dispatcher.Invoke((Action)delegate
                {
                    if (++strokeCount % strokesBeforeSlideChange == 0)
                    {
                        var ids = details.Slides.Select(s=>s.id).ToList();
                        var currentIndex = ids.IndexOf(location);
                        location = (ids.Count() - 1) > currentIndex ? ids[location + 1] :ids[0];
                        Commands.MoveTo.Execute(location);
                    }
                    else
                    {
                        myNameInLights = new StrokeCollection(
                            from s in myNameInLights
                            select new Stroke(new StylusPointCollection(from p in s.StylusPoints
                                              select
                                                  new StylusPoint(p.X+2, p.Y)),
                                                  new DrawingAttributes { Color = s.DrawingAttributes.Color.Next() }));
                        foreach (var stroke in myNameInLights)
                        {
                            Commands.AddForeignStroke.Execute(
                                new SandRibbon.Utils.Connection.Interpreter.ForeignStroke { 
                                    author = username, location = location.ToString(), stroke = stroke });
                            Commands.AddMyStroke.Execute(stroke);
                        }
                    }
                    count.Text = strokeCount.ToString();
                });
            }, null, 0, 1000);
        }
    }
}
