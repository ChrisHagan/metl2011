using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using SandRibbon.Components;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;
using OxyPlot;
using OxyPlot.Series;

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System;
using OxyPlot.Axes;
using SandRibbon.Pages.Conversations.Models;

namespace SandRibbon.Pages.Collaboration
{
    public class ConversationParticipant : MeTLUser
    {
        public int slide { get; set; }
        public int index { get; set; }
        public ConversationParticipant(string name, int slide, int activityCount) : base(name)
        {
            this.slide = slide;
            this.activityCount = activityCount;
        }
    };
    public partial class ConversationOverviewPage : Page
    {
        ReticulatedConversation conversation;
        public ConversationOverviewPage(ConversationDetails presentationPath)
        {
            InitializeComponent();
            DataContext = conversation = new ReticulatedConversation { PresentationPath = presentationPath };
            conversation.CalculateLocations();
            //We need the location references to remain stable so we can bind to them and modify them in parsers
            var participantList = new ObservableCollection<ConversationParticipant>();
            processing.Maximum = conversation.Locations.Count;
            foreach (var slide in conversation.Locations)
            {
                ClientFactory.Connection().getHistoryProvider().Retrieve<PreParser>(
                                    null,
                                    null,
                                    (parser) =>
                                    {
                                        processing.Value++;
                                        foreach (var user in process(parser))
                                        {
                                            user.index = slide.Slide.index;
                                            participantList.Add(user);
                                            var grouped = participantList.GroupBy(cp => cp.index)
                                            .ToDictionary(g => g.Key, g =>
                                            {
                                                return new ConversationParticipant("", g.Key, g.Select(u => u.activityCount).Sum());
                                            });
                                            slide.Activity = grouped[slide.Slide.index]?.activityCount ?? 0;
                                        }
                                    },
                                    slide.Slide.id.ToString());
            }
        }

        private void inc(Dictionary<string, int> dict, string author)
        {
            if (!dict.ContainsKey(author))
            {
                dict[author] = 1;
            }
            else
            {
                dict[author]++;
            }
        }

        private IEnumerable<ConversationParticipant> process(PreParser p)
        {
            var tallies = new Dictionary<string, int>();
            foreach (var s in p.ink)
            {
                inc(tallies, s.author);
            }
            foreach (var t in p.text.Values)
            {
                inc(tallies, t.author);
            }
            foreach (var i in p.images.Values)
            {
                inc(tallies, i.author);
            }
            return tallies.Select(kv => new ConversationParticipant(kv.Key, p.location.currentSlide, kv.Value));
        }

        private void SlideSelected(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            var slide = element.DataContext as VmSlide;
            NavigationService.Navigate(new GroupCollaborationPage(slide.Slide.id));
        }
    }
    public class GridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            GridLength gridLength = new GridLength(val);

            return gridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLength val = (GridLength)value;

            return val.Value;
        }
    }
}
