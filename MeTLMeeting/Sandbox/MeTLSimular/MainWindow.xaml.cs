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
using MeTLLib;

namespace MeTLSimular
{
    public partial class MainWindow : Window{
        ClientConnection conn;
        public MainWindow(){
            InitializeComponent();
            Loaded += onLoad;
        }
        private void onLoad(object sender, RoutedEventArgs e) {
            conn = ClientFactory.Connection();
            conn.Connect("chagan","vidcajun2");
        }
    }
}
