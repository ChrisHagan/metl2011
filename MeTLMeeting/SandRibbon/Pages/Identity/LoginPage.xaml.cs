using Awesomium.Core;
using Awesomium.Windows.Controls;
using MeTLLib;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using mshtml;
using SandRibbon.Components.Sandpit;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.Conversations;
using SandRibbon.Pages.Identity;
using SandRibbon.Providers;
using SandRibbon.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using SandRibbon.Components;
using System.Windows.Navigation;
using SandRibbon.Pages.Conversations.Models;
using System.Xml;
using System.Diagnostics;

namespace SandRibbon.Pages.Login
{

    public class CookieTrackingMeTLResourceInterceptor : IResourceInterceptor
    {
        protected List<Uri> uriWatchList = new List<Uri>();
        protected Action<Uri, Cookie> onCookie = (u, c) => { };
        public CookieTrackingMeTLResourceInterceptor(List<Uri> cookiesToWatch, Action<Uri, Cookie> onCookieDetect)
        {
            onCookie = onCookieDetect;
            uriWatchList = cookiesToWatch;
        }
        bool IResourceInterceptor.OnFilterNavigation(NavigationRequest request)
        {
            return false;
            //throw new NotImplementedException();
        }

        ResourceResponse IResourceInterceptor.OnRequest(ResourceRequest request)
        {
            if (uriWatchList.Exists(u => u.Host == request.Url.Host))
            {
                var wq = request.ToWebRequest((HttpWebRequest)HttpWebRequest.Create(request.Url), new CookieContainer());
                wq.GetResponse(); //I feel bad about making the request twice, just to get the cookie.
                foreach (var uri in uriWatchList)
                {
                    if (wq.SupportsCookieContainer && wq.CookieContainer != null)
                    {
                        foreach (var cookie in wq.CookieContainer.GetCookies(uri))
                        {
                            onCookie(uri, cookie as Cookie);
                        }
                    }
                }
            }
            return null;
        }
    }

    public partial class LoginPage : GlobalAwarePage
    {
        public static RoutedCommand CheckAuthentication = new RoutedCommand();
        public static RoutedCommand LoginPending = new RoutedCommand();
        public MeTLConfigurationProxy backend { get; set; }
        protected WebControl logonBrowser;
        protected List<Uri> browseHistory = new List<Uri>();
        public UserGlobalState userGlobal { get; protected set; }
        public LoginPage(UserGlobalState _userGlobal, MeTLConfigurationProxy _backend)
        {
            UserGlobalState = _userGlobal;
            backend = _backend;
            InitializeComponent();
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            ResetWebBrowser(null);
        }
        protected Timer showTimeoutButton;
        protected int loginTimeout = 5 * 1000;
        protected void restartLoginProcess(object sender, RoutedEventArgs e)
        {
            ResetWebBrowser(null);
        }
        protected void showResetButton()
        {
            restartLoginProcessContainer.Visibility = Visibility.Visible;
            showTimeoutButton.Change(Timeout.Infinite, Timeout.Infinite);
        }
        protected void ResetWebBrowser(object _unused)
        {
            var loginUri = backend.authenticationUrl;
            logonBrowser = new WebControl();
            logonBrowser.ShowContextMenu += (s, a) =>
            {
                a.Handled = true;
            };
            logonBrowser.ShowPopupMenu += (s, a) =>
            {
                a.Cancel = true;
                a.Handled = true;
            };
            logonBrowser.ShowCreatedWebView += (sender, e) =>
            {
                var nwc = new WebControl();
                logonBrowserContainer.Children.Add(nwc);
                nwc.NativeView = e.NewViewInstance;
            };
            logonBrowserContainer.Children.Add(logonBrowser);
            var loginAttempted = false;
            logonBrowser.DocumentReady += (sender, args) =>
            {
                if (loginAttempted) return;
                var html = (sender as WebControl).HTML;
                if (html.Contains("authdata"))
                {
                    var xml = XDocument.Parse(html).Elements().ToList();
                    var authData = getElementsByTag(xml, "authdata");
                    var usernameNode = getElementsByTag(authData, "username").First();
                    var authGroupsNodes = getElementsByTag(authData, "authGroup");
                    var infoGroupsNodes = getElementsByTag(authData, "infoGroup");
                    var username = usernameNode.Value.ToString();
                    var authGroups = authGroupsNodes.Select((xel) => new AuthorizedGroup(xel.Attribute("name").Value.ToString(), xel.Attribute("type").Value.ToString())).ToList();
                    var authenticated = getElementsByTag(authData, "authenticated").First().Value.ToString().Trim().ToLower() == "true";
                    var emailAddressNode = infoGroupsNodes.Find((xel) => xel.Attribute("type").Value.ToString().Trim().ToLower() == "emailaddress");
                    var emailAddress = "";
                    if (emailAddressNode != null)
                    {
                        emailAddress = emailAddressNode.Attribute("name").Value.ToString();
                    }
                    if (authenticated)
                    {
                        try
                        {
                            var JSESSIONID = logonBrowser.ExecuteJavascriptWithResult("document.cookie");
                            Commands.Mark.Execute("Login");
                            var newServer = App.metlConfigManager.parseConfig(backend, authData.First()).First();
                            var credentials = new Credentials(newServer.xmppUsername, newServer.xmppPassword, authGroups, emailAddress);
                            credentials.cookie = JSESSIONID;
                            var controller = new NetworkController(newServer);
                            controller.connect(credentials);
                            if (!controller.client.Connect(credentials))
                            {
                                Commands.LoginFailed.Execute(null);
                            }
                            else
                            {
                                loginAttempted = true;
                                var userServer = new UserServerState();
                                userServer.AuthenticatedWebSession = logonBrowser.WebSession;
                                userServer.OneNoteConfiguration = new OneNoteConfiguration
                                {
                                    apiKey = "exampleApiKey",
                                    apiSecret = "exampleApiSecret",
                                    networkController = controller
                                };
                                userServer.ThumbnailProvider = new ThumbnailProvider(controller);
                                logonBrowser.Stop();
                                logonBrowser.Dispose();
                                NavigationService.Navigate(new ConversationSearchPage(userGlobal, userServer, controller, controller.credentials.name));
                            }
                        }
                        catch (TriedToStartMeTLWithNoInternetException)
                        {
                            Commands.Mark.Execute("Internet not found");
                            Commands.LoginFailed.Execute(null);
                            Commands.NoNetworkConnectionAvailable.Execute(null);
                        }
                    }
                };
                if (showTimeoutButton != null)
                {
                    showTimeoutButton.Change(Timeout.Infinite, Timeout.Infinite);
                    showTimeoutButton.Dispose();
                    showTimeoutButton = null;
                }
                showTimeoutButton = new System.Threading.Timer((s) =>
                {
                    Dispatcher.adoptAsync(delegate
                    {
                        showResetButton();
                    });
                }, null, Timeout.Infinite, Timeout.Infinite);     
            };
            logonBrowser.Source = loginUri;
        }
        protected List<XElement> getElementsByTag(List<XElement> x, String tagName)
        {
            // it's not recursive!
            var children = x.Select(xel => { return getElementsByTag(xel.Elements().ToList(), tagName); });
            var root = x.FindAll((xel) =>
            {
                return xel.Name.LocalName.ToString().Trim().ToLower() == tagName.Trim().ToLower();
            });
            foreach (List<XElement> child in children)
            {
                root.AddRange(child);
            }
            return root;
        }

        private void SetIdentity(Credentials identity)
        {
            Commands.RemoveWindowEffect.ExecuteAsync(null);
            //var options = App.controller.client.UserOptionsFor(identity.name);
            //Commands.SetUserOptions.Execute(options);
            Globals.loadProfiles(identity);
            Commands.Mark.Execute("Identity is established");
        }
    }
}
