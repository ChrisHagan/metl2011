using SandRibbon.Pages.Conversations.Models;
using SandRibbon.Providers;
using System;
using System.Web;
using System.Windows.Controls;

namespace SandRibbon.Pages.Collaboration
{
    public partial class OneNotePage : Page
    {        
        public OneNotePage(NotebookPage page)
        {
            InitializeComponent();
            DataContext = page;
            getUserAuthorization(Globals.OneNoteConfiguration);            
        }

        private void getUserAuthorization(OneNoteConfiguration config)
        {            
            wb.Navigated += oauthNavigated;            
            var scope = "office.onenote_update";
            var responseType = "token";
            var clientId = config.apiKey;
            var redirectUri = "https://login.live.com/oauth20_desktop.srf";
            var req = "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type={2}&redirect_uri={3}";
            var uri = new Uri(String.Format(req,
                config.apiKey,
                scope,
                responseType,
                redirectUri));
            wb.Navigate(uri);
        }

        private void oauthNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var page = DataContext as NotebookPage;
            var queryPart = e.Uri.AbsoluteUri.Split('#');
            if (queryPart.Length > 1)
            {
                var ps = HttpUtility.ParseQueryString(queryPart[1]);
                var token = ps["access_token"];
                if (token != null)
                {
                    wb.Navigate(page.Html,null,null,
                        string.Format("Authorization: Bearer {0}", token));
                }
            }
        }
    }
}
