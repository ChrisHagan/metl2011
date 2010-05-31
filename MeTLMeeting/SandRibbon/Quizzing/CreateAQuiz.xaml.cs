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
        public List<Option> QUIZ_ANSWERS = new List<Option>
                                                     {
                                                         new Option {name = "A" },
                                                         new Option {name = "B"},
                                                         new Option {name = "C"},
                                                         new Option {name = "D"},
                                                     };

        public List<Brush> QUESTIONCOLORS = new List<Brush>
                                          {
                                              Brushes.Red,
                                              Brushes.Yellow, 
                                              Brushes.Blue,
                                              Brushes.Green
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
                var questionIcon = new Ellipse {Fill = QUESTIONCOLORS.ElementAt((num += 1) % QUESTIONCOLORS.Count) , Height = 40, Width = 40, Name = question.name};
                container.Children.Add(questionIcon);
                container.Children.Add(new TextBox{Width = 300});
                container.Children.Add(new CheckBox{Margin = new Thickness(10, 10, 0, 0)});
                quizQuestions.Children.Add(container);
            }
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void canCreateQuizQuestion(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {

                bool a = Globals.conversationDetails.Author == "pants";
                if (title == null)
                    e.CanExecute = false;
                else
                    e.CanExecute = !(title.Text == "Please enter a quiz title");
            }
            catch(NotSetException err)
            {
                e.CanExecute = false;
            }
        }

        private void CreateQuizQuestion(object sender, ExecutedRoutedEventArgs e)
        {
           var quiz = new QuizQuestion {title = title.Text, question = question.Text, author = Globals.me, id = DateTime.Now.Ticks};
                foreach(StackPanel questionField in quizQuestions.Children)
                {
                    if(((TextBox)questionField.Children[1]).Text.Length > 0)
                    {
                        quiz.options.Add(new Option
                            {
                                name = ((Ellipse) questionField.Children[0]).Name,
                                color = ((SolidColorBrush)((Ellipse) questionField.Children[0]).Fill).Color,
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
