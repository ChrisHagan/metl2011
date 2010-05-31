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
using Button=SandRibbonInterop.Button;

namespace SandRibbon.Quizzing
{
    /// <summary>
    /// Interaction logic for AnswerAQuiz.xaml
    /// </summary>
    public partial class AnswerAQuiz : Window
    {
        private QuizQuestion theQuiz;
        public AnswerAQuiz()
        {
            InitializeComponent();
        }
        public AnswerAQuiz(QuizQuestion thisQuiz):this()
        {
            theQuiz = thisQuiz;
            Quiz.Children.Add(new Label {Content = string.Format("Title: {0}", thisQuiz.title)});
            if(thisQuiz.question.Length > 0)
                Quiz.Children.Add(new Label {Content = string.Format("Question: {0}", thisQuiz.question)});

            foreach(var option in thisQuiz.options)
            {
                var container = new StackPanel {Orientation = Orientation.Horizontal, Margin = new Thickness(0,10,0,10)};
                var button = new System.Windows.Controls.Button {Background = Brushes.Transparent, Tag = option.correct};
                button.Click += new RoutedEventHandler(answerQuiz);
                var questionIcon = new Ellipse {Fill = new SolidColorBrush(option.color), Height = 40, Width = 40, Name = option.name};
                button.Content = questionIcon;
                container.Children.Add(button);
                container.Children.Add(new Label{Content=option.optionText});
                Quiz.Children.Add(container);
            }
        }

        private void answerQuiz(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.Controls.Button)sender).Tag.ToString().ToLower() == "true")
                MessageBox.Show("Nice Shooting Text");

            Commands.SendQuizAnswer.Execute(new QuizAnswer
                                                {
                                                    answerer = Globals.me,
                                                    answer = ((Ellipse) ((System.Windows.Controls.Button) sender).Content).Name,
                                                    id = theQuiz.id
                                               });
            this.Close();
        }
    }
}
