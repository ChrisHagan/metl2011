using MeTLLib.DataTypes;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Navigation;
using System.Linq;

namespace SandRibbon.Pages.Conversations
{
    public partial class ConversationEditPage : ConversationAwarePage
    {
        public ConversationEditPage(UserGlobalState _userGlobal, UserServerState _userServer, UserConversationState _userConv, NetworkController _networkController, ConversationState conversationState)
        {

            UserGlobalState = _userGlobal;
            UserServerState = _userServer;
            UserConversationState = _userConv;
            ConversationState = conversationState;
            NetworkController = _networkController;
            InitializeComponent();
            errorDisplay.DataContext = Errors;
            DataContext = conversationState;
            sharing.ItemsSource = NetworkController.credentials.authorizedGroups.Select(s => s.groupKey);
        }

        public string Errors
        {
            get { return (string)GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
        }
        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(string), typeof(ConversationEditPage), new UIPropertyMetadata(""));

        private string errorsFor(ConversationDetails proposedDetails)
        {
            proposedDetails.Title = proposedDetails.Title.Trim();
            var thisTitleIsASCII = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(proposedDetails.Title)).Equals(proposedDetails.Title);
            var thisIsAValidTitle = !String.IsNullOrEmpty(proposedDetails.Title.Trim());
            var errorText = String.Empty;
            if (proposedDetails.Title.Length > 110) errorText += "Conversation titles have a maximum length of 110 characters";
            if (!thisTitleIsASCII)
                errorText += "Conversation title can only contain letters, numbers and punctuation marks. ";
            if (!thisIsAValidTitle) { errorText += "Invalid conversation title.  "; }
            return errorText;
        }
        private void saveEdit(object sender, RoutedEventArgs e)
        {
            var conversation = DataContext as ConversationDetails;
            var details = SearchConversationDetails.HydrateFromServer(NetworkController.client, conversation);
            var errors = errorsFor(details);
            if (string.IsNullOrEmpty(errors))
            {
                NetworkController.client.UpdateConversationDetails(details);
                NavigationService.Navigate(new ConversationSearchPage(UserGlobalState, UserServerState, NetworkController, details.Title));
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
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void deleteConversation(object sender, RoutedEventArgs e)
        {
            if (MeTLMessage.Question("Really delete this conversation?") == MessageBoxResult.Yes)
            {
                var conversation = DataContext as ConversationDetails;
                NetworkController.client.DeleteConversation(conversation);
                NavigationService.Navigate(new ConversationSearchPage(UserGlobalState, UserServerState, NetworkController, NetworkController.credentials.name));
            }
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
}
