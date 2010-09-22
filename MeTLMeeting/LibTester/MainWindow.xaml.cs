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
            client.StatusChanged += (sender, args) => { Console.WriteLine("StatusChange(isConnected) = " + args.isConnected.ToString()); };
            client.StrokeAvailable += (sender, args) => { inkCanvas.Strokes.Add(args.stroke.stroke); };
            //Console.WriteLine("StrokeReceived: " + args.stroke.startingChecksum.ToString()); };
            client.TextBoxAvailable += (sender, args) => { Console.WriteLine("TextBoxReceived: " + args.textBox.box.Text); };
            client.ImageAvailable += (sender, args) => { Console.WriteLine("ImageReceieved: " + args.image.id); };
            client.PreParserAvailable += (sender, args) =>
            {
                var parser = ((PreParser)args.parser);
                foreach (TargettedStroke stroke in parser.ink)
                    inkCanvas.Strokes.Add(stroke.stroke);
                foreach (TargettedImage image in parser.images.Values)
                    inkCanvas.Children.Add(image.image);
                foreach (TargettedTextBox textBox in parser.text.Values)
                    inkCanvas.Children.Add(textBox.box);
                foreach (TargettedVideo video in parser.videos.Values)
                    inkCanvas.Children.Add(video.video);
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
            if (client != null && client.isConnected)
            {
                inkCanvas.Children.Clear();
                inkCanvas.Strokes.Clear();
                client.MoveTo(Int32.Parse(location.Text));
            }
            else Console.WriteLine("not yet logged in");
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
    }
}
