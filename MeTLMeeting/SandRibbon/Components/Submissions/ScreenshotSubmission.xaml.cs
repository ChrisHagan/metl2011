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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbon.Utils.Connection;
using SandRibbonObjects;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;

namespace SandRibbon.Components.Submissions
{
    public class ScreenshotDetails
    {
        public string message;
        public long time;
        public bool showPrivate;
    }
    public partial class ScreenshotSubmission : UserControl
    {
        public static RoutedCommand ViewSubmissions = new RoutedCommand();
        public List<TargettedSubmission> submissionList = new List<TargettedSubmission>();
        public ScreenshotSubmission()
        {
            InitializeComponent();
            Commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(receiveSubmission));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<object>(detailsChanged));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(conversationChanged));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(PreParserAvailable));
            conversationChanged(null);
        }
        private void viewSubmissions(object sender, ExecutedRoutedEventArgs e)
        { 
            new ViewSubmissions(submissionList).Show();
        }
        private void canViewSubmissions(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = submissionList.Count > 0;
        }
        private void PreParserAvailable(PreParser parser)
        {
            foreach(var submission in parser.submissions)
                receiveSubmission(submission);
        }
        private void detailsChanged(object obj)
        {
            Dispatcher.adoptAsync( delegate
                                                {
                                                    try
                                                    {
                                                        if (Globals.conversationDetails.Author == Globals.me)
                                                            amTeacher();
                                                        else
                                                            amStudent();
                                                    }
                                                    catch(NotSetException)
                                                    {
                                                    }
      
                                                });
        }
        private void conversationChanged(object details)
        {
            Dispatcher.adoptAsync( delegate
                                                {
                                                    try
                                                    {
                                                        submissionList = new List<TargettedSubmission>();
                                                        if (Globals.conversationDetails.Author == Globals.me)
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
            if(!IHaveThisSubmission(submission))
                submissionList.Add(submission);
        }
        private bool IHaveThisSubmission(MeTLLib.DataTypes.TargettedSubmission submission)
        {
            if (submissionList.Where(s => s.time == submission.time && s.author == submission.author && s.url == submission.url).ToList().Count > 0)
                return true;
            return false;
        }
        private void generateScreenshot(object sender, RoutedEventArgs e)
        {
            var time = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            DelegateCommand<string> sendScreenshot = null;
            sendScreenshot = new DelegateCommand<string>(hostedFileName =>
                             {
                                 Commands.ScreenshotGenerated.UnregisterCommand(sendScreenshot);
                                 MeTLLib.ClientFactory.Connection().uploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                                 (Globals.location.currentSlide,Globals.me,"submission","public",hostedFileName));
                                MessageBox.Show("Submission sent to " + Globals.conversationDetails.Author);
                             });
            Commands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
                                                    {
                                                        time = time,
                                                        message = string.Format("{0}'s submission at {1}", Globals.me, new DateTime(time)),
                                                        showPrivate = true
                                                    });
        }
     
    }
}
