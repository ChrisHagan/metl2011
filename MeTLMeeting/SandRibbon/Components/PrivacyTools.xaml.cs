using System;
using System.Windows;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Controls.Ribbon;
using SandRibbon.Pages;
using System.ComponentModel;

namespace SandRibbon.Components
{
    public partial class PrivacyTools : RibbonGroup
    {
        public SlideAwarePage rootPage { get; protected set; }
        public PrivacyTools()
        {
            InitializeComponent();
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(updateConversationDetails);
            var privacyChangedEventHandler = new EventHandler((evs, eve) =>
            {
                var requestedPrivacy = rootPage.UserConversationState.Privacy;
                var conversation = rootPage.ConversationDetails;
                var me = rootPage.NetworkController.credentials.name;
                constrainChoice(conversation.isAuthor(me) || conversation.Permissions.studentCanPublish, requestedPrivacy);

            });
            var privacyProperty = DependencyPropertyDescriptor.FromProperty(UserConversationState.PrivacyProperty, typeof(UserConversationState));
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                {
                    rootPage = DataContext as SlideAwarePage;
                }
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                privacyProperty.AddValueChanged(this, privacyChangedEventHandler);

                var details = rootPage.ConversationDetails;
                var userConv = rootPage.UserConversationState;                
                constrainChoice(
                    details.isAuthor(rootPage.NetworkController.credentials.name) || details.Permissions.studentCanPublish, 
                    userConv.Privacy == Privacy.NotSet ? Privacy.Public : Privacy.NotSet);                
            };
            Unloaded += (s, e) =>
            {
                privacyProperty.RemoveValueChanged(this, privacyChangedEventHandler);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
            };
        }

        private void constrainChoice(bool canChoosePublic, Privacy requestedPrivacy)
        {
            var chosePublic = requestedPrivacy == Privacy.Public;
            if (canChoosePublic)
            {
                publicMode.IsEnabled = true;
                (requestedPrivacy == Privacy.Public ? publicMode : privateMode).IsChecked = true;
            }
            else if (chosePublic)
            {
                /*Correct upstream model and let it cascade back past this branch*/
                rootPage.UserConversationState.Privacy = Privacy.Private;
            }
            else
            {
                publicMode.IsEnabled = false;
                privateMode.IsChecked = true;
            }
            if (rootPage.UserConversationState.Privacy == Privacy.NotSet)
            {
                rootPage.UserConversationState.Privacy = requestedPrivacy;
            }
        }

        private void updateConversationDetails(ConversationDetails details)
        {
            var userConv = rootPage.UserConversationState;
            var canChoose = details.isAuthor(rootPage.NetworkController.credentials.name) || details.Permissions.studentCanPublish;
            constrainChoice(canChoose, userConv.Privacy);            
        }
        private void publicMode_Click(object sender, RoutedEventArgs e)
        {
            rootPage.UserConversationState.Privacy = Privacy.Public;
        }
        private void privateMode_Click(object sender, RoutedEventArgs e)
        {
            rootPage.UserConversationState.Privacy = Privacy.Private;
        }
    }    
}