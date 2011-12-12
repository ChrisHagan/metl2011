using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Components.SimpleImpl
{
    /// <summary>
    /// Interaction logic for SimpleModeSwitcher.xaml
    /// </summary>
    public partial class SimpleModeSwitcher : UserControl
    {
        public SimpleModeSwitcher()
        {
            InitializeComponent();
            Commands.JoinConversation.RegisterCommandToDispatcher<object>(new DelegateCommand<object>(JoinConversation));
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
        }

        private void SetLayer(string layer)
        {
            switch(layer)
            {
                case "Insert":
                    Image.IsChecked = true;
                    break;
                case "Text":
                    Text.IsChecked = true;
                    break;
                case "View":
                    View.IsChecked = true;
                    break;
                default:
                    Pen.IsChecked = true;
                    break;
            }
        }
        private void JoinConversation(object obj)
        {
            Commands.SetLayer.ExecuteAsync("Sketch");
            Pen.IsChecked = true;
        }

        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void CutButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationCommands.Cut.Execute(null, null);
        }

        private void PasteClick(object sender, RoutedEventArgs e)
        {
            ApplicationCommands.Paste.Execute(null, null);
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            ApplicationCommands.Copy.Execute(null, null);
        }
    }
}
