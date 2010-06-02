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
using System.Windows.Shapes;
using SandRibbon.Providers;
using SandRibbonInterop;
using CheckBox=System.Windows.Controls.CheckBox;

namespace SandRibbon.Quizzing
{
    public partial class CreateAQuiz : Window
    {
        public static readonly string PROMPT_TEXT = "Please enter a quiz title";
        public List<Option> QUIZ_ANSWERS = new List<Option>
                                                     {
                                                         new Option {name = "A" },
                                                         new Option {name = "B"},
                                                         new Option {name = "C"},
                                                         new Option {name = "D"},
                                                     };
        public List<Color> QUESTIONCOLORS = new List<Color>
                                          {
                                              Colors.Red,
                                              Colors.Yellow, 
                                              Colors.Blue,
                                              Colors.Green
        };
        public CreateAQuiz()
        {
            InitializeComponent();
            addCreateQuestions();
        }
        private void addCreateQuestions()
        {
            var num = new Random().Next(QUESTIONCOLORS.Count);

            foreach(var question in QUIZ_ANSWERS)
            {
                var container = new StackPanel {Orientation = Orientation.Horizontal, Margin = new Thickness(0,10,0,10)};
                var questionIcon = new QuizButton{
                    DataContext=new Option{
                        color=QUESTIONCOLORS.ElementAt(num++ % QUESTIONCOLORS.Count),
                        name=question.name
                    }
                };
                container.Children.Add(questionIcon);
                container.Children.Add(new TextBox{Width = 300});
                var checkBox = new CheckBox{Margin = new Thickness(10, 10, 0, 0)};
                container.Children.Add(checkBox);
                KeyboardNavigation.SetIsTabStop(checkBox, false);
                quizQuestions.Children.Add(container);
            }
        }
        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void canCreateQuizQuestion(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = title != null && !(string.IsNullOrEmpty(title.Text) || title.Text == CreateAQuiz.PROMPT_TEXT);
        }
        private void CreateQuizQuestion(object sender, ExecutedRoutedEventArgs e)
        {
            var quiz = new QuizQuestion {title = title.Text, question = question.Text, author = Globals.me, id = DateTime.Now.Ticks};
            foreach(StackPanel questionField in quizQuestions.Children)
            {
                if(((TextBox)questionField.Children[1]).Text.Length > 0)
                {
                    var option = ((Option)((FrameworkElement)questionField.Children[0]).DataContext);
                    quiz.options.Add(new Option
                    {
                        name = option.name,
                        color = option.color,
                        optionText = ((TextBox) questionField.Children[1]).Text,
                        correct = (bool)((CheckBox) questionField.Children[2]).IsChecked, 
                    });
                }
            }
            Commands.SendQuiz.Execute(quiz);
            this.Close();
        }
    }
}
