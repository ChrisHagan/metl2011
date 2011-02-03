using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonInterop;
using System.Collections.ObjectModel;
using SandRibbon.Components;
using SandRibbon.Providers;
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
            answers.CollectionChanged +=
                (sender, args) =>
                    represent(answers, question);
        }
        private void represent(IEnumerable<MeTLLib.DataTypes.QuizAnswer> answers, MeTLLib.DataTypes.QuizQuestion question)
        {
            Trace.TraceInformation("AssessAQuiz {0}", question.id);
            Dispatcher.adoptAsync(delegate
            {
                responseCount.Content = string.Format("({0} responses)", answers.Count());
                resultDisplay.ItemsSource = question.options.Select(o =>
                {
                    if (answers != null && answers.Count() > 0)
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
                    else return new DisplayableResultSet
                    {

                    };
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