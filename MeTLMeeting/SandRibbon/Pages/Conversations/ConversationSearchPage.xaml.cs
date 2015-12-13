using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using SandRibbon.Providers;
using System.Collections.ObjectModel;
using MeTLLib.DataTypes;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Automation.Peers;
using MeTLLib;
using System.ComponentModel;
using System.Collections.Generic;
using SandRibbon.Components.Utility;
using System.Windows.Automation;
using SandRibbon.Pages.Collaboration;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Pages.Analytics;
using SandRibbon.Pages.Conversations.Models;
using SandRibbon.Components;
using SandRibbon.Utils;
using SandRibbon.Frame.Flyouts;
using SandRibbon.Pages.Integration;

namespace SandRibbon.Pages.Conversations
{
    public class VisibleToAuthor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values[0];
            var controller = values[1] as NetworkController;
            return (controller.credentials.name == ((ConversationDetails)value).Author) ? Visibility.Visible : Visibility.Hidden;
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return new object[] { };
        }
    }
    public partial class ConversationSearchPage : ServerAwarePage
    {
        private ObservableCollection<SearchConversationDetails> searchResultsObserver = new ObservableCollection<SearchConversationDetails>();

        private System.Threading.Timer typingDelay;
        private ListCollectionView sortedConversations;
        public ConversationSearchPage(UserGlobalState _userGlobal, UserServerState _userServer, NetworkController _networkController, string query)
        {
            UserGlobalState = _userGlobal;
            UserServerState = _userServer;
            NetworkController = _networkController;
            InitializeComponent();
            DataContext = this;
            SearchResults.DataContext = searchResultsObserver;
            sortedConversations = CollectionViewSource.GetDefaultView(this.searchResultsObserver) as ListCollectionView;
            sortedConversations.Filter = isWhatWeWereLookingFor;
            sortedConversations.CustomSort = new ConversationComparator();
            SearchResults.ItemsSource = searchResultsObserver;
            typingDelay = new Timer(delegate { FillSearchResultsFromInput(); });
            this.PreviewKeyUp += OnPreviewKeyUp;
            SearchInput.Text = query;
            FillSearchResultsFromInput();
            var importConversationCommand = new DelegateCommand<object>(ImportPowerpoint);
            var browseOneNoteCommand = new DelegateCommand<object>(BrowseOneNote);
            Loaded += (s,e) => {
                Commands.ImportPowerpoint.RegisterCommand(importConversationCommand);
                Commands.BrowseOneNote.RegisterCommand(browseOneNoteCommand);
            };
            Unloaded += (s,e) => {
                Commands.ImportPowerpoint.UnregisterCommand(importConversationCommand);
                Commands.BrowseOneNote.UnregisterCommand(browseOneNoteCommand);
            };
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            Dispatcher.adopt(() =>
            {
                if (keyEventArgs.Key == Key.Enter)
                {
                    RefreshSortedConversationsList();
                    FillSearchResultsFromInput();
                }
            });
        }
        private void ImportPowerpoint(object obj)
        {
            Commands.AddFlyoutCard.Execute(new PowerpointLoaderFlyout(NavigationService,NetworkController,UserGlobalState,UserServerState));
        }
        private void BrowseOneNote(object obj)
        {
            NavigationService.Navigate(new OneNoteAuthenticationPage(UserGlobalState, UserServerState, NetworkController, (ugs, uss, nc, oc) => new OneNoteListingPage(ugs, uss, nc, oc), UserServerState.OneNoteConfiguration));
        }

        private void FillSearchResultsFromInput()
        {
            Dispatcher.Invoke((Action)delegate
            {
                var trimmedSearchInput = SearchInput.Text.Trim();
                if (String.IsNullOrEmpty(trimmedSearchInput))
                {

                    searchResultsObserver.Clear();
                    RefreshSortedConversationsList();
                    return;
                }
                if (!String.IsNullOrEmpty(trimmedSearchInput))
                {
                    FillSearchResults(trimmedSearchInput);
                }
                else
                    searchResultsObserver.Clear();
            });
        }

        private void FillSearchResults(string searchString)
        {
            if (searchString == null) return;
            var search = new BackgroundWorker();
            search.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                Commands.BlockSearch.ExecuteAsync(null);
                var bw = sender as BackgroundWorker;
                e.Result = NetworkController.client.ConversationsFor(searchString, SearchConversationDetails.DEFAULT_MAX_SEARCH_RESULTS);
            };

            search.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                Commands.UnblockSearch.ExecuteAsync(null);
                if (e.Error == null)
                {
                    var conversations = e.Result as List<SearchConversationDetails>;
                    if (conversations != null)
                    {
                        searchResultsObserver.Clear();
                        conversations.ForEach(cd => searchResultsObserver.Add(cd));
                    }
                }

                #region Automation events
                if (AutomationPeer.ListenerExists(AutomationEvents.AsyncContentLoaded))
                {
                    var peer = UIElementAutomationPeer.FromElement(this) as UIElementAutomationPeer;
                    if (peer != null)
                    {
                        peer.RaiseAsyncContentLoadedEvent(new AsyncContentLoadedEventArgs(AsyncContentLoadedState.Completed, 100));
                    }
                }
                #endregion
            };

            search.RunWorkerAsync();
        }

        private void clearState()
        {
            SearchInput.Clear();
            RefreshSortedConversationsList();
            SearchInput.SelectionStart = 0;
        }

        private void PauseRefreshTimer()
        {
            typingDelay.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void RestartRefreshTimer()
        {
            typingDelay.Change(500, Timeout.Infinite);
        }

        private void UpdateAllConversations(ConversationDetails details)
        {
            if (details.IsEmpty) return;
            // can I use the following test to determine if we're in a conversation?
            RefreshSortedConversationsList();
        }
        private bool shouldShowConversation(ConversationDetails conversation)
        {
            return conversation.UserHasPermission(NetworkController.credentials);
        }
        private void RefreshSortedConversationsList()
        {
            if (sortedConversations != null)
                sortedConversations.Refresh();
        }
        private bool isWhatWeWereLookingFor(object sender)
        {
            var conversation = sender as ConversationDetails;
            if (conversation == null) return false;
            if (!shouldShowConversation(conversation))
                return false;
            if (conversation.isDeleted)
                return false;
            var author = conversation.Author;
            if (author != NetworkController.credentials.name && onlyMyConversations.IsChecked.Value)
                return false;
            var title = conversation.Title.ToLower();
            var searchField = new[] { author.ToLower(), title };
            var searchQuery = SearchInput.Text.ToLower().Trim();
            if (searchQuery.Length == 0 && author == NetworkController.credentials.name)
            {//All my conversations show up in an empty search
                return true;
            }
            return searchQuery.Split(' ').All(token => searchField.Any(field => field.Contains(token)));
        }
        private void searchConversations_Click(object sender, RoutedEventArgs e)
        {
            FillSearchResultsFromInput();
        }
        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            RestartRefreshTimer();
        }

        private void EditConversation(object sender, RoutedEventArgs e)
        {
            var conversation = (ConversationDetails)((FrameworkElement)sender).DataContext;
            var userConv = new UserConversationState();
            NavigationService.Navigate(new ConversationEditPage(UserGlobalState, UserServerState, userConv, NetworkController, conversation));
        }

        private void onlyMyConversations_Checked(object sender, RoutedEventArgs e)
        {
            RefreshSortedConversationsList();
        }

        private void JoinConversation(object sender, RoutedEventArgs e)
        {
            var requestedConversation = (ConversationDetails)((FrameworkElement)sender).DataContext;
            var conversation = NetworkController.client.DetailsOf(requestedConversation.Jid);
            if (conversation.UserHasPermission(NetworkController.credentials))
            {
                Commands.JoinConversation.Execute(conversation.Jid);
                var userConversation = new UserConversationState();
                NavigationService.Navigate(new ConversationOverviewPage(UserGlobalState, UserServerState, userConversation, NetworkController, conversation));
            }
            else
                MeTLMessage.Information("You no longer have permission to view this conversation.");
        }

        private void AnalyzeSelectedConversations(object sender, RoutedEventArgs e)
        {
            if (SearchResults.SelectedItems.Count > 0)
            {
                NavigationService.Navigate(new ConversationComparisonPage(UserGlobalState, UserServerState, NetworkController, SearchResults.SelectedItems.Cast<SearchConversationDetails>()));
            }
        }

        private void SynchronizeToOneNote(object sender, RoutedEventArgs e)
        {
            var cs = SearchResults.SelectedItems.Cast<SearchConversationDetails>();
            if (cs.Count() > 0)
            {
                NavigationService.Navigate(new OneNoteAuthenticationPage(UserGlobalState,UserServerState,NetworkController,(ugs,uss,nc,oc) => new OneNoteSynchronizationPage(ugs,uss,nc,
                new OneNoteSynchronizationSet
                {
                    config = oc,
                    networkController = NetworkController,
                    conversations = cs.Select(c => new OneNoteSynchronization { Conversation = c, Progress = 0 })
                }),UserServerState.OneNoteConfiguration));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var details = new ConversationDetails(ConversationDetails.DefaultName(NetworkController.credentials.name), "", NetworkController.credentials.name, new List<MeTLLib.DataTypes.Slide>(), Permissions.LECTURE_PERMISSIONS, "Unrestricted");
            NetworkController.client.CreateConversation(details);
            SearchInput.Text = details.Title;
            FillSearchResultsFromInput();
        }
    }
    public class ConversationComparator : System.Collections.IComparer
    {
        private SearchConversationDetails ConvertToSearchConversationDetails(object obj)
        {
            if (obj is MeTLLib.DataTypes.SearchConversationDetails)
                return obj as SearchConversationDetails;

            if (obj is MeTLLib.DataTypes.ConversationDetails)
                return new SearchConversationDetails(obj as ConversationDetails);

            throw new ArgumentException("obj is of invalid type: " + obj.GetType().Name);
        }

        public int Compare(object x, object y)
        {
            var dis = ConvertToSearchConversationDetails(x);
            var dat = ConvertToSearchConversationDetails(y);
            return -1 * dis.Created.CompareTo(dat.Created);
        }
    }


    /// <summary>
    /// The ItemsView is the same as the ItemsControl but provides an Automation ID.
    /// </summary>
    public class ItemsView : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ItemsViewAutomationPeer(this);
        }
    }

    public class ItemsViewAutomationPeer : FrameworkElementAutomationPeer
    {
        private readonly ItemsView itemsView;

        public ItemsViewAutomationPeer(ItemsView itemsView) : base(itemsView)
        {
            this.itemsView = itemsView;
        }

        protected override string GetAutomationIdCore()
        {
            return itemsView.Name;
        }
    }
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////