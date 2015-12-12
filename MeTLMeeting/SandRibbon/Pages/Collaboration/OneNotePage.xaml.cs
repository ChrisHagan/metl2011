using SandRibbon.Components;
using SandRibbon.Pages.Conversations.Models;
using System.Windows.Controls;

namespace SandRibbon.Pages.Collaboration
{
    public partial class OneNotePage : ServerAwarePage
    {
        public OneNotePage(UserGlobalState userGlobalState, UserServerState userServerState, NetworkController networkController, OneNoteConfiguration config, NotebookPage page) : base()
        {
            UserGlobalState = userGlobalState;
            UserServerState = userServerState;
            NetworkController = networkController;
            InitializeComponent();
            DataContext = page;
            wb.NativeViewInitialized += delegate {
                wb.LoadHTML(page.Html);
            };            
        }        
    }
}
