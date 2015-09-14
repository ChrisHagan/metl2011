using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using SandRibbon.Components;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;

namespace SandRibbon.Pages.Collaboration
{
    public class ConversationParticipant : MeTLUser
    {
        public int slide { get; set; }
        public ConversationParticipant(string name, int slide, int activityCount) : base(name)
        {
            this.slide = slide;
            this.activityCount = activityCount;
        }
    };
    public partial class ConversationOverviewPage : Page
    {
        public ConversationOverviewPage(ConversationDetails conversation)
        {
            InitializeComponent();
            DataContext = conversation;
            var participantList = new ObservableCollection<ConversationParticipant>();
            participants.ItemsSource = participantList;
            processing.Maximum = conversation.Slides.Count;
            conversation.Slides.ForEach(slide =>
            {
                ClientFactory.Connection().getHistoryProvider().Retrieve<PreParser>(
                                    null,
                                    null,
                                    (parser) =>
                                    {
                                        processing.Value++;
                                        foreach(var user in process(parser)) {
                                            participantList.Add(user);
                                        }                                        
                                    },
                                    slide.id.ToString());
            });
        }

        private void inc(Dictionary<string, int> dict, string author) {
            if (!dict.ContainsKey(author))
            {
                dict[author] = 1;
            }
            else {
                dict[author]++;
            }
        }

        private IEnumerable<ConversationParticipant> process(PreParser p) {
            var tallies = new Dictionary<string, int>();
            foreach (var s in p.ink) {
                inc(tallies, s.author);
            }
            foreach (var t in p.text.Values) {
                inc(tallies, t.author);
            }
            foreach (var i in p.images.Values) {
                inc(tallies, i.author);
            }
            return tallies.Select(kv => new ConversationParticipant(kv.Key, p.location.currentSlide, kv.Value));
        }

        private void locations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var slideJid = (e.AddedItems[0] as Slide).id;
            NavigationService.Navigate(new CollaborationPage(slideJid));
        }
    }
}
