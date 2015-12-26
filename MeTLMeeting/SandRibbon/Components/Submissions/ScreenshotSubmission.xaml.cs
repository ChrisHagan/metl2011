using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using System.Diagnostics;
using SandRibbon.Components.Utility;
using System.Windows.Media;
using SandRibbon.Pages;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.Threading.Tasks;

namespace SandRibbon.Components.Submissions
{
    public class ScreenshotDetails
    {
        public string message;
        public long time;
        public bool showPrivate;
        public Size dimensions;
        internal string filename;
        internal int slide;
    }
    public partial class ScreenshotSubmission : UserControl
    {
        public List<TargettedSubmission> submissionList = new List<TargettedSubmission>();
        public SlideAwarePage rootPage { get; protected set; }
        public ScreenshotSubmission()
        {
            InitializeComponent();
            var receiveScreenshotSubmissionCommand = new DelegateCommand<TargettedSubmission>(receiveSubmission);
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(detailsChanged);
            var preParserAvailableCommand = new DelegateCommand<PreParser>(PreParserAvailable);
            var viewSubmissionsCommand = new DelegateCommand<object>(viewSubmissions, canViewSubmissions);
            var beginScreenshotSubmissionCommand = new DelegateCommand<object>(generateScreenshot, canGenerateScreenshot);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as SlideAwarePage;
                Commands.ReceiveScreenshotSubmission.RegisterCommand(receiveScreenshotSubmissionCommand);
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.PreParserAvailable.RegisterCommand(preParserAvailableCommand);
                Commands.ViewSubmissions.RegisterCommand(viewSubmissionsCommand);
                Commands.SubmitScreenshotSubmission.RegisterCommand(beginScreenshotSubmissionCommand);
                conversationChanged(null);
                rootPage.NetworkController.client.historyProvider.Retrieve<PreParser>(delegate { }, delegate { },
                    PreParserAvailable, rootPage.ConversationDetails.Jid);
            };
            Unloaded += (s, e) =>
            {
                Commands.ReceiveScreenshotSubmission.UnregisterCommand(receiveScreenshotSubmissionCommand);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.PreParserAvailable.UnregisterCommand(preParserAvailableCommand);
                Commands.ViewSubmissions.UnregisterCommand(viewSubmissionsCommand);
                Commands.SubmitScreenshotSubmission.UnregisterCommand(beginScreenshotSubmissionCommand);
            };
        }
        private void viewSubmissions(object _obj)
        {
            rootPage.NavigationService.Navigate(new ViewSubmissions(rootPage));
        }
        private bool canViewSubmissions(object _e)
        {
            return submissionList.Count > 0;
        }
        private void PreParserAvailable(PreParser parser)
        {
            foreach (var submission in parser.submissions)
                receiveSubmission(submission);
        }
        private void detailsChanged(ConversationDetails details)
        {
            Dispatcher.adopt(delegate
            {
                if (rootPage.ConversationDetails.Author == rootPage.NetworkController.credentials.name)
                    amTeacher();
                else
                    amStudent();
            });
        }
        private void conversationChanged(object details)
        {
            if (rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name))
                amTeacher();
            else
                amStudent();
        }
        private void amTeacher()
        {
            submitSubmission.Visibility = Visibility.Visible;
            viewSubmission.Visibility = Visibility.Visible;

        }
        private void amStudent()
        {
            submitSubmission.Visibility = Visibility.Visible;
            viewSubmission.Visibility = Visibility.Collapsed;
        }
        private void receiveSubmission(MeTLLib.DataTypes.TargettedSubmission submission)
        {
            if (!String.IsNullOrEmpty(submission.target) && submission.target != "submission")
                return;

            if (!IHaveThisSubmission(submission))
            {
                submissionList.Add(submission);
                Commands.RequerySuggested(Commands.ViewSubmissions);
            }
        }
        private bool IHaveThisSubmission(TargettedSubmission submission)
        {
            return submissionList.Any(s => s.time == submission.time && s.author == submission.author && s.url == submission.url);
        }
        protected bool canGenerateScreenshot(object _unused)
        {
            return true;
        }
        protected async void generateScreenshot(object _unused)
        {
            Trace.TraceInformation("SubmittedScreenshot");
            var time = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            DelegateCommand<ScreenshotDetails> sendScreenshot = null;
            sendScreenshot = new DelegateCommand<ScreenshotDetails>(details=>
            {
                Commands.ScreenshotGenerated.UnregisterCommand(sendScreenshot);
                rootPage.NetworkController.client.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                (rootPage.NetworkController.client.location.currentSlide, rootPage.NetworkController.credentials.name, 
                "submission", Privacy.Public, details.time, details.filename, details.message, 
                new Dictionary<string, Color>(), Globals.generateId(rootPage.NetworkController.credentials.name, details.filename)));
                MeTLMessage.Information("Submission sent to " + rootPage.ConversationDetails.Author);
            });
            Commands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            var message = await DialogManager.ShowInputAsync(App.Current.MainWindow as MetroWindow, "Submit a screenshot", "You may add a message for the teacher here");
            Commands.GenerateScreenshot.Execute(new ScreenshotDetails
            {
                slide = rootPage.Slide.id,
                time = time,
                message = message,
                showPrivate = true
            });
        }
        private void generateScreenshot(object sender, RoutedEventArgs e)
        {
            generateScreenshot(null);
        }
    }
}
