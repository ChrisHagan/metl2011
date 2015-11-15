using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Divelements.SandRibbon;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Win32;
using SandRibbon.Components;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using MeTLLib.DataTypes;
using System.Diagnostics;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Tabs.Groups;
using SandRibbon.Components.Utility;
using System.Windows.Documents;
using MeTLLib;
using MeTLLib.Providers.Connection;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using SandRibbon.Components.Sandpit;
using System.Threading;
using SandRibbon.Properties;
using SandRibbon.Components.Pages;

namespace SandRibbon
{
    public partial class MainWindow
    {
        private System.Windows.Threading.DispatcherTimer displayDispatcherTimer;

        public readonly string RECENT_DOCUMENTS = "recentDocuments.xml";
        #region SurroundingServers
        #endregion
        private PowerPointLoader loader;
        private UndoHistory undoHistory;
        public string CurrentProgress { get; set; }
        public static RoutedCommand ProxyMirrorExtendedDesktop = new RoutedCommand();
        public string log
        {
            get { return Logger.log; }
        }
        public MainWindow()
        {
            DoConstructor();
            Commands.AllStaticCommandsAreRegistered();
            App.mark("Window1 constructor complete");
            mainFrame.Navigate(new LoginPage());
        }
        private void DoConstructor()
        {
            InitializeComponent();            
            Commands.MeTLType.DefaultValue = Globals.METL;
            Title = Strings.Global_ProductName;
            try
            {
                //Icon = (ImageSource)new ImageSourceConverter().ConvertFromString("resources\\" + Globals.MeTLType + ".ico");
                Icon = (ImageSource)new ImageSourceConverter().ConvertFromString("resources\\MeTL Presenter.ico");
            }
            catch (Exception)
            {
                Console.WriteLine("Window1 constructor couldn't find app appropriate icon");
            }
            //create
            Commands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(ImportPowerpoint));
            Commands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.CreateBlankConversation.RegisterCommand(new DelegateCommand<object>(createBlankConversation, mustBeLoggedIn));
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<object>(createConversation, canCreateConversation));
            Commands.ConnectToSmartboard.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            Commands.DisconnectFromSmartboard.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            //conversation movement
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<string>(JoinConversation, mustBeLoggedIn));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(setSync));
            Commands.EditConversation.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversationAndBeAuthor));

            Commands.CloseApplication.RegisterCommand(new DelegateCommand<object>((_unused) => { Logger.CleanupLogQueue(); Application.Current.Shutdown(); }));
            Commands.CloseApplication.RegisterCommand(new DelegateCommand<object>((_unused) => { Logger.CleanupLogQueue(); Application.Current.Shutdown(); }));
            Commands.LogOut.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.Redo.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            Commands.Undo.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));

            Commands.PrintConversation.RegisterCommand(new DelegateCommand<object>(PrintConversation, mustBeInConversation));
            
            Commands.MirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            
            
            Commands.ImageDropped.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.ToggleNavigationLock.RegisterCommand(new DelegateCommand<object>(toggleNavigationLock));
            Commands.SetConversationPermissions.RegisterCommand(new DelegateCommand<object>(SetConversationPermissions, CanSetConversationPermissions));            
            Commands.SendWakeUp.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.ReceiveWakeUp.RegisterCommand(new DelegateCommand<object>(wakeUp));
            Commands.ReceiveSleep.RegisterCommand(new DelegateCommand<object>(sleep));            
                        
            Commands.FileUpload.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeAuthor));

            Commands.ChangeLanguage.RegisterCommand(new DelegateCommand<System.Windows.Markup.XmlLanguage>(changeLanguage));
            Commands.CheckExtendedDesktop.RegisterCommand(new DelegateCommand<object>((_unused) => { CheckForExtendedDesktop(); }));
            
            Commands.SetUserOptions.RegisterCommandToDispatcher(new DelegateCommand<UserOptions>(SetUserOptions));            
            Commands.LaunchDiagnosticWindow.RegisterCommandToDispatcher(new DelegateCommand<object>(launchDiagnosticWindow));
            Commands.DuplicateSlide.RegisterCommand(new DelegateCommand<object>((obj) =>
            {
                duplicateSlide((KeyValuePair<ConversationDetails, Slide>)obj);
            }, (kvp) => {
                try {
                    return (kvp != null && ((KeyValuePair<ConversationDetails, Slide>)kvp).Key != null) ? userMayAdministerConversation(((KeyValuePair<ConversationDetails, Slide>)kvp).Key) : false;
                } catch {
                    return false;
                }
                }));
            Commands.DuplicateConversation.RegisterCommand(new DelegateCommand<ConversationDetails>(duplicateConversation,userMayAdministerConversation));
            Commands.CreateGrouping.RegisterCommand(new DelegateCommand<object>(createGrouping,(o) => mustBeInConversationAndBeAuthor(o)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, PrintBinding));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, HelpBinding, (_unused, e) => { e.Handled = true; e.CanExecute = true; }));
            Commands.Reconnecting.RegisterCommandToDispatcher(new DelegateCommand<bool>(Reconnecting));
            WorkspaceStateProvider.RestorePreviousSettings();
            getDefaultSystemLanguage();
            undoHistory = new UndoHistory();
            displayDispatcherTimer = createExtendedDesktopTimer();
        }

        private void MoveTo(int slide)
        {
            mainFrame.Navigate(new InConversationPage(slide));
        }

        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main()
        {
            // want to customise the main function so we'll create one here instead of the automatically generated one from app.g.cs
            SandRibbon.App.ShowSplashScreen();
            SandRibbon.App app = new SandRibbon.App();

            app.InitializeComponent();
            app.Run();
        }

        private static int checkExtendedInProgress = 0;

        private void dispatcherEventHandler(Object sender, EventArgs args)
        {
            if (1 == Interlocked.Increment(ref checkExtendedInProgress))
            {
                try
                {
                    /// There are three conditions we want to handle
                    /// 1. Extended mode activated, there are now 2 screens
                    /// 2. Extended mode deactivated, back to 1 screen
                    /// 3. Extended screen position has changed, so need to reinit the projector window

                    var screenCount = System.Windows.Forms.Screen.AllScreens.Count();


                    if (Projector.Window == null && screenCount > 1)
                        Commands.ProxyMirrorPresentationSpace.ExecuteAsync(null);
                    else if (Projector.Window != null && screenCount == 1)
                        Projector.Window.Close();
                    else if (Projector.Window != null && screenCount > 1)
                    {
                        // Case 3.
                        Commands.ProxyMirrorPresentationSpace.ExecuteAsync(null);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref checkExtendedInProgress, 0);
                }
            }
        }

        private void CheckForExtendedDesktop()
        {
            if (!displayDispatcherTimer.IsEnabled)
            {
                //This dispather timer is left running in the background and is never stopped
                displayDispatcherTimer.Start();
            }
        }

        private System.Windows.Threading.DispatcherTimer createExtendedDesktopTimer()
        {
            displayDispatcherTimer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.ApplicationIdle, this.Dispatcher);
            displayDispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            displayDispatcherTimer.Tick += new EventHandler(dispatcherEventHandler);
            displayDispatcherTimer.Start();
            return displayDispatcherTimer;
        }

        private void getDefaultSystemLanguage()
        {
            try
            {
                Commands.ChangeLanguage.Execute(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.IetfLanguageTag));
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
        #region helpLinks
        private void OpenEULABrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Settings.Default.UserAgreementUrl);
        }
        private void OpenTutorialBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Settings.Default.TutorialUrl);
        }
        private void OpenReportBugBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Settings.Default.BugReportUrl);
        }
        private void OpenAboutMeTLBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Settings.Default.DescriptionUrl);
        }
        private void launchDiagnosticWindow(object _unused)
        {
            if (App.diagnosticWindow != null) {
                App.diagnosticWindow.Activate();
            }
            else
            {
                Thread newDiagnosticThread = new Thread(new ThreadStart(() =>
                {
                    SynchronizationContext.SetSynchronizationContext(
                        new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Threading.Dispatcher.CurrentDispatcher));
                    var window = new DiagnosticWindow();
                    App.diagnosticWindow = window;
                    window.Closed += (s, a) =>
                    {
                        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background);
                        App.diagnosticWindow = null;
                    };
                    window.Show();
                    System.Windows.Threading.Dispatcher.Run();
                }));
                newDiagnosticThread.SetApartmentState(ApartmentState.STA);
                newDiagnosticThread.IsBackground = true;
                newDiagnosticThread.Start();
            }
        }

        #endregion
        private void changeLanguage(System.Windows.Markup.XmlLanguage lang)
        {
            try
            {
                var culture = lang.GetSpecificCulture();
                FlowDirection = culture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                var mergedDicts = System.Windows.Application.Current.Resources.MergedDictionaries;
                var currentToolTips = mergedDicts.Where(rd => ((ResourceDictionary)rd).Source.ToString().ToLower().Contains("tooltips")).First();
                var rdUri = new Uri("Components\\ResourceDictionaries\\ToolTips_" + lang + ".xaml", UriKind.Relative);
                var newDict = (ResourceDictionary)App.LoadComponent(rdUri);
                var sourceUri = new Uri("ToolTips_" + lang + ".xaml", UriKind.Relative);
                newDict.Source = sourceUri;
                mergedDicts[mergedDicts.IndexOf(currentToolTips)] = newDict;
            }

            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
        private void ApplicationPopup_ShowOptions(object sender, EventArgs e)
        {
            Trace.TraceInformation("UserOptionsDialog_Show");
            if (mustBeLoggedIn(null))
            {
                var userOptions = new UserOptionsDialog();
                userOptions.Owner = Window.GetWindow(this);
                userOptions.ShowDialog();
            }
            else MeTLMessage.Warning("You must be logged in to edit your options");
        }
        private void ImportPowerpoint(object obj)
        {
            if (loader == null) loader = new PowerPointLoader();
            loader.ImportPowerpoint(this);
        }
   
        private void createBlankConversation(object obj)
        {
            var element = Keyboard.FocusedElement;
            if (loader == null) loader = new PowerPointLoader();
            loader.CreateBlankConversation();
        }

        private void HelpBinding(object sender, EventArgs e)
        {
            LaunchHelp(null);
        }

        private void LaunchHelp(object _arg)
        {
            try
            {
                Process.Start("http://monash.edu/eeducation/metl/help.html");
            }
            catch (Exception)
            {
            }
        }
        private void PrintBinding(object sender, EventArgs e)
        {
            PrintConversation(null);
        }
        private void PrintConversation(object _arg)
        {
            if (Globals.UserOptions.includePrivateNotesOnPrint)
                new Printer().PrintPrivate(Globals.conversationDetails.Jid, Globals.me);
            else
                new Printer().PrintHandout(Globals.conversationDetails.Jid, Globals.me);
        }
        private void SetUserOptions(UserOptions options)
        {
            //this next line should be removed.
            App.controller.client.SaveUserOptions(Globals.me, options);
        }
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }
        
        
        /*
        private void ToggleFriendsVisibility(object unused)
        {
            if (chatGridsplitter.Visibility == Visibility.Visible)
            {
                chatGridsplitter.Visibility = Visibility.Collapsed;
                leftDrawer.Visibility = Visibility.Collapsed;
                LHSSplitterDefinition.Width = new GridLength(0);
                LHSDrawerDefinition.Width = new GridLength(0);
            }
            else
            {
                chatGridsplitter.Visibility = Visibility.Visible;
                leftDrawer.Visibility = Visibility.Visible;
                LHSSplitterDefinition.Width = new GridLength(10);
                LHSDrawerDefinition.Width = new GridLength((columns.ActualWidth - rightDrawer.ActualWidth) / 4);
            }
        }
        */               
        
        private void JoinConversation(string title)
        {
            try {                
                var thisDetails = App.controller.client.DetailsOf(title);
                App.controller.client.AsyncRetrieveHistoryOf(Int32.Parse(title));
                applyPermissions(thisDetails.Permissions);
                Commands.SetPrivacy.Execute(thisDetails.Author == Globals.me ? "public" : "private");
                Commands.RequerySuggested(Commands.SetConversationPermissions);
                Commands.SetLayer.ExecuteAsync("Sketch");
            } catch
            {
                Console.WriteLine("couldn't join conversation: " + title);
            }
        }
        private string messageFor(ConversationDetails details)
        {
            var permissionLabel = Permissions.InferredTypeOf(details.Permissions).Label;
            if (details.Equals(ConversationDetails.Empty))
                return Strings.Global_ProductName;
            return string.Format("Collaboration {0}  -  {1}'s \"{2}\" - MeTL", (permissionLabel == "tutorial") ? "ENABLED" : "DISABLED", details.Author, details.Title);
        }
        
        private bool canCreateConversation(object obj)
        {
            return mustBeLoggedIn(obj);
        }
        private bool mustBeLoggedIn(object _arg)
        {
            var v = !Globals.credentials.ValueEquals(Credentials.Empty);
            return v;
        }                
        private bool mustBeInConversationAndBeAuthor(object _arg)
        {
            return mustBeInConversation(_arg) && mustBeAuthor(_arg);
        }
        private bool mustBeInConversation(object _arg)
        {
            var details = Globals.conversationDetails;
            if (!details.IsValid)
                if (Globals.credentials.authorizedGroups.Select(su => su.groupKey.ToLower()).Contains("superuser")) return true;
            var validGroups = Globals.credentials.authorizedGroups.Select(g => g.groupKey.ToLower()).ToList();
            validGroups.Add("unrestricted");
            if (!details.isDeleted && validGroups.Contains(details.Subject.ToLower())) return true;
            return false;
        }
        private bool mustBeAuthor(object _arg)
        {
            return Globals.isAuthor;
        }

        private void duplicateSlide(KeyValuePair<ConversationDetails, Slide> _kvp) {
            var kvp = new KeyValuePair<ConversationDetails,Slide>(Globals.conversationDetails, Globals.slideDetails);
            if (kvp.Key.UserHasPermission(Globals.credentials) && kvp.Key.Slides.Exists(s => s.id == kvp.Value.id))
            {
                App.controller.client.DuplicateSlide(kvp.Key,kvp.Value);
            }
        }
        private void duplicateConversation(ConversationDetails _conversationToDuplicate) {
            var conversationToDuplicate = Globals.conversationDetails;
            if (conversationToDuplicate.UserHasPermission(Globals.credentials))
            {
                App.controller.client.DuplicateConversation(conversationToDuplicate);
            }
        }
        private bool userMayAdministerConversation(ConversationDetails _conversation)
        {
            var conversation = Globals.conversationDetails;
            if (conversation == null)
            {
                return false;
            }
            return conversation.UserHasPermission(Globals.credentials);
        }
        private void createGrouping(object groupingDefinition) {
        }  
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (details.IsEmpty) return;
            Dispatcher.adopt(delegate
                                 {
                                     if (details.Jid.GetHashCode() == Globals.location.activeConversation.GetHashCode() || String.IsNullOrEmpty(Globals.location.activeConversation))
                                     {
                                         UpdateTitle(details);
                                         if (!mustBeInConversation(null))
                                         {                                             
                                             Commands.LeaveLocation.Execute(null);
                                         }
                                     }
                                 });
        }
        private void UpdateTitle(ConversationDetails details)
        {
            if (Globals.conversationDetails != null && mustBeInConversation(null))
            {
                Title = messageFor(Globals.conversationDetails);
            }
            else
                Title = Strings.Global_ProductName;
        }
        private DelegateCommand<object> canOpenFriendsOverride;
        private void applyPermissions(Permissions permissions)
        {
            if (canOpenFriendsOverride != null)
                Commands.ToggleFriendsVisibility.UnregisterCommand(canOpenFriendsOverride);
            canOpenFriendsOverride = new DelegateCommand<object>((_param) => { }, (_param) => true);
            Commands.ToggleFriendsVisibility.RegisterCommand(canOpenFriendsOverride);
        }        
        private void createConversation(object detailsObject)
        {         
            var details = (ConversationDetails)detailsObject;
            if (details == null) return;
            if (Commands.CreateConversation.CanExecute(details))
            {
                if (details.Tag == null)
                    details.Tag = "unTagged";
                details.Author = Globals.userInformation.credentials.name;
                var connection = App.controller.client;
                details = connection.CreateConversation(details);
                CommandManager.InvalidateRequerySuggested();
                if (Commands.JoinConversation.CanExecute(details.Jid))
                    Commands.JoinConversation.ExecuteAsync(details.Jid);
            }
        }
        private void setSync(object _obj)
        {
            Globals.userInformation.policy.isSynced = !Globals.userInformation.policy.isSynced;
        }
        
        public Visibility GetVisibilityOf(UIElement target)
        {
            return target.Visibility;
        }
        public void toggleNavigationLock(object _obj)
        {
            try
            {
                var details = Globals.conversationDetails;
                if (details == null)
                    return;
                details.Permissions.NavigationLocked = !details.Permissions.NavigationLocked;
                App.controller.client.UpdateConversationDetails(details);
            }
            catch (NotSetException)
            {
                return;
            }
        }
        private void SetConversationPermissions(object obj)
        {
            var style = (string)obj;
            try
            {
                var details = Globals.conversationDetails;
                if (details == null)
                    return;
                if (style == "lecture")
                    details.Permissions.applyLectureStyle();
                else
                    details.Permissions.applyTuteStyle();
                App.controller.client.UpdateConversationDetails(details);
            }
            catch (NotSetException)
            {
                return;
            }
        }
        private bool CanSetConversationPermissions(object _style)
        {
            return Globals.isAuthor;
        }

        private void sleep(object _obj)
        {
            Dispatcher.adoptAsync(delegate
            {
                Hide();
            });
        }
        private void wakeUp(object _obj)
        {
            Dispatcher.adoptAsync(delegate
            {
                Show();
                WindowState = System.Windows.WindowState.Maximized;
            });
        }        
        private class PreferredDisplayIndexComparer : IComparer<FrameworkElement>
        {
            public int Compare(FrameworkElement anX, FrameworkElement aY)
            {
                try
                {
                    var x = Int32.Parse((string)anX.FindResource("preferredDisplayIndex"));
                    var y = Int32.Parse((string)aY.FindResource("preferredDisplayIndex"));
                    return x - y;
                }
                catch (FormatException)
                {
                    return 0;
                }
            }
        }

        private void Reconnecting(bool success)
        {
            if (success)
            {
                try
                {
                    hideReconnectingDialog();
                    var details = Globals.conversationDetails;
                    if (details == null || details.Equals(ConversationDetails.Empty))
                    {
                        Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
                    }
                    else
                    {
                        var jid = Globals.conversationDetails.Jid;
                        Commands.UpdateConversationDetails.Execute(App.controller.client.DetailsOf(jid));
                        Commands.MoveTo.Execute(Globals.location.currentSlide);
                        SlideDisplay.SendSyncMove(Globals.location.currentSlide);
                        App.controller.client.historyProvider.Retrieve<PreParser>(
                                    null,
                                    null,
                                    (parser) =>
                                    {
                                        Commands.PreParserAvailable.Execute(parser);
                                    },
                                    jid);
                    }
                }
                catch (NotSetException e)
                {
                    Logger.Crash(e);
                    Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
                }
                catch (Exception e)
                {
                    Logger.Log(string.Format("CRASH: (Fixed) Window1::Reconnecting crashed {0}", e.Message));
                    Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
                }
            }
            else
            {
                showReconnectingDialog();
            }
        }

        private void showReconnectingDialog()
        {
            //throw new NotImplementedException();
        }

        private void hideReconnectingDialog()
        {
            //throw new NotImplementedException();
        }

        private void ribbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.AccidentallyClosing.AddMilliseconds(250) > DateTime.Now)
            {
                e.Cancel = true;
            }
            else
            {
                Commands.CloseApplication.Execute(null);
                Application.Current.Shutdown();
            }
        }
        private void ApplicationPopup_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            App.AccidentallyClosing = DateTime.Now;
        }

        private void ribbon_SelectedTabChanged(object sender, EventArgs e)
        {
            var ribbon = sender as Ribbon;
        }
        /* Awesomium commented out
        private void setStackVisibility(Visibility visible)
        {
            if (visible == Visibility.Visible)
            {
                webMeTLGridSplitter.Visibility = Visibility.Visible;
                WebMeTLSplitterDefinition.Width = new GridLength(10);
                WebMeTLDrawerDefinition.Width = new GridLength(400);
                WebMeTLDrawerDefinition.MinWidth = 100;
                webMeTLDrawer.Visibility = Visibility.Visible;
            }
            else
            {
                webMeTLGridSplitter.Visibility = Visibility.Collapsed;
                WebMeTLDrawerDefinition.MinWidth = 0;
                WebMeTLDrawerDefinition.Width = new GridLength(0);
                WebMeTLSplitterDefinition.Width = new GridLength(0);
                webMeTLDrawer.Visibility = Visibility.Collapsed;
            }
        }
        // End of Awesomium comment out
        */
    }
}
