using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Controls.Ribbon;
using System.Windows;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Tabs
{
    public partial class Plugins : RibbonTab
    { 
        public Plugins()
        {
            InitializeComponent();
            var updateCommand = new DelegateCommand<ConversationDetails>(Update);
            Loaded += (s, e) => {     
                Commands.UpdateConversationDetails.RegisterCommand(updateCommand);
            };
            Unloaded += (s, e) =>
            {
                Commands.UpdateConversationDetails.UnregisterCommand(updateCommand);
            };
        }

        public object Visibiity { get; private set; }

        private void Update(ConversationDetails obj)
        {
            var rootPage = DataContext as DataContextRoot;
            Dispatcher.adopt(delegate {
                teacherPlugins.Visibility = (obj.Author == rootPage.NetworkController.credentials.name) ? Visibility.Visible : Visibility.Collapsed;
            });
        }
    }
}
