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
using MeTLLib;
using SandRibbon.Components.Sandpit;

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
            Commands.AddWindowEffect.ExecuteAsync(null);
            Version = ConfigurationProvider.instance.getMetlVersion();
            App.Now(string.Format("The Version of MeTL is -> {0}", Version));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<object>(SetIdentity));
            Commands.ServersDown.RegisterCommand(new DelegateCommand<IEnumerable<ServerStatus>>(ServersDown));
            //Commands.ConnectWithUnauthenticatedCredentials.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.Credentials>(ConnectWithUnauthenticatedCredentials));
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
        private void checkAuthenticationAttemptIsPlausible(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = username != null && username.Text.Length > 0 && password != null && password.Password.Length > 0;
        }
        private void ConnectWithUnauthenticatedCredentials(MeTLLib.DataTypes.Credentials credentials)
        {
            doAttemptAuthentication(credentials.name, credentials.password);
        }
        private void attemptAuthentication(object sender, ExecutedRoutedEventArgs e)
        {
            doAttemptAuthentication(username.Text, password.Password);
        }
        private void doAttemptAuthentication(string username, string password)
        {
            var connection = ClientFactory.Connection();
            connection.Connect(username, password);
        }
        private void SetIdentity(object _args)
        {
            Commands.RemoveWindowEffect.ExecuteAsync(null);
            Commands.ShowConversationSearchBox.ExecuteAsync(null);
            Dispatcher.adoptAsync(() =>
                {
                    Pedagogicometer.SetPedagogyLevel(Globals.pedagogy);
                    this.Visibility = Visibility.Collapsed;
                });
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