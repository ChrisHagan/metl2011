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
        protected HTMLDocument doc;
        public Login()
        {
            InitializeComponent();
            this.DataContext = this;
            var failingCredentials = new Credentials("forbidden", "", null, "");
            App.Login(failingCredentials);
            logonBrowser.Navigating += (sender, args) =>
            {
                hideBrowser();
            };
            logonBrowser.LoadCompleted += (sender, args) => {
                doc = (HTMLDocument)logonBrowser.Document;
                var authResult = checkWhetherWebBrowserAuthenticationSucceeded();
                if (authResult)
                {
                    hideBrowser();
                }
                else
                {
                    showBrowser();
                }
                //attachDocumentHandlers(doc);
            };
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
        protected void hideBrowser()
        {
            logonBrowser.Visibility = Visibility.Collapsed;
            loadingImage.Visibility = Visibility.Visible;
            logonBrowser.IsHitTestVisible = true;
        }
        protected void showBrowser()
        {
            logonBrowser.Visibility = Visibility.Visible;
            loadingImage.Visibility = Visibility.Collapsed;
            logonBrowser.IsHitTestVisible = true;
        }
        protected void ResetWebBrowser(object _unused)
        {
            hideBrowser();
            logonBrowser.Navigate(ClientFactory.Connection().server.webAuthenticationEndpoint);
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
                var authenticationUri = ClientFactory.Connection().server.webAuthenticationEndpoint;
                return uri.Scheme == authenticationUri.Scheme && uri.AbsolutePath == authenticationUri.AbsolutePath && uri.Authority == authenticationUri.Authority;
            }
            catch (Exception e)
            {
              System.Console.WriteLine("exception in checking uri: " + e.Message);
                return false;
            }
        }
        /*
        protected void attachDocumentHandlers(HTMLDocument doc)
        {
            if (doc != null)
            {
                Console.WriteLine("attaching");
                HTMLDocumentEvents2_Event docEvent = (HTMLDocumentEvents2_Event)doc;
                docEvent.onreadystatechange += docEvent_onreadystatechange;
                docEvent.oncontextmenu += docEvent_oncontextmenu;
                docEvent.onmouseover += docEvent_onmouseover;
                docEvent.onmousedown += docEvent_onmousedown;
                docEvent.onmouseup += docEvent_onmouseup;
                docEvent.onclick += docEvent_onclick;
                doc.focus();
            }
        }
        protected void detachDocumentHandlers(HTMLDocument doc)
        { 
            if (doc != null)
            {
                Console.WriteLine("detaching");
                HTMLDocumentEvents2_Event docEvent = (HTMLDocumentEvents2_Event)doc;
                docEvent.onreadystatechange -= docEvent_onreadystatechange;
                docEvent.oncontextmenu -= docEvent_oncontextmenu;
                docEvent.onmouseover -= docEvent_onmouseover;
                docEvent.onmousedown -= docEvent_onmousedown;
                docEvent.onmouseup -= docEvent_onmouseup;
                docEvent.onclick -= docEvent_onclick;
            }
        }

        protected void docEvent_onmouseover(mshtml.IHTMLEventObj e)
        {
            //Console.WriteLine(String.Format("mouseover: {0}",e.fromElement.toString()));
        }
        protected bool docEvent_oncontextmenu(mshtml.IHTMLEventObj e)
        {
            e.cancelBubble = true;
            e.returnValue = false;
            return false;
        }
        protected bool docEvent_onclick(mshtml.IHTMLEventObj e)
        {
            Console.WriteLine("click");
            e.cancelBubble = false;
            e.returnValue = true;
            if (e.srcElement != null)
            {
                e.srcElement.click();
            }
            return true;
        }
        protected void docEvent_onmousedown(mshtml.IHTMLEventObj e)
        {
            Console.WriteLine("mouse down");
            e.cancelBubble = false;
            e.returnValue = true;
        }
        protected void docEvent_onmouseup(mshtml.IHTMLEventObj e)
        {
            Console.WriteLine("mouse up");
            e.cancelBubble = false;
            e.returnValue = true;
        }
        protected void docEvent_onreadystatechange(mshtml.IHTMLEventObj e)
        {
            Console.WriteLine("onreadystatechanged!");
            checkWhetherWebBrowserAuthenticationSucceeded();
            e.cancelBubble = false;
            e.returnValue = true;
        }
         */
        protected bool checkWhetherWebBrowserAuthenticationSucceeded(){
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
                        var emailAddress = infoGroupsNodes.Find((xel) => xel.Attribute("type").Value.ToString().Trim().ToLower() == "emailaddress").Attribute("name").Value.ToString(); ;
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
            //detachDocumentHandlers(doc);
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
