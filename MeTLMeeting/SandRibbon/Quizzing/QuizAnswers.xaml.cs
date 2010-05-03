using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SandRibbon.Quizzing
{
    public partial class QuizAnswers : UserControl
    {
        private ObservableCollection<Option> options = new ObservableCollection<Option>();
        public QuizAnswers()
        {
            InitializeComponent();
            quizOptions.ItemsSource = options;
            quizOptions.SelectionChanged += optionSelectionChanged;
        }

        private void optionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Commands.SetQuizOptions.Execute(quizOptions.SelectedItem);
        }

        public QuizAnswers SetOptions(IEnumerable<Option> options)
        {
            this.options.Clear();
            foreach (var option in options)
                this.options.Add(option);
            return this;

        }
    }
}
