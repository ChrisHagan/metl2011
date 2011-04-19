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
using Divelements.SandRibbon;
using SandRibbon.Components;
using SandRibbon.Utils;

namespace SandRibbon.Tabs
{
    public partial class AdvancedTools: RibbonTab
    {
        public AdvancedTools()
        {
            InitializeComponent();
        }

        private void manageBlacklist(object sender, RoutedEventArgs e)
        {
            new blacklistController().ShowDialog();
        }
    }
}
