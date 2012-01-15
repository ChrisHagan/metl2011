using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MeTLLib;
using MeTLLib.DataTypes;
using SandRibbon.Providers;
using SandRibbon.Components.Utility;

namespace SandRibbon.Quizzing
{
    public partial class EditQuiz : Window
    {
        #region DependencyProperties
        public static readonly DependencyProperty EditedQuizProperty = DependencyProperty.Register("EditedQuiz", typeof (QuizQuestion), typeof(EditQuiz));
        public QuizQuestion EditedQuiz
        {
            get { return (QuizQuestion)GetValue(EditedQuizProperty); }
            set { SetValue(EditedQuizProperty, value); }
        }
        public static readonly DependencyProperty OptionErrorProperty = DependencyProperty.Register("OptionError", typeof (bool), typeof (EditQuiz));
        public bool OptionError { 
            get { return (bool) GetValue(OptionErrorProperty); }
            set{SetValue(OptionErrorProperty, value);}
        }
        public static readonly DependencyProperty QuestionErrorProperty = DependencyProperty.Register("QuestionError", typeof (bool), typeof (EditQuiz));
        public bool QuestionError { 
            get { return (bool)GetValue(QuestionErrorProperty); }
            set{SetValue(QuestionErrorProperty, value);}
        }
        public static readonly DependencyProperty ResultsExistProperty = DependencyProperty.Register("ResultsExist", typeof(bool), typeof(EditQuiz));
        public bool ResultsExist 
        {
            get
            {
                return (bool)GetValue(ResultsExistProperty);
            }
            set
            {
                SetValue(ResultsExistProperty, value);
            }
        }
        #endregion

        public ObservableWithPropertiesCollection<Option> Options { get; private set; }

        public EditQuiz(QuizQuestion quiz)
        {
            EditedQuiz = quiz.DeepCopy();
            InitializeComponent();
            DataContext = this;

            Options = new ObservableWithPropertiesCollection<Option>(EditedQuiz.options);

            QuestionError = false;
            OptionError = false;
            ensureQuizHasAnEmptyOption();
            ResultsExist = CheckResultsExist(quiz);
        }

        public void ensureQuizHasAnEmptyOption()
        {
            var emptyOptions = Options.Where(o => String.IsNullOrEmpty(o.optionText)).ToList();
            if (emptyOptions.Count() == 0)
            {
                var lastOption = Options.Last();
                var newName = Option.GetNextOptionName(lastOption.name);
                var newIndex = Options.IndexOf(lastOption) + 1;
                Options.Add(new Option(newName, String.Empty, false, generateColor(newIndex)));
            }
            else
            {
                foreach (var o in emptyOptions.Skip(1))
                    Options.Remove(o);
            }
        }
        private Color generateColor(int index)
        {
            return index%2 == 0 ? Colors.White : (Color) ColorConverter.ConvertFromString("#FF4682B4");
        }
        private void updateOptionText(object sender, TextChangedEventArgs e)
        {

            /*
            var text = ((TextBox)sender).Text;
            var option = (Option)((FrameworkElement)sender).DataContext;
            if (!String.IsNullOrEmpty(text) || option.optionText != text)
            {
                option.optionText = text;
                AddNewEmptyOption();
            }
            ((TextBox) sender).Focus();
            */
        }

        public bool CheckResultsExist(QuizQuestion quizQuestion)
        {
            return Globals.quiz.answers.FirstOrDefault(answer => answer.Key == quizQuestion.id).Value.Count > 0;
        }

        private void deleteQuiz(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);
            if (MeTLMessage.Question("Really delete quiz?", owner) == MessageBoxResult.Yes)
            {
                EditedQuiz.SetDeleted(true);
                Commands.SendQuiz.Execute(EditedQuiz);
                this.Close();
            }    
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ensureQuizHasAnEmptyOption();
        }

        private bool shouldAddNewEmptyOption()
        {
            var emptyOptions = Options.Where(o => string.IsNullOrEmpty(o.optionText));
            if (emptyOptions.Count() == 0) return true;
            return false;
        }

        private void AddNewEmptyOption()
        {
            if (!shouldAddNewEmptyOption()) return;
            foreach (var option in Options)
            {
                var container = ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(option)); 
                if (container != null) container.Opacity = 1;
            }

            var newIndex =  1;
            var newName = Option.GetOptionNameFromIndex(0);
            if (Options.Count > 0)
            {
                var lastOption = Options.Last();
                newIndex = Options.IndexOf(lastOption) + 1; 
                newName = Option.GetNextOptionName(lastOption.name);
            }
            var newOption = new Option(newName, String.Empty, false, generateColor(newIndex));
            if (shouldAddNewEmptyOption())
            {
                Options.Add(newOption);
                var container = ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(newOption));
                if (container != null) container.Opacity = 0.5;
            }
            Commands.RequerySuggested();
        }

        private void RemoveQuizAnswer(object sender, RoutedEventArgs e)
        {
            var owner = ((FrameworkElement)sender).DataContext;
            Options.Remove((Option)owner);

            // relabel the option names
            for (int i = 0; i < Options.Count; i++)
            {
                Options[i].name = Option.GetOptionNameFromIndex(i);
                Options[i].color = generateColor(i);
            }

            AddNewEmptyOption();
            foreach (var obj in Options)
                if (String.IsNullOrEmpty(obj.optionText))
                    ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(obj)).Opacity = 0.5;
            CommandManager.InvalidateRequerySuggested();
        }
        
        private void quizCommitButton_Click(object sender, RoutedEventArgs e)
        {
            EditedQuiz.options.Clear();
            EditedQuiz.options = null;

            EditedQuiz.options = Options.Where(o => !string.IsNullOrEmpty(o.optionText)).ToList();
            EditedQuiz.created = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            if (validateQuiz(EditedQuiz))
            {
                Commands.SendQuiz.Execute(EditedQuiz);
                this.Close();
            }
        }

        private bool validateQuiz(QuizQuestion editedQuiz)
        {
            if (string.IsNullOrEmpty(editedQuiz.question))
                QuestionError = true;
            if (editedQuiz.options.Count < 2)
                OptionError = true;
            return !(OptionError && QuestionError);
        }

        private void CloseEdit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void quizAnswer_GotFocus(object sender, RoutedEventArgs e)
        {
            var optionContainer = GetQuestionContainerFromItem(sender);
            if (optionContainer != null) 
                optionContainer.Opacity = 1;
        }

        private void quizAnswer_Loaded(object sender, RoutedEventArgs e)
        {
            var optionContainer = GetQuestionContainerFromItem(sender);
            if (optionContainer != null)
            {
                var option = (optionContainer.DataContext as Option);
                if (String.IsNullOrEmpty(option.optionText) && Options.IndexOf(option) > 0)
                    optionContainer.Opacity = 0.5;
            }
        }
        private FrameworkElement GetQuestionContainerFromItem(object itemContainer)
        {
            if (itemContainer is FrameworkElement)
            {
                var currentOption = (itemContainer as FrameworkElement).DataContext;    
                return quizQuestions.ItemContainerGenerator.ContainerFromItem(currentOption) as FrameworkElement; 
            }

            return null;
        }
    }
}
