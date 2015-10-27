using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SandRibbon.Components;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Diagnostics;
using System;


namespace SandRibbon.Quizzing
{
    public partial class ViewQuizResults : Window
    {
        private Brush quizBorderBackground;
        private Dictionary<long, ObservableCollection<QuizAnswer>> answers = new Dictionary<long, ObservableCollection<QuizAnswer>>();
        private Dictionary<long, AssessAQuiz> assessQuizzes = new Dictionary<long, AssessAQuiz>();
        private ObservableCollection<QuizQuestion> activeQuizes = new ObservableCollection<QuizQuestion>();

        public ViewQuizResults()
        {
            InitializeComponent();
            Closing += new System.ComponentModel.CancelEventHandler(ViewQuizResults_Closing);
        }
        void ViewQuizResults_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppCommands.UnblockInput.Execute(null);
        }
        public ViewQuizResults(Dictionary<long, ObservableCollection<QuizAnswer>> answers, ObservableCollection<QuizQuestion> Quizes): this()
        {
            if (Quizes.Count < 1) return;
            this.answers = answers;
            foreach(var answer in answers)
                assessQuizzes.Add(answer.Key, new AssessAQuiz(answer.Value, Quizes.Where(q => q.Id == answer.Key).FirstOrDefault())); 
            foreach(var quiz in Quizes)
                activeQuizes.Add(quiz);
            quizzes.ItemsSource = activeQuizes;
            if (quizzes.Items.Count > 0)
                quizzes.SelectedIndex = 0;
            Trace.TraceInformation("ViewingQuizResults");
        }
        private void QuizChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.adopt(() =>
                                 {
                                     var thisQuiz = (QuizQuestion) ((ListBox) sender).SelectedItem;
                                     QuizResults.Children.Clear();
                                     QuizResults.Children.Add(assessQuizzes[thisQuiz.Id]);
                                 });
        }

        private void PrepareForRender()
        {
            var quiz = (AssessAQuiz)QuizResults.Children[0];
            quizBorderBackground = QuizBorder.Background;
            QuizBorder.Background = Brushes.Transparent;
            QuizResultsBorder.Background = Brushes.Transparent;
            quiz.SnapshotHost.Background = Brushes.Transparent;
        }

        private void RestoreAfterRender()
        {
            var quiz = (AssessAQuiz)QuizResults.Children[0];
            QuizBorder.Background = quizBorderBackground;
            QuizResultsBorder.Background = quizBorderBackground;
            quiz.SnapshotHost.Background = quizBorderBackground;
        }

        private void DisplayResults(object sender, RoutedEventArgs e)
        {
            PrepareForRender();

            this.Hide();
            var quiz = (AssessAQuiz)QuizResults.Children[0];
            quiz.TimestampLabel.Text = "Results collected at:\r\n" + SandRibbonObjects.DateTimeFactory.Now().ToLocalTime().ToString();
            quiz.SnapshotHost.UpdateLayout();
            var dpi = 96;
            var dimensions = ResizeHelper.ScaleMajorAxisToCanvasSize(quiz.SnapshotHost);
            var bitmap = new RenderTargetBitmap((int)dimensions.Width, (int)dimensions.Height, dpi, dpi, PixelFormats.Default);
            var dv = new DrawingVisual();
            using (var context = dv.RenderOpen())
                context.DrawRectangle(new VisualBrush(quiz.SnapshotHost), null, dimensions);
            bitmap.Render(dv);
            quiz.TimestampLabel.Text = "";
            AppCommands.QuizResultsAvailableForSnapshot.ExecuteAsync(new UnscaledThumbnailData{id=Globals.slide,data=bitmap});

            RestoreAfterRender();

            Trace.TraceInformation("DisplayingQuiz");
            this.Close();
        }
    }
}
