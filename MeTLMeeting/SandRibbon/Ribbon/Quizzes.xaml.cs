using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Quizzing;
using MeTLLib.Providers.Connection;
using ImageDrop = SandRibbon.Components.ImageDrop;
using System.Collections.Generic;
using System.Windows.Controls.Ribbon;
using SandRibbon.Pages;

namespace SandRibbon.Tabs
{
    public partial class Quizzes : RibbonTab
    {        
        public SlideAwarePage rootPage { get; protected set; }
        public Quizzes()
        {
            InitializeComponent();
            var receiveQuizCommand = new DelegateCommand<MeTLLib.DataTypes.QuizQuestion>(ReceiveQuiz);
            var receiveQuizAnswerCommand = new DelegateCommand<MeTLLib.DataTypes.QuizAnswer>(ReceiveQuizAnswer);                        
            var quizResultsSnapshotAvailableCommand = new DelegateCommand<string>(importQuizSnapshot);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as SlideAwarePage;
                Commands.ReceiveQuiz.RegisterCommand(receiveQuizCommand);
                Commands.ReceiveQuizAnswer.RegisterCommand(receiveQuizAnswerCommand);            
                Commands.QuizResultsSnapshotAvailable.RegisterCommand(quizResultsSnapshotAvailableCommand);
                quizzes.ItemsSource = rootPage.ConversationState.QuizData.activeQuizzes;
                rootPage.NetworkController.client.historyProvider.Retrieve<PreParser>(() => { }, (i, j) => { }, (parser) => PreParserAvailable(parser), rootPage.ConversationDetails.Jid);
            };
            Unloaded += (s, e) => {
                Commands.ReceiveQuiz.UnregisterCommand(receiveQuizCommand);
                Commands.ReceiveQuizAnswer.UnregisterCommand(receiveQuizAnswerCommand);                
                Commands.QuizResultsSnapshotAvailable.UnregisterCommand(quizResultsSnapshotAvailableCommand);
            };
        }                     
        protected void PreParserAvailable(PreParser parser)
        {
            parser.quizzes.ForEach(q => {
                ReceiveQuiz(q);
            });
            parser.quizAnswers.ForEach(qa => {
                ReceiveQuizAnswer(qa);
            });
        }
        private void ReceiveQuizAnswer(MeTLLib.DataTypes.QuizAnswer answer)
        {
            Dispatcher.adoptAsync(() =>
            {
                var qd = rootPage.ConversationState.QuizData;
                if (qd.answers.ContainsKey(answer.id))
                {
                    if (qd.answers[answer.id].Where(a => a.answerer == answer.answerer).Count() > 0)
                    {
                        var oldAnswer = qd.answers[answer.id].Where(a => a.answerer == answer.answerer).First();
                        qd.answers[answer.id].Remove(oldAnswer);
                    }
                    qd.answers[answer.id].Add(answer);
                }
                else
                {
                    var newList = new ObservableCollection<QuizAnswer> { answer };
                    qd.answers.Add(answer.id, newList);
                }
            });
        }
        private void ReceiveQuiz(QuizQuestion quiz)
        {
            Dispatcher.adoptAsync(() =>
            {
                var qd = rootPage.ConversationState.QuizData;
                int oldQuizIndex = -1;
                if (qd.activeQuizzes.Any(q => q.Id == quiz.Id))
                {
                    List<QuizQuestion> oldQuizzes = qd.activeQuizzes.Where(q => q.Id == quiz.Id).ToList();
                    QuizQuestion oldQuiz = oldQuizzes.First();
                    if (quiz.Created >= oldQuiz.Created)
                    {
                        oldQuizIndex = qd.activeQuizzes.IndexOf(oldQuiz);
                        foreach (var q in oldQuizzes)
                        {
                            qd.activeQuizzes.Remove(q);
                        }
                    }
                }
                if (!qd.answers.ContainsKey(quiz.Id))
                    qd.answers[quiz.Id] = new ObservableCollection<QuizAnswer>();
                if (!quiz.IsDeleted)
                {
                    if (oldQuizIndex == -1)
                        qd.activeQuizzes.Add(quiz);
                    else
                        qd.activeQuizzes.Insert(oldQuizIndex, quiz);
                }
                // force the UI to update the labels. this is horrible
                var tempQuizzes = new List<QuizQuestion>();
                tempQuizzes.AddRange(rootPage.ConversationState.QuizData.activeQuizzes);
                qd.activeQuizzes.Clear();
                foreach (var reindexQuiz in tempQuizzes)
                {
                    qd.activeQuizzes.Add(reindexQuiz);
                }
                quizzes.ScrollToEnd();
            });
        }
        private void CreateQuiz(object sender, RoutedEventArgs e)
        {
            Commands.BlockInput.ExecuteAsync("Create a quiz dialog open.");
            Dispatcher.adoptAsync(() =>
            {
                var quizDialog = new CreateAQuiz(rootPage.NetworkController,rootPage.ConversationDetails,rootPage.Slide,rootPage.ConversationState.QuizData.activeQuizzes.Count);
                quizDialog.Owner = Window.GetWindow(this);
                quizDialog.ShowDialog();
            });
        }
        private void quiz_Click(object sender, RoutedEventArgs e)
        {
            var thisQuiz = (MeTLLib.DataTypes.QuizQuestion)((FrameworkElement)sender).DataContext;
            Commands.BlockInput.ExecuteAsync("Answering a Quiz.");

            var viewEditAQuiz = new ViewEditAQuiz(thisQuiz,rootPage.NetworkController,rootPage.ConversationDetails, rootPage.ConversationState, rootPage.Slide,rootPage.NetworkController.credentials.name);
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
            var qd = rootPage.ConversationState.QuizData;
            e.CanExecute = (qd.activeQuizzes != null && qd.activeQuizzes.Count > 0);
        }
        private void OpenResults(object sender, RoutedEventArgs e)
        {
            Commands.BlockInput.ExecuteAsync("Viewing a quiz.");
            var viewQuizResults = new ViewQuizResults(rootPage.Slide, rootPage.ConversationState.QuizData.answers, rootPage.ConversationState.QuizData.activeQuizzes);
            viewQuizResults.Owner = Window.GetWindow(this);
            viewQuizResults.ShowDialog();
        }
    }
}