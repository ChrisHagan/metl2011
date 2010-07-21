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
using SandRibbon.Providers;

namespace SandRibbon.Tabs
{
    /*
     * The slide display should be on the right.  This is one of the features of CP3.
     * Color dialog to highlight current selection
     */
    public partial class Quizzes : Divelements.SandRibbon.RibbonTab
    {
        public ObservableCollection<QuizQuestion> activeQuizes = new ObservableCollection<QuizQuestion>();
        public Dictionary<long, ObservableCollection<QuizAnswer>> answers = new Dictionary<long, ObservableCollection<QuizAnswer>>();
        public Quizzes()
        {
            InitializeComponent();
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<QuizQuestion>(ReceiveQuiz));
            Commands.ReceiveQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(ReceiveQuizAnswer));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>(MoveTo));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preparserAvailable));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<object>(updateConversationDetails));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(joinConversation));
            quizzes.ItemsSource = activeQuizes;
            
        }
        private void joinConversation(object obj)
        {
            activeQuizes = new ObservableCollection<QuizQuestion>();
        }
        private void updateConversationDetails(object obj)
        {
            Dispatcher.adoptAsync(delegate
                                      {
                                          try
                                          {
                                              if (Globals.isAuthor)
                                                  createQuiz.Visibility = Visibility.Visible;
                                              else
                                                  createQuiz.Visibility = Visibility.Collapsed;
                                          }
                                          catch (NotSetException)
                                          {
                                          }
                                      });
            
        }
        private void preparserAvailable(PreParser preParser)
        {
            foreach(var quiz in preParser.quizzes)
                ReceiveQuiz(quiz);
            foreach(var answer in preParser.quizAnswers)
                ReceiveQuizAnswer(answer);
        }
        private void MoveTo(object obj)
        {
            quizzes.ItemsSource = activeQuizes;
        }
        private void ReceiveQuizAnswer(QuizAnswer answer)
        {
            if (answers.ContainsKey(answer.id))
            {
                if(answers[answer.id].Where(a => a.answerer == answer.answerer).Count() > 0)
                {
                    var oldAnswer = answers[answer.id].Where(a => a.answerer == answer.answerer).First();
                    answers[answer.id].Remove(oldAnswer);
                }
                answers[answer.id].Add(answer);
            }
            else
            {
                var newList = new ObservableCollection<QuizAnswer>();
                newList.Add(answer);
                answers.Add(answer.id, newList);
            }
        }
        private void ReceiveQuiz(QuizQuestion quiz)
        {
            if (activeQuizes.Any(q => q.id == quiz.id)) return;
            if (!answers.ContainsKey(quiz.id))
                answers[quiz.id] = new ObservableCollection<QuizAnswer>();
            activeQuizes.Add(quiz);
        }
        private void CreateQuiz(object sender, RoutedEventArgs e)
        {
            new CreateAQuiz().Show();
        }
        private void quiz_Click(object sender, RoutedEventArgs e)
        {
            var thisQuiz = (QuizQuestion) ((FrameworkElement)sender).DataContext;
            if (thisQuiz.author == Globals.me)
                new AssessAQuiz(answers[thisQuiz.id], thisQuiz).Show();
            else
                new AnswerAQuiz(thisQuiz).Show();
        }
    }
}