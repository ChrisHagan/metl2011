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
using System.Collections.ObjectModel;
using System.IO;
using Akka;
using System.Threading.Tasks;
using Akka.Actor;
using System.Windows.Controls;

//[assembly: UIPermission(SecurityAction.RequestMinimum)]

namespace SandRibbon
{
    public partial class App : Application
    {


        public static ActorSystem actorSystem = ActorSystem.Create("MeTLActors");
        public static IActorRef diagnosticModelActor = actorSystem.ActorOf<DiagnosticsCollector>("diagnosticsCollector");
        public static DiagnosticWindow diagnosticWindow = null;
        public static IAuditor auditor = new FuncAuditor((g) =>
        {
            diagnosticModelActor.Tell(g);
        }, (m) =>
        {
            diagnosticModelActor.Tell(m);
        });

        public static DateTime AccidentallyClosing = DateTime.Now;
        public static MetlConfigurationManager metlConfigManager = new RemoteAppMeTLConfigurationManager();

        private static SplashScreen splashScreen;
        public static void ShowSplashScreen()
        {
            splashScreen = new SplashScreen("resources/logo-metl-splash.png");
            splashScreen.Show(false);
        }
        public static void CloseSplashScreen()
        {
            splashScreen.Close(TimeSpan.Zero);
        }

        public static List<MeTLConfigurationProxy> availableServers()
        {
            if (metlConfigManager.servers.Count == 0)
            {
                metlConfigManager.reload();
            }
            return metlConfigManager.servers;
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
        public static readonly StringWriter outputWriter = new StringWriter();

        public static Process proc;
        static App()
        {
            proc = Process.GetCurrentProcess();
            Console.SetOut(outputWriter);
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
            //WorkspaceStateProvider.ClearSettings();
            Application.Current.Shutdown();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            //MeTLConfiguration.Load();            
            base.OnStartup(e);
            Commands.LogOut.RegisterCommandToDispatcher(new DelegateCommand<object>(LogOut));
            Commands.NoNetworkConnectionAvailable.RegisterCommandToDispatcher(new DelegateCommand<object>((_unused) => { NoNetworkConnectionAvailable(); }));
            DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.Current.Exit += new ExitEventHandler(Current_Exit);
            new MainWindow().Show();
        }
        String[] falseAlarms = new[]{
                "Index was out of range. Must be non-negative and less than the size of the collection.",
                "The operation completed successfully",
                "Thread was being aborted."
            };
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            if (!falseAlarms.Any(m => ex.Message.StartsWith(m)))
            {
                var detail = string.Format("{0}\n{1}", ex.Message, ex.StackTrace);
                EventLog.WriteEntry("MeTL", detail);
                MeTLMessage.Error(string.Format("MeTL has encountered an unrecoverable error:\n{0}",detail));
            }
        }
        void Current_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Commands.LeaveAllRooms.Execute(null);
            }
            catch (Exception) { }
            if (App.diagnosticWindow != null)
            {
                diagnosticWindow.Dispatcher.adopt(delegate
                {
                    diagnosticWindow.Close();
                });
            }
        }
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var msg = e.Exception.Message;
            if (msg != null && falseAlarms.Any(m => msg.StartsWith(m)))
            {
                Commands.Mark.Execute(string.Format("Unhandled exception: {0}", msg));
                e.Handled = true;
            }
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
        }
    }
}