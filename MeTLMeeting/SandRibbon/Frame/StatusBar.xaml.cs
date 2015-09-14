using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Properties;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System;
using MeTLLib;

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
            Commands.SetIdentity.RegisterCommandToDispatcher(new DelegateCommand<object>((_unused) => SetIdentity()));
            Commands.BanhammerActive.RegisterCommandToDispatcher(new DelegateCommand<bool>((_unused) => BanhammerActive()));
        }
        private void SetIdentity()
        {
            showDetails();
        }
        private void SetPrivacy(string _privacy)
        {
            showDetails();
        }
        private void JoinConversation(string _jid)
        {
            showDetails();
        }
        private void BanhammerActive() 
        {
            showDetails();
        }
        private void UpdateConversationDetails(ConversationDetails details) 
        {
            // commented out next line because we want to update the status bar if the details have changed in all cases
            //if (details.IsEmpty) return;
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
                        status = details.IsEmpty || String.IsNullOrEmpty(Globals.location.activeConversation) ? Strings.Global_ProductName : string.Format(
                             "{3} is working {0}ly in {1} style, in a conversation whose participants are {2}",
                             Globals.privacy,
                             MeTLLib.DataTypes.Permissions.InferredTypeOf(details.Permissions).Label,
                             details.Subject, Globals.me);
                    }
#if DEBUG
                    var activeStack = MeTLConfiguration.Config.ActiveStack;
                    status += String.Format(" | ({0}) Connected to [{1}] {2}", String.IsNullOrEmpty(Globals.me) ? "Unknown" : Globals.me, 
                        activeStack.Name, activeStack.Host);
#endif
                    StatusLabel.Text = status;
                });
            }
            catch(NotSetException)
            {
            }
        }
    }
}
