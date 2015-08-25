using System;
using Microsoft.Ink;
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
        public HashSet<String> words = new HashSet<string>();
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
        public Dictionary<string, MeTLUser> people = new Dictionary<string, MeTLUser>();
        public HashSet<String> seen = new HashSet<string>();
        public IStemmer stemmer = new EnglishStemmer();
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
            Analyze(p);
        }
        private void Ensure(string key, Dictionary<String,List<String>> dict) {
            if (!dict.ContainsKey(key)) {
                dict[key] = new List<String>();
            }
        }
        private List<string> Filter(List<string> words) {
            return words.Where(w => w.Count() > 3).Select(w => stemmer.Stem(w)).ToList();
        }
        private void Analyze(PreParser p)
        {
            var interim = new Dictionary<string, List<String>>();
            foreach (var pInk in p.ink.GroupBy(s => s.author)) {
                Ensure(pInk.Key, interim);
                interim[pInk.Key].AddRange(Filter(AnalyzeStrokes(pInk.ToList())));
            }
            foreach (var pText in p.text.Values.GroupBy(t => t.author)) {
                Ensure(pText.Key, interim);
                foreach (var words in pText.Select(t => Filter(t.box.Text.Split().ToList())))
                {
                    interim[pText.Key].AddRange(words);
                }
            }
            foreach (var result in interim) {
                var person = people[result.Key];
                foreach (var word in result.Value)
                {
                    person.words.Add(word);
                }
                person.themes = String.Join(",", person.words);
            }
        }
        private List<String> AnalyzeStrokes(List<TargettedStroke> strokes)
        {
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
                        return recoResult.TopString.Split().ToList();
                    }                    
                    return new List<String>();                    
                }
            }
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
