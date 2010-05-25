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

namespace SandRibbon
{
    public partial class Window1 : PedagogicallyVariable
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
        private string privacy;
        public string CurrentProgress { get; set; }
        /*Clunky - these are to be exposed to the debugging and support RePL, which has access to this
         * window instance through the visual hierarchy.*/
        public static RoutedCommand ProxyMirrorExtendedDesktop = new RoutedCommand();
        public static ScrollViewer MAIN_SCROLL;
        public string log
        {
            get { return Logger.log; }
        }
        public Window1()
        {
            ProviderMonitor.HealthCheck(DoConstructor);
        }
        private void DoConstructor()
        {
            InitializeComponent();
            userInformation.policy = new JabberWire.Policy { isSynced = false, isAuthor = false };
            Commands.ChangeTab.RegisterCommand(new DelegateCommand<string>(text =>
            {
                foreach (var tab in ribbon.Tabs)
                    if (((RibbonTab)tab).Text == text)
                        ribbon.SelectedTab = (RibbonTab)tab;
            }));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(SetIdentity));
            Commands.SetLayer.Execute("Sketch");
            powerPointFinished = new DelegateCommand<string>((_whatever) => Dispatcher.BeginInvoke((Action)(finishedPowerpoint)));
            Commands.PowerPointLoadFinished.RegisterCommand(powerPointFinished);
            powerpointProgress = new DelegateCommand<string>(
                (progress) => Dispatcher.BeginInvoke((Action)(() => showPowerPointProgress(progress))));
            Commands.PowerPointProgress.RegisterCommand(powerpointProgress);
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(ExecuteMoveTo, CanExecuteMoveTo));
            Commands.ClearDynamicContent.RegisterCommand(new DelegateCommand<object>(ClearDynamicContent));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation, mustBeLoggedIn));
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<object>(createConversation, mustBeLoggedIn));
            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(_obj => { }, mustBeLoggedIn));
            Commands.ToggleFriendsVisibility.RegisterCommand(new DelegateCommand<object>(toggleFriendsVisibility, mustBeLoggedIn));
            Commands.PrintConversation.RegisterCommand(new DelegateCommand<object>((_arg) => { }, mustBeInConversation));
            Commands.PrintConversationHandout.RegisterCommand(new DelegateCommand<object>((_arg) => { }, mustBeInConversation));
            Commands.PrintCompleted.RegisterCommand(new DelegateCommand<object>(_obj => HideProgressBlocker()));
            Commands.ImportPowerpoint.RegisterCommand(new DelegateCommand<object>(
                _obj =>
                {
                    ShowPowerpointBlocker("Starting Powerpoint Import");
                    Commands.PostImportPowerpoint.Execute(null);
                }, mustBeLoggedIn));
            Commands.StartPowerPointLoad.RegisterCommand(new DelegateCommand<object>(
                (conversationDetails) =>
                {
                    ShowPowerpointBlocker("Starting Powerpoint Import");
                    Commands.PostStartPowerPointLoad.Execute(conversationDetails);
                },
                mustBeLoggedIn));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<object>(CreateConversation));
            Commands.EditConversation.RegisterCommand(new DelegateCommand<object>(EditConversation, mustBeInConversation));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(setSync));
            Commands.SetInkCanvasMode.RegisterCommand(new DelegateCommand<object>(_obj => setLayer("Sketch"), mustBeInConversation));
            Commands.ToggleScratchPadVisibility.RegisterCommand(new DelegateCommand<object>(ToggleNotePadVisibility, mustBeLoggedIn));
            Commands.SetLayer.RegisterCommand(new DelegateCommand<object>(_obj => { }, mustBeInConversation));
            Commands.ImageDropped.RegisterCommand(new DelegateCommand<object>(_obj => { }, mustBeLoggedIn));
            Commands.SetTutorialVisibility.RegisterCommand(new DelegateCommand<object>(SetTutorialVisibility, mustBeInConversation));
            Logger.Log("Started MeTL");
            Commands.CreateQuiz.RegisterCommand(new DelegateCommand<object>(CreateQuiz, canCreateQuiz));
            Commands.CreateQuiz.RegisterCommand(new DelegateCommand<object>(obj => { }, amAuthor));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<object>(_obj => { }, mustBeLoggedIn));
            Commands.MoveToQuiz.RegisterCommand(new DelegateCommand<QuizDetails>(moveToQuiz));
            Commands.Relogin.RegisterCommand(new DelegateCommand<object>(Relogin));
            Commands.MirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(_o => { }, mustBeInConversation));
            Commands.ProxyMirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(_arg => Commands.MirrorPresentationSpace.Execute(this)));
            Commands.ReceiveWormMove.RegisterCommand(new DelegateCommand<string>(ReceiveWormMove));
            Commands.SetConversationPermissions.RegisterCommand(new DelegateCommand<object>(SetConversationPermissions, CanSetConversationPermissions));
            Commands.AddWindowEffect.RegisterCommand(new DelegateCommand<object>(AddWindowEffect));
            Commands.RemoveWindowEffect.RegisterCommand(new DelegateCommand<object>(RemoveWindowEffect));
            Commands.SendWakeUp.RegisterCommand(new DelegateCommand<object>(_nil => { }, mustBeLoggedIn));
            Commands.ReceiveWakeUp.RegisterCommand(new DelegateCommand<object>(wakeUp));
            Commands.ReceiveSleep.RegisterCommand(new DelegateCommand<object>(sleep));
            Commands.FitToView.RegisterCommand(new DelegateCommand<object>(FitToView));
            Commands.FitToPageWidth.RegisterCommand(new DelegateCommand<object>(FitToPageWidth));
            Commands.SetZoomRect.RegisterCommand(new DelegateCommand<Rectangle>(SetZoomRect));
            adornerScroll.scroll = scroll;
            adornerScroll.scroll.SizeChanged += adornerScroll.scrollChanged;
            adornerScroll.scroll.ScrollChanged += adornerScroll.scroll_ScrollChanged;
            AddWindowEffect(null);
            if (SmartBoardMeTLAlreadyLoaded)
                checkIfSmartboard();
            Pedagogicometer.RegisterVariant(this);
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
        }
        private void EditConversation(object _unused)
        {
            ShowPowerpointBlocker("Editing Conversation Dialog Open");
        }
        private void SetIdentity(SandRibbon.Utils.Connection.JabberWire.Credentials credentials)
        {
            connect(credentials.name, credentials.password, 0, null);
            if (credentials.name.Contains("S15")) sleep(null);
            var conversations = ConversationDetailsProviderFactory.Provider.ListConversations();
            Commands.RequerySuggested();
            Commands.AllStaticCommandsAreRegistered();
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
                bool isSmartboardMeTL = true;
                var thisProcess = Process.GetCurrentProcess();
                foreach (Process p in Process.GetProcessesByName("MeTL"))
                {
                    if ((p.MainWindowTitle.StartsWith("S15") || p.MainWindowTitle.Equals("")) && p.Id != thisProcess.Id)
                        isSmartboardMeTL = false;
                }
                return isSmartboardMeTL;
            }
        }
        public void checkIfSmartboard()
        {
            var path = "C:\\Program Files\\MeTL\\boardIdentity.txt";
            if (!File.Exists(path)) return;
            var myFile = new StreamReader(path);
            var username = myFile.ReadToEnd();
            MessageBox.Show("logging in as {0}", username);
            JabberWire.SwitchServer("staging");
            var doDetails = (Action)delegate
            {
                Title = username + " MeTL waiting for wakeup";
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doDetails);
            else
                doDetails();
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
                Dispatcher.Invoke((Action)delegate
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
                removeQuiz();
                details = ConversationDetailsProviderFactory.Provider.DetailsOf(userInformation.location.activeConversation);
                applyPermissions(details.Permissions);
                Commands.UpdateConversationDetails.Execute(details);
                Logger.Log("Joined conversation " + title);
                Commands.RequerySuggested(Commands.SetConversationPermissions);
            });
        }
        private string messageFor(ConversationDetails details)
        {
            var permissionLabel = Permissions.InferredTypeOf(details.Permissions).Label;
            return string.Format("{3} is in {0}'s \"{1}\", currently in {2} style", details.Author, details.Title, permissionLabel, userInformation.credentials.name);
        }
        private void ClearDynamicContent(object obj)
        {
            removeQuiz();
        }
        private void MoveTo(int slide)
        {
            removeQuiz();
            if (userInformation.policy.isAuthor && userInformation.policy.isSynced)
                Commands.SendSyncMove.Execute(slide);
            var moveTo = (Action)delegate
                                     {
                                         if (canvas.Visibility == Visibility.Collapsed)
                                             canvas.Visibility = Visibility.Visible;
                                         scroll.Width = Double.NaN;
                                         scroll.Height = Double.NaN;
                                         canvas.Width = Double.NaN;
                                         canvas.Height = Double.NaN;
                                     };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(moveTo);
            else
                moveTo();
            CommandManager.InvalidateRequerySuggested();
        }
        private void removeQuiz()
        {
            if (dynamicContent.Children.Count > 0)
            {
                dynamicContent.Children.Clear();
                Commands.RequerySuggested(Commands.CreateQuiz);
            }
        }
        private bool canCreateQuiz(object arg)
        {
            return dynamicContent.Children.Count == 0;
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
        private void CreateQuiz(object numberOfQuestions)
        {//Insert the new slide after this.  Query the resultant conversation for the new slide's id.  Use that id as the quiz location.
            var slides = ConversationDetailsProviderFactory.Provider.AppendSlideAfter(
                userInformation.location.currentSlide,
                userInformation.location.activeConversation,
                Slide.TYPE.POLL).Slides;
            var quizSlideId = slides[slides.Single(s => s.id == userInformation.location.currentSlide).index + 1].id;
            dynamicContent.Children.Add(new AuthorAQuiz(userInformation, quizSlideId).SetQuestionCount((int)numberOfQuestions));
        }
        private void moveToQuiz(QuizDetails quiz)
        {
            dynamicContent.Children.Clear();
            dynamicContent.Children.Add(new AnswerAQuiz(userInformation, quiz).SetQuestionCount(quiz.optionCount));
        }
        private bool amAuthor(object _obj)
        {
            return userInformation.policy.isAuthor;
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
            if (details.Jid != userInformation.location.activeConversation) return;
            var doUpdate = (Action)delegate
            {
                userInformation.location.availableSlides = details.Slides.Select(s => s.id).ToList();
                HideTutorial();
                UpdateTitle();
                var isAuthor = (details.Author != null) && details.Author == userInformation.credentials.name;
                userInformation.policy.isAuthor = isAuthor;
                Commands.RequerySuggested();
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doUpdate);
            else
                doUpdate();
        }
        private void UpdateTitle()
        {
            Title = messageFor(Globals.conversationDetails);
        }
        private DelegateCommand<object> canOpenFriendsOverride;
        private void applyPermissions(Permissions permissions)
        {
            if (canOpenFriendsOverride != null)
                Commands.ToggleFriendsVisibility.UnregisterCommand(canOpenFriendsOverride);
            canOpenFriendsOverride = new DelegateCommand<object>((_param) => { }, (_param) => true);
            Commands.ToggleFriendsVisibility.RegisterCommand(canOpenFriendsOverride);
        }
        private DelegateCommand<bool> canPublishOverride;
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
            InputBlocker.Visibility = Visibility.Visible;
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
        private void toggleFriendsVisibility(object _param)
        {
            if (friends.Visibility == Visibility.Visible)
                hideFriends();
            else
                showFriends();
        }
        private void showFriends()
        {
            friendContent.Width = new GridLength(300);
            SetVisibilityOf(friends, Visibility.Visible);
        }
        private void hideFriends()
        {
            friendContent.Width = new GridLength(0);
            SetVisibilityOf(friends, Visibility.Collapsed);
        }
        private static void SetVisibilityOf(UIElement target, Visibility visibility)
        {
            target.Visibility = visibility;
        }
        private void ToggleNotePadVisibility(object _param)
        {
            if (drawer.Visibility == Visibility.Visible)
                hideScratchPad();
            else
                showScratchPad();
        }
        private void hideScratchPad()
        {
            scratchPadContent.Width = new GridLength(0);
            SetVisibilityOf(drawer, Visibility.Collapsed);
        }
        private void showScratchPad()
        {
            scratchPadContent.Width = new GridLength(300);
            SetVisibilityOf(drawer, Visibility.Visible);
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
            if (scroll == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
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
            var doHide = (Action)Hide;
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doHide);
            else
                doHide();
        }
        private void wakeUp(object _obj)
        {
            var doShow = (Action)delegate
            {
                Show();
                WindowState = System.Windows.WindowState.Maximized;
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doShow);
            else
                doShow();
        }
        private void minimizeWindow()
        {
            Hide();
            if (m_notifyIcon != null)
                m_notifyIcon.ShowBalloonTip(2000);
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }
        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }
        public bool CanSetLevel(PedagogyLevel level)
        {
            return true;
        }
        public bool SetLevel(PedagogyLevel level)
        {
            SetupUI(level);
            return true;
        }
        public void ClearUI()
        {
            Commands.UnregisterAllCommands();
            ribbon.Tabs.Clear();
            ribbon.ToolBar = null;
        }
        public void SetupUI(PedagogyLevel level)
        {
            foreach (var i in Enumerable.Range(0, level.code+1))
            {
                switch (i)
                {
                    case 0:
                        ClearUI();
                        break;
                    case 1:
                        ribbon.Tabs.Add(new Tabs.Home { DataContext=scroll });
                        ribbon.ToolBar = new Chrome.ToolBar();
                        break;
                    case 2:
                        ribbon.Tabs.Add(new Tabs.Quizzes());
                        ribbon.Tabs.Add(new Tabs.Analytics());
                        break;
                    default: 
                        break;
                }
            }
        }
    }
}