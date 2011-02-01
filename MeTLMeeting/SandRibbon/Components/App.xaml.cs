using System;
using System.Collections.Specialized;
using System.Deployment.Application;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using MeTLLib;
using Npgsql;
using SandRibbon.Components.Sandpit;
using SandRibbon.Utils;
using System.Security.Permissions;
using SandRibbon.Providers;
using SandRibbon.Utils.Connection;
using SandRibbon.Quizzing;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using System.Security;
using System.Diagnostics;
using MeTLLib.DataTypes;
using System.Windows.Threading;

[assembly: UIPermission(SecurityAction.RequestMinimum)]

namespace SandRibbon
{
    public partial class App : Application
    {
        public static NetworkController controller;
        public static bool isStaging = false;
        public static DateTime AccidentallyClosing = DateTime.Now;
        public static Credentials Login(String username, String password)
        {
            string finalUsername = username;
            if (username.Contains("_"))
            {
                var parts = username.Split('_');
                finalUsername = parts[0];
                parts[0] = "";
                foreach (String part in parts)
                {
                    switch (part)
                    {
                        case "prod":
                            isStaging = false;
                            break;
                        case "production":
                            isStaging = false;
                            break;
                        case "staging":
                            isStaging = true;
                            break;
                    }
                }
            }
            if (controller == null)
                controller = new NetworkController();
            else
                controller.switchServer();
            return MeTLLib.ClientFactory.Connection().Connect(finalUsername, password);
        }
        public static void noop(object _arg)
        {
        }
        public static void noop(params object[] args)
        {
        }
        public static string Now(string message)
        {
            var now = SandRibbonObjects.DateTimeFactory.Now();
            var s = string.Format("{2} {0}:{1}", now, now.Millisecond, message);
            //saveInformationToDatabase(message);
            Logger.Log(message);
            Trace.TraceInformation(s);
            return s;
        }
        private static bool failedLogging = false;
        private static NpgsqlConnection conn = null;
        private static void saveInformationToDatabase(string message)
        {
            if (failedLogging) return;
            if (conn == null)
            {
                conn = createDatabaseConnection();
                conn.Open();
            }
            try
            {
                if (Globals.me.Length == 0) return;
                var insertionString = string.Format("INSERT INTO metl_log (username, message, server) VALUES('{0}', '{1}', '{2}');COMMIT",
                                                    Globals.me, message, ClientFactory.Connection().server.host);
                var insertCommand = conn.CreateCommand();
                insertCommand.CommandText = insertionString;
                insertCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                failedLogging = true;
            }
        }
        private static string server = "madam.adm.monash.edu";
        private static string port = "5432";
        private static string databaseName = "postgres";
        private static string databaseUsername = "ejabberd";
        private static string databasePassword = "";
        private static NpgsqlConnection createDatabaseConnection()
        {
            var connectionString = string.Format("Server={0};Port={1};User Id={2};Database={4}", server, port, databaseUsername, databasePassword, databaseName);
            var conn = new NpgsqlConnection(connectionString);
            return conn;
        }

        public static string Now(string format, params object[] args) { 
            return Now(String.Format(format,args));
        }
        static App()
        {
            Now("Static App start");
            setDotNetPermissionState();
        }
        private static void setDotNetPermissionState()
        {
            var set = new PermissionSet(PermissionState.None);
            set.SetPermission(new UIPermission(UIPermissionWindow.AllWindows, UIPermissionClipboard.AllClipboard));
            //Asserting new permission set to all referenced assemblies
            set.Assert();
        }
        private void LogOut(object _Unused)
        {
            WorkspaceStateProvider.ClearSettings();
            Application.Current.Shutdown();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            MessageBox.Show("You are operating against the staging server.");
            isStaging = true;
#else
            isStaging = false;
#endif
            base.OnStartup(e);
            new Worm();
            new CommandParameterProvider();
            Commands.LogOut.RegisterCommandToDispatcher(new DelegateCommand<object>(LogOut));
            DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            Application.Current.Exit += new ExitEventHandler(Current_Exit);
        }
        void Current_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Commands.LeaveAllRooms.Execute(null);
                MeTLLib.ClientFactory.Connection().Disconnect();
            }
            catch (Exception) { }
        }
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e){
            Logger.Crash(e.Exception);
            Commands.LeaveAllRooms.Execute(null);
            MessageBox.Show(string.Format("MeTL has encountered an unexpected error and has to close:{0}\n{1} ",
                e.Exception.Message,
                e.Exception.InnerException == null ?
                    "No inner exception" : e.Exception.InnerException.Message));
            this.Shutdown();
        }
        private void AncilliaryButton_Click(object sender, RoutedEventArgs e)
        {
            var AncilliaryButton = (Button)sender;
            var CurrentGrid = (StackPanel)AncilliaryButton.Parent;
            var CurrentPopup = new System.Windows.Controls.Primitives.Popup();
            foreach (FrameworkElement f in CurrentGrid.Children)
                if (f.GetType().ToString() == "System.Windows.Controls.Primitives.Popup")
                    CurrentPopup = (System.Windows.Controls.Primitives.Popup)f;
            if (CurrentPopup.IsOpen == false)
                CurrentPopup.IsOpen = true;
            else CurrentPopup.IsOpen = false;
        }
        private NameValueCollection GetQueryStringParameters()
        {
            NameValueCollection nameValueTable = new NameValueCollection();
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                string queryString = ApplicationDeployment.CurrentDeployment.ActivationUri.Query;
                if (queryString != null)
                    nameValueTable = HttpUtility.ParseQueryString(queryString);
            }
            return (nameValueTable);
        }
        private void AnyTextBoxGetsFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                var source = (TextBox)sender;
                source.CaretIndex = source.Text.Length;
                source.SelectAll();
            }, DispatcherPriority.Background);
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            EventManager.RegisterClassHandler(typeof(TextBox),
            TextBox.GotKeyboardFocusEvent,
            new RoutedEventHandler(AnyTextBoxGetsFocus));
            try
            {
                var parameters = GetQueryStringParameters();
                foreach (var key in parameters.Keys)
                {
                    App.Now("Added uri query parameter(" + key + "): " + parameters.Get((String)key));
                    Application.Current.Properties.Add(key, parameters.Get((string)key));
                }
                /*int cmdLineArg = 0;
                foreach (string arg in Environment.GetCommandLineArgs())
                {
                    Application.Current.Properties.Add("commandLineArgument" + cmdLineArg++, arg);
                    App.Now("Added commandline argument(" + cmdLineArg + "): " + arg);
                }*/
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }
        }
    }
}
