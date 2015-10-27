using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.Providers.Connection;
using System.IO;
using System.Windows.Ink;
using Iveonik.Stemmers;

namespace SandRibbon.Components
{
    public class MeTLUser : DependencyObject
    {
        public string username { private set; get; }
        public Dictionary<string, int> usages = new Dictionary<string, int>();        
        public HashSet<String> words
        {
            get
            {
                return (HashSet<string>)GetValue(themeProperty);
            }
            set
            {
                SetValue(themeProperty, value);
            }
        }
        public static readonly DependencyProperty themeProperty = DependencyProperty.Register("themes", typeof(HashSet<String>), typeof(MeTLUser), new UIPropertyMetadata(new HashSet<String>()));
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

        public object Words { get; internal set; }

        public static readonly DependencyProperty submissionCountProperty = DependencyProperty.Register("submissionCount", typeof(int), typeof(MeTLUser), new UIPropertyMetadata(0));

        public MeTLUser(string user)
        {
            username = user;
            activityCount = 0;
            submissionCount = 0;
        }
    }

    public partial class Participants : UserControl
    {
        protected MeTLLib.MetlConfiguration backend;
                
        public Dictionary<string, MeTLUser> people = new Dictionary<string, MeTLUser>();
        public HashSet<String> seen = new HashSet<string>();
        public IStemmer stemmer = new EnglishStemmer();
        public Participants()
        {
            InitializeComponent();
            App.getContextFor(backend).controller.commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<List<TargettedStroke>>(ReceiveStrokes));
            App.getContextFor(backend).controller.commands.ReceiveStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(ReceiveStroke));
            App.getContextFor(backend).controller.commands.ReceiveTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>(ReceiveTextbox));
            App.getContextFor(backend).controller.commands.ReceiveImage.RegisterCommand(new DelegateCommand<TargettedImage>(ReceiveImage));
            App.getContextFor(backend).controller.commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(ReceivePreParser));
            App.getContextFor(backend).controller.commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            App.getContextFor(backend).controller.commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(ReceiveConversationDetails));
            App.getContextFor(backend).controller.commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(ReceiveSubmission));
        }
        private void JoinConversation(string newJid)
        {
            ClearList();
        }
        private void ClearList()
        {
            seen.Clear();
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
            if (details.Jid == Providers.Globals.conversationDetails.Jid)
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
            if (seen.Contains(s.identity)) return;
            seen.Add(s.identity);
            RegisterAction(s.author);
        }
        private void ReceiveTextbox(TargettedTextBox t)
        {
            if (seen.Contains(t.identity)) return;
            seen.Add(t.identity);
            RegisterAction(t.author);
        }
        private void ReceiveImage(TargettedImage i)
        {
            if (seen.Contains(i.identity)) return;
            seen.Add(i.identity);
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
        private void Ensure(string key, Dictionary<String,List<String>> dict) {
            if (!dict.ContainsKey(key)) {
                dict[key] = new List<String>();
            }
        }
        private List<string> Filter(List<string> words) {
            return words.Where(w => w.Count() > 3).Select(w => stemmer.Stem(w)).ToList();
        }

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
