using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Pages;
using SandRibbon.Pages.Analytics;
using SandRibbon.Pages.Conversations.Models;
using SandRibbon.Pages.Integration;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace SandRibbon.Frame.Flyouts
{
    public partial class ConversationOptionsFlyout : FlyoutCard
    {
        public IEnumerable<SearchConversationDetails> Conversations {
            get; set; }
        public NetworkController NetworkController { get; set; }
        public NavigationService NavigationService { get; private set; }
        public UserGlobalState UserGlobalState { get; private set; }
        public UserServerState UserServerState { get; private set; }

        public ConversationOptionsFlyout(IEnumerable<SearchConversationDetails> Conversations,
            NetworkController NetworkController,
            NavigationService NavigationService,
            UserGlobalState UserGlobalState,
            UserServerState UserServerState)
        {
            this.Conversations = Conversations;
            this.NetworkController = NetworkController;
            this.NavigationService = NavigationService;
            this.UserGlobalState = UserGlobalState;
            this.UserServerState = UserServerState;
            var expire = new DelegateCommand<object>(o => Commands.CloseFlyoutCard.Execute(this));
            Commands.ConversationSelectionChanged.RegisterCommand(expire);
            InitializeComponent();
        }

        private void AnalyzeSelectedConversations(object sender, RoutedEventArgs e)
        {
            var userConversation = new UserConversationState();
            NavigationService.Navigate(new ConversationComparisonPage(UserGlobalState, UserServerState, userConversation, NetworkController, Conversations));
        }

        private void SynchronizeToOneNote(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OneNoteAuthenticationPage(UserGlobalState, UserServerState, NetworkController, (ugs, uss, nc, oc) => new OneNoteSynchronizationPage(ugs, uss, nc,
               new OneNoteSynchronizationSet
               {
                   config = oc,
                   networkController = NetworkController,
                   conversations = Conversations.Select(c => new OneNoteSynchronization { Conversation = c, Progress = 0 })
               }), UserServerState.OneNoteConfiguration));
        }
    }
}
