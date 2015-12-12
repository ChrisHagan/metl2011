using Awesomium.Core;
using Awesomium.Windows.Controls;
using SandRibbon.Components;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.Conversations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SandRibbon.Pages.Integration
{
    /// <summary>
    /// Interaction logic for OneNoteListingPage.xaml
    /// </summary>
    public partial class OneNoteListingPage : ServerAwarePage
    {
        public OneNoteConfiguration OneNoteConfiguration { get; protected set; }
        public OneNoteListingPage(UserGlobalState userGlobalState,UserServerState userServerState, NetworkController networkController, OneNoteConfiguration config)
        {
            config.LoadNotebooks();
            UserGlobalState = userGlobalState;
            UserServerState = userServerState;
            NetworkController = networkController;
            OneNoteConfiguration = config;
            InitializeComponent();
        }
        protected void SelectNotebook(object sender, RoutedEventArgs e) {
            var page = (NotebookPage)((FrameworkElement)sender).DataContext;
            NavigationService.Navigate(new OneNotePage(UserGlobalState, UserServerState, NetworkController, OneNoteConfiguration, page));
        }
    }
}
