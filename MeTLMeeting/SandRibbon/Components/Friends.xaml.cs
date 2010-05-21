using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbonInterop;
using SandRibbonInterop.MeTLStanzas;

namespace SandRibbon.Components
{
    public partial class Friends
    {
        public DelegateCommand<string> setAuthor;
        public Friends()
        {
            InitializeComponent();
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(_obj => history.Children.Clear()));
            Commands.ReceiveChatMessage.RegisterCommand(new DelegateCommand<TargettedTextBox>(receiveChatMessage));
        }

        public void SetWidth(double width)
        {
            friendsDock.Width = width;
        }
        private void sendKeyMessage(object sender, KeyEventArgs e)
        {
            if (messageField.Text == "") return;
            if (e.Key == Key.Return)
                SendMessage();
        }
        private void sendMessage(object sender, RoutedEventArgs e)
        {
            if (messageField.Text == "") return;
            SendMessage();
        }
        public void SendMessage()
        {
            var message = messageField;
            message.Text = SandRibbonObjects.DateTimeFactory.Now().ToString() + " " + Globals.me + ":\n" + message.Text;
            message.tag(new TextTag
            {
                author = Globals.me,
                privacy = "public",
                id = string.Format("{0}:{1}", Globals.me, SandRibbonObjects.DateTimeFactory.Now())
            });
            Commands.SendChatMessage.Execute(new TargettedTextBox
                {
                    box = message,
                    author = Globals.me,
                    privacy = "public",
                    slide = 0,
                    target = "chat",
                });
            clearTextInput();
        }

        private void AddMessage(TextBox msg)
        {
            Dispatcher.BeginInvoke((Action)delegate
                                          {
                                              var block = new TextBlock
                                              {
                                                  Text = msg.Text,
                                                  Foreground = (Brush)msg.Foreground,
                                                  TextWrapping = TextWrapping.Wrap
                                              };
                                              history.Children.Add(block);
                                              historyScrollViewer.ScrollToBottom();
                                          });
        }
        private void clearTextInput()
        {
            messageField.Text = "";
        }

        private void receiveChatMessage(TargettedTextBox box)
        {
            var textBox = box.box;
            textBox.BorderThickness = new Thickness(0);
            if (!messageAllowed(textBox)) return;
            AddMessage(textBox);
        }

        private bool messageAllowed(TextBox box)
        {
            var message = box.Text.Split(new[] { ":\n" }, StringSplitOptions.RemoveEmptyEntries);
            var parts = message[1].Split(new[] { ' ' }, 2);
            if (!parts[0].StartsWith("@"))
                return true;
            
            box.Foreground = new SolidColorBrush(Colors.Purple);
            
            var target = parts[0];
            target = target.Remove(0, 1);
            if (message[0].Contains(Globals.me))
            {
                box.Text = message[0] + ": (whisper " + target + ") " + ": " + parts[1];
                return true;
            }
            if (target == Globals.me)
            {
                box.Text = message[0] + ": (private message to me)" + ": " + parts[1];
                return true;
            }
            return false;
        }
    }
}
