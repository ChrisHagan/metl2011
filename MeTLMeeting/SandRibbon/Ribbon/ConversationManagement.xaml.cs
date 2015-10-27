using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Divelements.SandRibbon;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.BannedContent;
using SandRibbon.Providers;
using System.Windows.Input;
using SandRibbon.Utils;
using MeTLLib;

namespace SandRibbon.Tabs
{
    public partial class ConversationManagement : RibbonTab
    {
        public List<TargettedSubmission> submissionList = new List<TargettedSubmission>();
        public static RoutedCommand ManageBannedContent = new RoutedCommand();
        public MetlConfiguration backend;
        public ConversationManagement()
        {
            InitializeComponent();
            App.getContextFor(backend).controller.commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(updateConversationDetails));
            App.getContextFor(backend).controller.commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<string>(JoinConversation));
            App.getContextFor(backend).controller.commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(receiveSubmission));
            App.getContextFor(backend).controller.commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(PreParserAvailable));
            AppCommands.ViewBannedContent.RegisterCommand(new DelegateCommand<object>(viewBannedContent, canViewBannedContent));
        }

        private void viewBannedContent(object _obj)
        {
            var bannedContent = new BannedContent(backend,submissionList);
            bannedContent.Owner = Window.GetWindow(this);
            bannedContent.Show();
            banContent.IsChecked = false;
            AppCommands.BanhammerActive.Execute(false);
            ManageBannedContent.Execute(null, null);
        }

        private bool canViewBannedContent(object _e)
        {
            return submissionList.Count > 0;
        }

        private void CheckManageBannedAllowed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = StateHelper.mustBeInConversation();
        }

        private void PreParserAvailable(PreParser parser)
        {
            foreach (var submission in parser.submissions)
                receiveSubmission(submission);
        }
        private void updateConversationDetails(ConversationDetails details)
        {
            editConversation.Visibility = details.Author == App.getContextFor(backend).controller.creds.name ? Visibility.Visible : Visibility.Collapsed;
            banContent.Visibility = Globals.isAuthor(App.getContextFor(backend).controller.creds.name) ? Visibility.Visible : Visibility.Collapsed;
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
                AppCommands.RequerySuggested(AppCommands.ViewBannedContent);
            }
        }

        private bool IHaveThisSubmission(MeTLLib.DataTypes.TargettedSubmission submission)
        {
            if (submissionList.Where(s => s.time == submission.time && s.author == submission.author && s.url == submission.url).ToList().Count > 0)
                return true;
            return false;
        }

        private void OnBanContentchanged(object sender, ExecutedRoutedEventArgs e)
        {
            var banMode = banContent.IsChecked ?? false;
            AppCommands.BanhammerActive.Execute(banMode);
            if (banMode)
            {
                AppCommands.SetInkCanvasMode.Execute("Select");
            }
        }
    }
}
