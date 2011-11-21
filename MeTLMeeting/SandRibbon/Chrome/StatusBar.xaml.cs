using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using MeTLLib.DataTypes;

namespace SandRibbon.Chrome
{
    public partial class StatusBar : Divelements.SandRibbon.StatusBar
    {
        public StatusBar()
        {
            InitializeComponent();
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(SetPrivacy));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
#if DEBUG
            StatusLabel.Text = "You are operating against the staging server.";
#endif
        }
        private void SetPrivacy(string _privacy)
        {
            showDetails();
        }
        private void JoinConversation(string _jid)
        {
            showDetails();
        }
        private void UpdateConversationDetails(ConversationDetails details) 
        {
            if (details.IsEmpty) return;
            showDetails();
        }
        private void showDetails()
        {
            try
            {
                Dispatcher.adopt(() =>
                {
                    var details = Globals.conversationDetails;
                    StatusLabel.Text = details.IsEmpty?"MeTL 2011":string.Format(
                            "{3} is working {0}ly in {1} style, in a conversation whose participants are {2}",
                            Globals.privacy,
                            MeTLLib.DataTypes.Permissions.InferredTypeOf(details.Permissions).Label,
                            details.Subject, Globals.me);
                });
            }
            catch(NotSetException)
            {
            }
        }
    }
}
