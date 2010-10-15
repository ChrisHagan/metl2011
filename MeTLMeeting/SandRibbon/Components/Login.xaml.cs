using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows.Navigation;
using MeTLLib.DataTypes;

namespace SandRibbon.Components
{
    public partial class Login : UserControl
    {
        public static RoutedCommand CheckAuthentication = new RoutedCommand();
        static Random random = new Random();
        public string Version { get; set; }
        public string ReleaseNotes
        {
            get
            {
                var releaseNotes = HttpResourceProvider.insecureGetString("http://metl.adm.monash.edu.au/MeTL/MeTLPresenterReleaseNotes.txt");
                if (!string.IsNullOrEmpty(releaseNotes))
                    releaseNotesViewer.Visibility = Visibility.Visible;
                else
                    releaseNotesViewer.Visibility = Visibility.Collapsed;
                return releaseNotes;
            }
        }
        public Login()
        {
            InitializeComponent();
            this.DataContext = this;
            Commands.AddWindowEffect.Execute(null);
            Version = ConfigurationProvider.instance.getMetlVersion();
            Logger.Log(string.Format("The Version of MeTL is -> {0}", Version));
            Commands.ServersDown.RegisterCommand(new DelegateCommand<IEnumerable<ServerStatus>>(ServersDown));
            Commands.ConnectWithUnauthenticatedCredentials.RegisterCommand(new DelegateCommand<Utils.Connection.JabberWire.Credentials>(ConnectWithUnauthenticatedCredentials));
            if (WorkspaceStateProvider.savedStateExists())
            {
                rememberMe.IsChecked = true;
                loggingIn.Visibility = Visibility.Visible;
                usernameAndPassword.Visibility = Visibility.Collapsed;
            }
            Loaded += loaded;
        }
        private void loaded(object sender, RoutedEventArgs e)
        {
            username.Focus();
        }
        private void ServersDown(IEnumerable<ServerStatus> servers)
        {
            Dispatcher.adopt((Action)delegate
            {
                this.servers.ItemsSource = servers;
            });
        }
        public bool isAuthenticatedAgainstLDAP(string username, string password)
        {
            if (username.StartsWith(BackDoor.USERNAME_PREFIX)) return true;
            string LDAPServerURL = @"LDAP://directory.monash.edu.au:389/";
            string LDAPBaseOU = "o=Monash University,c=AU";
            try
            {
                DirectoryEntry LDAPAuthEntry = new DirectoryEntry(LDAPServerURL + LDAPBaseOU, "", "", AuthenticationTypes.Anonymous);
                DirectorySearcher LDAPDirectorySearch = new DirectorySearcher(LDAPAuthEntry);
                LDAPDirectorySearch.ClientTimeout = new System.TimeSpan(0, 0, 5);
                LDAPDirectorySearch.Filter = "uid=" + username;
                SearchResult LDAPSearchResponse = LDAPDirectorySearch.FindOne();

                string NewSearchPath = LDAPSearchResponse.Path.ToString();
                string NewUserName = NewSearchPath.Substring(NewSearchPath.LastIndexOf("/") + 1);

                DirectoryEntry AuthedLDAPAuthEntry = new DirectoryEntry(NewSearchPath, NewUserName, password, AuthenticationTypes.None);
                DirectorySearcher AuthedLDAPDirectorySearch = new DirectorySearcher(AuthedLDAPAuthEntry);
                AuthedLDAPDirectorySearch.ClientTimeout = new System.TimeSpan(0, 0, 5);
                AuthedLDAPDirectorySearch.Filter = "";
                SearchResultCollection AuthedLDAPSearchResponse = AuthedLDAPDirectorySearch.FindAll();
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("Failed authentication against LDAP because {0}", e.Message));
                return false;
            }
            return true;
        }
        public bool isAuthenticatedAgainstWebProxy(string username, string password)
        {
            try
            {
                var resource = String.Format("https://my.monash.edu.au/login?username={0}&password={1}", username, password);
                String test = HttpResourceProvider.insecureGetString(resource);
                return !test.Contains("error-text");
            }
            catch (Exception e)
            {
                MessageBox.Show("Web proxy auth error:" + e.Message);
                return false;
            }
        }
        private void checkAuthenticationAttemptIsPlausible(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = username != null && username.Text.Length > 0 && password != null && password.Password.Length > 0;
        }
        private void ConnectWithUnauthenticatedCredentials(Utils.Connection.JabberWire.Credentials credentials) {
            doAttemptAuthentication(credentials.name, credentials.password);
        }
        private void attemptAuthentication(object sender, ExecutedRoutedEventArgs e)
        {
            doAttemptAuthentication(username.Text,password.Password);
        }
        private void doAttemptAuthentication(string username, string password){
            var TempUsername = username;
            var AuthcateUsername = username;
#if DEBUG
            JabberWire.SwitchServer("staging");
#else
            ConfigurationProvider.instance.isStaging = false;
#endif
            if (TempUsername.Contains("_"))
            {
                var Parameters = TempUsername.Split('_');
                if (Parameters.Contains<string>("prod"))
                {
                    JabberWire.SwitchServer("prod");
                }
                if (Parameters.Contains<string>("staging"))
                {
                    JabberWire.SwitchServer("staging");
                }
                AuthcateUsername = TempUsername.Remove(TempUsername.IndexOf("_"));
            }
            else
                AuthcateUsername = TempUsername;

            if (authenticateAgainstFailoverSystem(AuthcateUsername, password) || isBackdoorUser(AuthcateUsername))
            {
                var eligibleGroups = new AuthorisationProvider().getEligibleGroups(AuthcateUsername, password);
                Commands.ConnectWithAuthenticatedCredentials.Execute(new SandRibbon.Utils.Connection.JabberWire.Credentials
                {
                    name = AuthcateUsername,
                    password = password,
                    authorizedGroups = eligibleGroups
                });
                if(rememberMe.IsChecked == true)
                    WorkspaceStateProvider.SaveCurrentSettings();
                else
                    WorkspaceStateProvider.ClearSettings();
                Commands.RemoveWindowEffect.Execute(null);
                Commands.ShowConversationSearchBox.Execute(null);
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Failed to Login.  Please check your details and try again.");
                this.password.Clear();
                this.password.Focus();
            }
        }
        private bool isBackdoorUser(string user)
        {
            return user.Contains(BackDoor.USERNAME_PREFIX);
        }
        private bool authenticateAgainstFailoverSystem(string username, string password)
        {
            if (isAuthenticatedAgainstLDAP(username, password))
                return true;
            else if (isAuthenticatedAgainstWebProxy(username, password))
                return true;
            else
                return false;
        }
        private void checkDestinationAlreadyKnown()
        {
            if (Application.Current.Properties.Contains("destination"))
            {
                Commands.LoggedIn.RegisterCommand(new DelegateCommand<string>((username) =>
                {//Technically this will mean that every time we login we will go here, this app session.
                    Dispatcher.adoptAsync((Action)delegate
                    {
                        var destination = Application.Current.Properties["destination"].ToString();
                        var iDestination = Int32.Parse(destination);
                        var destinationConversation = iDestination - ((iDestination % 1000) % 400);
                        Commands.JoinConversation.Execute(destinationConversation.ToString());
                        DelegateCommand<PreParser> joinSlide = null;
                        joinSlide = new DelegateCommand<PreParser>((_parser) =>
                        {
                            Commands.MoveTo.Execute(iDestination);
                            Commands.PreParserAvailable.UnregisterCommand(joinSlide);
                        });
                        Commands.PreParserAvailable.RegisterCommand(joinSlide);
                    });
                }));
            }
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
            Constants.JabberWire.SERVER = value;
        }
        public string Value
        {
            get { return Constants.JabberWire.SERVER; }
        }
    }
}