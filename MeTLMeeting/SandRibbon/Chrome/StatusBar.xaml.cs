using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Properties;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System;

namespace SandRibbon.Chrome
{
    public partial class StatusBar : Divelements.SandRibbon.StatusBar
    {
        public StatusBar()
        {
            InitializeComponent();
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(SetPrivacy));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<ConversationDetails>(JoinConversation));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<object>(SetIdentity));
            Commands.BanhammerActive.RegisterCommand(new DelegateCommand<object>(BanhammerActive));
        }
        private void SetIdentity(object o)
        {
            showDetails();
        }
        private void SetPrivacy(string _privacy)
        {
            showDetails();
        }
        private void JoinConversation(ConversationDetails _jid)
        {
            showDetails();
        }
        private void BanhammerActive(object b)
        {
            showDetails();
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            showDetails();
        }
        private void showDetails()
        {
            try
            {
                Dispatcher.adopt(() =>
                {
                    var details = Globals.conversationDetails;
                    var status = "";
                    if (details.UserIsBlackListed(Globals.me))
                    {
                        status = "Banned for inappropriate content: public exposure has been disabled";
                    }
                    else if (Globals.IsBanhammerActive)
                    {
                        status = "Administer content mode is active.  You may edit other people's content.";
                    }
                    else
                    {
                        status = details.IsEmpty || Globals.location.activeConversation.IsEmpty ? Strings.Global_ProductName : string.Format(
                             "{0} is working {1}ly in an {2} conversation",
                             Globals.me,
                             Globals.privacy,
                             details.Subject
                             );
                    }
                    collaborationStatus.Text = string.Format("Collaboration {0}", details.Permissions.studentCanWorkPublicly ? "ENABLED" : "DISABLED");
                    followStatus.Text = string.Format("Following teacher {0}", details.Permissions.usersAreCompulsorilySynced ? "MANDATORY" : "OPTIONAL");
                    StatusLabel.Text = status;
                });
            }
            catch (NotSetException)
            {
            }
        }
    }
}
