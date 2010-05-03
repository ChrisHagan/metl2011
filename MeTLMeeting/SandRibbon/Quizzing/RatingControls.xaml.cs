using System;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Quizzing
{
    public partial class RatingControls : UserControl
    {
        private string me;
        private string conversation;
        public static RoutedCommand RegisterInterest = new RoutedCommand();
        public RatingControls()
        {
            InitializeComponent();
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(
                who=>me=who.name));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(ConversationJoined));
            CommandBindings.Add(new CommandBinding(RegisterInterest, DoRegister, CanRegister));
        }
        private void ConversationJoined(String jid)
        {
            conversation=jid;
        }
        private void CanRegister(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = me != null && conversation != null;
        }
        private void DoRegister(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.SendWormMove.Execute(new WormMove { conversation = conversation, direction = (string)e.Parameter });
        }
    }
}
