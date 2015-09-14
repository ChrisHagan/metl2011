using MeTLLib;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using mshtml;
using SandRibbon.Components.Sandpit;
using SandRibbon.Pages.Collaboration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace SandRibbon.Pages.Login
{

    public partial class LoginPage : Page
    {                
        public static RoutedCommand CheckAuthentication = new RoutedCommand();
        public static RoutedCommand LoginPending = new RoutedCommand();
        public MeTLServerAddress backend { get; set; }
        protected WebBrowser logonBrowser;
        protected List<Uri> browseHistory = new List<Uri>();
        public LoginPage(MeTLServerAddress backend)
        {
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
        protected List<String> updatedCookieKeys = new List<String> { "expires", "path", "domain" };
        protected void DeleteCookieForUrl(Uri uri)
        {
            try
            {
                DateTime expiration = DateTime.UtcNow - TimeSpan.FromDays(1);
                var newCookie = Application.GetCookie(uri);
                var finalCookies = newCookie.Split(';').Where(cps => cps.Contains('=')).Select(cps =>
                {
                    var parts = cps.Split('=');
                    return new KeyValuePair<String, String>(parts[0], parts[1]);
                });
                foreach (var cp in finalCookies)
                {
                    try
                    {
                        var replacementCookie = String.Format(@"{0}={1}; Expires={2}; Path=/; Domain={3}", cp.Key, cp.Value, expiration.ToString("R"), uri.Host);
                        Application.SetCookie(uri, replacementCookie);
                        Application.SetCookie(new Uri("http://" + uri.Host + "/"), replacementCookie);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Failed to delete cookie for: " + uri.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.ToLower().Contains("no more data")) { }
                else
                    System.Console.WriteLine("Failed to read cookie prior to deletion for: " + uri.ToString());
            }
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
        protected void DestroyWebBrowser(object _unused)
        {
            if (logonBrowser != null)
            {
                browseHistory.Distinct(new UriHostComparer()).ToList().ForEach((uri) => DeleteCookieForUrl(uri));
                logonBrowserContainer.Children.Clear();
                logonBrowser.Dispose();
                logonBrowser = null;
                browseHistory.Clear();
            }
        }
        protected Boolean detectIEErrors(Uri uri)
        {
            return (uri.Scheme == "res");
        }

        protected void ResetWebBrowser(object _unused)
        {
            var loginUri = ClientFactory.Connection().server.webAuthenticationEndpoint;
            DestroyWebBrowser(null);
            DeleteCookieForUrl(loginUri);
            logonBrowser = new WebBrowser();
            logonBrowserContainer.Children.Add(logonBrowser);
            logonBrowser.Navigating += (sender, args) =>
            {
                browseHistory.Add(args.Uri);
                var cookie = "unknown";
                try
                {
                    cookie = Application.GetCookie(args.Uri);
                }
                catch (Exception e)
                {
                }
                Console.WriteLine(String.Format("{0} => {1}", args.Uri.ToString(), cookie));
                if (detectIEErrors(args.Uri))
                {
                    if (browseHistory.Last() != null)
                    {
                        logonBrowser.Navigate(browseHistory.Last());
                    }
                    else
                    {
                        ResetWebBrowser(null);
                    }
                }
            };
            logonBrowser.LoadCompleted += (sender, args) =>
            {
                var doc = ((sender as WebBrowser).Document as HTMLDocument);
                checkWhetherWebBrowserAuthenticationSucceeded(doc);

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
            logonBrowser.Navigate(loginUri);
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
        protected Boolean checkUri(String url)
        {
            try
            {
                var uri = new Uri(url);
                browseHistory.Add(uri);
                var authenticationUri = ClientFactory.Connection().server.webAuthenticationEndpoint;
                return uri.Scheme == authenticationUri.Scheme && uri.AbsolutePath == authenticationUri.AbsolutePath && uri.Authority == authenticationUri.Authority;
            }
            catch (Exception e)
            {
                System.Console.WriteLine("exception in checking uri: " + e.Message);
                return false;
            }
        }

        protected bool checkWhetherWebBrowserAuthenticationSucceeded(HTMLDocument doc)
        {
            if (doc != null && checkUri(doc.url))
            {
                if (doc.readyState != null && (doc.readyState == "complete" || doc.readyState == "interactive"))
                {
                    try
                    {
                        var authDataContainer = doc.getElementById("authData");
                        if (authDataContainer == null) return false;
                        var html = authDataContainer.innerHTML;
                        if (html == null) return false;
                        var xml = XDocument.Parse(html).Elements().ToList();
                        var authData = getElementsByTag(xml, "authdata");
                        var authenticated = getElementsByTag(authData, "authenticated").First().Value.ToString().Trim().ToLower() == "true";
                        var usernameNode = getElementsByTag(authData, "username").First();
                        var authGroupsNodes = getElementsByTag(authData, "authGroup");
                        var infoGroupsNodes = getElementsByTag(authData, "infoGroup");
                        var username = usernameNode.Value.ToString();
                        var authGroups = authGroupsNodes.Select((xel) => new AuthorizedGroup(xel.Attribute("name").Value.ToString(), xel.Attribute("type").Value.ToString())).ToList();
                        var emailAddressNode = infoGroupsNodes.Find((xel) => xel.Attribute("type").Value.ToString().Trim().ToLower() == "emailaddress");
                        var emailAddress = "";
                        if (emailAddressNode != null)
                        {
                            emailAddress = emailAddressNode.Attribute("name").Value.ToString();
                        }
                        var credentials = new Credentials(username, "", authGroups, emailAddress);
                        if (authenticated)
                        {
                            Commands.AddWindowEffect.ExecuteAsync(null);
                            App.Login(credentials);
                            NavigationService.Navigate(new ChooseCollaborationContextPage());
                        }
                        return authenticated;
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("exception in checking auth response data: " + e.Message);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void SetIdentity(Credentials identity)
        {
            Commands.RemoveWindowEffect.ExecuteAsync(null);
            Dispatcher.adoptAsync(() =>
            {
                logonBrowserContainer.Visibility = Visibility.Collapsed;
                var options = ClientFactory.Connection().UserOptionsFor(identity.name);
                Commands.SetUserOptions.Execute(options);
                Commands.SetPedagogyLevel.Execute(Pedagogicometer.level((Components.Pedagogicometry.PedagogyCode)options.pedagogyLevel));
                DestroyWebBrowser(null);
                this.Visibility = Visibility.Collapsed;
            });
            App.mark("Login knows identity");
            Commands.ShowConversationSearchBox.ExecuteAsync(null);
        }       
    }
}
