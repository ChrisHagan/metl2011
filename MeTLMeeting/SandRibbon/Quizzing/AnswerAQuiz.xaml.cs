using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Diagnostics;

namespace SandRibbon.Quizzing
{
    public partial class AnswerAQuiz : Window
    {
        private Thickness quizOptionsBorderThickness;

        public static TitleConverter TitleConverter = new TitleConverter();
        public static QuestionConverter QuestionConverter = new QuestionConverter();
        private MeTLLib.DataTypes.QuizQuestion question
        {
            get {
                return (MeTLLib.DataTypes.QuizQuestion)DataContext;
            }
        }
        public AnswerAQuiz()
        {
            InitializeComponent();
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
            Closing += AnswerAQuiz_Closing;
        }

        void AnswerAQuiz_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Commands.UnblockInput.Execute(null);
        }

        private void closeMe(object obj)
        {
            Close();
        }
        public AnswerAQuiz(MeTLLib.DataTypes.QuizQuestion thisQuiz)
            : this()
        {
            DataContext = thisQuiz;
            TeacherControls.Visibility = Globals.isAuthor ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = ((Option)e.AddedItems[0]);
            Commands.SendQuizAnswer.ExecuteAsync(new QuizAnswer(question.id, Globals.me, selection.name, DateTime.Now.Ticks));
            Trace.TraceInformation("ChoseQuizAnswer {0} {1}", selection.name, question.id);
            
            this.Close();
       }

        private void PrepareForRender()
        {
            quizOptionsBorderThickness = quizOptions.BorderThickness;
            quizOptions.BorderThickness = new Thickness(0);
        }

        private void RestoreAfterRender()
        {
            quizOptions.BorderThickness = quizOptionsBorderThickness;
        }
        
       public void DisplayQuiz(object sender, RoutedEventArgs e)
        {
            PrepareForRender();
            var quiz = SnapshotHost;
            quiz.UpdateLayout();
            var dpi = 96;
            var dimensions = ResizeHelper.ScaleMajorAxisToCanvasSize(quiz);
            var bitmap = new RenderTargetBitmap((int)dimensions.Width, (int)dimensions.Height, dpi, dpi, PixelFormats.Default);
            var dv = new DrawingVisual();
            using (var context = dv.RenderOpen())
                context.DrawRectangle(new VisualBrush(quiz), null, dimensions);
            bitmap.Render(dv);
            Commands.QuizResultsAvailableForSnapshot.ExecuteAsync(new UnscaledThumbnailData{id=Globals.slide,data=bitmap});

            RestoreAfterRender();
            this.Close();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var newQuiz = new QuizQuestion(question.id, question.created, question.title, question.author,
                                           question.question, question.options) {url = question.url};
            var editQuiz = new EditQuiz(newQuiz);
            editQuiz.Owner = Window.GetWindow(this);
            editQuiz.ShowDialog();
            closeMe(null);
        }
    }
    public class TitleConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format("Title: {0}", value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class QuestionConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var question = (string)value;
            return string.IsNullOrEmpty(question)? "" :string.Format("Question: {0}", question);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
