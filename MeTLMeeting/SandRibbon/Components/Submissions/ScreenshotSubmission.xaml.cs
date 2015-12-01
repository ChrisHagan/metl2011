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
using SandRibbon.Pages.Collaboration;

namespace SandRibbon.Components.Submissions
{
    public class ScreenshotDetails
    {
        public string message;
        public long time;
        public bool showPrivate;
        public Size dimensions;
    }
    public partial class ScreenshotSubmission : UserControl
    {
        public List<TargettedSubmission> submissionList = new List<TargettedSubmission>();
        public RibbonCollaborationPage rootPage { get; protected set; }
        public ScreenshotSubmission()
        {
            InitializeComponent();
            var receiveScreenshotSubmissionCommand = new DelegateCommand<TargettedSubmission>(receiveSubmission);
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(detailsChanged);
            var preParserAvailableCommand = new DelegateCommand<PreParser>(PreParserAvailable);
            var viewSubmissionsCommand = new DelegateCommand<object>(viewSubmissions, canViewSubmissions);
            var requestScreenshotSubmissionCommand = new DelegateCommand<object>(generateScreenshot, canGenerateScreenshot);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as RibbonCollaborationPage;
                Commands.ReceiveScreenshotSubmission.RegisterCommand(receiveScreenshotSubmissionCommand);
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateConversationDetailsCommand);
                Commands.PreParserAvailable.RegisterCommand(preParserAvailableCommand);
                Commands.ViewSubmissions.RegisterCommand(viewSubmissionsCommand);
                Commands.RequestScreenshotSubmission.RegisterCommand(requestScreenshotSubmissionCommand);
                conversationChanged(null);
            };
            Unloaded += (s, e) =>
            {
                Commands.ReceiveScreenshotSubmission.UnregisterCommand(receiveScreenshotSubmissionCommand);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.PreParserAvailable.UnregisterCommand(preParserAvailableCommand);
                Commands.ViewSubmissions.UnregisterCommand(viewSubmissionsCommand);
                Commands.RequestScreenshotSubmission.UnregisterCommand(requestScreenshotSubmissionCommand);
            };
        }
        private void viewSubmissions(object _obj)
        {
            var view = new ViewSubmissions(submissionList);
            view.Owner = Window.GetWindow(this);
            view.Show();
        }
        private bool canViewSubmissions(object _e)
        {
           return submissionList.Count > 0;
        }
        private void PreParserAvailable(PreParser parser)
        {
            foreach(var submission in parser.submissions)
                receiveSubmission(submission);
        }
        private void detailsChanged(ConversationDetails details)
        {
            if (ConversationDetails.Empty.Equals(details)) return;
            try
            {
                if (rootPage.details.Author == rootPage.networkController.credentials.name)
                    amTeacher();
                else
                    amStudent();
            }
            catch(NotSetException)
            {
            }
      
        }
        private void conversationChanged(object details)
        {
            Dispatcher.adoptAsync( delegate
                                                {
                                                    try
                                                    {
                                                        submissionList = new List<TargettedSubmission>();
                                                        if (rootPage.details.Author == rootPage.networkController.credentials.name)
                                                            amTeacher();
                                                        else
                                                            amStudent();
                                                    }
                                                    catch(NotSetException)
                                                    {
                                                    }
      
                                                });
        }
        private void amTeacher()
        {
            submitSubmission.Visibility = Visibility.Collapsed;
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
        private bool IHaveThisSubmission(MeTLLib.DataTypes.TargettedSubmission submission)
        {
            if (submissionList.Where(s => s.time == submission.time && s.author == submission.author && s.url == submission.url).ToList().Count > 0)
                return true;
            return false;
        }
        protected bool canGenerateScreenshot(object _unused)
        {
            return true;
        }
        protected void generateScreenshot(object _unused)
        {
            Trace.TraceInformation("SubmittedScreenshot");
            var time = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            DelegateCommand<string> sendScreenshot = null;
            sendScreenshot = new DelegateCommand<string>(hostedFileName =>
            {
                Commands.ScreenshotGenerated.UnregisterCommand(sendScreenshot);
                rootPage.getNetworkController().client.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                (rootPage.getNetworkController().client.location.currentSlide, rootPage.networkController.credentials.name, "submission", Privacy.Public, -1L, hostedFileName, rootPage.details.Title, new Dictionary<string, Color>(), Globals.generateId(rootPage.networkController.credentials.name,hostedFileName)));
                MeTLMessage.Information("Submission sent to " + rootPage.getDetails().Author);
            });
            Commands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
            {
                time = time,
                message = string.Format("Submission by {1} at {0}", new DateTime(time), rootPage.networkController.credentials.name),
                showPrivate = true
            });
        }
        private void generateScreenshot(object sender, RoutedEventArgs e)
        {
            generateScreenshot(null);
        }
    }
}
