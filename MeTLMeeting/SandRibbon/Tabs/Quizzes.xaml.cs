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
using Button=System.Windows.Controls.Button;

namespace SandRibbon.Tabs
{
    /*
     * The slide display should be on the right.  This is one of the features of CP3.
     * Color dialog to highlight current selection
     */
    public partial class Quizzes : Divelements.SandRibbon.RibbonTab
    {
        public ObservableCollection<MeTLLib.DataTypes.QuizQuestion> activeQuizes = new ObservableCollection<MeTLLib.DataTypes.QuizQuestion>();
        public Dictionary<long, ObservableCollection<MeTLLib.DataTypes.QuizAnswer>> answers = new Dictionary<long, ObservableCollection<MeTLLib.DataTypes.QuizAnswer>>();
        public Quizzes()
        {
            InitializeComponent();
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.QuizQuestion>(ReceiveQuiz));
            Commands.ReceiveQuizAnswer.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.QuizAnswer>(ReceiveQuizAnswer));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>(MoveTo));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preparserAvailable));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<object>(updateConversationDetails));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(joinConversation));
            Commands.QuizResultsSnapshotAvailable.RegisterCommand(new DelegateCommand<string>(importQuizSnapshot));
                quizzes.ItemsSource = activeQuizes;
        }
        private void joinConversation(string jid)
        {
            activeQuizes = new ObservableCollection<MeTLLib.DataTypes.QuizQuestion>();
        }
        private void updateConversationDetails(object obj)
        {
            Dispatcher.adoptAsync(delegate
                                      {
                                          try
                                          {
                                              if (Globals.isAuthor)
                                              {
                                                  quizResultsRibbonGroup.Header = "View results";
                                                  quizRibbonGroup.Visibility = Visibility.Visible;
                                                  createQuiz.Visibility = Visibility.Visible;
                                                  createQuiz.IsEnabled = true;
                                              }
                                              else
                                              {
                                                  quizResultsRibbonGroup.Header = "Respond";
                                                  quizRibbonGroup.Visibility = Visibility.Collapsed;
                                                  createQuiz.Visibility = Visibility.Collapsed;
                                              }
                                          }
                                          catch (NotSetException)
                                          {
                                          }
                                      });

        }
        private void preparserAvailable(PreParser preParser)
        {
            foreach (var quiz in preParser.quizzes)
            {
                ReceiveQuiz(quiz);
            }
            foreach (var answer in preParser.quizAnswers)
                ReceiveQuizAnswer(answer);
        }
        private void MoveTo(object obj)
        {
            quizzes.ItemsSource = activeQuizes;
        }
        private void ReceiveQuizAnswer(MeTLLib.DataTypes.QuizAnswer answer)
        {
            if (answers.ContainsKey(answer.id))
            {
                if (answers[answer.id].Where(a => a.answerer == answer.answerer).Count() > 0)
                {
                    var oldAnswer = answers[answer.id].Where(a => a.answerer == answer.answerer).First();
                    answers[answer.id].Remove(oldAnswer);
                }
                answers[answer.id].Add(answer);
            }
            else
            {
                var newList = new ObservableCollection<MeTLLib.DataTypes.QuizAnswer>();
                newList.Add(answer);
                answers.Add(answer.id, newList);
            }
        }
        private void ReceiveQuiz(MeTLLib.DataTypes.QuizQuestion quiz)
        {
            if (activeQuizes.Any(q => q.id == quiz.id)) return;
            if (!answers.ContainsKey(quiz.id))
                answers[quiz.id] = new ObservableCollection<MeTLLib.DataTypes.QuizAnswer>();
            activeQuizes.Add(quiz);
            quizzes.ScrollToEnd();
        }
        private void CreateQuiz(object sender, RoutedEventArgs e)
        {
            Commands.BlockInput.Execute("Create a quiz dialog open.");
            var quizDialog = new CreateAQuiz(activeQuizes.Count);
            quizDialog.Owner = Window.GetWindow(this);
            quizDialog.ShowDialog();
        }
        private void quiz_Click(object sender, RoutedEventArgs e)
        {
            var thisQuiz = (MeTLLib.DataTypes.QuizQuestion)((FrameworkElement)sender).DataContext;
            if (thisQuiz.author == Globals.me)
                new AssessAQuiz(answers[thisQuiz.id], thisQuiz).Show();
            else
                new AnswerAQuiz(thisQuiz).Show();
        }

        private void importQuizSnapshot(string filename)
        {
            DelegateCommand<PreParser> onPreparserAvailable = null;
            onPreparserAvailable = new DelegateCommand<PreParser>((parser) =>
            {
                Commands.PreParserAvailable.UnregisterCommand(onPreparserAvailable);
                Commands.PlaceQuizSnapshot.Execute(filename);
            });
            Commands.PreParserAvailable.RegisterCommand(onPreparserAvailable);
            Commands.AddSlide.Execute(null);
        }
    }
}