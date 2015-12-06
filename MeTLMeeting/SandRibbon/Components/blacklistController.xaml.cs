using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Components
{
    public partial class BlacklistController : Window
    {
        ObservableCollection<string> blacklistedUsersList = new ObservableCollection<string>(); 
        List<string> blacklist = new List<string>();        
        public BlacklistController()
        {            
            InitializeComponent();
            var rootPage = DataContext as DataContextRoot;
            blacklistedUsers.ItemsSource = blacklistedUsersList;
            blacklist = rootPage.ConversationState.Blacklist.Distinct().ToList();
            foreach(var user in blacklist)
                blacklistedUsersList.Add(user);
        }
        private void updateBlacklist(object sender, RoutedEventArgs e)
        {
            var rootPage = DataContext as DataContextRoot;
            rootPage.ConversationState.Blacklist = blacklist;
            rootPage.ConversationState.Broadcast();
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
