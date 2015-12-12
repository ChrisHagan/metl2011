using System.Windows;

namespace SandRibbon.Quizzing
{
    public partial class DisplayAQuiz : Window
    {
        public DisplayAQuiz(MeTLLib.DataTypes.QuizQuestion thisQuiz)
        {
            DataContext = thisQuiz;
            InitializeComponent();
        }
    }
}
