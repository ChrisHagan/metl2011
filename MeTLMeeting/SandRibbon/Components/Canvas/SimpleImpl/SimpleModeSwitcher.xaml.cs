using System;
using System.Windows.Controls;
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
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(setDefaults));
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(setLayer));
        }

        private void setLayer(string layer)
        {
            switch(layer)
            {
                case "Insert":
                    Image.IsChecked = true;
                    break;
                case "Text":
                    Text.IsChecked = true;
                    break;
                default:
                    Pen.IsChecked = true;
                    break;
            }
        }

        private void setDefaults(object obj)
        {
            Commands.SetLayer.Execute("Sketch");
            Pen.IsChecked = true;
        }
    }
}
