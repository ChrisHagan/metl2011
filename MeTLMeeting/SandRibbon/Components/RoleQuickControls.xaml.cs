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
            var setSyncCommand = new DelegateCommand<bool>(SetSync);
            var toggleSyncCommand = new DelegateCommand<object>(toggleSync);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                {
                    rootPage = DataContext as SlideAwarePage;
                }
                ConversationDetails = rootPage.ConversationDetails;
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
            DelegateCommand<ScreenshotDetails> sendScreenshot = null;
            sendScreenshot = new DelegateCommand<ScreenshotDetails>(details =>
            {
                Commands.ScreenshotGenerated.UnregisterCommand(sendScreenshot);
                rootPage.NetworkController.client.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation
                (rootPage.Slide.id, rootPage.NetworkController.credentials.name, "submission",
                Privacy.Public, details.time, details.filename, details.message,
                new Dictionary<string, Color>(), Globals.generateId(rootPage.NetworkController.credentials.name, details.filename)));
                MeTLMessage.Information("Submission sent to " + rootPage.ConversationDetails.Author);
            });
            Commands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
            {
                slide = rootPage.Slide.id,
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
