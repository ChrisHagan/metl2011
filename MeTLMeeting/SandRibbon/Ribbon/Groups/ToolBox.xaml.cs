using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Tabs.Groups
{
    public partial class ToolBox : UserControl
    {
        protected MeTLLib.MetlConfiguration backend;

        public ToolBox()
        {
            InitializeComponent();
            AppCommands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
            App.getContextFor(backend).controller.commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(joinConversation));
            AppCommands.ChangeTextMode.RegisterCommand(new DelegateCommand<string>(changeTextMode));

        }
        private void changeTextMode(string mode)
        {
            switch (mode.ToLower())
            {
                case "none":
                    type.IsChecked = true;
                    break;
                default:
                    select.IsChecked = true;
                    break;
            }
            AppCommands.SetTextCanvasMode.Execute(mode);
        }
        private void joinConversation(object obj)
        {
            type.IsChecked = true;
            AppCommands.SetTextCanvasMode.Execute("None");
        }
        private void SetLayer(string layer)
        {
            hideAll();
            this.Visibility = Visibility.Visible;
            switch (layer)
            {
                case "Text":
                    TextOptions.Visibility = Visibility.Visible;
                    AppCommands.TogglePens.ExecuteAsync(false);
                    break;
                case "Insert":
                    ImageOptions.Visibility = Visibility.Visible;
                    AppCommands.TogglePens.ExecuteAsync(false);
                    break;
                default:
                    this.Visibility = Visibility.Collapsed;
                    AppCommands.TogglePens.ExecuteAsync(true);
                    //InkOptions.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void hideAll()
        {
            TextOptions.Visibility = Visibility.Collapsed;
            ImageOptions.Visibility = Visibility.Collapsed;
        }
    }
}