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
                var releaseNotes = "MeTL is unable to retrieve announcements.  Please check your internet connection.";
                try
                {
                    releaseNotes = HttpResourceProvider.insecureGetString("http://metl.adm.monash.edu.au/MeTL/MeTLPresenterReleaseNotes.txt");
                }
                catch (Exception e)
                {
                }
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
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<Credentials>(SetIdentity));
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
        private void checkAuthenticationAttemptIsPlausible(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = username != null && username.Text.Length > 0 && password != null && password.Password.Length > 0;
        }
        private void attemptAuthentication(object sender, ExecutedRoutedEventArgs e)
        {
            App.Login(username.Text, password.Password);
        }
        private void SetIdentity(Credentials identity)
        {
            Commands.RemoveWindowEffect.ExecuteAsync(null);
            Commands.ShowConversationSearchBox.ExecuteAsync(null);
            Dispatcher.adoptAsync(() =>
            {
                if (rememberMe.IsChecked == true)
                {
                    Commands.RememberMe.Execute(true);
                    WorkspaceStateProvider.SaveCurrentSettings();
                }
                var options = ClientFactory.Connection().UserOptionsFor(identity.name);
                Commands.SetUserOptions.Execute(options);
                Commands.SetPedagogyLevel.Execute(Pedagogicometer.level(options.pedagogyLevel));
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

        private void clearAndClose(object sender, RoutedEventArgs e)
        {
            WorkspaceStateProvider.ClearSettings();
            Commands.CloseApplication.Execute(null, this);
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
