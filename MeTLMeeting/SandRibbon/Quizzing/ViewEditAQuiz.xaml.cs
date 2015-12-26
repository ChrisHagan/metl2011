using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MeTLLib.DataTypes;
using SandRibbon.Components;
using System.Windows.Input;
using System.Windows.Data;
using SandRibbon.Components.Utility;
using SandRibbon.Pages;
using System.IO;
using SandRibbon.Pages.Collaboration;

namespace SandRibbon.Quizzing
{
    [ValueConversion(typeof(string), typeof(bool))]
    public class StringNullOrEmptyToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var valueString = value as string;
            return String.IsNullOrEmpty(valueString) || valueString == "none";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return String.IsNullOrEmpty(value as String) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class IndexOfItemToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var collection = values[0] as ObservableCollection<Option>;
            var item = values[1] as Option;
            return collection != null && collection.IndexOf(item) > 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ViewEditAQuiz : Page
    {
        #region DependencyProperties

        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(ViewEditAQuiz), new PropertyMetadata(false));

        public static readonly DependencyProperty OptionErrorProperty = DependencyProperty.Register("OptionError", typeof(bool), typeof(ViewEditAQuiz));

        public bool OptionError
        {
            get { return (bool)GetValue(OptionErrorProperty); }
            set { SetValue(OptionErrorProperty, value); }
        }

        public static readonly DependencyProperty QuestionErrorProperty = DependencyProperty.Register("QuestionError", typeof(bool), typeof(ViewEditAQuiz));

        public bool QuestionError
        {
            get { return (bool)GetValue(QuestionErrorProperty); }
            set { SetValue(QuestionErrorProperty, value); }
        }

        public static readonly DependencyProperty ResultsExistProperty = DependencyProperty.Register("ResultsExist", typeof(bool), typeof(ViewEditAQuiz));

        public bool ResultsExist
        {
            get { return (bool)GetValue(ResultsExistProperty); }
            set { SetValue(ResultsExistProperty, value); }
        }

        #endregion


        public QuizQuestion question
        {
            get { return (QuizQuestion)GetValue(questionProperty); }
            set { SetValue(questionProperty, value); }
        }

        public static readonly DependencyProperty questionProperty =
            DependencyProperty.Register("question", typeof(QuizQuestion), typeof(ViewEditAQuiz), new PropertyMetadata(null));

        private SlideAwarePage rootPage { get; set; }
        public ViewEditAQuiz(QuizQuestion thisQuiz, SlideAwarePage rootPage)
        {
            this.rootPage = rootPage;
            question = thisQuiz;
            DataContext = this;

            InitializeComponent();

            populateImagePreview();
            Loaded += delegate
            {
                question.Options.CollectionChanged += UpdateOptionError;
            };
            Unloaded += delegate
            {
                question.Options.CollectionChanged -= UpdateOptionError;
            };
            ResultsExist = CheckResultsExist(question);
            EditMode = false;
            if (rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name))
            {
                EditMode = true;
                TeacherControls.Visibility = Visibility.Visible;
            }
        }

        private void populateImagePreview()
        {
            if (String.IsNullOrEmpty(question.Url))
            {
                var bytes = rootPage.NetworkController.client.resourceProvider.secureGetData(new Uri(question.Url, UriKind.Absolute));
                Dispatcher.adopt(() =>
                {
                    var image = new Image();

                    BitmapImage source = new BitmapImage();
                    source.BeginInit();
                    source.StreamSource = new MemoryStream(bytes);
                    source.EndInit();

                    image.Source = source;
                    image.Width = 300;
                    image.Height = 300;

                    imagePreviewContainer.Children.Add(image);
                });
            }
        }

        private void UpdateOptionError(object sender, NotifyCollectionChangedEventArgs e)
        {
            var optionList = sender as ObservableCollection<Option>;
            OptionError = (e.Action == NotifyCollectionChangedAction.Remove && optionList.Count < 3);
        }

        public bool CheckResultsExist(QuizQuestion quizQuestion)
        {
            return rootPage.ConversationState.QuizData.answers.FirstOrDefault(answer => answer.Key == quizQuestion.Id).Value.Count > 0;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!question.IsInEditMode)
            {
                var selection = ((Option)e.AddedItems[0]);
                rootPage.NetworkController.client.SendQuizAnswer(new QuizAnswer(question.Id, rootPage.NetworkController.credentials.name,
                    selection.name, DateTime.Now.Ticks), rootPage.ConversationDetails.Jid);
                Trace.TraceInformation("ChoseQuizAnswer {0} {1}", selection.name, question.Id);
            }
        }

        public void DisplayQuiz(object sender, RoutedEventArgs e)
        {
            var quizDisplay = new DisplayAQuiz(question);
            /*We're literally snapshotting this element*/
            quizDisplay.Show();

            try
            {
                var quiz = quizDisplay.SnapshotHost;
                var dpi = 96;
                var dimensions = ResizeHelper.ScaleMajorAxisToCanvasSize(quiz);
                var bitmap = new RenderTargetBitmap((int)dimensions.Width, (int)dimensions.Height, dpi, dpi, PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var context = dv.RenderOpen())
                    context.DrawRectangle(new VisualBrush(quiz), null, dimensions);
                bitmap.Render(dv);
                var frame = BitmapFrame.Create(bitmap);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(frame);
                var tmpFile = Path.GetTempFileName();
                using (var w = new StreamWriter(tmpFile))
                {
                    encoder.Save(w.BaseStream);
                }
                var identity = rootPage.NetworkController.client.resourceUploader.uploadResource(rootPage.Slide.id.ToString(), "tmp", tmpFile);
                rootPage.NetworkController.client.JoinRoom(rootPage.Slide.id.ToString());
                rootPage.NetworkController.client.SendImage(new TargettedImage(
                   rootPage.Slide.id,
                   rootPage.NetworkController.credentials.name,
                   "presentationSpace",
                   Privacy.Public,
                   identity,
                   0,
                   0,
                   frame.Width,
                   frame.Height,
                   identity,
                   -1L
                   ));

                NavigationService.Navigate(new RibbonCollaborationPage(
                    rootPage.UserGlobalState,
                    rootPage.UserServerState,
                    rootPage.UserConversationState,
                    rootPage.ConversationState,
                    rootPage.UserSlideState,
                    rootPage.NetworkController,
                    rootPage.ConversationDetails,
                    rootPage.Slide));
            }
            finally
            {
                quizDisplay.Close();
            }
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            EditMode = !EditMode;
        }

        private void QuizButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                tryPrefillOption((FrameworkElement)sender);
            }
        }

        private void tryPrefillOption(FrameworkElement sender)
        {
            var currentOption = sender.DataContext;
            if (currentOption is Option)
            {
                var co = ((Option)currentOption);
                if (string.IsNullOrEmpty(co.optionText))
                {
                    co.optionText = String.Empty;
                }
                AddNewEmptyOption();
            }
        }

        private void deleteQuiz(object sender, RoutedEventArgs e)
        {
            var windowOwner = Window.GetWindow(this);
            if (MeTLMessage.Question("Really delete quiz?", windowOwner) == MessageBoxResult.Yes)
            {
                question.Delete();
                Commands.SendQuiz.Execute(question);
            }
        }

        private void quizCommitButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidQuiz(question))
            {
                question.RemoveEmptyOptions();
                question.Created = SandRibbonObjects.DateTimeFactory.Now().Ticks;
                Commands.SendQuiz.Execute(question);
                question.EndEdit();
            }
        }

        private bool ValidQuiz(QuizQuestion editedQuiz)
        {
            var questionError = false;
            var optionError = false;
            var questionValid = question.Validate(out questionError, out optionError);
            QuestionError = questionError;
            OptionError = optionError;

            return questionValid;
        }

        private void CloseEdit(object sender, RoutedEventArgs e)
        {
            question.CancelEdit();
            question.RemoveEmptyOptions();
        }

        private void updateOptionText(object sender, TextChangedEventArgs e)
        {
            var text = ((TextBox)sender).Text;
            var option = (Option)((FrameworkElement)sender).DataContext;
            if (!String.IsNullOrEmpty(text) || option.optionText != text)
            {
                option.optionText = text;
                AddNewEmptyOption();
            }
        }

        private void RemoveQuizAnswer(object sender, RoutedEventArgs e)
        {
            var owner = ((FrameworkElement)sender).DataContext;
            question.Options.Remove((Option)owner);

            // relabel the option names
            for (int i = 0; i < question.Options.Count; i++)
            {
                //options[i].name = Option.GetOptionNameFromIndex(i);
                question.Options[i].color = generateColor(i);
            }

            AddNewEmptyOption();
            CommandManager.InvalidateRequerySuggested();
        }

        private bool shouldAddNewEmptyOption()
        {
            var emptyOptions = question.Options.Where(o => string.IsNullOrEmpty(o.optionText));
            return emptyOptions.Count() == 0;
        }

        private Color generateColor(int index)
        {
            return index % 2 == 0 ? Colors.White : (Color)ColorConverter.ConvertFromString("#FF4682B4");
        }

        private void AddNewEmptyOption()
        {
            if (!shouldAddNewEmptyOption()) return;

            var newIndex = 1;
            var newName = Option.GetOptionNameFromIndex(0);
            if (question.Options.Count > 0)
            {
                var lastOption = question.Options.Last();
                newIndex = question.Options.IndexOf(lastOption) + 1;
                newName = Option.GetNextOptionName(lastOption.name);
            }
            var newOption = new Option(newName, String.Empty, false, generateColor(newIndex));
            if (shouldAddNewEmptyOption())
            {
                question.Options.Add(newOption);
            }
            Commands.RequerySuggested();
        }
    }
}
