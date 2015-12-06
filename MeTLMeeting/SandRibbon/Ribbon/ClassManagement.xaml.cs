using System.Windows;
using Divelements.SandRibbon;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Providers;
using SandRibbon.Pages;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Tabs
{
    public partial class ClassManagement: RibbonTab
    {
        public ClassManagement()
        {
            InitializeComponent();
            var updateCommand = new DelegateCommand<ConversationDetails>(UpdateConversationDetails);
            Loaded += (s, e) =>
            {
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateCommand);
            };
            Commands.UpdateConversationDetails.UnregisterCommand(updateCommand);
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            var rootPage = DataContext as DataContextRoot;
            manageBlackList.Visibility = details.Author == rootPage.NetworkController.credentials.name ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ManageBlacklist(object sender, RoutedEventArgs e)
        {            
            var blacklist = new BlacklistController();
            blacklist.Owner = Window.GetWindow(this);
            blacklist.DataContext = DataContext;
            blacklist.ShowDialog();
        }
    }
}
