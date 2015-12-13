using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Tabs.Groups
{
    public partial class ToolBox : UserControl
    {
        public ToolBox()
        {
            InitializeComponent();
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(SetLayer));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(joinConversation));
            Commands.ChangeTextMode.RegisterCommand(new DelegateCommand<string>(changeTextMode));

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
            Commands.SetTextCanvasMode.Execute(mode);
        }
        private void joinConversation(object obj)
        {
            type.IsChecked = true;
            Commands.SetTextCanvasMode.Execute("None");
        }
        private void SetLayer(string layer)
        {
            Dispatcher.adopt(delegate
            {

                hideAll();
                this.Visibility = Visibility.Visible;
                switch (layer)
                {
                    case "Text":
                        TextOptions.Visibility = Visibility.Visible;
                        Commands.TogglePens.ExecuteAsync(false);
                        break;
                    case "Insert":
                        ImageOptions.Visibility = Visibility.Visible;
                        Commands.TogglePens.ExecuteAsync(false);
                        break;
                    default:
                        this.Visibility = Visibility.Collapsed;
                        Commands.TogglePens.ExecuteAsync(true);
                        //InkOptions.Visibility = Visibility.Visible;
                        break;
                }
            });
        }
        private void hideAll()
        {
            Dispatcher.adopt(delegate
            {

                TextOptions.Visibility = Visibility.Collapsed;
                ImageOptions.Visibility = Visibility.Collapsed;
            });
        }
    }
}