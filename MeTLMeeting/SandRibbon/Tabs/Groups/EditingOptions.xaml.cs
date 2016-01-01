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
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(SetLayer));
        }
        private void SetLayer(string mode)
        {
            Dispatcher.adopt(delegate
            {
                hideAll();
                switch (mode)
                {
                    case "Select":
                        this.Visibility = Visibility.Collapsed;
                        penColors.Visibility = Visibility.Collapsed;
                        toolBox.Visibility = Visibility.Collapsed;
                        textTools.Visibility = Visibility.Collapsed;
                        break;
                    case "Sketch":
                        this.Visibility = Visibility.Visible;
                        penColors.Visibility = Visibility.Visible;
                        toolBox.Visibility = Visibility.Collapsed;
                        textTools.Visibility = Visibility.Collapsed;
                        Header = "Ink Tools";
                        break;
                    case "Text":
                        this.Visibility = Visibility.Visible;
                        penColors.Visibility = Visibility.Collapsed;
                        toolBox.Visibility = Visibility.Visible;
                        textTools.Visibility = Visibility.Visible;
                        Header = "Text Tools";
                        break;
                    case "Insert":
                        this.Visibility = Visibility.Visible;
                        penColors.Visibility = Visibility.Collapsed;
                        toolBox.Visibility = Visibility.Visible;
                        textTools.Visibility = Visibility.Collapsed;
                        Header = "Image Tools";
                        break;
                    case "View":
                        this.Visibility = Visibility.Collapsed;
                        penColors.Visibility = Visibility.Collapsed;
                        toolBox.Visibility = Visibility.Collapsed;
                        textTools.Visibility = Visibility.Collapsed;
                        Header = "View Tools";
                        break;
                }
            });
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
