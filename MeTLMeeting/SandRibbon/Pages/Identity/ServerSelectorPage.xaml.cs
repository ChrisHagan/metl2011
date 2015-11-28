using MeTLLib;
using SandRibbon.Pages.Login;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Pages.ServerSelection
{
    public class ServerChoice : DependencyObject
    {
        public string name { get; protected set; }
        public Uri baseUri { get; protected set; }
        public Uri imageUri { get; protected set; }
        public MeTLConfigurationProxy server { get; protected set; }
        public ServerChoice(MeTLConfigurationProxy _server, bool _alwaysShow)
        {
            server = _server;
            imageUri = _server.imageUrl;
            baseUri = _server.host;
            name = _server.name;
            ready = true;
            ready = false;
            alwaysShow = _alwaysShow;
        }
        protected bool _ready = false;
        public bool ready
        {
            get
            { return _ready; }
            set
            {
                Dispatcher.adopt(delegate
                {
                    SetValue(enabledProperty, value);
                    if (!alwaysShow)
                    {
                        SetValue(visibleProperty, value ? Visibility.Visible : Visibility.Collapsed);
                    }
                    SetValue(loadingVisibleProperty, value ? Visibility.Collapsed : Visibility.Visible);
                    _ready = true;
                });
            }
        }
        public bool alwaysShow { get; protected set; }
        public Visibility visible
        {
            get { return (Visibility)GetValue(visibleProperty); }
            set { SetValue(visibleProperty, value); }
        }
        public Visibility loadingVisible
        {
            get { return (Visibility)GetValue(visibleProperty) == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; }
            set { SetValue(visibleProperty, value  == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible); }
        }

        public bool enabled
        {
            get { return (bool)GetValue(enabledProperty); }
            set { SetValue(enabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty enabledProperty = DependencyProperty.Register("enabled", typeof(bool), typeof(ServerChoice), new PropertyMetadata(false));
        public static readonly DependencyProperty visibleProperty = DependencyProperty.Register("visible", typeof(Visibility), typeof(ServerChoice), new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty loadingVisibleProperty = DependencyProperty.Register("loadingVisible", typeof(Visibility), typeof(ServerChoice), new PropertyMetadata(Visibility.Collapsed));



    }
    public class ServerCollection : ObservableCollection<ServerChoice>
    {
    }
    public partial class ServerSelectorPage : Page
    {
        protected int recheckInterval = 3000;
        protected Timer dispatcherTimer;
        protected ServerCollection servers = new ServerCollection();
        public ServerSelectorPage()
        {
            InitializeComponent();
            foreach (ServerChoice sc in App.availableServers().Select(server => new ServerChoice(server, true)).ToList().Concat(new List<ServerChoice> {
                new ServerChoice(new MeTLConfigurationProxy("localhost",new Uri("http://localhost:8080/static/images/puppet.jpg"),new System.Uri("http://localhost:8080",UriKind.Absolute)),false)
            }))
            {
                servers.Add(sc);
            }
            DataContext = servers;
            dispatcherTimer = new Timer(delegate
            {
                var wc = new WebClient();
                foreach (ServerChoice sc in servers)
                {
                    var oldState = sc.ready;
                    var newState = false;
                    try
                    {
                        newState = wc.DownloadString(sc.server.serverStatus).Trim().ToLower() == "ok";
                    }
                    catch
                    {

                    }
                    sc.ready = newState;
                    /*
                    if (oldState != newState)
                    {
                        servers.
                    }
                    */
                }
            }, null, 0, recheckInterval);
        }
        private void ServerSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            dispatcherTimer.Change(Timeout.Infinite, Timeout.Infinite);
            var source = sender as FrameworkElement;
            var selection = source.DataContext as MeTLConfigurationProxy;
            App.SetBackendProxy(selection);
            Commands.BackendSelected.Execute(selection);
            NavigationService.Navigate(new LoginPage(selection));
        }
    }
}
