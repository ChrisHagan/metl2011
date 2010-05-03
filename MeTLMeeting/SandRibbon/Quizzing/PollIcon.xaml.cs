using System.Windows;
using System.Windows.Controls;
using SandRibbonInterop;

namespace SandRibbon.Quizzing
{
    public partial class PollIcon : UserControl
    {
        public QuizDetails Quiz
        {
            get { return (QuizDetails)GetValue(quizProperty); }
            set { 
                SetValue(quizProperty, value); 
                button.CommandParameter = value; 
                Commands.RequerySuggested(Commands.MoveToQuiz);
            }
        }
        public static readonly DependencyProperty quizProperty =
            DependencyProperty.Register("Quiz", typeof(QuizDetails), typeof(PollIcon), new UIPropertyMetadata(null));
        public PollIcon()
        {
            InitializeComponent();
        }
        public PollIcon(QuizDetails details) : this()
        {
            this.Quiz = details;
        }
    }
}
