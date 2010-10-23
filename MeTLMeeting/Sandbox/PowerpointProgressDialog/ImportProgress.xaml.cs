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
    public partial class ProgressDialog : UserControl
    {
        public static SubtractionConverter subtract = new SubtractionConverter();
        public static SlideDisplacementConverter SlideDisplacement = new SlideDisplacementConverter();
        public ObservableCollection<SlideProgress> fromStack = new ObservableCollection<SlideProgress>();
        public ObservableCollection<SlideProgress> toStack = new ObservableCollection<SlideProgress>();
        private Storyboard animation;
        public string Label
        {
            get { return (string)GetValue(labelProperty); }
            set { SetValue(labelProperty, value); }
        }
        public static readonly DependencyProperty labelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(ProgressDialog), new UIPropertyMetadata("Please Wait"));

        public ProgressDialog()
        {
            InitializeComponent();
            animation = (Storyboard) FindResource("transition");
            from.ItemsSource = fromStack;
            to.ItemsSource = toStack;
            goldLabel.DataContext = Label;
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
                var inProgress = fromStack.Last();
                transit.Visibility = Visibility.Visible;
                animation.Begin();
                fromStack.Remove(inProgress);
                toStack.Insert(0, inProgress);
                transit.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(progressReport.progress.uri);
            });
        }
        public void finished() {
            Dispatcher.Invoke((Action)delegate
            {
                transit.Visibility = Visibility.Collapsed ;
                animation.Stop();
            });
        }
    }
    public class SlideProgress {
        public string uri { get; set; }
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
