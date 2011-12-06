using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MeTLLib.DataTypes;
using SandRibbon.Providers;

namespace SandRibbon.Quizzing
{
    public partial class EditQuiz : Window
    {
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
        public static readonly DependencyProperty TitleErrorProperty = DependencyProperty.Register("TitleError", typeof (bool), typeof (EditQuiz));
        public bool TitleError { 
            get { return (bool) GetValue(TitleErrorProperty); }
            set{SetValue(TitleErrorProperty, value);}
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

        public ObservableCollection<Option> Options { get; set; }
        public EditQuiz(QuizQuestion quiz)
        {
            EditedQuiz = quiz;
            InitializeComponent();
            DataContext = this;
            Options = new ObservableCollection<Option>(EditedQuiz.options);
            TitleError = false;
            OptionError = false;
            ensureQuizHasAnEmptyOption();
            ResultsExist = CheckResultsExist(quiz);
        }
        private const int alphabetLength = 26;
        public void ensureQuizHasAnEmptyOption()
        {
            var emptyOptions = Options.Where(o => String.IsNullOrEmpty(o.optionText)).ToList();
            if (emptyOptions.Count() == 0)
            {
                var newName = getOptionName(Options.ToList());
                var newIndex = AllColors.all.IndexOf(Options.Last().color) + 1;
                Options.Add(new Option(newName, String.Empty, false, AllColors.all.ElementAt(newIndex)));
            }
            else
            {
                foreach (var o in emptyOptions.Skip(1))
                    Options.Remove(o);
                    
            }
        }

        public bool CheckResultsExist(QuizQuestion quizQuestion)
        {
            return Globals.quiz.answers.Where(answer => answer.Key == quizQuestion.id).FirstOrDefault().Value.Count > 0;
        }

        private string getOptionName(List<Option> options)
        {
            var newName = "A";
            if (options.Count > 0)
            {
                var temp = new String(new[] {(char) (options.Last().name.ToCharArray()[options.Last().name.Length - 1] + 1)}).ToUpper();
                if (temp.ToCharArray()[0] <= 90)
                    newName = temp;

                if (options.Count >= alphabetLength)
                {
                    var prefix = new string(new char[] {(char) ("A".ToCharArray()[0] + ((options.Count/alphabetLength) - 1))}).ToUpper();
                    newName = string.Format("{0}{1}", prefix, newName);
                }
            }
            return newName;
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ensureQuizHasAnEmptyOption();
        }

        private void quizCommitButton_Click(object sender, RoutedEventArgs e)
        {
            EditedQuiz.options.Clear();
            foreach(var option in Options.Where(o => !string.IsNullOrEmpty(o.optionText)))
            {
                option.name = getOptionName(EditedQuiz.options.ToList());
                int newIndex;
                if (EditedQuiz.options.Count == 0)
                    newIndex = 1;
                else
                    newIndex = AllColors.all.IndexOf(EditedQuiz.options.Last().color) + 1;
                option.color = AllColors.all.ElementAt(newIndex);
                EditedQuiz.options.Add(option);
            }
            EditedQuiz.created = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            if (validateQuiz(EditedQuiz))
            {
                Commands.SendQuiz.Execute(EditedQuiz);
                this.Close();
            }
        }

        private bool validateQuiz(QuizQuestion editedQuiz)
        {
            if (string.IsNullOrEmpty(editedQuiz.title))
                TitleError = true;
            if (editedQuiz.options.Count < 2)
                OptionError = true;
            return !(OptionError && TitleError);
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
