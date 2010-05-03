using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Interfaces;

namespace SandRibbon.Components
{
    public partial class SimplePencilCaseDisplay : UserControl, IPencilCaseDisplay
    {
        public SimplePencilCaseDisplay()
        {
            InitializeComponent();
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(updateToolBox));
        }
        private void updateToolBox(string layer)
        {
            if (layer == "Sketch")
                LayoutRoot.Visibility = Visibility.Visible;
            else
                LayoutRoot.Visibility = Visibility.Collapsed;
        }
        private void colors_ColorPicked(object sender, Divelements.SandRibbon.ColorEventArgs e)
        {
            Commands.SetPenColor.Execute(e.Color);
        }
        public void pressHighlighterButton(object sender, RoutedEventArgs e)
        {
            Commands.ToggleHighlighterMode.Execute(null);
        }
        public void pressDecreasePenSizeButton(object sender, RoutedEventArgs e)
        {
            Commands.DecreasePenSize.Execute(null);
        }
        public void pressIncreasePenSizeButton(object sender, RoutedEventArgs e)
        {
            Commands.IncreasePenSize.Execute(null);
        }
        public void pressDefaultPenSizeButton(object sender, RoutedEventArgs e)
        {
            Commands.RestorePenSize.Execute(null);
        }
        public void Disable()
        {
            this.colors.BitmapEffect = new BlurBitmapEffect();
            this.IncreasePenSizeButton.IsEnabled = false;
            this.DecreasePenSizeButton.IsEnabled = false;
            this.RestorePenSizeButton.IsEnabled = false;
            this.ToggleHighlighterModeButton.IsEnabled = false;
        }
        public void Enable()
        {
            this.colors.BitmapEffect = null;
            this.IncreasePenSizeButton.IsEnabled = true;
            this.DecreasePenSizeButton.IsEnabled = true;
            this.RestorePenSizeButton.IsEnabled = true;
            this.ToggleHighlighterModeButton.IsEnabled = true;

        }
    }
}
