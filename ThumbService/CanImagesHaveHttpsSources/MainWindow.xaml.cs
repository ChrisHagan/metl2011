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
using System.Net;
using System.IO;

namespace CanImagesHaveHttpsSources
{
    public class HttpsDownloader : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var url = (string)value;
            return new ImageSourceConverter().ConvertFrom(url);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class MainWindow : Window
    {
        public static HttpsDownloader proxy = new HttpsDownloader();
        public MainWindow()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, error) => true;
            InitializeComponent();
        }
    }
}
