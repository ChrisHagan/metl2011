using MeTLLib;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Profiles;
using SandRibbon.Providers;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SandRibbon.Pages.Identity
{
    public partial class ProfileSelectorPage : ServerAwarePage
    {
        public ProfileSelectorPage(MetlConfiguration _backend, IEnumerable<Profile> profiles) : base(_backend)
        {
            InitializeComponent();
            this.profiles.ItemsSource = profiles;
        }

        private void ChooseProfile(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var profile = button.DataContext as Profile;
            Globals.currentProfile = profile;
            NavigationService.Navigate(new ChooseCollaborationContextPage(ServerConfig));
        }

        private void AddProfile(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateProfilePage(ServerConfig));
        }
    }
}
