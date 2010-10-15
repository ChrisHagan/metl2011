using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Canvas;
using SandRibbon.Providers;
using SandRibbonInterop;
using SandRibbonInterop.MeTLStanzas;
using CheckBox=System.Windows.Controls.CheckBox;
using MeTLLib.DataTypes;

namespace SandRibbon.Components
{
    public partial class Friends
    {
        public ObservableCollection<VisibilityInformation> publishers;
        public Friends()
        {
            InitializeComponent();
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(_obj => history.Children.Clear()));
            Commands.ReceiveChatMessage.RegisterCommand(new DelegateCommand<TargettedTextBox>(receiveChatMessage));
            publishers = new ObservableCollection<VisibilityInformation>();
            Commands.ReceiveAuthor.RegisterCommand(new DelegateCommand<string>(ReceiveAuthor));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>(moveTo));
            users.ItemsSource = publishers;
        }

        private void moveTo(object obj)
        {
            publishers.Clear();
            if (Globals.me == Globals.conversationDetails.Author)
                myToggle.Visibility = Visibility.Collapsed;
        }
        private void ReceiveAuthor(string author )
        {
                if(publishers.ToList().Where(uv => uv.user == author).Count() == 0)
                {
                    publishers.Add(new VisibilityInformation
                                       {
                                           user = author,
                                           visible = true
                                       });
                }
        }
        private void teacherToggle(object sender, RoutedEventArgs e)
        {
            var user = publishers.Where(uv =>uv.user == "Teacher").First();
            user.visible = ((CheckBox)sender).IsChecked == true;
            Commands.UserVisibility.Execute(new VisibilityInformation
                                                {
                                                    user = "toggleTeacher",
                                                    visible =((CheckBox) sender).IsChecked == true
                                                });
        }
        private void meToggle(object sender, RoutedEventArgs e)
        {
            publishers.Where(uv =>uv.user == Globals.me).First().visible = ((CheckBox)sender).IsChecked == true;
            Commands.UserVisibility.Execute(new VisibilityInformation
                                                {
                                                    user = "toggleMe",
                                                    visible =((CheckBox) sender).IsChecked == true
                                                });
        }
        private void otherToggle(object sender, RoutedEventArgs e)
        {
            foreach(var uv in publishers)
            {
                if (uv.user != "Teacher" && uv.user != Globals.me)
                    uv.visible = ((CheckBox) sender).IsChecked == true;

            }
            Commands.UserVisibility.Execute(new VisibilityInformation
                                                {
                                                    user = "toggleStudents",
                                                    visible = ((CheckBox) sender).IsChecked == true
                                                });
        }
        //Chat functionality
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
            message.tag(new MeTLLib.DataTypes.TextTag
            {
                author = Globals.me,
                privacy = "public",
                id = string.Format("{0}:{1}", Globals.me, SandRibbonObjects.DateTimeFactory.Now())
            });
            Commands.SendChatMessage.Execute(new TargettedTextBox
            (0,Globals.me,"chat","public",message));
            clearTextInput();
        }

        private void AddMessage(TextBox msg)
        {
            Dispatcher.adoptAsync(delegate
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

        private void userClick(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.CheckBox) sender;
            Commands.UserVisibility.Execute(new VisibilityInformation
                                                {
                                                    user = ((VisibilityInformation)button.DataContext).user,
                                                    visible = button.IsChecked == true
                                                });
        }


    }
}
