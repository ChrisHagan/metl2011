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
using System.Net;

namespace Sandbox
{
    public partial class ResourceUpload : Window
    {
        public ResourceUpload()
        {
            InitializeComponent();
        }
        private void File(object sender, RoutedEventArgs e)
        {
            var res = new WebClient().UploadFile("http://drawkward.adm.monash.edu:1188/upload_nested.yaws?path=Resource&overwrite=true","C:\\1.gif");
            MessageBox.Show(Encoding.UTF8.GetString(res));
        }
        private void Data(object sender, RoutedEventArgs e)
        {
            var res = new WebClient().UploadData("http://drawkward.adm.monash.edu:1188/upload_nested.yaws?path=Resource&overwrite=true&filename=Chris", Encoding.UTF8.GetBytes("<chris made xml />"));
            MessageBox.Show(Encoding.UTF8.GetString(res));
        }
    }
}