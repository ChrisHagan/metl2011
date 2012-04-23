using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MeTLLib;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using System.Collections.Specialized;

namespace SandRibbon.Quizzing
{
    public partial class EditQuiz : Window
    {
        #region DependencyProperties

        public static readonly DependencyProperty EditedQuizProperty = DependencyProperty.Register("EditedQuiz",
                                                                                                   typeof (QuizQuestion),
                                                                                                   typeof (EditQuiz));

        public QuizQuestion EditedQuiz
        {
            get { return (QuizQuestion) GetValue(EditedQuizProperty); }
            set { SetValue(EditedQuizProperty, value); }
        }

        public static readonly DependencyProperty OptionErrorProperty = DependencyProperty.Register("OptionError",
                                                                                                    typeof (bool),
                                                                                                    typeof (EditQuiz));

        public bool OptionError
        {
            get { return (bool) GetValue(OptionErrorProperty); }
            set { SetValue(OptionErrorProperty, value); }
        }

        public static readonly DependencyProperty QuestionErrorProperty = DependencyProperty.Register("QuestionError",
                                                                                                      typeof (bool),
                                                                                                      typeof (EditQuiz));

        public bool QuestionError
        {
            get { return (bool) GetValue(QuestionErrorProperty); }
            set { SetValue(QuestionErrorProperty, value); }
        }

        public static readonly DependencyProperty ResultsExistProperty = DependencyProperty.Register("ResultsExist",
                                                                                                     typeof (bool),
                                                                                                     typeof (EditQuiz));

        public bool ResultsExist
        {
            get { return (bool) GetValue(ResultsExistProperty); }
            set { SetValue(ResultsExistProperty, value); }
        }

        #endregion

        public static ObservableCollection<Option> options = new ObservableCollection<Option>
                                                     {
                                                     };

        public EditQuiz(QuizQuestion quiz)
        {
            options.CollectionChanged += UpdateOptionError;
            EditedQuiz = quiz.DeepCopy();
            loadOptions();
            InitializeComponent();
            DataContext = this;
            QuestionError = false;
            ResultsExist = CheckResultsExist(quiz);
            AddNewEmptyOption();
        }

        private void loadOptions()
        {
            options.Clear();
            foreach(var option in EditedQuiz.options)
                options.Add(option);
        }

        private void UpdateOptionError(object sender, NotifyCollectionChangedEventArgs e)
        {
            var optionList = sender as ObservableCollection<Option>;
            if (e.Action == NotifyCollectionChangedAction.Remove && optionList.Count == 2 )
                OptionError = true;
            else
                OptionError = false;
        }

        private void deleteQuiz(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        public bool CheckResultsExist(QuizQuestion quizQuestion)
        {
            return Globals.quiz.answers.FirstOrDefault(answer => answer.Key == quizQuestion.id).Value.Count > 0;
        }
        
          private void quizCommitButton_Click(object sender, RoutedEventArgs e)
          {
              EditedQuiz.options.Clear();
              EditedQuiz.options = null;
              EditedQuiz.options = options.Where(o => !string.IsNullOrEmpty(o.optionText)).ToList();
              EditedQuiz.created = SandRibbonObjects.DateTimeFactory.Now().Ticks;
              if (validateQuiz(EditedQuiz))
              {
                  Commands.SendQuiz.Execute(EditedQuiz);
                  this.Close();
              }
          }
          private bool validateQuiz(QuizQuestion editedQuiz)
          {
              QuestionError = false;
              OptionError = false;
              if (string.IsNullOrEmpty(editedQuiz.question))
                  QuestionError = true;
              if (editedQuiz.options.Count < 2)
                  OptionError = true;
              return !(OptionError || QuestionError);
          }
          private void CloseEdit(object sender, RoutedEventArgs e)
          {
              Close();
          }
        private void QuizButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                tryPrefillOption((FrameworkElement)sender);
            }
        }
        #region Helpers
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
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var optionContainer = GetQuestionContainerFromItem(sender);
            if (optionContainer != null) 
                optionContainer.Opacity = 1;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var optionContainer = GetQuestionContainerFromItem(sender);
            if (optionContainer != null)
            {
                var option = (optionContainer.DataContext as Option);
                if (String.IsNullOrEmpty(option.optionText) && options.IndexOf(option) > 0)
                    optionContainer.Opacity = 0.5;
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
        private bool shouldAddNewEmptyOption()
        {
            var emptyOptions = options.Where(o => string.IsNullOrEmpty(o.optionText));
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
            foreach (var option in options)
            {
                var container = ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(option)); 
                if (container != null) container.Opacity = 1;
            }

            var newIndex =  1;
            var newName = Option.GetOptionNameFromIndex(0);
            if (options.Count > 0)
            {
                var lastOption = options.Last();
                newIndex = options.IndexOf(lastOption) + 1; 
                newName = Option.GetNextOptionName(lastOption.name);
            }
            var newOption = new Option(newName, String.Empty, false, generateColor(newIndex));
            if (shouldAddNewEmptyOption())
            {
                options.Add(newOption);
                var container = ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(newOption));
                if (container != null) container.Opacity = 0.5;
            }
            Commands.RequerySuggested();
        }
        private void RemoveQuizAnswer(object sender, RoutedEventArgs e)
        {
            var owner = ((FrameworkElement)sender).DataContext;
            options.Remove((Option)owner);

            // relabel the option names
            for (int i = 0; i < options.Count; i++)
            {
                //options[i].name = Option.GetOptionNameFromIndex(i);
                options[i].color = generateColor(i);
            }

            AddNewEmptyOption();
            foreach (var obj in options)
                if (String.IsNullOrEmpty(obj.optionText))
                    ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(obj)).Opacity = 0.5;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
