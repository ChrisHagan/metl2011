using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Canvas;
using SandRibbon.Utils.Connection;
using SandRibbonInterop.MeTLStanzas;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;


namespace SandRibbon.Quizzing
{
    public partial class ViewQuizResults : Window
    {
        private Dictionary<long, ObservableCollection<QuizAnswer>> answers = new Dictionary<long, ObservableCollection<QuizAnswer>>();
        private ObservableCollection<QuizQuestion> activeQuizes = new ObservableCollection<QuizQuestion>();

        public ViewQuizResults()
        {
            InitializeComponent();
            Commands.DisplayQuizResults.RegisterCommand(new DelegateCommand<object>(CloseQuizResults));
        }
        public ViewQuizResults(Dictionary<long, ObservableCollection<QuizAnswer>> answers, ObservableCollection<QuizQuestion> Quizes): this()
        {
            this.answers = answers;
            foreach(var quiz in Quizes)
                activeQuizes.Add(quiz);
            quizzes.ItemsSource = activeQuizes;
            quizzes.SelectedIndex = 0;
        }
        private void CloseQuizResults(object _obj)
        {
            this.Close();
        }
        private void QuizChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.adopt(() =>
                                 {
                                     var thisQuiz = (QuizQuestion) ((ListBox) sender).SelectedItem;
                                     var quizResult = new AssessAQuiz(answers[thisQuiz.id], thisQuiz);
                                     QuizResults.Children.Clear();
                                     QuizResults.Children.Add(quizResult);
                                 });
        }
    }
}
