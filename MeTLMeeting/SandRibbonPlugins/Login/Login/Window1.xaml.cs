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
using System.Globalization;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;
using System.Net;

namespace Login
{
    public partial class Window1 : Window
    {
        public static List<int> HOLES = Enumerable.Range(0, 4).ToList();
        static int TOOTH_COUNT = 18;
        public static List<double> TEETH = Enumerable.Range(1,TOOTH_COUNT).Select(i=>(360.0/TOOTH_COUNT)*i).ToList();
        static Random random = new Random();
        static ObservableCollection<ServerStatus> SERVERS =
            new List<string> { "Print", "Powerpoint", "Resource", "Messaging", "Config", "Authentication", "Structure" }.Aggregate(
                new ObservableCollection<ServerStatus>(),
                (acc, item) => {
                    acc.Add(new ServerStatus{
                        job=item,
                        server = random.Next(50)<20?"civic.adm.monash.edu.au":"notaserver"
                    });
                    return acc;
                });
        public Window1()
        {
            InitializeComponent();
            servers.ItemsSource = SERVERS;
        }
        private void loaded(object sender, RoutedEventArgs e)
        {
            foreach (var server in SERVERS)
            {
                var _server = server;
                var ping = new Ping();
                ping.PingCompleted += (_sender, pingArgs) =>
                {
                    if (pingArgs.Reply != null && pingArgs.Reply.Status == IPStatus.Success)
                        _server.ok = true;
                    else
                        _server.ok = false;
                };
                ping.SendAsync(_server.server, null);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var a = sender;
        }
    }
    class ServerStatus : DependencyObject
    {
        public string job
        {
            get { return (string)GetValue(jobProperty); }
            set { SetValue(jobProperty, value); }
        }
        public static readonly DependencyProperty jobProperty =
            DependencyProperty.Register("job", typeof(string), typeof(ServerStatus), new UIPropertyMetadata("Nothing"));
        public string server
        {
            get { return (string)GetValue(serverProperty); }
            set { SetValue(serverProperty, value); }
        }
        public static readonly DependencyProperty serverProperty =
            DependencyProperty.Register("server", typeof(string), typeof(ServerStatus), new UIPropertyMetadata("localhost"));
        public bool ok
        {
            get { return (bool)GetValue(okProperty); }
            set { SetValue(okProperty, value); }
        }
        public static readonly DependencyProperty okProperty =
            DependencyProperty.Register("ok", typeof(bool), typeof(ServerStatus), new UIPropertyMetadata(false));
    }
    public class ServerStatusAsBackgroundColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                var brush = new RadialGradientBrush { Center = new Point(0.5, 0.5), RadiusX = 0.5, RadiusY = 0.5 };
                brush.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0 });
                brush.GradientStops.Add(new GradientStop { Color = Colors.DarkGray, Offset = 0.8 });
                brush.GradientStops.Add(new GradientStop { Color = Colors.Black, Offset = 1 });
                return brush;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Fraction : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) / Double.Parse((string)parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class HalfValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
                              Type targetType,
                              object parameter,
                              CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                throw new ArgumentException(
                    "HalfValueConverter expects 2 double values to be passed" +
                    " in this order -> totalWidth, width",
                    "values");
            }

            double totalWidth = (double)values[0];
            double width = (double)values[1];
            return (object)((totalWidth - width) / 2);
        }
        public object[] ConvertBack(object value,
                                    Type[] targetTypes,
                                    object parameter,
                                    CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
