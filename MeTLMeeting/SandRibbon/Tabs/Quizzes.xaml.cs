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
using SandRibbon.Components.Canvas;

namespace SandRibbon.Tabs
{
    public partial class Quizzes : Divelements.SandRibbon.RibbonTab
    {
        public static RoutedCommand openQuizResults = new RoutedCommand();
        public Quizzes()
        {
            InitializeComponent();
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.QuizQuestion>(ReceiveQuiz));
            Commands.ReceiveQuizAnswer.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.QuizAnswer>(ReceiveQuizAnswer));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preparserAvailable));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(updateConversationDetails));
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(JoinConversation));
            Commands.QuizResultsSnapshotAvailable.RegisterCommand(new DelegateCommand<string>(importQuizSnapshot));
            quizzes.ItemsSource = Globals.quiz.activeQuizzes;
            SetupUI();
        }
        private void SetupUI()
        {
            Dispatcher.adoptAsync( 
                delegate
                {
                    try
                    {
                        if (Globals.isAuthor)
                            amAuthor();
                        else
                            amRespondent();
                    }
                    catch(NotSetException)
                    {
                    }
                });
        }
        private void JoinConversation(object _jid)
        {
            Globals.quiz.activeQuizzes.Clear();
            SetupUI();
        }
        private void amAuthor()
        {
            quizResultsRibbonGroup.Header = "Quizzes";
            quizRibbonGroup.Visibility = Visibility.Visible;
            createQuiz.Visibility = Visibility.Visible;
            createQuiz.IsEnabled = true;
            results.Visibility = Visibility.Visible;
        }
        private void amRespondent()
        {
            quizResultsRibbonGroup.Header = "Respond";
            quizRibbonGroup.Visibility = Visibility.Collapsed;
            //createQuiz.Visibility = Visibility.Collapsed;
            createQuiz.IsEnabled = false;
            results.Visibility = Visibility.Collapsed;
        }
        private void updateConversationDetails(ConversationDetails details)
        {
            if (ConversationDetails.Empty.Equals(details)) return;
            try
            {
                if (Globals.isAuthor)
                {
                    amAuthor();            
                }
                else
                {
                    amRespondent();
                }
            }
            catch (NotSetException)
            {
            }
            if (details.Jid == Globals.location.activeConversation && details.Subject.ToLower() == "deleted")
            {
                Globals.quiz.answers.Clear();
                Globals.quiz.activeQuizzes.Clear();
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
                if (Globals.quiz.answers.ContainsKey(answer.id))
                {
                    if (Globals.quiz.answers[answer.id].Where(a => a.answerer == answer.answerer).Count() > 0)
                    {
                        var oldAnswer = Globals.quiz.answers[answer.id].Where(a => a.answerer == answer.answerer).First();
                        Globals.quiz.answers[answer.id].Remove(oldAnswer);
                    }
                    Globals.quiz.answers[answer.id].Add(answer);
                }
                else
                {
                    var newList = new ObservableCollection<QuizAnswer> { answer };
                    Globals.quiz.answers.Add(answer.id, newList);
                }
            });
        }
        private void ReceiveQuiz(QuizQuestion quiz)
        {
            Dispatcher.adoptAsync(() =>
            {
                if (Globals.quiz.activeQuizzes.Any(q => q.id == quiz.id)) return;
                if (!Globals.quiz.answers.ContainsKey(quiz.id))
                    Globals.quiz.answers[quiz.id] = new ObservableCollection<QuizAnswer>();
                Globals.quiz.activeQuizzes.Add(quiz);
                quizzes.ScrollToEnd();
            });
        }
        private void CreateQuiz(object sender, RoutedEventArgs e)
        {
            Commands.BlockInput.ExecuteAsync("Create a quiz dialog open.");
            Dispatcher.adoptAsync(() =>
            {
                var quizDialog = new CreateAQuiz(Globals.quiz.activeQuizzes.Count);
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
                Commands.PlaceQuizSnapshot.ExecuteAsync(new ImageDropParameters
                {
                    file = filename,
                    location = new Point(200,200)
                });
            });
            Commands.PreParserAvailable.RegisterCommand(onPreparserAvailable);
            Commands.AddSlide.ExecuteAsync(null);
        }
        private void canOpenResults(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Globals.quiz.activeQuizzes != null && Globals.quiz.activeQuizzes.Count > 0) ? true : false;
        }
        private void OpenResults(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.BlockInput.ExecuteAsync("Viewing a quiz.");
            new ViewQuizResults(Globals.quiz.answers, Globals.quiz.activeQuizzes).ShowDialog();
        }
    }
}