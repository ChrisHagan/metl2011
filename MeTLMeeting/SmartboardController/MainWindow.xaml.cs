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

namespace SmartboardController
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.initializeSmartboard(this);
        }
        private void Connect_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !App.isConnectedToSmartboard;
            e.Handled = true;
        }
        private void Connect_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            App.ConnectToSmartboard();
        }
        private void Disconnect_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = App.isConnectedToSmartboard;
            e.Handled = true;
        }
        private void Disconnect_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            App.DisconnectFromSmartboard();
        }
    }
}
