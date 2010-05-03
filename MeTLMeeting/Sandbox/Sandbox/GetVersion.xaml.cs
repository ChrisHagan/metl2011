using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for GetVersion.xaml
    /// </summary>
    public partial class GetVersion : UserControl
    {
        private static string MetL = "http://drawkward.adm.monash.edu/MeTL/";
        public GetVersion()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var client = new WebClient();
            var html = client.DownloadString(MetL);
            if(html.Contains("Version:"))
            {
                var start = html.IndexOf("Version:");
                var line = html.Substring(start, 100);
                var versionRegex = new Regex("([\\d\\.]{7,})");
                var version = versionRegex.Match(line).Value;
                MessageBox.Show(version);
            }
            else
               MessageBox.Show("Button Clicked");

        }
    }
}
