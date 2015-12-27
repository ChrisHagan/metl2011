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
using MeTLLib.Providers;

namespace SandRibbon.Pages.Analytics
{
    public class Counter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var things = value as IEnumerable<string>;
            return things.Count();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Lister : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var things = value as IEnumerable<string>;
            return String.Join(",", things);
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
            DataContext = new ConversationComparableCorpus(NetworkController, cs);
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

        public ObservableCollection<HistorySummary> Histories
        {
            get { return (ObservableCollection<HistorySummary>)GetValue(HistoriesProperty); }
            set { SetValue(HistoriesProperty, value); }
        }
        public static readonly DependencyProperty HistoriesProperty =
                    DependencyProperty.Register("Histories", typeof(ObservableCollection<HistorySummary>), typeof(ConversationComparableCorpus), new PropertyMetadata(new ObservableCollection<HistorySummary>()));

        public ObservableCollection<VmSlide> Locations { get; set; } = new ObservableCollection<VmSlide>();
        
        public ObservableCollection<ReticulatedConversation> Conversations
        {
            get { return (ObservableCollection<ReticulatedConversation>)GetValue(ConversationsProperty); }
            set { SetValue(ConversationsProperty, value); }
        }
        public static readonly DependencyProperty ConversationsProperty =
            DependencyProperty.Register("Conversations", typeof(ObservableCollection<ReticulatedConversation>), typeof(ConversationComparableCorpus), new PropertyMetadata(new ObservableCollection<ReticulatedConversation>()));
        
        public ConversationComparableCorpus(NetworkController networkController, IEnumerable<SearchConversationDetails> cds)
        {
            this.NetworkController = networkController;
            foreach (var c in cds)
            {
                Conversations.Add(new ReticulatedConversation
                {
                    PresentationPath = c,
                    networkController = networkController
                }.CalculateLocations().AnalyzeLocations());
                                
                foreach (var s in c.Slides)
                {
                    var description = networkController.client.historyProvider.Describe(s.id);
                    Locations.Add(new VmSlide {
                        Slide = s,
                        Details = c,
                        HistorySummary = description
                    });
                    Histories.Add(description);
                }
            }
        }
    }
}
