using System.Windows;
using Divelements.SandRibbon;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Providers;
using MeTLLib;

namespace SandRibbon.Tabs
{
    public partial class ClassManagement: RibbonTab
    {
        public MetlConfiguration backend;

        public ClassManagement()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            manageBlackList.Visibility = details.Author == Globals.me ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ManageBlacklist(object sender, RoutedEventArgs e)
        {
            var blacklist = new blacklistController(backend);
            blacklist.Owner = Window.GetWindow(this);
            blacklist.ShowDialog();
        }
    }
}
