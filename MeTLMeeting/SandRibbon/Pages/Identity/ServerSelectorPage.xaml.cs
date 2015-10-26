using MeTLLib;
using SandRibbon.Pages.Login;
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
            DataContext = App.availableServers().OrderBy(s => s.displayIndex).ToList();            
        }        
        private void ServerSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            var source = sender as FrameworkElement;
            var selection = source.DataContext as MetlConfiguration;
            App.SetBackend(selection);
            Commands.BackendSelected.Execute(selection);
            NavigationService.Navigate(new LoginPage(selection));
        }
    }
}
