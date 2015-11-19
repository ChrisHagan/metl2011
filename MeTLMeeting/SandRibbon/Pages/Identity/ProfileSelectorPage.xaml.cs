using SandRibbon.Components;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Profiles;
using SandRibbon.Providers;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SandRibbon.Pages.Identity
{
    public partial class ProfileSelectorPage : Page
    {
        protected NetworkController networkController;
        public ProfileSelectorPage(NetworkController _networkController, IEnumerable<Profile> profiles)
        {
            networkController = _networkController;
            InitializeComponent();
            this.profiles.ItemsSource = profiles;
        }

        private void ChooseProfile(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var profile = button.DataContext as Profile;
            Globals.currentProfile = profile;
            NavigationService.Navigate(new ChooseCollaborationContextPage(networkController));
        }

        private void AddProfile(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateProfilePage(networkController));
        }
    }
}
