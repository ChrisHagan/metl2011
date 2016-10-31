using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MeTLLib;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace SandRibbon
{
    public partial class Window1
    {
        protected DispatcherTimer retryTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);

        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main() {
            SandRibbon.App app = new SandRibbon.App();
            app.InitializeComponent();
            app.Run();
        }
        public Window1()
        {
            InitializeComponent();
            retryTimer.Interval = new TimeSpan(0, 0, 5);
            retryTimer.Tick += (s, e) => {
                refreshServerListing();
            };
            refreshServerListing();
        }
        protected List<MeTLConfigurationProxy> availableServers = new List<MeTLConfigurationProxy>();
        public void refreshServerListing()
        {
            Console.WriteLine("tick!");
            retryTimer.Stop();
            try
            {
                status.Text = "fetching available servers";
                workingProgress.Visibility = Visibility.Visible;
                status.Visibility = Visibility.Visible;
                MetlConfigurationManager metlConfigManager = new RemoteAppMeTLConfigurationManager(null);
                var servers = metlConfigManager.servers;
                Console.WriteLine(String.Format("found servers: {0}", servers.Count));
                workingProgress.Visibility = Visibility.Collapsed;
                status.Visibility = Visibility.Collapsed;
                availableServers.Clear();
                foreach (var s in servers)
                {
                    availableServers.Add(s);
                }
                possibleServers.ItemsSource = null;
                possibleServers.ItemsSource = availableServers;
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("exception finding servers: {0}", e.Message));
                availableServers.Clear();
                status.Text = "error attempting to fetch the available servers.  Retrying.";
                workingProgress.Visibility = Visibility.Visible;
                status.Visibility = Visibility.Visible;
                retryTimer.Start();
            }
        }
        
        private void Server_Click(object sender, RoutedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            var dc = (MeTLConfigurationProxy)fe.DataContext;
            System.Diagnostics.Process.Start(dc.host.ToString());
        }
    }
}
