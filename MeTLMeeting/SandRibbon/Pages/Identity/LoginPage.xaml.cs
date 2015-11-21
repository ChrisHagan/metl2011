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

    public partial class LoginPage : Page
    {
        public static RoutedCommand CheckAuthentication = new RoutedCommand();
        public static RoutedCommand LoginPending = new RoutedCommand();
        public MeTLConfigurationProxy backend { get; set; }
        protected WebControl logonBrowser;
        protected List<Uri> browseHistory = new List<Uri>();
        public LoginPage(MeTLConfigurationProxy _backend)
        {
            backend = _backend;
            InitializeComponent();
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            Commands.LoginFailed.RegisterCommand(new DelegateCommand<object>(ResetWebBrowser));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<Credentials>(SetIdentity));
            ResetWebBrowser(null);
        }
        protected Timer showTimeoutButton;
        protected int loginTimeout = 5 * 1000;
        protected void restartLoginProcess(object sender, RoutedEventArgs e)
        {
            ResetWebBrowser(null);
        }
        protected void hideBrowser()
        {
            logonBrowserContainer.Visibility = Visibility.Collapsed;
            loadingImage.Visibility = Visibility.Visible;
            logonBrowserContainer.IsHitTestVisible = true;
            restartLoginProcessContainer.Visibility = Visibility.Collapsed;
            showTimeoutButton.Change(loginTimeout, Timeout.Infinite);
        }
        protected void showBrowser()
        {
            loadingImage.Visibility = Visibility.Collapsed;
            hideResetButton();
            logonBrowserContainer.Visibility = Visibility.Visible;
            logonBrowserContainer.IsHitTestVisible = true;
        }
        protected void showResetButton()
        {
            restartLoginProcessContainer.Visibility = Visibility.Visible;
            showTimeoutButton.Change(Timeout.Infinite, Timeout.Infinite);
        }
        protected void hideResetButton()
        {
            restartLoginProcessContainer.Visibility = Visibility.Collapsed;
            showTimeoutButton.Change(Timeout.Infinite, Timeout.Infinite);
        }
        class UriHostComparer : IEqualityComparer<System.Uri>
        {

            public bool Equals(Uri x, Uri y)
            {
                return x.Host == y.Host;
            }

            public int GetHashCode(Uri obj)
            {
                return obj.Host.GetHashCode();
            }
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
                /*
                WebControl webControl = sender as WebControl;

                if (webControl == null)
                    return;

                if (!webControl.IsLive)
                    return;

                ChildWindow newWindow = new ChildWindow();

                if (e.IsPopup && !e.IsUserSpecsOnly)
                {
                    Int32Rect screenRect = e.Specs.InitialPosition.GetInt32Rect();

                    newWindow.NativeView = e.NewViewInstance;
                    newWindow.ShowInTaskbar = false;
                    newWindow.WindowStyle = System.Windows.WindowStyle.ToolWindow;
                    newWindow.ResizeMode = e.Specs.Resizable ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;

                    if ((screenRect.Width > 0) && (screenRect.Height > 0))
                    {
                        newWindow.Width = screenRect.Width;
                        newWindow.Height = screenRect.Height;
                    }
                    newWindow.Show();
                    if ((screenRect.Y > 0) && (screenRect.X > 0))
                    {
                        newWindow.Top = screenRect.Y;
                        newWindow.Left = screenRect.X;
                    }
                }
                else if (e.IsWindowOpen || e.IsPost)
                {
                    newWindow.NativeView = e.NewViewInstance;
                    newWindow.Show();
                }
                else
                {
                    e.Cancel = true;
                    newWindow.Source = e.TargetURL;
                    newWindow.Show();
                }
                */
            };
            logonBrowser.TargetURLChanged += (s, a) =>
            {
                Console.WriteLine("target url changed: " + a.Url.ToString());
            };
            logonBrowserContainer.Children.Add(logonBrowser);
            var loginAttempted = false;
            logonBrowser.DocumentReady += (sender, args) =>
            {
                if (loginAttempted) return;
                var html = (sender as WebControl).HTML;
                if (string.IsNullOrEmpty(html)) return;
                try
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
                            App.SetBackend(newServer);
                            var credentials = new Credentials(newServer.xmppUsername, newServer.xmppPassword, authGroups, emailAddress);
                            credentials.cookie = JSESSIONID;
                            App.controller.connect(credentials);
                            if (!App.controller.client.Connect(credentials))
                            {
                                Commands.LoginFailed.Execute(null);
                            }
                            else
                            {
                                loginAttempted = true;
                                Commands.SetIdentity.Execute(credentials);
                                Globals.authenticatedWebSession = logonBrowser.WebSession;
                                logonBrowser.Stop();
                                logonBrowser.Dispose();
                                NavigationService.Navigate(new ConversationSearchPage(App.controller));
                            }
                        }
                        catch (TriedToStartMeTLWithNoInternetException)
                        {
                            Commands.Mark.Execute("Internet not found");
                            Commands.LoginFailed.Execute(null);
                            Commands.NoNetworkConnectionAvailable.Execute(null);
                        }                        
                    }
                }
                catch (Exception e)
                {
                    Commands.Mark.Execute(e.Message);
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
            logonBrowser.NativeViewInitialized += delegate
            {
                logonBrowser.Source = loginUri;
            };
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
            var options = App.controller.client.UserOptionsFor(identity.name);
            Commands.SetUserOptions.Execute(options);
            Globals.loadProfiles(identity);
            Commands.Mark.Execute("Identity is established");
        }
    }
}
