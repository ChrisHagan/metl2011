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
using MeTLLib.Providers;
using System.Collections.ObjectModel;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.Providers.Connection;

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
    public class MeTLUser : DependencyObject
    {

        public bool isBanned
        {
            get { return (bool)GetValue(isBannedProperty); }
            set { SetValue(isBannedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for isBanned.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty isBannedProperty =
            DependencyProperty.Register("isBanned", typeof(bool), typeof(MeTLUser), new UIPropertyMetadata(false));

        public string displayName { get; set; }
        public string username { private set; get; }



        public string email
        {
            get { return (string)GetValue(emailProperty); }
            set { SetValue(emailProperty, value); }
        }

        // Using a DependencyProperty as the backing store for email.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty emailProperty =
            DependencyProperty.Register("email", typeof(string), typeof(MeTLUser), new UIPropertyMetadata(""));

        public MeTLUser(string user)
        {
            username = user;
            isBanned = false;
            displayName = "anonymous";
        }
        public void update(MeTLUserInformation info){
            if (info.username == username)
            email = info.emailAddress;
        }
    }
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Participants : UserControl
    {
        public ObservableCollection<MeTLUser> people { get; set;}
        private List<string> requestedPeople;
        private List<string> pendingPeople;
        public static BannedToColorConverter bannedToColorConverter = new BannedToColorConverter();
        public Participants()
        {
            InitializeComponent();
            people = new ObservableCollection<MeTLUser>();
            requestedPeople = new List<string>();
            pendingPeople = new List<string>();
            participantListBox.ItemsSource = people;
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<List<TargettedStroke>>(ReceiveStrokes));
            Commands.ReceiveStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(ReceiveStroke));
            Commands.ReceiveTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>(ReceiveTextbox));
            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<TargettedImage>(ReceiveImage));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(ReceivePreParser));
            Commands.ReceiveMeTLUserInformations.RegisterCommand(new DelegateCommand<List<MeTLUserInformation>>(ReceiveMeTLUserInformations));
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
            people.Clear();
            pendingPeople.Clear();
            requestedPeople.Clear();
        }
        private void ReceiveSubmission(TargettedSubmission sub)
        {
            if (sub.target == "bannedcontent")
              foreach (var historicallyBannedUser in sub.blacklisted)
                  constructPersonFromUsername(historicallyBannedUser.UserName);
        }
        private void ReceiveConversationDetails(ConversationDetails details)
        {
            if (details.Jid == SandRibbon.Providers.Globals.conversationDetails.Jid)
            {
                foreach (var bannedUsername in details.blacklist)
                {
                    constructPersonFromUsername(bannedUsername);
                }
                Dispatcher.adoptAsync(delegate
                {
                    foreach (var u in people)
                    {
                        u.isBanned = details.UserIsBlackListed(u.username);
                    }
                    Console.WriteLine("Number of people banned: " + people.Where(p => p.isBanned).Count().ToString());
                });
            }
        }
        private void unbanThisUser(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                var dc = ((Button)sender).DataContext;
                if (dc is MeTLUser)
                {
                    var u = (MeTLUser)dc;
                    Console.WriteLine("UNBANNING!: " + u.username);
                }
            }
        }
        private void ReceiveMeTLUserInformations(List<MeTLUserInformation> infos)
        {
            foreach (var info in infos){
                if (pendingPeople.Any(rp => rp == info.username))
                {
                    Dispatcher.adoptAsync(delegate
                    {
                        var person = people.First(p => p.username == info.username);
                        person.update(info);
                        if (SandRibbon.Providers.Globals.conversationDetails.UserIsBlackListed(person.username))
                        {
                            person.isBanned = true;
                        }
                        pendingPeople.Remove(person.username);
                    });
                }
            }
        }
        private void ReceiveStrokes(List<TargettedStroke> strokes)
        {
            foreach (var s in strokes)
                ReceiveStroke(s);
        }
        private bool isContained(string username)
        {
            return requestedPeople.Any(p => p.ToLower().Trim() == username.ToLower().Trim());
        }
        private void ReceiveStroke(TargettedStroke s)
        {
            constructPersonFromUsername(s.author);
        }
        private void ReceiveTextbox(TargettedTextBox t)
        {
            constructPersonFromUsername(t.author);
        }
        private void ReceiveImage(TargettedImage i)
        {
            constructPersonFromUsername(i.author);
        }
        private void ReceivePreParser(PreParser p)
        {
            foreach (var i in p.ink)
                ReceiveStroke(i);
            foreach (var t in p.text.Values.ToList())
                ReceiveTextbox(t);
            foreach (var i in p.images.Values.ToList())
                ReceiveImage(i);
            foreach (var s in p.submissions)
                ReceiveSubmission(s);
        }
        private void constructPersonFromUsername(string username)
        {
            if (!isContained(username))
            {
                requestedPeople.Add(username);
                pendingPeople.Add(username);
                Dispatcher.adoptAsync(delegate
                {
                    people.Add(new MeTLUser(username));
                });
                Commands.RequestMeTLUserInformations.Execute(new List<string> { username });
            }
        }
    }
}
