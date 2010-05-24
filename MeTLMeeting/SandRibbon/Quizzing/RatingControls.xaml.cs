using System;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;

namespace SandRibbon.Quizzing
{
    public partial class RatingControls : UserControl
    {
        public static RoutedCommand RegisterInterest = new RoutedCommand();
        public RatingControls()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(RegisterInterest, DoRegister, CanRegister));
        }
        private void CanRegister(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = Globals.me != null && Globals.conversationDetails != null;
            }
            catch(NotSetException exception)
            {
                e.CanExecute = false;
            }
        }
        private void DoRegister(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.SendWormMove.Execute(new WormMove { conversation = Globals.conversationDetails.Jid, direction = (string)e.Parameter });
        }
    }
}
