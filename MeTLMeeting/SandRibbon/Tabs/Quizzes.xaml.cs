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
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Quizzing;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbon.Providers;
using Button = System.Windows.Controls.Button;
using MeTLLib.Providers.Connection;

namespace SandRibbon.Tabs
{
    public partial class Quizzes : Divelements.SandRibbon.RibbonTab
    {
        public static RoutedCommand openQuizResults = new RoutedCommand();
        public ObservableCollection<QuizQuestion> activeQuizzes = new ObservableCollection<MeTLLib.DataTypes.QuizQuestion>();
        public Dictionary<long, ObservableCollection<MeTLLib.DataTypes.QuizAnswer>> answers = new Dictionary<long, ObservableCollection<MeTLLib.DataTypes.QuizAnswer>>();
        public Quizzes()
        {
            InitializeComponent();
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.QuizQuestion>(ReceiveQuiz));
            Commands.ReceiveQuizAnswer.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.QuizAnswer>(ReceiveQuizAnswer));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preparserAvailable));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(updateConversationDetails));
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(JoinConversation));
            Commands.QuizResultsSnapshotAvailable.RegisterCommand(new DelegateCommand<string>(importQuizSnapshot));
            quizzes.ItemsSource = activeQuizzes;
        }
        private void JoinConversation(object _jid)
        {
            activeQuizzes.Clear();
        }
        private void updateConversationDetails(ConversationDetails details)
        {
            if (ConversationDetails.Empty.Equals(details)) return;
            try
            {
                if (Globals.isAuthor)
                {
                    quizResultsRibbonGroup.Header = "Quizzes";
                    quizRibbonGroup.Visibility = Visibility.Visible;
                    createQuiz.Visibility = Visibility.Visible;
                    createQuiz.IsEnabled = true;
                    results.Visibility = Visibility.Visible;
                }
                else
                {
                    quizResultsRibbonGroup.Header = "Respond";
                    quizRibbonGroup.Visibility = Visibility.Collapsed;
                    createQuiz.Visibility = Visibility.Collapsed;
                    results.Visibility = Visibility.Collapsed;
                }
            }
            catch (NotSetException)
            {
            }
            if (details.Jid == Globals.location.activeConversation && details.Subject.ToLower() == "deleted")
            {
                answers.Clear();
                activeQuizzes.Clear();
            }
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
        private void ReceiveQuizAnswer(MeTLLib.DataTypes.QuizAnswer answer)
        {
            Dispatcher.adoptAsync(() =>
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
                    var newList = new ObservableCollection<QuizAnswer> { answer };
                    answers.Add(answer.id, newList);
                }
            });
        }
        private void ReceiveQuiz(QuizQuestion quiz)
        {
            Dispatcher.adoptAsync(() =>
            {
                if (activeQuizzes.Any(q => q.id == quiz.id)) return;
                if (!answers.ContainsKey(quiz.id))
                    answers[quiz.id] = new ObservableCollection<QuizAnswer>();
                activeQuizzes.Add(quiz);
                quizzes.ScrollToEnd();
            });
        }
        private void CreateQuiz(object sender, RoutedEventArgs e)
        {
            Commands.BlockInput.ExecuteAsync("Create a quiz dialog open.");
            Dispatcher.adoptAsync(() =>
            {
                var quizDialog = new CreateAQuiz(activeQuizzes.Count);
                quizDialog.Owner = Window.GetWindow(this);
                quizDialog.ShowDialog();
            });
        }
        private void quiz_Click(object sender, RoutedEventArgs e)
        {
            var thisQuiz = (MeTLLib.DataTypes.QuizQuestion)((FrameworkElement)sender).DataContext;
            Commands.BlockInput.ExecuteAsync("Answering a Quiz.");
            new AnswerAQuiz(thisQuiz).ShowDialog();
        }
        private void importQuizSnapshot(string filename)
        {
            DelegateCommand<PreParser> onPreparserAvailable = null;
            onPreparserAvailable = new DelegateCommand<PreParser>((parser) =>
            {
                Commands.PreParserAvailable.UnregisterCommand(onPreparserAvailable);
                Commands.PlaceQuizSnapshot.ExecuteAsync(filename);
            });
            Commands.PreParserAvailable.RegisterCommand(onPreparserAvailable);
            Commands.AddSlide.ExecuteAsync(null);
        }
        private void canOpenResults(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (activeQuizzes != null && activeQuizzes.Count > 0) ? true : false;
        }
        private void OpenResults(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.BlockInput.ExecuteAsync("Viewing a quiz.");
            new ViewQuizResults(answers, activeQuizzes).ShowDialog();
        }
    }
}