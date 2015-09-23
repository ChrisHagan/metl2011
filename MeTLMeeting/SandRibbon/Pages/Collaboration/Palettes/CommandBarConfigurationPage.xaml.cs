using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SandRibbon.Pages.Collaboration.Palettes
{    
    public partial class CommandBarConfigurationPage : Page
    {
        public CommandBarConfigurationPage()
        {
            InitializeComponent();
            TopBar.ItemsSource = Enumerable.Range(0,10);
            sliders.ItemsSource = new[] {
                "ButtonWidth",
                "ButtonHeight",
                "SlotSpacing"
            };
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = e.Source as UIElement;
            Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange);
            Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange);
        }

        private void SliderMoved(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var key = sender as FrameworkElement;
            Resources[key.DataContext as string] = e.NewValue;
        }        
    }
}
