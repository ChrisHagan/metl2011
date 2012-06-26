using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Divelements.SandRibbon;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.BannedContent;
using SandRibbon.Providers;

namespace SandRibbon.Tabs
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConversationManagement : RibbonTab
    {
        public List<TargettedSubmission> submissionList = new List<TargettedSubmission>();
        public ConversationManagement()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(updateConversationDetails));
            Commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(receiveSubmission));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(PreParserAvailable));
            Commands.ViewBannedContent.RegisterCommand(new DelegateCommand<object>(viewBannedContent, canViewBannedContent));
        }
        private void viewBannedContent(object _obj)
        {
            var bannedContent = new BannedContent(submissionList);
            bannedContent.Owner = Window.GetWindow(this);
            bannedContent.Show();
        }
        private bool canViewBannedContent(object _e)
        {
           return submissionList.Count > 0;
        }
        private void PreParserAvailable(PreParser parser)
        {
            foreach(var submission in parser.submissions)
                receiveSubmission(submission);
        }
        private void updateConversationDetails(ConversationDetails details)
        {
            editConversation.Visibility = details.Author == Globals.me ? Visibility.Visible : Visibility.Collapsed;
            banContent.Visibility = Globals.isAuthor ? Visibility.Visible : Visibility.Collapsed;
        }
        private void receiveSubmission(MeTLLib.DataTypes.TargettedSubmission submission)
        {
            if (submission.target != "bannedcontent")
                return;

            if (!IHaveThisSubmission(submission))
            {
                submissionList.Add(submission);
                Commands.RequerySuggested(Commands.ViewBannedContent);
            }
        }
        private bool IHaveThisSubmission(MeTLLib.DataTypes.TargettedSubmission submission)
        {
            if (submissionList.Where(s => s.time == submission.time && s.author == submission.author && s.url == submission.url).ToList().Count > 0)
                return true;
            return false;
        }

        private void OnBanContentChanged(object sender, RoutedEventArgs e)
        {
            Globals.IsManagementAccessible = banContent.IsChecked ?? false; 
        }
    }
}
