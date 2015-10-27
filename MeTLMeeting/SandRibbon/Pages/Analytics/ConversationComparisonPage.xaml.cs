using MeTLLib.DataTypes;
using SandRibbon.Pages.Collaboration.Models;
using SandRibbon.Pages.Conversations.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
        private void SlideSelected(object sender, RoutedEventArgs e)
        {
            var context = DataContext as ConversationComparableCorpus;
            var source = sender as FrameworkElement;
            var slide = source.DataContext as VmSlide;
            if (!(context.SlideContexts.Any(s => s.context.Slide == slide.Slide.id)))
            {
                context.SlideContexts.Add(new ToolableSpaceModel
                {
                    context = new VisibleSpaceModel
                    {
                        Slide = slide.Slide.id
                    }
                });
                Commands.WatchRoom.Execute(slide.Slide.id.ToString());
            }
        }
    }
    public class ConversationComparableCorpus : DependencyObject
    {
        public ObservableCollection<ReticulatedConversation> Conversations
        {
            get { return (ObservableCollection<ReticulatedConversation>)GetValue(ConversationsProperty); }
            set { SetValue(ConversationsProperty, value); }
        }
        public static readonly DependencyProperty ConversationsProperty =
            DependencyProperty.Register("Conversations", typeof(ObservableCollection<ReticulatedConversation>), typeof(ConversationComparableCorpus), new PropertyMetadata(new ObservableCollection<ReticulatedConversation>()));

        public ObservableCollection<ToolableSpaceModel> SlideContexts
        {
            get { return (ObservableCollection<ToolableSpaceModel>)GetValue(SlideContextsProperty); }
            set { SetValue(SlideContextsProperty, value); }
        }
        public static readonly DependencyProperty SlideContextsProperty =
            DependencyProperty.Register("SlideContexts", typeof(ObservableCollection<ToolableSpaceModel>), typeof(ConversationComparableCorpus), new PropertyMetadata(new ObservableCollection<ToolableSpaceModel>()));

        public ConversationComparableCorpus(IEnumerable<SearchConversationDetails> cds)
        {            
            foreach (var c in cds)
            {
                var conversation = new ReticulatedConversation{ PresentationPath = c };
                Conversations.Add(conversation);
                conversation.CalculateLocations();
                conversation.AnalyzeLocations();
            }
        }
    }
}
