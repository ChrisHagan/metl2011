using SandRibbon.Components;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Profiles;
using SandRibbon.Providers;
using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.Windows.Navigation;

namespace SandRibbon.Pages.Identity
{
    public partial class ProfileSelectorPage : ServerAwarePage
    {
        public ProfileSelectorPage(UserGlobalState _userGlobal, UserServerState _userServer, NetworkController _networkController, IEnumerable<Profile> profiles)
        {
            NetworkController = _networkController;
            UserGlobalState = _userGlobal;
            UserServerState = _userServer;
            InitializeComponent();
            this.profiles.ItemsSource = profiles;
        }

        private void ChooseProfile(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var profile = button.DataContext as Profile;
            Globals.currentProfile = profile;
            NavigationService.Navigate(new ChooseCollaborationContextPage(NetworkController));
        }

        private void AddProfile(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateProfilePage(UserGlobalState,UserServerState,NetworkController));
        }
    }
}
