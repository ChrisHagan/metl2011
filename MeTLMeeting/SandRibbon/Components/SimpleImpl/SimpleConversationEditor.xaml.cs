using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers.Structure;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using SandRibbonObjects;
using System.Collections.ObjectModel;

namespace SandRibbon.Components.SimpleImpl
{
    public partial class SimpleConversationEditor : UserControl
    {
        public static IEnumerable<ConversationDetails> extantConversations = new List<ConversationDetails>();
        public static IsActivePermissionConverter IsActivePermissionConverter = new IsActivePermissionConverter();
        public static RoutedCommand ProxyUpdateConversationDetails = new RoutedCommand();
        public DelegateCommand<ConversationDetails> update;
        public JabberWire.Credentials credentials;
        public ObservableCollection<string> authorizedGroups = new ObservableCollection<string>();
        private string currentJid;
        public SimpleConversationEditor()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<JabberWire.Credentials>(SetIdentity));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            CommandBindings.Add(new CommandBinding(ProxyUpdateConversationDetails, 
                hydrateAndForwardConversation, 
                checkConversationRouted));
            subjectList.ItemsSource = authorizedGroups;
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (details.Jid != currentJid) return;
            Dispatcher.BeginInvoke((Action)delegate
            {
                this.DataContext = null;
                this.DataContext = details;
            });
        }
        private void JoinConversation(string jid)
        {
            currentJid = jid;
        }
        private void SetIdentity(JabberWire.Credentials credentials)
        {
            this.credentials = credentials;
            authorizedGroups.Clear();
            foreach(var group in credentials.authorizedGroups)
                authorizedGroups.Add(group.groupKey);
            extantConversations = ConversationDetailsProviderFactory.Provider.ListConversations();
        }
        private void raisePotentialConflict(object sender, TextChangedEventArgs e)
        {
            if (update != null)
                update.RaiseCanExecuteChanged();
        }
        private void hydrateAndForwardConversation(object sender, ExecutedRoutedEventArgs e)
        {//Vulnerable to forgetting a datapoint in ConversationDetails
            var proposedDetails = (ConversationDetails)e.Parameter;
            var currentDetails = (ConversationDetails)DataContext;
            proposedDetails.Slides = currentDetails.Slides;
            proposedDetails.Created = currentDetails.Created;
            proposedDetails.LastAccessed = currentDetails.LastAccessed;
            proposedDetails.Id = currentDetails.Id;
            proposedDetails.Rev = currentDetails.Rev;
            proposedDetails.Author = currentDetails.Author;
            proposedDetails.Jid = currentDetails.Jid;
            string sSubject = currentDetails.Subject; // is returning TAG, NOT SUBJECT
            if (checkConversation(proposedDetails))
            {
                ConversationDetailsProviderFactory.Provider.Update(proposedDetails);
            }
            else
                MessageBox.Show("You may not modify this conversation in this way.");
        }
        private void checkConversationRouted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = checkConversation((ConversationDetails)e.Parameter);
        }
        private bool checkConversationCommand(ConversationDetails details)
        {
            return checkConversation(details);
        }
        private bool checkConversation(ConversationDetails proposedDetails)
        {
            if (DataContext == null) return false;//There is no current conversation
            if (extantConversations == null) return false;
            proposedDetails.Title = proposedDetails.Title.Trim();
            var currentDetails = (ConversationDetails)DataContext;
            var iAmTheAuthor = credentials.name == (((ConversationDetails)DataContext).Author);
            var thisIsAValidTitle = !String.IsNullOrEmpty(proposedDetails.Title.Trim());
            var thisTitleIsNotTaken = (extantConversations.Where(c=> (c.Title != currentDetails.Title) && c.Title.ToLower().Equals(proposedDetails.Title.ToLower())).Count() == 0);
            return iAmTheAuthor && thisIsAValidTitle && thisTitleIsNotTaken;
        }
        private void conversationTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }
        public void FocusEdit()
        {
            if (DataContext == null) return;
            var currentDetails = (ConversationDetails)DataContext;
            subjectList.SelectedItem = currentDetails.Subject;
        }
    }
    public class IsActivePermissionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {//Don't offer to let them change to the one they already are
            var actualPermissions = (Permissions)value;
            var isEnabled = !((string)parameter).Equals(Permissions.InferredTypeOf(actualPermissions).Label);
            return isEnabled;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}