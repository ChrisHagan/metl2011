using MeTLLib;
using SandRibbon.Pages.Login;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SandRibbon.Pages.ServerSelection
{
    public partial class ServerSelectorPage : Page
    {
        public ServerSelectorPage(List<MetlConfiguration> configs)
        {
            InitializeComponent();
            this.DataContext = configs;
        }

        private void servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = e.AddedItems[0] as MetlConfiguration;
            //App.SetBackend(selection);
            //var backend = App.controller.client.server;
            Commands.BackendSelected.Execute(selection);
            NavigationService.Navigate(new LoginPage(selection));
        }
    }
}
