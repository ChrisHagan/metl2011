using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using SandRibbonObjects;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Input;
using System.Windows;
using SandRibbon.Providers.Structure;
using System.Collections.Generic;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;

namespace SandRibbon.Components.SimpleImpl
{
    public partial class SimpleConversationEditor : UserControl
    {
        public static IEnumerable<ConversationDetails> extantConversations = new List<ConversationDetails>();
        public static IsActivePermissionConverter IsActivePermissionConverter = new IsActivePermissionConverter();
        public static RoutedCommand UpdateConversationDetails = new RoutedCommand();
        public DelegateCommand<ConversationDetails> update;
        public string me { get; set; }
        public JabberWire.AuthorizedGroups oGroups { get; set; }
        public SimpleConversationEditor()
        {
            InitializeComponent();
            extantConversations = ConversationDetailsProviderFactory.Provider.ListConversations();
            Commands.ReceiveConversationListUpdate.RegisterCommand(new DelegateCommand<IEnumerable<ConversationDetails>>(
                list =>{
                    if(list != null) extantConversations = list;
                }));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(
                details => Dispatcher.Invoke((Action)delegate{
                    this.DataContext = null;
                    this.DataContext = details;
                })));
            Commands.ModifyCurrentConversation.RegisterCommand(new DelegateCommand<ConversationDetails>(
                _details => { },
                checkConversationCommand));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(
                author => me = author.name));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(
                author => oGroups = author.authorizedGroups));
            CommandBindings.Add(new CommandBinding(UpdateConversationDetails, 
                hydrateAndForwardConversation, 
                checkConversationRouted));
            // ST: This doesn't work because it is fired before the user has logged in
            //SandRibbon.Utils.GroupsManagement.fillComboWithEligibleGroups(subjectList, oGroups);
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
            /*
            SandRibbon.Utils.WebServiceUtilities.setSubject(subjectList, sSubject);
             */
            if (checkConversation(proposedDetails))
                Commands.ModifyCurrentConversation.Execute(proposedDetails);
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
            proposedDetails.Title = proposedDetails.Title.Trim();
            var currentDetails = (ConversationDetails)DataContext;
            var iAmTheAuthor = me == (((ConversationDetails)DataContext).Author);
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
            GroupsManagement.fillComboWithEligibleGroups(subjectList, oGroups);
            // The combo is then set to the current conversation's subject value
            if (DataContext == null) return;//There is no current conversation
            var currentDetails = (ConversationDetails)DataContext;
            GroupsManagement.setSubject(subjectList, currentDetails.Subject);
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