using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SandRibbon.Quizzing
{
    /*
     * Rename Submit to Create - done
     * Sexy up the buttons
     * Make Accept and Cancel work - done
     * Snapshot the ink canvas if there is content and send the right url
     * Display the snapshotted thumbnails
     * Retrieve history
     * 
     * Students can only submit once ever in each quiz
     * Submission is a two phase submit - you select the option, optionally add ink and then submit
     * Submitting returns you to the parent slide - done
     * Submitting publishes a 'Stuart has answered' to the parent room
     * Cancel returns you to the parent slide - done
     * Conversation level quiz listing to be reified
     * Conversation level quiz listing has to retrieve each quiz history to determine whether it's been answered
     * Visual representation of whether we have already answered
     */
    public partial class QuizControls : UserControl
    {
        public static RoutedCommand ProxyCreateQuiz = new RoutedCommand();
        public static RoutedCommand ProxyConvertPresentationSpaceToQuiz = new RoutedCommand();
        public static QuizButtonLister ListHydrator = new QuizButtonLister();
        public static OptionToColor OptionToColor = new OptionToColor();
        public static UnindexOptionIndex UnindexOptionIndex = new UnindexOptionIndex();
        public QuizControls()
        {
            InitializeComponent(); 
            CommandBindings.Add(new CommandBinding(ProxyCreateQuiz, CreateQuiz, CanCreateQuiz));
            CommandBindings.Add(new CommandBinding(ProxyConvertPresentationSpaceToQuiz, ConvertPresentationSpaceToQuiz, CanCreateQuiz));
        }
        private void CreateQuiz(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.CreateQuiz.Execute(numberOfButtons.SelectedItem);
        }
        private void ConvertPresentationSpaceToQuiz(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.ConvertPresentationSpaceToQuiz.Execute(numberOfButtons.SelectedItem);
        }
        private void CanCreateQuiz(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = numberOfButtons.SelectedItem != null && Commands.CreateQuiz.CanExecute(numberOfButtons.SelectedItem);
        }
    }
    public class QuizButtonLister : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var buttonCount = (int)value;
            return Enumerable.Range(0, buttonCount).Select(i => QuizControls.UnindexOptionIndex.Convert(i,null,null,null)).ToList();
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class OptionToColor : IValueConverter
    {
        private static Brush ERROR_BRUSH = Brushes.Black;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int index = 0;
                if (value is Option)
                    index = ((Option)value).index;
                else if (value is Int32)
                    index = (int)value;
                else
                    index = Int32.Parse(value.ToString());
                if (index < 0 || index > 4)
                    return ERROR_BRUSH;
                else
                    return new[] { Brushes.Red, Brushes.Blue, Brushes.Yellow, Brushes.Green, Brushes.Purple }[((int)index) - 1];
            }
            catch (Exception e)
            {
                return ERROR_BRUSH;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class UnindexOptionIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((int)value) + 1;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
