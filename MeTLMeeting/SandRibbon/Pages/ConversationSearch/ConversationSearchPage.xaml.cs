using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Practices.Composite.Presentation.Commands;
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

namespace SandRibbon.Pages.ConversationSearch
{
    [ValueConversion(typeof(int), typeof(string))]
    public class SearchResultsCountToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                if ((int)value == 0)
                    return "No conversations match your search.";
                else
                    return String.Format("Found {0} result{1}.", (int)value, (int)value > 1 ? "s" : String.Empty);
            }
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public partial class ConversationSearchPage : Page
    {
        public class HideIfNotCurrentConversation : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return activeConversation == ((MeTLLib.DataTypes.ConversationDetails)value).Jid ? Visibility.Visible : Visibility.Collapsed;
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return false;
            }
        }
        public class HideErrorsIfEmptyConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }
        }
        public class IsMeConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (me == ((MeTLLib.DataTypes.ConversationDetails)value).Author).ToString();
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return false;
            }
        }

        private bool editInProgress = false;
        public static RoutedCommand RenameConversation = new RoutedCommand();
        public static RoutedCommand ShareConversation = new RoutedCommand();
        public static RoutedCommand DeleteConversation = new RoutedCommand();

        private void CheckEditAllowed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !editInProgress;
        }

        public static HideErrorsIfEmptyConverter HideErrorsIfEmpty = new HideErrorsIfEmptyConverter();
        public static IsMeConverter isMe = new IsMeConverter();
        public static HideIfNotCurrentConversation hideIfNotCurrentConversation = new HideIfNotCurrentConversation();
        private ObservableCollection<MeTLLib.DataTypes.ConversationDetails> searchResultsObserver = new ObservableCollection<MeTLLib.DataTypes.ConversationDetails>();
        protected static string activeConversation;
        public string Errors
        {
            get { return (string)GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
        }
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(string), typeof(ConversationSearchPage), new UIPropertyMetadata(""));
        public string Version { get; set; }
        private static string me;
        private System.Threading.Timer refreshTimer;
        private ListCollectionView sortedConversations;
        public ConversationSearchPage()
        {
            InitializeComponent();
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(ShowConversationSearchBox));
            Commands.HideConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(HideConversationSearchBox));
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<string>(JoinConversation));
            Version = ConfigurationProvider.instance.getMetlVersion();
            sortedConversations = CollectionViewSource.GetDefaultView(this.searchResultsObserver) as ListCollectionView;
            sortedConversations.Filter = isWhatWeWereLookingFor;
            sortedConversations.CustomSort = new ConversationComparator();
            SearchResults.ItemsSource = searchResultsObserver;
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<object>(_arg => me = Globals.me));
            refreshTimer = new Timer(delegate { FillSearchResultsFromInput(); });
            this.PreviewKeyUp += OnPreviewKeyUp;
            App.mark("Initialized conversation search");
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

        DelegateCommand<ConversationDetails> conversationDetailsCommand = null;
        DelegateCommand<string> joinConversationCommand = null;
        DelegateCommand<string> leaveConversationCommand = null;
        DelegateCommand<object> setConversationPermissionsCommand = null;
        DelegateCommand<string> backstageModeChangedCommand = null;

        private void RegisterCommands()
        {
            // *cries* ...
            UnregisterCommands();
            if (conversationDetailsCommand == null)
            {
                conversationDetailsCommand = new DelegateCommand<ConversationDetails>(UpdateAllConversations);
                joinConversationCommand = new DelegateCommand<string>(JoinConversation);
                leaveConversationCommand = new DelegateCommand<string>(LeaveConversation);
                setConversationPermissionsCommand = new DelegateCommand<object>(App.noop, canSetPermissions);
                backstageModeChangedCommand = new DelegateCommand<string>(BackstageModeChanged);
            }

            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(conversationDetailsCommand);
            Commands.UpdateForeignConversationDetails.RegisterCommandToDispatcher(conversationDetailsCommand);
            Commands.JoinConversation.RegisterCommand(joinConversationCommand);
            Commands.LeaveConversation.RegisterCommand(leaveConversationCommand);
            Commands.SetConversationPermissions.RegisterCommand(setConversationPermissionsCommand);
            Commands.BackstageModeChanged.RegisterCommand(backstageModeChangedCommand);
        }

        private void UnregisterCommands()
        {
            Commands.UpdateConversationDetails.UnregisterCommand(conversationDetailsCommand);
            Commands.UpdateForeignConversationDetails.UnregisterCommand(conversationDetailsCommand);
            Commands.JoinConversation.UnregisterCommand(joinConversationCommand);
            Commands.LeaveConversation.UnregisterCommand(leaveConversationCommand);
            Commands.SetConversationPermissions.UnregisterCommand(setConversationPermissionsCommand);
            Commands.BackstageModeChanged.UnregisterCommand(backstageModeChangedCommand);
        }

        private void FillSearchResultsFromInput()
        {
            Dispatcher.Invoke((Action)delegate
            {
                var trimmedSearchInput = SearchInput.Text.Trim();
                if (String.IsNullOrEmpty(trimmedSearchInput))
                {
                    if (backstageNav.currentMode == "mine")
                        trimmedSearchInput = Globals.me;
                    else if (backstageNav.currentMode == "currentConversation")
                    {
                        searchResultsObserver.Clear();
                        if (!Globals.conversationDetails.IsEmpty && !Globals.conversationDetails.isDeleted)
                            searchResultsObserver.Add(Globals.conversationDetails);
                        RefreshSortedConversationsList();
                        return;
                    }
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
            var search = new BackgroundWorker();
            search.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                Commands.BlockSearch.ExecuteAsync(null);
                var bw = sender as BackgroundWorker;
                e.Result = ClientFactory.Connection().ConversationsFor(searchString, SearchConversationDetails.DEFAULT_MAX_SEARCH_RESULTS);
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
        private bool canSetPermissions(object arg)
        {
            return this.Visibility == Visibility.Collapsed;
        }
        private void BackstageModeChanged(string mode)
        {
            Action changeBackstage = () =>
            {
                updateLiveButton(mode);

                string searchButtonText;
                switch (mode)
                {
                    case "mine":
                        searchButtonText = "Filter my Conversations";
                        FillSearchResults(Globals.me);
                        break;
                    default:
                        searchButtonText = "Search all Conversations";
                        FillSearchResultsFromInput();
                        break;
                }

                if (searchConversations == null) return;
                searchConversations.Content = searchButtonText;
            };

            Dispatcher.Invoke(changeBackstage, DispatcherPriority.Normal);
        }

        private void updateLiveButton(string mode)
        {
            var elements = new[] { mine, find, currentConversation };
            foreach (var button in elements)
                if (button.Name == mode)
                {
                    button.IsChecked = true;
                }
        }

        private void clearState()
        {
            SearchInput.Clear();
            RefreshSortedConversationsList();
            SearchInput.SelectionStart = 0;
        }

        private void PauseRefreshTimer()
        {
            refreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void RestartRefreshTimer()
        {
            refreshTimer.Change(500, Timeout.Infinite);
        }

        private void ShowConversationSearchBox(object o)
        {
            RegisterCommands();
            RestartRefreshTimer();

            var currentConversationDetails = Globals.conversationDetails;
            activeConversation = Globals.location.activeConversation;
            me = Globals.me;

            if (String.IsNullOrEmpty(activeConversation) || (currentConversationDetails != null && currentConversationDetails.isDeleted))
                currentConversation.Visibility = Visibility.Collapsed;
            else
            {
                currentConversation.Visibility = Visibility.Visible;
            }
            this.Visibility = Visibility.Visible;
            clearState();

            if (!string.IsNullOrEmpty((string)o) && (string)o == "MyConversations")
            {
                BackstageModeChanged("mine");
            }

            Dispatcher.queueFocus(SearchInput);
        }
        private void HideConversationSearchBox(object o)
        {
            UnregisterCommands();
            PauseRefreshTimer();
            editInProgress = false;
            this.Visibility = Visibility.Collapsed;
            Commands.RequerySuggested();
        }
        private void JoinConversation(object o)
        {
            editInProgress = false;
            CloseConversationSearchBox();
        }
        private void CloseConversationSearchBox()
        {
            editInProgress = false;
            Commands.HideConversationSearchBox.Execute(null);
        }
        private void LeaveConversation(string jid)
        {
        }
        private void UpdateAllConversations(MeTLLib.DataTypes.ConversationDetails details)
        {
            if (details.IsEmpty) return;
            // can I use the following test to determine if we're in a conversation?
            if (String.IsNullOrEmpty(Globals.location.activeConversation))
                return;

            /*if (!details.isDeleted)
                searchResultsObserver.Add(new SearchConversationDetails(details));
             */
            //if (!details.IsJidEqual(Globals.location.activeConversation) || details.isDeleted)
            // was the above line but this doesn't handle the case of bug #1492
            // line below was copied from BackStageNav to handle its responsibilities, I believe for the same condition
            if (details.IsJidEqual(Globals.location.activeConversation))
            {
                if (details.isDeleted || (!details.UserHasPermission(Globals.credentials)))//(!Globals.credentials.authorizedGroups.Select(s => s.groupKey.ToLower()).Contains(details.Subject.ToLower()) && !details.isDeleted))
                {
                    currentConversation.Visibility = Visibility.Collapsed;
                    // really don't like the following line, but it stops the backstagemode changing if already switched to it
                    if (Commands.BackstageModeChanged.IsInitialised && (string)Commands.BackstageModeChanged.LastValue() != "mine")
                        Commands.BackstageModeChanged.ExecuteAsync("find");
                }
                if (!shouldShowConversation(details) || details.isDeleted)
                {
                    Commands.RequerySuggested();
                    this.Visibility = Visibility.Visible;
                }
            }
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
            if (conversation != null)
            {
                if (!shouldShowConversation(conversation))
                    return false;
                if (conversation.isDeleted)
                    return false;
                if (backstageNav.currentMode == "currentConversation" && conversation.IsJidEqual(activeConversation))
                    return true;
                var author = conversation.Author;
                var title = conversation.Title.ToLower();
                var searchField = new[] { author.ToLower(), title };
                var searchQuery = SearchInput.Text.ToLower().Trim();
                if (backstageNav.currentMode == "find" && searchQuery.Length == 0)
                {
                    return false;
                }
                if (backstageNav.currentMode == "mine" && author != Globals.me)
                {
                    return false;
                }
                return searchQuery.Split(' ').All(token => searchField.Any(field => field.Contains(token)));
            }
            return false;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        private void searchConversations_Click(object sender, RoutedEventArgs e)
        {
            FillSearchResultsFromInput();
            //RefreshSortedConversationsList();
        }
        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            RestartRefreshTimer();
        }
        private void ChooseConversationToEnter(object sender, RoutedEventArgs e)
        {
            editInProgress = false;
            if (originalContext != null)
            {
                foreach (var result in SearchResults.ItemsSource)
                    if (((ConversationDetails)result).IsJidEqual(originalContext.Jid))
                        ((ConversationDetails)result).Title = originalContext.Title;
                originalContext = null;
            }
            var requestedJid = ((FrameworkElement)sender).Tag as string;
            if (requestedJid.Equals(Globals.location.activeConversation) && !Globals.conversationDetails.IsEmpty)
            {
                Commands.HideConversationSearchBox.Execute(null);
            }
            else
            {
                // Check that the permissions have not changed since user searched for the conversation
                var conversation = ClientFactory.Connection().DetailsOf(requestedJid);
                if (conversation.UserHasPermission(Globals.credentials))
                {                    
                    Commands.JoinConversation.ExecuteAsync(requestedJid);
                    NavigationService.Navigate(new CollaborationPage(conversation.Slides.First().id));
                }
                else
                    MeTLMessage.Information("You no longer have permission to view this conversation.");
            }
        }

        private void deleteConversation(object sender, ExecutedRoutedEventArgs e)
        {
            if (MeTLMessage.Question("Really delete this conversation?") == MessageBoxResult.Yes)
            {
                var details = context(e.OriginalSource);
                MeTLLib.ClientFactory.Connection().DeleteConversation(details);

            }
            FillSearchResultsFromInput();
        }
        private void mode_Checked(object sender, RoutedEventArgs e)
        {
            var mode = ((FrameworkElement)sender).Name;
            backstageNav.currentMode = mode;
        }
        private ContentPresenter view(object backedByConversation)
        {
            var conversation = (ConversationDetails)((FrameworkElement)backedByConversation).DataContext;
            var item = SearchResults.ItemContainerGenerator.ContainerFromItem(conversation);
            var view = (ContentPresenter)item;
            return view;
        }
        private ConversationDetails context(object sender)
        {
            return ((FrameworkElement)sender).DataContext as ConversationDetails;
        }
        ConversationDetails originalContext;
        private void assignTemplate(string dataTemplateResourceKey, object sender)
        {
            var sentContext = context(sender);
            var presenter = view(sender);
            presenter.Content = sentContext;
            presenter.ContentTemplate = (DataTemplate)FindResource(dataTemplateResourceKey);
            originalContext = sentContext.Clone();
        }
        private void renameConversation(object sender, ExecutedRoutedEventArgs e)
        {
            editInProgress = true;
            if (originalContext != null)
            {
                var item = SearchResults.ItemContainerGenerator.ContainerFromItem(originalContext);
                cancelEdit(item, null);
            }
            assignTemplate("rename", e.OriginalSource);
        }
        private void shareConversation(object sender, ExecutedRoutedEventArgs e)
        {
            editInProgress = true;
            assignTemplate("share", e.OriginalSource);
        }
        private void cancelEdit(object sender, RoutedEventArgs e)
        {
            editInProgress = false;
            if (sender == null)
                return;

            var source = (FrameworkElement)sender;
            source.DataContext = originalContext;
            originalContext = null;
            assignTemplate("viewing", sender);
        }
        private string errorsFor(ConversationDetails proposedDetails)
        {
            proposedDetails.Title = proposedDetails.Title.Trim();
            var thisTitleIsASCII = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(proposedDetails.Title)).Equals(proposedDetails.Title);
            var thisIsAValidTitle = !String.IsNullOrEmpty(proposedDetails.Title.Trim());
            var titleAlreadyUsed = searchResultsObserver.Except(new[] { proposedDetails }).Any(c => c.Title.Equals(proposedDetails.Title, StringComparison.InvariantCultureIgnoreCase));
            var errorText = String.Empty;
            if (proposedDetails.Title.Length > 110) errorText += "Conversation titles have a maximum length of 110 characters";
            if (!thisTitleIsASCII)
                errorText += "Conversation title can only contain letters, numbers and punctuation marks. ";
            if (!thisIsAValidTitle) { errorText += "Invalid conversation title.  "; }
            if (titleAlreadyUsed) { errorText += "Conversation title already used.  "; }
            return errorText;
        }
        private void saveEdit(object sender, RoutedEventArgs e)
        {
            editInProgress = false;
            var details = SearchConversationDetails.HydrateFromServer(context(sender));

            var errors = errorsFor(details);
            if (string.IsNullOrEmpty(errors))
            {
                MeTLLib.ClientFactory.Connection().UpdateConversationDetails(details);
                originalContext = null;
                assignTemplate("viewing", sender);
            }
            else
            {
                this.Errors = errors;
            }
        }
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke((Action)delegate
            {
                ((TextBox)sender).Focus();
            }, DispatcherPriority.Background);
        }
        private void EditTitleChanged(object sender, TextChangedEventArgs e)
        {
            //Be slow to complain and quick to forgive.  Remove the errors output as soon as the user starts editing.
            this.Errors = String.Empty;
        }
        private void KeyPressedInTitleRename(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var source = (TextBox)sender;
                var context = (ConversationDetails)source.DataContext;
                context.Title = source.Text;
                saveEdit(source, null);
            }
            else if (e.Key == Key.Escape)
            {
                cancelEdit(sender, null);
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
