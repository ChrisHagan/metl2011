using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
