using MeTLLib;
using SandRibbon.Pages.Login;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SandRibbon.Pages.ServerSelection
{
    /*
    public class Server
    {
        public String image { get; set; }
        public MetlConfiguration config { get; set; }
    }
    */
    public partial class ServerSelectorPage : Page
    {
        public ServerSelectorPage()
        {
            InitializeComponent();
            servers.ItemsSource = App.availableServers();/* List<MetlConfiguration>(); new Dictionary<String, Server>
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
            */
        }

        private void servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = e.AddedItems[0] as MetlConfiguration;// ((KeyValuePair<String, Server>)e.AddedItems[0]).Value;
            App.SetBackend(selection);
            //var backend = App.controller.client.server;
            Commands.BackendSelected.Execute(selection);
            NavigationService.Navigate(new LoginPage(selection));
        }
    }
}
