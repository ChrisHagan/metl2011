using System.Collections.Generic;
using System.Linq;
using System.Windows;
//using Divelements.SandRibbon;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.BannedContent;
using SandRibbon.Providers;
using System.Windows.Input;
using SandRibbon.Utils;
using System.Windows.Controls.Ribbon;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages;

namespace SandRibbon.Tabs
{
    public partial class ConversationManagement : RibbonTab
    {
        public List<TargettedSubmission> submissionList = new List<TargettedSubmission>();
        public static RoutedCommand ManageBannedContent = new RoutedCommand();
        public SlideAwarePage rootPage { get; protected set; }
        public ConversationManagement()
        {
            InitializeComponent();
            var updateConvCommand = new DelegateCommand<ConversationDetails>(updateConversationDetails);
            var receiveScreenshotCommand = new DelegateCommand<TargettedSubmission>(receiveSubmission);
            var preparserAvailableCommand = new DelegateCommand<PreParser>(PreParserAvailable);
            var viewBannedContentCommand = new DelegateCommand<object>(viewBannedContent, canViewBannedContent);
            var manageBannedContentCommand = new DelegateCommand<object>(OnBanContentchanged, CheckManageBannedAllowed);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as SlideAwarePage;
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateConvCommand);
                Commands.ReceiveScreenshotSubmission.RegisterCommand(receiveScreenshotCommand);
                Commands.PreParserAvailable.RegisterCommand(preparserAvailableCommand);
                Commands.ViewBannedContent.RegisterCommand(viewBannedContentCommand);
                Commands.ManageBannedContent.RegisterCommand(manageBannedContentCommand);
            };
            Unloaded += (s, e) =>
            {
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateConvCommand);
                Commands.ReceiveScreenshotSubmission.RegisterCommand(receiveScreenshotCommand);
                Commands.PreParserAvailable.RegisterCommand(preparserAvailableCommand);
                Commands.ViewBannedContent.RegisterCommand(viewBannedContentCommand);
                Commands.ManageBannedContent.RegisterCommand(manageBannedContentCommand);
            };
        }

        private void viewBannedContent(object _obj)
        {
            /*
            var bannedContent = new BannedContent(submissionList);
            bannedContent.Owner = Window.GetWindow(this);
            bannedContent.Show();
            banContent.IsChecked = false;
            Commands.BanhammerActive.Execute(false);
            ManageBannedContent.Execute(null, null);
            */
        }

        private bool canViewBannedContent(object _e)
        {
            return submissionList.Count > 0;
        }

        private bool CheckManageBannedAllowed(object sender)
        {
            return true;
        }

        private void PreParserAvailable(PreParser parser)
        {
            foreach (var submission in parser.submissions)
                receiveSubmission(submission);
        }
        private void updateConversationDetails(ConversationDetails details)
        {
            editConversation.Visibility = details.Author == rootPage.NetworkController.credentials.name ? Visibility.Visible : Visibility.Collapsed;
            banContent.Visibility = rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name) ? Visibility.Visible : Visibility.Collapsed;
            bannedContentManagement.Visibility = banContent.Visibility;
        }

        private void JoinConversation(string jid)
        {
            submissionList.Clear();
        }

        private void receiveSubmission(MeTLLib.DataTypes.TargettedSubmission submission)
        {
            if (string.IsNullOrEmpty(submission.target) || submission.target != "bannedcontent")
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

        private void OnBanContentchanged(object sender)
        {
            var banMode = banContent.IsChecked ?? false;
            Commands.BanhammerActive.Execute(banMode);
            if (banMode)
            {
                Commands.SetInkCanvasMode.Execute("Select");
            }
        }
    }
}
