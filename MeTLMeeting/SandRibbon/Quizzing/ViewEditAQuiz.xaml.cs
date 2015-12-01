using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Providers;
using System.Windows.Input;
using System.Windows.Data;
using SandRibbon.Components.Utility;
using System.ComponentModel;
using SandRibbon.Pages;

namespace SandRibbon.Quizzing
{
    [ValueConversion(typeof(string), typeof(bool))]
    public class StringNullOrEmptyToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var valueString = value as string;
            return String.IsNullOrEmpty(valueString) || valueString == "none";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;  
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return String.IsNullOrEmpty(value as String) ? Visibility.Hidden : Visibility.Visible; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null; 
        }
    }

    public class IndexOfItemToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var collection = values[0] as ObservableCollection<Option>;
            var item = values[1] as Option;
            return collection != null && collection.IndexOf(item) > 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(); 
        }
    }

    public partial class ViewEditAQuiz : Window
    {
        #region DependencyProperties

        public static readonly DependencyProperty OptionErrorProperty = DependencyProperty.Register("OptionError", typeof (bool), typeof (ViewEditAQuiz));

        public bool OptionError
        {
            get { return (bool) GetValue(OptionErrorProperty); }
            set { SetValue(OptionErrorProperty, value); }
        }

        public static readonly DependencyProperty QuestionErrorProperty = DependencyProperty.Register("QuestionError", typeof (bool), typeof (ViewEditAQuiz));

        public bool QuestionError
        {
            get { return (bool) GetValue(QuestionErrorProperty); }
            set { SetValue(QuestionErrorProperty, value); }
        }

        public static readonly DependencyProperty ResultsExistProperty = DependencyProperty.Register("ResultsExist", typeof (bool), typeof (ViewEditAQuiz));

        public bool ResultsExist
        {
            get { return (bool) GetValue(ResultsExistProperty); }
            set { SetValue(ResultsExistProperty, value); }
        }

        #endregion

        private QuizQuestion question
        {
            get { return (QuizQuestion)DataContext; }
        }
        public ConversationState convState { get; protected set; }
        public Slide slide { get; protected set; }
        public string me { get; protected set; }
        public ViewEditAQuiz(QuizQuestion thisQuiz, ConversationState _convState, Slide _slide, string _me)
        {
            me = _me;
            slide = _slide;
            convState = _convState;
            DataContext = thisQuiz;

            InitializeComponent();
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>((_unused) => { Close(); }));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>((_unused) => { Close(); }));

            question.Options.CollectionChanged += UpdateOptionError;
            //QuestionError = false;
            ResultsExist = CheckResultsExist(question);
        }

        private void UpdateOptionError(object sender, NotifyCollectionChangedEventArgs e)
        {
            var optionList = sender as ObservableCollection<Option>;
            OptionError = (e.Action == NotifyCollectionChangedAction.Remove && optionList.Count < 3 );
        }

        public bool CheckResultsExist(QuizQuestion quizQuestion)
        {
            return convState.quizData.answers.FirstOrDefault(answer => answer.Key == quizQuestion.Id).Value.Count > 0;
        }

        void ViewEditAQuiz_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseEdit(null, null);
            Commands.UnblockInput.Execute(null);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!question.IsInEditMode)
            {
                var selection = ((Option)e.AddedItems[0]);
                Commands.SendQuizAnswer.ExecuteAsync(new QuizAnswer(question.Id, me, selection.name, DateTime.Now.Ticks));
                Trace.TraceInformation("ChoseQuizAnswer {0} {1}", selection.name, question.Id);
                Close();
            }
       }

        public void DisplayQuiz(object sender, RoutedEventArgs e)
        {
            var quizDisplay = new DisplayAQuiz(question);
            quizDisplay.Show();

            try
            {
                var quiz = quizDisplay.SnapshotHost;
                var dpi = 96;
                var dimensions = ResizeHelper.ScaleMajorAxisToCanvasSize(quiz);
                var bitmap = new RenderTargetBitmap((int)dimensions.Width, (int)dimensions.Height, dpi, dpi, PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var context = dv.RenderOpen())
                    context.DrawRectangle(new VisualBrush(quiz), null, dimensions);
                bitmap.Render(dv);
                Commands.QuizResultsAvailableForSnapshot.ExecuteAsync(new UnscaledThumbnailData{id=slide.id,data=bitmap});
            }
            finally
            {
                quizDisplay.Close();
                Close();
            }
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            AddNewEmptyOption();
            question.BeginEdit();
        }

        private void QuizButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                tryPrefillOption((FrameworkElement)sender);
            }
        }

        private void tryPrefillOption(FrameworkElement sender)
        {
            var currentOption = sender.DataContext;
            if (currentOption is Option)
            {
                var co = ((Option)currentOption);
                if (string.IsNullOrEmpty(co.optionText))
                {
                    co.optionText = String.Empty;
                }
                AddNewEmptyOption();
            }
        }

        private void deleteQuiz(object sender, RoutedEventArgs e)
        {
            var windowOwner = Window.GetWindow(this);
            if (MeTLMessage.Question("Really delete quiz?", windowOwner) == MessageBoxResult.Yes)
            {
                question.Delete();
                Commands.SendQuiz.Execute(question);
                Close();
            }    
        }

        private void quizCommitButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidQuiz(question))
            {
                question.RemoveEmptyOptions();
                question.Created = SandRibbonObjects.DateTimeFactory.Now().Ticks;
                Commands.SendQuiz.Execute(question);
                question.EndEdit();
            }
        }

        private bool ValidQuiz(QuizQuestion editedQuiz)
        {
            var questionError = false;
            var optionError = false;
            var questionValid = question.Validate(out questionError, out optionError);
            QuestionError = questionError;
            OptionError = optionError;

            return questionValid;
        }

        private void CloseEdit(object sender, RoutedEventArgs e)
        {
            question.CancelEdit();
            question.RemoveEmptyOptions();
        }

        private void updateOptionText(object sender, TextChangedEventArgs e)
        {
            var text = ((TextBox)sender).Text;
            var option = (Option)((FrameworkElement)sender).DataContext;
            if (!String.IsNullOrEmpty(text) || option.optionText != text)
            {
                option.optionText = text;
                AddNewEmptyOption();
            }
        }

        private void RemoveQuizAnswer(object sender, RoutedEventArgs e)
        {
            var owner = ((FrameworkElement)sender).DataContext;
            question.Options.Remove((Option)owner);

            // relabel the option names
            for (int i = 0; i < question.Options.Count; i++)
            {
                //options[i].name = Option.GetOptionNameFromIndex(i);
                question.Options[i].color = generateColor(i);
            }

            AddNewEmptyOption();
            CommandManager.InvalidateRequerySuggested();
        }

        private bool shouldAddNewEmptyOption()
        {
            var emptyOptions = question.Options.Where(o => string.IsNullOrEmpty(o.optionText));
            return emptyOptions.Count() == 0; 
        }

        private Color generateColor(int index)
        {
            return index%2 == 0 ? Colors.White : (Color) ColorConverter.ConvertFromString("#FF4682B4");
        }

        private void AddNewEmptyOption()
        {
            if (!shouldAddNewEmptyOption()) return;

            var newIndex =  1;
            var newName = Option.GetOptionNameFromIndex(0);
            if (question.Options.Count > 0)
            {
                var lastOption = question.Options.Last();
                newIndex = question.Options.IndexOf(lastOption) + 1; 
                newName = Option.GetNextOptionName(lastOption.name);
            }
            var newOption = new Option(newName, String.Empty, false, generateColor(newIndex));
            if (shouldAddNewEmptyOption())
            {
                question.Options.Add(newOption);
            }
            Commands.RequerySuggested();
        }
    }
}
