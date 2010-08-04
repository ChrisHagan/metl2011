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
    /// <summary>
    /// Interaction logic for PrivacyToggleButton.xaml
    /// </summary>
    public partial class PrivacyToggleButton : UserControl
    {
        public PrivacyToggleButton(Rect bounds)
        {
            InitializeComponent();
            System.Windows.Controls.Canvas.SetLeft(privacyButton, bounds.Right);
            System.Windows.Controls.Canvas.SetTop(privacyButton, bounds.Top);
        }
    }
}
