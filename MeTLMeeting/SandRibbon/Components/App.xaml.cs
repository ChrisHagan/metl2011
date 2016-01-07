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

//[assembly: UIPermission(SecurityAction.RequestMinimum)]

namespace SandRibbon
{
    public partial class App : Application
    {
        public static ActorSystem actorSystem = ActorSystem.Create("MeTLActors");
        public static IActorRef diagnosticModelActor = actorSystem.ActorOf<DiagnosticsCollector>("diagnosticsCollector");
        public static DiagnosticWindow diagnosticWindow = null;
        public static DiagnosticModel diagnosticStore { get; set; }
        public static IAuditor auditor = new FuncAuditor((g) => {
            diagnosticModelActor.Tell(g);
        }, (m) => {
            diagnosticModelActor.Tell(m);
        }, (e) => {
            diagnosticModelActor.Tell(e);
        });
        public static string dumpFile = String.Format(@"{0}\MonashMeTL\MeTL-diagnostics-{1}.txt", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DateTime.Now.Ticks);

        public static Divelements.SandRibbon.RibbonAppearance colorScheme = 0;
        public static NetworkController controller;
        public static bool isStaging = false;
        public static bool isExternal = false;
        public static DateTime AccidentallyClosing = DateTime.Now;
        public static MetlConfigurationManager metlConfigManager = new RemoteAppMeTLConfigurationManager(auditor);

        private static SplashScreen splashScreen;
        public static void ShowSplashScreen()
        {
            splashScreen = new SplashScreen("resources/splashScreen.png");
            splashScreen.Show(false);
        }
        public static void CloseSplashScreen()
        {
            splashScreen.Close(TimeSpan.Zero);
        }

        public static void SetBackendProxy(MeTLConfigurationProxy server)
        {
            getCurrentServer = server;
        }
        public static void SetBackend(MetlConfiguration configuration)
        {
            controller = new NetworkController(configuration);
        }
        public static List<MeTLConfigurationProxy> availableServers()
        {
            return metlConfigManager.servers;
        }
        public static MeTLConfigurationProxy getCurrentServer
        {
            get;
            protected set;
        }
        public static MetlConfiguration getCurrentBackend
        {
            get {
                return controller.config;
            }
        }

        public static void Login(Credentials credentials)
        {
            try
            {
                controller.connect(credentials);
                if (!controller.client.Connect(credentials))
                {
                    Commands.LoginFailed.Execute(null);
                }
                else {
                    Commands.SetIdentity.Execute(credentials);
                }
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
        public static void mark(string msg)
        {
            App.auditor.log(String.Format("{0} : {1}", msg, DateTime.Now - AccidentallyClosing));
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
            set.Assert();
        }
        private void NoNetworkConnectionAvailable(object o)
        {
            Dispatcher.adopt(delegate
            {
                MeTLMessage.Error("MeTL cannot contact the server.  Please check your internet connection.");
            });
        }
        private void LogOut(object showErrorMessage)
        {
            Dispatcher.adopt(delegate
            {
                if (showErrorMessage != null && (bool)showErrorMessage)
                {
                    MeTLMessage.Error("MeTL was unable to connect as your saved details were corrupted. Relaunch MeTL to try again.");
                }
                auditor.log("LoggingOut","App");
                WorkspaceStateProvider.ClearSettings();
                Application.Current.Shutdown();
            });
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Commands.LogOut.RegisterCommand(new DelegateCommand<object>(LogOut));
            Commands.NoNetworkConnectionAvailable.RegisterCommand(new DelegateCommand<object>(NoNetworkConnectionAvailable));
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
            diagnosticStore.AddError(new ErrorMessage("CurrentDomain_UnhandledException", "App", DateTime.Now, ex));
            if (!falseAlarms.Any(m => ex.Message.StartsWith(m)))
            {
                MeTLMessage.Error("We're sorry.  MeTL has encountered an unexpected error and has to close.  Please find the exceptions at: "+dumpFile);
            }
            Environment.Exit(1);
        }
        void Current_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Commands.LeaveAllRooms.Execute(null);
                if (controller != null && controller.client != null)
                    controller.client.Disconnect();         
            }
            catch (Exception) { }
            if (App.diagnosticWindow != null)
            {
                diagnosticWindow.Dispatcher.adopt(delegate {
                    diagnosticWindow.Close();
                });
            }
        }
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var msg = e.Exception.Message;
            if (msg != null && falseAlarms.Any(m => msg.StartsWith(m)))
            {
                auditor.error("DispatcherUnhandledException - Handled", "App", e.Exception);
                e.Handled = true;
            }
            else
            {
                diagnosticStore.AddError(new ErrorMessage("DispatcherUnhandledException - Unhandled", "App", DateTime.Now,e.Exception));
            }
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
        }
    }
}