using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Quizzing;
using SandRibbon.Providers;
using MeTLLib.Providers.Connection;
using SandRibbon.Components.Canvas;
using ImageDrop = SandRibbon.Components.ImageDrop;

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
            if (details.IsEmpty) return;
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
            if (details.IsJidEqual(Globals.location.activeConversation) && details.isDeleted)
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
                if (Globals.quiz.activeQuizzes.Any(q => q.id == quiz.id))
                {
                    QuizQuestion oldQuiz = Globals.quiz.activeQuizzes.Where(q => q.id == quiz.id).First();
                    if(quiz.created >= oldQuiz.created || quiz.IsDeleted)
                        Globals.quiz.activeQuizzes.Remove(oldQuiz);
                }
                if (!Globals.quiz.answers.ContainsKey(quiz.id))
                    Globals.quiz.answers[quiz.id] = new ObservableCollection<QuizAnswer>();
                if (!quiz.IsDeleted)
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
            var answerAQuiz = new AnswerAQuiz(thisQuiz);
            answerAQuiz.Owner = Window.GetWindow(this);
            answerAQuiz.ShowDialog();
        }
        private void importQuizSnapshot(string filename)
        {
            DelegateCommand<PreParser> onPreparserAvailable = null;
            onPreparserAvailable = new DelegateCommand<PreParser>((parser) =>
            {
                Commands.PreParserAvailable.UnregisterCommand(onPreparserAvailable);
                Commands.ImageDropped.ExecuteAsync(new ImageDrop
                {
                    Filename = filename,
                    Point = new Point(0,0),
                    Position = 1,
                    OverridePoint = false,
                    Target = "presentationSpace"
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
            var viewQuizResults = new ViewQuizResults(Globals.quiz.answers, Globals.quiz.activeQuizzes);
            viewQuizResults.Owner = Window.GetWindow(this);
            viewQuizResults.ShowDialog();
        }
    }
}