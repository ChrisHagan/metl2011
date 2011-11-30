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
        public CollaborationControlsHost()
        {
            InitializeComponent();
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(joinConversation));
        }

        private void joinConversation(object obj)
        {
            if(Globals.isAuthor)
                this.Visibility = Visibility.Visible;
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }
    }
}
