using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using agsXMPP;
using SandRibbon.Utils.Connection;

namespace SandRibbon.Components
{
    public partial class ProviderMonitor : UserControl
    {
        private static ProviderMonitor self;
        static ProviderMonitor()
        {
            JabberWire.LookupServer();
        }
        public ProviderMonitor()
        {
            InitializeComponent();
            self = this;
        }
        public static ObservableCollection<ServerStatus> SERVERS =
                new ObservableCollection<ServerStatus>(){
                    new ServerStatus{
                        label="Resource", 
                        CheckStatus=(server)=>server.Ping(Constants.JabberWire.SERVER)},
                    new ServerStatus{
                        label="Messaging", 
                        CheckStatus=(server)=>{
                            var conn= new XmppClientConnection(Constants.JabberWire.SERVER);
                            conn.AutoAgents = false;
                            conn.OnReadXml += (_sender,xml)=>{
                                if(Application.Current != null)
                                    Application.Current.Dispatcher.BeginInvoke((Action)delegate
                                    {
                                        server.ok=true;
                                    });
                            };
                            conn.Open("NOT_AUTHORIZED", "aCriminal");
                        }},
                    new ServerStatus{
                        label="Authentication", 
                        CheckStatus=(server)=>server.Ping("my.monash.edu.au")}
                };
        public static void HealthCheck(Action healthyBehaviour)
        {
            var currentStack = new System.Diagnostics.StackTrace();
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                try
                {
                    foreach (var server in SERVERS)
                        server.ok = false;
                    checkServers();
                    int attempts = 0;
                    const int MILIS_BETWEEN_TRIES = 500;
                    var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(MILIS_BETWEEN_TRIES), DispatcherPriority.Normal,
                    (sender, args) =>
                    {
                        var brokenServers = SERVERS.Where(s => !s.ok);
                        attempts++;
                        if (brokenServers.Count() == 0)
                        {
                            self.Visibility = Visibility.Collapsed;
                            ((DispatcherTimer)sender).Stop();
                            healthyBehaviour();
                        }
                    }, Application.Current.Dispatcher);
                    timer.Start();
                }
                catch (Exception e)
                {
                    throw new Exception(currentStack.ToString(), e);
                }
            });
        }
        private static void checkServers()
        {
            foreach (var server in SERVERS)
                server.CheckStatus(server);
        }
    }
    public class ServerStatus : DependencyObject
    {
        public string label
        {
            get { return (string)GetValue(labelProperty); }
            set { SetValue(labelProperty, value); }
        }
        public static readonly DependencyProperty labelProperty =
            DependencyProperty.Register("label", typeof(string), typeof(ServerStatus), new UIPropertyMetadata("Nothing"));
        public bool ok
        {
            get { return (bool)GetValue(okProperty); }
            set { SetValue(okProperty, value); }
        }
        public static readonly DependencyProperty okProperty =
            DependencyProperty.Register("ok", typeof(bool), typeof(ServerStatus), new UIPropertyMetadata(false));
        public Action<ServerStatus> CheckStatus;
        private bool alreadyRetried = false;
        public void Ping(string uri)
        {
            var ping = new System.Net.NetworkInformation.Ping();
            ping.PingCompleted += (_sender, pingArgs) =>
            {
                if (pingArgs.Reply != null && pingArgs.Reply.Status == IPStatus.Success)
                {
                    ok = true;
                }
                else
                {
                    ok = false;
                    if (!alreadyRetried)
                    {
                        ping.SendAsync(uri, null);//Try again
                        alreadyRetried = true;
                    }
                }
                CommandManager.InvalidateRequerySuggested();
            };
            ping.SendAsync(uri, null);
        }
    }
}