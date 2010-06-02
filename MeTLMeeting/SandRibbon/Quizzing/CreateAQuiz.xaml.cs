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
using System.Collections.ObjectModel;

namespace SandRibbon.Quizzing
{
    public partial class CreateAQuiz : Window
    {
        public static readonly string PROMPT_TEXT = "Please enter a quiz title";
        public ObservableCollection<Option> QUIZ_ANSWERS = new ObservableCollection<Option>
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
            quizQuestions.ItemsSource = QUIZ_ANSWERS.Select(a => new Option
            {
                color = QUESTIONCOLORS.ElementAt(num++ % QUESTIONCOLORS.Count),
                name = a.name,
                optionText="",
                correct=false
            });
        }
        private void help(object sender, RoutedEventArgs e)
        {
            var finalImage = new Image();
            BitmapImage helpImage = new BitmapImage();
            helpImage.BeginInit();
            helpImage.UriSource = new Uri("pack://application:,,,/MeTL;component/Resources/createAQuizHelp.PNG");
            helpImage.EndInit();
            finalImage.Source = helpImage;
            new Window{
                Content=finalImage,
                SizeToContent=SizeToContent.WidthAndHeight
            }.ShowDialog();
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
            foreach(object obj in quizQuestions.Items)
            {
                var answer = (Option)obj;
                if (!string.IsNullOrEmpty(answer.optionText))
                    quiz.options.Add(answer);
            }
            Commands.SendQuiz.Execute(quiz);
            this.Close();
        }
    }
}
