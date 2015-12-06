using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using MeTLLib.DataTypes;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Automation.Peers;
using System.ComponentModel;
using System.Collections.Generic;
using SandRibbon.Components.Utility;
using System.Windows.Automation;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.Analytics;
using SandRibbon.Pages.Conversations.Models;
using SandRibbon.Components;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Pages.Conversations
{
    public class VisibleToAuthor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values[0];
            var controller = values[1] as NetworkController;
            if (controller == null) return Visibility.Hidden;
            return (controller.credentials.name == ((ConversationDetails)value).Author) ? Visibility.Visible : Visibility.Hidden;
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return new object[] { };
        }
    }
    public partial class ConversationSearchPage : Page
    {
        private ObservableCollection<SearchConversationDetails> searchResultsObserver = new ObservableCollection<SearchConversationDetails>();
        private ListCollectionView sortedConversations;
        public ConversationSearchPage(string query)
        {
            InitializeComponent();
            SearchResults.DataContext = searchResultsObserver;
            sortedConversations = CollectionViewSource.GetDefaultView(this.searchResultsObserver) as ListCollectionView;
            sortedConversations.Filter = isWhatWeWereLookingFor;
            sortedConversations.CustomSort = new ConversationComparator();
            SearchResults.ItemsSource = searchResultsObserver;
            this.PreviewKeyUp += OnPreviewKeyUp;
            SearchInput.Text = query;
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Enter)
            {
                RefreshSortedConversationsList();
                FillSearchResultsFromInput();
            }
        }

        private void FillSearchResultsFromInput()
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
        }

        private void FillSearchResults(string searchString)
        {
            if (searchString == null) return;
            var rootPage = DataContext as DataContextRoot;
            var conversations = rootPage.NetworkController.client.ConversationsFor(searchString, SearchConversationDetails.DEFAULT_MAX_SEARCH_RESULTS);
            searchResultsObserver.Clear();
            conversations.ForEach(cd => searchResultsObserver.Add(cd));
        }

        private void clearState()
        {
            SearchInput.Clear();
            RefreshSortedConversationsList();
            SearchInput.SelectionStart = 0;
        }        
        private void UpdateAllConversations(ConversationDetails details)
        {
            if (details.IsEmpty) return;
            // can I use the following test to determine if we're in a conversation?
            RefreshSortedConversationsList();
        }
        private bool shouldShowConversation(ConversationDetails conversation)
        {
            var rootPage = DataContext as DataContextRoot;
            return conversation.UserHasPermission(rootPage.NetworkController.credentials);
        }
        private void RefreshSortedConversationsList()
        {
            if (sortedConversations != null)
                sortedConversations.Refresh();
        }
        private bool isWhatWeWereLookingFor(object sender)
        {
            var rootPage = DataContext as DataContextRoot;
            var conversation = sender as ConversationDetails;
            if (conversation == null) return false;
            if (!shouldShowConversation(conversation))
                return false;
            if (conversation.isDeleted)
                return false;
            var author = conversation.Author;
            if (author != rootPage.NetworkController.credentials.name && onlyMyConversations.IsChecked.Value)
                return false;
            var title = conversation.Title.ToLower();
            var searchField = new[] { author.ToLower(), title };
            var searchQuery = SearchInput.Text.ToLower().Trim();
            if (searchQuery.Length == 0 && author == rootPage.NetworkController.credentials.name)
            {//All my conversations show up in an empty search
                return true;
            }
            return searchQuery.Split(' ').All(token => searchField.Any(field => field.Contains(token)));
        }
        private void searchConversations_Click(object sender, RoutedEventArgs e)
        {
            FillSearchResultsFromInput();
        }        

        private void EditConversation(object sender, RoutedEventArgs e)
        {
            var rootPage = DataContext as DataContextRoot;
            var source = sender as FrameworkElement;
            rootPage.ConversationState = new ConversationState(source.DataContext as ConversationDetails, rootPage.NetworkController);
            NavigationService.Navigate(new ConversationEditPage());
        }

        private void onlyMyConversations_Checked(object sender, RoutedEventArgs e)
        {
            RefreshSortedConversationsList();
        }

        private void JoinConversation(object sender, RoutedEventArgs e)
        {
            var rootPage = DataContext as DataContextRoot;
            var source = sender as FrameworkElement;
            var requestedConversation = source.DataContext as ConversationDetails;
            var conversation = rootPage.NetworkController.client.DetailsOf(requestedConversation.Jid);
            if (conversation.UserHasPermission(rootPage.NetworkController.credentials))
            {
                var userConversation = new UserConversationState();
                rootPage.ConversationState = new ConversationState(conversation, rootPage.NetworkController);
                NavigationService.Navigate(new ConversationOverviewPage());
            }
            else
                MeTLMessage.Information("You no longer have permission to view this conversation.");
        }

        private void AnalyzeSelectedConversations(object sender, RoutedEventArgs e)
        {
            if (SearchResults.SelectedItems.Count > 0)
            {
                NavigationService.Navigate(new ConversationComparisonPage(SearchResults.SelectedItems.Cast<SearchConversationDetails>()));
            }
        }

        private void SynchronizeToOneNote(object sender, RoutedEventArgs e)
        {
            var rootPage = DataContext as DataContextRoot;
            var cs = SearchResults.SelectedItems.Cast<SearchConversationDetails>();
            if (cs.Count() > 0)
            {
                Commands.SerializeConversationToOneNote.Execute(new OneNoteSynchronizationSet
                {
                    config = rootPage.UserServerState.OneNoteConfiguration,
                    networkController = rootPage.NetworkController,
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