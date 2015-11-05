using System.Windows;
using Divelements.SandRibbon;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Tabs.Groups
{
    public partial class EditingOptions : RibbonGroup
    {
        public EditingOptions()
        {
            InitializeComponent();
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
        }
        private void SetLayer(string mode)
        {
            hideAll();
            this.Visibility = Visibility.Visible;
            switch (mode)
            {
                case "Select":
                    break;
                case "Sketch":
                    penColors.Visibility = Visibility.Visible;
                    Header = "Ink Tools";
                    break;
                case "Text":
                    toolBox.Visibility = Visibility.Visible;
                    textTools.Visibility = Visibility.Visible;
                    Header = "Text Tools";
                    break;
                case "Insert":
                    toolBox.Visibility = Visibility.Visible;
                    Header = "Image Tools";
                    break;
                case "View":
                    Header = "View Tools";
                    break;
            }
        }
        private void hideAll()
        {
            foreach (FrameworkElement child in new FrameworkElement[] { penColors, textTools, toolBox })
            {
                child.Visibility = Visibility.Collapsed;
            }
        }
    }
}
