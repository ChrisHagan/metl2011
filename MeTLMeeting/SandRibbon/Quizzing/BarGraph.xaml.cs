using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonInterop;

namespace SandRibbon.Quizzing
{
    public partial class BarGraph : UserControl
    {
        private Dictionary<object, List<object>> results = new Dictionary<object, List<object>>();
        private int quizId;
        public BarGraph()
        {
            InitializeComponent();
        }
        public void setQuiz(int id)
        {
            quizId = id;
        }
        private void recalculateBars()
        {
            var total = results.Values.Sum(v=>v.Count);
            bars.ItemsSource = (from pair in results orderby pair.Key select new Bar { label = pair.Key.ToString(), proportion = pair.Value.Count / (double)total }).ToList();
            
        }
        class Bar
        {
            public string label { get; set; }
            public double proportion { get; set; }
        }

        public void loadResults(Dictionary<object, List<object>> dictionary)
        {
            results = dictionary;
            recalculateBars();
        }
    }
}
