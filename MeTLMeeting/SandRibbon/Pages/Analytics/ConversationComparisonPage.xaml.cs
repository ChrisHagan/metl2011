using MeTLLib.DataTypes;
using SandRibbon.Pages.Conversations.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System;
using System.Globalization;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Components;

namespace SandRibbon.Pages.Analytics
{
    public class ParticipantsEnumerator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var participants = value as ILookup<string, LocatedActivity>;
            return String.Join(",", participants.Select(p => p.Key));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class ConversationComparisonPage : ServerAwarePage
    {
        UserConversationState UserConversationState { get; set; }
        public ConversationComparisonPage(UserGlobalState _userGlobal, UserServerState _userServer, UserConversationState _userConversation, NetworkController _networkController, IEnumerable<SearchConversationDetails> cs)
        {
            NetworkController = _networkController;
            UserGlobalState = _userGlobal;
            UserServerState = _userServer;
            UserConversationState = _userConversation;
            InitializeComponent();
            DataContext = new ConversationComparableCorpus(NetworkController,cs);
        }
        private void SlideSelected(object sender, RoutedEventArgs e)
        {
            var context = DataContext as ConversationComparableCorpus;
            var source = sender as FrameworkElement;
            var slide = source.DataContext as VmSlide;
            if (!(context.WatchedSpaces.Any(s => (s.DataContext as SlideAwarePage).Slide.id == slide.Slide.id)))
            {
                context.WatchedSpaces.Add(new PresentationSpace
                {
                    DataContext = new SlideAwarePage
                    {
                        NetworkController = NetworkController,
                        ConversationDetails = slide.Details,
                        Slide = slide.Slide,
                        UserGlobalState = UserGlobalState,
                        UserServerState = UserServerState,
                        UserConversationState = UserConversationState
                    }
                });
                //Commands.WatchRoom.Execute(slide.Slide.id.ToString());
            }
        }

        public NetworkController getNetworkController()
        {
            return NetworkController;
        }

        public UserServerState getUserServerState()
        {
            return UserServerState;
        }

        public UserGlobalState getUserGlobalState()
        {
            return UserGlobalState;
        }
    }
    public class ConversationComparableCorpus : DependencyObject
    {
        public ObservableCollection<PresentationSpace> WatchedSpaces
        {
            get { return (ObservableCollection<PresentationSpace>)GetValue(WatchedSpacesProperty); }
            set { SetValue(WatchedSpacesProperty, value); }
        }
        public static readonly DependencyProperty WatchedSpacesProperty =
            DependencyProperty.Register("WatchedSpaces", typeof(ObservableCollection<PresentationSpace>), typeof(ConversationComparableCorpus), new PropertyMetadata(new ObservableCollection<PresentationSpace>()));


        public NetworkController NetworkController
        {
            get { return (NetworkController)GetValue(NetworkControllerProperty); }
            set { SetValue(NetworkControllerProperty, value); }
        }
        
        public static readonly DependencyProperty NetworkControllerProperty =
            DependencyProperty.Register("NetworkController", typeof(NetworkController), typeof(ConversationComparableCorpus), new PropertyMetadata(null));
        
        public ObservableCollection<ReticulatedConversation> Conversations
        {
            get { return (ObservableCollection<ReticulatedConversation>)GetValue(ConversationsProperty); }
            set { SetValue(ConversationsProperty, value); }
        }
        public static readonly DependencyProperty ConversationsProperty =
            DependencyProperty.Register("Conversations", typeof(ObservableCollection<ReticulatedConversation>), typeof(ConversationComparableCorpus), new PropertyMetadata(new ObservableCollection<ReticulatedConversation>()));

       
        public ConversationComparableCorpus(NetworkController _networkController, IEnumerable<SearchConversationDetails> cds)
        {            
            foreach (var c in cds)
            {
                var conversation = new ReticulatedConversation{
                    networkController = _networkController,
                    PresentationPath = c
                };
                NetworkController = _networkController;
                Conversations.Add(conversation);
                conversation.CalculateLocations();
                conversation.AnalyzeLocations();
            }
        }
    }
}
