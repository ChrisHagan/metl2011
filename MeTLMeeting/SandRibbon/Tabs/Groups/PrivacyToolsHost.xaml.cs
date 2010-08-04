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
using SandRibbon.Providers.Structure;
using SandRibbon.Providers;

namespace SandRibbon.Tabs.Groups
{
    public partial class PrivacyToolsHost : RibbonGroup
    {
        public PrivacyToolsHost()
        {
            InitializeComponent();
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(updatePrivacyTools));
        }
        private void updatePrivacyTools(string jid)
        {
            try
            {
                if (new FileConversationDetailsProvider().DetailsOf(jid).Author == Globals.me || Globals.pedagogy.code > 2)
                    //ribbonPrivacyTools.Visibility = Visibility.Visible;
                    this.Visibility = Visibility.Visible;
                else {
                    //ribbonPrivacyTools.Visibility = Visibility.Collapsed; 
                    this.Visibility = Visibility.Collapsed; 
                }
            }
            catch (Exception ex) { }
        }
     }
}
