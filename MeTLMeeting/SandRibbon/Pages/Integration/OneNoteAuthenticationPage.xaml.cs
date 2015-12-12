using Awesomium.Core;
using Awesomium.Windows.Controls;
using SandRibbon.Components;
using SandRibbon.Pages.Conversations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SandRibbon.Pages.Integration
{
    /// <summary>
    /// Interaction logic for OneNoteAuthenticationPage.xaml
    /// </summary>
    public partial class OneNoteAuthenticationPage : ServerAwarePage
    {
        public OneNoteAuthenticationPage(UserGlobalState userGlobalState, UserServerState userServerState, NetworkController networkController, Func<UserGlobalState,UserServerState,NetworkController,OneNoteConfiguration,ServerAwarePage> onAuthenticated, OneNoteConfiguration config) : base()
        { 
            UserGlobalState = userGlobalState;
            UserServerState = userServerState;
            NetworkController = networkController;
            InitializeComponent();
            if (config.token != null)
            {
                NavigationService.Navigate(onAuthenticated(userGlobalState,userServerState,networkController,config));
            }
            else
            {
                var w = new WebControl();
                DocumentReadyEventHandler ready = null;
                ready = (s, e) =>
                {
                    var queryPart = e.Url.AbsoluteUri.Split('#');
                    if (queryPart.Length > 1)
                    {
                        var ps = HttpUtility.ParseQueryString(queryPart[1]);
                        var token = ps["access_token"];
                        if (token != null)
                        {
                            w.DocumentReady -= ready;
                            config.token = token;
                            NavigationService.Navigate(onAuthenticated(userGlobalState, userServerState, networkController, config));
                        }
                    }
                };
                w.DocumentReady += ready;
                Content = w;
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
                w.Source = uri;
            }
        }
    }
}
