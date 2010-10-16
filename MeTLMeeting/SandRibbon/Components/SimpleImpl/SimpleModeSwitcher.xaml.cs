using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using System;
using System.Threading;

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
    }
}
