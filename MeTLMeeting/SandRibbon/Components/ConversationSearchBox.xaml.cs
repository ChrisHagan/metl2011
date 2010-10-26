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
using System.Globalization;

namespace SandRibbon.Components
{

    public partial class ConversationSearchBox : UserControl
    {
        public class HideIfNotCurrentConversation : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return activeConversation == ((MeTLLib.DataTypes.ConversationDetails)value).Jid ? Visibility.Visible : Visibility.Collapsed;
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return false;
            }
        }

        public class IsMeConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                bool result = false;
                return (me == ((MeTLLib.DataTypes.ConversationDetails)value).Author).ToString();
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return false;
            }
        }
        public static IsMeConverter isMe = new IsMeConverter();
        public static HideIfNotCurrentConversation hideIfNotCurrentConversation = new HideIfNotCurrentConversation();
        private ObservableCollection<MeTLLib.DataTypes.ConversationDetails> searchResults = new ObservableCollection<MeTLLib.DataTypes.ConversationDetails>();
        protected static string activeConversation;
        protected static string me;
        public ConversationSearchBox()
        {
            InitializeComponent();
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<object>(SetIdentity));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateAllConversations));
            Commands.UpdateForeignConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateAllConversations));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.LeaveConversation.RegisterCommand(new DelegateCommand<string>(LeaveConversation));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(ShowConversationSearchBox));
            Commands.HideConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(HideConversationSearchBox));
            Commands.BackstageModeChanged.RegisterCommand(new DelegateCommand<string>(BackstageModeChanged));
            SearchResults.ItemsSource = searchResults;
            activeConversation = Globals.location.activeConversation;
            me = Globals.me;
            var view = GetListCollectionView();
            view.Filter = isWhatWeWereLookingFor;
            view.CustomSort = new ConversationComparator();
        }
        private void BackstageModeChanged(string mode)
        {
            Dispatcher.adoptAsync(() =>
            {
                updateLiveButton(mode);
                GetListCollectionView().Refresh();
            });
                string searchButtonText;
            switch (mode)
            {
                case "mine": searchButtonText = "Filter my Conversations"; break;
                case "all": searchButtonText = "Filter all Conversations"; break;
                default: searchButtonText = "Search all Conversations"; break;
            }
            Dispatcher.adoptAsync(() =>
            {
                if (searchConversations == null) return;
                searchConversations.Content = searchButtonText;
            });
        }

        private void updateLiveButton(string mode)
        {
            var elements = new[] {mine, all, find, currentConversation};
            foreach (var button in elements)
                if (button.Name == mode)
                    button.IsChecked = true;
        }
        private void SetIdentity(object _arg){
            var availableConversations = MeTLLib.ClientFactory.Connection().AvailableConversations;
            Dispatcher.adoptAsync(() =>
            {
                foreach (var conversation in availableConversations)
                    if(conversation.Subject.ToLower() != "deleted")
                        searchResults.Add(conversation);
            });
        }
        private void clearState(){
            Dispatcher.adoptAsync(() => {
                SearchInput.Text = "";
                GetListCollectionView().Refresh();
                SearchInput.Focus();
            });
        }
        private void ShowConversationSearchBox(object o)
        {
            activeConversation = Globals.location.activeConversation;
            me = Globals.me;
            if (String.IsNullOrEmpty(activeConversation))

                currentConversation.Visibility = Visibility.Collapsed;
            else {
                currentConversation.Visibility = Visibility.Visible;
            }
            clearState();
            this.Visibility = Visibility.Visible;
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
            this.Visibility = Visibility.Collapsed;
            Commands.RequerySuggested();
        }
        private void JoinConversation(object o)
        {
            CloseConversationSearchBox();
        }
        private void CloseConversationSearchBox()
        {
            Commands.HideConversationSearchBox.Execute(null);
        }
        private void LeaveConversation(string jid)
        {
        }
        private void UpdateAllConversations(MeTLLib.DataTypes.ConversationDetails details)
        {
            if (details == null) return;
            foreach (var result in searchResults.Where(c => c.Jid == details.Jid).ToList())
                searchResults.Remove(result);
            if(details.Subject.ToLower() != "deleted")
                searchResults.Add(details);

            if (!(shouldShowConversation(details)) && details.Jid == Globals.conversationDetails.Jid)
            {
                Commands.LeaveConversation.ExecuteAsync(details.Jid);
                Commands.RequerySuggested();
                this.Visibility = Visibility.Visible;
            }
            GetListCollectionView().Refresh();
        }
        private bool shouldShowConversation(ConversationDetails conversation)
        {
            if (!(Globals.credentials.authorizedGroups.Select(g => g.groupKey).Contains(conversation.Subject))
                && conversation.Subject != "Unrestricted"
                && !(String.IsNullOrEmpty(conversation.Subject))
                && !(Globals.credentials.authorizedGroups.Select(su => su.groupKey).Contains("Superuser"))) return false;
            return true;
        }
        private ListCollectionView GetListCollectionView()
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(this.searchResults);
        }
        private bool isWhatWeWereLookingFor(object o)
        {
            var conversation = (MeTLLib.DataTypes.ConversationDetails)o;
            if (!shouldShowConversation(conversation)) 
                return false;
            if (backstageNav.currentMode == "currentConversation")
                if(conversation.Jid != activeConversation) 
                    return false;
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
                var details = (MeTLLib.DataTypes.ConversationDetails)((FrameworkElement)sender).DataContext;
                details.Subject = "Deleted";
                MeTLLib.ClientFactory.Connection().UpdateConversationDetails(details);
            }
        }
        private void mode_Checked(object sender, RoutedEventArgs e)
        {
            var mode = ((FrameworkElement)sender).Name;
            backstageNav.currentMode = mode;
        }
        private ContentPresenter view(object backedByConversation) { 
            var conversation = (ConversationDetails)((FrameworkElement)backedByConversation).DataContext;
            var item = SearchResults.ItemContainerGenerator.ContainerFromItem(conversation);
            var presenter = (ContentPresenter)item;
            return presenter;
        }
        private ConversationDetails context(object sender) {
            return (ConversationDetails)((FrameworkElement)sender).DataContext;
        }
        private void assignTemplate(string dataTemplateResourceKey, object sender){
            var sentContext = context(sender);
            var presenter = view(sender);
            presenter.Content = sentContext;
            presenter.ContentTemplate = (DataTemplate)FindResource(dataTemplateResourceKey);
        }
        private void renameConversation(object sender, RoutedEventArgs e)
        {
            assignTemplate("rename", sender);
        }
        private void shareConversation(object sender, RoutedEventArgs e)
        {
            assignTemplate("share", sender);
        }
        private void cancelEdit(object sender, RoutedEventArgs e)
        {
            assignTemplate("viewing", sender);
        }
        private void saveEdit(object sender, RoutedEventArgs e)
        {
            var details = (MeTLLib.DataTypes.ConversationDetails)((FrameworkElement)sender).DataContext;
            MeTLLib.ClientFactory.Connection().UpdateConversationDetails(details);
            assignTemplate("viewing", sender);
        }
    }
    public class ConversationComparator : System.Collections.IComparer
    {
        public int Compare(object x, object y) {
            var dis = (MeTLLib.DataTypes.ConversationDetails)x;
            var dat = (MeTLLib.DataTypes.ConversationDetails)y;
            return -1 * dis.Created.CompareTo(dat.Created);
        }
    }
}
