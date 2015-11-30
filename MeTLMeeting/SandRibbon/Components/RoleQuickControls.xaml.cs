using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Submissions;
using SandRibbon.Components.Utility;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SandRibbon.Components
{    
    public partial class RoleQuickControls : UserControl
    {
        public RibbonCollaborationPage rootPage { get; protected set; }
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
                    rootPage = DataContext as RibbonCollaborationPage;
                }
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.SetSync.RegisterCommand(setSyncCommand);
                Commands.SetSync.Execute(false);
                Commands.ToggleSync.RegisterCommand(toggleSyncCommand);
                UpdatedConversationDetails(rootPage.details);
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
            var cd = rootPage.details;
            cd.Permissions.studentCanPublish = studentsCanPublishValue;
            App.controller.client.UpdateConversationDetails(cd);
        }
        private void StudentsMustFollowTeacherChecked(object sender, RoutedEventArgs e)
        {
            var studentsMustFollowTeacherValue = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.details;
            cd.Permissions.usersAreCompulsorilySynced = studentsMustFollowTeacherValue;
            App.controller.client.UpdateConversationDetails(cd);
        }
        protected void UpdatedConversationDetails(ConversationDetails conv)
        {
            Dispatcher.adopt(delegate
            {
                if (rootPage.details.isAuthor(Globals.me))
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
            syncButton.IsChecked = Globals.synched; 
            if (Globals.synched)
            {                
                try
                {
                    var teacherSlide = (int)Globals.teacherSlide;
                    if (rootPage.details.Slides.Select(s => s.id).Contains(teacherSlide) && !rootPage.details.isAuthor(Globals.me))
                        Commands.MoveToCollaborationPage.Execute((int)Globals.teacherSlide);
                }
                catch (NotSetException) { }
            }            
        }
        private void toggleSync(object _unused)
        {
            var synch = !Globals.synched;
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
                App.controller.client.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                (App.controller.client.location.currentSlide, Globals.me, "submission", Privacy.Public, -1L, hostedFileName, rootPage.details.Title, new Dictionary<string, Color>(), Globals.generateId(hostedFileName)));
                MeTLMessage.Information("Submission sent to " + rootPage.details.Author);
            });
            Commands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
            {
                time = time,
                message = string.Format("Submission by {1} at {0}", new DateTime(time), Globals.me),
                showPrivate = true
            });
        }

    }
}
