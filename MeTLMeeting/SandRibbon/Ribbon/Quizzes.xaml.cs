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
using ImageDrop = SandRibbon.Components.ImageDrop;
using System.Collections.Generic;
using System.Windows.Controls.Ribbon;
using SandRibbon.Pages.Collaboration;

namespace SandRibbon.Tabs
{
    public partial class Quizzes : RibbonTab
    {
        public static RoutedCommand openQuizResults = new RoutedCommand();
        public RibbonCollaborationPage rootPage { get; protected set; }
        public Quizzes()
        {
            InitializeComponent();
            var receiveQuizCommand = new DelegateCommand<MeTLLib.DataTypes.QuizQuestion>(ReceiveQuiz);
            var receiveQuizAnswerCommand = new DelegateCommand<MeTLLib.DataTypes.QuizAnswer>(ReceiveQuizAnswer);
            var preParserAvailableCommand = new DelegateCommand<PreParser>(preparserAvailable);
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(updateConversationDetails);
            var joinConversationCommand = new DelegateCommand<object>(JoinConversation);
            var quizResultsSnapshotAvailableCommand = new DelegateCommand<string>(importQuizSnapshot);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as RibbonCollaborationPage;
                Commands.ReceiveQuiz.RegisterCommand(receiveQuizCommand);
                Commands.ReceiveQuizAnswer.RegisterCommand(receiveQuizAnswerCommand);
                Commands.PreParserAvailable.RegisterCommand(preParserAvailableCommand);
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateConversationDetailsCommand);
                Commands.JoinConversation.RegisterCommandToDispatcher(joinConversationCommand);
                Commands.QuizResultsSnapshotAvailable.RegisterCommand(quizResultsSnapshotAvailableCommand);
                quizzes.ItemsSource = Globals.quiz.activeQuizzes;
                SetupUI();
            };
            Unloaded += (s, e) => {
                Commands.ReceiveQuiz.UnregisterCommand(receiveQuizCommand);
                Commands.ReceiveQuizAnswer.UnregisterCommand(receiveQuizAnswerCommand);
                Commands.PreParserAvailable.UnregisterCommand(preParserAvailableCommand);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.JoinConversation.UnregisterCommand(joinConversationCommand);
                Commands.QuizResultsSnapshotAvailable.UnregisterCommand(quizResultsSnapshotAvailableCommand);
            };
        }
        private void SetupUI()
        {
            Dispatcher.adoptAsync( 
                delegate
                {
                    try
                    {
                        if (rootPage.details.isAuthor(Globals.me))
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
                if (rootPage.details.isAuthor(Globals.me))
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
                int oldQuizIndex = -1;
                if (Globals.quiz.activeQuizzes.Any(q => q.Id == quiz.Id))
                {
                    List<QuizQuestion> oldQuizzes = Globals.quiz.activeQuizzes.Where(q => q.Id == quiz.Id).ToList();
                    QuizQuestion oldQuiz = oldQuizzes.First();
                    if (quiz.Created >= oldQuiz.Created)
                    {
                        oldQuizIndex = Globals.quiz.activeQuizzes.IndexOf(oldQuiz);
                        foreach (var q in oldQuizzes)
                        {
                            Globals.quiz.activeQuizzes.Remove(q);
                        }
                    }
                }
                if (!Globals.quiz.answers.ContainsKey(quiz.Id))
                    Globals.quiz.answers[quiz.Id] = new ObservableCollection<QuizAnswer>();
                if (!quiz.IsDeleted)
                {
                    if (oldQuizIndex == -1)
                        Globals.quiz.activeQuizzes.Add(quiz);
                    else
                        Globals.quiz.activeQuizzes.Insert(oldQuizIndex, quiz);
                }
                // force the UI to update the labels. this is horrible
                var tempQuizzes = new List<QuizQuestion>();
                tempQuizzes.AddRange(Globals.quiz.activeQuizzes);
                Globals.quiz.activeQuizzes.Clear();
                foreach (var reindexQuiz in tempQuizzes)
                {
                    Globals.quiz.activeQuizzes.Add(reindexQuiz);
                }
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

            var viewEditAQuiz = new ViewEditAQuiz(thisQuiz);
            viewEditAQuiz.Owner = Window.GetWindow(this);
            viewEditAQuiz.ShowDialog();
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
            e.CanExecute = (Globals.quiz.activeQuizzes != null && Globals.quiz.activeQuizzes.Count > 0);
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