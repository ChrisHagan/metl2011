using MeTLLib;
using SandRibbon.Pages.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Pages.ServerSelection
{
    public partial class ServerSelectorPage : Page
    {
        public ServerSelectorPage()
        {
            InitializeComponent();
            DataContext = App.availableServers().ToList().Concat(new List<MeTLConfigurationProxy> {
                new MeTLConfigurationProxy("localhost",new Uri("http://localhost:8080/static/images/puppet.jpg"),new System.Uri("http://localhost:8080",UriKind.Absolute))
            });            
        }        
        private void ServerSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            var source = sender as FrameworkElement;
            var selection = source.DataContext as MeTLConfigurationProxy;
            App.SetBackendProxy(selection);
            Commands.BackendSelected.Execute(selection);
            NavigationService.Navigate(new LoginPage(selection));
        }
    }
}
