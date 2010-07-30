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
    public partial class EditingOptions : RibbonGroup
    {
        public EditingOptions()
        {
            InitializeComponent();
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(switchTools));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(updatePrivacyTools));
        }
        private void updatePrivacyTools(string jid)
        {
            try
            {
                if (new FileConversationDetailsProvider().DetailsOf(jid).Author == Globals.me || Globals.pedagogy.code > 2)
                    ribbonPrivacyTools.Visibility = Visibility.Visible;
                else { ribbonPrivacyTools.Visibility = Visibility.Collapsed; }
            }
            catch (Exception ex) { }
        }
        private void switchTools(string mode)
        {
            hideAll();
            this.Visibility = Visibility.Visible;
            switch (mode)
            {
                case "Sketch":
                    penColors.Visibility = Visibility.Visible;
                    Header = "Drawing Tools";
                    break;
                case "Text":
                    toolBox.Visibility = Visibility.Visible;
                    textTools.Visibility = Visibility.Visible;
                    Header = "Writing Tools";
                    break;
                case "Insert":
                    toolBox.Visibility = Visibility.Visible;
                    Header = "Image Tools";
                    break;
                case "View":
                    Header = "View Tools";
                    this.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void hideAll() {
            foreach (FrameworkElement child in new FrameworkElement[]{penColors,textTools,toolBox})
            {
                child.Visibility = Visibility.Collapsed;
            }
        }
    }
}
