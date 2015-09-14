using System;
using System.Windows;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Providers;
using MeTLLib.DataTypes;

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
        public CollaborationControlsHost()
        {
            InitializeComponent();
            DataContext = this;
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(joinConversation));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(updateConversationDetails));
        }
        private void updateConversationDetails(ConversationDetails details)
        {
            NavigationIsLocked = details.Permissions.NavigationLocked;
            if (details.Permissions.studentCanPublish)
            {
                tutorialStyle.IsChecked = true;
            }
            else {
                lectureStyle.IsChecked = true;
            }
        }
        private void joinConversation(object obj)
        {
            this.Visibility = Globals.isAuthor ? Visibility.Visible : Visibility.Collapsed;
            NavigationIsLocked = Globals.conversationDetails.Permissions.NavigationLocked;
        }
    }
}
