using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using MeTLLib.DataTypes;

namespace SandRibbon
{
    public class NotSetException : Exception{
        public NotSetException(string msg) : base(msg) { }
    }
    public class DefaultableCompositeCommand : CompositeCommand 
    {
        private bool isSet = false;
        private object commandValue = null;

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

        public DefaultableCompositeCommand()
        {
        }

        public DefaultableCompositeCommand(object newValue) 
        {
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

        public override void RegisterCommand(ICommand command)
        {
            base.RegisterCommand(command);
        }
    }
    public class Commands
    {
        public static DefaultableCompositeCommand RequestMeTLUserInformations = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveMeTLUserInformations = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand RequestTeacherStatus = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveTeacherStatus = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetStackVisibility = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendNewSlideOrder = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ChangeLanguage = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand PresentVideo= new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReorderDragDone = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ConnectToSmartboard = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DisconnectFromSmartboard = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ViewSubmissions = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ViewBannedContent = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand Reconnecting = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand LeaveAllRooms = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand BackstageModeChanged = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand UpdatePowerpointProgress = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ShowOptionsDialog = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetUserOptions = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ZoomChanged = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ExtendCanvasBySize = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand CheckExtendedDesktop = new DefaultableCompositeCommand();

        public static DefaultableCompositeCommand AddPrivacyToggleButton = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand RemovePrivacyAdorners = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand MirrorVideo = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand VideoMirrorRefreshRectangle = new DefaultableCompositeCommand();

        #region Sandpit
        public static DefaultableCompositeCommand SendWakeUp = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendSleep = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveWakeUp = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveSleep = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendMoveBoardToSlide = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveMoveBoardToSlide = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand CloseBoardManager = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendPing = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceivePong = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DoWithCurrentSelection = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand BubbleCurrentSelection = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveNewBubble = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ExploreBubble = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ThoughtLiveWindow = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetZoomRect = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand Highlight = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand RemoveHighlight = new DefaultableCompositeCommand();

        #endregion
        public static DefaultableCompositeCommand ServersDown= new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand GenerateScreenshot = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ScreenshotGenerated = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendScreenshotSubmission = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveScreenshotSubmission = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ImportSubmission = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ImportSubmissions = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SaveFile = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DummyCommandToProcessCanExecute = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DummyCommandToProcessCanExecuteForPrivacyTools = new DefaultableCompositeCommand();

        public static DefaultableCompositeCommand TogglePens = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetPedagogyLevel = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand GetMainScrollViewer = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ShowConversationSearchBox = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand HideConversationSearchBox = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand AddWindowEffect = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand RemoveWindowEffect = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand NotImplementedYet = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand NoOp = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand MirrorPresentationSpace = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ProxyMirrorPresentationSpace = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand InitiateDig = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendDig = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DugPublicSpace = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendLiveWindow = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendDirtyLiveWindow = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveLiveWindow = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveDirtyLiveWindow = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DeleteSelectedItems = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand BanhammerSelectedItems= new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand VisualizeContent = new DefaultableCompositeCommand();

        #region Attendance
        public static DefaultableCompositeCommand SendAttendance = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveAttendance = new DefaultableCompositeCommand();
        #endregion

        //public static DefaultableCompositeCommand Relogin = new DefaultableCompositeCommand();
        #region Quizzing
        public static DefaultableCompositeCommand SendWormMove = new DefaultableCompositeCommand(); 
        public static DefaultableCompositeCommand ReceiveWormMove = new DefaultableCompositeCommand(); 
        public static DefaultableCompositeCommand ConvertPresentationSpaceToQuiz = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendQuiz = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendQuizAnswer = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveQuiz = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveQuizAnswer = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DisplayQuizResults = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand QuizResultsAvailableForSnapshot = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand QuizResultsSnapshotAvailable = new DefaultableCompositeCommand();
        //public static DefaultableCompositeCommand PlaceQuizSnapshot = new DefaultableCompositeCommand();

        #endregion
        #region InkCanvas
        public static DefaultableCompositeCommand SetInkCanvasMode = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetPrivacyOfItems = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand GotoThread = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetDrawingAttributes = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendStroke = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveStroke = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveStrokes = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendDirtyStroke = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveDirtyStrokes = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetPrivacy = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetContentVisibility = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand UpdateContentVisibility = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ForcePageRefresh = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand OriginalView = new DefaultableCompositeCommand();
        public static RoutedCommand Flush = new RoutedCommand();
        public static RoutedCommand CreateQuizStructure = new RoutedCommand();
        public static RoutedCommand ZoomIn = new RoutedCommand();
        public static RoutedCommand ZoomOut = new RoutedCommand();
        public static DefaultableCompositeCommand ExtendCanvasBothWays = new DefaultableCompositeCommand();
        #endregion
        #region ImageCanvas
        public static DefaultableCompositeCommand ImageDropped = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ImagesDropped = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand AddImage = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand FileUpload = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendImage = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveImage = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendMoveDelta = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveMoveDelta = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendDirtyImage = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveDirtyImage = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendDirtyAutoShape = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveAutoShape = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveDirtyAutoShape = new DefaultableCompositeCommand();

        public static DefaultableCompositeCommand SendFileResource = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveFileResource = new DefaultableCompositeCommand();

        #endregion
        #region TextCanvas
        public static DefaultableCompositeCommand ChangeTextMode = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand TextboxFocused = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand TextboxSelected = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendDirtyText = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveDirtyText = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetTextCanvasMode = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand IncreaseFontSize = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DecreaseFontSize = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendTextBox = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveTextBox = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand RestoreTextDefaults = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand NewTextCursorPosition = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand InitiateGrabZoom = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand EndGrabZoom = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand MoveCanvasByDelta = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand FitToView = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand FitToPageWidth= new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand UpdateTextStyling = new DefaultableCompositeCommand();

        public static DefaultableCompositeCommand ToggleBold = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ToggleItalic = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ToggleUnderline = new DefaultableCompositeCommand();
        #endregion
        #region AppLevel
        public static DefaultableCompositeCommand RegisterPowerpointSourceDirectoryPreference = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand MeTLType = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand LogOut = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand LoginFailed = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetIdentity = new DefaultableCompositeCommand(Credentials.Empty);
        public static DefaultableCompositeCommand NoNetworkConnectionAvailable = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand EstablishPrivileges = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand CloseApplication = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetLayer = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand UpdateForeignConversationDetails = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand RememberMe = new DefaultableCompositeCommand(false);
        public static DefaultableCompositeCommand ClipboardManager = new DefaultableCompositeCommand();
        #endregion
        public static DefaultableCompositeCommand Undo = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand Redo = new DefaultableCompositeCommand();
        public static RoutedCommand ProxyJoinConversation = new RoutedCommand();
        public static DefaultableCompositeCommand ChangeTab = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetRibbonAppearance = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SaveUIState = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand RestoreUIState = new DefaultableCompositeCommand();
        #region ConversationLevel
        public static DefaultableCompositeCommand SyncedMoveRequested = new DefaultableCompositeCommand(0);
        public static DefaultableCompositeCommand SendSyncMove = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand MoveTo = new DefaultableCompositeCommand(0);
        public static DefaultableCompositeCommand SneakInto = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SneakIntoAndDo = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SneakOutOf = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand PreParserAvailable = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ConversationPreParserAvailable = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand MoveToPrevious = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand MoveToNext = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetConversationPermissions = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ToggleNavigationLock = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand JoinConversation = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand LeaveConversation = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand LeaveLocation = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendDirtyConversationDetails = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand UpdateConversationDetails = new DefaultableCompositeCommand(ConversationDetails.Empty);
        public static DefaultableCompositeCommand ReceiveDirtyConversationDetails = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SetSync = new DefaultableCompositeCommand(false);
        public static DefaultableCompositeCommand AddSlide = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand UpdateNewSlideOrder = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand CreateBlankConversation = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ShowEditSlidesDialog = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand CreateConversation = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DuplicateSlide = new DefaultableCompositeCommand(new KeyValuePair<ConversationDetails,Slide>(ConversationDetails.Empty,Slide.Empty));
        public static DefaultableCompositeCommand DuplicateConversation = new DefaultableCompositeCommand(ConversationDetails.Empty);
        public static DefaultableCompositeCommand CreateGrouping = new DefaultableCompositeCommand();
        //public static DefaultableCompositeCommand PreEditConversation = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand EditConversation = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand BlockInput = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand UnblockInput = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand BlockSearch = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand UnblockSearch = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand CanEdit = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand PrintConversation = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand HideProgressBlocker = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendChatMessage = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveChatMessage = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand BanhammerActive = new DefaultableCompositeCommand();
        #endregion
        #region ppt
        public static DefaultableCompositeCommand ImportPowerpoint = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand UploadPowerpoint = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand PowerpointFinished = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveMove = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveJoin = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceivePing = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ReceiveFlush = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand SendMeTLType = new DefaultableCompositeCommand();
        #endregion
        #region Drawers
        public static DefaultableCompositeCommand ToggleScratchPadVisibility = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand ToggleFriendsVisibility = new DefaultableCompositeCommand();
        #endregion
        #region Friends
        public static DefaultableCompositeCommand ReceivePublicChat = new DefaultableCompositeCommand();
        public static RoutedCommand HighlightFriend = new RoutedCommand();
        public static RoutedCommand PostHighlightFriend = new RoutedCommand();
        public static RoutedCommand HighlightUser = new RoutedCommand();
        public static RoutedCommand PostHighlightUser = new RoutedCommand();

        public static DefaultableCompositeCommand LaunchDiagnosticWindow = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DiagnosticMessage = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DiagnosticGaugeUpdated = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DiagnosticMessagesUpdated = new DefaultableCompositeCommand();
        public static DefaultableCompositeCommand DiagnosticGaugesUpdated = new DefaultableCompositeCommand();

        #endregion
        Commands()
        {
            NotImplementedYet.RegisterCommand(new DelegateCommand<object>((_param) => { }, (_param) => false));
        }
        public static int HandlerCount{
            get {
                return all.Aggregate(0, (acc, item) => acc += item.RegisteredCommands.Count());
            }
        }
        private static List<ICommand> staticHandlers = new List<ICommand>();
        public static void AllStaticCommandsAreRegistered() {
            foreach (var command in all) {
                foreach (var handler in command.RegisteredCommands)
                    staticHandlers.Add(handler);
            }
        }
        private static IEnumerable<DefaultableCompositeCommand> all{
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
        public static void UnregisterAllCommands() {
            foreach (var command in all)
                foreach (var handler in command.RegisteredCommands)
                    if(!staticHandlers.Contains(handler))
                        command.UnregisterCommand(handler);
        }
        public static string which(ICommand command) {
            foreach (var field in typeof(Commands).GetFields())
                if (field.GetValue(null) == command)
                    return field.Name;
            return "Not a member of commands";
        }
        public static DefaultableCompositeCommand called(string name) {
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
                var delegateCommand = command.RegisteredCommands[0];
                delegateCommand.GetType().InvokeMember("RaiseCanExecuteChanged", BindingFlags.InvokeMethod, null, delegateCommand, new object[] { });
            }
        }
    }
    public static class CommandExtensions
    {
        public static void ExecuteAsync(this DefaultableCompositeCommand command, object arg) {
            if (command.CanExecute(arg))
            {
                command.Execute(arg);
            }
        }
        public static void RegisterCommandToDispatcher<T>(this DefaultableCompositeCommand command, DelegateCommand<T> handler) {
            command.RegisterCommand(new DelegateCommand<T>(arg =>
                                   {
                                       try
                                       {
                                           var dispatcher = Application.Current.Dispatcher;
                                           if (!dispatcher.CheckAccess())
                                               dispatcher.Invoke((Action)delegate
                                                                              {
                                                                                  if ( handler.CanExecute(arg)) handler.Execute (arg);
                                                                              });
                                           else if (handler.CanExecute(arg))
                                               handler.Execute(arg);
                                       }
                                       catch (Exception e)
                                       {
                                           Trace.TraceError("CRASH: fixed EXCEPTION:{0} INNER:{1}", e, e.InnerException);
                                       }
                                   }
                               , handler.CanExecute));
        } 
    }
}