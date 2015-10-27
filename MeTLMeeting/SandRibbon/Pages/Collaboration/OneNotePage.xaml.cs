using MeTLLib;
using SandRibbon.Providers;
using System.Windows.Controls;

namespace SandRibbon.Pages.Collaboration
{
    public partial class OneNotePage : Page
    {
        protected MetlConfiguration backend;
        public OneNotePage(MetlConfiguration _backend, NotebookPage page)
        {
            backend = _backend;
            InitializeComponent();
            DataContext = page;
            wb.NativeViewInitialized += delegate {
                wb.LoadHTML(page.Html);
            };            
        }        
    }
}
