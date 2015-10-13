using SandRibbon.Providers;
using System.Collections.ObjectModel;

namespace SandRibbon.Pages.Conversations.Models
{
    public class OneNoteConfiguration
    {
        public ObservableCollection<Notebook> Books {get;set;} = new ObservableCollection<Notebook>();
        public string apiKey { get; set; }
        public string apiSecret { get; set; }
    }
}
