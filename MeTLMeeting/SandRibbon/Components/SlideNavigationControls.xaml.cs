using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.DataTypes;

namespace SandRibbon.Components
{
    public partial class SlideNavigationControls : UserControl
    {
        public SlideNavigationControls()
        {
            InitializeComponent();
            this.PreviewKeyDown += KeyPressed;
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageUp || e.Key == Key.Up)
            {
                if(Commands.MoveToPrevious.CanExecute(null))
                  Commands.MoveToPrevious.Execute(null);
                e.Handled = true;
            }
            if (e.Key == Key.PageDown || e.Key == Key.Down)
            {
                if(Commands.MoveToNext.CanExecute(null))
                  Commands.MoveToNext.Execute(null);
                e.Handled = true;
            }
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (ConversationDetails.Empty.Equals(details)) return;
            Dispatcher.adopt(delegate
            {
                nav.Visibility = Visibility.Visible;
            });
        }
    }
}
