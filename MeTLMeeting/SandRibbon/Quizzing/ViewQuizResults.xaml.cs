using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SandRibbon.Components;
using SandRibbon.Components.Canvas;
using SandRibbon.Providers;
using SandRibbon.Utils.Connection;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;


namespace SandRibbon.Quizzing
{
    public partial class ViewQuizResults : Window
    {
        private Dictionary<long, ObservableCollection<QuizAnswer>> answers = new Dictionary<long, ObservableCollection<QuizAnswer>>();
        private Dictionary<long, AssessAQuiz> assessQuizzes = new Dictionary<long, AssessAQuiz>();
        private ObservableCollection<QuizQuestion> activeQuizes = new ObservableCollection<QuizQuestion>();

        public ViewQuizResults()
        {
            InitializeComponent();
        }
        public ViewQuizResults(Dictionary<long, ObservableCollection<QuizAnswer>> answers, ObservableCollection<QuizQuestion> Quizes): this()
        {
            this.answers = answers;
            foreach(var answer in answers)
                assessQuizzes.Add(answer.Key, new AssessAQuiz(answer.Value, Quizes.Where(q => q.id == answer.Key).First())); 
            foreach(var quiz in Quizes)
                activeQuizes.Add(quiz);
            quizzes.ItemsSource = activeQuizes;
            quizzes.SelectedIndex = 0;
        }
        private void QuizChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.adopt(() =>
                                 {
                                     var thisQuiz = (QuizQuestion) ((ListBox) sender).SelectedItem;
                                     QuizResults.Children.Clear();
                                     QuizResults.Children.Add(assessQuizzes[thisQuiz.id]);
                                 });
        }
        private void DisplayResults(object sender, RoutedEventArgs e)
        {
            var quiz = (AssessAQuiz)QuizResults.Children[0];
            quiz.TimestampLabel.Text = "Results collected at:\r\n" + SandRibbonObjects.DateTimeFactory.Now().ToLocalTime().ToString();
            quiz.SnapshotHost.UpdateLayout();
            var dpi = 96;
            var dimensions = new Rect(0, 0, quiz.ActualWidth, quiz.ActualHeight);
            var bitmap = new RenderTargetBitmap((int)quiz.ActualWidth, (int)quiz.ActualHeight, dpi, dpi, PixelFormats.Default);
            var dv = new DrawingVisual();
            using (var context = dv.RenderOpen())
                context.DrawRectangle(new VisualBrush(quiz.SnapshotHost), null, dimensions);
            bitmap.Render(dv);
            quiz.TimestampLabel.Text = "";
            Commands.QuizResultsAvailableForSnapshot.ExecuteAsync(new UnscaledThumbnailData{id=Globals.slide,data=bitmap});
            this.Close();
        }
    }
}
