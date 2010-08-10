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
using SandRibbon.Components.Submissions;
using SandRibbon.Providers;
using SandRibbonInterop;
using CheckBox = System.Windows.Controls.CheckBox;
using System.Collections.ObjectModel;
using WPFColors = System.Windows.Media.Colors;

namespace SandRibbon.Quizzing
{
    public partial class CreateAQuiz : Window
    {
        public static readonly string PROMPT_TEXT = "Please enter a quiz title";
        private string url = "none";
        public static ObservableCollection<Option> options = new ObservableCollection<Option>
                                                     {
                                                         new Option {name = "A",optionText = "A"},
                                                         new Option {name = "B",optionText = "B"}
                                                     };
        public CreateAQuiz(int count)
        {
            InitializeComponent();
            quizTitle.Text = string.Format("Quiz {0}", count + 1);
            options.First().color = AllColors.all[0];
            quizTitle.GotFocus += selectAll;
            quizTitle.GotMouseCapture += selectAll;
            quizTitle.GotKeyboardFocus += selectAll;
        }
        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void canCreateQuizQuestion(object sender, CanExecuteRoutedEventArgs e)
        {
            if(quizTitle == null) return;
            var quizTitleIsntDefault = quizTitle.Text != CreateAQuiz.PROMPT_TEXT ;
            var activeOptions = options.Where(o => o.optionText.Length > 0).ToList();
            e.CanExecute = (quizTitle != null && quizTitleIsntDefault) && activeOptions.Count >= 2;
        }
        private void CreateQuizQuestion(object sender, ExecutedRoutedEventArgs e)
        {
            var quiz = new QuizQuestion { title = quizTitle.Text, url = url, question = question.Text, author = Globals.me, id = DateTime.Now.Ticks };
            foreach (object obj in quizQuestions.Items)
            {
                var answer = (Option)obj;
                if (!string.IsNullOrEmpty(answer.optionText))
                    quiz.options.Add(answer);
            }
            Commands.SendQuiz.Execute(quiz);
            this.Close();
        }
        private void QuizButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                tryPrefillOption((FrameworkElement)sender);
            }
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
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
                    co.optionText = co.name;
                }
                AddNewEmptyOption();
            }
        }
        private bool shouldAddNewEmptyOption()
        {
            var emptyOptions = options.Where(o => string.IsNullOrEmpty(o.optionText));
            if (emptyOptions.Count() == 0) return true;
            return false;
        }
        private void AddNewEmptyOption()
        {
            if (!shouldAddNewEmptyOption()) return;
            foreach (var option in options)
                ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(option)).Opacity = 1;
            var newOption = new Option
            {
                name = new String(new[]{
                    (char)(options.Last().name.ToCharArray()[0]+1)
                }).ToUpper(),
                color = AllColors.all.ElementAt(AllColors.all.IndexOf(options.Last().color) + 1)
            };
            //newOption.optionText = newOption.name;
            if (shouldAddNewEmptyOption())
            {
                options.Add(newOption);
                ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(newOption)).Opacity = 0.5;
            }
            Commands.RequerySuggested();
        }
        private void RemoveQuizAnswer(object sender, RoutedEventArgs e)
        {
            var owner = ((FrameworkElement)sender).DataContext;
            options.Remove((Option)owner);
            CommandManager.InvalidateRequerySuggested();
        }
        private void screenshotAsAQuestion(object sender, RoutedEventArgs e)
        {
            DelegateCommand<string> gotScreenshot = null;
            gotScreenshot = new DelegateCommand<string>(hostedFilename =>
                            {
                                Commands.ScreenshotGenerated.UnregisterCommand(gotScreenshot);
                                url = hostedFilename;
                                var image = new Image();
                                BitmapImage source = new BitmapImage();
                                source.BeginInit();
                                source.UriSource = new Uri(hostedFilename);
                                source.EndInit();
                                image.Source = source;
                                image.Width = 300;
                                image.Height = 300;
                                questionSnapshotContainer.Children.Add(image);
                                var slide = Globals.slides.Where(s => s.id == Globals.slide).First();
                                quizTitle.Text = string.Format("Quiz referencing slide {0}", slide.index + 1);
                            });
            Commands.ScreenshotGenerated.RegisterCommand(gotScreenshot);
            Commands.GenerateScreenshot.Execute(new ScreenshotDetails
                                                    {
                                                        time = DateTime.Now.Ticks,
                                                        message = ""
                                                    });
        }

        private void selectAll(object sender, RoutedEventArgs e)
        {
            quizTitle.SelectAll();

        }
        private void refreshCollection()
        {
            options = new ObservableCollection<Option>
                                                     {
                                                         new Option {name = "A",optionText = "A"},
                                                         new Option {name = "B",optionText = "B"}
                                                     };
        }
        private void createAQuiz_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            refreshCollection();
            Commands.UnblockInput.Execute(null);
        }

        private void createAQuiz_Loaded(object sender, RoutedEventArgs e)
        {
            AddNewEmptyOption();
        }

    }
}