using System;
using System.Windows;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages;

namespace SandRibbon.Tabs.Groups
{
    /// <summary>
    /// Interaction logic for CollaborationControlsHost.xaml
    /// </summary>
    public partial class CollaborationControlsHost 
    {
        public static readonly DependencyProperty NavigationIsLockedProperty = DependencyProperty.Register("NavigationIsLocked", typeof (bool), typeof(CollaborationControlsHost));
        public bool NavigationIsLocked 
        {
            get { return (bool)GetValue(NavigationIsLockedProperty); }
            set { SetValue(NavigationIsLockedProperty, value); }

        }
        public SlideAwarePage rootPage { get; protected set; }
        public CollaborationControlsHost()
        {
            InitializeComponent();
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(updateConversationDetails);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as SlideAwarePage;
                DataContext = this;
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateConversationDetailsCommand);
                joinConversation(rootPage.ConversationDetails);
            };
            Unloaded += (s, e) =>
            {
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
            };
        }
        private void updateConversationDetails(ConversationDetails details)
        {
            NavigationIsLocked = details.Permissions.NavigationLocked;
            /*
            if (details.Permissions.studentCanPublish)
            {
                tutorialStyle.IsChecked = true;
            }
            else {
                lectureStyle.IsChecked = true;
            }
            */
        }
        private void joinConversation(ConversationDetails details)
        {
            this.Visibility = details.isAuthor(rootPage.NetworkController.credentials.name) ? Visibility.Visible : Visibility.Collapsed;
            NavigationIsLocked = details.Permissions.NavigationLocked;
        }
    }
}
