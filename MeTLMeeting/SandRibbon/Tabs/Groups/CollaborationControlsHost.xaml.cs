using System.Windows;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;

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
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(joinConversation));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<object>(updateConversationDetails));
        }
        private void updateConversationDetails(object obj)
        {
            NavigationIsLocked = Globals.conversationDetails.Permissions.NavigationLocked;
        }
        private void joinConversation(object obj)
        {
            this.Visibility = Globals.isAuthor ? Visibility.Visible : Visibility.Collapsed;
            NavigationIsLocked = Globals.conversationDetails.Permissions.NavigationLocked;
        }
    }
}
