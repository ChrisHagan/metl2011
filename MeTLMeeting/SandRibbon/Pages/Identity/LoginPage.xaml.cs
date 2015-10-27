using Awesomium.Windows.Controls;
using MeTLLib;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using mshtml;
using SandRibbon.Components.Sandpit;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.Identity;
using SandRibbon.Providers;
using SandRibbon.Utils;
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

    public partial class LoginPage : ServerAwarePage
    {
        public static RoutedCommand CheckAuthentication = new RoutedCommand();
        public static RoutedCommand LoginPending = new RoutedCommand();
        protected WebControl logonBrowser;
        protected List<Uri> browseHistory = new List<Uri>();
        public LoginPage(MetlConfiguration _backend) : base(_backend)
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
        protected void ResetWebBrowser(object _unused)
        {
            var loginUri = ServerContext.config.authenticationUrl;
            DeleteCookieForUrl(new Uri(loginUri));
            logonBrowser = new WebControl();
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
                    var credentials = new Credentials(username, "", authGroups, emailAddress);
                    if (authenticated)
                    {
                        try
                        {
                            ServerContext.initController(credentials);
                            Commands.Mark.Execute("Login");
                            if (ServerContext.controller.client.Connect(credentials))
                            {
                                Commands.LoginFailed.Execute(null);
                            }
                            else
                            {
                                loginAttempted = true;
                                Commands.SetIdentity.Execute(credentials);
                            }
                        }
                        catch (TriedToStartMeTLWithNoInternetException)
                        {
                            Commands.Mark.Execute("Internet not found");
                            Commands.LoginFailed.Execute(null);
                            Commands.NoNetworkConnectionAvailable.Execute(null);
                        }
                        NavigationService.Navigate(new ProfileSelectorPage(ServerConfig,Globals.profiles));
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
                logonBrowser.Source = new Uri(loginUri);
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
            var options = ServerContext.controller.client.UserOptionsFor(identity.name);
            Commands.SetUserOptions.Execute(options);
            Globals.loadProfiles(identity);            
            Commands.Mark.Execute("Identity is established");
        }
    }
}
