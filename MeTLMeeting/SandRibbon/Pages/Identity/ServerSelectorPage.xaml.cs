using MeTLLib;
using SandRibbon.Pages.Login;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Pages.ServerSelection
{
    public class Server
    {
        public String image { get; set; }
        public MeTLServerAddress.serverMode mode { get; set; }
    }
    public partial class ServerSelectorPage : Page
    {
        public ServerSelectorPage()
        {
            InitializeComponent();
            servers.ItemsSource = new Dictionary<String, Server>
            {
                {
                    "Saint Leo University",
                    new Server {
                        image = "/Resources/slu.jpg",
                        mode = MeTLServerAddress.serverMode.PRODUCTION
                    }
                },
                {
                    "Open MeTL Server",
                    new Server {
                        image = "/Resources/splashScreen.png",
                        mode = MeTLServerAddress.serverMode.STAGING
                    }
                }
            };
            servers.SelectedIndex = 1;
        }

        private void SetBackend(object sender, RoutedEventArgs e)
        {
            serversContainer.Visibility = Visibility.Collapsed;
            Commands.RemoveWindowEffect.ExecuteAsync(null);
            var selection = ((KeyValuePair<String, Server>)servers.SelectedItem).Value;
            App.SetBackend(selection.mode);
            var backend = App.controller.client.server;
            Commands.BackendSelected.Execute(backend);
            NavigationService.Navigate(new LoginPage(backend));
        }
    }
}
