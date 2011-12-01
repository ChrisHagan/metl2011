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
using System.Windows.Shapes;
using System.Threading;
using System.Net.NetworkInformation;

namespace LectureMeTL
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, EventArgs e)
        {
            bool CanConnect = false;
            try
            {
                var client = new System.Net.WebClient();
                var currentServerXML = client.DownloadString("http://metl.adm.monash.edu.au/server.xml");
                var currentServer = currentServerXML.Split(new char[] { '>', '<' })[2];
                var ping = new Ping();
                var pingResponse = ping.Send(currentServer);
                if (pingResponse.Status == IPStatus.Success)
                    CanConnect = true;
            }
            catch (Exception)
            {
                MessageBox.Show("MeTL cannot contact the server.  Please check that you are connected to the internet.");
            }
            if (CanConnect)
            {
                var p = System.Diagnostics.Process.Start("metl.exe");
                Thread.Sleep(6000);
            }
            App.Current.Shutdown();
        }

    }
}
