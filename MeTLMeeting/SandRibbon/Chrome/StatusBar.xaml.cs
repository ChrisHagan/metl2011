using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbonObjects;
using System.Threading;
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
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
        }
        private void SetPrivacy(string _privacy)
        {
            showDetails();
        }
        private void JoinConversation(string _jid)
        {
            showDetails();
        }
        private void UpdateConversationDetails(ConversationDetails _details) 
        { 
            showDetails();
        }
        private void showDetails()
        {
            try
            {
                var details = Globals.conversationDetails;
                Dispatcher.adoptAsync(delegate
                                             {
                                                 try
                                                 {
                                                     if (details == null)
                                                         StatusLabel.Text = "not in a conversation";
                                                     else
                                                         string.Format(
                                                             "{3} is working {0}ly in {1} style, in a conversation whose participants are {2}",
                                                             Globals.privacy,
                                                             MeTLLib.DataTypes.Permissions.InferredTypeOf(details.Permissions).Label,
                                                             details.Subject, Globals.me);
                                                 }
                                                 catch (NotSetException)
                                                 {
                                                 }
                                             });
            }
            catch(NotSetException)
            {
            }
        }
    }
}
