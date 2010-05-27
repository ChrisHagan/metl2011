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
                    Commands.DisablePens.Execute(null);
                    break;
                case "Insert":
                    ImageOptions.Visibility = Visibility.Visible;
                    Commands.DisablePens.Execute(null);
                    break;
                default:
                    this.Visibility = Visibility.Collapsed;
                    Commands.EnablePens.Execute(null);
                    //InkOptions.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void hideAll()
        {
            //InkOptions.Visibility = Visibility.Collapsed;
            TextOptions.Visibility = Visibility.Collapsed;
            ImageOptions.Visibility = Visibility.Collapsed;
        }
    }
}