using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using MeTLLib.DataTypes;

namespace SandRibbon.Quizzing
{
    public partial class CreateAQuiz : Window
    {
        public static readonly string PROMPT_TEXT = "Please enter a quiz title";
        private string url = "none";
        public static ObservableCollection<Option> options = new ObservableCollection<Option>
                                                     {
                                                         new Option("A"," ",false,Colors.Blue),
                                                         new Option("B"," ",false,Colors.Red)
                                                     };
        public CreateAQuiz(int count)
        {
            InitializeComponent();
            quizTitle.Text = string.Format("Quiz {0}", count + 1);
            options.First().color = AllColors.all[0];
            quizTitle.GotFocus += selectAll;
            quizTitle.GotMouseCapture += selectAll;
            quizTitle.GotKeyboardFocus += selectAll;
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(joinConversation));
        }

        private void joinConversation(object obj)
        {
            Commands.JoinConversation.UnregisterCommand(new DelegateCommand<object>(joinConversation));
            Close();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Commands.JoinConversation.UnregisterCommand(new DelegateCommand<object>(joinConversation));
            this.Close();
        }
        private void canCreateQuizQuestion(object sender, CanExecuteRoutedEventArgs e)
        {
            if (quizTitle == null) return;
            var quizTitleIsntDefault = quizTitle.Text != PROMPT_TEXT;
            var activeOptions = options.Where(o => o.optionText.Length > 0).ToList();
            e.CanExecute = (quizTitle != null && quizTitleIsntDefault) && activeOptions.Count >= 2;
        }
        private void CreateQuizQuestion(object sender, ExecutedRoutedEventArgs e)
        {
            var quiz = new QuizQuestion(SandRibbonObjects.DateTimeFactory.Now().Ticks, quizTitle.Text, Globals.me, question.Text, new List<Option>());
            quiz.url = url;
            foreach (object obj in quizQuestions.Items)
            {
                var answer = (Option)obj;
                if (!string.IsNullOrEmpty(answer.optionText))
                    quiz.options.Add(answer);
            }
            Commands.SendQuiz.ExecuteAsync(quiz);
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
                    co.optionText = " ";
                }
                AddNewEmptyOption();
            }
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
        private bool shouldAddNewEmptyOption()
        {
            var emptyOptions = options.Where(o => string.IsNullOrEmpty(o.optionText));
            if (emptyOptions.Count() == 0) return true;
            return false;
        }

        private const int alphabetLength = 26;
        private void AddNewEmptyOption()
        {
            if (!shouldAddNewEmptyOption()) return;
            foreach (var option in options)
            {
                var container = ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(option)); 
                if (container != null) container.Opacity = 1;
            }
            var newName = "A";
            var newIndex = 1;
            if (options.Count > 0)
            {
                var temp = new String(new[] { (char)(options.Last().name.ToCharArray()[options.Last().name.Length - 1] + 1) }).ToUpper();
                if (temp.ToCharArray()[0] <= 90)
                    newName = temp;

                if (options.Count >= alphabetLength)
                {
                    var prefix = new string(new char[] { (char)("A".ToCharArray()[0] + ((options.Count / alphabetLength) - 1)) }).ToUpper();
                    newName = string.Format("{0}{1}", prefix, newName);
                }
                newIndex = AllColors.all.IndexOf(options.Last().color) + 1;
            }
            var newOption = new Option(newName, "", false, AllColors.all.ElementAt(newIndex));
            if (shouldAddNewEmptyOption())
            {
                options.Add(newOption);
                var container = ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(newOption));
                if (container != null) container.Opacity = 0.5;
            }
            Commands.RequerySuggested();
        }
        private void RemoveQuizAnswer(object sender, RoutedEventArgs e)
        {
            var owner = ((FrameworkElement)sender).DataContext;
            options.Remove((Option)owner);
            var size = options.Count;
            var newList = new List<Option>();
            foreach (var obj in options)
                newList.Add(obj);
            options.Clear();

            var name = "A";
            foreach (var option in newList)
            {
                if (option.name == option.optionText)
                    option.optionText = name;
                option.name = name;
                var temp = new String(new[] { (char)(name.ToCharArray()[name.Length - 1] + 1) }).ToUpper();
                if (temp.ToCharArray()[0] <= 90)
                    name = temp;
                else
                    name = "A";

                if (options.Count + 1 >= alphabetLength)
                {
                    var offset = (options.Count / alphabetLength) - 1;
                    if (offset < 0)
                        offset = 0;
                    var prefix = new string(new char[] { (char)("A".ToCharArray()[0] + offset) }).ToUpper();
                    name = string.Format("{0}{1}", prefix, name);
                }
                options.Add(option);
            }
            AddNewEmptyOption();
            foreach (var obj in options)
                if (!(obj.optionText.Length > 0))
                    ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(obj)).Opacity = 0.5;
            CommandManager.InvalidateRequerySuggested();
        }
        private void screenshotAsAQuestion(object sender, RoutedEventArgs e)
        {
            DelegateCommand<string> gotScreenshot = null;
            gotScreenshot = new DelegateCommand<string>(hostedFilename =>
                            {
                                Dispatcher.adopt(() =>
                                {
                                    Commands.ScreenshotGenerated.UnregisterCommand(gotScreenshot);
                                    
                                    url = MeTLLib.ClientFactory.Connection().NoAuthUploadResource(new Uri(hostedFilename, UriKind.RelativeOrAbsolute), Int32.Parse(Globals.conversationDetails.Jid)).ToString();
                                    var image = new Image();
                                    BitmapImage source = new BitmapImage();
                                    source.BeginInit();
                                    source.UriSource = new Uri(url);
                                    source.EndInit();
                                    image.Source = source;
                                    image.Width = 300;
                                    image.Height = 300;
                                    questionSnapshotContainer.Children.Add(image);
                                    screenshot.Visibility = Visibility.Collapsed;
                                    var slide = Globals.slides.Where(s => s.id == Globals.slide).First();
                                    if(isAutogeneratedTitle(quizTitle.Text))
                                        quizTitle.Text = string.Format("Quiz referencing slide {0}", slide.index + 1);
                                });

                            });
            Commands.ScreenshotGenerated.RegisterCommand(gotScreenshot);
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
                                                    {
                                                        time = SandRibbonObjects.DateTimeFactory.Now().Ticks,
                                                        message = ""
                                                    });
        }

        private bool isAutogeneratedTitle(string text)
        {
            var generatedTitle = new Regex("Quiz [0-9]+$");
            return generatedTitle.IsMatch(text);
        }

        private void selectAll(object sender, RoutedEventArgs e)
        {
            var origin = ((TextBox)sender);
            origin.SelectAll();
        }
        private void refreshCollection()
        {
            options = new ObservableCollection<Option>
                                                     {
                                                         new Option("A"," ",false, Colors.Blue),
                                                         new Option("B"," ",false, Colors.Red)
                                                     };
        }
        private void createAQuiz_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            refreshCollection();
            Commands.UnblockInput.ExecuteAsync(null);
        }

        private void createAQuiz_Loaded(object sender, RoutedEventArgs e)
        {
            AddNewEmptyOption();
        }
    }
}