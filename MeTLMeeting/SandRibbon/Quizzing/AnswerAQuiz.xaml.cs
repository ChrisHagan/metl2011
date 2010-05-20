using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;

namespace SandRibbon.Quizzing
{
    public partial class AnswerAQuiz
    {
        public QuizDetails Quiz;
        private Dictionary<object, List<object>> results = new Dictionary<object, List<object>>();
        private readonly ObservableCollection<Option> options = new ObservableCollection<Option>();
        private readonly JabberWire.UserInformation info;
        private bool answered;
        private Option selectedOption;
        public AnswerAQuiz(JabberWire.UserInformation userInfo, QuizDetails quiz)
        {
            InitializeComponent();
            info = userInfo;
            Quiz = quiz;
            canvasStack.SetEditable(true);
            barGraph.setQuiz(quiz.targetSlide);
            SetBarGraphVisibility();
            Loaded += loaded;
            Commands.ReceiveQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(receiveQuizAnswer));
            Commands.SetQuizOptions.RegisterCommand(new DelegateCommand<Option>(option => selectedOption = option));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(receivedQuizParser));
        }
        private void receivedQuizParser(PreParser parser)
        {
            if (!(parser is QuizParser)) return;
            foreach(var answer in parser.quizAnswers)
            {
                processQuizAnswer(answer);
                if((answer.targetQuiz == Quiz.targetSlide)&& (answer.answerer == info.credentials.name)) answered = true;
            }
            barGraph.loadResults(results);
            SetBarGraphVisibility();
        }
        private void SetBarGraphVisibility()
        {
            barGraph.Visibility = (answered || Quiz.author == info.credentials.name) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void processQuizAnswer(QuizAnswer answer)
        {
            if (answer.answer == null) return;
            if(answer.targetQuiz != Quiz.targetSlide) return;
            if (!results.ContainsKey(answer.answer)) results[answer.answer] = new List<object>();
            results[answer.answer].Add(answer.answerer);
        }
        private void receiveQuizAnswer(QuizAnswer answer)
        {
            processQuizAnswer(answer);
            if(answer.answerer == info.credentials.name)
                answered = true;
            barGraph.loadResults(results);
            SetBarGraphVisibility();
        }
        private void loaded(object sender, RoutedEventArgs e)
        {
            var adorner = AdornerLayer.GetAdornerLayer(this);
            retrieveQuizResults(Quiz.returnSlide);
            adorner.Add(new UIAdorner(this, new QuizAnswers().SetOptions(options.ToList())));
            adorner.Add(new UIAdorner(this, new AnswerAQuizControl(this)));
            canvasStack.handwriting.DefaultPenAttributes();
            setUpQuizSpace();
        }
        private void setUpQuizSpace()
        {
            var width = container.Width - barGraph.Width;
            var height = container.Height/2;
            var uri = new Uri(Quiz.quizPath, UriKind.RelativeOrAbsolute);
            var image = new Image
                            {
                                Source = new BitmapImage(uri),
                                Width = width,
                                Height = height
                            };
            quizQuestion.Children.Add(image);
            quizQuestion.Height = height;
            canvasStack.Width = container.Width;
            canvasStack.Height = height;
        }

        private static void retrieveQuizResults(int id)
        {
            HistoryProviderFactory.provider.Retrieve<QuizParser>(
                null, 
                null, 
                finishedParser => Commands.PreParserAvailable.Execute(finishedParser), 
                id.ToString());
        }
        public void Submit()
        {
            if (selectedOption == null)
            {
                MessageBox.Show("Please select an option");
                return;
            }
            var quizAnswer = new QuizAnswer
             {
                 answer = selectedOption.index.ToString(), 
                 answerURL = "DONT CLICK ME",
                 targetQuiz = Quiz.targetSlide,
                 answerer = info.credentials.name,
                 quizParent = Quiz.returnSlide
             };
            if(!answered)
                Commands.SendQuizAnswer.Execute(quizAnswer);
            else
                MessageBox.Show("Can only answer a quiz once");
            Cancel();
        }
        public void Cancel()
        {
            ((Panel)Parent).Children.Remove(this);
        }
        private void addOption()
        {
            int index = options.Count()+1;
            options.Add(new Option {index=index});
        }
        public AnswerAQuiz SetQuestionCount(int count)
        {
            for (int i = 0; i < count; i++)
                addOption();
            return this;
        }
    }
}
