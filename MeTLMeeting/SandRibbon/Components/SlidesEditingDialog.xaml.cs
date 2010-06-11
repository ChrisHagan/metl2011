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
using SandRibbon.Providers;
using SandRibbonInterop;

namespace SandRibbon.Components
{
    public partial class SlidesEditingDialog
    {
        private readonly static string[] availableModes = new string[]{"Sharing", "Rearranging"};
        private static string mode = availableModes[0];
        public SlidesEditingDialog()
        {
            InitializeComponent();
            var slides = Globals.slides.Select(s=>new ThumbnailInformation{ 
                Exposed=s.exposed,
                slideId=s.id,
                slideNumber=s.index+1,
                Thumbnail=ThumbnailProvider.get(s.id)
            });
            exposed.ItemsSource = slides;
            modes.ItemsSource=availableModes;
        }
        private void modeSelected(object sender, RoutedEventArgs args) { 
            SlidesEditingDialog.mode = (string)((FrameworkElement)sender).DataContext;
        }
    }
    public class RedFogApplicator : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Brushes.Transparent : Brushes.Red;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class VerticalOffsetCalculator : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? new Thickness(0,0,0,0) : new Thickness(0,0,80,0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
