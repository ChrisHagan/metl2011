using MeTLLib;
using SandRibbon.Pages.Login;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Pages.ServerSelection
{

    public partial class ServerSelectorPage : Page
    {
        public ServerSelectorPage()
        {
            InitializeComponent();
            servers.ItemsSource = new Dictionary<String, MeTLServerAddress.serverMode> {
                { "Saint Leo University", MeTLServerAddress.serverMode.PRODUCTION },
                { "MeTL Demo Server (this houses data in Amazon)", MeTLServerAddress.serverMode.STAGING }
            };
            servers.SelectedIndex = 1;
        }

        private void SetBackend(object sender, RoutedEventArgs e)
        {
            serversContainer.Visibility = Visibility.Collapsed;
            Commands.RemoveWindowEffect.ExecuteAsync(null);
            var backendMode = ((KeyValuePair<String, MeTLServerAddress.serverMode>)servers.SelectedItem).Value;
            App.SetBackend(backendMode);
            var backend = App.controller.client.server;
            Commands.BackendSelected.Execute(backend);
            NavigationService.Navigate(new LoginPage(backend));                   
        }
    }
}
