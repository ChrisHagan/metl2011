using System;
using Microsoft.Ink;
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
using System.Collections.ObjectModel;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.Providers.Connection;
using System.IO;
using System.Windows.Ink;

namespace SandRibbon.Components
{
    public class BannedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((value is bool) && (bool)value)
                return Brushes.Red;
            else return Brushes.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class BanButtonBannedToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //If the user is the conversion author, do not display the ban button
            if ((string)values[1] == SandRibbon.Providers.Globals.conversationDetails.Author)
            {
                return Visibility.Collapsed;
            }

            //If isBanned, then do not show ban button, else show
            if ((bool)values[0])
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class MeTLUser : DependencyObject
    {
        public string username { private set; get; }
        public Dictionary<string, int> usages = new Dictionary<string, int>();
        public string themes
        {
            get
            {
                return (string)GetValue(themeProperty);
            }
            set
            {
                SetValue(themeProperty, value);
            }
        }
        public static readonly DependencyProperty themeProperty = DependencyProperty.Register("themes", typeof(string), typeof(MeTLUser), new UIPropertyMetadata(""));
        public int activityCount
        {
            get
            {
                return (int)GetValue(activityCountProperty);
            }
            set
            {
                SetValue(activityCountProperty, value);
            }
        }
        public static readonly DependencyProperty activityCountProperty = DependencyProperty.Register("activityCount", typeof(int), typeof(MeTLUser), new UIPropertyMetadata(0));
        public int submissionCount
        {
            get
            {
                return (int)GetValue(submissionCountProperty);
            }
            set
            {
                SetValue(submissionCountProperty, value);
            }
        }
        public static readonly DependencyProperty submissionCountProperty = DependencyProperty.Register("submissionCount", typeof(int), typeof(MeTLUser), new UIPropertyMetadata(0));

        public MeTLUser(string user)
        {
            username = user;
            activityCount = 0;
            submissionCount = 0;
        }
    }
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Participants : UserControl
    {
        public Dictionary<string, MeTLUser> people = new Dictionary<string, MeTLUser>();
        public Participants()
        {
            InitializeComponent();
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<List<TargettedStroke>>(ReceiveStrokes));
            Commands.ReceiveStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(ReceiveStroke));
            Commands.ReceiveTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>(ReceiveTextbox));
            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<TargettedImage>(ReceiveImage));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(ReceivePreParser));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(ReceiveConversationDetails));
            Commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(ReceiveSubmission));
        }
        private void JoinConversation(string newJid)
        {
            ClearList();
        }
        private void ClearList()
        {
            Dispatcher.adopt(() =>
            {
                people.Clear();
            });
        }
        private void ReceiveSubmission(TargettedSubmission sub)
        {
            if (sub.target == "bannedcontent")
            {
                foreach (var historicallyBannedUser in sub.blacklisted)
                {
                    constructPersonFromUsername(historicallyBannedUser.UserName);
                }
            }
            RegisterAction(sub.author);
            RegisterSubmission(sub.author);
        }
        private void ReceiveConversationDetails(ConversationDetails details)
        {
            if (details.Jid == SandRibbon.Providers.Globals.conversationDetails.Jid)
            {
                foreach (var bannedUsername in details.blacklist)
                {
                    Dispatcher.adoptAsync(() =>
                    {
                        constructPersonFromUsername(bannedUsername);
                    });
                }
            }
        }

        private void ReceiveStrokes(List<TargettedStroke> strokes)
        {
            foreach (var s in strokes)
            {
                ReceiveStroke(s);
            }
            using (MemoryStream ms = new MemoryStream())
            {
                new StrokeCollection(strokes.Select(s => s.stroke)).Save(ms);
                var myInkCollector = new InkCollector();
                var ink = new Ink();
                ink.Load(ms.ToArray());

                using (RecognizerContext myRecoContext = new RecognizerContext())
                {
                    RecognitionStatus status;
                    myRecoContext.Strokes = ink.Strokes;
                    var recoResult = myRecoContext.Recognize(out status);

                    if (status == RecognitionStatus.NoError)
                    {
                        MessageBox.Show(recoResult.TopString);
                    }
                    else
                    {
                        MessageBox.Show("ERROR: " + status.ToString());
                    }
                }
            }
        }
        private void RegisterAction(string username)
        {
            Dispatcher.adopt(() =>
            {
                constructPersonFromUsername(username);
                people[username].activityCount++;
            });
        }
        private void RegisterSubmission(string username)
        {
            Dispatcher.adopt(() =>
            {
                constructPersonFromUsername(username);
                people[username].submissionCount++;
            });
        }
        private void ReceiveStroke(TargettedStroke s)
        {
            RegisterAction(s.author);
        }
        private void ReceiveTextbox(TargettedTextBox t)
        {
            RegisterAction(t.author);
        }
        private void ReceiveImage(TargettedImage i)
        {
            RegisterAction(i.author);
        }
        private void ReceivePreParser(PreParser p)
        {
            ReceiveStrokes(p.ink);            
            foreach (var t in p.text.Values.ToList())
                ReceiveTextbox(t);
            foreach (var i in p.images.Values.ToList())
                ReceiveImage(i);
            foreach (var s in p.submissions)
                ReceiveSubmission(s);
        }
        private Object l = new Object();
        private void constructPersonFromUsername(string username)
        {
            if (!people.ContainsKey(username))
            {
                people[username] = new MeTLUser(username);
                participantListBox.ItemsSource = people.Values.ToList();
            }
        }
    }
}
