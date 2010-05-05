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

namespace SandRibbon.Components.Sandpit
{
    public partial class S15Boards : UserControl
    {
        public S15Boards()
        {
            InitializeComponent();
            boardDisplay.ItemsSource = BoardManager.boards["S15"];
            Commands.WakeUp.RegisterCommand(new DelegateCommand<object>(_nothing=>boardSelector.IsOpen = true));
        }
        private void ClosePopup(object sender, RoutedEventArgs e)
        {
            boardSelector.IsOpen = false;
        }
    }
}
