using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Submissions;
using SandRibbon.Components.Utility;
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
    /// <summary>
    /// Interaction logic for RoleQuickControls.xaml
    /// </summary>
    public partial class RoleQuickControls : UserControl
    {
        public RoleQuickControls()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdatedConversationDetails));
            Commands.SetSync.RegisterCommand(new DelegateCommand<bool>(SetSync));
            Commands.SetSync.Execute(false);
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
            }); ;
        }
        private void SetSync(bool sync)
        {
            var synced = new Uri(Directory.GetCurrentDirectory() + "\\Resources\\SyncRed.png");
            var deSynced = new Uri(Directory.GetCurrentDirectory() + "\\Resources\\SyncGreen.png");
            BitmapImage source;
            if (Globals.synched)
            {
                source = new BitmapImage(synced);
                try
                {
                    var teacherSlide = (int)Globals.teacherSlide;
                    if (Globals.location.availableSlides.Contains(teacherSlide) && !Globals.isAuthor)
                        Commands.MoveTo.Execute((int)Globals.teacherSlide);
                }
                catch (NotSetException) { }
            }
            else
            {
                source = new BitmapImage(deSynced);
            }
            Dispatcher.adoptAsync(() => syncButton.Icon = source);
        }
        private void toggleSync(object sender, RoutedEventArgs e)
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
                (App.controller.client.location.currentSlide, Globals.me, "submission", Privacy.Public, -1L, hostedFileName, Globals.conversationDetails.Title, new Dictionary<string, Color>(), Globals.generateId(hostedFileName)));
                MeTLMessage.Information("Submission sent to " + Globals.conversationDetails.Author);
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
