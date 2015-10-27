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
using MeTLLib;

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
        public MetlConfiguration backend;
        public List<TargettedSubmission> submissionList = new List<TargettedSubmission>();
        public ScreenshotSubmission()
        {
            InitializeComponent();
            App.getContextFor(backend).controller.commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(receiveSubmission));
            App.getContextFor(backend).controller.commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(detailsChanged));
            App.getContextFor(backend).controller.commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(conversationChanged));
            App.getContextFor(backend).controller.commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(PreParserAvailable));
            AppCommands.ViewSubmissions.RegisterCommand(new DelegateCommand<object>(viewSubmissions, canViewSubmissions));
            conversationChanged(null);
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
                if (Globals.conversationDetails.Author == App.getContextFor(backend).controller.creds.name)
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
                                                        if (Globals.conversationDetails.Author == App.getContextFor(backend).controller.creds.name)
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
                AppCommands.RequerySuggested(AppCommands.ViewSubmissions);
            }
        }
        private bool IHaveThisSubmission(MeTLLib.DataTypes.TargettedSubmission submission)
        {
            if (submissionList.Where(s => s.time == submission.time && s.author == submission.author && s.url == submission.url).ToList().Count > 0)
                return true;
            return false;
        }
        private void generateScreenshot(object sender, RoutedEventArgs e)
        {
            Trace.TraceInformation("SubmittedScreenshot");
            var time = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            DelegateCommand<string> sendScreenshot = null;
            sendScreenshot = new DelegateCommand<string>(hostedFileName =>
                             {
                                 AppCommands.ScreenshotGenerated.UnregisterCommand(sendScreenshot);
                                 App.getContextFor(backend).controller.client.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                                 (App.getContextFor(backend).controller.client.location.currentSlide, App.getContextFor(backend).controller.creds.name, "submission",Privacy.Public, -1L, hostedFileName, Globals.conversationDetails.Title, new Dictionary<string, Color>(), Globals.generateId(hostedFileName)));
                                 MeTLMessage.Information("Submission sent to " + Globals.conversationDetails.Author);
                             });
            AppCommands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            AppCommands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
                                                    {
                                                        time = time,
                                                        message = string.Format("Submission by {1} at {0}", new DateTime(time), App.getContextFor(backend).controller.creds.name),
                                                        showPrivate = true
                                                    });
        }
     
    }
}
