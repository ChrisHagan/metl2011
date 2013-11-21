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
        private bool canLoginAgain = true;
        public static RoutedCommand CheckAuthentication = new RoutedCommand();
        public static RoutedCommand LoginPending = new RoutedCommand();
        public string Version { get; private set; }
        private UserCredentials credentials = new UserCredentials();
        private int numErrorsOfCredentials = 0;
        public Login()
        {
            InitializeComponent();
            this.DataContext = this;
            var failingCredentials = new Credentials("forbidden", "", null, "");
            App.Login(failingCredentials);
            logonBrowser.ContextMenu = new ContextMenu();
            logonBrowser.Navigated += (sender, args) => {
                checkWhetherWebBrowserAuthenticationSucceeded(args.Uri);
            };
            logonBrowser.Navigate(ClientFactory.Connection().server.webAuthenticationEndpoint);

            SandRibbon.App.CloseSplashScreen();

            Commands.AddWindowEffect.ExecuteAsync(null);
#if DEBUG
            Version = String.Format("{0} Build: {1}", ConfigurationProvider.instance.getMetlVersion(), "not merc"); //SandRibbon.Properties.HgID.Version); 
#else
            Version = ConfigurationProvider.instance.getMetlVersion();
#endif
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<Credentials>(SetIdentity));
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
        protected Boolean checkUri(Uri uri)
        {
            var authenticationUri = ClientFactory.Connection().server.webAuthenticationEndpoint;
            return uri.Scheme == authenticationUri.Scheme && uri.AbsolutePath == authenticationUri.AbsolutePath && uri.Authority == authenticationUri.Authority;
        }
        protected void checkWhetherWebBrowserAuthenticationSucceeded(Uri uri){
            if (checkUri(uri))
            {
                try
                {
                    var document = logonBrowser.Document;
                    HTMLDocument doc = new mshtml.HTMLDocumentClass();
                    doc = (HTMLDocument)document;
                    if (!(doc.readyState == "complete" || doc.readyState == "interactive"))
                    {
                        var timer = new Timer((s) => {
                            Dispatcher.adoptAsync(() => { 
                                checkWhetherWebBrowserAuthenticationSucceeded(uri); 
                            });
                        }, null, 1000, Timeout.Infinite);
                    }
                    else
                    {
                        var authDataContainer = doc.getElementById("authData");
                        var html = authDataContainer.innerHTML;

                        var xml = XDocument.Parse(html).Elements().ToList();
                        var authData = getElementsByTag(xml,"authdata");
                        var authenticated = getElementsByTag(authData,"authenticated").First().Value.ToString().Trim().ToLower() == "true";
                        var usernameNode = getElementsByTag(authData,"username").First();
                        var authGroupsNodes = getElementsByTag(authData,"authGroup");
                        var infoGroupsNodes = getElementsByTag(authData, "infoGroup");
                        var username = usernameNode.Value.ToString();
                        var authGroups = authGroupsNodes.Select((xel) => new AuthorizedGroup(xel.Attribute("name").Value.ToString(), xel.Attribute("type").Value.ToString())).ToList();
                        var emailAddress = infoGroupsNodes.Find((xel) => xel.Attribute("type").Value.ToString().Trim().ToLower() == "emailaddress").Attribute("name").Value.ToString(); ;
                        var credentials = new Credentials(username, "", authGroups, emailAddress);
                        if (authenticated)
                        {
                            App.Login(credentials);
                        }
                    }
                }
                catch (Exception e)
                {

                }
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
