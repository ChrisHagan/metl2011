using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Tabs.Groups
{
    public partial class ToolBox : Divelements.SandRibbon.RibbonGroup
    {
        public ToolBox()
        {
            InitializeComponent();
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(SetLayer));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(setDefaults));
            Commands.SetDrawingAttributes.RegisterCommand(new DelegateCommand<object>(setDefaults));
            SetLayer((string)Commands.SetLayer.lastValue());
        }
        private void setDefaults(object obj)
        {
            Draw.IsChecked = true;
            Commands.SetInkCanvasMode.Execute("Ink");
        }
        private void SetLayer(string layer)
        {
            hideAll();
            switch (layer)
            {
                case "Text":
                    TextOptions.Visibility = Visibility.Visible;
                    break;
                case "Insert":
                    ImageOptions.Visibility = Visibility.Visible;
                    break;
                default:
                    InkOptions.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void hideAll()
        {
            InkOptions.Visibility = Visibility.Collapsed;
            TextOptions.Visibility = Visibility.Collapsed;
            ImageOptions.Visibility = Visibility.Collapsed;
        }
    }
}