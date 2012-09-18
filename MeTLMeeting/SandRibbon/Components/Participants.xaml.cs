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
                return Colors.Red;
            else return Colors.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Participants : UserControl
    {
        public ObservableCollection<MeTLUserInformation> people { get; set;} 
        private List<string> requestedPeople {get;set;}
        public static BannedToColorConverter bannedToColorConverter = new BannedToColorConverter();
        public Participants()
        {
            InitializeComponent();
            people = new ObservableCollection<MeTLUserInformation>();
            requestedPeople = new List<string>();
            participantListBox.ItemsSource = people;
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<List<TargettedStroke>>(ReceiveStrokes));
            Commands.ReceiveStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(ReceiveStroke));
            Commands.ReceiveTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>(ReceiveTextbox));
            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<TargettedImage>(ReceiveImage));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(ReceivePreParser));
            Commands.ReceiveMeTLUserInformations.RegisterCommand(new DelegateCommand<List<MeTLUserInformation>>(ReceiveMeTLUserInformations));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(ReceiveConversationDetails));
            //Commands.ReceiveScreenshotSubmission.RegisterCommandToDispatcher<TargettedSubmission>(new DelegateCommand<TargettedSubmission>(ReceiveSubmission));
        }
        private void ReceiveSubmission(TargettedSubmission sub)
        {
            if (sub.target == "bannedcontent")
            {
                foreach (var bannedUser in sub.blacklisted)
                    Dispatcher.adoptAsync(delegate
                    {
                        var spec = people.First(p => p.username == bannedUser.UserName);
                        if (spec != null)
                        {
                            spec.isBanned = true;
                            spec.colorHex = bannedUser.Color;
                        }
                    });
            }
        }
        private void JoinConversation(string newJid)
        {
            ClearList();
        }
        private void ClearList()
        {
            people.Clear();
            requestedPeople.Clear();
        }
        private void ReceiveConversationDetails(ConversationDetails details)
        {
            if (details.Jid == SandRibbon.Providers.Globals.conversationDetails.Jid)
            {
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
        private void ReceiveMeTLUserInformations(List<MeTLUserInformation> infos)
        {
            foreach (var info in infos){
                if (requestedPeople.Any(rp => rp == info.username))
                {
                    Dispatcher.adoptAsync(delegate
                    {
                        if (SandRibbon.Providers.Globals.conversationDetails.UserIsBlackListed(info.username))
                        {
                            info.isBanned = true;
                        }
                        people.Add(info);
                        requestedPeople.Remove(info.username);
                    });
                }
            }
        }
        private void manuallyRefreshBox(){
            participantListBox.Items.Clear();
            foreach (var p in people)
                participantListBox.Items.Add(p);
        }
        private void ReceiveStrokes(List<TargettedStroke> strokes)
        {
            foreach (var s in strokes)
                ReceiveStroke(s);
        }
        private bool isContained(string username)
        {
            return people.Any(p => p.username.ToLower().Trim() == username.ToLower().Trim());
        }
        private void ReceiveStroke(TargettedStroke s)
        {
            if (!isContained(s.author))
                constructPersonFromUsername(s.author);
        }
        private void ReceiveTextbox(TargettedTextBox t)
        {
            if (!isContained(t.author))
                constructPersonFromUsername(t.author);
        }
        private void ReceiveImage(TargettedImage i)
        {
            if (!isContained(i.author))
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
        }
        private void constructPersonFromUsername(string username)
        {
            requestedPeople.Add(username);
            Commands.RequestMeTLUserInformations.Execute(new List<string> { username });
        }
    }
}
