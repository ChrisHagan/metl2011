using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Submissions;
using SandRibbon.Providers;
using System.Collections.ObjectModel;
using MeTLLib.DataTypes;
using System.Diagnostics;
using System.Xml.Linq;

namespace SandRibbon.Quizzing
{
    public partial class CreateAQuiz : Window
    {
        private string identity = "";
        public static ObservableCollection<Option> options = new ObservableCollection<Option>
                                                     {
                                                         new Option("A",String.Empty,false,Colors.Blue)
                                                     };
        public CreateAQuiz(int count)
        {
            InitializeComponent();
            question.Text = string.Format("Poll {0}", count + 1);
            options.First().color = Colors.White;
            question.GotFocus += selectAll;
            question.GotMouseCapture += selectAll;
            question.GotKeyboardFocus += selectAll;
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(JoinConversation));
            Commands.CreateQuizStructure.RegisterCommand(new DelegateCommand<object>(CreateQuizQuestion, canCreateQuizQuestion));
            question.Focus();
        }
        private void JoinConversation(object obj)
        {
            Dispatcher.adopt(delegate
            {
                Commands.JoinConversation.UnregisterCommand(new DelegateCommand<object>(JoinConversation));
                Close();
            });
        }
        private void Close(object sender, RoutedEventArgs e)
        {
            Commands.JoinConversation.UnregisterCommand(new DelegateCommand<object>(JoinConversation));
            this.Close();
        }
        private bool canCreateQuizQuestion(object sender)
        {
            if (question == null) return false;
            var activeOptions = options.Where(o => o.optionText.Length > 0).ToList();
            return !String.IsNullOrEmpty(question.Text) && activeOptions.Count >= 2;
        }
        private void CreateQuizQuestion(object sender)
        {
            var creationTimeAndId = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            var quiz = new QuizQuestion(creationTimeAndId, creationTimeAndId, "Unused", Globals.me, question.Text, new List<Option>());
            quiz.Url = identity;
            foreach (object obj in quizQuestions.Items)
            {
                var answer = (Option)obj;
                if (!string.IsNullOrEmpty(answer.optionText))
                    quiz.Options.Add(answer);
            }
            Commands.SendQuiz.ExecuteAsync(quiz);
            Trace.TraceInformation("CreatedPollQuestion {0}", question.Text);
            this.Close();
        }
        private void QuizButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                tryPrefillOption((FrameworkElement)sender);
            }
        }
        #region Helpers
        private FrameworkElement GetQuestionContainerFromItem(object itemContainer)
        {
            if (itemContainer is FrameworkElement)
            {
                var currentOption = (itemContainer as FrameworkElement).DataContext;
                return quizQuestions.ItemContainerGenerator.ContainerFromItem(currentOption) as FrameworkElement;
            }

            return null;
        }
        #endregion
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var optionContainer = GetQuestionContainerFromItem(sender);
            if (optionContainer != null)
                optionContainer.Opacity = 1;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var optionContainer = GetQuestionContainerFromItem(sender);
            if (optionContainer != null)
            {
                var option = (optionContainer.DataContext as Option);
                if (String.IsNullOrEmpty(option.optionText) && options.IndexOf(option) > 0)
                    optionContainer.Opacity = 0.5;
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
        private Color generateColor(int index)
        {
            return index % 2 == 0 ? Colors.White : (Color)ColorConverter.ConvertFromString("#FF4682B4");
        }

        private void AddNewEmptyOption()
        {
            if (!shouldAddNewEmptyOption()) return;
            foreach (var option in options)
            {
                var container = ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(option));
                if (container != null) container.Opacity = 1;
            }

            var newIndex = 1;
            var newName = Option.GetOptionNameFromIndex(0);
            if (options.Count > 0)
            {
                var lastOption = options.Last();
                newIndex = options.IndexOf(lastOption) + 1;
                newName = Option.GetNextOptionName(lastOption.name);
            }
            var newOption = new Option(newName, String.Empty, false, generateColor(newIndex));
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

            // relabel the option names
            for (int i = 0; i < options.Count; i++)
            {
                options[i].name = Option.GetOptionNameFromIndex(i);
                options[i].color = generateColor(i);
            }

            AddNewEmptyOption();
            foreach (var obj in options)
                if (String.IsNullOrEmpty(obj.optionText))
                    ((FrameworkElement)quizQuestions.ItemContainerGenerator.ContainerFromItem(obj)).Opacity = 0.5;
            CommandManager.InvalidateRequerySuggested();
        }
        private void screenshotAsAQuestion(object sender, RoutedEventArgs e)
        {
            Commands.GenerateScreenshot.Execute(new ScreenshotDetails
            {
                time = SandRibbonObjects.DateTimeFactory.Now().Ticks,
                message = "",
                author = Globals.me,
                onGeneration = bytes =>
                                {
                                    var proposedIdentity = String.Format("{0}:{1}", Globals.me, DateTime.Now.Ticks);
                                    var hostedFileUriXml = App.controller.client.resourceProvider.securePutData(App.controller.config.uploadResource(proposedIdentity, Globals.conversationDetails.Jid), bytes);
                                    identity = XDocument.Parse(hostedFileUriXml).Descendants("resourceUrl").First().Value;
                                    Dispatcher.adopt(() =>
                                    {

                                        //identity = App.controller.client.NoAuthUploadResource(new Uri(hostedFilename, UriKind.RelativeOrAbsolute), Int32.Parse(Globals.conversationDetails.Jid)).ToString();
                                        var image = new Image();
                                        BitmapImage source = new BitmapImage();
                                        try
                                        {
                                            source.BeginInit();
                                            source.UriSource = App.controller.config.getResource(identity);
                                            image.Source = source;
                                        }
                                        catch (Exception ex)
                                        {
                                            Trace.TraceError("Poll screenshot failure: {0}", ex.Message);
                                        }
                                        finally
                                        {
                                            source.EndInit();
                                        }
                                        questionSnapshotContainer.Child = image;
                                        //screenshot.Visibility = Visibility.Collapsed;
                                        var slide = Globals.slides.Where(s => s.id == Globals.slide).First();
                                        if (isAutogeneratedTitle(question.Text))
                                            question.Text = string.Format("Poll referencing page {0}", slide.index + 1);
                                    });
                                }
            });
        }
        private bool isAutogeneratedTitle(string text)
        {
            var generatedTitle = new Regex("Poll [0-9]+$");
            return generatedTitle.IsMatch(text);
        }

        private void selectAll(object sender, RoutedEventArgs e)
        {
            var origin = ((TextBox)sender);
            origin.SelectAll();
        }
        private void refreshCollection()
        {
            options = new ObservableCollection<Option> {
                                                         new Option("A",String.Empty,false, Colors.Blue)
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