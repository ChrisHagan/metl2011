using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Divelements.SandRibbon;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Components.Interfaces;
using SandRibbon.Components.SimpleImpl;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbon.Providers.Structure;
using SandRibbon.Quizzing;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonInterop.Interfaces;
using SandRibbonObjects;
using System.Windows.Automation.Peers;

namespace SandRibbon
{
    public partial class Window1
    {
        public readonly string RECENT_DOCUMENTS = "recentDocuments.xml";
        #region SurroundingServers
        #endregion
        #region CoreModules
        private readonly Dictionary<Type, Func<FrameworkElement>> DEFAULT_COMPONENTS = new Dictionary<Type, Func<FrameworkElement>>
        {//These are the default implementations of the core modules.  Plugins may override these implementations but each of them must be supplied for MeTL to function.
            {typeof(ISlideDisplay), ()=>new SimpleSlideDisplay()},
            {typeof(IPencilCaseDisplay), ()=>new SimplePencilCaseDisplay()},
            {typeof(IFriendsDisplay), ()=>new SimpleFriendsListing()},
            {typeof(IConversationSelector), ()=>new SimpleConversationSelector()},
            {typeof(ITextTools), ()=>new SimpleTextTools()}
        };
        private ISlideDisplay slideDisplayProperty;
        public ISlideDisplay slideDisplay
        {//This is to ensure that the implementation of the module is always correctly housed.  If it is a plugin the PluginManager will have already housed it, otherwise it needs a RibbonGroup.
            get { return slideDisplayProperty; }
            set
            {
                slideDisplayProperty = value;
                if (slideDisplayProperty is UserControl && ((UserControl)slideDisplayProperty).Parent == null)
                {
                    Navigate.Items.Add(slideDisplayProperty);
                }
            }
        }
        private IPencilCaseDisplay pencilCaseDisplayProperty;
        public IPencilCaseDisplay pencilCaseDisplay
        {
            get
            {
                if (pencilCaseDisplayProperty == null)
                {
                    try
                    {
                        pencilCaseDisplay = (IPencilCaseDisplay)(new SimplePencilCaseDisplay());

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception Creating Default PencilCaseDisplay: " + ex);
                    }
                }
                return pencilCaseDisplayProperty;
            }
            set
            {
                pencilCaseDisplayProperty = value;
                if (pencilCaseDisplayProperty is UserControl && ((UserControl)pencilCaseDisplayProperty).Parent == null)
                {
                    //var parent = new RibbonGroup();
                    //parent.Items.Add(pencilCaseDisplayProperty);
                    //StableTools.Items.Add(parent);
                    StableTools.Children.Add((UIElement)pencilCaseDisplayProperty);
                }
            }
        }
        private IPenTools penToolsDisplayProperty;
        public IPenTools penToolsDisplay
        {
            get { return penToolsDisplayProperty; }
            set
            {
                penToolsDisplayProperty = value;
                if (penToolsDisplayProperty is UserControl && ((UserControl)penToolsDisplayProperty).Parent == null)
                {
                    //var parent = new RibbonGroup();
                    //parent.Items.Add(penToolsDisplayProperty);
                    //Sketch.Items.Insert(0,parent);
                }
            }
        }
        private IFriendsDisplay friendDisplayProperty;
        public IFriendsDisplay friendDisplay
        {
            get { return friendDisplayProperty; }
            set
            {
                friendDisplayProperty = value;
                if (friendDisplayProperty is UserControl && ((UserControl)friendDisplayProperty).Parent == null)
                {
                    // friends.friendScrollViewer.Content = friendDisplayProperty;
                }
            }
        }
        private IConversationSelector conversationSelectorProperty;
        public IConversationSelector conversationSelector
        {
            get { return conversationSelectorProperty; }
            set
            {
                conversationSelectorProperty = value;
                if (conversationSelectorProperty is UserControl && ((UserControl)conversationSelectorProperty).Parent == null)
                {
                    ConversationListing.Items.Add(conversationSelectorProperty);
                }
            }
        }
        private ITextTools textToolsProperty;
        public ITextTools textTools
        {
            get { return textToolsProperty; }
            set
            {
                textToolsProperty = value;
                if (textToolsProperty is UserControl && ((UserControl)textToolsProperty).Parent == null)
                {
                    //var parent = new RibbonGroup { Header = "Font" };
                    //parent.Items.Add(textToolsProperty);
                    //Text.Items.Add(parent);
                    StableTools.Children.Add((UIElement)textToolsProperty);
                }
            }
        }
        #endregion
        public JabberWire.UserInformation userInformation = new JabberWire.UserInformation();
        private DelegateCommand<string> powerpointProgress;
        private DelegateCommand<string> powerPointFinished;
        private DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials> setIdentity;
        private PowerPointLoader loader = new PowerPointLoader();
        private UndoHistory history = new UndoHistory();
        public ConversationDetails details;
        private JabberWire wire;
        private string privacy;
        public string CurrentProgress { get; set; }
        /*Clunky - these are to be exposed to the debugging and support RePL, which has access to this
         * window instance through the visual hierarchy.*/
        public static RoutedCommand ProxyMirrorExtendedDesktop = new RoutedCommand();
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
            loadPlugins();
            loadRemainingCoreComponents();
            userInformation.policy = new JabberWire.Policy { isSynced = false, isAuthor = false };
            Commands.ChangeTab.RegisterCommand(new DelegateCommand<string>(text =>
            {
                foreach (var tab in ribbon.Tabs)
                    if (((RibbonTab)tab).Text == text)
                        ribbon.SelectedTab = (RibbonTab)tab;
            }));
            setIdentity = new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(author =>
            {
                connect(author.name, author.password, 0, null);
                if (author.name.Contains("S15")) sleep(null);
                var conversations = ConversationDetailsProviderFactory.Provider.ListConversations();
                conversationSelector.List(conversations);
                recentDocuments.ListRecentConversations();
                Commands.RequerySuggested();
            });
            Commands.SetIdentity.RegisterCommand(setIdentity);
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(updateToolBox));
            Commands.SetLayer.Execute("Sketch");
            powerPointFinished = new DelegateCommand<string>((_whatever) => Dispatcher.BeginInvoke((Action)(finishedPowerpoint)));
            Commands.PowerPointLoadFinished.RegisterCommand(powerPointFinished);
            powerpointProgress = new DelegateCommand<string>(
                (progress) => Dispatcher.BeginInvoke((Action)(() => showPowerPointProgress(progress))));
            Commands.PowerPointProgress.RegisterCommand(powerpointProgress);

            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(setPrivacy));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(ExecuteMoveTo, CanExecuteMoveTo));
            Commands.ClearDynamicContent.RegisterCommand(new DelegateCommand<object>(ClearDynamicContent));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation, mustBeLoggedIn));
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<ConversationDetails>(createConversation, mustBeLoggedIn));
            Commands.ToggleFriendsVisibility.RegisterCommand(new DelegateCommand<object>(toggleFriendsVisibility, mustBeLoggedIn));
            Commands.PrintConversation.RegisterCommand(new DelegateCommand<object>((_arg) => { }, mustBeInConversation));
            Commands.PrintConversationHandout.RegisterCommand(new DelegateCommand<object>((_arg) => { }, mustBeInConversation));
            Commands.PrintCompleted.RegisterCommand(new DelegateCommand<object>(_obj => HideProgressBlocker()));
            Commands.StartPowerPointLoad.RegisterCommand(new DelegateCommand<object>(
                (conversationDetails) =>
                {
                    Import.Clear();
                    ShowPowerpointBlocker("Starting Powerpoint Import");
                    Commands.PostStartPowerPointLoad.Execute(conversationDetails);
                },
                mustBeLoggedIn));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(setSync));
            Commands.SetInkCanvasMode.RegisterCommand(new DelegateCommand<object>(_obj => setLayer("Sketch"), mustBeInConversation));
            Commands.ToggleScratchPadVisibility.RegisterCommand(new DelegateCommand<object>(ToggleNotePadVisibility, mustBeLoggedIn));
            Commands.SetLayer.RegisterCommand(new DelegateCommand<object>(_obj => { }, mustBeInConversation));
            Commands.ImageDropped.RegisterCommand(new DelegateCommand<object>(_obj => { }, mustBeLoggedIn));
            Commands.SetTutorialVisibility.RegisterCommand(new DelegateCommand<object>(vis =>
            {
                if ((Visibility)vis == Visibility.Visible)
                    ShowTutorial();
                else
                    HideTutorial();
            }, mustBeInConversation));
            Logger.Log("Started MeTL");
            Commands.CreateQuiz.RegisterCommand(new DelegateCommand<object>(CreateQuiz, canCreateQuiz));
            Commands.CreateQuiz.RegisterCommand(new DelegateCommand<object>(obj => { }, amAuthor));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<object>(_obj => { }, mustBeLoggedIn));
            Commands.MoveToQuiz.RegisterCommand(new DelegateCommand<QuizDetails>(moveToQuiz));
            Commands.Relogin.RegisterCommand(new DelegateCommand<object>(Relogin));
            Commands.MirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(_o => { }, mustBeInConversation));
            Commands.ProxyMirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(_arg => Commands.MirrorPresentationSpace.Execute(this)));
            Commands.ReceiveWormMove.RegisterCommand(new DelegateCommand<string>(ReceiveWormMove));
            Commands.SetConversationPermissions.RegisterCommand(new DelegateCommand<string>(SetConversationPermissions, CanSetConversationPermissions));
            Commands.AddWindowEffect.RegisterCommand(new DelegateCommand<object>(AddWindowEffect));
            Commands.RemoveWindowEffect.RegisterCommand(new DelegateCommand<object>(RemoveWindowEffect));
            Commands.ReceiveWakeUp.RegisterCommand(new DelegateCommand<object>(wakeUp));
            Commands.ReceiveSleep.RegisterCommand(new DelegateCommand<object>(sleep));
            adornerScroll.scroll = scroll;
            adornerScroll.scroll.SizeChanged += adornerScroll.scrollChanged;
            adornerScroll.scroll.ScrollChanged += adornerScroll.scroll_ScrollChanged;
            AddWindowEffect(null);
            checkIfSmartboard();
        }
        public void checkIfSmartboard()
        {
            var path = "C:\\Program Files\\MeTL\\boardIdentity.txt";
            if (!File.Exists(path)) return;
            MessageBox.Show("logging in as S15");
            JabberWire.SwitchServer("staging");
            var myFile = new StreamReader(path);
            var username = myFile.ReadToEnd();
            Commands.SetIdentity.Execute(new JabberWire.Credentials{authorizedGroups = new List<JabberWire.AuthorizedGroup>(), name= username, password ="examplePassword"});
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
        private void setPrivacy(string privacy)
        {
            this.privacy = privacy;
            showDetails(details);
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
                //startupConversations.Visibility = Visibility.Collapsed;
                userInformation.location.activeConversation = title;
                details = ConversationDetailsProviderFactory.Provider.DetailsOf(userInformation.location.activeConversation);
                showDetails(details);
                applyPermissions(details.Permissions);
                addRecentDocument(details);
                Commands.UpdateConversationDetails.Execute(details);
                Logger.Log("Joined conversation " + title);
                Commands.RequerySuggested(Commands.SetConversationPermissions);
            });
        }
        private void showDetails(ConversationDetails details)
        {
            if (details == null || privacy == null) return;
            var doDetails = (Action)delegate
                                         {
                                             StatusLabel.Text = string.Format("{3} is working {0}ly in {1} style, in a conversation whose participants are {2}",
                                                     privacy, Permissions.InferredTypeOf(details.Permissions).Label, details.Subject, userInformation.credentials.name);
                                             Title = messageFor(details);
                                         };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doDetails);
            else
                doDetails();
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
        private void updateToolBox(string layer)
        {
            if (layer == "Text")
                ModalToolsGroup.Header = "Text Options";
            else if (layer == "Insert")
                ModalToolsGroup.Header = "Image Options";
            else
                ModalToolsGroup.Header = "Ink Options";
        }
        private bool mustBeLoggedIn(object _arg)
        {
            return userInformation.location != null;
        }
        private bool mustBeInConversation(object _arg)
        {
            return mustBeLoggedIn(_arg) && userInformation.location != null && userInformation.location.activeConversation != null;
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (details.Jid != userInformation.location.activeConversation) return;
            var doUpdate = (Action)delegate
            {
                this.details = details;
                userInformation.location.availableSlides = details.Slides.Select(s => s.id).ToList();
                HideTutorial();
                var isAuthor = (details.Author != null) && details.Author == userInformation.credentials.name;
                userInformation.policy.isAuthor = isAuthor;
                showDetails(details);
                Commands.RequerySuggested();
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doUpdate);
            else
                doUpdate();
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
        private void createConversation(ConversationDetails details)
        {
            if (Commands.CreateConversation.CanExecute(details))
            {
                if (details.Tag == null)
                    details.Tag = "unTagged";
                details.Author = userInformation.credentials.name;
                details = ConversationDetailsProviderFactory.Provider.Create(details);
                CommandManager.InvalidateRequerySuggested();
                Create.Clear();
                if (Commands.JoinConversation.CanExecute(details.Jid))
                {
                    Commands.JoinConversation.Execute(details.Jid);
                }
            }
        }
        private void loadPlugins()
        {
            try
            {
                var pluginRoots = new Dictionary<string, IAddChild>
                                      {
                                          {"Sketch", Sketch},
                                          {"PenTools", StableTools},
                                          {"Type", Sketch},
                                          {"Navigate", Navigate},
                                          {"ListConversations", ConversationListing}
                                      };
                var loadedPlugins = PluginManager.LoadPlugins(pluginRoots, pluginRoot);
                foreach (var loadedPlugin in loadedPlugins)
                {
                    FrameworkElement loadedPlugin1 = loadedPlugin;
                    var coreModule = DEFAULT_COMPONENTS.Keys.FirstOrDefault(k => k.IsAssignableFrom(loadedPlugin1.GetType()));
                    if (coreModule != null)
                    {
                        GetType().GetProperties().Single(f => f.PropertyType == coreModule).SetValue(this, loadedPlugin, new object[0]);
                    }
                }
            }
            catch (InvalidPluginFormatException e)
            {
                MessageBox.Show("Plugin loading has been aborted because of a bad plugin:\n" + e.Message);
            }
        }
        private void loadRemainingCoreComponents()
        {
            foreach (var coreModuleSlot in DEFAULT_COMPONENTS.Keys)
            {
                Type coreModuleSlot1 = coreModuleSlot;
                var slot = GetType().GetProperties().Single(f => f.PropertyType == coreModuleSlot1);
                if (slot.GetValue(this, new object[0]) == null)
                {
                    slot.SetValue(this, DEFAULT_COMPONENTS[coreModuleSlot](), new object[0]);
                }
            }
        }
        private void debugTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            Console.WriteLine(sender.GetType());
        }
        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            recentDocuments.ListRecentConversations();
            ApplicationButtonPopup.Opened += ApplicationButtonPopup_Opened;
            ApplicationButtonPopup.Closed += ApplicationButtonPopup_Closed;
        }
        private void addRecentDocument(ConversationDetails document)
        {
            RecentConversationProvider.addRecentConversation(document, userInformation.credentials.name);
            recentDocuments.ListRecentConversations();
        }
        private void canCloseApplication(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
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
        private void RestoreView(object sender, RoutedEventArgs e)
        {
            if (scroll != null && scroll.Height > 0 && scroll.Width > 0)
            {
                scroll.Height = scroll.ExtentHeight;
                scroll.Width = scroll.ExtentWidth;
                scroll.Height = double.NaN;
                scroll.Width = double.NaN;
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
        #region helpLinks
        private void OpenEULABrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://metl.adm.monash.edu.au/MeTL/docs/tabletSupport/MLS_UserAgreement.html");
        }
        private void OpenTutorialBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://metl.adm.monash.edu.au/MeTL/docs/tabletSupport/MLS_Tutorials.html");
        }
        private void OpenReportBugBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://metl.adm.monash.edu.au/MeTL/docs/report_a_bug.html");
        }
        private void OpenAboutMeTLBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.monash.edu.au/eeducation/myls2010/students/resources/software/metl/");
        }
        #endregion
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
        private void Viewbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(minimap);
            var x = (pos.X / minimap.Width) * scroll.ExtentWidth;
            var y = (pos.Y / minimap.Height) * scroll.ExtentHeight;
            if (new[] { x, y }.Any(i => Double.IsNaN(i))) return;
            var viewBoxXOffset = scroll.ViewportWidth / 2;
            var viewBoxYOffset = scroll.ViewportHeight / 2;
            var finalX = x - viewBoxXOffset;
            if (!(finalX > 0))
                finalX = 0;
            scroll.ScrollToHorizontalOffset(finalX);
            var finalY = y - viewBoxYOffset;
            if (!(finalY > 0))
                finalY = 0;
            scroll.ScrollToVerticalOffset(finalY);
        }
        private void Viewbox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(minimap);
                var x = (pos.X / minimap.Width) * scroll.ExtentWidth;
                var y = (pos.Y / minimap.Height) * scroll.ExtentHeight;
                if (new[] { x, y }.Any(i => Double.IsNaN(i))) return;
                var viewBoxXOffset = scroll.ViewportWidth / 2;
                var viewBoxYOffset = scroll.ViewportHeight / 2;
                var finalX = x - viewBoxXOffset;
                if (!(finalX > 0))
                    finalX = 0;
                scroll.ScrollToHorizontalOffset(finalX);
                var finalY = y - viewBoxYOffset;
                if (!(finalY > 0))
                    finalY = 0;
                scroll.ScrollToVerticalOffset(finalY);
            }
        }
        private void HighlightFriend(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.PostHighlightFriend.Execute(e.Parameter, canvas);
        }
        private void HighlightUser(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.PostHighlightUser.Execute(e.Parameter, (IInputElement)friendDisplay);
        }
        public Visibility GetVisibilityOf(UIElement target)
        {
            return target.Visibility;
        }
        private void RibbonPopup_BeforeOpenConversationCreator(object sender, BeforeOpenEventArgs e)
        {
            var creator = (SimpleConversationCreator)((RibbonPopup)sender).Items.GetItemAt(0);
            creator.create.RaiseCanExecuteChanged();
        }
        private void RibbonPopup_BeforeOpenConversationEditor(object sender, BeforeOpenEventArgs e)
        {
            if (userInformation.location == null ||
                userInformation.location.activeConversation == null ||
                !userInformation.policy.isAuthor)
                e.Cancel = true;
            else
            {
                var editor = (SimpleConversationEditor)((RibbonPopup)sender).Items.GetItemAt(0);
                var conversation = ConversationDetailsProviderFactory.Provider.DetailsOf(userInformation.location.activeConversation);
                editor.DataContext = conversation;
                conversation.Refresh();
            }
        }
        private void ApplicationButtonPopup_Closed(object sender, EventArgs e)
        {
            HideTutorial();
        }
        private void ApplicationButtonPopup_Opened(object sender, EventArgs e)
        {
            ShowTutorial();
            Commands.RequerySuggested(Commands.MirrorPresentationSpace);
        }
        private void CreateOpened(object sender, EventArgs e)
        {
            Create.FocusCreate();
        }
        private void EditOpened(object sender, EventArgs e)
        {
            Edit.FocusEdit();
        }
        private void SearchOpened(object sender, EventArgs e)
        {
            Search.FocusSearch();
        }
        private void ImportOpened(object sender, EventArgs e)
        {
            Import.FocusCreate();
        }
        private void SetConversationPermissions(string style)
        {
            if (style == "tutorial")
            {
                details.Permissions.studentCanPublish = true;
                details.Permissions.studentCanOpenFriends = true;
                details.Permissions.usersAreCompulsorilySynced = false;
            }
            else if (style == "lecture")
            {
                details.Permissions.studentCanPublish = false;
                details.Permissions.studentCanOpenFriends = false;
                details.Permissions.usersAreCompulsorilySynced = true;
            }
            ConversationDetailsProviderFactory.Provider.Update(details);
        }
        private bool CanSetConversationPermissions(string _style)
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
    }
}