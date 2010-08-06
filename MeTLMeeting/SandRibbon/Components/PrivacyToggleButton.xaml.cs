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
        public PrivacyToggleButton(string mode, Rect bounds)
        {
            InitializeComponent();
            System.Windows.Controls.Canvas.SetLeft(privacyButtons, bounds.Right);
            System.Windows.Controls.Canvas.SetTop(privacyButtons, bounds.Top);
            if (mode == "show")
            {
                showButton.Visibility = Visibility.Visible;
                hideButton.Visibility = Visibility.Collapsed;
            
            }
            else if(mode == "hide")
            {
                showButton.Visibility = Visibility.Collapsed;
                hideButton.Visibility = Visibility.Visible;
            }
            else
            {
                showButton.Visibility = Visibility.Visible;
                hideButton.Visibility = Visibility.Visible;
            }
        }
        private void showContent(object sender, RoutedEventArgs e)
        {
            Commands.SetPrivacyOfItems.Execute("public");
        }
        private void hideContent(object sender, RoutedEventArgs e)
        {
            Commands.SetPrivacyOfItems.Execute("private");
        }

        private void deleteContent(object sender, RoutedEventArgs e)
        {
            Commands.DeleteSelectedItems.Execute(null);
        }
        public class PrivacyToggleButtonInfo
        {
            public string privacyChoice;
            public Rect ElementBounds;
            public PrivacyToggleButtonInfo(string privacy, Rect bounds)
            {
                privacyChoice = privacy;
                ElementBounds = bounds;
            }
        }
    }
}
