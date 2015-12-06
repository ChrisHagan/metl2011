using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Submissions;
using SandRibbon.Components.Utility;
using SandRibbon.Pages;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.Collaboration.Models;
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
        public ConversationDetails ConversationDetails { get; protected set; }
        public RoleQuickControls()
        {
            InitializeComponent();            
            var setSyncCommand = new DelegateCommand<bool>(SetSync);
            var toggleSyncCommand = new DelegateCommand<object>(toggleSync);
            Loaded += (s, e) =>
            {     
                Commands.SetSync.RegisterCommand(setSyncCommand);
                Commands.SetSync.Execute(false);
                Commands.ToggleSync.RegisterCommand(toggleSyncCommand);                
            };
            Unloaded += (s, e) =>
            {                
                Commands.SetSync.UnregisterCommand(setSyncCommand);
                Commands.ToggleSync.UnregisterCommand(toggleSyncCommand);
            };
        }

        private void StudentsCanPublishChecked(object sender, RoutedEventArgs e)
        {
            var rootPage = DataContext as DataContextRoot;
            rootPage.ConversationState.StudentsCanPublish = (bool)(sender as CheckBox).IsChecked;
        }
        private void StudentsMustFollowTeacherChecked(object sender, RoutedEventArgs e)
        {
            var rootPage = DataContext as DataContextRoot;
            rootPage.ConversationState.StudentsCanMoveFreely = !(bool)(sender as CheckBox).IsChecked;            
        }
        protected void UpdatedConversationDetails(ConversationState conv)
        {
            var rootPage = DataContext as DataContextRoot;
            Dispatcher.adopt(delegate
            {
                if (rootPage.ConversationState.IsAuthor)
                {
                    ownerQuickControls.Visibility = Visibility.Visible;
                    participantQuickControls.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ownerQuickControls.Visibility = Visibility.Collapsed;
                    participantQuickControls.Visibility = Visibility.Visible;

                }
                studentCanPublishCheckbox.IsChecked = conv.StudentsCanPublish;
                studentMustFollowTeacherCheckbox.IsChecked = !conv.StudentsCanMoveFreely;
            }); ;
        }
        private void SetSync(bool sync)
        {
            var rootPage = DataContext as DataContextRoot;
            syncButton.IsChecked = rootPage.UserConversationState.Synched; 
            if (rootPage.UserConversationState.Synched)
            {                
                try
                {
                    var teacherSlide = (int)rootPage.UserConversationState.TeacherSlide;
                    if (rootPage.ConversationState.Slides.Select(s => s.id).Contains(teacherSlide) && !rootPage.ConversationState.IsAuthor)
                        NavigationService.GetNavigationService(this).Navigate(new RibbonCollaborationPage(rootPage.UserGlobalState,rootPage.UserServerState,rootPage.UserConversationState,rootPage.ConversationState,new UserSlideState(),rootPage.NetworkController, rootPage.ConversationState.Slides.First(s => s.id == teacherSlide)));                        
                }
                catch (NotSetException) { }
            }            
        }
        private void toggleSync(object _unused)
        {
            var rootPage = DataContext as DataContextRoot;
            var synch = !rootPage.UserConversationState.Synched;
            System.Diagnostics.Trace.TraceInformation("ManuallySynched {0}", synch);
            Commands.SetSync.Execute(synch);
        }
        private void generateScreenshot(object sender, RoutedEventArgs e)
        {
            var rootPage = DataContext as DataContextRoot;
            Trace.TraceInformation("SubmittedScreenshot");
            var time = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            DelegateCommand<string> sendScreenshot = null;
            sendScreenshot = new DelegateCommand<string>(hostedFileName =>
            {
                Commands.ScreenshotGenerated.UnregisterCommand(sendScreenshot);
                rootPage.NetworkController.client.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                (rootPage.ConversationState.Slide.id, rootPage.NetworkController.credentials.name, "submission", Privacy.Public, -1L, hostedFileName, rootPage.ConversationState.Title, new Dictionary<string, Color>(), Globals.generateId(rootPage.NetworkController.credentials.name,hostedFileName)));
                MeTLMessage.Information("Submission sent to " + rootPage.ConversationState.Author);
            });
            Commands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
            {
                time = time,
                message = string.Format("Submission by {1} at {0}", new DateTime(time), rootPage.NetworkController.credentials.name),
                showPrivate = true
            });
        }

    }
}
