using System.Windows;
using Divelements.SandRibbon;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Providers;
using SandRibbon.Pages;

namespace SandRibbon.Tabs
{
    public partial class ClassManagement: RibbonTab
    {
        public SlideAwarePage rootPage { get; protected set; }
        public ClassManagement()
        {
            InitializeComponent();
            var updateCommand = new DelegateCommand<ConversationDetails>(UpdateConversationDetails);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                {
                    rootPage = DataContext as SlideAwarePage;
                }
                Commands.UpdateConversationDetails.RegisterCommand(updateCommand);
            };
            Commands.UpdateConversationDetails.UnregisterCommand(updateCommand);
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            Dispatcher.adopt(delegate
            {

                manageBlackList.Visibility = details.Author == rootPage.NetworkController.credentials.name ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void ManageBlacklist(object sender, RoutedEventArgs e)
        {
            var blacklist = new blacklistController(rootPage);
            blacklist.Owner = Window.GetWindow(this);
            blacklist.ShowDialog();
        }
    }
}
