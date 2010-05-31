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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Quizzing;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;

namespace SandRibbon.Tabs
{
    public partial class Quizzes : Divelements.SandRibbon.RibbonTab
    {
        public ObservableCollection<QuizQuestion> activeQuizes = new ObservableCollection<QuizQuestion>();
        public Dictionary<long, List<QuizAnswer>> answers = new Dictionary<long, List<QuizAnswer>>();
        public Quizzes()
        {
            InitializeComponent();
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<QuizQuestion>(receiveQuiz));
            Commands.ReceiveQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(recieveQuizAnswer));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>(MoveTo));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preparserAvailable));
            quizzes.ItemsSource = activeQuizes;

        }

        private void preparserAvailable(PreParser preParser)
        {
            foreach(var quiz in preParser.quizs)
                receiveQuiz(quiz);
        }

        private void MoveTo(object obj)
        {
            activeQuizes = new ObservableCollection<QuizQuestion>();
            quizzes.ItemsSource = activeQuizes;
        }

        private void recieveQuizAnswer(QuizAnswer answer)
        {
            if(answers.ContainsKey(answer.id))
                answers[answer.id].Add(answer);
            else
            {
                var newList = new List<QuizAnswer>();
                newList.Add(answer);
                answers.Add(answer.id, newList);
            }
            MessageBox.Show(answer.answer);
        }

        private void receiveQuiz(QuizQuestion quiz)
        {
            activeQuizes.Add(quiz);
        }
        private void CreateQuiz(object sender, RoutedEventArgs e)
        {
            new CreateAQuiz().ShowDialog();
        }

        private void quiz_Click(object sender, RoutedEventArgs e)
        {
            var thisQuiz = (QuizQuestion) ((System.Windows.Controls.Button) sender).DataContext;
            new AnswerAQuiz(thisQuiz).ShowDialog();
            var a = 1;
        }
    }
}
