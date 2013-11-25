using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using MeTLLib;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Sandpit;
using SandRibbon.Providers;
using SandRibbon.Components.Utility;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using mshtml;

namespace SandRibbon.Components
{
    public class UserCredentials : IDataErrorInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string credential]
        {
            get
            {
                string result = ""; 
                if (credential == "Username")
                {
                    if (string.IsNullOrEmpty(Username))
                        result = "Please enter your username";
                }
                if (credential == "Password")
                {
                    if (string.IsNullOrEmpty(Password))
                        result = "Please enter your password";
                }
                return result;
            }
        }
    }

    public partial class Login : UserControl
    {
        public static RoutedCommand CheckAuthentication = new RoutedCommand();
        public static RoutedCommand LoginPending = new RoutedCommand();
        public string Version { get; private set; }
        protected WebBrowser logonBrowser;
        protected List<Uri> browseHistory = new List<Uri>();
        public Login()
        {
            InitializeComponent();
            this.DataContext = this;
            var failingCredentials = new Credentials("forbidden", "", null, "");
            App.Login(failingCredentials);
            ResetWebBrowser(null);
            SandRibbon.App.CloseSplashScreen();
            Commands.AddWindowEffect.ExecuteAsync(null);
#if DEBUG
            Version = String.Format("{0} Build: {1}", ConfigurationProvider.instance.getMetlVersion(), "not merc"); //SandRibbon.Properties.HgID.Version); 
#else
            Version = ConfigurationProvider.instance.getMetlVersion();
#endif
            Commands.LoginFailed.RegisterCommand(new DelegateCommand<object>(ResetWebBrowser));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<Credentials>(SetIdentity));
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
            showTimeoutButton.Change(loginTimeout,Timeout.Infinite);
        }
        protected void showBrowser()
        {
            hideResetButton();
            logonBrowserContainer.Visibility = Visibility.Visible;
            loadingImage.Visibility = Visibility.Collapsed;
            logonBrowserContainer.IsHitTestVisible = true;
        }
        protected void showResetButton()
        {
            restartLoginProcessContainer.Visibility = Visibility.Visible;
            showTimeoutButton.Change(Timeout.Infinite,Timeout.Infinite);
        }
        protected void hideResetButton()
        {
            restartLoginProcessContainer.Visibility = Visibility.Collapsed;
            showTimeoutButton.Change(Timeout.Infinite,Timeout.Infinite);
        }
        protected void DeleteCookieForUrl(Uri uri)
        {
            try
            {
                DateTime expiration = DateTime.UtcNow - TimeSpan.FromDays(1);
                var newCookie = Application.GetCookie(uri);
                var cookieNameGroup = newCookie.Split(';')[0];
                if (cookieNameGroup != null)
                {
                    var cookieName = cookieNameGroup.Split('=')[0];
                    if (cookieName != null)
                    {
                        newCookie = String.Format("{0}=; expires={1}; path=/; domani={2}", cookieName, expiration.ToString("R"), uri.ToString());
                        Application.SetCookie(uri, newCookie);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Failed to delete cookie for: " + uri.ToString());
            }
        }
        protected void DestroyWebBrowser(object _unused)
        {
            if (logonBrowser != null)
            {
                logonBrowserContainer.Children.Clear();
                logonBrowser.Dispose();
                logonBrowser = null;
                browseHistory.ForEach((uri) => DeleteCookieForUrl(uri));
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
            logonBrowser = new WebBrowser();
            logonBrowserContainer.Children.Add(logonBrowser);
            logonBrowser.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            logonBrowser.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            logonBrowser.Width = Double.NaN;
            logonBrowser.Height = Double.NaN;
            logonBrowser.Navigating += (sender, args) =>
            {
                hideBrowser();
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
            logonBrowser.LoadCompleted += (sender, args) => {
                var doc = ((sender as WebBrowser).Document as HTMLDocument);
                var authResult = checkWhetherWebBrowserAuthenticationSucceeded(doc);
                if (!authResult)
                {
                    showBrowser();
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
            hideBrowser();
            logonBrowser.Navigate(loginUri);
        }
        protected List<XElement> getElementsByTag(List<XElement> x, String tagName){ 
            // it's not recursive!
            var children = x.Select(xel => { return getElementsByTag(xel.Elements().ToList(), tagName); });
            var root = x.FindAll((xel) =>
            {
                return xel.Name.ToString().Trim().ToLower() == tagName.Trim().ToLower();
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
        protected bool checkWhetherWebBrowserAuthenticationSucceeded(HTMLDocument doc){
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
                var options = ClientFactory.Connection().UserOptionsFor(identity.name);
                Commands.SetUserOptions.Execute(options);
                Commands.SetPedagogyLevel.Execute(Pedagogicometer.level((Pedagogicometry.PedagogyCode)options.pedagogyLevel));
                DestroyWebBrowser(null); 
                this.Visibility = Visibility.Collapsed;
            });
            App.mark("Login knows identity");
            Commands.ShowConversationSearchBox.ExecuteAsync(null);
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new LoginAutomationPeer(this);
        }
    }
    class LoginAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public LoginAutomationPeer(Login parent) : base(parent) { }
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
                return this;
            return base.GetPattern(patternInterface);
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public void SetValue(string value)
        {
            //Constants.JabberWire.SERVER = value;
        }
        public string Value
        {
            get { return ""; } //Constants.JabberWire.SERVER; }
        }
    }
}
