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
using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;

namespace SandRibbon.Components.Submissions
{
    public class ScreenshotDetails
    {
        public string message;
        public long time;
    }
    public partial class ScreenshotSubmission : UserControl
    {
        public List<TargettedSubmission> submissionList = new List<TargettedSubmission>();
        public ScreenshotSubmission()
        {
            InitializeComponent();
            Commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(receiveSubmission));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<object>(conversationChanged));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(conversationChanged));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(PreParserAvailable));
            conversationChanged(null);
        }

        private void PreParserAvailable(PreParser parser)
        {
            foreach(var submission in parser.submissions)
                receiveSubmission(submission);
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
                                                        Commands.SneakInto.Execute(Globals.conversationDetails.Jid);
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
        private void receiveSubmission(TargettedSubmission submission)
        {
            submissionList.Add(submission);
        }
        private void generateScreenshot(object sender, RoutedEventArgs e)
        {
            var time = DateTime.Now.Ticks;
            DelegateCommand<string> sendScreenshot = null;
            sendScreenshot = new DelegateCommand<string>(hostedFileName =>
                             {
                                                                 
                                 Commands.ScreenshotGenerated.UnregisterCommand(sendScreenshot);
                                 Commands.SendScreenshotSubmission.Execute(new TargettedSubmission
                                                                                  {
                                                                                      author = Globals.me,
                                                                                      url = hostedFileName,
                                                                                      slide = Globals.slide,
                                                                                      time = time 
                                                                                  });
                             });
            Commands.SendScreenshotSubmission.RegisterCommand(sendScreenshot);
            Commands.GenerateScreenshot.Execute(new ScreenshotDetails
                                                    {
                                                        time = time,
                                                        message = string.Format("{0}'s submission at {1}", Globals.me, new DateTime(time)),
                                                    });
            MessageBox.Show("Submission sent to " + Globals.conversationDetails.Author);
        }

        private void viewSubmissions(object sender, RoutedEventArgs e)
        {
            new ViewSubmissions(submissionList).Show();
        }
    }
}
