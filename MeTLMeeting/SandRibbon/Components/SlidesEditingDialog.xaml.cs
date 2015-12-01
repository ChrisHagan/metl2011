using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SandRibbon.Providers;
using SandRibbon.Pages;

namespace SandRibbon.Components
{
    public partial class SlidesEditingDialog : Window
    {
        private readonly static string[] availableModes = new string[]{"Sharing", "Rearranging"};
        private static string mode = availableModes[0];
        public ConversationAwarePage rootPage { get; protected set; }
        public SlidesEditingDialog(ConversationAwarePage _rootPage)
        {
            rootPage = _rootPage;
            InitializeComponent();
            var slides = rootPage.getDetails().Slides.Select(s=>new MeTLLib.DataTypes.ThumbnailInformation{ 
                Exposed=s.exposed,
                slideId=s.id,
                slideNumber=s.index+1
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
