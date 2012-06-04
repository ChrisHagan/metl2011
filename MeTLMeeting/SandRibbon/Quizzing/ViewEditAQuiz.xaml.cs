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

namespace SandRibbon.Quizzing
{
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
        private Thickness quizOptionsBorderThickness;

        #region DependencyProperties

        public static readonly DependencyProperty EditedQuizProperty = DependencyProperty.Register("EditedQuiz",
                                                                                                   typeof (QuizQuestion),
                                                                                                   typeof (ViewEditAQuiz));

        public static readonly DependencyProperty OptionErrorProperty = DependencyProperty.Register("OptionError",
                                                                                                    typeof (bool),
                                                                                                    typeof (ViewEditAQuiz));

        public bool OptionError
        {
            get { return (bool) GetValue(OptionErrorProperty); }
            set { SetValue(OptionErrorProperty, value); }
        }

        public static readonly DependencyProperty QuestionErrorProperty = DependencyProperty.Register("QuestionError",
                                                                                                      typeof (bool),
                                                                                                      typeof (ViewEditAQuiz));

        public bool QuestionError
        {
            get { return (bool) GetValue(QuestionErrorProperty); }
            set { SetValue(QuestionErrorProperty, value); }
        }

        public static readonly DependencyProperty ResultsExistProperty = DependencyProperty.Register("ResultsExist",
                                                                                                     typeof (bool),
                                                                                                     typeof (ViewEditAQuiz));

        public bool ResultsExist
        {
            get { return (bool) GetValue(ResultsExistProperty); }
            set { SetValue(ResultsExistProperty, value); }
        }

        #endregion

        public static TitleConverter TitleConverter = new TitleConverter();
        public static QuestionConverter QuestionConverter = new QuestionConverter();
        private MeTLLib.DataTypes.QuizQuestion question
        {
            get { return (MeTLLib.DataTypes.QuizQuestion)DataContext; }
        }

        public ViewEditAQuiz()
        {
            InitializeComponent();
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
            Closing += AnswerAQuiz_Closing;

            question.Options.CollectionChanged += UpdateOptionError;
            QuestionError = false;
            ResultsExist = CheckResultsExist(question);
            AddNewEmptyOption();
        }

        public ViewEditAQuiz(MeTLLib.DataTypes.QuizQuestion thisQuiz)
        {
            DataContext = thisQuiz;
            InitializeComponent();
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
            Closing += AnswerAQuiz_Closing;
        }

        private void UpdateOptionError(object sender, NotifyCollectionChangedEventArgs e)
        {
            var optionList = sender as ObservableCollection<Option>;
            if (e.Action == NotifyCollectionChangedAction.Remove && optionList.Count == 2 )
                OptionError = true;
            else
                OptionError = false;
        }

        public bool CheckResultsExist(QuizQuestion quizQuestion)
        {
            return Globals.quiz.answers.FirstOrDefault(answer => answer.Key == quizQuestion.Id).Value.Count > 0;
        }

        void AnswerAQuiz_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Commands.UnblockInput.Execute(null);
        }

        private void closeMe(object obj)
        {
            Close();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = ((Option)e.AddedItems[0]);
            Commands.SendQuizAnswer.ExecuteAsync(new QuizAnswer(question.Id, Globals.me, selection.name, DateTime.Now.Ticks));
            Trace.TraceInformation("ChoseQuizAnswer {0} {1}", selection.name, question.Id);
            
            //this.Close();
       }

        public void DisplayQuiz(object sender, RoutedEventArgs e)
        {
            //PrepareForRender();
            var quiz = SnapshotHost;
            quiz.UpdateLayout();
            var dpi = 96;
            var dimensions = ResizeHelper.ScaleMajorAxisToCanvasSize(quiz);
            var bitmap = new RenderTargetBitmap((int)dimensions.Width, (int)dimensions.Height, dpi, dpi, PixelFormats.Default);
            var dv = new DrawingVisual();
            using (var context = dv.RenderOpen())
                context.DrawRectangle(new VisualBrush(quiz), null, dimensions);
            bitmap.Render(dv);
            Commands.QuizResultsAvailableForSnapshot.ExecuteAsync(new UnscaledThumbnailData{id=Globals.slide,data=bitmap});

            //RestoreAfterRender();
            this.Close();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            question.IsInEditMode = true;
            /*var windowOwner = this; 
            var newQuiz = new QuizQuestion(question.Id, question.Created, question.Title, question.Author,
                                           question.Question, question.Options) {Url = question.Url};
            var editQuiz = new EditQuiz(newQuiz);
            editQuiz.Owner = windowOwner;

            bool? deleteQuiz = editQuiz.ShowDialog();
            if (deleteQuiz ?? false)
            {
                if (MeTLMessage.Question("Really delete quiz?", windowOwner) == MessageBoxResult.Yes)
                {
                    newQuiz.IsDeleted = true;
                    Commands.SendQuiz.Execute(newQuiz);
                    closeMe(null);
                }    
            }*/
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
            DialogResult = true;
            Close();
        }

        private void quizCommitButton_Click(object sender, RoutedEventArgs e)
        {
        }

      private void CloseEdit(object sender, RoutedEventArgs e)
      {
          question.IsInEditMode = false;
      }

        /*#region Helpers
        private FrameworkElement GetQuestionContainerFromItem(object itemContainer)
        {
            if (itemContainer is FrameworkElement)
            {
                var currentOption = (itemContainer as FrameworkElement).DataContext;    
                return quizQuestions.ItemContainerGenerator.ContainerFromItem(currentOption) as FrameworkElement; 
            }

            return null;
        }
        #endregion
        */

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            /*var optionContainer = GetQuestionContainerFromItem(sender);
            if (optionContainer != null) 
                optionContainer.Opacity = 1;*/
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            /*var optionContainer = GetQuestionContainerFromItem(sender);
            if (optionContainer != null)
            {
                var option = (optionContainer.DataContext as Option);
                if (String.IsNullOrEmpty(option.optionText) && question.Options.IndexOf(option) > 0)
                    optionContainer.Opacity = 0.5;
            }*/
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
            /*foreach (var obj in question.Options)
                if (String.IsNullOrEmpty(obj.optionText))
                    ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(obj)).Opacity = 0.5;*/
            CommandManager.InvalidateRequerySuggested();
        }

        private bool shouldAddNewEmptyOption()
        {
            var emptyOptions = question.Options.Where(o => string.IsNullOrEmpty(o.optionText));
            if (emptyOptions.Count() == 0) return true;
            return false;
        }
        private Color generateColor(int index)
        {
            return index%2 == 0 ? Colors.White : (Color) ColorConverter.ConvertFromString("#FF4682B4");
        }

        private void AddNewEmptyOption()
        {
            if (!shouldAddNewEmptyOption()) return;
            /*foreach (var option in question.Options)
            {
                var container = ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(option)); 
                if (container != null) container.Opacity = 1;
            }*/

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
                /*var container = ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(newOption));
                if (container != null) container.Opacity = 0.5;*/
            }
            Commands.RequerySuggested();
        }
    }
}
