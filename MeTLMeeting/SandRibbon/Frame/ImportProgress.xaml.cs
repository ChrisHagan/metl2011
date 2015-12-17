using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using SandRibbon.Utils;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon
{
    public partial class ProgressDialog : UserControl
    {
        public static SubtractionConverter subtract = new SubtractionConverter();
        public static SlideDisplacementConverter SlideDisplacement = new SlideDisplacementConverter();
        public ObservableCollection<PowerpointImportProgress> fromStack = new ObservableCollection<PowerpointImportProgress>();

        public ProgressDialog()
        {
            InitializeComponent();
            from.ItemsSource = fromStack;
            Commands.UpdatePowerpointProgress.RegisterCommand(new DelegateCommand<PowerpointImportProgress>(UpdatePowerpointProgress));
            Commands.JoiningConversation.RegisterCommand(new DelegateCommand<object>(JoinConversation));
            Commands.PrintConversation.RegisterCommand(new DelegateCommand<object>(PrintConversation));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<object>(PreParserAvailable));
            Commands.CreateBlankConversation.RegisterCommand(new DelegateCommand<object>(JoinConversation));
            Commands.HideProgressBlocker.RegisterCommand(new DelegateCommand<object>(HideProgressBlocker));
        }
        private void HideProgressBlocker(object _arg) {
            Dispatcher.adopt(delegate
            {

                Visibility = Visibility.Collapsed;
                reset();
            });
        }
        private void setProgress(double percentage) {
            if(Visibility == Visibility.Collapsed)
                Visibility = Visibility.Visible;
            progress.Value = percentage;
            progress.IsIndeterminate = false;
        }
        private void setContent(string content) {
            goldLabel.Content = content; 
        }
        private void reset()
        {
            slidesAnalyzed = 0;
            slidesExtracted = 0;
            progress.Value = 0;
            progress.IsIndeterminate = false;
            Dispatcher.adopt(delegate
            {
                fromStack.Clear();
            });
        }
        private void PrintConversation(object _arg) {
            Dispatcher.adopt(delegate
            {

                reset();
                Visibility = Visibility.Visible;
                setContent("Printing");
            });
        }
        private void JoinConversation(object _arg) {
            Dispatcher.adopt(delegate
            {

                reset();
                Visibility = Visibility.Visible;
                setContent("Joining");
                setProgress(100);
            });
        }
        private void PreParserAvailable(object _arg) {
            Dispatcher.adopt(delegate
            {

                Commands.RequerySuggested();
                Visibility = Visibility.Collapsed;
            });
        }
        private int slidesExtracted;
        private int slidesAnalyzed;
        private void UpdatePowerpointProgress(PowerpointImportProgress progress) {
            Dispatcher.adopt(delegate
            {

                switch (progress.stage)
                {
                    case PowerpointImportProgress.IMPORT_STAGE.DESCRIBED:
                        reset();
                        Visibility = Visibility.Visible;
                        setContent("Importing");
                        this.progress.IsIndeterminate = true;
                        break;
                    case PowerpointImportProgress.IMPORT_STAGE.EXTRACTED_IMAGES:
                        slidesExtracted++;
                        setContent("Processing");
                        setProgress((slidesAnalyzed + slidesExtracted) / Convert.ToDouble(progress.totalSlides * 2) * 100);
                        break;
                    case PowerpointImportProgress.IMPORT_STAGE.ANALYSED:
                        slidesAnalyzed++;
                        setContent("Loading");
                        setProgress((slidesAnalyzed + slidesExtracted) / Convert.ToDouble(progress.totalSlides * 2) * 100);
                        break;
                    case PowerpointImportProgress.IMPORT_STAGE.PRINTING:
                        slidesAnalyzed++;
                        setContent("Printing");
                        setProgress((slidesAnalyzed + slidesExtracted) / Convert.ToDouble(progress.totalSlides * 2) * 100);
                        break;
                }
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
