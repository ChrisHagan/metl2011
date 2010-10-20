using System;
using System.Collections.Specialized;
using System.Deployment.Application;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using SandRibbon.Components.Sandpit;
using SandRibbon.Utils;
using System.Security.Permissions;
using SandRibbon.Providers;
using SandRibbon.Utils.Connection;
using SandRibbon.Quizzing;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using System.Security;


[assembly:UIPermission(SecurityAction.RequestMinimum)]
//[assembly:UIPermission(SecurityAction.Assert)]

namespace SandRibbon
{
    public partial class App : Application
    {
        private bool loggingOut = false;
        public static NetworkController controller;

        public static bool isStaging = false;

        public static void Login(String username, String password)
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
            controller = new NetworkController();
            MeTLLib.ClientFactory.Connection().Connect(username, password);
        }

        public static void LookupServer()
        {
            if (Constants.JabberWire.SERVER == null)
                try
                {
                    Constants.JabberWire.SERVER = ConfigurationProvider.instance.SERVER;
                }
                catch (Exception e)
                {
                    MessageBox.Show("MeTL cannot find the server and so cannot start.  Please check your internet connection and try again.");
                    if (Application.Current != null)
                        Application.Current.Shutdown();
                }
                finally
                {
                    Logger.Log(string.Format("Logged into MeTL server {0}", Constants.JabberWire.SERVER));
                }
        }

        public static void dontDoAnything()
        {
        }

        public static void dontDoAnything(int _obj, int _obj2)
        {
        }

        public static string Now(string title){
            var now = SandRibbonObjects.DateTimeFactory.Now();
            var s = string.Format("{2} {0}:{1}", now, now.Millisecond, title);
            Logger.Log(s);
            Console.WriteLine(s);
            return s;
        }
        static App() {
            Now("Static App start");
            setDotNetPermissionState();
        }
        private static void setDotNetPermissionState()
        {
            Now("Creating permissionState to allow all actions");
            PermissionSet set = new PermissionSet(PermissionState.Unrestricted);
            Now("Asserting new permission set to all referenced assemblies");
            set.Assert();
        }
        
        private void LogOut(object _Unused)
        {
            //loggingOut = true;
            WorkspaceStateProvider.ClearSettings();
            ThumbnailProvider.ClearThumbnails();
            Application.Current.Shutdown();
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            isStaging = true;
#else
            isStaging = false;
#endif
            base.OnStartup(e);
            //controller = new NetworkController();
            new Worm();
            new Printer();
            new CommandParameterProvider();
            Commands.LogOut.RegisterCommand(new DelegateCommand<object>(LogOut));
            DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Log(e.Exception.Message);
            MessageBox.Show(string.Format("MeTL has encountered an unexpected error and has to close:{0}\n{1} ",
                e.Exception.Message,
                e.Exception.InnerException == null? 
                    "No inner exception": e.Exception.InnerException.Message));
            this.Shutdown();
        }
        private void AncilliaryButton_Click(object sender, RoutedEventArgs e)
        {
            var AncilliaryButton = (Button) sender;
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
                if(queryString != null)
                    nameValueTable = HttpUtility.ParseQueryString(queryString);
            }
            return (nameValueTable);
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                var parameters = GetQueryStringParameters();
                foreach (var key in parameters.Keys)
                    Application.Current.Properties.Add(key, parameters.Get((string)key));
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }
    }
}