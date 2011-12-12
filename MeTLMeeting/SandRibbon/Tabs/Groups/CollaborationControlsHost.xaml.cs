using System;
using System.Windows;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Pedagogicometry;
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
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(joinConversation));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<object>(updateConversationDetails));
            Commands.SetPedagogyLevel.RegisterCommand(new DelegateCommand<PedagogyLevel>(SetPedagogyLevel));
        }

        private void SetPedagogyLevel(PedagogyLevel level)
        {
            if((int)level.code > 2)
            {
                lectureStyle.Visibility = System.Windows.Visibility.Visible;
                tutorialStyle.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lectureStyle.Visibility = System.Windows.Visibility.Collapsed;
                tutorialStyle.Visibility = System.Windows.Visibility.Collapsed;
            }
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
