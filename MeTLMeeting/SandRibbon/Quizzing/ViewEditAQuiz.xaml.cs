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
using SandRibbon.Components.Utility;

namespace SandRibbon.Quizzing
{
    public partial class ViewEditAQuiz : Window
    {
        private Thickness quizOptionsBorderThickness;

        #region DependencyProperties

        public static readonly DependencyProperty EditedQuizProperty = DependencyProperty.Register("EditedQuiz",
                                                                                                   typeof (QuizQuestion),
                                                                                                   typeof (ViewEditAQuiz));

        public QuizQuestion EditedQuiz
        {
            get { return (QuizQuestion) GetValue(EditedQuizProperty); }
            set { SetValue(EditedQuizProperty, value); }
        }

        public static readonly DependencyProperty OptionErrorProperty = DependencyProperty.Register("OptionError",
                                                                                                    typeof (bool),
                                                                                                    typeof (ViewEditAQuiz));

        public bool OptionError
        {
            get { return (bool) GetValue(OptionErrorProperty); }
            set { SetValue(OptionErrorProperty, value); }
        }

        public static readonly DependencyProperty QuestionErrorProperty = DependencyProperty.Register("QuestionError",
                                                                                                      typeof (bool),
                                                                                                      typeof (ViewEditAQuiz));

        public bool QuestionError
        {
            get { return (bool) GetValue(QuestionErrorProperty); }
            set { SetValue(QuestionErrorProperty, value); }
        }

        public static readonly DependencyProperty ResultsExistProperty = DependencyProperty.Register("ResultsExist",
                                                                                                     typeof (bool),
                                                                                                     typeof (ViewEditAQuiz));

        public bool ResultsExist
        {
            get { return (bool) GetValue(ResultsExistProperty); }
            set { SetValue(ResultsExistProperty, value); }
        }

        #endregion

        public static TitleConverter TitleConverter = new TitleConverter();
        public static QuestionConverter QuestionConverter = new QuestionConverter();
        private MeTLLib.DataTypes.QuizQuestion question
        {
            get {
                return (MeTLLib.DataTypes.QuizQuestion)DataContext;
            }
        }
        public ViewEditAQuiz()
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
        public ViewEditAQuiz(MeTLLib.DataTypes.QuizQuestion thisQuiz)
        {
            DataContext = thisQuiz;
            InitializeComponent();
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
            Closing += AnswerAQuiz_Closing;
            //TeacherControls.Visibility = Globals.isAuthor ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = ((Option)e.AddedItems[0]);
            Commands.SendQuizAnswer.ExecuteAsync(new QuizAnswer(question.Id, Globals.me, selection.name, DateTime.Now.Ticks));
            Trace.TraceInformation("ChoseQuizAnswer {0} {1}", selection.name, question.Id);
            
            //this.Close();
       }

       public void DisplayQuiz(object sender, RoutedEventArgs e)
        {
            //PrepareForRender();
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

            //RestoreAfterRender();
            this.Close();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            question.IsInEditMode = true;
            /*var windowOwner = this; 
            var newQuiz = new QuizQuestion(question.Id, question.Created, question.Title, question.Author,
                                           question.Question, question.Options) {Url = question.Url};
            var editQuiz = new EditQuiz(newQuiz);
            editQuiz.Owner = windowOwner;

            bool? deleteQuiz = editQuiz.ShowDialog();
            if (deleteQuiz ?? false)
            {
                if (MeTLMessage.Question("Really delete quiz?", windowOwner) == MessageBoxResult.Yes)
                {
                    newQuiz.IsDeleted = true;
                    Commands.SendQuiz.Execute(newQuiz);
                    closeMe(null);
                }    
            }*/
        }

        private void deleteQuiz(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void quizCommitButton_Click(object sender, RoutedEventArgs e)
        {
        }

      private void CloseEdit(object sender, RoutedEventArgs e)
      {
          Close();
      }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void updateOptionText(object sender, TextChangedEventArgs e)
        {
        }

        private void RemoveQuizAnswer(object sender, RoutedEventArgs e)
        {

        }
    }
}
