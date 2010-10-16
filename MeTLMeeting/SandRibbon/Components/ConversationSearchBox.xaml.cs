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
using SandRibbon.Providers.Structure;
using SandRibbon.Utils;
using MeTLLib.DataTypes;
using System.ComponentModel;

namespace SandRibbon.Components
{
    public partial class ConversationSearchBox : UserControl
    {
        private ObservableCollection<MeTLLib.DataTypes.ConversationDetails> searchResults = new ObservableCollection<MeTLLib.DataTypes.ConversationDetails>();

        public ConversationSearchBox()
        {
            InitializeComponent();
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<object>(SetIdentity));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateAllConversations));
            Commands.UpdateForeignConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateAllConversations));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(ShowConversationSearchBox));
            Commands.HideConversationSearchBox.RegisterCommand(new DelegateCommand<object>(HideConversationSearchBox));
            Commands.BackstageModeChanged.RegisterCommand(new DelegateCommand<string>(BackstageModeChanged));
            SearchResults.ItemsSource = searchResults;
            var view = GetListCollectionView();
            view.Filter = isWhatWeWereLookingFor;
            view.CustomSort = new ConversationComparator();
        }
        private void BackstageModeChanged(string mode)
        {
            GetListCollectionView().Refresh();
            string searchButtonText;
            switch (mode)
            {
                case "mine": searchButtonText = "Filter my Conversations"; break;
                case "all": searchButtonText = "Filter all Conversations"; break;
                default: searchButtonText = "Search all Conversations"; break;
            }
            searchConversations.Content = searchButtonText;
        }
        private void SetIdentity(object _arg){
            var availableConversations = MeTLLib.ClientFactory.Connection().AvailableConversations;
            Dispatcher.adoptAsync(()=>{
            foreach(var conversation in availableConversations)
                searchResults.Add(conversation);
            });
        }
        private void clearState()
        {
            Dispatcher.adoptAsync(() =>
            {
                SearchInput.Text = "";
                GetListCollectionView().Refresh();
            });
        }
        private void ShowConversationSearchBox(object o)
        {
            clearState();
            Dispatcher.adoptAsync(() =>
            this.Visibility = Visibility.Visible);
            Commands.RequerySuggested();
            slideOut();
        }
        private void slideOut()
        {
            Dispatcher.adoptAsync(() =>
            {
                if (this.ActualWidth > 0)
                {
                    VerticalAlignment = VerticalAlignment.Top;
                    HorizontalAlignment = HorizontalAlignment.Left;
                    slidePropertyOut(FrameworkElement.WidthProperty, this.ActualWidth);
                    slidePropertyOut(FrameworkElement.HeightProperty, this.ActualHeight);
                }
            });
        }
        private void slidePropertyOut(DependencyProperty property, double limit)
        {
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
            Dispatcher.adoptAsync(delegate { this.Visibility = Visibility.Collapsed; });
            Commands.RequerySuggested();
        }
        private void UpdateAllConversations(MeTLLib.DataTypes.ConversationDetails details)
        {
            Dispatcher.adopt((Action)delegate
            {
                if (searchResults.Where(c => c.Jid == details.Jid).Count() == 1)
                    searchResults.Remove(searchResults.Where(c => c.Jid == details.Jid).First());
                if (!details.Subject.Contains("Deleted"))
                    searchResults.Add(details);
                else //conversation deleted
                {
                    Commands.RequerySuggested();
                    //if (Globals.location.activeConversation == details.Jid && this.Visibility == Visibility.Collapsed)
                    if (Globals.conversationDetails.Jid == details.Jid && this.Visibility == Visibility.Collapsed)
                    {
                        this.Visibility = Visibility.Visible;
                    }
                }
                GetListCollectionView().Refresh();
            });
        }
        private ListCollectionView GetListCollectionView()
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(this.searchResults);
        }
        private bool isWhatWeWereLookingFor(object o)
        {
            var conversation = (MeTLLib.DataTypes.ConversationDetails)o;
            var author = conversation.Author.ToLower();
            var title = conversation.Title.ToLower();
            var searchField = new[] { author, title };
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
                    result = searchField.Any(field => field.Contains(token));
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
        private void searchConversations_Click(object sender, RoutedEventArgs e)
        {
            GetListCollectionView().Refresh();
        }
        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Count() > 2)
                GetListCollectionView().Refresh();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Commands.JoinConversation.ExecuteAsync(((FrameworkElement)sender).Tag);
        }
        private void deleteConversation(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Really delete this conversation?", "Delete Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var details = (MeTLLib.DataTypes.ConversationDetails)((SandRibbonInterop.Button)sender).DataContext;
                details.Subject = "Deleted";
                MeTLLib.ClientFactory.Connection().UpdateConversationDetails(details);
                //ConversationDetailsProviderFactory.Provider.Update(details);
            }
        }
        private void editConversation(object sender, RoutedEventArgs e)
        {
            Commands.EditConversation.Execute(((MeTLLib.DataTypes.ConversationDetails)((SandRibbonInterop.Button)sender).DataContext).Jid);
        }
    }
    public class ConversationComparator : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            return -1 * ((MeTLLib.DataTypes.ConversationDetails)x).Created.CompareTo(((MeTLLib.DataTypes.ConversationDetails)y).Created);
        }
    }
}
