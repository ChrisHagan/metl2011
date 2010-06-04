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
using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;

namespace SandRibbon.Components.Submissions
{

    public partial class ScreenshotSubmission : UserControl
    {
        public List<TargettedSubmission> submissionList = new List<TargettedSubmission>();
        public ScreenshotSubmission()
        {
            InitializeComponent();
            Commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(receiveSubmission));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(conversationChanged));
        }

        private void conversationChanged(ConversationDetails details)
        {
            Dispatcher.BeginInvoke((Action) delegate
                                                {
                                                    if (details.Author == Globals.me)
                                                        amTeacher();
                                                    else
                                                        amStudent();
                                                });
        }
        private void amTeacher()
        {
            submit.Visibility = Visibility.Collapsed;
            view.Visibility = Visibility.Visible;

        }
        private void amStudent()
        {
            submit.Visibility = Visibility.Visible;
            view.Visibility = Visibility.Collapsed;

        }
        private void receiveSubmission(TargettedSubmission submission)
        {
            submissionList.Add(submission);
        }
        private void generateScreenshot(object sender, RoutedEventArgs e)
        {
            Commands.GenerateScreenshot.Execute(DateTime.Now.Ticks);
        }

        private void viewSubmissions(object sender, RoutedEventArgs e)
        {
            new ViewSubmissions(submissionList).Show();
        }
    }
}
