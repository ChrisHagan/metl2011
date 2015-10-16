using SandRibbon.Providers;
using System.Windows.Controls;

namespace SandRibbon.Pages.Collaboration
{
    public partial class OneNotePage : Page
    {
        public OneNotePage(NotebookPage page)
        {
            InitializeComponent();
            DataContext = page;
            wb.NavigateToString(page.Html);            
        }        
    }
}
