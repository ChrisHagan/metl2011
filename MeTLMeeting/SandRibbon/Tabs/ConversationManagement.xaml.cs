using System.Windows;
using Divelements.SandRibbon;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.DataTypes;
using SandRibbon.Providers;

namespace SandRibbon.Tabs
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConversationManagement : RibbonTab
    {
        public ConversationManagement()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(updateConversationDetails));
        }
        private void updateConversationDetails(ConversationDetails details)
        {
            editConversation.Visibility = details.Author == Globals.me ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
