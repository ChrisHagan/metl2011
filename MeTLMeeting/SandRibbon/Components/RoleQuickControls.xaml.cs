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
        public SlideAwarePage rootPage { get; protected set; }
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
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.SetSync.RegisterCommand(setSyncCommand);
                Commands.SetSync.Execute(false);
                Commands.ToggleSync.RegisterCommand(toggleSyncCommand);
                UpdatedConversationDetails(rootPage.getDetails());
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
            var cd = rootPage.getDetails();
            cd.Permissions.studentCanPublish = studentsCanPublishValue;
            rootPage.getNetworkController().client.UpdateConversationDetails(cd);
        }
        private void StudentsMustFollowTeacherChecked(object sender, RoutedEventArgs e)
        {
            var studentsMustFollowTeacherValue = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.getDetails();
            cd.Permissions.usersAreCompulsorilySynced = studentsMustFollowTeacherValue;
            rootPage.getNetworkController().client.UpdateConversationDetails(cd);
        }
        protected void UpdatedConversationDetails(ConversationDetails conv)
        {
            Dispatcher.adopt(delegate
            {
                if (rootPage.getDetails().isAuthor(rootPage.getNetworkController().credentials.name))
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
            syncButton.IsChecked = rootPage.getUserConversationState().synched; 
            if (rootPage.getUserConversationState().synched)
            {                
                try
                {
                    var teacherSlide = (int)rootPage.getUserConversationState().teacherSlide;
                    if (rootPage.getDetails().Slides.Select(s => s.id).Contains(teacherSlide) && !rootPage.getDetails().isAuthor(rootPage.getNetworkController().credentials.name))
                        rootPage.getNavigationService().Navigate(new RibbonCollaborationPage(rootPage.getUserGlobalState(),rootPage.getUserServerState(),rootPage.getUserConversationState(),rootPage.getConversationState(),new UserSlideState(),rootPage.getNetworkController(), rootPage.getDetails(),rootPage.getDetails().Slides.First(s => s.id == teacherSlide)));
                        //Commands.MoveToCollaborationPage.Execute((int)Globals.teacherSlide);
                }
                catch (NotSetException) { }
            }            
        }
        private void toggleSync(object _unused)
        {
            var synch = !rootPage.getUserConversationState().synched;
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
                rootPage.getNetworkController().client.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                (rootPage.getSlide().id, rootPage.getNetworkController().credentials.name, "submission", Privacy.Public, -1L, hostedFileName, rootPage.getDetails().Title, new Dictionary<string, Color>(), Globals.generateId(rootPage.getNetworkController().credentials.name,hostedFileName)));
                MeTLMessage.Information("Submission sent to " + rootPage.getDetails().Author);
            });
            Commands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
            {
                time = time,
                message = string.Format("Submission by {1} at {0}", new DateTime(time), rootPage.getNetworkController().credentials.name),
                showPrivate = true
            });
        }

    }
}
