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
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;

namespace PowerpointProgressDialog
{
    public partial class ImportProgress : UserControl
    {
        public static SubtractionConverter subtract = new SubtractionConverter();
        public static SlideDisplacementConverter SlideDisplacement = new SlideDisplacementConverter();
        public ObservableCollection<SlideProgress> fromStack = new ObservableCollection<SlideProgress>();
        public ObservableCollection<SlideProgress> toStack = new ObservableCollection<SlideProgress>();
        public ImportProgress()
        {
            InitializeComponent();
            from.ItemsSource = fromStack;
            to.ItemsSource = toStack;
            setItemsToTransfer(Enumerable.Range(1, 5).Select(i => new SlideProgress { index = i }));
        }
        public void setItemsToTransfer(IEnumerable<SlideProgress> items) {
            Dispatcher.Invoke((Action)delegate
            {
                fromStack.Clear();
                toStack.Clear();
                foreach (var item in items)
                    fromStack.Add(item);
            });
        }
        public void setItemInProgress(PowerpointProgressEventArgs progressReport)
        {
            Dispatcher.Invoke((Action)delegate{
                ((Storyboard)FindResource("transition")).Begin();
                var inProgress = fromStack.Last();
                fromStack.Remove(inProgress);
                toStack.Add(inProgress);
                transit.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(progressReport.filename);
            });
        }
        public void finished() {
            Dispatcher.Invoke((Action)delegate
            {
                Visibility = Visibility.Collapsed;
            });
        }
    }
    public class SlideProgress {
        public int index { get; set; }
    }
    public class SubtractionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value - (double)parameter;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class SlideDisplacementConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value * 30;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
