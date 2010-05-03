using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbonInterop;

namespace SandRibbon.Quizzing
{
    public partial class PollListing : UserControl
    {
        ObservableCollection<QuizDetails> quizzes = new ObservableCollection<QuizDetails>();
        public PollListing()
        {
            InitializeComponent();
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<QuizDetails>(ReceiveQuiz));
            this.polls.ItemsSource = quizzes;
        }
        private void JoinConversation(string title)
        {
            var polls = PollProviderFactory.Provider.ListPolls(title).Select(p=>p.id);
            HistoryProviderFactory.provider.Retrieve<QuizParser>(null,null,
            parser=>{
                Dispatcher.BeginInvoke((Action)delegate{
                    quizzes.Clear();
                    foreach(var quiz in parser.quizs.Where(q => polls.Contains(q.targetSlide)))
                        quizzes.Add(quiz);
                });
            }, title);
        }
        private void ReceiveQuiz(QuizDetails details)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if(quizzes.Where(q=>q.target == details.target).Count() == 0)
                    quizzes.Add(details);
            });
        }
        private void GoToAnyQuiz(object sender, RoutedEventArgs e)
        {//Navigates to a quiz even if we are on the wrong slide
            Commands.MoveToQuiz.Execute((QuizDetails)((FrameworkElement)sender).DataContext); 
        }
    }
}