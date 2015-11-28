using MeTLLib;
using SandRibbon.Pages.Login;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            alwaysShow = _alwaysShow;
            ready = false;
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
                    else
                    {
                        SetValue(visibleProperty, Visibility.Visible);

                    }
                    SetValue(loadingVisibleProperty, value ? Visibility.Collapsed : Visibility.Visible);
                    _ready = value;
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
            set { SetValue(visibleProperty, value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible); }
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



        public ImageSource imageSource
        {
            get { return (ImageSource)GetValue(imageSourceProperty); }
            set
            {
                SetValue(imageSourceProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for imageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty imageSourceProperty = DependencyProperty.Register("imageSource", typeof(ImageSource), typeof(ServerChoice), new PropertyMetadata(null));
    }
    public class ServerCollection : ObservableCollection<ServerChoice>
    {
        public void refreshDisplay(ServerChoice s, bool removed)
        {
            if (!s.alwaysShow)
            {
                if (removed)
                {
                    OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Remove, s));
                }
                else
                {
                    OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, s));
                }
            }
        }
        public int enabledServers
        {
            get
            {
                var newCount = this.Count(s => s.alwaysShow || s.ready);
                return newCount;
            }
        }
    }
    public partial class ServerSelectorPage : Page
    {
        protected int recheckInterval = 3000;
        //protected Timer dispatcherTimer;
        protected ServerCollection servers = new ServerCollection();
        protected List<Timer> timers = new List<Timer>();
        public ServerSelectorPage()
        {
            InitializeComponent();
            foreach (ServerChoice sc in App.availableServers().Select(server => new ServerChoice(server, true)))
            {
                servers.Add(sc);
            }
            DataContext = servers;
            timers = servers.Concat(new List<ServerChoice> {
                new ServerChoice(new MeTLConfigurationProxy("localhost",new Uri("http://localhost:8080/static/images/puppet.jpg"),new System.Uri("http://localhost:8080",UriKind.Absolute)),false)
            }).ToList().Select(sc => new Timer(delegate
            {
                var wc = new WebClient();
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
                if (oldState != newState && !sc.alwaysShow)
                {
                    Dispatcher.adopt(delegate
                    {
                        if (newState)
                        {
                            servers.Add(sc);
                        }
                        else
                        {
                            servers.Remove(sc);
                        }
                    });
                }
            }, null, Timeout.Infinite, Timeout.Infinite)).ToList();
            Unloaded += (s, e) =>
            {
                timers.ForEach(t =>
                {
                    t.Change(Timeout.Infinite, Timeout.Infinite);
                    t.Dispose();
                    t = null;
                });
                timers.Clear();
            };
            Loaded += (s, e) =>
            {
                timers.ForEach(t =>
                {
                    t.Change(5000, recheckInterval);
                });
            };
        }
        private void ServerSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            timers.ForEach(t =>
            {
                t.Change(Timeout.Infinite, Timeout.Infinite);
            });
            var source = sender as FrameworkElement;
            var selection = source.DataContext as ServerChoice;
            App.SetBackendProxy(selection.server);
            Commands.BackendSelected.Execute(selection);
            NavigationService.Navigate(new LoginPage(selection.server));
        }
    }
}
