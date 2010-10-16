using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Tabs.Groups
{
    public partial class ToolBox : UserControl
    {
        public ToolBox()
        {
            InitializeComponent();
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
            SetLayer((string)Commands.SetLayer.lastValue());
        }
        private void SetLayer(string layer)
        {
            hideAll();
            this.Visibility = Visibility.Visible;
            switch (layer)
            {
                case "Text":
                    TextOptions.Visibility = Visibility.Visible;
                    Commands.TogglePens.Execute(false);
                    break;
                case "Insert":
                    ImageOptions.Visibility = Visibility.Visible;
                    Commands.TogglePens.Execute(false);
                    break;
                default:
                    this.Visibility = Visibility.Collapsed;
                    Commands.TogglePens.Execute(true);
                    //InkOptions.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void hideAll()
        {
            TextOptions.Visibility = Visibility.Collapsed;
            ImageOptions.Visibility = Visibility.Collapsed;
        }
    }
}