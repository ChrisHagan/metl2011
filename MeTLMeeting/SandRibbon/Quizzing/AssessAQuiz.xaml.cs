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
using System.Collections.ObjectModel;
using SandRibbon.Components;
using SandRibbon.Providers;

namespace SandRibbon.Quizzing
{
    public partial class AssessAQuiz : Window
    {
        public AssessAQuiz()
        {
            InitializeComponent();
        }
        public AssessAQuiz(ObservableCollection<QuizAnswer> answers, QuizQuestion question) : this() 
        {
            DataContext = question;
            represent(answers, question);
            answers.CollectionChanged += 
                (sender,args)=>
                    represent(answers, question);
        }
        private void represent(IEnumerable<QuizAnswer> answers, QuizQuestion question)
        {
            responseCount.Content = string.Format("({0} responses)",answers.Count());
            resultDisplay.ItemsSource = question.options.Select(o =>{
                var relevant = answers.Where(a=>a.answer==o.name);
                return new DisplayableResultSet
                {
                    color = o.color,
                    count = relevant.Count(),
                    proportion = answers.Count() == 0 ? 0 :
                        (double)relevant.Count() / answers.Count(),
                    tooltip = o.optionText,
                    name=o.name
                };
            });
        }
        private void SnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            TimestampLabel.Text = "Results collected at:\r\n"+SandRibbonObjects.DateTimeFactory.Now().ToLocalTime().ToString();
            SnapshotHost.UpdateLayout();
            var dpi = 96;
            var dimensions = new Rect(0, 0, ActualWidth, ActualHeight);
            var bitmap = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, dpi, dpi, PixelFormats.Default);
            var dv = new DrawingVisual();
            using (var context = dv.RenderOpen())
                context.DrawRectangle(new VisualBrush(SnapshotHost), null, dimensions);
            bitmap.Render(dv);
            TimestampLabel.Text = "";
            Commands.QuizResultsAvailableForSnapshot.Execute(new UnscaledThumbnailData{id=Globals.slide,data=bitmap});
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
    public class DisplayableResultSet 
    {
        public Color color { get; set; }
        public int count { get; set; }
        public double proportion { get; set; }
        public string tooltip { get; set; }
        public string name { get; set; }
        public string percentage { get {
            return string.Format("{0:0.00}%", proportion * 100);
        } }
    }
}