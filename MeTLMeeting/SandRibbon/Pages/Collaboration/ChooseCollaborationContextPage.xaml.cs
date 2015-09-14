using SandRibbon.Pages.ConversationSearch;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SandRibbon.Pages.Collaboration
{
    public partial class ChooseCollaborationContextPage : Page
    {
        public ChooseCollaborationContextPage()
        {
            InitializeComponent();
        }

        private void GoToClass(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ConversationSearchPage());
        }
    }
}
