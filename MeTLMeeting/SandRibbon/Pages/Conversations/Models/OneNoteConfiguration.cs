using SandRibbon.Providers;
using System.Collections.ObjectModel;
using System;

namespace SandRibbon.Pages.Conversations.Models
{
    public class OneNoteConfiguration
    {
        public ObservableCollection<Notebook> Books { get; set; } = new ObservableCollection<Notebook>();
        public string apiKey { get; set; }
        public string apiSecret { get; set; }

        public delegate void PagesProcessingStatus(int current, int max);
        public event PagesProcessingStatus PagesProcessingProgress;
        public delegate void PageProcessingStatus(int current, int max, string title);
        public event PageProcessingStatus PageProcessingProgress;
        
        internal void ReportPagesProgress(int v1, int v2)
        {
            PagesProcessingProgress(v1, v2);
        }

        internal void ReportPageProgress(int processed, int pageCount, string title)
        {
            PageProcessingProgress(processed, pageCount, title);
        }
    }    
}
