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

namespace SandRibbon.Pages.Conversations
{
    public class VisibleToAuthor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Globals.me == ((ConversationDetails)value).Author) ? Visibility.Visible : Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
    public partial class ConversationSearchPage : Page
    {
        private ObservableCollection<SearchConversationDetails> searchResultsObserver = new ObservableCollection<SearchConversationDetails>();

        private System.Threading.Timer typingDelay;
        private ListCollectionView sortedConversations;
        protected NetworkController networkController;

        public ConversationSearchPage(NetworkController _networkController, string query)
        {
            networkController = _networkController;
            InitializeComponent();
            DataContext = searchResultsObserver;
            sortedConversations = CollectionViewSource.GetDefaultView(this.searchResultsObserver) as ListCollectionView;
            sortedConversations.Filter = isWhatWeWereLookingFor;
            sortedConversations.CustomSort = new ConversationComparator();
            SearchResults.ItemsSource = searchResultsObserver;
            typingDelay = new Timer(delegate { FillSearchResultsFromInput(); });
            this.PreviewKeyUp += OnPreviewKeyUp;
            SearchInput.Text = query;
            FillSearchResultsFromInput();
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
                e.Result = networkController.client.ConversationsFor(searchString, SearchConversationDetails.DEFAULT_MAX_SEARCH_RESULTS);
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
            if (String.IsNullOrEmpty(Globals.location.activeConversation))
                return;
            RefreshSortedConversationsList();
        }
        private static bool shouldShowConversation(ConversationDetails conversation)
        {
            return conversation.UserHasPermission(Globals.credentials);
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
            if (author != Globals.me && onlyMyConversations.IsChecked.Value)
                return false;
            var title = conversation.Title.ToLower();
            var searchField = new[] { author.ToLower(), title };
            var searchQuery = SearchInput.Text.ToLower().Trim();
            if (searchQuery.Length == 0 && author == Globals.me)
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
            NavigationService.Navigate(new ConversationEditPage(networkController,conversation));
        }

        private void onlyMyConversations_Checked(object sender, RoutedEventArgs e)
        {
            RefreshSortedConversationsList();
        }

        private void JoinConversation(object sender, RoutedEventArgs e)
        {
            var requestedConversation = (ConversationDetails)((FrameworkElement)sender).DataContext;
            var conversation = networkController.client.DetailsOf(requestedConversation.Jid);
            if (conversation.UserHasPermission(Globals.credentials))
            {
                Commands.JoinConversation.ExecuteAsync(conversation.Jid);
                NavigationService.Navigate(new ConversationOverviewPage(networkController,conversation));
            }
            else
                MeTLMessage.Information("You no longer have permission to view this conversation.");
        }

        private void AnalyzeSelectedConversations(object sender, RoutedEventArgs e)
        {
            if (SearchResults.SelectedItems.Count > 0)
            {
                NavigationService.Navigate(new ConversationComparisonPage(networkController,SearchResults.SelectedItems.Cast<SearchConversationDetails>()));
            }
        }

        private void SynchronizeToOneNote(object sender, RoutedEventArgs e)
        {
            var cs = SearchResults.SelectedItems.Cast<SearchConversationDetails>();
            if (cs.Count() > 0)
            {
                Commands.SerializeConversationToOneNote.Execute(new OneNoteSynchronizationSet {
                    config = Globals.OneNoteConfiguration,
                    networkController = networkController,
                    conversations = cs.Select(c => new OneNoteSynchronization { Conversation = c, Progress = 0 })
                });
            }
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