using SandRibbon.Pages.Conversations.Models;
using System.Windows.Controls;

namespace SandRibbon.Pages.Collaboration
{
    public partial class OneNotePage : Page
    {
        public OneNotePage(NotebookPage page)
        {
            InitializeComponent();
            DataContext = page;
            wb.NativeViewInitialized += delegate {
                wb.LoadHTML(page.Html);
            };            
        }        
    }
}
