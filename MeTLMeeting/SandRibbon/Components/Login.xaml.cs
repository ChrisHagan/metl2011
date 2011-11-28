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
                string result = null; // doesn't make sense, should be initialised to an empty string but the compiler complains
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
        public string Version { get; set; }
        private UserCredentials credentials = new UserCredentials();
        private int numErrorsOfCredentials = 0;
        public Login()
        {
            InitializeComponent();
            this.DataContext = this;
            credentialsGrid.DataContext = credentials;

            SandRibbon.App.CloseSplashScreen();

            RetrieveReleaseNotes();
            Commands.AddWindowEffect.ExecuteAsync(null);
            Version = ConfigurationProvider.instance.getMetlVersion();
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<Credentials>(SetIdentity));
            Commands.LoginFailed.RegisterCommandToDispatcher(new DelegateCommand<object>((_unused) => { LoginFailed(); }));
            if (WorkspaceStateProvider.savedStateExists())
            {
                rememberMe.IsChecked = true;
                loggingIn.Visibility = Visibility.Visible;
                usernameAndPassword.Visibility = Visibility.Collapsed;
            }
            Loaded += loaded;
        }
        
        private void ValidationErrorHandler(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                numErrorsOfCredentials++;
            else
                numErrorsOfCredentials--;
        }

        private void RetrieveReleaseNotes()
        {
            ThreadPool.QueueUserWorkItem(_arg =>
            {
                var notes = new WebClient().DownloadString("http://metl.adm.monash.edu.au/MeTL/MeTLPresenterReleaseNotes.txt");
                Dispatcher.adoptAsync(delegate
                {
                    if (!string.IsNullOrEmpty(notes))
                    {
                        ReleaseNotes.Text = notes;
                        releaseNotesViewer.Visibility = Visibility.Visible;
                    }
                    else
                        releaseNotesViewer.Visibility = Visibility.Collapsed;
                });
            });
        }
        private void loaded(object sender, RoutedEventArgs e)
        {
            username.Focus();
        }
        private void checkAuthenticationAttemptIsPlausible(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = /*numErrorsOfCredentials == 0 &&*/ canLoginAgain;
        }
        private void attemptAuthentication(object sender, ExecutedRoutedEventArgs e)
        {
            canLoginAgain = false;
            loginErrors.Visibility = Visibility.Collapsed;
            App.Login(username.Text.ToLower(), password.Password);
        }
        private void SetIdentity(Credentials identity)
        {
            //Commands.ShowConversationSearchBox.Execute(null);
            Commands.RemoveWindowEffect.ExecuteAsync(null);
            Dispatcher.adoptAsync(() =>
            {
                if (rememberMe.IsChecked == true)
                {
                    Commands.RememberMe.Execute(true);
                    WorkspaceStateProvider.SaveCurrentSettings();
                }
                var options = ClientFactory.Connection().UserOptionsFor(identity.name);
                Commands.SetUserOptions.Execute(options);
                Commands.SetPedagogyLevel.Execute(Pedagogicometer.level((Pedagogicometry.PedagogyCode)options.pedagogyLevel));
                this.Visibility = Visibility.Collapsed;
            });
            App.mark("Login knows identity");
            Commands.ShowConversationSearchBox.ExecuteAsync(null);
        }
        private void LoginFailed()
        {
            canLoginAgain = true;
            loginErrors.Visibility = Visibility.Visible;
            username.Text = string.Empty;
            password.Password = string.Empty;

            username.Focus();
        }
        private void checkLoginPending(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = canLoginAgain;
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
