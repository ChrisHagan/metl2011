using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using MeTLLib.DataTypes;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace SandRibbon
{
    public class NotSetException : Exception
    {
        public NotSetException(string msg) : base(msg) { }
    }
    public class Registrations : DependencyObject
    {
        private static Dictionary<string, int> registrations = new Dictionary<string, int>();
        public Dictionary<string, int> Commands
        {
            get { return (Dictionary<string, int>)GetValue(CommandsProperty); }
            set { SetValue(CommandsProperty, value); }
        }
        public static readonly DependencyProperty CommandsProperty =
            DependencyProperty.Register("Commands", typeof(Dictionary<string, int>), typeof(Registrations), new PropertyMetadata(new Dictionary<string, int>()));

        public void add(string key) {
            if (!registrations.ContainsKey(key)) registrations.Add(key, 0);
            registrations[key]++;
            Dispatcher.Invoke(()=> Commands = registrations);
        }

        public void remove(string key) {
            if (registrations.ContainsKey(key))
            {
                registrations[key]--;
                Dispatcher.Invoke(() => Commands = registrations);
            }
        }
    }
    public class DefaultableCompositeCommand : CompositeCommand
    {
        public static Registrations Registrations {
            get; set;
        }
        static DefaultableCompositeCommand()
        {
            Registrations = new Registrations();
        }
        private bool isSet = false;
        private object commandValue = null;
        public string Label { get; set; }
        public object DefaultValue
        {
            get
            {
                Debug.Assert(isSet, "Default value has not been set");
                return commandValue;
            }
            set
            {
                isSet = true;
                commandValue = value;
            }
        }        

        public DefaultableCompositeCommand(string label)
        {
            this.Label = label;
        }

        public DefaultableCompositeCommand(string label, object newValue)
        {
            this.Label = label;
            DefaultValue = newValue;
        }

        public bool IsInitialised
        {
            get
            {
                return isSet;
            }
        }

        public object LastValue()
        {
            Debug.Assert(isSet, "Default value has not been set");
            return DefaultValue;
        }
        public override void Execute(object arg)
        {
            DefaultValue = arg;
            base.Execute(arg);
        }
        private int skipFrames = 2;
        public override void RegisterCommand(ICommand command)
        {
            if (RegisteredCommands.Contains(command)) return;
            StackFrame frame = new StackFrame(skipFrames);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var name = method.Name;            
            var key = string.Format("{0}.{1} -> {2}", type.Name, name, Label);
            Registrations.add(key);       
            base.RegisterCommand(command);
        }
        public override void UnregisterCommand(ICommand command)
        {
            StackFrame frame = new StackFrame(skipFrames);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var name = method.Name;
            var c = command.ToString();
            var key = string.Format("{0}.{1} -> {2}", type.Name, name, Label);
            Registrations.remove(key);            
            base.UnregisterCommand(command);
        }
    }
    public class Commands
    {
        public static DefaultableCompositeCommand Mark = new DefaultableCompositeCommand("Mark");

        public static DefaultableCompositeCommand WordCloud = new DefaultableCompositeCommand("WordCloud");
        public static DefaultableCompositeCommand BrowseOneNote = new DefaultableCompositeCommand("BrowseOneNote");
        public static DefaultableCompositeCommand ManuallyConfigureOneNote = new DefaultableCompositeCommand("ManuallyConfigureOneNote");
        public static DefaultableCompositeCommand SerializeConversationToOneNote = new DefaultableCompositeCommand("SerializeConversationToOneNote");
        public static DefaultableCompositeCommand RequestMeTLUserInformations = new DefaultableCompositeCommand("RequestMeTLUserInformation");
        public static DefaultableCompositeCommand MoveToNotebookPage = new DefaultableCompositeCommand("MoveToNotebookPage");
        public static DefaultableCompositeCommand ReceiveMeTLUserInformations = new DefaultableCompositeCommand("ReceiveMeTLUserInformations");
        public static DefaultableCompositeCommand RequestTeacherStatus = new DefaultableCompositeCommand("RequestTeacherStatus");
        public static DefaultableCompositeCommand ReceiveTeacherStatus = new DefaultableCompositeCommand("ReceiveTeacherStatus");
        public static DefaultableCompositeCommand SetStackVisibility = new DefaultableCompositeCommand("SetStackVisibility");
        public static DefaultableCompositeCommand SendNewSlideOrder = new DefaultableCompositeCommand("SendNewSlideOrder");
        public static DefaultableCompositeCommand ChangeLanguage = new DefaultableCompositeCommand("ChangeLanguage");
        public static DefaultableCompositeCommand PresentVideo = new DefaultableCompositeCommand("PresentVideo");
        public static DefaultableCompositeCommand ReorderDragDone = new DefaultableCompositeCommand("ReorderDragDone");
        public static DefaultableCompositeCommand ConnectToSmartboard = new DefaultableCompositeCommand("ConnectToSmartboard");
        public static DefaultableCompositeCommand DisconnectFromSmartboard = new DefaultableCompositeCommand("DisconnectFromSmartboard");
        public static DefaultableCompositeCommand ViewSubmissions = new DefaultableCompositeCommand("ViewSubmissions");
        public static DefaultableCompositeCommand ViewBannedContent = new DefaultableCompositeCommand("ViewBannedContent");
        public static DefaultableCompositeCommand Reconnecting = new DefaultableCompositeCommand("Reconnecting");
        public static DefaultableCompositeCommand ShuttingDown = new DefaultableCompositeCommand("LeaveAllRooms");
        public static DefaultableCompositeCommand BackstageModeChanged = new DefaultableCompositeCommand("BackstageModeChanged");
        public static DefaultableCompositeCommand UpdatePowerpointProgress = new DefaultableCompositeCommand("UpdatePowerpointProgress");
        public static DefaultableCompositeCommand ShowOptionsDialog = new DefaultableCompositeCommand("ShowOptionsDialog");
        public static DefaultableCompositeCommand SetUserOptions = new DefaultableCompositeCommand("SetUserOptions");
        public static DefaultableCompositeCommand ZoomChanged = new DefaultableCompositeCommand("ZoomChanged");

        public static DefaultableCompositeCommand ExtendCanvasBySize = new DefaultableCompositeCommand("ExtendCanvasBySize");
        public static DefaultableCompositeCommand ExtendCanvasUp = new DefaultableCompositeCommand("ExtendCanvasUp");
        public static DefaultableCompositeCommand ExtendCanvasDown = new DefaultableCompositeCommand("ExtendCanvasDown");

        public static DefaultableCompositeCommand CheckExtendedDesktop = new DefaultableCompositeCommand("CheckExtendedDesktop");

        public static DefaultableCompositeCommand AddPrivacyToggleButton = new DefaultableCompositeCommand("AddPrivacyToggleButton");
        public static DefaultableCompositeCommand RemovePrivacyAdorners = new DefaultableCompositeCommand("RemovePrivacyAdorners");
        public static DefaultableCompositeCommand MirrorVideo = new DefaultableCompositeCommand("MirrorVideo");
        public static DefaultableCompositeCommand VideoMirrorRefreshRectangle = new DefaultableCompositeCommand("VideoMirrorRefreshRectangle");

        public static DefaultableCompositeCommand AnalyzeSelectedConversations = new DefaultableCompositeCommand("AnalyzeSelectedConversations");
        public static DefaultableCompositeCommand SendWakeUp = new DefaultableCompositeCommand("SendWakeUp");
        public static DefaultableCompositeCommand SendSleep = new DefaultableCompositeCommand("SendSleep");
        public static DefaultableCompositeCommand ReceiveWakeUp = new DefaultableCompositeCommand("ReceiveWakeUp");
        public static DefaultableCompositeCommand ReceiveSleep = new DefaultableCompositeCommand("ReceiveSleep");
        public static DefaultableCompositeCommand SendMoveBoardToSlide = new DefaultableCompositeCommand("SendMoveBoardToSlide");
        public static DefaultableCompositeCommand ReceiveMoveBoardToSlide = new DefaultableCompositeCommand("ReceiveMoveBoardToSlide");
        public static DefaultableCompositeCommand SendPing = new DefaultableCompositeCommand("SendPing");
        public static DefaultableCompositeCommand ReceivePong = new DefaultableCompositeCommand("ReceivePong");
        public static DefaultableCompositeCommand DoWithCurrentSelection = new DefaultableCompositeCommand("DoWithCurrentSelection");
        public static DefaultableCompositeCommand BubbleCurrentSelection = new DefaultableCompositeCommand("BubbleCurrentSelection");
        public static DefaultableCompositeCommand ReceiveNewBubble = new DefaultableCompositeCommand("ReceiveNewBubble");
        public static DefaultableCompositeCommand ExploreBubble = new DefaultableCompositeCommand("ExploreBubble");
        public static DefaultableCompositeCommand ThoughtLiveWindow = new DefaultableCompositeCommand("ThoughtLiveWindow");
        public static DefaultableCompositeCommand SetZoomRect = new DefaultableCompositeCommand("SetZoomRect");
        public static DefaultableCompositeCommand Highlight = new DefaultableCompositeCommand("Highlight");
        public static DefaultableCompositeCommand RemoveHighlight = new DefaultableCompositeCommand("RemoveHighlight");

        public static DefaultableCompositeCommand ServersDown = new DefaultableCompositeCommand("ServersDown");
        public static DefaultableCompositeCommand RequestScreenshotSubmission = new DefaultableCompositeCommand("RequestScreenshotSubmission");
        public static DefaultableCompositeCommand GenerateScreenshot = new DefaultableCompositeCommand("GenerateScreenshot");
        public static DefaultableCompositeCommand ScreenshotGenerated = new DefaultableCompositeCommand("ScreenshotGenerated");
        public static DefaultableCompositeCommand SendScreenshotSubmission = new DefaultableCompositeCommand("SendScreenshotSubmission");
        public static DefaultableCompositeCommand ReceiveScreenshotSubmission = new DefaultableCompositeCommand("ReceiveScreenshotSubmission");
        public static DefaultableCompositeCommand ImportSubmission = new DefaultableCompositeCommand("ImportSubmission");
        public static DefaultableCompositeCommand ImportSubmissions = new DefaultableCompositeCommand("ImportSubmissions");
        public static DefaultableCompositeCommand SaveFile = new DefaultableCompositeCommand("SaveFile");
        public static DefaultableCompositeCommand DummyCommandToProcessCanExecuteForTextTools = new DefaultableCompositeCommand("DummyCommandToProcessCanExecuteForTextTools");
        public static DefaultableCompositeCommand DummyCommandToProcessCanExecuteForPrivacyTools = new DefaultableCompositeCommand("DummyCommandToProcessCanExecuteForPrivacyTools");

        public static DefaultableCompositeCommand PublishBrush = new DefaultableCompositeCommand("PublishBrush");
        public static DefaultableCompositeCommand ModifySelection = new DefaultableCompositeCommand("ModifySelection");
        public static DefaultableCompositeCommand TogglePens = new DefaultableCompositeCommand("TogglePens");
        public static DefaultableCompositeCommand SetPedagogyLevel = new DefaultableCompositeCommand("SetPedagogyLevel");
        public static DefaultableCompositeCommand GetMainScrollViewer = new DefaultableCompositeCommand("GetMainScrollViewer");
        public static DefaultableCompositeCommand ShowConversationSearchBox = new DefaultableCompositeCommand("ShowConversationSearchBox");
        public static DefaultableCompositeCommand HideConversationSearchBox = new DefaultableCompositeCommand("HideConversationSearchBox");
        public static DefaultableCompositeCommand AddWindowEffect = new DefaultableCompositeCommand("AddWindowEffect");
        public static DefaultableCompositeCommand RemoveWindowEffect = new DefaultableCompositeCommand("RemoveWindowEffect");
        public static DefaultableCompositeCommand NotImplementedYet = new DefaultableCompositeCommand("NotImplementedYet");
        public static DefaultableCompositeCommand NoOp = new DefaultableCompositeCommand("NoOp");
        public static DefaultableCompositeCommand MirrorPresentationSpace = new DefaultableCompositeCommand("MirrorPresentationSpace");
        public static DefaultableCompositeCommand ProxyMirrorPresentationSpace = new DefaultableCompositeCommand("ProxyMirrorPresentationSpace");
        public static DefaultableCompositeCommand InitiateDig = new DefaultableCompositeCommand("InitiateDig");
        public static DefaultableCompositeCommand SendDig = new DefaultableCompositeCommand("SendDig");
        public static DefaultableCompositeCommand DugPublicSpace = new DefaultableCompositeCommand("DugPublicSpace");
        public static DefaultableCompositeCommand SendLiveWindow = new DefaultableCompositeCommand("SendLiveWindow");
        public static DefaultableCompositeCommand SendDirtyLiveWindow = new DefaultableCompositeCommand("SendDirtyLiveWindow");
        public static DefaultableCompositeCommand ReceiveLiveWindow = new DefaultableCompositeCommand("ReceiveLiveWindow");
        public static DefaultableCompositeCommand ReceiveDirtyLiveWindow = new DefaultableCompositeCommand("ReceiveDirtyLiveWindow");
        public static DefaultableCompositeCommand DeleteSelectedItems = new DefaultableCompositeCommand("DeleteSelectedItems");
        public static DefaultableCompositeCommand BanhammerSelectedItems = new DefaultableCompositeCommand("BanhammerSelectedItems");
        public static DefaultableCompositeCommand VisualizeContent = new DefaultableCompositeCommand("VisualiseContent");

        public static DefaultableCompositeCommand SendAttendance = new DefaultableCompositeCommand("SendAttendance");
        public static DefaultableCompositeCommand ReceiveAttendance = new DefaultableCompositeCommand("ReceiveAttendance");

        //public static DefaultableCompositeCommand Relogin = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ToggleWorm = new DefaultableCompositeCommand("ToggleWorm");
        public static DefaultableCompositeCommand SendWormMove = new DefaultableCompositeCommand("SendWormMove");
        public static DefaultableCompositeCommand ReceiveWormMove = new DefaultableCompositeCommand("ReceiveWormMove");
        public static DefaultableCompositeCommand ConvertPresentationSpaceToQuiz = new DefaultableCompositeCommand("ConvertPresentationSpaceToQuiz");
        public static DefaultableCompositeCommand SendQuiz = new DefaultableCompositeCommand("SendQuiz");
        public static DefaultableCompositeCommand SendQuizAnswer = new DefaultableCompositeCommand("SendQuizAnswer");
        public static DefaultableCompositeCommand ReceiveQuiz = new DefaultableCompositeCommand("ReceiveQuiz");
        public static DefaultableCompositeCommand ReceiveQuizAnswer = new DefaultableCompositeCommand("ReceiveQuizAnswer");
        public static DefaultableCompositeCommand DisplayQuizResults = new DefaultableCompositeCommand("DisplayQuizResults");
        public static DefaultableCompositeCommand QuizResultsAvailableForSnapshot = new DefaultableCompositeCommand("QuizResultsAvailableForSnapshot");
        public static DefaultableCompositeCommand QuizResultsSnapshotAvailable = new DefaultableCompositeCommand("QuizResultsSnapshotAvailable");
        //public static DefaultableCompositeCommand PlaceQuizSnapshot = new DefaultableCompositeCommand();

        public static DefaultableCompositeCommand ShowDiagnostics = new DefaultableCompositeCommand("ShowDiagnostics");
        public static DefaultableCompositeCommand SetInkCanvasMode = new DefaultableCompositeCommand("SetInkCanvasMode");
        public static DefaultableCompositeCommand SetPrivacyOfItems = new DefaultableCompositeCommand("SetPrivacyOfItems");
        public static DefaultableCompositeCommand GotoThread = new DefaultableCompositeCommand("GotoThread");
        public static DefaultableCompositeCommand SetDrawingAttributes = new DefaultableCompositeCommand("SetDrawingAttributes");
        public static DefaultableCompositeCommand SetPenAttributes = new DefaultableCompositeCommand("SetPenAttributes");
        public static DefaultableCompositeCommand ReplacePenAttributes = new DefaultableCompositeCommand("ReplacePenAttributes");
        public static DefaultableCompositeCommand RequestReplacePenAttributes = new DefaultableCompositeCommand("RequestReplacePenAttributes");
        public static DefaultableCompositeCommand RequestResetPenAttributes = new DefaultableCompositeCommand("RequestResetPenAttributes");


        public static DefaultableCompositeCommand SendStroke = new DefaultableCompositeCommand("SendStroke");
        public static DefaultableCompositeCommand ReceiveStroke = new DefaultableCompositeCommand("ReceiveStroke");
        public static DefaultableCompositeCommand ReceiveStrokes = new DefaultableCompositeCommand("ReceiveStrokes");
        public static DefaultableCompositeCommand SendDirtyStroke = new DefaultableCompositeCommand("SendDirtyStroke");
        public static DefaultableCompositeCommand ReceiveDirtyStrokes = new DefaultableCompositeCommand("ReceiveDirtyStrokes");
        public static DefaultableCompositeCommand SetPrivacy = new DefaultableCompositeCommand("SetPrivacy");
        public static DefaultableCompositeCommand SetContentVisibility = new DefaultableCompositeCommand("SetContentVisibility");
        public static DefaultableCompositeCommand UpdateContentVisibility = new DefaultableCompositeCommand("UpdateContentVisibility");
        public static DefaultableCompositeCommand ForcePageRefresh = new DefaultableCompositeCommand("ForcePageRefresh");
        public static DefaultableCompositeCommand OriginalView = new DefaultableCompositeCommand("OriginalView");
        public static DefaultableCompositeCommand CreateQuizStructure = new DefaultableCompositeCommand("CreateQuizStructure");
        public static DefaultableCompositeCommand Flush = new DefaultableCompositeCommand("Flush");
        public static DefaultableCompositeCommand ZoomIn = new DefaultableCompositeCommand("ZoomIn");
        public static DefaultableCompositeCommand ZoomOut = new DefaultableCompositeCommand("ZoomOut");
        public static DefaultableCompositeCommand ExtendCanvasBothWays = new DefaultableCompositeCommand("ExtendCanvasBothWays");
        public static DefaultableCompositeCommand ToggleBrowser = new DefaultableCompositeCommand("ToggleBrowser");
        public static DefaultableCompositeCommand ToggleBrowserControls = new DefaultableCompositeCommand("ToggleBrowserControls");
        public static DefaultableCompositeCommand MoreImageOptions = new DefaultableCompositeCommand("MoreImageOptions");
        public static DefaultableCompositeCommand PickImages = new DefaultableCompositeCommand("PickImages");
        public static DefaultableCompositeCommand ImageDropped = new DefaultableCompositeCommand("ImageDropped");
        public static DefaultableCompositeCommand ImagesDropped = new DefaultableCompositeCommand("ImagesDropped");
        public static DefaultableCompositeCommand AddImage = new DefaultableCompositeCommand("AddImage");
        public static DefaultableCompositeCommand FileUpload = new DefaultableCompositeCommand("FileUpload");
        public static DefaultableCompositeCommand SendImage = new DefaultableCompositeCommand("SendImage");
        public static DefaultableCompositeCommand ReceiveImage = new DefaultableCompositeCommand("ReceiveImage");
        public static DefaultableCompositeCommand SendMoveDelta = new DefaultableCompositeCommand("SendMoveDelta");
        public static DefaultableCompositeCommand ReceiveMoveDelta = new DefaultableCompositeCommand("ReceiveMoveDelta");
        public static DefaultableCompositeCommand SendDirtyImage = new DefaultableCompositeCommand("SendDirtyImage");
        public static DefaultableCompositeCommand ReceiveDirtyImage = new DefaultableCompositeCommand("ReceiveDirtyImage");
        public static DefaultableCompositeCommand SendDirtyAutoShape = new DefaultableCompositeCommand("SendDirtyAutoShape");
        public static DefaultableCompositeCommand ReceiveAutoShape = new DefaultableCompositeCommand("ReceiveAutoShape");
        public static DefaultableCompositeCommand ReceiveDirtyAutoShape = new DefaultableCompositeCommand("ReceiveDirtyAutoShape");

        /*These are fired after the content buffers have compensated for negative cartesian space and checked ownership*/
        public static DefaultableCompositeCommand StrokePlaced = new DefaultableCompositeCommand("StrokePlaced");
        public static DefaultableCompositeCommand ImagePlaced = new DefaultableCompositeCommand("ImagePlaced");
        public static DefaultableCompositeCommand TextPlaced = new DefaultableCompositeCommand("TextPlaced");
        public static DefaultableCompositeCommand ToggleLens = new DefaultableCompositeCommand("ToggleLens");

        public static DefaultableCompositeCommand SendFileResource = new DefaultableCompositeCommand("SendFileResource");
        public static DefaultableCompositeCommand ReceiveFileResource = new DefaultableCompositeCommand("ReceiveFileResource");

        /*
        public static DefaultableCompositeCommand FontSizeChanged = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand FontChanged = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetTextColor = new DefaultableCompositeCommand();        
        */
        public static DefaultableCompositeCommand ChangeTextMode = new DefaultableCompositeCommand("ChangeTextMode");
        public static DefaultableCompositeCommand TextboxFocused = new DefaultableCompositeCommand("TextboxFocused");
        public static DefaultableCompositeCommand TextboxSelected = new DefaultableCompositeCommand("TextboxSelected");
        public static DefaultableCompositeCommand SendDirtyText = new DefaultableCompositeCommand("SendDirtyText");
        public static DefaultableCompositeCommand ReceiveDirtyText = new DefaultableCompositeCommand("ReceiveDirtyText");
        public static DefaultableCompositeCommand SetTextCanvasMode = new DefaultableCompositeCommand("SetTextCanvasMode");
        public static DefaultableCompositeCommand IncreaseFontSize = new DefaultableCompositeCommand("IncreaseFontSize");
        public static DefaultableCompositeCommand DecreaseFontSize = new DefaultableCompositeCommand("DecreaseFontSize");
        public static DefaultableCompositeCommand SendTextBox = new DefaultableCompositeCommand("SendTextBox");
        public static DefaultableCompositeCommand ReceiveTextBox = new DefaultableCompositeCommand("ReceiveTextBox");
        public static DefaultableCompositeCommand RestoreTextDefaults = new DefaultableCompositeCommand("RestoreTextDefaults");
        public static DefaultableCompositeCommand NewTextCursorPosition = new DefaultableCompositeCommand("NewTextCursorPosition");
        public static DefaultableCompositeCommand InitiateGrabZoom = new DefaultableCompositeCommand("InitiateGrabZoom");
        public static DefaultableCompositeCommand EndGrabZoom = new DefaultableCompositeCommand("EndGrabZoom");
        public static DefaultableCompositeCommand MoveCanvasByDelta = new DefaultableCompositeCommand("MoveCanvasDelta");
        public static DefaultableCompositeCommand FitToView = new DefaultableCompositeCommand("FitToView");
        public static DefaultableCompositeCommand FitToPageWidth = new DefaultableCompositeCommand("FitToPageWidth");
        public static DefaultableCompositeCommand UpdateTextStyling = new DefaultableCompositeCommand("UpdateTextStyling");

        public static DefaultableCompositeCommand SetFontSize = new DefaultableCompositeCommand("SetFontSize");
        public static DefaultableCompositeCommand SetFont = new DefaultableCompositeCommand("SetFont");
        public static DefaultableCompositeCommand SetTextColor = new DefaultableCompositeCommand("SetTextColor");
        public static DefaultableCompositeCommand SetTextBold = new DefaultableCompositeCommand("SetTextBold");
        public static DefaultableCompositeCommand SetTextItalic = new DefaultableCompositeCommand("SetTextItalic");
        public static DefaultableCompositeCommand SetTextUnderline = new DefaultableCompositeCommand("SetTextUnderline");
        public static DefaultableCompositeCommand SetTextStrikethrough = new DefaultableCompositeCommand("SetTextStrikethrough");
        public static DefaultableCompositeCommand FontSizeNotify = new DefaultableCompositeCommand("FontSizeNotify");
        public static DefaultableCompositeCommand FontNotify = new DefaultableCompositeCommand("FontNotify");
        public static DefaultableCompositeCommand TextColorNotify = new DefaultableCompositeCommand("TextColorNotify");
        public static DefaultableCompositeCommand TextBoldNotify = new DefaultableCompositeCommand("TextBoldNotify");
        public static DefaultableCompositeCommand TextItalicNotify = new DefaultableCompositeCommand("TextItalicNotify");
        public static DefaultableCompositeCommand TextUnderlineNotify = new DefaultableCompositeCommand("TextUnderlineNotify");
        public static DefaultableCompositeCommand TextStrikethroughNotify = new DefaultableCompositeCommand("TextStrikethroughNotify");

        public static DefaultableCompositeCommand ToggleBold = new DefaultableCompositeCommand("ToggleBold");
        public static DefaultableCompositeCommand ToggleItalic = new DefaultableCompositeCommand("ToggleItalic");
        public static DefaultableCompositeCommand ToggleUnderline = new DefaultableCompositeCommand("ToggleUnderline");
        public static DefaultableCompositeCommand ToggleStrikethrough = new DefaultableCompositeCommand("ToggleStrikethrough");

        public static DefaultableCompositeCommand MoreTextOptions = new DefaultableCompositeCommand("MoreTextOptions");
        public static DefaultableCompositeCommand RegisterPowerpointSourceDirectoryPreference = new DefaultableCompositeCommand("RegisterPowerpointSourceDirectoryPreference");
        public static DefaultableCompositeCommand MeTLType = new DefaultableCompositeCommand("MeTLType");
        public static DefaultableCompositeCommand LogOut = new DefaultableCompositeCommand("LogOut");
        public static DefaultableCompositeCommand BackendSelected = new DefaultableCompositeCommand("BackendSelected");
        public static DefaultableCompositeCommand LoginFailed = new DefaultableCompositeCommand("LoginFailed");
        public static DefaultableCompositeCommand SetIdentity = new DefaultableCompositeCommand("SetIdentity",Credentials.Empty);
        public static DefaultableCompositeCommand NoNetworkConnectionAvailable = new DefaultableCompositeCommand("NetworkConnectionAvailable");
        public static DefaultableCompositeCommand EstablishPrivileges = new DefaultableCompositeCommand("EstablishPrivileges");
        public static DefaultableCompositeCommand CloseApplication = new DefaultableCompositeCommand("CloseApplication");
        public static DefaultableCompositeCommand SetLayer = new DefaultableCompositeCommand("SetLayer");
        public static DefaultableCompositeCommand UpdateForeignConversationDetails = new DefaultableCompositeCommand("UpdateForeignConversationDetails");
        public static DefaultableCompositeCommand RememberMe = new DefaultableCompositeCommand("RememberMe",false);
        public static DefaultableCompositeCommand ClipboardManager = new DefaultableCompositeCommand("ClipboardManager");

        public static DefaultableCompositeCommand Undo = new DefaultableCompositeCommand("Undo");
        public static DefaultableCompositeCommand Redo = new DefaultableCompositeCommand("Redo");
        public static DefaultableCompositeCommand ChangeTab = new DefaultableCompositeCommand("ChangeTab");
        public static DefaultableCompositeCommand SetRibbonAppearance = new DefaultableCompositeCommand("SetRibbonAppearance");
        public static DefaultableCompositeCommand SaveUIState = new DefaultableCompositeCommand("SaveUIState");
        public static DefaultableCompositeCommand RestoreUIState = new DefaultableCompositeCommand("RestoreUIState");
        /*Moving is a metaphor which implies that I am only in one location.  Watching can happen to many places.*/
        public static DefaultableCompositeCommand WatchRoom = new DefaultableCompositeCommand("WatchRoom");

        public static DefaultableCompositeCommand MovingTo = new DefaultableCompositeCommand("Moving to");
        public static DefaultableCompositeCommand JoiningConversation = new DefaultableCompositeCommand("JoinConversation");

        public static DefaultableCompositeCommand SyncedMoveRequested = new DefaultableCompositeCommand("SyncedMoveRequested",0);
        public static DefaultableCompositeCommand SendSyncMove = new DefaultableCompositeCommand("SendSyncMove");
        public static DefaultableCompositeCommand MoveToCollaborationPage = new DefaultableCompositeCommand("MoveToCollaborationPage",0);
        public static DefaultableCompositeCommand PreParserAvailable = new DefaultableCompositeCommand("PreParserAvaialble");
        public static DefaultableCompositeCommand SignedRegions = new DefaultableCompositeCommand("SignedRegions");
        public static DefaultableCompositeCommand ConversationPreParserAvailable = new DefaultableCompositeCommand("ConversationPreParserAvailable");
        public static DefaultableCompositeCommand MoveToPrevious = new DefaultableCompositeCommand("MoveToPrevious");
        public static DefaultableCompositeCommand MoveToNext = new DefaultableCompositeCommand("MoveToNext");
        public static DefaultableCompositeCommand SetConversationPermissions = new DefaultableCompositeCommand("SetConversationPermissions");
        public static DefaultableCompositeCommand ToggleNavigationLock = new DefaultableCompositeCommand("ToggleNavigationLock");
        public static DefaultableCompositeCommand LeaveConversation = new DefaultableCompositeCommand("LeaveConversation");
        public static DefaultableCompositeCommand LeaveLocation = new DefaultableCompositeCommand("LeaveLocation");
        public static DefaultableCompositeCommand SendDirtyConversationDetails = new DefaultableCompositeCommand("SendDirtyConversationDetails");
        public static DefaultableCompositeCommand UpdateConversationDetails = new DefaultableCompositeCommand("UpdateConversationDetails",ConversationDetails.Empty);
        public static DefaultableCompositeCommand ReceiveDirtyConversationDetails = new DefaultableCompositeCommand("ReceiveDirtyConversationDetails");
        public static DefaultableCompositeCommand SetSync = new DefaultableCompositeCommand("SetSync",false);
        public static DefaultableCompositeCommand ToggleSync = new DefaultableCompositeCommand("ToggleSync");
        public static DefaultableCompositeCommand AddSlide = new DefaultableCompositeCommand("AddSlide");
        public static DefaultableCompositeCommand UpdateNewSlideOrder = new DefaultableCompositeCommand("UpdateNewSlideOrder");
        public static DefaultableCompositeCommand CreateBlankConversation = new DefaultableCompositeCommand("CreateBlankConversation");
        public static DefaultableCompositeCommand ShowEditSlidesDialog = new DefaultableCompositeCommand("ShowEditSlidesDialog");
        public static DefaultableCompositeCommand CreateConversation = new DefaultableCompositeCommand("CreateConversation");
        public static DefaultableCompositeCommand DuplicateSlide = new DefaultableCompositeCommand("DuplicateSlide",new KeyValuePair<ConversationDetails, Slide>(ConversationDetails.Empty, Slide.Empty));
        public static DefaultableCompositeCommand DuplicateConversation = new DefaultableCompositeCommand("DuplicateConversation",ConversationDetails.Empty);
        public static DefaultableCompositeCommand CreateGrouping = new DefaultableCompositeCommand("CreateGrouping");
        //public static DefaultableCompositeCommand PreEditConversation = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand EditConversation = new DefaultableCompositeCommand("EditConversation");
        public static DefaultableCompositeCommand BlockInput = new DefaultableCompositeCommand("BlockInput");
        public static DefaultableCompositeCommand UnblockInput = new DefaultableCompositeCommand("UnblockInput");
        public static DefaultableCompositeCommand BlockSearch = new DefaultableCompositeCommand("BlockSearch");
        public static DefaultableCompositeCommand UnblockSearch = new DefaultableCompositeCommand("UnblockSearch");
        public static DefaultableCompositeCommand CanEdit = new DefaultableCompositeCommand("CanEdit");
        public static DefaultableCompositeCommand PrintConversation = new DefaultableCompositeCommand("PrintConversation");
        public static DefaultableCompositeCommand HideProgressBlocker = new DefaultableCompositeCommand("HideProgressBlocker");
        public static DefaultableCompositeCommand SendChatMessage = new DefaultableCompositeCommand("SendChatMessage");
        public static DefaultableCompositeCommand ReceiveChatMessage = new DefaultableCompositeCommand("ReceiveChatMessage");
        public static DefaultableCompositeCommand BanhammerActive = new DefaultableCompositeCommand("BanhammerActive");
        public static DefaultableCompositeCommand ManageBannedContent = new DefaultableCompositeCommand("ManageBannedContent");

        public static DefaultableCompositeCommand ImportPowerpoint = new DefaultableCompositeCommand("ImportPowerpoint");
        public static DefaultableCompositeCommand UploadPowerpoint = new DefaultableCompositeCommand("UploadPowerpoint");
        public static DefaultableCompositeCommand PowerpointFinished = new DefaultableCompositeCommand("PowerpointFinished");
        public static DefaultableCompositeCommand JoinCreatedConversation = new DefaultableCompositeCommand("JoinCreatedConversation");
        public static DefaultableCompositeCommand ReceiveMove = new DefaultableCompositeCommand("ReceiveMove");
        public static DefaultableCompositeCommand ReceiveJoin = new DefaultableCompositeCommand("ReceiveJoin");
        public static DefaultableCompositeCommand ReceivePing = new DefaultableCompositeCommand("ReceivePing");
        public static DefaultableCompositeCommand ReceiveFlush = new DefaultableCompositeCommand("ReceiveFlush");
        public static DefaultableCompositeCommand SendMeTLType = new DefaultableCompositeCommand("SendMeTLType");
        public static DefaultableCompositeCommand ToggleScratchPadVisibility = new DefaultableCompositeCommand("ToggleScratchPadVisibility");
        public static DefaultableCompositeCommand ToggleFriendsVisibility = new DefaultableCompositeCommand("ToggleFriendsVisibility");
        public static DefaultableCompositeCommand ReceivePublicChat = new DefaultableCompositeCommand("ReceivePublicChat");

        public static DefaultableCompositeCommand DummyCommandToProcessCanExecute = new DefaultableCompositeCommand("DummyCommandToProcessCanExecute");

        public static RoutedCommand HighlightFriend = new RoutedCommand();
        public static RoutedCommand PostHighlightFriend = new RoutedCommand();
        public static RoutedCommand HighlightUser = new RoutedCommand();
        public static RoutedCommand PostHighlightUser = new RoutedCommand();


        public static DefaultableCompositeCommand LaunchDiagnosticWindow = new DefaultableCompositeCommand("LaunchDiagnosticWindow");
        public static DefaultableCompositeCommand DiagnosticMessage = new DefaultableCompositeCommand("DiagnosticMessage");
        public static DefaultableCompositeCommand DiagnosticGaugeUpdated = new DefaultableCompositeCommand("DiagnosticGaugeUpdated");
        public static DefaultableCompositeCommand DiagnosticMessagesUpdated = new DefaultableCompositeCommand("DiagnosticMessagesUpdated");
        public static DefaultableCompositeCommand DiagnosticGaugesUpdated = new DefaultableCompositeCommand("DiagnosticGaugesUpdated");

        public static DefaultableCompositeCommand ShowProjector = new DefaultableCompositeCommand("ShowProjector");
        public static DefaultableCompositeCommand HideProjector = new DefaultableCompositeCommand("HideProjector");
        public static DefaultableCompositeCommand EnableProjector = new DefaultableCompositeCommand("EnableProjector");

        public static DefaultableCompositeCommand AddFlyoutCard = new DefaultableCompositeCommand("AddFlyoutCard");
        public static DefaultableCompositeCommand CloseFlyoutCard = new DefaultableCompositeCommand("CloseFlyoutCard");
        public static DefaultableCompositeCommand CreateDummyCard = new DefaultableCompositeCommand("CreateDummyCard"); // for testing only


        Commands()
        {
            NotImplementedYet.RegisterCommand(new DelegateCommand<object>((_param) => { }, (_param) => false));
        }
        public static int HandlerCount
        {
            get
            {
                return all.Aggregate(0, (acc, item) => acc += item.RegisteredCommands.Count());
            }
        }
        private static List<ICommand> staticHandlers = new List<ICommand>();

        public static void AllStaticCommandsAreRegistered()
        {
            foreach (var command in all)
            {
                foreach (var handler in command.RegisteredCommands)
                    staticHandlers.Add(handler);
            }
        }
        private static IEnumerable<DefaultableCompositeCommand> all
        {
            get
            {
                return typeof(Commands).GetFields()
                    .Where(p => p.FieldType == typeof(DefaultableCompositeCommand))
                    .Select(f => (DefaultableCompositeCommand)f.GetValue(null));
            }
        }


        public static IEnumerable<ICommand> allHandlers()
        {
            var handlers = new List<ICommand>();
            foreach (var command in all)
                foreach (var handler in command.RegisteredCommands)
                    handlers.Add(handler);
            return handlers.ToList();
        }
        public static void UnregisterAllCommands()
        {
            foreach (var command in all)
                foreach (var handler in command.RegisteredCommands)
                    if (!staticHandlers.Contains(handler))
                        command.UnregisterCommand(handler);
        }
        public static string which(ICommand command)
        {
            foreach (var field in typeof(Commands).GetFields())
                if (field.GetValue(null) == command)
                    return field.Name;
            return "Not a member of commands";
        }
        public static DefaultableCompositeCommand called(string name)
        {
            return (DefaultableCompositeCommand)typeof(Commands).GetField(name).GetValue(null);
        }
        public static void RequerySuggested()
        {
            RequerySuggested(all.ToArray());
        }
        public static void RequerySuggested(params DefaultableCompositeCommand[] commands)
        {
            foreach (var command in commands)
                Requery(command);
        }
        private static void Requery(DefaultableCompositeCommand command)
        {
            if (command.RegisteredCommands.Count() > 0)
            {
                //wrapping this in a try-catch for those commands who have non-nullable types on their canExecutes
                try
                {
                    var delegateCommand = command.RegisteredCommands[0];
                    delegateCommand.GetType().InvokeMember("RaiseCanExecuteChanged", BindingFlags.InvokeMethod, null, delegateCommand, new object[] { });
                }
                catch (Exception e)
                {
                    Console.WriteLine("exception while requerying: " + e.Message);
                }
            }
        }
    }
    public static class CommandExtensions
    {
        public static void ExecuteAsync(this DefaultableCompositeCommand command, object arg)
        {
            if (command.CanExecute(arg))
            {
                command.Execute(arg);
            }
        }
    }
}