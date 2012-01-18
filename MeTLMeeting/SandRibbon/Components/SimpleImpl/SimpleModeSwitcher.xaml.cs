using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Diagnostics;
using SandRibbon.Providers;

namespace SandRibbon.Components.SimpleImpl
{
    public enum InputMode
    {
        Pen,
        Image,
        Text,
        View
    }

    public class SimpleModeSwitcherUIState
    {
        public InputMode CurrentInputMode; 
    }
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
            Commands.SaveUIState.RegisterCommandToDispatcher<object>(new DelegateCommand<object>(SaveUIState));
            Commands.RestoreUIState.RegisterCommandToDispatcher<object>(new DelegateCommand<object>(RestoreUIState));
        }

        private void SaveUIState(object parameter)
        {
            var saveState = new SimpleModeSwitcherUIState();

            if (Pen.IsChecked ?? false)
                saveState.CurrentInputMode = InputMode.Pen;
            if (Image.IsChecked ?? false)
                saveState.CurrentInputMode = InputMode.Image;
            if (Text.IsChecked ?? false)
                saveState.CurrentInputMode = InputMode.Text;
            if (View.IsChecked ?? false)
                saveState.CurrentInputMode = InputMode.View;

            Globals.StoredUIState.SimpleModeSwitcherUIState = saveState; 
        }

        private void RestoreUIState(object parameter)
        {
            var saveState = Globals.StoredUIState.SimpleModeSwitcherUIState;
            string inputMode = "Sketch";
            if (saveState != null)
            {
                switch (saveState.CurrentInputMode)
                {
                    case InputMode.Pen:
                        inputMode = "Sketch";
                        break;
                
                    case InputMode.Image:
                        inputMode = "Insert";
                        break;
                
                    case InputMode.Text:
                        inputMode = "Text";
                        break;
                    
                    case InputMode.View:
                        inputMode = "View";
                        break;

                    default:
                        inputMode = "Sketch";
                        break;
                }
            }
            Commands.SetLayer.ExecuteAsync(inputMode);
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
            Commands.ClipboardManager.Execute(ClipboardAction.Cut);
        }

        private void PasteClick(object sender, RoutedEventArgs e)
        {
            Commands.ClipboardManager.Execute(ClipboardAction.Paste);
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            Commands.ClipboardManager.Execute(ClipboardAction.Copy);
        }
    }
}
