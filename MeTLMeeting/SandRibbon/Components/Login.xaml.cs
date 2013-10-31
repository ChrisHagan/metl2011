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
            credentialsGrid.DataContext = credentials;

            SandRibbon.App.CloseSplashScreen();

            RetrieveReleaseNotes();
            Commands.AddWindowEffect.ExecuteAsync(null);
#if DEBUG
            Version = String.Format("{0} Build: {1}", ConfigurationProvider.instance.getMetlVersion(), "not merc"); //SandRibbon.Properties.HgID.Version); 
#else
            Version = ConfigurationProvider.instance.getMetlVersion();
#endif
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
            var retriever = new BackgroundWorker();
            retriever.DoWork += (object sender, DoWorkEventArgs e) => 
            { 
                var bw = sender as BackgroundWorker;
                try
                {
                    e.Result = new WebClient().DownloadString( Properties.Settings.Default.ReleaseNotesUrl);
                    if (bw.CancellationPending)
                        e.Cancel = true;
                }
                catch(WebException)
                {

                }
            };

            retriever.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                if (e.Error == null)
                {
                    var notes = e.Result as string;
                    if (!string.IsNullOrEmpty(notes))
                    {
                        ReleaseNotes.Text = notes;
                        releaseNotesViewer.Visibility = Visibility.Visible;
                        return;
                    }
                }
                releaseNotesViewer.Visibility = Visibility.Collapsed;
            };

            retriever.RunWorkerAsync();
        }

        private void hideLoginErrors(object sender, RoutedEventArgs e)
        {
            loginErrors.Visibility = Visibility.Collapsed;
        }
        
        private void loaded(object sender, RoutedEventArgs e)
        {
            username.Focus();
#if DEBUG
            if (!string.IsNullOrEmpty(App.OverrideUsername) && !string.IsNullOrEmpty(App.OverridePassword))
            {
                username.Text = App.OverrideUsername;
                password.Password = App.OverridePassword;
                attemptAuthentication(null, null);
            }
#endif
        }
        private void checkAuthenticationAttemptIsPlausible(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = /*numErrorsOfCredentials == 0 &&*/ canLoginAgain;
        }
        private void attemptAuthentication(object sender, ExecutedRoutedEventArgs e)
        {
            canLoginAgain = false;
            loginErrors.Visibility = Visibility.Collapsed;

            var worker = new BackgroundWorker();
            var usernameText = username.Text.ToLower();
            var passwordText = password.Password;
            worker.DoWork += (_unused1, _unused2) => App.Login(usernameText, passwordText);
            worker.RunWorkerCompleted += LoginWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void LoginWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Error != null)
            {
                Trace.TraceInformation("Login Exception: " + args.Error.Message);
                Commands.LoginFailed.Execute(null);
            }
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
            CommandManager.InvalidateRequerySuggested();
            loginErrors.Visibility = Visibility.Visible;
            password.SelectAll();
            password.Focus();

            //On Login failed, we hide the loggingIn page and show the Login page
            loggingIn.Visibility = Visibility.Collapsed;
            usernameAndPassword.Visibility = Visibility.Visible;
            
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
            Commands.CloseApplication.Execute(null);
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
