using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Components
{
    /// <summary>
    /// Interaction logic for ConversationSearchBox.xaml
    /// </summary>
    public partial class ConversationSearchBox : UserControl
    {
        public static RoutedCommand Search = new RoutedCommand();
        private string username;
        private List<SandRibbon.Utils.Connection.JabberWire.AuthorizedGroup> authorizedGroups;
        private string lastSearch;
        private List<SandRibbonObjects.ConversationDetails> allConversations;
        private List<ConversationSummary> myRecommendedConversationsSource;
        private List<ConversationSummary> myOwnedConversationsSource;
        private List<ConversationSummary> myRecentConversationsSource;
        private List<ConversationSummary> allConversationsSource;

        public ConversationSearchBox()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<SandRibbonObjects.ConversationDetails>(UpdateAllConversations));
            Commands.UpdateForeignConversationDetails.RegisterCommand(new DelegateCommand<SandRibbonObjects.ConversationDetails>(UpdateAllConversations));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(CloseConversationSearchBox));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(StoreIdentity));
            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(ShowConversationSearchBox));
            Commands.HideConversationSearchBox.RegisterCommand(new DelegateCommand<object>(HideConversationSearchBox));
        }
        private void ShowConversationSearchBox(object o)
        {
            DoUpdateAllConversations();
            this.Visibility = Visibility.Visible;
        }
        private void HideConversationSearchBox(object o)
        {
            CloseConversationSearchBox("000000");
        }
        private void StoreIdentity(SandRibbon.Utils.Connection.JabberWire.Credentials identity)
        {
            username = identity.name;
            authorizedGroups = identity.authorizedGroups;
            DoUpdateAllConversations();
        }
        private void CloseConversationSearchBox(string jid)
        {
            HideConversationSearchBoxButton.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Collapsed;
            //unbindAllItemSources();
            //((Grid)this.Parent).Children.Remove(this);
        }
        private void UpdateAllConversations(SandRibbonObjects.ConversationDetails details)
        {
            Dispatcher.Invoke((Action)delegate
            {

                if (this.Visibility == Visibility.Visible)
                {
                    DoUpdateAllConversations();
                }
            });
        }
        private void DoUpdateAllConversations()
        {
            allConversations = SandRibbon.Providers.Structure.ConversationDetailsProviderFactory.Provider.ListConversations().ToList();
            if (allConversations.Count != 0)
            {
                updateAllConversationsSource();
                updateMyOwnedConversations();
                updateMyRecentConversationsSource();
                updateMyRecommendedConversationsSource();
                if (!string.IsNullOrEmpty(lastSearch))
                {
                    searchFor((lastSearch).ToLower());
                }
            }
        }
        private void unbindAllItemSources()
        {
            myOwnedConversationsItemsControl.ItemsSource = null;
            myRecommendedConversationsItemsControl.ItemsSource = null;
            SearchResults.ItemsSource = null;
            myRecentConversationsItemsControl.ItemsSource = null;
            myOwnedConversationsItemsControl.Items.Clear();
            myRecommendedConversationsItemsControl.Items.Clear();
            SearchResults.Items.Clear();
            if (allConversations != null)
                allConversations.Clear();
            if (myRecommendedConversationsSource != null)
                myRecommendedConversationsSource.Clear();
            if (myOwnedConversationsSource != null)
                myOwnedConversationsSource.Clear();
            if (myRecentConversationsSource != null)
                myRecentConversationsSource.Clear();
            if (allConversationsSource != null)
                allConversationsSource.Clear();
        }

        private void updateMyRecommendedConversationsSource()
        {
            var list = new List<SandRibbonObjects.ConversationDetails>();
            var recentConversations = SandRibbon.Providers.RecentConversationProvider.loadRecentConversations().Where(c => c.IsValid
                && allConversations.Contains(c)).Reverse().Take(10);
            var recentAuthors = recentConversations.Select(c => c.Author).Where(c => c != username).Distinct().ToList();
            foreach (var author in recentAuthors)
            {
                var otherConversationsByThisAuthor = allConversations.Where(c => c.IsValid && !list.Contains(c) && c.Author == author).Reverse();
                if (otherConversationsByThisAuthor.Count() > 0)
                {
                    list.AddRange(otherConversationsByThisAuthor.Take(10));
                }
            }
            myRecommendedConversationsSource = convertToSummaries(list);
            myRecommendedConversationsCount.Content = "(" + myRecommendedConversationsSource.Count.ToString() + ")";
        }
        private void updateMyRecentConversationsSource()
        {
            myRecentConversationsSource = convertToSummaries(SandRibbon.Providers.RecentConversationProvider.loadRecentConversations().Where(c => c.IsValid && allConversations.Contains(c))
                        .Reverse()
                        .Take(10).ToList());
            myRecentConversationsCount.Content = "(" + myRecentConversationsSource.Count.ToString() + ")";
        }

        private List<ConversationSummary> convertToSummaries(List<SandRibbonObjects.ConversationDetails> source)
        {
            var matchingItemsStrings = new List<ConversationSummary>();
            foreach (SandRibbonObjects.ConversationDetails details in source)
            {
                string tags = "";
                string slides = "";
                if (!string.IsNullOrEmpty(details.Tag))
                {
                    tags = ", Tag: " + details.Tag;
                }
                if (details.Slides.Count > 1)
                    slides = "\r\n" + details.Slides.Count.ToString() + " slides";
                else slides = "\r\n1 slide";
                var description = "created by: " + details.Author
                    + ", restricted to: " + details.Subject
                    + "\r\ncreated on: " + details.Created.ToString()
                    + slides + tags;
                var summary = new ConversationSummary() { description = description, jid = details.Jid, title = details.Title };
                matchingItemsStrings.Add(summary);
            }
            return matchingItemsStrings;
        }

        private void updateAllConversationsSource()
        {
            if (allConversations.Count == 0)
                DoUpdateAllConversations();
            allConversationsSource = convertToSummaries(allConversations);
            allConversationsCount.Content = "(" + allConversationsSource.Count.ToString() + ")";
            if (allConversations.Count != 0)
            {
                updateMyOwnedConversations();
                updateMyRecentConversationsSource();
                updateMyRecommendedConversationsSource();
            }
        }

        private void updateMyOwnedConversations()
        {
            if (allConversations.Count == 0)
                DoUpdateAllConversations();
            myOwnedConversationsSource = convertToSummaries(allConversations.Where(s => s.Author == username).ToList());
            myOwnedConversationsCount.Content = "(" + myOwnedConversationsSource.Count.ToString() + ")";
        }
        private void showMyOwnedConversations()
        {
            updateMyOwnedConversations();
            myOwnedConversationsItemsControl.ItemsSource = myOwnedConversationsSource;
            myOwnedConversationsItemsControl.Visibility = Visibility.Visible;
        }
        private void showAllConversations()
        {
            updateAllConversationsSource();
            allConversationsItemsControl.ItemsSource = allConversationsSource;
            allConversationsItemsControl.Visibility = Visibility.Visible;
        }
        private void showMyRecommendedConversations()
        {
            updateMyRecommendedConversationsSource();
            myRecommendedConversationsItemsControl.ItemsSource = myRecommendedConversationsSource;
            myRecommendedConversationsItemsControl.Visibility = Visibility.Visible;
        }
        private void showMyRecentConversations()
        {
            updateMyRecentConversationsSource();
            myRecentConversationsItemsControl.ItemsSource = myRecentConversationsSource;
            myRecentConversationsItemsControl.Visibility = Visibility.Visible;
        }

        private void searchFor(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                SearchResults.ItemsSource = null;
                SearchResults.Items.Clear();
                ResultsCount.Visibility = Visibility.Collapsed;
                SearchResults.Visibility = Visibility.Collapsed;
            }
            else
            {
                lastSearch = searchText;
                var matchingItems = specificSearch(searchText);
                SearchResults.ItemsSource = convertToSummaries(matchingItems);
                updateConversationCount(matchingItems.Count);
                SearchResults.Visibility = Visibility.Visible;
                ResultsCount.Visibility = Visibility.Visible;
            }
        }
        private List<SandRibbonObjects.ConversationDetails> specificSearch(string searchText)
        {
            var tokens = (searchText.Trim()).Split(new char[] { Convert.ToChar(" ") });
            var matchingItems = (List<SandRibbonObjects.ConversationDetails>)allConversations;
            foreach (string token in tokens)
            {
                if (token.Contains(":"))
                {
                    if ((token.ToLower()).StartsWith("title:"))
                    {
                        var searchTerm = token.Substring(6);
                        matchingItems = (matchingItems.Where(c => c.Title.Contains(searchTerm))).ToList();
                    }
                    if ((token.ToLower()).StartsWith("author:"))
                    {
                        var searchTerm = token.Substring(7);
                        matchingItems = (matchingItems.Where(c => c.Author.Contains(searchTerm))).ToList();
                    }
                    //This'll have all sorts of issues, which we'll get into if we have to.
                    /*if ((token.ToLower()).StartsWith("date:"))
                    {
                        var searchTerm = token.Substring(5);
                        matchingItems = (matchingItems.Where(c => (c.Created.ToString()).Contains(searchTerm) || (c.LastAccessed.ToString()).Contains(searchTerm))).ToList();
                    }*/
                    if ((token.ToLower()).StartsWith("jid:"))
                    {
                        var searchTerm = token.Substring(4);
                        matchingItems = (matchingItems.Where(c => c.Jid.Contains(searchTerm))).ToList();
                    }
                    if ((token.ToLower()).StartsWith("tag:"))
                    {
                        var searchTerm = token.Substring(4);
                        matchingItems = (matchingItems.Where(c => c.Tag.Contains(searchTerm))).ToList();
                    }
                    if ((token.ToLower()).StartsWith("subject:"))
                    {
                        var searchTerm = token.Substring(8);
                        matchingItems = (matchingItems.Where(c => c.Subject.Contains(searchTerm))).ToList();
                    }
                    if ((token.ToLower()).StartsWith("slides:"))
                    {
                        var searchTerm = token.Substring(7);
                        bool isNumber = false;
                        try
                        {
                            Int32.Parse(searchTerm);
                            isNumber = true;
                        }
                        catch
                        {
                        }
                        if (!String.IsNullOrEmpty(searchTerm) && isNumber)
                            matchingItems = (matchingItems.Where(c => (c.Slides.Count > Convert.ToInt32(searchTerm)))).ToList();
                    }
                }
                else
                {
                    matchingItems = matchingItems.Where(conv => conv.Title.ToLower().Contains(token)
                                           || conv.Author.ToLower().Contains(token)
                                           || conv.Tag.ToLower().Contains(token)).ToList();
                }
            }
            return matchingItems;
        }

        private void updateConversationCount(int Count)
        {
            if (Count > 0)
            {
                ResultsCount.Visibility = Visibility.Visible;
                if (Count == 1)
                    ResultsCount.Content = "1 result found for '"+lastSearch+"'";
                else
                    ResultsCount.Content = Count.ToString() + " results found for '"+lastSearch+"'";
            }
            else
            {
                ResultsCount.Content = "No results found that match your search terms";
            }
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        private void checkWhetherCanSearch(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(string.IsNullOrEmpty(SearchInput.Text));
        }
        private void SearchConversationButton_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (allConversations.Count == 0)
                DoUpdateAllConversations();
            searchFor((SearchInput.Text).ToLower());
        }
        private void SelectConversation_Click(object sender, RoutedEventArgs e)
        {
            var conversation = ((Hyperlink)sender).Tag;
            Commands.JoinConversation.Execute(conversation);
        }
        private void SelectConversation_MouseDown(object sender, MouseEventArgs e)
        {
            Commands.JoinConversation.Execute(((FrameworkElement)sender).Tag);
        }
        private void myOwnedConversations_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (myOwnedConversationsItemsControl.Visibility == Visibility.Collapsed)
            {
                showMyOwnedConversations();
                myOwnedConversationsLabel.Content = "[-] My conversations";
            }
            else
            {
                myOwnedConversationsItemsControl.Visibility = Visibility.Collapsed;
                myOwnedConversationsLabel.Content = "[+] My conversations";
            }
        }
        private void recommendedConversations_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (recommendedConversations.Visibility == Visibility.Collapsed)
            {
                DoUpdateAllConversations();
                updateAllConversationsSource();
                updateMyOwnedConversations();
                updateMyRecentConversationsSource();
                updateMyRecommendedConversationsSource();
                recommendedConversations.Visibility = Visibility.Visible;
                recommendedConversationsLabel.Content = "[-] Recommended conversations";
            }
            else
            {
                recommendedConversations.Visibility = Visibility.Collapsed;
                recommendedConversationsLabel.Content = "[+] Recommended conversations";
            }
        }
        private void allConversations_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (allConversationsItemsControl.Visibility == Visibility.Collapsed)
            {
                showAllConversations();
                allConversationsLabel.Content = "[-] All Conversations";
            }
            else
            {
                allConversationsItemsControl.Visibility = Visibility.Collapsed;
                allConversationsLabel.Content = "[+] All Conversations";
            }
        }
        private void myRecommendedConversations_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (myRecommendedConversationsItemsControl.Visibility == Visibility.Collapsed)
            {
                showMyRecommendedConversations();
                myRecommendedConversationsLabel.Content = "[-] Other conversations by these authors";
            }
            else
            {
                myRecommendedConversationsItemsControl.Visibility = Visibility.Collapsed;
                myRecommendedConversationsLabel.Content = "[+] Other conversations by these authors";
            }
        }
        private void myRecentConversations_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (myRecentConversationsItemsControl.Visibility == Visibility.Collapsed)
            {
                showMyRecentConversations();
                myRecentConversationsLabel.Content = "[-] Conversations I've recently visited";
            }
            else
            {
                myRecentConversationsItemsControl.Visibility = Visibility.Collapsed;
                myRecentConversationsLabel.Content = "[+] Conversations I've recently visited";
            }
        }

        private void HideConversationSearchBoxButton_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
    class ConversationSummary
    {
        public string jid { get; set; }
        public string description { get; set; }
        public string title { get; set; }
    }
}
