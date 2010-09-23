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
using MeTLLib.Providers;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;

namespace LibTester
{
    public partial class MainWindow : Window
    {
        private ClientConnection client;
        public MainWindow()
        {
            InitializeComponent();
            client = new ClientConnection();
            client.LogMessageAvailable += (sender, args) => { Console.WriteLine(args.logMessage); };
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
            inkCanvas.Children.Clear();
            inkCanvas.Strokes.Clear();
            client.MoveTo(Int32.Parse(location.Text));
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
    }
}
