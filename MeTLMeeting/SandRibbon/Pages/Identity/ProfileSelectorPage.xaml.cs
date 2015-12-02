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
        public NetworkController networkController { get; protected set; }
        public UserGlobalState userGlobal { get; protected set; }
        public UserServerState userServer { get; protected set; }
        public ProfileSelectorPage(UserGlobalState _userGlobal, UserServerState _userServer, NetworkController _networkController, IEnumerable<Profile> profiles)
        {
            networkController = _networkController;
            userGlobal = _userGlobal;
            userServer = _userServer;
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
            NavigationService.Navigate(new CreateProfilePage(userGlobal,userServer,networkController));
        }

        public NetworkController getNetworkController()
        {
            return networkController;
        }

        public UserServerState getUserServerState()
        {
            return userServer;
        }

        public UserGlobalState getUserGlobalState()
        {
            return userGlobal;
        }

        public NavigationService getNavigationService()
        {
            return NavigationService;
        }
    }
}
