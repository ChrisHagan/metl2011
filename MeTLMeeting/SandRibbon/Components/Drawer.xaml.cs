using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Pedagogicometry;
using System.Windows;

namespace SandRibbon.Components
{
    public partial class Drawer : UserControl
    {
        public Drawer()
        {
            InitializeComponent();
           // adornerScroll.scroll = scroll;
            //adornerScroll.scroll.SizeChanged += adornerScroll.scrollChanged;
            //adornerScroll.scroll.ScrollChanged += adornerScroll.scroll_ScrollChanged;
            Commands.SetPedagogyLevel.RegisterCommandToDispatcher(new DelegateCommand<PedagogyLevel>(setPedagogy));
        }

        private void setPedagogy(PedagogyLevel level)
        {
            switch (level.code)
            {
                case PedagogyCode.CollaborativePresentation:
                    controls.Visibility = Visibility.Visible;
                    break;
                default:
                    controls.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void changeMode(object sender, RoutedEventArgs e)
        {
            var mode = ((FrameworkElement)sender).Name;
            if (notes == null || slideDisplay == null) return;
            switch (mode)
            {
                case "noteMode":
                    notes.Visibility = Visibility.Visible;
                    slideDisplay.Visibility = System.Windows.Visibility.Hidden;
                    break;
                default:
                    notes.Visibility = Visibility.Hidden;
                    slideDisplay.Visibility = Visibility.Visible;
                    break;
            }
    
        }
    }
}
