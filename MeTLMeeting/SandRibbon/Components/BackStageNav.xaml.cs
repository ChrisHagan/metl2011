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

namespace SandRibbon.Components
{
    public partial class BackStageNav : UserControl
    {
        public BackStageNav()
        {
            InitializeComponent();
        }
        public string currentMode { 
            get{
                return new[]{help,mine,all, find}.Where(box=>box.IsChecked.Value).First().Name;
            }
        }
        private void mode_Checked(object sender, RoutedEventArgs e)
        {
            var mode = ((FrameworkElement)sender).Name;
            Commands.BackstageModeChanged.Execute(mode);
        }
    }
}
