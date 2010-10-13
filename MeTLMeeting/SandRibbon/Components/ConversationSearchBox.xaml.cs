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
using SandRibbon.Providers;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using SandRibbon.Utils;
using SandRibbonObjects;

namespace SandRibbon.Components
{
    public partial class ConversationSearchBox : UserControl
    {
        private ObservableCollection<ConversationDetails> searchResults = new ObservableCollection<ConversationDetails>();

        public ConversationSearchBox()
        {
            InitializeComponent();
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<object>(SetIdentity));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<SandRibbonObjects.ConversationDetails>(UpdateAllConversations));
            Commands.UpdateForeignConversationDetails.RegisterCommand(new DelegateCommand<SandRibbonObjects.ConversationDetails>(UpdateAllConversations));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(ShowConversationSearchBox));
            Commands.HideConversationSearchBox.RegisterCommand(new DelegateCommand<object>(HideConversationSearchBox));
            Commands.BackstageModeChanged.RegisterCommand(new DelegateCommand<string>(BackstageModeChanged));
            SearchResults.ItemsSource = searchResults;
            var view = GetListCollectionView();
            view.Filter = isWhatWeWereLookingFor;
            view.CustomSort = new ConversationComparator();
        }
        private void CloseBackstage(object _arg) {
            Visibility = Visibility.Collapsed;
        }
        private void BackstageModeChanged(string mode) {
            GetListCollectionView().Refresh();
        }
        private void SetIdentity(object _arg){
            foreach(var conversation in SandRibbon.Providers.Structure.ConversationDetailsProviderFactory.Provider.ListConversations())
                searchResults.Add(conversation);
        }
        private void clearState() {
            SearchInput.Text = "";
        }
        private void openMyConversations() { }
        private void openAllConversations() { }
        private void openCorrectTab(String o) {
            if ("MyConversations" == o)
                openMyConversations();
            else
                openAllConversations();
        }
        private void ShowConversationSearchBox(object o)
        {
            clearState();
            openCorrectTab((String)o);
            DoUpdateAllConversations();
            this.Visibility = Visibility.Visible;
            Commands.RequerySuggested();
            slideOut();
        }
        private void slideOut()
        {
            if (this.ActualWidth > 0)
            {
                VerticalAlignment = VerticalAlignment.Top;
                HorizontalAlignment = HorizontalAlignment.Left;
                slidePropertyOut(FrameworkElement.WidthProperty, this.ActualWidth);
                slidePropertyOut(FrameworkElement.HeightProperty, this.ActualHeight);
            }
        }
        private void slidePropertyOut(DependencyProperty property, double limit){
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = 150;
            anim.To = limit;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.8));
            anim.AutoReverse = false;
            BeginAnimation(property, anim);
        }
        private void HideConversationSearchBox(object o)
        {
            CloseConversationSearchBox();
        }
        private void JoinConversation(object o)
        {
            CloseConversationSearchBox();
        }
        private void CloseConversationSearchBox()
        {
            this.Visibility = Visibility.Collapsed;
            Commands.RequerySuggested();
        }
        private void UpdateAllConversations(SandRibbonObjects.ConversationDetails details)
        {
            Dispatcher.adopt((Action)delegate
            {
                if (this.Visibility == Visibility.Visible)
                    DoUpdateAllConversations();
            });
        }
        private void DoUpdateAllConversations()
        {
            /*
            Dispatcher.adoptAsync(() =>
                      {
                          SandRibbon.Providers.Structure.ConversationDetailsProviderFactory.Provider.ListConversations().ToList().AddRange(searchResults);
                          if (allConversations.Count != 0)
                          {
                              updateAllConversationsSource();
                              updateMyOwnedConversations();
                              Commands.getCurrentClasses.Execute(null);
                              updateCurrentlyTeachingConversations();
                              if (!string.IsNullOrEmpty(lastSearch))
                              {
                                  searchFor(lastSearch.ToLower());
                              }
                          }
                      });
             */
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
        private ListCollectionView GetListCollectionView()
        {
            return (ListCollectionView) CollectionViewSource.GetDefaultView(this.searchResults );
        }
        private bool isWhatWeWereLookingFor(object o) {
            var conversation = (ConversationDetails) o;     
            var author = conversation.Author.ToLower();
            var title = conversation.Title.ToLower();
            var searchField = new[]{author,title};
            var searchQuery = SearchInput.Text.ToLower().Trim();
            if (backstageNav.currentMode == "find" && searchQuery.Length == 0) return false;
            if (backstageNav.currentMode == "mine" && author != Globals.me) return false;
            var target = searchQuery.Split(' ').Aggregate(false, (acc, token) =>
            {
                if (acc) return true;
                if (String.IsNullOrEmpty(token)) return true;
                var criteria = token.Split(':');
                bool result = false;
                if (criteria.Count() == 1)
                    result = searchField.Any(field=>field.Contains(token));
                else
                {
                    var criterion = criteria[0];
                    var value = criteria[1];
                    switch (criterion)
                    {
                        case "title": result = title.Contains(value); break;
                        case "author": result = author.Contains(value); break;
                        case "jid": result = conversation.Jid.Contains(value); break;
                        case "subject": result = conversation.Subject.Contains(value); break;
                        case "slides":
                            try
                            {
                                result = conversation.Slides.Count > Int32.Parse(value);
                            }
                            catch (FormatException)
                            {
                                result = false;
                            }
                            break;
                        default: result = false; break;
                    }
                }
                return result;
            });
            return target;
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
        private void SelectConversation_Click(object sender, RoutedEventArgs e)
        {
            var conversation = ((Hyperlink)sender).Tag;
            Commands.JoinConversation.Execute(conversation);
        }
        private void SelectConversation_MouseDown(object sender, MouseEventArgs e)
        {
            Commands.JoinConversation.Execute(((FrameworkElement)sender).Tag);
        }
        private void HideConversationSearchBoxButton_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void searchConversations_Click(object sender, RoutedEventArgs e)
        {
            GetListCollectionView().Refresh();
        }
        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(((TextBox)sender).Text.Count() > 2)
                GetListCollectionView().Refresh();
        }
    }
    public class ConversationComparator : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            return -1 * ((ConversationDetails)x).Created.CompareTo(((ConversationDetails)y).Created);
        }
    }
    class ConversationSummary
    {
        public string jid { get; set; }
        public string description { get; set; }
        public string title { get; set; }
    }
}
