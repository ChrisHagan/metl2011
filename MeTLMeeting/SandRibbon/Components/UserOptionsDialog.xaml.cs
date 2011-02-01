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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using SandRibbon.Components.Sandpit;

namespace SandRibbon.Components
{
    public partial class UserOptionsDialog : Window
    {
        public UserOptionsDialog()
        {
            InitializeComponent();
            DataContext = Globals.UserOptions;
        }
        private void Apply(object sender, RoutedEventArgs e)
        {
            Commands.SetUserOptions.Execute(DataContext);
            //this should be wired to a new command - SaveUserOptions, which is commented out in SandRibbonInterop.Commands
            //Commands.SaveUserOptions.Execute(DataContext);
            Commands.SetPedagogyLevel.Execute(Pedagogicometer.level(((UserOptions)DataContext).pedagogyLevel));
            Close();
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Reset(object sender, RoutedEventArgs e)
        {
            Commands.SetUserOptions.Execute(UserOptions.DEFAULT);
            Close();
        }
    }
}
