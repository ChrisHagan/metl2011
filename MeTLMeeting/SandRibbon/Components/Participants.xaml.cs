using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.Providers.Connection;
using Iveonik.Stemmers;
using System.Windows.Data;
using System.Globalization;
using SandRibbon.Providers;
using SandRibbon.Pages;

namespace SandRibbon.Components
{
    public class SlideCollectionDescriber : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<string>)
            {
                return (value as List<string>).Aggregate("", (acc, item) => (acc == "" ? item : acc + ", " + item));
            }
            else
            {
                return "";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
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

        public List<string> slideLocation
        {
            get
            {
                return (List<string>)GetValue(slideLocationProperty);
            }
            set
            {
                SetValue(slideLocationProperty, value);
            }
        }

        public static readonly DependencyProperty slideLocationProperty = DependencyProperty.Register("slideLocation", typeof(List<string>), typeof(MeTLUser), new UIPropertyMetadata(new List<string>()));


        public MeTLUser(string user)
        {
            username = user;
            slideLocation = new List<string>();
            activityCount = 0;
            submissionCount = 0;
        }
    }

    public partial class Participants : UserControl
    {
        public static SlideCollectionDescriber slideCollectionDescriber = new SlideCollectionDescriber();
        public Dictionary<string, MeTLUser> people = new Dictionary<string, MeTLUser>();
        public HashSet<String> seen = new HashSet<string>();
        public IStemmer stemmer = new EnglishStemmer();
        public ConversationAwarePage rootPage { get; protected set; }
        public Participants()
        {
            InitializeComponent();
            var receiveStrokesCommand = new DelegateCommand<List<TargettedStroke>>(ReceiveStrokes);
            var receiveStrokeCommand = new DelegateCommand<TargettedStroke>(ReceiveStroke);
            var receiveTextboxCommand = new DelegateCommand<TargettedTextBox>(ReceiveTextbox);
            var receiveImageCommand = new DelegateCommand<TargettedImage>(ReceiveImage);
            var receivePreParserCommand = new DelegateCommand<PreParser>(ReceivePreParser);
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(ReceiveConversationDetails);
            var receiveScreenshotSubmissionCommand = new DelegateCommand<TargettedSubmission>(ReceiveSubmission);
            var receiveAttendanceCommand = new DelegateCommand<Attendance>(ReceivePresence);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as ConversationAwarePage;
                Commands.ReceiveStrokes.RegisterCommand(receiveStrokesCommand);
                Commands.ReceiveStroke.RegisterCommand(receiveStrokeCommand);
                Commands.ReceiveTextBox.RegisterCommand(receiveTextboxCommand);
                Commands.ReceiveImage.RegisterCommand(receiveImageCommand);
                Commands.PreParserAvailable.RegisterCommand(receivePreParserCommand);
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.ReceiveScreenshotSubmission.RegisterCommand(receiveScreenshotSubmissionCommand);
                Commands.ReceiveAttendance.RegisterCommand(receiveAttendanceCommand);
            };
            Unloaded += (s, e) =>
            {
                Commands.ReceiveStrokes.UnregisterCommand(receiveStrokesCommand);
                Commands.ReceiveStroke.UnregisterCommand(receiveStrokeCommand);
                Commands.ReceiveTextBox.UnregisterCommand(receiveTextboxCommand);
                Commands.ReceiveImage.UnregisterCommand(receiveImageCommand);
                Commands.PreParserAvailable.UnregisterCommand(receivePreParserCommand);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.ReceiveScreenshotSubmission.UnregisterCommand(receiveScreenshotSubmissionCommand);
                Commands.ReceiveAttendance.UnregisterCommand(receiveAttendanceCommand);
            };
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
            if (details.Jid == rootPage.ConversationDetails.Jid)
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
        protected void ReceivePresence(Attendance presence)
        {
            if (rootPage.ConversationDetails.Slides.Select(s => s.id.ToString()).Contains(presence.location))
            {
                Dispatcher.adopt(delegate
                {
                    constructPersonFromUsername(presence.author);
                    var user = people[presence.author];
                    if (presence.present)
                    {
                        user.slideLocation = user.slideLocation.Union(new List<string> { presence.location }).Distinct().ToList();
                    }
                    else
                    {
                        user.slideLocation = user.slideLocation.Where(sl => sl != presence.location).ToList();
                    }
                });
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
            foreach (var a in p.attendances)
                ReceivePresence(a);
        }
        private void Ensure(string key, Dictionary<String, List<String>> dict)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = new List<String>();
            }
        }
        private List<string> Filter(List<string> words)
        {
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
