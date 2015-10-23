using System.Collections.ObjectModel;

namespace SandRibbon.Pages.Conversations.Models
{
    public class PickContext
    {
        public ObservableCollection<string> Files { get; set; } = new ObservableCollection<string>();
    }
}
