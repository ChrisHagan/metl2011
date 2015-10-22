using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.Conversations.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace SandRibbon.Pages.Analytics
{
    public partial class ConversationComparisonPage : Page
    {
        public ConversationComparisonPage(IEnumerable<SearchConversationDetails> cs)
        {
            InitializeComponent();
            DataContext = new ConversationComparableCorpus(cs);
        }
    }
    public class ConversationComparable {
        public int locationCount { get; set; } = 0;
        public int processingProgress { get; set; } = 0;
        public ObservableCollection<LocatedActivity> participantList { get; set; } = new ObservableCollection<LocatedActivity>();
    }
    public class ConversationComparableCorpus {
        public ObservableCollection<ConversationComparable> outputs { get; set; } = new ObservableCollection<ConversationComparable>();        
        public ObservableCollection<string> participants { get; set; }        
        public ConversationComparableCorpus(IEnumerable<SearchConversationDetails> cds) {            
            BuildComparisons(cds.Select(cd => new ReticulatedConversation
            {
                PresentationPath = cd
            }));
        }
        private void BuildComparisons(IEnumerable<ReticulatedConversation> conversations) {
            foreach (var conversation in conversations)
            {
                conversation.CalculateLocations();
                var output = new ConversationComparable { locationCount = conversation.LongestPathLength };
                outputs.Add(output);
                foreach (var slide in conversation.Locations)
                {
                    ClientFactory.Connection().getHistoryProvider().Retrieve<PreParser>(
                                        null,
                                        null,
                                        (parser) =>
                                        {
                                            output.processingProgress++;
                                            foreach (var user in process(parser))
                                            {
                                                user.index = slide.Slide.index;
                                                output.participantList.Add(user);
                                                Console.WriteLine(slide.Slide.id);
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
