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
//using SandRibbonObjects;
using MeTLLib.DataTypes;
using System.Diagnostics;
using System.Windows.Shapes;
using SandRibbon.Components.Sandpit;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Tabs;
using SandRibbon.Tabs.Groups;
using System.Collections;
using System.Windows.Ink;
using System.Collections.ObjectModel;
using SandRibbon.Components.Utility;
using System.Windows.Documents;

namespace SandRibbon
{
    public partial class Window1
    {
        public readonly string RECENT_DOCUMENTS = "recentDocuments.xml";
        #region SurroundingServers
        #endregion
        //public JabberWire.UserInformation userInformation = new JabberWire.UserInformation();
        private DelegateCommand<string> powerpointProgress;
        private DelegateCommand<string> powerPointFinished;
        private PowerPointLoader loader = new PowerPointLoader();
        private UndoHistory history = new UndoHistory();
        public ConversationDetails details;
        private MeTLLib.Providers.Connection.JabberWire wire;
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
            //This following healthcheck is providing a slowdown that allows the window to instantiate correctly.
            //We should fine out why it needs it at some point.  SimpleConversationSelector results in a XAML parse exception if it's not in.
            ProviderMonitor.HealthCheck(() =>
            {
                DoConstructor();
            });
            Commands.AllStaticCommandsAreRegistered();
        }
        private void DoConstructor()
        {
            InitializeComponent();
            var level = ConfigurationProvider.instance.getMeTLPedagogyLevel();
            CommandParameterProvider.parameters[Commands.SetPedagogyLevel] = Pedagogicometer.level(level);
            Title = Globals.MeTLType;
            try
            {
                Icon = (ImageSource)new ImageSourceConverter().ConvertFromString("resources\\" + Globals.MeTLType + ".ico");
            }
            catch (Exception) { }
            Globals.userInformation.policy = new Policy(false, false);
            Commands.ChangeTab.RegisterCommand(new DelegateCommand<string>(ChangeTab));
            //Commands.ConnectWithAuthenticatedCredentials.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.Credentials>(ConnectWithAuthenticatedCredentials));
            Commands.PowerpointFinished.RegisterCommand(new DelegateCommand<object>(UnblockInput));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(ExecuteMoveTo, CanExecuteMoveTo));
            Commands.LogOut.RegisterCommand(new DelegateCommand<object>(noop, mustBeLoggedIn));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation, mustBeLoggedIn));
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<object>(createConversation, mustBeLoggedIn));
            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(noop, mustBeLoggedIn));
            Commands.ShowPrintConversationDialog.RegisterCommand(new DelegateCommand<object>(noop, mustBeInConversation));
            Commands.PrintConversation.RegisterCommand(new DelegateCommand<object>(noop, mustBeInConversation));
            Commands.PrintConversationHandout.RegisterCommand(new DelegateCommand<object>(noop, mustBeInConversation));
            Commands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(ImportPowerPoint, mustBeLoggedIn));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.PreCreateConversation.RegisterCommand(new DelegateCommand<object>(CreateConversation));
            Commands.PreDeleteConversation.RegisterCommand(new DelegateCommand<object>(DeleteConversation, mustBeAuthor));
            Commands.PreEditConversation.RegisterCommand(new DelegateCommand<object>(EditConversation, mustBeAuthor));
            Commands.PreEditConversation.RegisterCommand(new DelegateCommand<object>(noop, mustBeInConversation));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(setSync));
            Commands.SetInkCanvasMode.RegisterCommand(new DelegateCommand<object>(SetInkCanvasMode, mustBeInConversation));
            Commands.ToggleScratchPadVisibility.RegisterCommand(new DelegateCommand<object>(noop, mustBeLoggedIn));
            Commands.SetLayer.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.FitToView.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.OriginalView.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.InitiateGrabZoom.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.ExtendCanvasBothWays.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.DummyCommandToProcessCanExecute.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
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
            Commands.SetLayer.ExecuteAsync("Sketch");
            Commands.MoveCanvasByDelta.RegisterCommand(new DelegateCommand<Point>(GrabMove));
            Commands.BlockInput.RegisterCommand(new DelegateCommand<string>(BlockInput));
            Commands.UnblockInput.RegisterCommand(new DelegateCommand<object>(UnblockInput));
            Commands.AddPrivacyToggleButton.RegisterCommand(new DelegateCommand<PrivacyToggleButton.PrivacyToggleButtonInfo>(AddPrivacyButton));
            Commands.RemovePrivacyAdorners.RegisterCommand(new DelegateCommand<object>(RemovePrivacyAdorners));
            Commands.DummyCommandToProcessCanExecute.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.DummyCommandToProcessCanExecuteForPrivacyTools.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosedAndMustBeAllowedToPublish));
            Commands.SetInkCanvasMode.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.HideConversationSearchBox.RegisterCommand(new DelegateCommand<object>(noop, mustBeInConversation));
            Commands.AddImage.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.SetTextCanvasMode.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.ToggleBold.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.ToggleItalic.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.ToggleUnderline.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.ToggleStrikethrough.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.RestoreTextDefaults.RegisterCommand(new DelegateCommand<object>(noop, conversationSearchMustBeClosed));
            Commands.ToggleFriendsVisibility.RegisterCommand(new DelegateCommand<object>(ToggleFriendsVisibility, conversationSearchMustBeClosed));

            adornerScroll.scroll = scroll;
            adornerScroll.scroll.SizeChanged += adornerScroll.scrollChanged;
            adornerScroll.scroll.ScrollChanged += adornerScroll.scroll_ScrollChanged;
            AddWindowEffect(null);
            App.Now("Restoring settings");
            WorkspaceStateProvider.RestorePreviousSettings();
            App.Now("Started MeTL");
        }

        private void noop(object unused)
        {
        }
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
        private void ShowEditSlidesDialog(object unused)
        {
            new SlidesEditingDialog().ShowDialog();
            var seDialog = new SlidesEditingDialog();
            seDialog.Owner = Window.GetWindow(this);
            seDialog.ShowDialog();
        }
        private void SetInkCanvasMode(object unused)
        {
            Commands.SetLayer.ExecuteAsync("Sketch");
        }
        private void AddPrivacyButton(PrivacyToggleButton.PrivacyToggleButtonInfo info)
        {
            var adorner = ((FrameworkElement)canvasViewBox);
            Dispatcher.adoptAsync(()=>{
                var adornerRect = new Rect(canvas.TranslatePoint(info.ElementBounds.TopLeft, canvasViewBox), canvas.TranslatePoint(info.ElementBounds.BottomRight, canvasViewBox));
            if (adornerRect.Right < 0 || adornerRect.Right > canvasViewBox.ActualWidth
                || adornerRect.Top < 0 || adornerRect.Top > canvasViewBox.ActualHeight) return;
            AdornerLayer.GetAdornerLayer(adorner).Add(new UIAdorner(adorner, new PrivacyToggleButton(info.privacyChoice, adornerRect)));
            });
        }
        private Adorner[] getPrivacyAdorners()
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(canvasViewBox);
            if (adornerLayer == null) return null;
            return adornerLayer.GetAdorners(canvasViewBox);
        }
        private void UpdatePrivacyAdorners()
        {
            var privacyAdorners = getPrivacyAdorners();
            RemovePrivacyAdorners(null);
            if (privacyAdorners != null && privacyAdorners.Count() > 0)
                try
                {
                    var lastValue = Commands.AddPrivacyToggleButton.lastValue();
                    if (lastValue != null)
                        AddPrivacyButton((PrivacyToggleButton.PrivacyToggleButtonInfo)lastValue);
                }
                catch (NotSetException) { }
        }
        private void RemovePrivacyAdorners(object _unused)
        {
            Dispatcher.adoptAsync(delegate
            {
                var adorners = getPrivacyAdorners();
                var adornerLayer = AdornerLayer.GetAdornerLayer(canvasViewBox);
                if (adorners != null)
                    foreach (var adorner in adorners)
                        adornerLayer.Remove(adorner);
            });

        }
        private void ProxyMirrorPresentationSpace(object unused)
        {
            Commands.MirrorPresentationSpace.ExecuteAsync(this);
        }
        private void GrabMove(Point moveDelta)
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
            catch (Exception e)
            {//out of range exceptions and the like 
            }
        }
        private void ChangeTab(string which)
        {
            foreach (var tab in ribbon.Tabs)
                if (((RibbonTab)tab).Text == which)
                    ribbon.SelectedTab = (RibbonTab)tab;
        }
        private void ImportPowerPoint(object unused)
        {
            ShowPowerpointBlocker("Starting PowerPoint Import");
        }
        private void AdjustReportedDrawingAttributesAccordingToZoom(object attributes)
        {
            var zoomIndependentAttributes = ((DrawingAttributes)attributes).Clone();
            if (zoomIndependentAttributes.Height == Double.NaN || zoomIndependentAttributes.Width == Double.NaN)
                if (zoomIndependentAttributes.Height == Double.NaN || zoomIndependentAttributes.Width == Double.NaN)
                    return;
            var currentZoomHeight = scroll.ActualHeight / canvasViewBox.ActualHeight;
            var currentZoomWidth = scroll.ActualWidth / canvasViewBox.ActualWidth;
            var currentZoom = Math.Max(currentZoomHeight, currentZoomWidth);
            var desiredZoom = zoomIndependentAttributes.Height / currentZoom;
            zoomIndependentAttributes.Height = correctZoom(desiredZoom);
            zoomIndependentAttributes.Width = correctZoom(desiredZoom);
            Commands.UpdateCursor.ExecuteAsync(CursorExtensions.generateCursor(zoomIndependentAttributes));
            Commands.ReportDrawingAttributes.ExecuteAsync(zoomIndependentAttributes);
        }
        private void AdjustReportedStrokeAttributesAccordingToZoom(object attributes)
        {
            var zoomIndependentAttributes = ((DrawingAttributes)attributes).Clone();
            if (zoomIndependentAttributes.Height == Double.NaN || zoomIndependentAttributes.Width == Double.NaN)
                return;
            var currentZoomHeight = scroll.ActualHeight / canvasViewBox.ActualHeight;
            var currentZoomWidth = scroll.ActualWidth / canvasViewBox.ActualWidth;
            var currentZoom = Math.Max(currentZoomHeight, currentZoomWidth);
            var desiredZoom = zoomIndependentAttributes.Height / currentZoom;
            zoomIndependentAttributes.Height = correctZoom(desiredZoom);
            zoomIndependentAttributes.Width = correctZoom(desiredZoom);
            Commands.ReportStrokeAttributes.ExecuteAsync(zoomIndependentAttributes);
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
            if (desiredZoom > DrawingAttributes.MinWidth && desiredZoom < DrawingAttributes.MaxWidth)
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
            Commands.ActualChangePenSize.ExecuteAsync(correctZoom(desiredZoom));
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
            Commands.UpdateCursor.ExecuteAsync(CursorExtensions.generateCursor(zoomCorrectAttributes));
            Commands.ActualSetDrawingAttributes.ExecuteAsync(zoomCorrectAttributes);
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
        private void CreateConversation(object _unused)
        {
            ShowPowerpointBlocker("Creating Conversation Dialog Open");
            Commands.CreateConversationDialog.ExecuteAsync(null);
        }
        private void DeleteConversation(object _unused)
        {
            ShowPowerpointBlocker("Delete Conversation Dialog Open");
            Commands.DeleteConversation.ExecuteAsync(null);
        }
        private void EditConversation(object _unused)
        {
            ShowPowerpointBlocker("Editing Conversation Dialog Open");
            Commands.EditConversation.ExecuteAsync(Globals.location.activeConversation);
        }
        private void BlockInput(string message)
        {
            ShowPowerpointBlocker(message);
        }
        private void UnblockInput(object _unused)
        {
            Dispatcher.adoptAsync((HideProgressBlocker));
        }
        private void ConnectWithAuthenticatedCredentials(MeTLLib.DataTypes.Credentials credentials)
        {
            //connect(credentials.name, credentials.password, 0, null);
            Commands.AllStaticCommandsAreRegistered();
            Commands.RequerySuggested();
            Pedagogicometer.SetPedagogyLevel(Globals.pedagogy);
            Commands.SetIdentity.ExecuteAsync(credentials);
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
        private static object reconnectionLock = new object();
        private static bool reconnecting = false;
        private void AddWindowEffect(object _o)
        {
            CanvasBlocker.Visibility = Visibility.Visible;

        }
        private void RemoveWindowEffect(object _o)
        {
            Dispatcher.adoptAsync(() =>
            CanvasBlocker.Visibility = Visibility.Collapsed);
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
                            wire.JoinConversation(Globals.userInformation.location.activeConversation);
                            Commands.MoveTo.ExecuteAsync(Globals.userInformation.location.currentSlide);
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
            MoveTo(slide);
            // ProviderMonitor.HealthCheck(() => MoveTo(slide));
        }
        private bool CanExecuteMoveTo(int slide)
        {
            return mustBeInConversation(slide);
        }
        private void JoinConversation(string title)
        {
            //ProviderMonitor.HealthCheck(() =>
            //{
            Commands.LoggedIn.ExecuteAsync(Globals.credentials.name);
            //var activeConversation = MeTLLib.ClientFactory.Connection().location.activeConversation;
            //var details = ConversationDetailsProviderFactory.Provider.DetailsOf(Globals.location.activeConversation);
            var details = Globals.conversationDetails;
            //MeTLLib.ClientFactory.Connection().DetailsOf(Globals.location.activeConversation);
            RecentConversationProvider.addRecentConversation(details, Globals.me);
            if (details.Author == Globals.me)
                Commands.SetPrivacy.ExecuteAsync("public");
            else
                Commands.SetPrivacy.ExecuteAsync("private");
            applyPermissions(details.Permissions);
            Commands.UpdateConversationDetails.ExecuteAsync(details);
            Logger.Log("Joined conversation " + title);
            Commands.RequerySuggested(Commands.SetConversationPermissions);
            Commands.SetLayer.ExecuteAsync("Sketch");
            if (automatedTest(details.Title))
                ribbon.SelectedTab = ribbon.Tabs[1];
            //});
        }

        private bool automatedTest(string conversationName)
        {
            if (Globals.me.Contains("Admirable") && conversationName.ToLower().Contains("automated")) return true;
            return false;
        }
        private string messageFor(ConversationDetails details)
        {
            var permissionLabel = Permissions.InferredTypeOf(details.Permissions).Label;
            return string.Format("{3} is in {0}'s \"{1}\", currently in {2} style", details.Author, details.Title, permissionLabel, Globals.userInformation.credentials.name);
        }
        private void MoveTo(int slide)
        {
            if ((Globals.userInformation.policy.isAuthor && Globals.userInformation.policy.isSynced) || (Globals.synched && Globals.userInformation.policy.isAuthor))
                Commands.SendSyncMove.ExecuteAsync(slide);
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
            return;
            lock (reconnectionLock)
            {
                showReconnectingDialog();
                reconnecting = true;
                connect(Globals.userInformation.credentials.name, Globals.userInformation.credentials.password, Globals.userInformation.location.currentSlide, Globals.userInformation.location.activeConversation);
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
        private void moveToQuiz(MeTLLib.DataTypes.QuizQuestion quiz)
        {
        }

        private void ShowTutorial()
        {
            Dispatcher.adoptAsync(() =>
            TutorialLayer.Visibility = Visibility.Visible);
        }
        private void HideTutorial()
        {
            if (Globals.userInformation.location != null && !String.IsNullOrEmpty(Globals.userInformation.location.activeConversation))
                Dispatcher.adoptAsync(() =>
                {
                    TutorialLayer.Visibility = Visibility.Collapsed;
                });
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
        private bool conversationSearchMustBeClosed(object _obj)
        {
            return currentConversationSearchBox.Visibility == Visibility.Collapsed && mustBeInConversation(null);
        }
        private bool conversationSearchMustBeClosedAndMustBeAllowedToPublish(object _obj)
        {
            if (conversationSearchMustBeClosed(null))
                return Globals.isAuthor || Globals.conversationDetails.Permissions.studentCanPublish;
            else return false;
        }

        private bool mustBeInConversation(object _arg)
        {
            try
            {
                if (Globals.location.activeConversation != null && Globals.conversationDetails.Subject != "Deleted")
                    return true;
                return false;

            }
            catch (NotSetException)
            {
                return false;
            }
        }
        private bool mustBeAuthor(object _arg)
        {
            try
            {
                return Globals.isAuthor;
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
                if (String.IsNullOrEmpty(details.Jid) || details.Jid != Globals.conversationDetails.Jid) return;
                //if (details.Jid != Globals.location.activeConversation) return;
            }
            catch (NotSetException)
            {
                //We're not anywhere yet so update away
            }
            Dispatcher.adoptAsync(delegate
            {
                Globals.userInformation.location.availableSlides = details.Slides.Select(s => s.id).ToList();
                HideTutorial();
                UpdateTitle();
                var isAuthor = (details.Author != null) && details.Author == Globals.userInformation.credentials.name;
                Globals.userInformation.policy.isAuthor = isAuthor;
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
        private void connect(string username, string pass, int location, string conversation)
        {
            /*   if (wire == null)
               {
                   Globals.userInformation.location = new Location { currentSlide = location, activeConversation = conversation };
                   Globals.userInformation.credentials = new Credentials { name = username, password = pass };
                   wire = new JabberWire(Globals.userInformation.credentials);
                   wire.Login(Globals.userInformation.location);
               }
               else
               {
                   Globals.userInformation.location.activeConversation = conversation;
                   Globals.userInformation.location.currentSlide = location;
                   wire.Reset("Window1");
               }
               loader.wire = wire;*/
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
                var connection = MeTLLib.ClientFactory.Connection();
                details = connection.CreateConversation(details);
                //ConversationDetailsProviderFactory.Provider.Create(details);
                CommandManager.InvalidateRequerySuggested();
                if (Commands.JoinConversation.CanExecute(details.Jid))
                    Commands.JoinConversation.ExecuteAsync(details.Jid);
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
        private static void SetVisibilityOf(UIElement target, Visibility visibility)
        {
            target.Visibility = visibility;
        }
        private void setSync(object _obj)
        {
            Globals.userInformation.policy.isSynced = !Globals.userInformation.policy.isSynced;
        }
        private void OriginalView(object _unused)
        {
            var currentSlide = Globals.conversationDetails.Slides.Where(s => s.id == Globals.slide).First();
            if (currentSlide.defaultHeight == 0 || currentSlide.defaultWidth == 0) return;
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
            e.CanExecute = !(scroll == null) && mustBeInConversation(null) && conversationSearchMustBeClosed(null);
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
                e.CanExecute = (hTrue || vTrue) && mustBeInConversation(null) && conversationSearchMustBeClosed(null);
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
        }
        private void doZoomOut(object sender, ExecutedRoutedEventArgs e)
        {
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
            MeTLLib.ClientFactory.Connection().UpdateConversationDetails(details);
            //ConversationDetailsProviderFactory.Provider.Update(details);
            try
            {
                details = Globals.conversationDetails;
                foreach (var s in new[]
                                      {
                                          Permissions.LABORATORY_PERMISSIONS,
                                          Permissions.TUTORIAL_PERMISSIONS,
                                          Permissions.LECTURE_PERMISSIONS,
                                          Permissions.MEETING_PERMISSIONS
                                      })
                    if (s.Label == style)
                        details.Permissions = s;
                var client = MeTLLib.ClientFactory.Connection();
                client.UpdateConversationDetails(details);
                //ConversationDetailsProviderFactory.Provider.Update(details);
            }
            catch (NotSetException e)
            {
                return;
            }
        }
        private bool CanSetConversationPermissions(object _style)
        {
            return details != null && Globals.userInformation.credentials.name == details.Author;
            try
            {
                return Globals.conversationDetails != null && Globals.userInformation.credentials.name == Globals.conversationDetails.Author;
            }
            catch (NotSetException e)
            {
                return false;
            }
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
            Dispatcher.adoptAsync(() =>
            {
                ribbon.Tabs.Clear();
                privacyTools.Children.Clear();
                RHSDrawerDefinition.Width = new GridLength(0);
            });
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
            Dispatcher.adoptAsync(() =>
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
                            tabs.Add(new Tabs.Attachments());
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
                            //privacyTools.Children.Add(new PrivacyTools());
                            //homeGroups.Add(new Notes());
                            break;
                        case 4:
                            homeGroups.Add(new Notes());
                            tabs.Add(new Tabs.Analytics());
                            tabs.Add(new Tabs.Plugins());
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
            });
            CommandManager.InvalidateRequerySuggested();
            Commands.RequerySuggested();
            Commands.SetLayer.ExecuteAsync("Sketch");
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
            UpdatePrivacyAdorners();
            updateCurrentPenAfterZoomChanged();
        }
        private void scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdatePrivacyAdorners();
            updateCurrentPenAfterZoomChanged();
        }
    }

}