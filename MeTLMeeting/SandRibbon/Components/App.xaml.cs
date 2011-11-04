using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MeTLLib;
using SandRibbon.Utils;
using System.Security.Permissions;
using SandRibbon.Providers;
using SandRibbon.Quizzing;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using System.Security;
using System.Diagnostics;
using System.Windows.Threading;
using SandRibbon.Components.Canvas;
using System.Threading;

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
            if (username.ToLower().StartsWith("ext-"))
                isExternal = true;
            try
            {
                controller = new NetworkController();
                MeTLLib.ClientFactory.Connection().Connect(finalUsername, password);
            }
            catch (TriedToStartMeTLWithNoInternetException e)
            {
                MessageBox.Show("MeTL cannot contact the server.  Please check your internet connection.");
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
        public static void mark(string msg) {
            //Console.WriteLine("{0} : {1}", msg, DateTime.Now - AccidentallyClosing);
            //MessageBox.Show(String.Format("{0} : {1}", msg, DateTime.Now - AccidentallyClosing));
        }
        static App()
        {
            //App.mark("App static constructor runs");
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
            Trace.TraceInformation("LoggingOut");
            WorkspaceStateProvider.ClearSettings();
            Application.Current.Shutdown();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            MessageBox.Show("You are operating against the staging server.");
            isStaging = false;
#else
            isStaging = false;
#endif
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, (DispatcherOperationCallback)delegate { CloseSplashScreen(); return null; }, this);
            Trace.Listeners.Add(new CouchTraceListener());
            base.OnStartup(e);
            Commands.LogOut.RegisterCommandToDispatcher(new DelegateCommand<object>(LogOut));
            DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.Current.Exit += new ExitEventHandler(Current_Exit);
           // mark("App.onStartup finished");
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            doCrash((Exception)e.ExceptionObject);
            MessageBox.Show(string.Format("We're sorry.  MeTL has encountered an unexpected error and has to close."));
        }
        void Current_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Commands.LeaveAllRooms.Execute(null);
                MeTLLib.ClientFactory.Connection().Disconnect();
                /* Awesomium comment out
                AbstractCanvas.deleteTempFiles();
                // End Awesomium comment out
                */
            }
            catch (Exception) { }
        }
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var falseAlarms = new[]{
                "Index was out of range. Must be non-negative and less than the size of the collection.",
                "The operation completed successfully"
            };
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
        private void CloseSplashScreen()
        {
            // signal process to close splash screen
            using (var closeSplashEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "CloseSplashScreenWithoutFadeEventSplashScreenStarter"))
            {
                closeSplashEvent.Set();
            }
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
        }
    }
}