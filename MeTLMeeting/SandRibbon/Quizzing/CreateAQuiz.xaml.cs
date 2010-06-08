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
using WPFColors = System.Windows.Media.Colors;

namespace SandRibbon.Quizzing
{
    public partial class CreateAQuiz : Window
    {
        public static readonly string PROMPT_TEXT = "Please enter a quiz title";
        public static ObservableCollection<Option> options = new ObservableCollection<Option>
                                                     {
                                                         new Option {name = "A" }
                                                     };
        public CreateAQuiz()
        {
            InitializeComponent();
            options.First().color = AllColors.all[0];
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
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var emptyOptions = options.Where(o=>string.IsNullOrEmpty(o.optionText));
            if(emptyOptions.Count() > 1) return;
            foreach (var option in options)
                ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(option)).Opacity = 1;
            var newOption = new Option
            {
                name = new String(new[]{
                    (char)(options.Last().name.ToCharArray()[0]+1)
                }).ToUpper(),
                color = AllColors.all.ElementAt(AllColors.all.IndexOf(options.Last().color)+1)
            };
            options.Add(newOption);
            ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(newOption)).Opacity = 0.5;
        }
    }
}