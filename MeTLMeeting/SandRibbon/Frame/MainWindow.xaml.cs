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
using System.Threading.Tasks;
using System.ComponentModel;
using Awesomium.Core;
using System.Windows.Navigation;
using SandRibbon.Pages.Conversations;
using SandRibbon.Pages.Integration;
using SandRibbon.Pages.Analytics;

namespace SandRibbon
{
    public enum PresentationStrategy {
        WindowCoordinating,PageCoordinating
    }
    public partial class MainWindow : MetroWindow
    {
        private System.Windows.Threading.DispatcherTimer displayDispatcherTimer;

        private PowerPointLoader loader;
        private UndoHistory undoHistory;
        public string CurrentProgress { get; set; }
        public static RoutedCommand ProxyMirrorExtendedDesktop = new RoutedCommand();

        public MainWindow()
        {
            MessageBox.Show("MainWindow construction begun");
            InitializeComponent();
            MessageBox.Show("MainWindow component initialized");
            DoConstructor();
            MessageBox.Show("MainWindow constructor executed");
            Commands.AllStaticCommandsAreRegistered();
            MessageBox.Show("MainWindow commands registered");
            mainFrame.Navigate(new ServerSelectorPage());
            MessageBox.Show("MainWindow server selector page");
            App.CloseSplashScreen();
            MessageBox.Show("MainWindow constructor complete");
        }        

        private void DoConstructor()
        {
            Commands.LaunchDiagnosticWindow.RegisterCommand(new DelegateCommand<object>(launchDiagnosticsWindow));

            Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
            Commands.SetPedagogyLevel.DefaultValue = ConfigurationProvider.instance.getMeTLPedagogyLevel();
            Commands.MeTLType.DefaultValue = Globals.METL;
            Title = Strings.Global_ProductName;
            //create
            Commands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(ImportPowerpoint));
            //Commands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(App.noop));
            Commands.CreateBlankConversation.RegisterCommand(new DelegateCommand<object>(createBlankConversation));
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<object>(createConversation));
            Commands.ConnectToSmartboard.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            Commands.DisconnectFromSmartboard.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            Commands.ManuallyConfigureOneNote.RegisterCommand(new DelegateCommand<object>(openOneNoteConfiguration));
            Commands.BrowseOneNote.RegisterCommand(new DelegateCommand<OneNoteConfiguration>(browseOneNote));
            Commands.SerializeConversationToOneNote.RegisterCommand(new DelegateCommand<OneNoteSynchronizationSet>(serializeConversationToOneNote, mustBeInConversation));
            //conversation movement
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(setSync));
            Commands.EditConversation.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversationAndBeAuthor));
            Commands.MoveToOverview.RegisterCommand(new DelegateCommand<NetworkController>(MoveToOverview, mustBeInConversation));
            if (presentationStrategy == PresentationStrategy.WindowCoordinating)
            {
                Commands.MoveToNext.RegisterCommand(new DelegateCommand<object>(o => Shift(1), mustBeInConversation));
                Commands.MoveToPrevious.RegisterCommand(new DelegateCommand<object>(o => Shift(-1), mustBeInConversation));
                Commands.MoveToNotebookPage.RegisterCommand(new DelegateCommand<NotebookPage>(NavigateToNotebookPage));
            }            
            Commands.WordCloud.RegisterCommand(new DelegateCommand<object>(WordCloud));
            
            Commands.Redo.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            Commands.Undo.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));

            Commands.MoreTextOptions.RegisterCommand(new DelegateCommand<object>(MoreTextOptions));
            Commands.MoreImageOptions.RegisterCommand(new DelegateCommand<object>(MoreImageOptions));

            Commands.PrintConversation.RegisterCommand(new DelegateCommand<NetworkController>(PrintConversation, mustBeInConversation));

            Commands.ImageDropped.RegisterCommand(new DelegateCommand<object>(App.noop));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<object>(App.noop));
            Commands.ToggleNavigationLock.RegisterCommand(new DelegateCommand<object>(toggleNavigationLock));
            Commands.SetConversationPermissions.RegisterCommand(new DelegateCommand<object>(SetConversationPermissions, CanSetConversationPermissions));

            Commands.FileUpload.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeAuthor));
            Commands.PickImages.RegisterCommand(new DelegateCommand<PickContext>(PickImages));

            Commands.ChangeLanguage.RegisterCommand(new DelegateCommand<System.Windows.Markup.XmlLanguage>(changeLanguage));
            Commands.CheckExtendedDesktop.RegisterCommand(new DelegateCommand<object>((_unused) => { CheckForExtendedDesktop(); }));

            Commands.Reconnecting.RegisterCommandToDispatcher(new DelegateCommand<bool>(Reconnecting));
            Commands.SetUserOptions.RegisterCommandToDispatcher(new DelegateCommand<UserOptions>(SetUserOptions));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, PrintBinding));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, HelpBinding, (_unused, e) => { e.Handled = true; e.CanExecute = true; }));

            Commands.ModifySelection.RegisterCommand(new DelegateCommand<IEnumerable<PrivateAwareStroke>>(ModifySelection));
            Commands.SerializeConversationToOneNote.RegisterCommand(new DelegateCommand<OneNoteSynchronizationSet>(synchronizeToOneNote));

            Commands.JoinCreatedConversation.RegisterCommand(new DelegateCommand<object>(joinCreatedConversation));

            WorkspaceStateProvider.RestorePreviousSettings();
            getDefaultSystemLanguage();
            undoHistory = new UndoHistory();
            displayDispatcherTimer = createExtendedDesktopTimer();
            mainFrame.Navigating += MainFrame_Navigating;
        }

        private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Globals.currentPage = e.Content.GetType().Name as String;
        }

        private void openProjectorWindow(object _unused)
        {
            Commands.MirrorPresentationSpace.Execute(this);
        }
        private void WordCloud(object obj)
        {
            mainFrame.Navigate(new TagCloudPage(App.controller));
        }

        private void serializeConversationToOneNote(OneNoteSynchronizationSet obj)
        {
            mainFrame.Navigate(obj.networkController.conversationSearchPage);// new ConversationSearchPage(obj.networkController));
        }

        private void PickImages(PickContext context)
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = "png";
            dialog.FileOk += delegate
            {
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
            mainFrame.Navigate(new OneNotePage(page));
        }
        private void synchronizeToOneNote(OneNoteSynchronizationSet sync) {            
            var w = new WebControl();
            var config = sync.config;
            DocumentReadyEventHandler ready = null;
            ready = (s, e) =>
            {
                var queryPart = e.Url.AbsoluteUri.Split('#');
                if (queryPart.Length > 1)
                {
                    var ps = HttpUtility.ParseQueryString(queryPart[1]);
                    var token = ps["access_token"];
                    if (token != null)
                    {
                        w.DocumentReady -= ready;
                        sync.token = token;
                        flyout.IsOpen = false;
                        mainFrame.Navigate(new OneNoteSynchronizationPage(sync));
                    }
                }
            };
            w.DocumentReady += ready;
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
        private void browseOneNote(OneNoteConfiguration config)
        {
            var w = new WebControl();
            DocumentReadyEventHandler ready = null;
            ready = (s, e) =>
            {
                var queryPart = e.Url.AbsoluteUri.Split('#');
                if (queryPart.Length > 1)
                {
                    var ps = HttpUtility.ParseQueryString(queryPart[1]);
                    var token = ps["access_token"];
                    if (token != null)
                    {
                        w.DocumentReady -= ready;
                        flyout.DataContext = Globals.OneNoteConfiguration;
                        flyout.Content = TryFindResource("oneNoteListing");
                        var oneNoteModel = flyout.DataContext as OneNoteConfiguration;
                        oneNoteModel.LoadNotebooks(token);
                    }
                }
            };
            w.DocumentReady += ready;
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

        private void openOneNoteConfiguration(object obj)
        {
            flyout.Content = TryFindResource("oneNoteConfiguration");
            flyout.DataContext = Globals.OneNoteConfiguration;
            flyout.IsOpen = true;
        }

        private void MoveToOverview(NetworkController obj)
        {
            mainFrame.Navigate(new ConversationOverviewPage(obj, Globals.conversationDetails));
        }

        private void Shift(int direction)
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
            Commands.MoveToCollaborationPage.Execute(slides[targetIndex]);
            //mainFrame.Navigate(new GroupCollaborationPage(slides[targetIndex]));
        }


        private void ModifySelection(IEnumerable<PrivateAwareStroke> obj)
        {
            this.flyout.Content = TryFindResource("worm");
            this.flyout.IsOpen = !this.flyout.IsOpen;
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
                        Commands.ProxyMirrorPresentationSpace.ExecuteAsync(this);
                    else if (Projector.Window != null && screenCount == 1)
                        Projector.Window.Close();
                    else if (Projector.Window != null && screenCount > 1)
                    {
                        // Case 3.
                        Commands.ProxyMirrorPresentationSpace.ExecuteAsync(this);
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
            }
        }
        private void ApplicationPopup_ShowOptions(object sender, EventArgs e)
        {
            Trace.TraceInformation("UserOptionsDialog_Show");           
                var userOptions = new UserOptionsDialog();
                userOptions.Owner = Window.GetWindow(this);
                userOptions.ShowDialog();                       
        }
        private void ImportPowerpoint(object obj)
        {
            if (loader == null) loader = new PowerPointLoader();
            loader.ImportPowerpoint(this,(PowerpointImportType)obj);
        }
        private void createBlankConversation(object obj)
        {            
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
            //not sure what to do about this yet - I partly think that this binding should be at frame level, rather than window level.
            //PrintConversation(networkController);
        }
        private void PrintConversation(NetworkController networkController)
        {
            if (Globals.UserOptions.includePrivateNotesOnPrint)
                new Printer(networkController).PrintPrivate(Globals.conversationDetails.Jid, Globals.me);
            else
                new Printer(networkController).PrintHandout(Globals.conversationDetails.Jid, Globals.me);
        }
        private void SetUserOptions(UserOptions options)
        {
            //this next line should be removed.
            SaveUserOptions(options);
        }
        private void SaveUserOptions(UserOptions options)
        {
            //this should be wired to a new command, SaveUserOptions, which is commented out in SandRibbonInterop.Commands
            App.controller.client.SaveUserOptions(Globals.me, options);
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
                        Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
                    }
                    else
                    {
                        var jid = Globals.conversationDetails.Jid;
                        Commands.UpdateConversationDetails.Execute(App.controller.client.DetailsOf(jid));
                        Commands.MoveToCollaborationPage.Execute(Globals.location.currentSlide);
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
                    Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
                }
                catch (Exception e)
                {
                    Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
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
            return string.Format("CONVERSATION {2}: {0}'s \"{1}\"", details.Author, details.Title, details.Permissions.studentCanPublish? "ENABLED" : "DISABLED");
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
        private PresentationStrategy presentationStrategy;

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
                    Commands.JoinConversation.Execute(details.Jid);
                mainFrame.NavigationService.Navigate(new ConversationOverviewPage(App.controller, details));
            }
        }
        protected void joinCreatedConversation(object detailsObject)
        {
            var details = (ConversationDetails)detailsObject;
            CommandManager.InvalidateRequerySuggested();
            if (Commands.JoinConversation.CanExecute(details.Jid))
                Commands.JoinConversation.Execute(details.Jid);
            Dispatcher.adopt(delegate {
                mainFrame.NavigationService.Navigate(new ConversationOverviewPage(App.controller, details));
            });
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

            var details = Globals.conversationDetails;
            if (details == null)
                return;
            details.Permissions.NavigationLocked = !details.Permissions.NavigationLocked;
            App.controller.client.UpdateConversationDetails(details);
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

        private void UserPreferences(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new CommandBarConfigurationPage());
        }        
        protected void ShowDiagnosticsWindow(object sender, RoutedEventArgs e)
        {
            launchDiagnosticsWindow(null);
        }
        protected void launchDiagnosticsWindow(object _unused)
        {
            if (App.diagnosticWindow == null)
            {
                App.diagnosticWindow = new DiagnosticWindow();
            } 
            App.diagnosticWindow.Show();
        }
    }
}
