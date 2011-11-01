using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Canvas;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using System.Diagnostics;

namespace SandRibbon.Components.Submissions
{
    public partial class ViewSubmissions : Window
    {
        public ObservableCollection<TargettedSubmission> submissionList = new ObservableCollection<TargettedSubmission>();
        public ViewSubmissions()
        {
            InitializeComponent();
            Commands.ReceiveScreenshotSubmission.RegisterCommandToDispatcher<TargettedSubmission>(new DelegateCommand<TargettedSubmission>(recieveSubmission));
            Commands.JoinConversation.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(joinConversation));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
        }
        private void closeMe(object obj)
        {
            Close();
        }
        private void joinConversation(string obj)
        {
            this.Close();
            submissionList = new ObservableCollection<TargettedSubmission>();
            UpdateLayout();
        }
        public ViewSubmissions(List<TargettedSubmission> userSubmissions):this()
        {
            Trace.TraceInformation("ViewingSubmissions");
            foreach (var list in userSubmissions)
                submissionList.Add(list);
            submissions.ItemsSource= submissionList;
            submissions.SelectedIndex = 0;
        }
        private void recieveSubmission(TargettedSubmission submission)
        {
            submissionList.Add(submission);
        }
        private void importSubmission(object sender, ExecutedRoutedEventArgs e)
        {
            var item = submissions.SelectedItem;
            DelegateCommand<PreParser> onPreparserAvailable = null;
            onPreparserAvailable = new DelegateCommand<PreParser>((parser) =>
               {
                   Commands.PreParserAvailable.UnregisterCommand(onPreparserAvailable);
                   var image = (TargettedSubmission) item;
                   Commands.ImageDropped.ExecuteAsync(new ImageDrop { 
                         filename = image.url.ToString (), 
                         target = "presentationSpace",
                         point = new Point (0, 0),
                         position = 1

                     });
               });
            Commands.PreParserAvailable.RegisterCommand(onPreparserAvailable);
            Commands.AddSlide.ExecuteAsync(null);
            this.Close();
        }
        private void canImportSubmission(object sender, CanExecuteRoutedEventArgs e)
        {
            if (submissions != null)
            e.CanExecute = submissions.SelectedItem != null;
        }
    }
}
