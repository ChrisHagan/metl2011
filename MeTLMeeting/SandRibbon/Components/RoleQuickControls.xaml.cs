using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Submissions;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SandRibbon.Components
{
    public partial class RoleQuickControls : UserControl
    {
        public RoleQuickControls()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdatedConversationDetails));
            Commands.SetSync.RegisterCommand(new DelegateCommand<bool>(SetSync));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>((obj) =>
            {
                Commands.RequerySuggested(
                    Commands.DuplicateConversation,
                    Commands.DuplicateSlide
                    );
            }));
        }

        private void StudentsCanPublishChecked(object sender, RoutedEventArgs e)
        {
            var studentsCanPublishValue = (bool)(sender as CheckBox).IsChecked;
            var cd = Globals.conversationDetails;
            if (cd.Permissions.studentCanWorkPublicly != studentsCanPublishValue)
            {
                cd.Permissions.studentCanWorkPublicly = studentsCanPublishValue;
                App.controller.client.UpdateConversationDetails(cd);
            }
        }
        private void StudentsMustFollowTeacherChecked(object sender, RoutedEventArgs e)
        {
            var studentsMustFollowTeacherValue = (bool)(sender as CheckBox).IsChecked;
            var cd = Globals.conversationDetails;
            if (cd.Permissions.usersAreCompulsorilySynced != studentsMustFollowTeacherValue)
            {
                cd.Permissions.usersAreCompulsorilySynced = studentsMustFollowTeacherValue;
                App.controller.client.UpdateConversationDetails(cd);
            }
        }

        private void toggleBanhammer(object sender, RoutedEventArgs e)
        {
            var banhammerActive = (bool)(sender as CheckBox).IsChecked;
            Commands.BanhammerActive.Execute(banhammerActive);
            if (banhammerActive) Commands.SetLayer.Execute("Select");
        }
        protected void UpdatedConversationDetails(ConversationDetails conv)
        {
            Dispatcher.adopt(delegate
            {
                if (Globals.isAuthor)
                {
                    ownerQuickControls.Visibility = Visibility.Visible;
                    participantQuickControls.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ownerQuickControls.Visibility = Visibility.Collapsed;
                    participantQuickControls.Visibility = Visibility.Visible;
                }
                studentCanPublishCheckbox.IsChecked = conv.Permissions.studentCanWorkPublicly;
                studentMustFollowTeacherCheckbox.IsChecked = conv.Permissions.usersAreCompulsorilySynced;
                if (conv.Permissions.usersAreCompulsorilySynced)
                {
                    syncButton.IsEnabled = false;
                    syncButton.IsChecked = true;
                }
            });
        }
        private void SetSync(bool sync)
        {
            if (sync)
            {
                syncButton.IsChecked = true;
                var teacherSlide = (int)Globals.teacherSlide;
                if (Globals.location.availableSlides.Contains(teacherSlide) && !Globals.isAuthor)
                    Commands.MoveTo.Execute((int)Globals.teacherSlide);
            }
        }
        private void toggleSync(object sender, RoutedEventArgs e)
        {
            Globals.synched = !Globals.synched;
            Commands.SetSync.Execute(Globals.synched);
        }
        private void generateScreenshot(object sender, RoutedEventArgs e)
        {
            Trace.TraceInformation("SubmittedScreenshot");
            var time = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
            {
                time = time,
                message = string.Format("Submission by {1} at {0}", new DateTime(time), Globals.me),
                showPrivate = true,
                author = Globals.me,
                onGeneration = hostedFileName =>
                {
                    App.controller.client.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                    (App.controller.client.location.currentSlide, Globals.me, "submission", Privacy.Public, -1L, hostedFileName, Globals.conversationDetails.Title, new Dictionary<string, Color>(), Globals.generateId(hostedFileName)));
                    MeTLMessage.Information("Submission sent to " + Globals.conversationDetails.Author);
                }
            });
        }

    }
}
