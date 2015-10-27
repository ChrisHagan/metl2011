using System;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using MeTLLib;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbon.Utils;
using MeTLLib.DataTypes;
using System.Collections.Generic;

[assembly: UIPermission(SecurityAction.RequestMinimum)]

namespace SandRibbon
{
    public class CouchTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            Logger.Log(message);
        }
        public override void WriteLine(string message)
        {
            Logger.Log(message);
        }
    }
    public partial class App : Application
    {
        public static Divelements.SandRibbon.RibbonAppearance colorScheme = 0;
        public static NetworkController controller;
        public static bool isStaging = false;
        public static bool isExternal = false;
        public static DateTime AccidentallyClosing = DateTime.Now;
        public static DiagnosticWindow diagnosticWindow = new DiagnosticWindow();
        public static MetlConfigurationManager metlConfigManager = new LocalAppMeTLConfigurationManager(); //change this to a remoteXml one when we're ready

#if DEBUG
        public static string OverrideUsername { get; private set; }
        public static string OverridePassword { get; private set; }
#endif

        private static SplashScreen splashScreen;
        public static void ShowSplashScreen()
        {
            Commands.DiagnosticMessage.Execute(new DiagnosticMessage("splash screen shown", "aesthetic", DateTime.Now));
            splashScreen = new SplashScreen("resources/splashScreen.png");
            splashScreen.Show(false);
        }
        public static void CloseSplashScreen()
        {
            Commands.DiagnosticMessage.Execute(new DiagnosticMessage("splash screen removed", "aesthetic", DateTime.Now));
            splashScreen.Close(TimeSpan.Zero);
        }

        public static void SetBackend(MetlConfiguration configuration)
        {
            Commands.DiagnosticMessage.Execute(new DiagnosticMessage("backend chosen: "+configuration.name, "connection", DateTime.Now));
            controller = new NetworkController(configuration);
            Commands.DiagnosticMessage.Execute(new DiagnosticMessage("network controller initiated: " + configuration.name, "connection", DateTime.Now));
//            App.mark(String.Format("Starting on backend mode {0}", configuration.name));//.ToString()));
        }
        public static List<MetlConfiguration> availableServers()
        {
            return metlConfigManager.Configs;
        }
        public static MetlConfiguration getCurrentServer
        {
            get {
                return controller.config;
            }
        }

        public static void Login(Credentials credentials)
        {
            try
            {
                Commands.DiagnosticMessage.Execute(new DiagnosticMessage("network controller connecting: " + controller.config.name, "connection", DateTime.Now));
                //App.mark("start network controller and log in");
                controller.connect(credentials);
                Commands.DiagnosticMessage.Execute(new DiagnosticMessage("network controller connected: " + controller.config.name, "connection", DateTime.Now));
                if (!controller.client.Connect(credentials))
                {
                    Commands.DiagnosticMessage.Execute(new DiagnosticMessage("credentials failed: " + controller.config.name, "connection", DateTime.Now));
                    Commands.LoginFailed.Execute(null);
                }
                else {
                    Commands.DiagnosticMessage.Execute(new DiagnosticMessage("identity set: " + controller.config.name, "connection", DateTime.Now));
                    Commands.SetIdentity.Execute(credentials);
                }
                //App.mark("finished logging in");
            }
            catch (TriedToStartMeTLWithNoInternetException)
            {
                Commands.LoginFailed.Execute(null);
                Commands.NoNetworkConnectionAvailable.Execute(null);
            }
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
            Trace.TraceInformation(s);
            return s;
        }
        public static void mark(string msg)
        {
            Console.WriteLine("{0} : {1}", msg, DateTime.Now - AccidentallyClosing);
        }
        static App()
        {
            App.mark("App static constructor runs");
            setDotNetPermissionState();
        }
        private static void setDotNetPermissionState()
        {
            var set = new PermissionSet(PermissionState.None);
            set.SetPermission(new UIPermission(UIPermissionWindow.AllWindows, UIPermissionClipboard.AllClipboard));
            //Asserting new permission set to all referenced assemblies
            set.Assert();
        }
        private void NoNetworkConnectionAvailable()
        {
            MeTLMessage.Error("MeTL cannot contact the server.  Please check your internet connection.");
        }
        private void LogOut(object showErrorMessage)
        {
            if (showErrorMessage != null && (bool)showErrorMessage)
            {
                MeTLMessage.Error("MeTL was unable to connect as your saved details were corrupted. Relaunch MeTL to try again.");
            }
            Trace.TraceInformation("LoggingOut");
            WorkspaceStateProvider.ClearSettings();
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
            Commands.LogOut.RegisterCommandToDispatcher(new DelegateCommand<object>(LogOut));
            Commands.NoNetworkConnectionAvailable.RegisterCommandToDispatcher(new DelegateCommand<object>((_unused) => { NoNetworkConnectionAvailable(); }));
            DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.Current.Exit += new ExitEventHandler(Current_Exit);
            mark("App.onStartup finished");
        }
        String[] falseAlarms = new[]{
                "Index was out of range. Must be non-negative and less than the size of the collection.",
                "The operation completed successfully",
                "Thread was being aborted."
            };
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            doCrash(ex);
            if (!falseAlarms.Any(m => ex.Message.StartsWith(m)))
            {
                MeTLMessage.Error("We're sorry.  MeTL has encountered an unexpected error and has to close.");
            }
        }
        void Current_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Commands.LeaveAllRooms.Execute(null);
                controller.client.Disconnect();         
            }
            catch (Exception) { }
        }
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var msg = e.Exception.Message;
            if (msg != null && falseAlarms.Any(m => msg.StartsWith(m)))
            {
                Logger.Fixed(msg);
                e.Handled = true;
            }
            else
                doCrash(e.Exception);
        }
        private void doCrash(Exception e)
        {
            Logger.Crash(e);
        }            

        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            if (e.Args.Length == 4)
            {
                if (e.Args[0] == "-user")
                {
                    OverrideUsername = e.Args[1];
                }
                if (e.Args[2] == "-pass")
                {
                    OverridePassword = e.Args[3];
                }
            }
#endif
        }
    }
}