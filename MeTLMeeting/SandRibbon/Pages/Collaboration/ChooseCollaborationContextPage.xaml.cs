using SandRibbon.Pages.ConversationSearch;
using System.Collections.Generic;
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
            collaborationContexts.ItemsSource = new List<CollaborationContext>
            {
                new CollaborationContext { code=1, label="I want to go to class",image=(Canvas)TryFindResource("appbar_user")},
                new CollaborationContext { code=2, label="I'm looking for someone",image=(Canvas)TryFindResource("appbar_user_add")},
                new CollaborationContext { code=3, label="I just need help",image=(Canvas)TryFindResource("appbar_book_perspective_help")}
            };
            collaborationContexts.SelectedIndex = 0;
        }        
        private void connect_Click(object sender, RoutedEventArgs e)
        {
            var selection = (CollaborationContext) collaborationContexts.SelectedItem;
            switch (selection.code) {
                case 1: NavigationService.Navigate(new ConversationSearchPage());
                    break;
                default: NavigationService.Navigate(new ConversationSearchPage());
                    break;
            }
        }
    }
    public class CollaborationContext {
        public string label { get; set; }
        public Canvas image { get; set; }
        public int code { get; set; }
    }
}
