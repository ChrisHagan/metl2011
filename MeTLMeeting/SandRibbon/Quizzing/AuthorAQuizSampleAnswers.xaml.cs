using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SandRibbon.Quizzing
{
    public partial class AuthorAQuizSampleAnswers : UserControl
    {
        private ObservableCollection<Option> options = new ObservableCollection<Option>();
        public AuthorAQuizSampleAnswers()
        {
            InitializeComponent();
            quizOptions.ItemsSource = options;
        }
        public AuthorAQuizSampleAnswers SetOptions(IEnumerable<Option> options)
        {
            this.options.Clear();
            foreach (var option in options)
                this.options.Add(option);
            return this;
        }
    }
}
