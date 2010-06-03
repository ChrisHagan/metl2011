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
using Org.TechA.Wpf.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Utils;

namespace SandRibbon.Components
{
    public partial class RubyRepl : UserControl
    {
        public RubyRepl()
        {
            InitializeComponent();
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
        }
        private void MoveTo(int slide) {
            DataContext = slide;
        }
        private void launchRepl(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Logger.log);
            Clipboard.SetText(Logger.log);
        }
    }
}
