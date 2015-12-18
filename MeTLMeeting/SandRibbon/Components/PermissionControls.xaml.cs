using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Pages;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Components
{
    public partial class PermissionControls : UserControl
    {
        public SlideAwarePage rootPage { get; protected set; }
        public ConversationDetails ConversationDetails { get; protected set; }

        public PermissionControls()
        {
            InitializeComponent();
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(UpdatedConversationDetails);                        
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                {
                    rootPage = DataContext as SlideAwarePage;
                }
                ConversationDetails = rootPage.ConversationDetails;
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);                                                
                UpdatedConversationDetails(rootPage.ConversationDetails);
            };
            Unloaded += (s, e) =>
            {
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);                
            };
        }

        protected void UpdatedConversationDetails(ConversationDetails conv)
        {            
            Dispatcher.adopt(delegate
            {
                studentCanPublishCheckbox.IsChecked = conv.Permissions.studentCanPublish;
                studentMustFollowTeacherCheckbox.IsChecked = conv.Permissions.usersAreCompulsorilySynced;
            });
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
            var val = (bool)(sender as CheckBox).IsChecked;
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
            cd.Permissions.studentsCanViewQuiz = val;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
        private void StudentsCanDisplayQuizResultsChecked(object sender, RoutedEventArgs e)
        {
            var val = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.ConversationDetails;
            cd.Permissions.studentsCanDisplayQuizResults = val;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
        private void StudentsMustFollowTeacherChecked(object sender, RoutedEventArgs e)
        {
            var studentsMustFollowTeacherValue = (bool)(sender as CheckBox).IsChecked;
            var cd = rootPage.ConversationDetails;
            cd.Permissions.usersAreCompulsorilySynced = studentsMustFollowTeacherValue;
            rootPage.NetworkController.client.UpdateConversationDetails(cd);
        }
    }
}
