using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
        public ConversationDetails cd
        {
            get; set;
        }
        public Quizzes()
        {
            InitializeComponent();
            var createQuizCommand = new DelegateCommand<object>(createQuiz,canCreateQuiz);
            var openQuizCommand = new DelegateCommand<QuizQuestion>(openQuiz, canOpenQuiz);
            var receiveQuizCommand = new DelegateCommand<MeTLLib.DataTypes.QuizQuestion>(ReceiveQuiz);
            var receiveQuizAnswerCommand = new DelegateCommand<MeTLLib.DataTypes.QuizAnswer>(ReceiveQuizAnswer);
            var quizResultsSnapshotAvailableCommand = new DelegateCommand<string>(importQuizSnapshot);            
            var viewQuizResultsCommand = new DelegateCommand<object>(viewQuizResults, canViewQuizResults);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as SlideAwarePage;
                cd = rootPage.ConversationDetails;
                Commands.CreateQuiz.RegisterCommand(createQuizCommand);
                Commands.OpenQuiz.RegisterCommand(openQuizCommand);
                Commands.ReceiveQuiz.RegisterCommand(receiveQuizCommand);
                Commands.ReceiveQuizAnswer.RegisterCommand(receiveQuizAnswerCommand);
                Commands.QuizResultsSnapshotAvailable.RegisterCommand(quizResultsSnapshotAvailableCommand);
                Commands.ViewQuizResults.RegisterCommand(viewQuizResultsCommand);
                quizzes.ItemsSource = rootPage.ConversationState.QuizData.activeQuizzes;
                rootPage.NetworkController.client.historyProvider.Retrieve<PreParser>(() => { }, (i, j) => { }, (parser) => PreParserAvailable(parser), rootPage.ConversationDetails.Jid);
            };
            Unloaded += (s, e) =>
            {
                Commands.CreateQuiz.UnregisterCommand(createQuizCommand);
                Commands.OpenQuiz.UnregisterCommand(openQuizCommand);
                Commands.ReceiveQuiz.UnregisterCommand(receiveQuizCommand);
                Commands.ReceiveQuizAnswer.UnregisterCommand(receiveQuizAnswerCommand);
                Commands.QuizResultsSnapshotAvailable.UnregisterCommand(quizResultsSnapshotAvailableCommand);
                Commands.ViewQuizResults.UnregisterCommand(viewQuizResultsCommand);
            };
        }

        private void viewQuizResults(object obj)
        {
            var viewQuizResults = new ViewQuizResults(rootPage.Slide, rootPage.ConversationState.QuizData.answers, rootPage.ConversationState.QuizData.activeQuizzes);
            viewQuizResults.Owner = Window.GetWindow(this);
            viewQuizResults.ShowDialog();
        }

        private bool canViewQuizResults(object arg)
        {
            var isAuthor = cd.isAuthor(rootPage.NetworkController.credentials.name);
            var studentsCanViewResults = cd.Permissions.studentsCanViewQuizResults;
            var resultsExist = rootPage.ConversationState.QuizData.answers.Count() > 0;
            return resultsExist && (isAuthor || studentsCanViewResults);
        }

        private void updateConversationDetails(ConversationDetails cd)
        {
            this.cd = cd;
        }

        protected void PreParserAvailable(PreParser parser)
        {
            parser.quizzes.ForEach(q =>
            {
                ReceiveQuiz(q);
            });
            parser.quizAnswers.ForEach(qa =>
            {
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
                Commands.RequerySuggested(Commands.ViewQuizResults);
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
        private void createQuiz(object sender)
        {            
            Dispatcher.adoptAsync(() =>
            {
                var quizDialog = new CreateAQuiz(rootPage.NetworkController, rootPage.ConversationDetails, rootPage.Slide, rootPage.ConversationState.QuizData.activeQuizzes.Count);
                quizDialog.Owner = Window.GetWindow(this);
                quizDialog.ShowDialog();
            });
        }
        private bool canCreateQuiz(object sender) {
            return rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name) || rootPage.ConversationDetails.Permissions.studentsCanCreateQuiz;
        } 
        private void importQuizSnapshot(string filename)
        {
            Commands.ImageDropped.Execute(new ImageDrop
            {
                Filename = filename,
                Point = new Point(0, 0),
                Position = 1,
                OverridePoint = false,
                Target = "presentationSpace"
            });
        }
        private bool canOpenQuiz(QuizQuestion quiz)
        {
            return rootPage.ConversationDetails.Permissions.studentsCanViewQuiz || rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name);
        } 
        private void openQuiz(QuizQuestion quiz)
        {   
            rootPage.NavigationService.Navigate(new ViewEditAQuiz(quiz,rootPage));
        }
    }
}