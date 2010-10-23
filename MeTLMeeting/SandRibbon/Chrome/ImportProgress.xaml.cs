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
using SandRibbon.Utils;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon
{
    public partial class ProgressDialog : UserControl
    {
        public static SubtractionConverter subtract = new SubtractionConverter();
        public static SlideDisplacementConverter SlideDisplacement = new SlideDisplacementConverter();
        public ObservableCollection<PowerpointImportProgress> fromStack = new ObservableCollection<PowerpointImportProgress>();
        public ObservableCollection<PowerpointImportProgress> toStack = new ObservableCollection<PowerpointImportProgress>();

        public ProgressDialog()
        {
            InitializeComponent();
            from.ItemsSource = fromStack;
            to.ItemsSource = toStack;
            Commands.UpdatePowerpointProgress.RegisterCommandToDispatcher<PowerpointImportProgress>(new DelegateCommand<PowerpointImportProgress>(UpdatePowerpointProgress));
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(JoinConversation));
            Commands.MoveTo.RegisterCommandToDispatcher(new DelegateCommand<object>(MoveTo));
            Commands.PreParserAvailable.RegisterCommandToDispatcher(new DelegateCommand<object>(PreParserAvailable));
            Commands.CreateBlankConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(JoinConversation));
        }
        private void reset()
        {
            fromStack.Clear();
            toStack.Clear();
        }
        private void JoinConversation(object _arg) {
            reset();
            Visibility = Visibility.Visible;
            goldLabel.Content = "Joining";
        }
        private void MoveTo(object _arg) {
            Visibility = Visibility.Visible;
            goldLabel.Content = "Moving";
        }
        private void PreParserAvailable(object _arg) {
            Commands.RequerySuggested();
            Visibility = Visibility.Collapsed;
        }
        private void UpdatePowerpointProgress(PowerpointImportProgress progress) {
            switch (progress.stage) { 
                case PowerpointImportProgress.IMPORT_STAGE.DESCRIBED:
                    reset();
                    Visibility = Visibility.Visible;
                    goldLabel.Content = "Importing";
                    break;
                case PowerpointImportProgress.IMPORT_STAGE.ANALYSED:
                   fromStack.Insert(0,progress);
                    break;
            }
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
