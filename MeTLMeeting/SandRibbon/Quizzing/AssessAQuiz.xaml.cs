using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SandRibbon.Quizzing
{
    public partial class AssessAQuiz : UserControl 
    {
        public AssessAQuiz()
        {
            InitializeComponent();
        }
        public AssessAQuiz(ObservableCollection<MeTLLib.DataTypes.QuizAnswer> answers, MeTLLib.DataTypes.QuizQuestion question)
            : this()
        {
            DataContext = question;
            represent(answers, question);
            answers.CollectionChanged += (sender, args) => represent(answers, question);
        }
        private void represent(IEnumerable<MeTLLib.DataTypes.QuizAnswer> answers, MeTLLib.DataTypes.QuizQuestion question)
        {
            if (answers == null || question == null) return;
            Trace.TraceInformation("AssessAQuiz {0}", question.Id);
            Dispatcher.adoptAsync(delegate
            {
                var response = (answers.Count() > 1 || answers.Count() == 0) ? "({0} responses)" : "({0} response)"; 
                responseCount.Content = string.Format(response, answers.Count());
                resultDisplay.ItemsSource = question.Options.Select(o =>
                {
                    if (answers != null)
                    {
                        var relevant = answers.Where(a => a.answer == o.name);
                        return new DisplayableResultSet
                        {
                            color = o.color,
                            count = relevant.Count(),
                            proportion = answers.Count() == 0 ? 0 :
                                (double)relevant.Count() / answers.Count(),
                            tooltip = o.optionText,
                            name = o.name
                        };
                    }
                    return new DisplayableResultSet();
                });
            });
        }
    }
    public class DisplayableResultSet
    {
        public Color color { get; set; }
        public int count { get; set; }
        public double proportion { get; set; }
        public string tooltip { get; set; }
        public string name { get; set; }
        public string percentage
        {
            get
            {
                return string.Format("{0:0.00}%", proportion * 100);
            }
        }
    }
}