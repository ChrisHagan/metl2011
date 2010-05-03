using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Quizzing
{
    public partial class AnswerAQuizControl : UserControl
    {
        public AnswerAQuizControl()
        {
            InitializeComponent();
        }
        private AnswerAQuiz parent;
        public AnswerAQuizControl(AnswerAQuiz parent): this()
        {
            this.parent = parent;
        }
        private void submit(object sender, RoutedEventArgs e)
        {
            parent.Submit(); 
        }
        private void cancel(object sender, RoutedEventArgs e)
        {
            parent.Cancel(); 
        }
    }
}
