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
using SandRibbonInterop;

namespace SandRibbon.Quizzing
{
    public partial class AssessAQuiz : Window
    {
        public AssessAQuiz()
        {
            InitializeComponent();
        }
        public AssessAQuiz(List<QuizAnswer> answers, QuizQuestion question) : this() 
        {
            resultDisplay.DataContext = question.options.Select(o =>{
                var relevant = answers.Where(a=>a.answer==o.name);
                    return new DisplayableResultSet
                    {
                        color = o.color,
                        count = relevant.Count(),
                        percentage = relevant.Count() / answers.Count() * 100,
                        tooltip = o.optionText
                    };
                }
            );
            allResults.DataContext = question;
        }
    }
    public class DisplayableResultSet 
    {
        public Color color { get; set; }
        public int count { get; set; }
        public int percentage { get; set; }
        public string tooltip { get; set; }
        public string label {
            get {
                return string.Format("{0}% ({1} responses)", percentage, count);
            }
        }
    }
}
