using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.Conversations.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Pages.Analytics
{
    public partial class ConversationComparisonPage : Page
    {
        protected MetlConfiguration backend;
        public ConversationComparisonPage(MetlConfiguration _backend, IEnumerable<SearchConversationDetails> cs)
        {
            backend = _backend;
            InitializeComponent();
            DataContext = new ConversationComparableCorpus(backend,cs);
        }
    }
    public class ConversationComparable : DependencyObject{        
        public int LocationCount
        {
            get { return (int)GetValue(LocationCountProperty); }
            set { SetValue(LocationCountProperty, value); }
        }
        public static readonly DependencyProperty LocationCountProperty =
            DependencyProperty.Register("LocationCount", typeof(int), typeof(ConversationComparable), new PropertyMetadata(0));

        public int ProcessingProgress
        {
            get { return (int)GetValue(ProcessingProgressProperty); }
            set { SetValue(ProcessingProgressProperty, value); }
        }        
        public static readonly DependencyProperty ProcessingProgressProperty =
            DependencyProperty.Register("ProcessingProgress", typeof(int), typeof(ConversationComparable), new PropertyMetadata(0));

        
        public ObservableCollection<LocatedActivity> ParticipantList
        {
            get { return (ObservableCollection<LocatedActivity>)GetValue(ParticipantListProperty); }
            set { SetValue(ParticipantListProperty, value); }
        }
        
        public static readonly DependencyProperty ParticipantListProperty =
            DependencyProperty.Register("ParticipantList", typeof(ObservableCollection<LocatedActivity>), typeof(ConversationComparable), new PropertyMetadata(new ObservableCollection<LocatedActivity>()));
    }
    public class ConversationComparableCorpus {
        protected MetlConfiguration backend;
        public ObservableCollection<ConversationComparable> outputs { get; set; } = new ObservableCollection<ConversationComparable>();        
        public ConversationComparableCorpus(MetlConfiguration _backend,IEnumerable<SearchConversationDetails> cds) {
            backend = _backend;
            BuildComparisons(cds.Select(cd => new ReticulatedConversation
            {
                PresentationPath = cd
            }));
        }
        private void BuildComparisons(IEnumerable<ReticulatedConversation> conversations) {            
            foreach (var conversation in conversations.AsParallel())
            {
                conversation.CalculateLocations();
                var output = new ConversationComparable { LocationCount = conversation.LongestPathLength };
                outputs.Add(output);
                foreach (var slide in conversation.Locations)
                {
                    App.getContextFor(backend).controller.client.getHistoryProvider().Retrieve<PreParser>(
                                        null,
                                        null,
                                        (parser) =>
                                        {
                                            output.ProcessingProgress++;
                                            foreach (var user in process(parser))
                                            {
                                                user.index = slide.Slide.index;
                                                output.ParticipantList.Add(user);                                                
                                            }
                                        },
                                        slide.Slide.id.ToString());
                }
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

        private IEnumerable<LocatedActivity> process(PreParser p)
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
            return tallies.Select(kv => new LocatedActivity(kv.Key, p.location.currentSlide, kv.Value, 0));
        }
    }
}
