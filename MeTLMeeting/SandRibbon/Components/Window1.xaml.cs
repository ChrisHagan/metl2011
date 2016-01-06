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
using MeTLLib.Providers.Connection;
using System.Windows.Media.Imaging;
using System.Threading;
using SandRibbon.Properties;

namespace SandRibbon
{
    public partial class Window1
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
        public Window1()
        {
            DoConstructor();
            Commands.AllStaticCommandsAreRegistered();
            App.mark("Window1 constructor complete");
        }
        private void DoConstructor()
        {
            InitializeComponent();

            Commands.SetIdentity.RegisterCommand(new DelegateCommand<object>(_arg =>
            {
                App.mark("Window1 knows about identity");
            }));
            Commands.ZoomIn.RegisterCommand(new DelegateCommand<object>(doZoomIn, canZoomIn));
            Commands.ZoomOut.RegisterCommand(new DelegateCommand<object>(doZoomOut, canZoomOut));

            Commands.UpdateConversationDetails.DefaultValue = ConversationDetails.Empty;
            Commands.SetPedagogyLevel.DefaultValue = ConfigurationProvider.instance.getMeTLPedagogyLevel();
            Commands.MeTLType.DefaultValue = Globals.METL;
            Title = Strings.Global_ProductName;
            PreviewKeyDown += new KeyEventHandler(KeyPressed);
            //create
            Commands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(ImportPowerpoint));
            Commands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.CreateBlankConversation.RegisterCommand(new DelegateCommand<object>(createBlankConversation, mustBeLoggedIn));
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<object>(createConversation, canCreateConversation));
            Commands.ConnectToSmartboard.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            Commands.DisconnectFromSmartboard.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            //conversation movement
            Commands.MoveTo.RegisterCommand(new DelegateCommand<Location>(ExecuteMoveTo));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<ConversationDetails>(JoinConversation, mustBeLoggedIn));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.EditConversation.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversationAndBeAuthor));

            Commands.CloseApplication.RegisterCommand(new DelegateCommand<object>((_unused) => { Logger.CleanupLogQueue(); Application.Current.Shutdown(); }));
            Commands.CloseApplication.RegisterCommand(new DelegateCommand<object>((_unused) => { Logger.CleanupLogQueue(); Application.Current.Shutdown(); }));
            Commands.LogOut.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.Redo.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            Commands.Undo.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));

            //zoom
            Commands.OriginalView.RegisterCommand(new DelegateCommand<object>(OriginalView, conversationSearchMustBeClosed));
            Commands.InitiateGrabZoom.RegisterCommand(new DelegateCommand<object>(App.noop, conversationSearchMustBeClosed));
            Commands.FitToView.RegisterCommand(new DelegateCommand<object>(FitToView, conversationSearchMustBeClosed));
            Commands.FitToPageWidth.RegisterCommand(new DelegateCommand<object>(FitToPageWidth));
            Commands.ExtendCanvasBothWays.RegisterCommand(new DelegateCommand<object>(App.noop, conversationSearchMustBeClosed));
            Commands.SetZoomRect.RegisterCommand(new DelegateCommand<Rect>(SetZoomRect));

            Commands.PrintConversation.RegisterCommand(new DelegateCommand<object>(PrintConversation, mustBeInConversation));

            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(ShowConversationSearchBox, mustBeLoggedIn));
            Commands.HideConversationSearchBox.RegisterCommand(new DelegateCommand<object>(HideConversationSearchBox));

            Commands.MirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeInConversation));
            Commands.ProxyMirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(ProxyMirrorPresentationSpace));

            Commands.BlockInput.RegisterCommand(new DelegateCommand<string>(BlockInput));
            Commands.UnblockInput.RegisterCommand(new DelegateCommand<object>(UnblockInput));

            Commands.BlockSearch.RegisterCommand(new DelegateCommand<object>((_unused) => BlockSearch()));
            Commands.UnblockSearch.RegisterCommand(new DelegateCommand<object>((_unused) => UnblockSearch()));

            Commands.DummyCommandToProcessCanExecute.RegisterCommand(new DelegateCommand<object>(App.noop, conversationSearchMustBeClosed));
            Commands.ImageDropped.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.ToggleNavigationLock.RegisterCommand(new DelegateCommand<object>(toggleNavigationLock));
            Commands.SetConversationPermissions.RegisterCommand(new DelegateCommand<object>(SetConversationPermissions, CanSetConversationPermissions));
            Commands.AddWindowEffect.RegisterCommand(new DelegateCommand<object>(AddWindowEffect));
            Commands.RemoveWindowEffect.RegisterCommand(new DelegateCommand<object>(RemoveWindowEffect));
            Commands.SendWakeUp.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeLoggedIn));
            Commands.ReceiveWakeUp.RegisterCommand(new DelegateCommand<object>(wakeUp));
            Commands.ReceiveSleep.RegisterCommand(new DelegateCommand<object>(sleep));

            //canvas stuff
            Commands.MoveCanvasByDelta.RegisterCommand(new DelegateCommand<Point>(GrabMove));
            Commands.AddImage.RegisterCommand(new DelegateCommand<object>(App.noop, conversationSearchMustBeClosed));
            Commands.SetTextCanvasMode.RegisterCommand(new DelegateCommand<object>(App.noop, conversationSearchMustBeClosed));
            Commands.UpdateTextStyling.RegisterCommand(new DelegateCommand<object>(App.noop, conversationSearchMustBeClosed));
            Commands.RestoreTextDefaults.RegisterCommand(new DelegateCommand<object>(App.noop, conversationSearchMustBeClosed));
            //Commands.ToggleFriendsVisibility.RegisterCommand(new DelegateCommand<object>(ToggleFriendsVisibility, conversationSearchMustBeClosed));

            Commands.SetPedagogyLevel.RegisterCommand(new DelegateCommand<PedagogyLevel>(SetPedagogyLevel, mustBeLoggedIn));
            Commands.SetLayer.ExecuteAsync("Sketch");

            Commands.AddPrivacyToggleButton.RegisterCommand(new DelegateCommand<PrivacyToggleButton.PrivacyToggleButtonInfo>(AddPrivacyButton));
            Commands.RemovePrivacyAdorners.RegisterCommand(new DelegateCommand<string>((target) => { RemovePrivacyAdorners(target); }));
            Commands.DummyCommandToProcessCanExecuteForPrivacyTools.RegisterCommand(new DelegateCommand<object>(App.noop, conversationSearchMustBeClosedAndMustBeAllowedToPublish));
            Commands.FileUpload.RegisterCommand(new DelegateCommand<object>(App.noop, mustBeAuthor));

            Commands.ChangeLanguage.RegisterCommand(new DelegateCommand<System.Windows.Markup.XmlLanguage>(changeLanguage));
            Commands.CheckExtendedDesktop.RegisterCommand(new DelegateCommand<object>((_unused) => { CheckForExtendedDesktop(); }));

            Commands.Reconnecting.RegisterCommand(new DelegateCommand<bool>(Reconnecting));
            Commands.SetUserOptions.RegisterCommand(new DelegateCommand<UserOptions>(SetUserOptions));
            Commands.SetRibbonAppearance.RegisterCommand(new DelegateCommand<RibbonAppearance>(SetRibbonAppearance));
            Commands.PresentVideo.RegisterCommand(new DelegateCommand<object>(presentVideo));
            Commands.LaunchDiagnosticWindow.RegisterCommand(new DelegateCommand<object>(launchDiagnosticWindow));
            Commands.DuplicateSlide.RegisterCommand(new DelegateCommand<object>((obj) =>
            {
                duplicateSlide((KeyValuePair<ConversationDetails, Slide>)obj);
            }, (kvp) =>
            {
                try
                {
                    return (kvp != null && ((KeyValuePair<ConversationDetails, Slide>)kvp).Key != null) ? userMayAddPage(((KeyValuePair<ConversationDetails, Slide>)kvp).Key) : false;
                }
                catch
                {
                    return false;
                }
            }));
            Commands.DuplicateConversation.RegisterCommand(new DelegateCommand<ConversationDetails>(duplicateConversation, userMayDuplicateConversation));
            Commands.CreateGrouping.RegisterCommand(new DelegateCommand<object>(createGrouping, (o) => mustBeInConversationAndBeAuthor(o)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, PrintBinding));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, HelpBinding, (_unused, e) => { e.Handled = true; e.CanExecute = true; }));
            AddWindowEffect(null);
            ribbon.Loaded += ribbon_Loaded;
            WorkspaceStateProvider.RestorePreviousSettings();
            getDefaultSystemLanguage();
            undoHistory = new UndoHistory();
            displayDispatcherTimer = createExtendedDesktopTimer();
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageUp)
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - 540);
            }
            if (e.Key == Key.PageDown)
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset + 540);
            }
        }

        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main()
        {
            // want to customise the main function so we'll create one here instead of the automatically generated one from app.g.cs
            App.ShowSplashScreen();
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
            Dispatcher.adopt(delegate
            {
                if (App.diagnosticWindow != null)
                {
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
            });
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
        private void SetRibbonAppearance(RibbonAppearance appearance)
        {
            Dispatcher.adopt(delegate
            {
                Appearance = appearance;
            });
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
            Dispatcher.adopt(delegate
            {
                //this next line should be removed.
                SaveUserOptions(options);

                if (!ribbon.IsMinimized && currentConversationSearchBox.Visibility == Visibility.Visible)
                    ribbon.ToggleMinimize();
            });
        }
        private void SaveUserOptions(UserOptions options)
        {
            //this should be wired to a new command, SaveUserOptions, which is commented out in SandRibbonInterop.Commands
            App.controller.client.SaveUserOptions(Globals.me, options);
        }
        void ribbon_Loaded(object sender, RoutedEventArgs e)
        {
            DelegateCommand<object> hideRibbon = null;
            hideRibbon = new DelegateCommand<object>((_obj) =>
            {

                Commands.SetPedagogyLevel.UnregisterCommand(hideRibbon);
                if (!ribbon.IsMinimized)
                    ribbon.ToggleMinimize();
            });
            Commands.SetPedagogyLevel.RegisterCommand(hideRibbon);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }

        private void ShowConversationSearchBox(object _arg)
        {
            Dispatcher.adopt(delegate
            {
                if (!ribbon.IsMinimized)
                    ribbon.ToggleMinimize();
            });
        }
        private void HideConversationSearchBox(object _arg)
        {
            Dispatcher.adopt(delegate
            {
                if (ribbon.IsMinimized)
                    ribbon.ToggleMinimize();
            });
        }
        private void Reconnecting(bool success)
        {
            if (success)
            {
                try
                {
                    Dispatcher.adopt(delegate
                    {
                        hideReconnectingDialog();
                    });
                    var details = Globals.conversationDetails;
                    if (details == null || details.Equals(ConversationDetails.Empty))
                    {
                        Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
                    }
                    else
                    {
                        var jid = Globals.conversationDetails.Jid;
                        Commands.UpdateConversationDetails.Execute(App.controller.client.DetailsOf(jid));
                        Commands.MoveTo.Execute(Globals.location);
                        App.controller.client.historyProvider.Retrieve<PreParser>(
                                    null,
                                    null,
                                    (parser) =>
                                    {
                                        Commands.PreParserAvailable.Execute(parser);
                                        hideReconnectingDialog();
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
                Dispatcher.adopt(delegate
                {
                    showReconnectingDialog();
                });
            }
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
        private bool LessThan(double val1, double val2, double tolerance)
        {
            var difference = val2 * tolerance;
            return val1 < (val2 - difference) && val1 < (val2 + difference);
        }
        private bool GreaterThan(double val1, double val2, double tolerance)
        {
            var difference = val2 * tolerance;
            return val1 > (val2 - difference) && val1 > (val2 + difference);
        }

        private void GetViewboxAndCanvasFromTarget(string targetName, out Viewbox viewbox, out UIElement container)
        {
            if (targetName == "presentationSpace")
            {
                viewbox = canvasViewBox;
                container = canvas;
                return;
            }
            if (targetName == "notepad")
            {
                viewbox = notesViewBox;
                container = privateNotes;
                return;
            }

            throw new ArgumentException(string.Format("Specified target {0} does not match a declared ViewBox", targetName));
        }

        private void AddPrivacyButton(PrivacyToggleButton.PrivacyToggleButtonInfo info)
        {
            Viewbox viewbox = null;
            UIElement container = null;
            GetViewboxAndCanvasFromTarget(info.AdornerTarget, out viewbox, out container);
            Dispatcher.adoptAsync(() =>
            {
                var adornerRect = new Rect(container.TranslatePoint(info.ElementBounds.TopLeft, viewbox), container.TranslatePoint(info.ElementBounds.BottomRight, viewbox));
                if (LessThan(adornerRect.Right, 0, 0.001) || GreaterThan(adornerRect.Right, viewbox.ActualWidth, 0.001)
                    || LessThan(adornerRect.Top, 0, 0.001) || GreaterThan(adornerRect.Top, viewbox.ActualHeight, 0.001))
                    return;
                var adornerLayer = AdornerLayer.GetAdornerLayer(viewbox);
                adornerLayer.Add(new UIAdorner(viewbox, new PrivacyToggleButton(info, adornerRect)));
            });
        }

        private Adorner[] GetPrivacyAdorners(Viewbox viewbox, out AdornerLayer adornerLayer)
        {
            adornerLayer = AdornerLayer.GetAdornerLayer(viewbox);
            if (adornerLayer == null)
                return null;

            return adornerLayer.GetAdorners(viewbox);
        }

        private void UpdatePrivacyAdorners(string targetName)
        {
            if (RemovePrivacyAdorners(targetName))
                try
                {
                    var lastValue = Commands.AddPrivacyToggleButton.LastValue();
                    if (lastValue != null)
                        AddPrivacyButton((PrivacyToggleButton.PrivacyToggleButtonInfo)lastValue);
                }
                catch (NotSetException) { }
        }

        private bool RemovePrivacyAdorners(string targetName)
        {
            Viewbox viewbox;
            UIElement container;
            GetViewboxAndCanvasFromTarget(targetName, out viewbox, out container);

            bool hasAdorners = false;
            AdornerLayer adornerLayer;
            var adorners = GetPrivacyAdorners(viewbox, out adornerLayer);
            Dispatcher.adopt(delegate
            {
                if (adorners != null && adorners.Count() > 0)
                {
                    hasAdorners = true;
                    foreach (var adorner in adorners)
                        adornerLayer.Remove(adorner);
                }
            });

            return hasAdorners;
        }

        private void ProxyMirrorPresentationSpace(object unused)
        {
            Commands.MirrorPresentationSpace.ExecuteAsync(this);
        }
        private void GrabMove(Point moveDelta)
        {
            Dispatcher.adopt(delegate
            {
                if (moveDelta.X != 0)
                    scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset + moveDelta.X);
                if (moveDelta.Y != 0)
                    scroll.ScrollToVerticalOffset(scroll.VerticalOffset + moveDelta.Y);
                try
                {
                    if (moveDelta.X != 0)
                    {
                        var HZoomRatio = (scroll.ExtentWidth / scroll.Width);
                        scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset + (moveDelta.X * HZoomRatio));
                    }
                    if (moveDelta.Y != 0)
                    {
                        var VZoomRatio = (scroll.ExtentHeight / scroll.Height);
                        scroll.ScrollToVerticalOffset(scroll.VerticalOffset + moveDelta.Y * VZoomRatio);
                    }
                }
                catch (Exception)
                {//out of range exceptions and the like 
                }
            });
        }
        private void BroadcastZoom()
        {
            var currentZoomHeight = scroll.ActualHeight / canvasViewBox.ActualHeight;
            var currentZoomWidth = scroll.ActualWidth / canvasViewBox.ActualWidth;
            var currentZoom = Math.Max(currentZoomHeight, currentZoomWidth);
            Commands.ZoomChanged.Execute(currentZoom);
        }
        private void BlockInput(string message)
        {
            ShowPowerpointBlocker(message);
        }
        private void UnblockInput(object _unused)
        {
            Dispatcher.adoptAsync((HideProgressBlocker));
        }

        private void BlockSearch()
        {
            Dispatcher.adopt(delegate
            {
                SearchBlocker.Visibility = Visibility.Visible;
            });
        }

        private void UnblockSearch()
        {
            Dispatcher.adopt(delegate
            {
                SearchBlocker.Visibility = Visibility.Collapsed;
            });
        }

        private void SetZoomRect(Rect viewbox)
        {
            Dispatcher.adopt(delegate
            {
                scroll.Width = viewbox.Width;
                scroll.Height = viewbox.Height;
                scroll.UpdateLayout();
                scroll.ScrollToHorizontalOffset(viewbox.X);
                scroll.ScrollToVerticalOffset(viewbox.Y);
                Trace.TraceInformation("ZoomRect changed to X:{0},Y:{1},W:{2},H:{3}", viewbox.X, viewbox.Y, viewbox.Width, viewbox.Height);
            });
        }
        private void AddWindowEffect(object _o)
        {
            CanvasBlocker.Visibility = Visibility.Visible;
        }
        private void RemoveWindowEffect(object _o)
        {
            Dispatcher.adopt(delegate
            {
                CanvasBlocker.Visibility = Visibility.Collapsed;
            });
        }

        private void EnsureConversationTabSelected()
        {
            // ensure the "pages" tab is selected
            Action selectPagesTab = () =>
                {
                    if (contentTabs.SelectedIndex != 0)
                        contentTabs.SelectedIndex = 0;
                };
            Dispatcher.Invoke(selectPagesTab, System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void ExecuteMoveTo(Location loc)
        {
            MoveTo(loc.currentSlide.id);
        }
        private void JoinConversation(ConversationDetails thisDetails)
        {
            if (thisDetails.IsEmpty) return;
            //var thisDetails = App.controller.client.DetailsOf(title);
            App.controller.client.AsyncRetrieveHistoryOf(Int32.Parse(thisDetails.Jid));
            try
            {
                Dispatcher.adopt(delegate
                {
                    EnsureConversationTabSelected();

                    if (ribbon.SelectedTab != null)
                        ribbon.SelectedTab = ribbon.Tabs[0];
                    applyPermissions(thisDetails.Permissions);
                });
                Commands.SetPrivacy.Execute(thisDetails.Author == Globals.me ? "public" : "private");
                Commands.RequerySuggested(Commands.SetConversationPermissions);
                Commands.SetLayer.ExecuteAsync("Sketch");
            }
            catch
            {
                Console.WriteLine("couldn't join conversation: " + thisDetails.Jid);
            }
        }
        private string messageFor(ConversationDetails details)
        {
            if (details.Equals(ConversationDetails.Empty))
                return Strings.Global_ProductName;
            return string.Format("Collaboration {0}  -  {1}'s \"{2}\" - MeTL", details.Permissions.studentCanWorkPublicly ? "ENABLED" : "DISABLED", details.Author, details.Title);
        }
        private void MoveTo(int slide)
        {
            Dispatcher.adoptAsync(delegate
                                     {
                                         if (canvas.Visibility == Visibility.Collapsed)
                                             canvas.Visibility = Visibility.Visible;
                                         scroll.Width = Double.NaN;
                                         scroll.Height = Double.NaN;
                                         canvas.Width = Double.NaN;
                                         canvas.Height = Double.NaN;
                                     });
            CommandManager.InvalidateRequerySuggested();
        }
        private void hideReconnectingDialog()
        {
            ProgressDisplay.Children.Clear();
            InputBlocker.Visibility = Visibility.Collapsed;
        }
        private void showReconnectingDialog()
        {
            ProgressDisplay.Children.Clear();
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
            ProgressDisplay.Children.Add(majorHeading);
            ProgressDisplay.Children.Add(minorHeading);
            InputBlocker.Visibility = Visibility.Visible;
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
        private bool conversationSearchMustBeClosed(object _obj)
        {
            return currentConversationSearchBox.Visibility == Visibility.Collapsed && mustBeInConversation(null);
        }
        private bool conversationSearchMustBeClosedAndMustBeAllowedToPublish(object _obj)
        {
            if (conversationSearchMustBeClosed(null))
                return Globals.isAuthor || Globals.conversationDetails.Permissions.studentCanWorkPublicly;
            else return false;
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

        private void duplicateSlide(KeyValuePair<ConversationDetails, Slide> _kvp)
        {
            var kvp = new KeyValuePair<ConversationDetails, Slide>(Globals.conversationDetails, Globals.slideDetails);
            if (kvp.Key.UserHasPermission(Globals.credentials) && kvp.Key.Slides.Exists(s => s.id == kvp.Value.id))
            {
                App.controller.client.DuplicateSlide(kvp.Key, kvp.Value);
            }
        }
        private void duplicateConversation(ConversationDetails conversationToDuplicate)
        {
            if (conversationToDuplicate.UserHasPermission(Globals.credentials))
            {
                App.controller.client.DuplicateConversation(conversationToDuplicate);
            }
        }
        private bool userMayDuplicateConversation(ConversationDetails conversationToDuplicate)
        {
            return conversationToDuplicate.UserHasPermission(Globals.credentials);
        }
        private bool userMayAddPage(ConversationDetails _conversation)
        {
            var conversation = Globals.conversationDetails;
            if (conversation == null)
            {
                return false;
            }
            return conversation.UserHasPermission(Globals.credentials);
        }
        private void createGrouping(object groupingDefinition)
        {
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (details.IsEmpty) return;

            if (details.Jid.GetHashCode() == Globals.location.activeConversation.GetHashCode() || Globals.location.activeConversation.IsEmpty)
            {
                Dispatcher.adopt(delegate
                {
                    UpdateTitle(details);
                });
                if (!mustBeInConversation(null))
                {
                    Dispatcher.adopt(delegate
                    {
                        ShowConversationSearchBox(null);
                    });
                    Commands.LeaveLocation.Execute(null); 
                }
            }
            Dispatcher.adopt(delegate
            {
                if (details.isAuthor(Globals.me))
                {
                    ParticipantsTabItem.Visibility = Visibility.Visible;
                }
                else
                {
                    ParticipantsTabItem.Visibility = Visibility.Collapsed;
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
        private void applyPermissions(Permissions permissions)
        {
            if (canOpenFriendsOverride != null)
                Commands.ToggleFriendsVisibility.UnregisterCommand(canOpenFriendsOverride);
            canOpenFriendsOverride = new DelegateCommand<object>((_param) => { }, (_param) => true);
            Commands.ToggleFriendsVisibility.RegisterCommand(canOpenFriendsOverride);
        }
        private void showPowerPointProgress(string progress)
        {
            var text = new TextBlock
            {
                Foreground = Brushes.WhiteSmoke,
                Text = progress
            };
            ProgressDisplay.Children.Add(text);
        }
        private void createConversation(object detailsObject)
        {
            EnsureConversationTabSelected();

            var details = (ConversationDetails)detailsObject;
            if (details == null) return;
            if (Commands.CreateConversation.CanExecute(details))
            {
                if (details.Tag == null)
                    details.Tag = "unTagged";
                details.Author = Globals.userInformation.credentials.name;
                var connection = App.controller.client;
                details = connection.CreateConversation(details);
                connection.JoinConversation(details);
                CommandManager.InvalidateRequerySuggested();
                if (Commands.JoinConversation.CanExecute(details))
                {
                    Commands.JoinConversation.Execute(details);
                    Commands.UpdateConversationDetails.Execute(details);
                }
            }
        }
        private void OriginalView(object _unused)
        {
            Trace.TraceInformation("ZoomToOriginalView");
            var currentSlide = Globals.conversationDetails.Slides.Where(s => s.id == Globals.slide).FirstOrDefault();
            if (currentSlide == null || currentSlide.defaultHeight == 0 || currentSlide.defaultWidth == 0) return;
            scroll.Width = currentSlide.defaultWidth;
            scroll.Height = currentSlide.defaultHeight;
            if (canvas != null && canvas.stack != null && !Double.IsNaN(canvas.stack.offsetX) && !Double.IsNaN(canvas.stack.offsetY))
            {
                scroll.ScrollToHorizontalOffset(Math.Min(scroll.ExtentWidth, Math.Max(0, -canvas.stack.offsetX)));
                scroll.ScrollToVerticalOffset(Math.Min(scroll.ExtentHeight, Math.Max(0, -canvas.stack.offsetY)));
            }
            else
            {
                scroll.ScrollToLeftEnd();
                scroll.ScrollToTop();
            }
            checkZoom();
        }
        private void FitToView(object _unused)
        {
            scroll.Height = double.NaN;
            scroll.Width = double.NaN;
            canvas.Height = double.NaN;
            canvas.Width = double.NaN;
            checkZoom();
        }
        private void FitToPageWidth(object _unused)
        {
            if (scroll != null)
            {
                var ratio = adornerGrid.ActualWidth / adornerGrid.ActualHeight;
                scroll.Height = canvas.ActualWidth / ratio;
                scroll.Width = canvas.ActualWidth;
            }
            checkZoom();
        }
        private void ShowPowerpointBlocker(string explanation)
        {
            Dispatcher.adoptAsync(() =>
            {
                showPowerPointProgress(explanation);
                InputBlocker.Visibility = Visibility.Visible;
            });
        }
        private void HideProgressBlocker()
        {
            ProgressDisplay.Children.Clear();
            InputBlocker.Visibility = Visibility.Collapsed;
        }
        private bool canZoomIn(object sender)
        {
            return !(scroll == null) && mustBeInConversation(null) && conversationSearchMustBeClosed(null);
        }
        private bool canZoomOut(object sender)
        {
            if (scroll == null)
            {
                return false;
            }
            else
            {
                var cvHeight = adornerGrid.ActualHeight;
                var cvWidth = adornerGrid.ActualWidth;
                var cvRatio = cvWidth / cvHeight;
                bool hTrue = scroll.ViewportWidth < scroll.ExtentWidth;
                bool vTrue = scroll.ViewportHeight < scroll.ExtentHeight;
                return (hTrue || vTrue) && mustBeInConversation(null) && conversationSearchMustBeClosed(null);
            }
        }

        private void doZoomIn(object sender)
        {
            Trace.TraceInformation("ZoomIn pressed");
            var ZoomValue = 0.9;
            var scrollHOffset = scroll.HorizontalOffset;
            var scrollVOffset = scroll.VerticalOffset;
            var cvHeight = adornerGrid.ActualHeight;
            var cvWidth = adornerGrid.ActualWidth;
            var cvRatio = cvWidth / cvHeight;
            double newWidth = 0;
            double newHeight = 0;
            double oldWidth = scroll.ActualWidth;
            double oldHeight = scroll.ActualHeight;
            var scrollRatio = oldWidth / oldHeight;
            if (scrollRatio > cvRatio)
            {
                newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
                newHeight = newWidth / cvRatio;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
            }
            if (scrollRatio < cvRatio)
            {
                newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                newWidth = newHeight * cvRatio;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            if (scrollRatio == cvRatio)
            {
                newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            scroll.ScrollToHorizontalOffset(scrollHOffset + ((oldWidth - newWidth) / 2));
            scroll.ScrollToVerticalOffset(scrollVOffset + ((oldHeight - newHeight) / 2));
            checkZoom();
        }
        private void doZoomOut(object sender)
        {
            Trace.TraceInformation("ZoomOut pressed");
            var ZoomValue = 1.1;
            var scrollHOffset = scroll.HorizontalOffset;
            var scrollVOffset = scroll.VerticalOffset;
            var cvHeight = adornerGrid.ActualHeight;
            var cvWidth = adornerGrid.ActualWidth;
            var cvRatio = cvWidth / cvHeight;
            var scrollRatio = scroll.ActualWidth / scroll.ActualHeight;
            double newWidth = 0;
            double newHeight = 0;
            double oldWidth = scroll.ActualWidth;
            double oldHeight = scroll.ActualHeight;
            if (scrollRatio > cvRatio)
            {
                newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
                newHeight = newWidth / cvRatio;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
            }
            if (scrollRatio < cvRatio)
            {
                newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                newWidth = newHeight * cvRatio;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            if (scrollRatio == cvRatio)
            {
                newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            scroll.ScrollToHorizontalOffset(scrollHOffset + ((oldWidth - newWidth) / 2));
            scroll.ScrollToVerticalOffset(scrollVOffset + ((oldHeight - newHeight) / 2));
            checkZoom();
        }
        private void checkZoom()
        {
            Commands.RequerySuggested(Commands.ZoomIn, Commands.ZoomOut);
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
        public void SetPedagogyLevel(PedagogyLevel level)
        {
            SetupUI(level);
        }
        public void ClearUI()
        {
            Commands.UnregisterAllCommands();
            Dispatcher.adoptAsync(() =>
            {
                foreach (RibbonTab tab in ribbon.Tabs)
                {
                    tab.Items.Clear();
                }
                ribbon.Tabs.Clear();
                privacyTools.Children.Clear();
                RHSDrawerDefinition.Width = new GridLength(0);
            });
        }

        public void SetupUI(PedagogyLevel level)
        {
            Dispatcher.adopt(() =>
            {
                ClearUI();
                var editingOptions = new EditingOptions();
                Commands.SaveUIState.Execute(null);
                List<FrameworkElement> homeGroups = new List<FrameworkElement>();
                List<FrameworkElement> tabs = new List<FrameworkElement>();
                foreach (var i in Enumerable.Range(0, ((int)level.code) + 1))
                {
                    switch (i)
                    {
                        case 0:
                            homeGroups.Add(editingOptions);
                            break;
                        case 1:
                            tabs.Add(new Tabs.Quizzes());
                            tabs.Add(new Tabs.Submissions());
                            tabs.Add(new Tabs.Attachments());
                            homeGroups.Add(new EditingModes());
                            break;
                        case 2:
                            tabs.Add(new Tabs.ConversationManagement());
                            RHSDrawerDefinition.Width = new GridLength(180);
                            homeGroups.Add(new ZoomControlsHost());
                            homeGroups.Add(new MiniMap());
#if TOGGLE_CONTENT
                            homeGroups.Add(new ContentVisibilityHost());
#endif
                            break;
                        case 3:
                            homeGroups.Add(new CollaborationControlsHost());
                            homeGroups.Add(new PrivacyToolsHost());
                            break;
                        case 4:
                            homeGroups.Add(new Notes());
                            tabs.Add(new Tabs.Analytics());
                            break;
                        default:
                            break;
                    }
                }
                var home = new Tabs.Home { DataContext = scroll };
                homeGroups.Sort(new PreferredDisplayIndexComparer());
                foreach (var group in homeGroups)
                    home.Items.Add((RibbonGroup)group);
                tabs.Add(home);
                tabs.Sort(new PreferredDisplayIndexComparer());
                foreach (var tab in tabs)
                    ribbon.Tabs.Add((RibbonTab)tab);
                ribbon.SelectedTab = home;
                if (!ribbon.IsMinimized && currentConversationSearchBox.Visibility == Visibility.Visible)
                    ribbon.ToggleMinimize();

            });
            Commands.RestoreUIState.Execute(null);
            CommandManager.InvalidateRequerySuggested();
            Commands.RequerySuggested();
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
        private void presentVideo(object _arg)
        {
            Dispatcher.adopt(delegate
            {
                var chooseVideo = new OpenFileDialog();
                var result = chooseVideo.ShowDialog(Window.GetWindow(this));
                if (result == true)
                {
                    var popup = new Window();
                    var sp = new StackPanel();
                    var player = new MediaElement
                    {
                        Source = new Uri(chooseVideo.FileName),
                        LoadedBehavior = MediaState.Manual
                    };
                    player.Play();
                    var buttons = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };
                    bool isPaused = false;
                    player.MouseLeftButtonUp += delegate
                    {
                        if (isPaused)
                            player.Play();
                        else
                            player.Pause();
                    };
                    Func<Func<Point>, RoutedEventHandler> handler = point =>
                    {
                        return delegate
                        {
                            var location = String.Format("{0}{1}.jpg", System.IO.Path.GetTempPath(), DateTime.Now.Ticks);
                            if (player.HasVideo)
                            {
                                var width = Convert.ToInt32(player.ActualWidth);
                                var height = Convert.ToInt32(player.ActualHeight);
                                var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                                rtb.Render(player);
                                var encoder = new JpegBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(rtb));
                                using (var fs = new FileStream(location, FileMode.CreateNew))
                                {
                                    encoder.Save(fs);
                                }
                                Commands.ImageDropped.Execute(new ImageDrop
                                {
                                    Filename = location,
                                    Point = point(),
                                    Position = 1,
                                    Target = "presentationSpace"
                                });
                            }
                        };
                    };
                    var under = new SandRibbon.Components.ResourceDictionaries.Button
                    {
                        Text = "Drop"
                    };
                    under.Click += handler(() => popup.TranslatePoint(new Point(0, 0), canvasViewBox));
                    var fullScreen = new SandRibbon.Components.ResourceDictionaries.Button
                    {
                        Text = "Fill"
                    };
                    fullScreen.Click += handler(() => new Point(0, 0));
                    buttons.Children.Add(under);
                    buttons.Children.Add(fullScreen);
                    sp.Children.Add(player);
                    sp.Children.Add(buttons);
                    popup.Content = sp;
                    popup.Topmost = true;
                    popup.Owner = Window.GetWindow(this);
                    popup.Show();
                }
            });
        }
        private void zoomConcernedControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePrivacyAdorners(adornerScroll.Target);
            BroadcastZoom();
        }

        private void scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdatePrivacyAdorners(adornerScroll.Target);
            BroadcastZoom();
        }

        private void notepadSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePrivacyAdorners(notesAdornerScroll.Target);
            //BroadcastZoom();
        }

        private void notepadScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdatePrivacyAdorners(notesAdornerScroll.Target);
            //BroadcastZoom();
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
