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
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher<object>(new DelegateCommand<object>(UpdateConversationDetails));
        }
        private void UpdateConversationDetails(object _arg)
        {
            try
            {
                if (Globals.isAuthor || Globals.pedagogy.code > 2)
                    this.Visibility = Visibility.Visible;
                else {
                    this.Visibility = Visibility.Collapsed; 
                }
            }
            catch (NotSetException e) {
                throw new Exception("PrivacyToolsHost tried to use unset values ",e);
            }
        }
     }
}
