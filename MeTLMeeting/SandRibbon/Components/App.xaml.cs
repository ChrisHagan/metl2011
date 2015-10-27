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
using SandRibbon.Pages.Conversations.Models;

[assembly: UIPermission(SecurityAction.RequestMinimum)]

namespace SandRibbon
{
    public class MetlContext
    {
        public MetlConfiguration config { get; protected set; }
        public MetlContext(MetlConfiguration _config)
        {
            config = _config;
            oneNoteConfiguration = new OneNoteConfiguration(config);
        }
        public void initController(Credentials creds)
        {
            controller = new NetworkController(config, creds);
            undoHistory = new UndoHistory(config);
        }
        public NetworkController controller { get; protected set; }
        public OneNoteConfiguration oneNoteConfiguration { get; protected set; }
        public readonly static MetlContext empty = new MetlContext(MetlConfiguration.empty);
        public UndoHistory undoHistory;

    }
    public partial class App : Application
    {
        public static Divelements.SandRibbon.RibbonAppearance colorScheme = 0;
        public static NetworkController controller;
        public static MetlConfigurationManager metlConfigManager = new LocalAppMeTLConfigurationManager(); //change this to a remoteXml one when we're ready
        public static DateTime AccidentallyClosing = DateTime.Now;

#if DEBUG
        public static string OverrideUsername { get; private set; }
        public static string OverridePassword { get; private set; }
#endif

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

        protected static Dictionary<MetlConfiguration, MetlContext> metlContexts = new Dictionary<MetlConfiguration, MetlContext>();

        public static MetlContext getContextFor(MetlConfiguration config)
        {
            if (config == null)
            {
                return MetlContext.empty;
            }
            MetlContext result = null;
            if (metlContexts.TryGetValue(config, out result))
            {
                return result;
            }
            else
            {
                var nc = new MetlContext(config);
                metlContexts.Add(config, nc);
                return nc;
            }
        }
        public static List<MetlConfiguration> getAvailableServers()
        {
            return metlConfigManager.Configs;
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
        static App()
        {
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
                MeTLMessage.Error("We're sorry.  MeTL has encountered an unexpected error and has to close.");
            }
        }
        void Current_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                metlContexts.Values.ToList().ForEach(mc =>
                {
                    if (mc.controller != null)
                    {
                        mc.controller.commands.LeaveAllRooms.Execute(null);
                        mc.controller.client.Disconnect();
                    }
                });
            }
            catch (Exception) { }
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