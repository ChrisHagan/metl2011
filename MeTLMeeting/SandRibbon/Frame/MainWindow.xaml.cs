using Microsoft.Windows.Controls.Ribbon;
using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.ServerSelection;
using SandRibbon.Properties;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SandRibbon.Pages.Collaboration.Palettes;
using MahApps.Metro.Controls;
using SandRibbon.Pages.Conversations.Models;
using System.Web;
using Awesomium.Windows.Controls;
using Microsoft.Win32;

namespace SandRibbon
{
    public partial class MainWindow : MetroWindow
    {
        private System.Windows.Threading.DispatcherTimer displayDispatcherTimer;

        private PowerPointLoader loader;
        private UndoHistory undoHistory;
        public string CurrentProgress { get; set; }
        public static RoutedCommand ProxyMirrorExtendedDesktop = new RoutedCommand();
        private AsyncObservableCollection<LogMessage> logs = new AsyncObservableCollection<LogMessage>();

        public MainWindow()
        {
            InitializeComponent();
            DoConstructor();
            AppCommands.AllStaticCommandsAreRegistered();
            mainFrame.Navigate(new ServerSelectorPage(App.getAvailableServers()));
            App.CloseSplashScreen();
        }


        public MetlConfiguration backend
        {
            get { return (MetlConfiguration)GetValue(backendProperty); }
            set { SetValue(backendProperty, value); }
        }

        // Using a DependencyProperty as the backing store for backend.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty backendProperty =
            DependencyProperty.Register("backend", typeof(MetlConfiguration), typeof(MainWindow), new PropertyMetadata(MetlConfiguration.empty));



        //public MetlConfiguration backend = MetlConfiguration.empty;
        protected void backendSelected(MetlConfiguration config)
        {
            backend = config;
        }
        public void Log(string message)
        {
            if (String.IsNullOrEmpty(message)) return;
            logs.Add(new LogMessage
            {
                content = message,
                user = App.getContextFor(backend).controller.creds.name,
                slide = Globals.location.currentSlide,
                timestamp = DateTime.Now.Ticks
            });
        }

        private void DoConstructor()
        {
            AppCommands.Mark.RegisterCommand(new DelegateCommand<string>(Log));
            AppCommands.BackendSelected.RegisterCommand(new DelegateCommand<MetlConfiguration>(backendSelected));
            //App.getContextFor(backend).controller.commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
            AppCommands.SetPedagogyLevel.DefaultValue = ConfigurationProvider.instance.getMeTLPedagogyLevel();
            AppCommands.MeTLType.DefaultValue = Globals.METL;
            Title = Strings.Global_ProductName;
            //create
            AppCommands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(ImportPowerpoint));
            AppCommands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            AppCommands.CreateBlankConversation.RegisterCommand(new DelegateCommand<object>(createBlankConversation, mustBeLoggedIn));
            AppCommands.CreateConversation.RegisterCommand(new DelegateCommand<object>(createConversation, canCreateConversation));
            AppCommands.ConnectToSmartboard.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            AppCommands.DisconnectFromSmartboard.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            AppCommands.ManuallyConfigureOneNote.RegisterCommand(new DelegateCommand<MetlConfiguration>(o => openOneNoteConfiguration(o)));
            AppCommands.BrowseOneNote.RegisterCommand(new DelegateCommand<OneNoteConfiguration>(browseOneNote));
            //conversation movement
            AppCommands.SetSync.RegisterCommand(new DelegateCommand<object>(setSync));
            AppCommands.EditConversation.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversationAndBeAuthor));
            AppCommands.MoveToOverview.RegisterCommand(new DelegateCommand<MetlConfiguration>(o => MoveToOverview(o), mustBeInConversation));
            AppCommands.MoveToNext.RegisterCommand(new DelegateCommand<MetlConfiguration>(o => Shift(o,1), mustBeInConversation));
            AppCommands.MoveToPrevious.RegisterCommand(new DelegateCommand<MetlConfiguration>(o => Shift(o,-1), mustBeInConversation));
            AppCommands.MoveToNotebookPage.RegisterCommand(new DelegateCommand<NotebookPage>(NavigateToNotebookPage));

            AppCommands.LogOut.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            //AppCommands.Redo.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            //AppCommands.Undo.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));

            AppCommands.MoreTextOptions.RegisterCommand(new DelegateCommand<object>(MoreTextOptions));
            AppCommands.MoreImageOptions.RegisterCommand(new DelegateCommand<object>(MoreImageOptions));

            AppCommands.PrintConversation.RegisterCommand(new DelegateCommand<object>(PrintConversation, mustBeInConversation));

            AppCommands.ImageDropped.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            //App.getContextFor(backend).controller.commands.SendQuiz.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            AppCommands.ToggleNavigationLock.RegisterCommand(new DelegateCommand<object>(toggleNavigationLock));
            AppCommands.SetConversationPermissions.RegisterCommand(new DelegateCommand<object>(SetConversationPermissions, CanSetConversationPermissions));

            AppCommands.FileUpload.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeAuthor));
            AppCommands.PickImages.RegisterCommand(new DelegateCommand<PickContext>(PickImages));

            AppCommands.ChangeLanguage.RegisterCommand(new DelegateCommand<System.Windows.Markup.XmlLanguage>(changeLanguage));
            AppCommands.CheckExtendedDesktop.RegisterCommand(new DelegateCommand<object>((_unused) => { CheckForExtendedDesktop(); }));

            AppCommands.Reconnecting.RegisterCommandToDispatcher(new DelegateCommand<bool>(Reconnecting));
            AppCommands.SetUserOptions.RegisterCommandToDispatcher(new DelegateCommand<UserOptions>(SetUserOptions));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, PrintBinding));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, HelpBinding, (_unused, e) => { e.Handled = true; e.CanExecute = true; }));

            AppCommands.ModifySelection.RegisterCommand(new DelegateCommand<IEnumerable<PrivateAwareStroke>>(ModifySelection));

            WorkspaceStateProvider.RestorePreviousSettings();
            getDefaultSystemLanguage();
            displayDispatcherTimer = createExtendedDesktopTimer();
        }

        private void PickImages(PickContext context)
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = "png";
            dialog.FileOk += delegate {
                foreach (var file in dialog.FileNames)
                {
                    context.Files.Add(file);
                }
            };
            dialog.ShowDialog();
        }

        private void MoreImageOptions(object obj)
        {
            flyout.Content = TryFindResource("moreImageOptions");
            flyout.DataContext = new PickContext();
            flyout.IsOpen = true;
        }

        private void MoreTextOptions(object obj)
        {
            flyout.Content = TryFindResource("moreTextOptions");
            flyout.IsOpen = true;
        }

        private void NavigateToNotebookPage(NotebookPage page)
        {
            mainFrame.Navigate(new OneNotePage(page.Backend, page));
        }

        private void browseOneNote(OneNoteConfiguration config)
        {
            var w = new WebControl();
            w.DocumentReady += delegate { W_DocumentReady(new OneNote(config)); };
            flyout.Content = w;
            flyout.Width = 600;
            flyout.IsOpen = true;
            var scope = "office.onenote_update";
            var responseType = "token";
            var clientId = config.apiKey;
            var redirectUri = "https://login.live.com/oauth20_desktop.srf";
            var req = "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type={2}&redirect_uri={3}";
            var uri = new Uri(String.Format(req,
                config.apiKey,
                scope,
                responseType,
                redirectUri));
            w.Source = uri;
        }

        private Action<object, Awesomium.Core.DocumentReadyEventArgs> W_DocumentReady(OneNote oneNote)
        {
            return new Action<object, Awesomium.Core.DocumentReadyEventArgs>((sender, e) =>
            {
                var queryPart = e.Url.AbsoluteUri.Split('#');
                if (queryPart.Length > 1)
                {
                    var ps = HttpUtility.ParseQueryString(queryPart[1]);
                    var token = ps["access_token"];
                    if (token != null)
                    {
                        var config = oneNote.config;
                        Console.WriteLine("Token: {0}", token);
                        flyout.DataContext = config;
                        flyout.Content = TryFindResource("oneNoteListing");
                        //var oneNoteModel = flyout.DataContext as OneNoteConfiguration;
                        config.Books.Clear();
                        foreach (var book in oneNote.Notebooks(token))
                        {
                            config.Books.Add(book);
                        }
                    }

                }
            });
        }

    private void openOneNoteConfiguration(MetlConfiguration backend)
        {            
            flyout.Content = TryFindResource("oneNoteConfiguration");
            flyout.DataContext = OneNoteConfiguration.create(backend);
            flyout.IsOpen = true;
        }
        
        private void MoveToOverview(MetlConfiguration backend)
        {
            mainFrame.Navigate(new ConversationOverviewPage(backend,Globals.conversationDetails));
        }
        

            
        private void Shift(MetlConfiguration backend,int direction)
        {
            var details = Globals.conversationDetails;
            var slides = details.Slides.OrderBy(s => s.index).Select(s => s.id).ToList();
            var currentIndex = slides.IndexOf(Globals.location.currentSlide);
            if (currentIndex < 0) return;
            var end = slides.Count - 1;
            var targetIndex = 0;
            if (direction >= 0 && currentIndex == end) targetIndex = 0;
            else if (direction >= 0) targetIndex = currentIndex + 1;
            else if (currentIndex == 0) targetIndex = end;
            else targetIndex = currentIndex - 1;

            mainFrame.Navigate(new GroupCollaborationPage(backend,slides[targetIndex]));
        }
        

        private void ModifySelection(IEnumerable<PrivateAwareStroke> obj)
        {
            this.flyout.Content = TryFindResource("worm");
            this.flyout.IsOpen= !this.flyout.IsOpen;
        }
        private void ImportPowerpoint(object obj)
        {
            if (loader == null) loader = new PowerPointLoader(backend);
            loader.ImportPowerpoint(this);
        }
        private void createBlankConversation(object obj)
        {
            var element = Keyboard.FocusedElement;
            if (loader == null) loader = new PowerPointLoader(backend);
            loader.CreateBlankConversation();
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
                        AppCommands.ProxyMirrorPresentationSpace.ExecuteAsync(null);
                    else if (Projector.Window != null && screenCount == 1)
                        Projector.Window.Close();
                    else if (Projector.Window != null && screenCount > 1)
                    {
                        // Case 3.
                        AppCommands.ProxyMirrorPresentationSpace.ExecuteAsync(null);
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
                AppCommands.ChangeLanguage.Execute(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.IetfLanguageTag));
            }
            catch (Exception e)
            {
                Log(string.Format("Exception in MainWindow.getDefaultSystemLanguage: {0}", e.Message));
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
                Log(string.Format("Failure in language set: {0}", e.Message));
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
                new Printer(backend).PrintPrivate(Globals.conversationDetails.Jid, App.getContextFor(backend).controller.creds.name);
            else
                new Printer(backend).PrintHandout(Globals.conversationDetails.Jid, App.getContextFor(backend).controller.creds.name);
        }
        private void SetUserOptions(UserOptions options)
        {
            //this next line should be removed.
            SaveUserOptions(options);            
        }
        private void SaveUserOptions(UserOptions options)
        {
            //this should be wired to a new command, SaveUserOptions, which is commented out in SandRibbonInterop.Commands
            App.getContextFor(backend).controller.client.SaveUserOptions(App.getContextFor(backend).controller.creds.name, options);
        }        

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }

        private void Reconnecting(bool success)
        {
            if (success)
            {
                try
                {                    
                    var details = Globals.conversationDetails;
                    if (details == null || details.Equals(ConversationDetails.Empty))
                    {
                        App.getContextFor(backend).controller.commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
                    }
                    else
                    {
                        var jid = Globals.conversationDetails.Jid;
                        App.getContextFor(backend).controller.commands.UpdateConversationDetails.Execute(App.getContextFor(backend).controller.client.DetailsOf(jid));
                        App.getContextFor(backend).controller.commands.MoveToCollaborationPage.Execute(Globals.location.currentSlide);
                        SlideDisplay.SendSyncMove(Globals.location.currentSlide,backend);
                        App.getContextFor(backend).controller.client.historyProvider.Retrieve<PreParser>(
                                    null,
                                    null,
                                    (parser) =>
                                    {
                                        App.getContextFor(backend).controller.commands.PreParserAvailable.Execute(parser);                 
                                    },
                                    jid);
                    }
                }
                catch (NotSetException e)
                {
                    Log(string.Format("Reconnecting: {0}",e.Message));
                    App.getContextFor(backend).controller.commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
                }
                catch (Exception e)
                {
                    Log(string.Format("Reconnecting: {0}", e.Message));
                    App.getContextFor(backend).controller.commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
                }
            }
            else
            {
                showReconnectingDialog();
            }
        }                    
        
        private string messageFor(ConversationDetails details)
        {
            var permissionLabel = Permissions.InferredTypeOf(details.Permissions).Label;
            if (details.Equals(ConversationDetails.Empty))
                return Strings.Global_ProductName;
            return string.Format("Collaboration {0}  -  {1}'s \"{2}\" - MeTL", (permissionLabel == "tutorial") ? "ENABLED" : "DISABLED", details.Author, details.Title);
        }                
        private void showReconnectingDialog()
        {
            var majorHeading = new TextBlock
                           {
                               Foreground = Brushes.White,
                               Text = "Connection lost...  Reconnecting",
                               FontSize = 72,
                               HorizontalAlignment = HorizontalAlignment.Center,
                               VerticalAlignment = VerticalAlignment.Center
                           };
            var minorHeading = new TextBlock
                           {
                               Foreground = Brushes.White,
                               Text = "You must have an active internet connection,\nand you must not be logged in twice with the same account.",
                               FontSize = 30,
                               HorizontalAlignment = HorizontalAlignment.Center,
                               VerticalAlignment = VerticalAlignment.Center
                           };            
        }
        private bool canCreateConversation(object obj)
        {
            return mustBeLoggedIn(obj);
        }
        private bool mustBeLoggedIn(object _arg)
        {
            var v = !App.getContextFor(backend).controller.creds.ValueEquals(Credentials.Empty);
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
                if (App.getContextFor(backend).controller.creds.authorizedGroups.Select(su => su.groupKey.ToLower()).Contains("superuser")) return true;
            var validGroups = App.getContextFor(backend).controller.creds.authorizedGroups.Select(g => g.groupKey.ToLower()).ToList();
            validGroups.Add("unrestricted");
            if (!details.isDeleted && validGroups.Contains(details.Subject.ToLower())) return true;
            return false;
        }
        private bool mustBeAuthor(object _arg)
        {
            return Globals.isAuthor(App.getContextFor(backend).controller.creds.name);
        }
        /*
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
                                             App.getContextFor(backend).controller.commands.LeaveLocation.Execute(null);
                                         }
                                     }
                                 });
        }
        */
        private void UpdateTitle(ConversationDetails details)
        {
            if (Globals.conversationDetails != null && mustBeInConversation(null))
            {
#if DEBUG
                    Title = String.Format("{0} [Build: {1}]", messageFor(Globals.conversationDetails), "not merc");//SandRibbon.Properties.HgID.Version); 
#else
                Title = messageFor(Globals.conversationDetails);
#endif
            }
            else
                Title = Strings.Global_ProductName;
        }
        private DelegateCommand<object> canOpenFriendsOverride;
        private void applyPermissions(Permissions permissions)
        {
            if (canOpenFriendsOverride != null)
                AppCommands.ToggleFriendsVisibility.UnregisterCommand(canOpenFriendsOverride);
            canOpenFriendsOverride = new DelegateCommand<object>((_param) => { }, (_param) => true);
            AppCommands.ToggleFriendsVisibility.RegisterCommand(canOpenFriendsOverride);
        }
        
        private void createConversation(object detailsObject)
        {
            var details = (ConversationDetails)detailsObject;
            if (details == null) return;
            if (AppCommands.CreateConversation.CanExecute(details))
            {
                if (details.Tag == null)
                    details.Tag = "unTagged";
                details.Author = App.getContextFor(backend).controller.creds.name;
                var connection = App.getContextFor(backend).controller.client;
                details = connection.CreateConversation(details);
                CommandManager.InvalidateRequerySuggested();
                if (App.getContextFor(backend).controller.commands.JoinConversation.CanExecute(details.Jid))
                    App.getContextFor(backend).controller.commands.JoinConversation.ExecuteAsync(details.Jid);
            }
        }
        private void setSync(object _obj)
        {
            Globals.SynchronizationPolicy = !Globals.SynchronizationPolicy;
        }
     
      
        public Visibility GetVisibilityOf(UIElement target)
        {
            return target.Visibility;
        }
        public void toggleNavigationLock(object _obj)
        {
           
                var details = Globals.conversationDetails;
                if (details == null)
                    return;
                details.Permissions.NavigationLocked = !details.Permissions.NavigationLocked;
                App.getContextFor(backend).controller.client.UpdateConversationDetails(details);                       
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
                App.getContextFor(backend).controller.client.UpdateConversationDetails(details);
            }
            catch (NotSetException)
            {
                return;
            }
        }
        private bool CanSetConversationPermissions(object _style)
        {
            return Globals.isAuthor(App.getContextFor(backend).controller.creds.name);
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
                WindowState = WindowState.Maximized;
            });
        }
        
        
        
        private void ribbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.AccidentallyClosing.AddMilliseconds(250) > DateTime.Now)
            {
                e.Cancel = true;
            }
            else
            {
                AppCommands.CloseApplication.Execute(null);
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

        private void UserPreferences(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new CommandBarConfigurationPage(backend,Globals.currentProfile));
        }        

        private void ShowDiagnostics(object sender, RoutedEventArgs e)
        {
            this.flyout.Content = TryFindResource("diagnostics");
            this.flyout.DataContext = logs;
            this.flyout.IsOpen = true;
        }
    }
}
