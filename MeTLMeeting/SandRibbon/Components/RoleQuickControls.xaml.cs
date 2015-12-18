using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Submissions;
using SandRibbon.Components.Utility;
using SandRibbon.Pages;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SandRibbon.Components
{
    public partial class RoleQuickControls : UserControl
    {
        public SlideAwarePage rootPage { get; protected set; }
        public ConversationDetails ConversationDetails { get; protected set; }
        public RoleQuickControls()
        {
            InitializeComponent();
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(UpdatedConversationDetails);
            var setSyncCommand = new DelegateCommand<bool>(SetSync);
            var toggleSyncCommand = new DelegateCommand<object>(toggleSync);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                {
                    rootPage = DataContext as SlideAwarePage;
                }
                ConversationDetails = rootPage.ConversationDetails;
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.SetSync.RegisterCommand(setSyncCommand);
                Commands.SetSync.Execute(false);
                Commands.ToggleSync.RegisterCommand(toggleSyncCommand);
                UpdatedConversationDetails(rootPage.ConversationDetails);
            };
            Unloaded += (s, e) =>
            {
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.SetSync.UnregisterCommand(setSyncCommand);
                Commands.ToggleSync.UnregisterCommand(toggleSyncCommand);
            };
        }

        private void StudentsCanPublishChecked(object sender, RoutedEventArgs e)
        {
            var studentsCanPublishValue = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.ConversationDetails;
            cd.Permissions.studentCanPublish = studentsCanPublishValue;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
        private void StudentsCanViewQuizzesChecked(object sender, RoutedEventArgs e)
        {
            var val= (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.ConversationDetails;
            cd.Permissions.studentsCanViewQuiz = val;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
        private void StudentsCanAnswerQuizzesChecked(object sender, RoutedEventArgs e)
        {
            var val = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.ConversationDetails;
            cd.Permissions.studentsCanAnswerQuiz = val;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
        private void StudentsCanDisplayQuizzesChecked(object sender, RoutedEventArgs e)
        {
            var val = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.ConversationDetails;
            cd.Permissions.studentsCanDisplayQuiz = val;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
        private void StudentsCanViewQuizResultsChecked(object sender, RoutedEventArgs e)
        {
            var val = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.ConversationDetails;
            cd.Permissions.studentsCanViewQuiz= val;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
        private void StudentsCanDisplayQuizResultsChecked(object sender, RoutedEventArgs e)
        {
            var val = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.ConversationDetails;
            cd.Permissions.studentsCanDisplayQuizResults= val;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
        private void StudentsMustFollowTeacherChecked(object sender, RoutedEventArgs e)
        {
            var studentsMustFollowTeacherValue = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.ConversationDetails;
            cd.Permissions.usersAreCompulsorilySynced = studentsMustFollowTeacherValue;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
        protected void UpdatedConversationDetails(ConversationDetails conv)
        {
            Dispatcher.adopt(delegate
            {
                if (rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name))
                {
                    ownerQuickControls.Visibility = Visibility.Visible;
                    participantQuickControls.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ownerQuickControls.Visibility = Visibility.Collapsed;
                    participantQuickControls.Visibility = Visibility.Visible;

                }
                studentCanPublishCheckbox.IsChecked = conv.Permissions.studentCanPublish;
                studentMustFollowTeacherCheckbox.IsChecked = conv.Permissions.usersAreCompulsorilySynced;
            }); ;
        }
        private void SetSync(bool sync)
        {
            syncButton.IsChecked = rootPage.UserConversationState.Synched;
            if (rootPage.UserConversationState.Synched)
            {
                var teacherSlide = (int)rootPage.UserConversationState.TeacherSlide;
                if (rootPage.ConversationDetails.Slides.Select(s => s.id).Contains(teacherSlide) && !rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name))
                {
                    rootPage.Slide = rootPage.ConversationDetails.Slides.Where(s => s.id == teacherSlide).First();
                    rootPage.NavigationService.Navigate(new RibbonCollaborationPage(rootPage.UserGlobalState, rootPage.UserServerState, rootPage.UserConversationState, rootPage.ConversationState, new UserSlideState(), rootPage.NetworkController, rootPage.ConversationDetails, rootPage.ConversationDetails.Slides.First(s => s.id == teacherSlide)));
                }
            }
        }
        private void toggleSync(object _unused)
        {
            var synch = !rootPage.UserConversationState.Synched;
            System.Diagnostics.Trace.TraceInformation("ManuallySynched {0}", synch);
            Commands.SetSync.Execute(synch);
        }
        private void generateScreenshot(object sender, RoutedEventArgs e)
        {
            Trace.TraceInformation("SubmittedScreenshot");
            var time = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            DelegateCommand<string> sendScreenshot = null;
            sendScreenshot = new DelegateCommand<string>(hostedFileName =>
            {
                Commands.ScreenshotGenerated.UnregisterCommand(sendScreenshot);
                rootPage.NetworkController.client.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                (rootPage.Slide.id, rootPage.NetworkController.credentials.name, "submission", Privacy.Public, -1L, hostedFileName, rootPage.ConversationDetails.Title, new Dictionary<string, Color>(), Globals.generateId(rootPage.NetworkController.credentials.name, hostedFileName)));
                MeTLMessage.Information("Submission sent to " + rootPage.ConversationDetails.Author);
            });
            Commands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
            {
                time = time,
                message = string.Format("Submission by {1} at {0}", new DateTime(time), rootPage.NetworkController.credentials.name),
                showPrivate = true
            });
        }

        private void duplicatePage_Click(object sender, RoutedEventArgs e)
        {
            Commands.DuplicateSlide.Execute(new KeyValuePair<ConversationDetails, Slide>(rootPage.ConversationDetails, rootPage.Slide));
        }
    }
}
