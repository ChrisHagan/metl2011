using MeTLLib.DataTypes;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace SandRibbon.Pages.Conversations
{
    public partial class ConversationEditPage : Page
    {
        
        protected NetworkController networkController;
        public ConversationEditPage(NetworkController _networkController)
        {
            networkController = _networkController;
            InitializeComponent();
            errorDisplay.DataContext = Errors;
        }

        public ConversationEditPage(NetworkController _networkController, ConversationDetails conversation) : this(_networkController)
        {
            DataContext = conversation;
        }

        public string Errors
        {
            get { return (string)GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
        }
        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(string), typeof(ConversationEditPage), new UIPropertyMetadata(""));
        private ConversationDetails conversation;

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
        private void deleteConversation(object sender, ExecutedRoutedEventArgs e)
        {
            if (MeTLMessage.Question("Really delete this conversation?") == MessageBoxResult.Yes)
            {
                var details = (ConversationDetails)e.OriginalSource;
                networkController.client.DeleteConversation(details);

            }
        }
        private void saveEdit(object sender, RoutedEventArgs e)
        {
            var conversation = DataContext as ConversationDetails;
            var details =  SearchConversationDetails.HydrateFromServer(networkController.client,conversation);
            var errors = errorsFor(details);
            if (string.IsNullOrEmpty(errors))
            {
                networkController.client.UpdateConversationDetails(details);
                NavigationService.Navigate(new ConversationSearchPage(networkController),details.Title);
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
