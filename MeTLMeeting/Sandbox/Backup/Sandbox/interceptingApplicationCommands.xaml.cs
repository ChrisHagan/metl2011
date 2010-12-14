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

namespace Sandbox
{
    /// <summary>
    /// Interaction logic for interceptingApplicationCommands.xaml
    /// </summary>
    public partial class interceptingApplicationCommands : UserControl
    {
        public interceptingApplicationCommands()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, doIT));
        }

        private void doIT(object sender, ExecutedRoutedEventArgs e)
        {
            var a = 1;
            e.Handled = false;
        }

        private void select(object sender, RoutedEventArgs e)
        {
            canvas.EditingMode = InkCanvasEditingMode.Select; 
        }

        private void alwaysTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
