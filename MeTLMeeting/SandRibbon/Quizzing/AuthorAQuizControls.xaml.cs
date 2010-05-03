using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Quizzing
{
    public partial class AuthorAQuizControls : UserControl
    {
        public AuthorAQuizControls()
        {
            InitializeComponent();
        }
        private AuthorAQuiz parent;
        public AuthorAQuizControls(AuthorAQuiz quiz) : this()
        {
            parent = quiz;
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
