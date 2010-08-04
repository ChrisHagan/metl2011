using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Divelements.SandRibbon;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Components.SimpleImpl;
using SandRibbon.Providers;
using SandRibbon.Providers.Structure;
using SandRibbon.Quizzing;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonObjects;
using System.Diagnostics;
using System.Windows.Shapes;
using SandRibbon.Components.Sandpit;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Tabs;
using SandRibbon.Tabs.Groups;
using System.Collections;
using System.Windows.Ink;
using System.Collections.ObjectModel;

namespace SandRibbon
{
    public partial class Window1
    {
        public readonly string RECENT_DOCUMENTS = "recentDocuments.xml";
        #region SurroundingServers
        #endregion
        public JabberWire.UserInformation userInformation = new JabberWire.UserInformation();
        private DelegateCommand<string> powerpointProgress;
        private DelegateCommand<string> powerPointFinished;
        private PowerPointLoader loader = new PowerPointLoader();
        private UndoHistory history = new UndoHistory();
        public ConversationDetails details;
        private JabberWire wire;
        public string CurrentProgress { get; set; }
        public static RoutedCommand ProxyMirrorExtendedDesktop = new RoutedCommand();
        public static ScrollViewer MAIN_SCROLL;
        public string log
        {
            get { return Logger.log; }
        }
        public Window1()
        {
            App.Now("Window 1 Constructor start");
            ProviderMonitor.HealthCheck(DoConstructor);
        }
        private void DoConstructor()
        {
            InitializeComponent();
            userInformation.policy = new JabberWire.Policy { isSynced = false, isAuthor = false };
            Commands.ChangeTab.RegisterCommand(new DelegateCommand<string>(ChangeTab));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(SetIdentity));
            Commands.PowerPointLoadFinished.RegisterCommand(new DelegateCommand<object>(PowerPointLoadFinished));
            Commands.PowerPointProgress.RegisterCommand(new DelegateCommand<string>(PowerPointProgress));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(ExecuteMoveTo, CanExecuteMoveTo));
            Commands.ClearDynamicContent.RegisterCommand(new DelegateCommand<object>(ClearDynamicContent));
            Commands.LogOut.RegisterCommand(new DelegateCommand<object>(noop, mustBeLoggedIn));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation, mustBeLoggedIn));
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<object>(createConversation, mustBeLoggedIn));
            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(noop, mustBeLoggedIn));
            Commands.PrintConversation.RegisterCommand(new DelegateCommand<object>(noop, mustBeInConversation));
            Commands.PrintConversationHandout.RegisterCommand(new DelegateCommand<object>(noop, mustBeInConversation));
            Commands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(ImportPowerPoint, mustBeLoggedIn));
            Commands.StartPowerPointLoad.RegisterCommand(new DelegateCommand<object>(StartPowerPointLoad, mustBeLoggedIn));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.PreShowPrintConversationDialog.RegisterCommand(new DelegateCommand<object>(ShowPrintConversationDialog));
            Commands.PreCreateConversation.RegisterCommand(new DelegateCommand<object>(CreateConversation));
            Commands.PreEditConversation.RegisterCommand(new DelegateCommand<object>(EditConversation, mustBeInConversation));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(setSync));
            Commands.SetInkCanvasMode.RegisterCommand(new DelegateCommand<object>(SetInkCanvasMode, mustBeInConversation));
            Commands.ToggleScratchPadVisibility.RegisterCommand(new DelegateCommand<object>(noop, mustBeLoggedIn));
            Commands.SetLayer.RegisterCommand(new DelegateCommand<object>(noop, mustBeInConversation));
            Commands.ImageDropped.RegisterCommand(new DelegateCommand<object>(noop, mustBeLoggedIn));
            Commands.SetTutorialVisibility.RegisterCommand(new DelegateCommand<object>(SetTutorialVisibility, mustBeInConversation));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<object>(noop, mustBeLoggedIn));
            Commands.Relogin.RegisterCommand(new DelegateCommand<object>(Relogin));
            Commands.MirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(noop, mustBeInConversation));
            Commands.ProxyMirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(ProxyMirrorPresentationSpace));
            Commands.ReceiveWormMove.RegisterCommand(new DelegateCommand<string>(ReceiveWormMove));
            Commands.SetConversationPermissions.RegisterCommand(new DelegateCommand<object>(SetConversationPermissions, CanSetConversationPermissions));
            Commands.AddWindowEffect.RegisterCommand(new DelegateCommand<object>(AddWindowEffect));
            Commands.RemoveWindowEffect.RegisterCommand(new DelegateCommand<object>(RemoveWindowEffect));
            Commands.SendWakeUp.RegisterCommand(new DelegateCommand<object>(noop, mustBeLoggedIn));
            Commands.ReceiveWakeUp.RegisterCommand(new DelegateCommand<object>(wakeUp));
            Commands.ReceiveSleep.RegisterCommand(new DelegateCommand<object>(sleep));
            Commands.FitToView.RegisterCommand(new DelegateCommand<object>(FitToView));
            Commands.OriginalView.RegisterCommand(new DelegateCommand<object>(OriginalView));
            Commands.FitToPageWidth.RegisterCommand(new DelegateCommand<object>(FitToPageWidth));
            Commands.SetZoomRect.RegisterCommand(new DelegateCommand<Rectangle>(SetZoomRect));
            Commands.ChangePenSize.RegisterCommand(new DelegateCommand<object>(AdjustPenSizeAccordingToZoom));
            Commands.SetDrawingAttributes.RegisterCommand(new DelegateCommand<object>(AdjustDrawingAttributesAccordingToZoom));
            Commands.ActualReportDrawingAttributes.RegisterCommand(new DelegateCommand<object>(AdjustReportedDrawingAttributesAccordingToZoom));
            Commands.ActualReportStrokeAttributes.RegisterCommand(new DelegateCommand<object>(AdjustReportedStrokeAttributesAccordingToZoom));
            Commands.SetPedagogyLevel.RegisterCommand(new DelegateCommand<PedagogyLevel>(SetPedagogyLevel, mustBeLoggedIn));
            Commands.ShowEditSlidesDialog.RegisterCommand(new DelegateCommand<object>(ShowEditSlidesDialog, mustBeInConversation));
            Commands.SetLayer.Execute("Sketch");
            Commands.MoveCanvasByDelta.RegisterCommand(new DelegateCommand<Point>(GrabMove));
            Commands.BlockInput.RegisterCommand(new DelegateCommand<string>(BlockInput));
            Commands.UnblockInput.RegisterCommand(new DelegateCommand<object>(UnblockInput));
            adornerScroll.scroll = scroll;
            adornerScroll.scroll.SizeChanged += adornerScroll.scrollChanged;
            adornerScroll.scroll.ScrollChanged += adornerScroll.scroll_ScrollChanged;
            AddWindowEffect(null);
            if (SmartBoardMeTLAlreadyLoaded)
                checkIfSmartboard();
            App.Now("Restoring settings");
            WorkspaceStateProvider.RestorePreviousSettings();
            App.Now("Started MeTL");
        }
        private void noop(object unused) { }
        private void ShowEditSlidesDialog(object unused)
        {
            new SlidesEditingDialog().ShowDialog();
        }
        private void SetInkCanvasMode(object unused)
        {
            setLayer("Sketch");
        }
        private void ProxyMirrorPresentationSpace(object unused)
        {
            Commands.MirrorPresentationSpace.Execute(this);
        }
        private void PowerPointLoadFinished(object unused)
        {
            Dispatcher.adoptAsync((finishedPowerpoint));
        }
        private void GrabMove(Point moveDelta)
        {
            if (moveDelta.X != 0)
                scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset + moveDelta.X);
            if (moveDelta.Y != 0)
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset + moveDelta.Y);
        }
        private void PowerPointProgress(string progress)
        {
            Dispatcher.adoptAsync(delegate
            {
                showPowerPointProgress(progress);
            });
        }
        private void ChangeTab(string which)
        {
            foreach (var tab in ribbon.Tabs)
                if (((RibbonTab)tab).Text == which)
                    ribbon.SelectedTab = (RibbonTab)tab;
        }
        private void StartPowerPointLoad(object unused)
        {
            ShowPowerpointBlocker("Starting Powerpoint Import");
            Commands.PostStartPowerPointLoad.Execute(null);
        }
        private void ImportPowerPoint(object unused)
        {
            ShowPowerpointBlocker("Starting Powerpoint Import");
            Commands.PostImportPowerpoint.Execute(null);
        }
        private void AdjustReportedDrawingAttributesAccordingToZoom(object attributes)
        {
            var zoomIndependentAttributes = ((DrawingAttributes)attributes).Clone();
            var currentZoomHeight = scroll.ActualHeight / canvasViewBox.ActualHeight;
            var currentZoomWidth = scroll.ActualWidth / canvasViewBox.ActualWidth;
            var currentZoom = Math.Max(currentZoomHeight, currentZoomWidth);
            var desiredZoom = zoomIndependentAttributes.Height / currentZoom;
            zoomIndependentAttributes.Height = correctZoom(desiredZoom);
            zoomIndependentAttributes.Width = correctZoom(desiredZoom);
            Commands.ReportDrawingAttributes.Execute(zoomIndependentAttributes);
        }
        private void AdjustReportedStrokeAttributesAccordingToZoom(object attributes)
        {
            var zoomIndependentAttributes = ((DrawingAttributes)attributes).Clone();
            var currentZoomHeight = scroll.ActualHeight / canvasViewBox.ActualHeight;
            var currentZoomWidth = scroll.ActualWidth / canvasViewBox.ActualWidth;
            var currentZoom = Math.Max(currentZoomHeight, currentZoomWidth);
            var desiredZoom = zoomIndependentAttributes.Height / currentZoom;
            zoomIndependentAttributes.Height = correctZoom(desiredZoom);
            zoomIndependentAttributes.Width = correctZoom(desiredZoom);
            Commands.ReportStrokeAttributes.Execute(zoomIndependentAttributes);
        }
        private double correctZoom(double desiredZoom)
        {
            double finalZoomWidth;
            double finalZoomHeight;
            if ((desiredZoom > DrawingAttributes.MinHeight) && (desiredZoom < DrawingAttributes.MaxHeight))
                finalZoomHeight = desiredZoom;
            else if (desiredZoom < DrawingAttributes.MaxHeight)
                finalZoomHeight = DrawingAttributes.MinHeight;
            else finalZoomHeight = DrawingAttributes.MaxHeight;
            if (desiredZoom > DrawingAttributes.MinWidth)
                finalZoomWidth = desiredZoom;
            else if (desiredZoom < DrawingAttributes.MaxWidth)
                finalZoomWidth = DrawingAttributes.MinWidth;
            else finalZoomWidth = DrawingAttributes.MaxWidth;
            if (Math.Max(finalZoomHeight, finalZoomWidth) == Math.Max(DrawingAttributes.MaxHeight, DrawingAttributes.MaxWidth))
                return Math.Min(finalZoomHeight, finalZoomWidth);
            else return Math.Max(finalZoomHeight, finalZoomWidth);
        }
        private void AdjustPenSizeAccordingToZoom(object pensize)
        {
            var zoomCorrectPenSize = ((double)pensize);
            var currentZoomHeight = scroll.ActualHeight / canvasViewBox.ActualHeight;
            var currentZoomWidth = scroll.ActualWidth / canvasViewBox.ActualWidth;
            var currentZoom = Math.Max(currentZoomHeight, currentZoomWidth);
            var desiredZoom = zoomCorrectPenSize * currentZoom;
            Commands.ActualChangePenSize.Execute(correctZoom(desiredZoom));
        }
        private void AdjustDrawingAttributesAccordingToZoom(object attributes)
        {
            var zoomCorrectAttributes = ((DrawingAttributes)attributes).Clone();
            var currentZoomHeight = scroll.ActualHeight / canvasViewBox.ActualHeight;
            var currentZoomWidth = scroll.ActualWidth / canvasViewBox.ActualWidth;
            var currentZoom = Math.Max(currentZoomHeight, currentZoomWidth);
            var desiredZoom = zoomCorrectAttributes.Height * currentZoom;
            zoomCorrectAttributes.Width = correctZoom(desiredZoom);
            zoomCorrectAttributes.Height = correctZoom(desiredZoom);
            Commands.ActualSetDrawingAttributes.Execute(zoomCorrectAttributes);
        }
        private void SetTutorialVisibility(object visibilityObject)
        {
            var newVisibility = (Visibility)visibilityObject;
            switch (newVisibility)
            {
                case Visibility.Visible:
                    ShowTutorial();
                    break;
                default:
                    HideTutorial();
                    break;
            }
        }
        private void ShowPrintConversationDialog(object _unused)
        {
            ShowPowerpointBlocker("Printing Dialog Open");
            Commands.ShowPrintConversationDialog.Execute(null);
        }
        private void CreateConversation(object _unused)
        {
            ShowPowerpointBlocker("Creating Conversation Dialog Open");
            Commands.CreateConversationDialog.Execute(null);
        }
        private void EditConversation(object _unused)
        {
            ShowPowerpointBlocker("Editing Conversation Dialog Open");
            Commands.EditConversation.Execute(null);
        }
        private void BlockInput(string message)
        {
            ShowPowerpointBlocker(message);
        }
        private void UnblockInput(object _unused)
        {
            Dispatcher.adoptAsync((finishedPowerpoint));
        }
        private void SetIdentity(SandRibbon.Utils.Connection.JabberWire.Credentials credentials)
        {
            connect(credentials.name, credentials.password, 0, null);
            var conversations = ConversationDetailsProviderFactory.Provider.ListConversations();
            Commands.AllStaticCommandsAreRegistered();
            Commands.RequerySuggested();
            Pedagogicometer.SetPedagogyLevel(Globals.pedagogy);
        }
        private void SetZoomRect(Rectangle viewbox)
        {
            var topleft = new Point(Canvas.GetLeft(viewbox), Canvas.GetTop(viewbox));
            scroll.Width = viewbox.Width;
            scroll.Height = viewbox.Height;
            scroll.ScrollToHorizontalOffset(topleft.X);
            scroll.ScrollToVerticalOffset(topleft.Y);
        }
        private bool SmartBoardMeTLAlreadyLoaded
        {
            get
            {
                var thisProcess = Process.GetCurrentProcess();
                return Process.GetProcessesByName("MeTL").Any(p =>
                    p.Id != thisProcess.Id &&
                    (p.MainWindowTitle.StartsWith("S15") || p.MainWindowTitle.Equals("")));
            }
        }
        public void checkIfSmartboard()
        {
            var path = "C:\\Program Files\\MeTL\\boardIdentity.txt";
            if (!File.Exists(path)) return;
            var myFile = new StreamReader(path);
            var username = myFile.ReadToEnd();
            MessageBox.Show("Logging in as {0}", username);
            JabberWire.SwitchServer("staging");
            Dispatcher.adoptAsync(delegate
            {
                Title = username + " MeTL waiting for wakeup";
            });
            Commands.SetIdentity.Execute(new JabberWire.Credentials { authorizedGroups = new List<JabberWire.AuthorizedGroup>(), name = username, password = "examplePassword" });
        }
        private static object reconnectionLock = new object();
        private static bool reconnecting = false;
        private void AddWindowEffect(object _o)
        {
            CanvasBlocker.Visibility = Visibility.Visible;

        }
        private void RemoveWindowEffect(object _o)
        {
            CanvasBlocker.Visibility = Visibility.Collapsed;
        }
        private void ReceiveWormMove(string _move)
        {
            if (!reconnecting) return;
            lock (reconnectionLock)
            {
                Logger.Log("Window1.ReceiveWormMove: Window 1 initiating move in reconnect");
                Dispatcher.adopt((Action)delegate
                {
                    try
                    {
                        if (InputBlocker.Visibility == Visibility.Visible)
                        {
                            InputBlocker.Visibility = Visibility.Collapsed;
                            reconnecting = false;
                            wire.JoinConversation(userInformation.location.activeConversation);
                            Commands.MoveTo.Execute(userInformation.location.currentSlide);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log(string.Format("Receive worm move problem in Window1: {0}", e.Message));
                    }
                });
            }
        }
        private void ExecuteMoveTo(int slide)
        {
            ProviderMonitor.HealthCheck(() => MoveTo(slide));
        }
        private bool CanExecuteMoveTo(int slide)
        {
            return mustBeInConversation(slide);
        }
        private void JoinConversation(string title)
        {
            ProviderMonitor.HealthCheck(() =>
            {
                Commands.LoggedIn.Execute(userInformation.credentials.name);
                var details = ConversationDetailsProviderFactory.Provider.DetailsOf(userInformation.location.activeConversation);
                RecentConversationProvider.addRecentConversation(details, Globals.me);
                if (details.Author == Globals.me)
                    Commands.SetPrivacy.Execute("public");
                else
                    Commands.SetPrivacy.Execute("private");
                applyPermissions(details.Permissions);
                Commands.UpdateConversationDetails.Execute(details);
                Logger.Log("Joined conversation " + title);
                Commands.RequerySuggested(Commands.SetConversationPermissions);
                if (automatedTest(details.Title))
                    ribbon.SelectedTab = ribbon.Tabs[1];
            });
        }

        private bool automatedTest(string conversationName)
        {
            if (Globals.me.Contains("Admirable") && conversationName.ToLower().Contains("automated")) return true;
            return false;
        }
        private string messageFor(ConversationDetails details)
        {
            var permissionLabel = Permissions.InferredTypeOf(details.Permissions).Label;
            return string.Format("{3} is in {0}'s \"{1}\", currently in {2} style", details.Author, details.Title, permissionLabel, userInformation.credentials.name);
        }
        private void ClearDynamicContent(object obj)
        {
        }
        private void MoveTo(int slide)
        {
            if ((userInformation.policy.isAuthor && userInformation.policy.isSynced) || (Globals.synched && userInformation.policy.isAuthor))
                Commands.SendSyncMove.Execute(slide);
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


        private void Relogin(object obj)
        {
            lock (reconnectionLock)
            {
                showReconnectingDialog();
                reconnecting = true;
                connect(userInformation.credentials.name, userInformation.credentials.password, userInformation.location.currentSlide, userInformation.location.activeConversation);
            }
        }
        private void showReconnectingDialog()
        {
            if (InputBlocker.Visibility == Visibility.Visible) return;
            ProgressDisplay.Children.Clear();
            var sp = new StackPanel();
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
            sp.Children.Add(majorHeading);
            sp.Children.Add(minorHeading);
            ProgressDisplay.Children.Add(sp);
            InputBlocker.Visibility = Visibility.Visible;
        }
        private void moveToQuiz(QuizQuestion quiz)
        {
        }

        private void ShowTutorial()
        {
            TutorialLayer.Visibility = Visibility.Visible;
        }
        private void HideTutorial()
        {
            if (userInformation.location != null && !String.IsNullOrEmpty(userInformation.location.activeConversation))
                TutorialLayer.Visibility = Visibility.Collapsed;
        }

        private bool mustBeLoggedIn(object _arg)
        {
            try
            {
                return Globals.credentials != null;
            }
            catch (NotSetException)
            {
                return false;
            }
        }
        private bool mustBeInConversation(object _arg)
        {
            try
            {
                return Globals.location.activeConversation != null;
            }
            catch (NotSetException)
            {
                return false;
            }
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            try
            {
                if (details.Jid != Globals.location.activeConversation) return;
            }
            catch (NotSetException)
            {
                //We're not anywhere yet so update away
            }
            Dispatcher.adoptAsync(delegate
            {
                userInformation.location.availableSlides = details.Slides.Select(s => s.id).ToList();
                HideTutorial();
                UpdateTitle();
                var isAuthor = (details.Author != null) && details.Author == userInformation.credentials.name;
                userInformation.policy.isAuthor = isAuthor;
                Commands.RequerySuggested();
            });
        }
        private void UpdateTitle()
        {
            try
            {
                Title = messageFor(Globals.conversationDetails);
            }
            catch (NotSetException) { }
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
        private void finishedPowerpoint()
        {
            ProgressDisplay.Children.Clear();
            InputBlocker.Visibility = Visibility.Collapsed;
            HideProgressBlocker();
        }
        private void connect(string username, string pass, int location, string conversation)
        {
            if (wire == null)
            {
                userInformation.location = new JabberWire.Location { currentSlide = location, activeConversation = conversation };
                userInformation.credentials = new JabberWire.Credentials { name = username, password = pass };
                wire = new JabberWire(userInformation.credentials);
                wire.Login(userInformation.location);
            }
            else
            {
                userInformation.location.activeConversation = conversation;
                userInformation.location.currentSlide = location;
                wire.Reset("Window1");
            }
            loader.wire = wire;
        }
        private void createConversation(object detailsObject)
        {
            var details = (ConversationDetails)detailsObject;
            if (details == null) return;
            if (Commands.CreateConversation.CanExecute(details))
            {
                if (details.Tag == null)
                    details.Tag = "unTagged";
                details.Author = userInformation.credentials.name;
                details = ConversationDetailsProviderFactory.Provider.Create(details);
                CommandManager.InvalidateRequerySuggested();
                if (Commands.JoinConversation.CanExecute(details.Jid))
                    Commands.JoinConversation.Execute(details.Jid);
            }
        }
        private void debugTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            Console.WriteLine(sender.GetType());
        }
        private void closeApplication(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
        private void setLayer(string layer)
        {
            Commands.SetLayer.Execute(layer);
        }
        private void receivedHistoryPortion(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.PostRetrievedHistoryPortion.Execute(e.Parameter);
        }
        private static void SetVisibilityOf(UIElement target, Visibility visibility)
        {
            target.Visibility = visibility;
        }
        private void setSync(object _obj)
        {
            userInformation.policy.isSynced = !userInformation.policy.isSynced;
        }
        private void progressReported(object sender, ExecutedRoutedEventArgs e)
        {
            var report = (int[])e.Parameter;
            progress.Show(report[0], report[1]);
        }
        private void OriginalView(object _unused)
        {
            var currentSlide = Globals.conversationDetails.Slides.Where(s => s.id == Globals.slide).First();
            if(currentSlide.defaultHeight == 0 || currentSlide.defaultWidth == 0) return;
            scroll.Width = currentSlide.defaultWidth;
            scroll.Height = currentSlide.defaultHeight;
            scroll.ScrollToLeftEnd();
            scroll.ScrollToTop();
        }
        private void FitToView(object _unused)
        {
            if (scroll != null && scroll.Height > 0 && scroll.Width > 0)
            {
                scroll.Height = scroll.ExtentHeight;
                scroll.Width = scroll.ExtentWidth;
                scroll.Height = double.NaN;
                scroll.Width = double.NaN;
            }
        }
        private void FitToPageWidth(object _unused)
        {
            if (scroll != null)
            {
                var ratio = adornerGrid.ActualWidth / adornerGrid.ActualHeight;
                scroll.Height = canvas.ActualWidth / ratio;
                scroll.Width = canvas.ActualWidth;
            }
        }
        private void ShowPowerpointBlocker(string explanation)
        {
            showPowerPointProgress(explanation);
            Canvas.SetTop(ProgressDisplay, (canvas.ActualHeight / 2));
            Canvas.SetLeft(ProgressDisplay, (ActualWidth / 2) - 100);
            InputBlocker.Visibility = Visibility.Visible;
        }
        private void HideProgressBlocker()
        {
            ProgressDisplay.Children.Clear();
            InputBlocker.Visibility = Visibility.Collapsed;
        }
        private void canZoomIn(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(scroll == null);
        }
        private void canZoomOut(object sender, CanExecuteRoutedEventArgs e)
        {
            if (scroll == null)
                e.CanExecute = false;
            else
            {
                var cvHeight = adornerGrid.ActualHeight;
                var cvWidth = adornerGrid.ActualWidth;
                var cvRatio = cvWidth / cvHeight;
                bool hTrue = scroll.ViewportWidth < scroll.ExtentWidth;
                bool vTrue = scroll.ViewportHeight < scroll.ExtentHeight;
                var scrollRatio = scroll.ActualWidth / scroll.ActualHeight;
                if (scrollRatio > cvRatio)
                {
                    e.CanExecute = hTrue;
                }
                if (scrollRatio < cvRatio)
                {
                    e.CanExecute = vTrue;
                }
                e.CanExecute = (hTrue || vTrue);
            }
        }
        private void adornerGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FixCanvasAspectAfterWindowSizeChanges(e.PreviousSize, e.NewSize);
        }
        private void FixCanvasAspectAfterWindowSizeChanges(System.Windows.Size oldSize, System.Windows.Size newSize)
        {
            var cvHeight = adornerGrid.ActualHeight;
            var cvWidth = adornerGrid.ActualWidth;
            var cvRatio = cvWidth / cvHeight;
            var scrollRatio = scroll.ActualWidth / scroll.ActualHeight;
            if (oldSize.Height == newSize.Height)
            {
                if (scroll.ActualHeight * cvRatio > scroll.ExtentWidth)
                {
                    scroll.Width = scroll.ExtentWidth;
                    return;
                }
                scroll.Width = scroll.ActualHeight * cvRatio;
                return;
            }
            if (oldSize.Width == newSize.Width)
            {
                if (scroll.ActualWidth / cvRatio > scroll.ExtentHeight)
                {
                    scroll.Height = scroll.ExtentHeight;
                    return;
                }
                scroll.Height = scroll.ActualWidth / cvRatio;
                return;
            }
            if (scrollRatio > cvRatio)
            {
                var newWidth = scroll.ActualHeight * cvRatio;
                scroll.Width = newWidth;
                return;
            }
            if (scrollRatio < cvRatio)
            {
                var newHeight = scroll.Width / cvRatio;
                scroll.Height = newHeight;
                return;
            }
            if (Double.IsNaN(scrollRatio))
            {
                scroll.Width = scroll.ExtentWidth;
                scroll.Height = scroll.ExtentHeight;
                return;
            }
        }
        private void doZoomIn(object sender, ExecutedRoutedEventArgs e)
        {
            var ZoomValue = 0.9;
            var cvHeight = adornerGrid.ActualHeight;
            var cvWidth = adornerGrid.ActualWidth;
            var cvRatio = cvWidth / cvHeight;
            var scrollRatio = scroll.ActualWidth / scroll.ActualHeight;
            if (scrollRatio > cvRatio)
            {
                var newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
                var newHeight = newWidth / cvRatio;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
            }
            if (scrollRatio < cvRatio)
            {
                var newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                var newWidth = newHeight * cvRatio;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            if (scrollRatio == cvRatio)
            {
                var newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                var newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
        }
        private void doZoomOut(object sender, ExecutedRoutedEventArgs e)
        {
            var ZoomValue = 1.1;
            var cvHeight = adornerGrid.ActualHeight;
            var cvWidth = adornerGrid.ActualWidth;
            var cvRatio = cvWidth / cvHeight;
            var scrollRatio = scroll.ActualWidth / scroll.ActualHeight;
            if (scrollRatio > cvRatio)
            {
                var newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
                var newHeight = newWidth / cvRatio;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
            }
            if (scrollRatio < cvRatio)
            {
                var newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                var newWidth = newHeight * cvRatio;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            if (scrollRatio == cvRatio)
            {
                var newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                var newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
        }
        public Visibility GetVisibilityOf(UIElement target)
        {
            return target.Visibility;
        }
        private void SetConversationPermissions(object obj)
        {
            var style = (string)obj;
            foreach (var s in new[]{
                Permissions.LABORATORY_PERMISSIONS,
                Permissions.TUTORIAL_PERMISSIONS,
                Permissions.LECTURE_PERMISSIONS,
                Permissions.MEETING_PERMISSIONS})
                if (s.Label == style)
                    details.Permissions = s;
            ConversationDetailsProviderFactory.Provider.Update(details);
        }
        private bool CanSetConversationPermissions(object _style)
        {
            return details != null && userInformation.credentials.name == details.Author;
        }
        /*taskbar management*/
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        private void sleep(object _obj)
        {
            Dispatcher.adoptAsync(delegate { Hide(); });
        }
        private void wakeUp(object _obj)
        {
            Dispatcher.adoptAsync(delegate
            {
                Show();
                WindowState = System.Windows.WindowState.Maximized;
            });
        }
        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }
        public void SetPedagogyLevel(PedagogyLevel level)
        {
            SetupUI(level);
        }
        public void ClearUI()
        {
            Commands.UnregisterAllCommands();
            ribbon.Tabs.Clear();
            privacyTools.Children.Clear();
            RHSDrawerDefinition.Width = new GridLength(0);
        }
        private GridSplitter split()
        {
            return new GridSplitter
            {
                ShowsPreview = true,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 10,
                Height = Double.NaN,
                ResizeBehavior = GridResizeBehavior.PreviousAndNext
            };
        }
        public void SetupUI(PedagogyLevel level)
        {
            List<FrameworkElement> homeGroups = new List<FrameworkElement>();
            List<FrameworkElement> tabs = new List<FrameworkElement>();
            foreach (var i in Enumerable.Range(0, level.code + 1))
            {
                switch (i)
                {
                    case 0:
                        ClearUI();
                        homeGroups.Add(new EditingOptions());
                        //homeGroups.Add(new PenColors());
                        break;
                    case 1:
                        tabs.Add(new Tabs.Quizzes());
                        tabs.Add(new Tabs.Submissions());
                        homeGroups.Add(new PrivacyToolsHost());
                        homeGroups.Add(new EditingModes());
                        //homeGroups.Add(new ToolBox());
                        //homeGroups.Add(new TextTools());
                        break;
                    case 2:
                        ribbon.ApplicationPopup = new Chrome.ApplicationPopup();
                        RHSDrawerDefinition.Width = new GridLength(180);
                        homeGroups.Add(new ZoomControlsHost());
                        homeGroups.Add(new MiniMap());
                        break;
                    case 3:
                        homeGroups.Add(new SandRibbon.Tabs.Groups.Friends());
                        homeGroups.Add(new Notes());
                        tabs.Add(new Tabs.Analytics());
                        tabs.Add(new Tabs.Plugins());
                        //privacyTools.Children.Add(new PrivacyTools());
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
            CommandManager.InvalidateRequerySuggested();
            Commands.RequerySuggested();
            Commands.SetLayer.Execute("Text");
            Commands.SetLayer.Execute("Sketch");
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
                catch (FormatException e)
                {
                    return 0;
                }
            }
        }
        private void updateCurrentPenAfterZoomChanged()
        {
            try
            {
                AdjustDrawingAttributesAccordingToZoom(Globals.drawingAttributes);
            }
            catch (NotSetException) { }
        }
        private void zoomConcernedControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateCurrentPenAfterZoomChanged();
        }

        private void fixAutoSizing(object sender, SizeChangedEventArgs e)
        {
            var lastValue = Commands.SetLayer.lastValue();
            Commands.SetLayer.Execute("Sketch");
            Commands.SetLayer.Execute("Text");
            Commands.SetLayer.Execute(lastValue);
            /*var currentTab = ribbon.SelectedTab;
            if (currentTab != null)
            {
                Dictionary<RibbonGroup, Visibility> currentVisibility = new Dictionary<RibbonGroup, Visibility>();
                foreach (RibbonGroup rg in currentTab.Items)
                {
                    currentVisibility[rg] = rg.Visibility;
                    rg.Visibility = Visibility.Collapsed;
                }
                foreach (RibbonGroup rg in currentTab.Items)
                {
                    //rg.Visibility = Visibility.Visible;
                    rg.Visibility = currentVisibility[rg];
                }
            }*/
        }
    }

}