using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MeTLLib;
using SandRibbon.Providers;
using SandRibbon.Pages;

namespace SandRibbon.Components
{
    public partial class blacklistController : Window
    {
        ObservableCollection<string> blacklistedUsersList = new ObservableCollection<string>(); 
        List<string> blacklist = new List<string>();
        public SlideAwarePage rootPage { get; protected set; }
        public blacklistController(SlideAwarePage _rootPage)
        {
            rootPage = _rootPage;
            InitializeComponent();
            blacklistedUsers.ItemsSource = blacklistedUsersList;
            blacklist = rootPage.getDetails().blacklist.Distinct().ToList();
            foreach(var user in blacklist)
                blacklistedUsersList.Add(user);
        }
        private void updateBlacklist(object sender, RoutedEventArgs e)
        {
            var details = rootPage.getDetails();
            details.blacklist = blacklist;
            rootPage.getNetworkController().client.UpdateConversationDetails(details);
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox= ((CheckBox)sender);
            var authcate = checkbox.DataContext.ToString();
            if (checkbox.IsChecked == true)
            {
                if(!blacklist.Contains(authcate))
                    blacklist.Add(authcate);
            }
            else
            {
                if (blacklist.Contains(authcate))
                    blacklist.Remove(authcate);
            }
        }
    }
}
