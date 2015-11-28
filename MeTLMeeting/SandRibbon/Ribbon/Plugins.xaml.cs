using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Controls.Ribbon;
using System.Windows;
using SandRibbon.Providers;

namespace SandRibbon.Tabs
{
    public partial class Plugins : RibbonTab
    {
        public Plugins()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(Update));
        }

        public object Visibiity { get; private set; }

        private void Update(ConversationDetails obj)
        {
            teacherPlugins.Visibility = (obj.Author == Globals.me) ? Visibility.Visible : Visibility.Collapsed;            
        }
    }
}
