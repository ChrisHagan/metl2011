using SandRibbon.Pages.Conversations;
using System.Collections.Generic;
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
                new CollaborationContext { code=1, label="I'm going to class",image=(Canvas)TryFindResource("appbar_chat")},
                new CollaborationContext { code=2, label="I'm studying",image=(Canvas)TryFindResource("appbar_book_open_hardcover")},
                new CollaborationContext { code=3, label="I'm looking for someone",image=(Canvas)TryFindResource("appbar_user_add")},
                new CollaborationContext { code=4, label="I just need help",image=(Canvas)TryFindResource("appbar_book_perspective_help")}
            };
        }                
        private void collaborationContexts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = (CollaborationContext)e.AddedItems[0];
            switch (selection.code)
            {
                case 1:
                    NavigationService.Navigate(new ConversationSearchPage());
                    break;
                default:
                    NavigationService.Navigate(new ConversationSearchPage());
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
