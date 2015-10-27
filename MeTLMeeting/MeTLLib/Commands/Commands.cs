using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using MeTLLib.Providers;
using System.Diagnostics;
using MeTLLib.DataTypes;

namespace MeTLLib
{
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
        public override void UnregisterCommand(ICommand command)
        {
            base.UnregisterCommand(command);
        }
    }
    public class ClientCommands
    {
        
        public DefaultableCompositeCommand Mark = new DefaultableCompositeCommand();

        //public DefaultableCompositeCommand BrowseOneNote = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ManuallyConfigureOneNote = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand RequestMeTLUserInformations = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand MoveToNotebookPage = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveMeTLUserInformations = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand RequestTeacherStatus = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveTeacherStatus = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetStackVisibility = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendNewSlideOrder = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ChangeLanguage = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand PresentVideo = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ReorderDragDone = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ConnectToSmartboard = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand DisconnectFromSmartboard = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ViewSubmissions = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ViewBannedContent = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand Reconnecting = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand LeaveAllRooms = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand BackstageModeChanged = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand UpdatePowerpointProgress = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ShowOptionsDialog = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetUserOptions = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ZoomChanged = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ExtendCanvasBySize = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand CheckExtendedDesktop = new DefaultableCompositeCommand();

        //public DefaultableCompositeCommand AddPrivacyToggleButton = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand RemovePrivacyAdorners = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand MirrorVideo = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand VideoMirrorRefreshRectangle = new DefaultableCompositeCommand();

        #region Sandpit
        public DefaultableCompositeCommand AnalyzeSelectedConversations = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendWakeUp = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendSleep = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveWakeUp = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveSleep = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendMoveBoardToSlide = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveMoveBoardToSlide = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand CloseBoardManager = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendPing = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceivePong = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand DoWithCurrentSelection = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand BubbleCurrentSelection = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveNewBubble = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ExploreBubble = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ThoughtLiveWindow = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetZoomRect = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand Highlight = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand RemoveHighlight = new DefaultableCompositeCommand();

        #endregion
        public DefaultableCompositeCommand ServersDown = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand GenerateScreenshot = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ScreenshotGenerated = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendScreenshotSubmission = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveScreenshotSubmission = new DefaultableCompositeCommand();
        //public RoutedCommand ImportSubmission = new RoutedCommand();
        //public RoutedCommand ImportSubmissions = new RoutedCommand();
        //public RoutedCommand SaveFile = new RoutedCommand();
        //public DefaultableCompositeCommand DummyCommandToProcessCanExecute = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand DummyCommandToProcessCanExecuteForPrivacyTools = new DefaultableCompositeCommand();

        //public DefaultableCompositeCommand ModifySelection = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand TogglePens = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetPedagogyLevel = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand GetMainScrollViewer = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ShowConversationSearchBox = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand HideConversationSearchBox = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand AddWindowEffect = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand RemoveWindowEffect = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand NotImplementedYet = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand NoOp = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand MirrorPresentationSpace = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ProxyMirrorPresentationSpace = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand InitiateDig = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendDig = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand DugPublicSpace = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendLiveWindow = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendDirtyLiveWindow = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveLiveWindow = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveDirtyLiveWindow = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand DeleteSelectedItems = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand BanhammerSelectedItems = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand VisualizeContent = new DefaultableCompositeCommand();

        //public DefaultableCompositeCommand Relogin = new DefaultableCompositeCommand();
        #region Quizzing
        //public DefaultableCompositeCommand ToggleWorm = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendWormMove = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveWormMove = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ConvertPresentationSpaceToQuiz = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendQuiz = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendQuizAnswer = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveQuiz = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveQuizAnswer = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand DisplayQuizResults = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand QuizResultsAvailableForSnapshot = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand QuizResultsSnapshotAvailable = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand PlaceQuizSnapshot = new DefaultableCompositeCommand();

        #endregion
        #region InkCanvas
        //public DefaultableCompositeCommand ShowDiagnostics = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetInkCanvasMode = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetPrivacyOfItems = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand GotoThread = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetDrawingAttributes = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendStroke = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveStroke = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveStrokes = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendDirtyStroke = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveDirtyStrokes = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetPrivacy = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetContentVisibility = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand UpdateContentVisibility = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ForcePageRefresh = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand OriginalView = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand CreateQuizStructure = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand Flush = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ZoomIn = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ZoomOut = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ExtendCanvasBothWays = new DefaultableCompositeCommand();
        #endregion
        #region ImageCanvas
        //public DefaultableCompositeCommand MoreImageOptions = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand PickImages = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ImageDropped = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ImagesDropped = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand AddImage = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand FileUpload = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendImage = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveImage = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendMoveDelta = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveMoveDelta = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendDirtyImage = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveDirtyImage = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendDirtyAutoShape = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveAutoShape = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveDirtyAutoShape = new DefaultableCompositeCommand();

        public DefaultableCompositeCommand SendFileResource = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveFileResource = new DefaultableCompositeCommand();

        #endregion
        #region TextCanvas
        //public DefaultableCompositeCommand ChangeTextMode = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand TextboxFocused = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand TextboxSelected = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendDirtyText = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveDirtyText = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetTextCanvasMode = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand IncreaseFontSize = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand DecreaseFontSize = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendTextBox = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveTextBox = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand RestoreTextDefaults = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand NewTextCursorPosition = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand InitiateGrabZoom = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand EndGrabZoom = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand MoveCanvasByDelta = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand FitToView = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand FitToPageWidth = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand UpdateTextStyling = new DefaultableCompositeCommand();

        //public DefaultableCompositeCommand ToggleBold = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ToggleItalic = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ToggleUnderline = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ToggleStrikethrough = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand MoreTextOptions = new DefaultableCompositeCommand();
        #endregion
        #region AppLevel
        //public DefaultableCompositeCommand RegisterPowerpointSourceDirectoryPreference = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand MeTLType = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand LogOut = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand BackendSelected = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand LoginFailed = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SetIdentity = new DefaultableCompositeCommand(Credentials.Empty);
        public DefaultableCompositeCommand NoNetworkConnectionAvailable = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand EstablishPrivileges = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand CloseApplication = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetLayer = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand UpdateForeignConversationDetails = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand RememberMe = new DefaultableCompositeCommand(false);
        //public DefaultableCompositeCommand ClipboardManager = new DefaultableCompositeCommand();
        #endregion
        public DefaultableCompositeCommand Undo = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand Redo = new DefaultableCompositeCommand();
        //public RoutedCommand ProxyJoinConversation = new RoutedCommand();
        //public DefaultableCompositeCommand ChangeTab = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetRibbonAppearance = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SaveUIState = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand RestoreUIState = new DefaultableCompositeCommand();
        #region ConversationLevel
        public DefaultableCompositeCommand SyncedMoveRequested = new DefaultableCompositeCommand(0);
        public DefaultableCompositeCommand SendSyncMove = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand MoveToCollaborationPage = new DefaultableCompositeCommand(0);
        public DefaultableCompositeCommand SneakInto = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SneakIntoAndDo = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SneakOutOf = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand PreParserAvailable = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ConversationPreParserAvailable = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand MoveToOverview = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand MoveToPrevious = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand MoveToNext = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetConversationPermissions = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ToggleNavigationLock = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand JoinConversation = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand LeaveConversation = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand LeaveLocation = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendDirtyConversationDetails = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand UpdateConversationDetails = new DefaultableCompositeCommand(ConversationDetails.Empty);
        public DefaultableCompositeCommand ReceiveDirtyConversationDetails = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand SetSync = new DefaultableCompositeCommand(false);
        //public DefaultableCompositeCommand AddSlide = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand UpdateNewSlideOrder = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand CreateBlankConversation = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ShowEditSlidesDialog = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand CreateConversation = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand PreEditConversation = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand EditConversation = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand BlockInput = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand UnblockInput = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand BlockSearch = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand UnblockSearch = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand CanEdit = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand PrintConversation = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand HideProgressBlocker = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendChatMessage = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveChatMessage = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand BanhammerActive = new DefaultableCompositeCommand();
        #endregion
        #region ppt
        //public DefaultableCompositeCommand ImportPowerpoint = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand UploadPowerpoint = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand PowerpointFinished = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveMove = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveJoin = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceivePing = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand ReceiveFlush = new DefaultableCompositeCommand();
        public DefaultableCompositeCommand SendMeTLType = new DefaultableCompositeCommand();
        #endregion
        #region Drawers
        //public DefaultableCompositeCommand ToggleScratchPadVisibility = new DefaultableCompositeCommand();
        //public DefaultableCompositeCommand ToggleFriendsVisibility = new DefaultableCompositeCommand();
        #endregion
        #region Friends
        public DefaultableCompositeCommand ReceivePublicChat = new DefaultableCompositeCommand();
        //public RoutedCommand HighlightFriend = new RoutedCommand();
        //public RoutedCommand PostHighlightFriend = new RoutedCommand();
        //public RoutedCommand HighlightUser = new RoutedCommand();
        //public RoutedCommand PostHighlightUser = new RoutedCommand();
        #endregion

        #region CommandInstatiation
        public CompositeCommand WakeUpBoards = new CompositeCommand();
        public CompositeCommand SleepBoards = new CompositeCommand();
        public CompositeCommand AllContentSent = new CompositeCommand();
        #endregion

        public ClientCommands()
        {
            NotImplementedYet.RegisterCommand(new DelegateCommand<object>((_param) => { }, (_param) => false));
        }
        public int HandlerCount
        {
            get
            {
                return all.Aggregate(0, (acc, item) => acc += item.RegisteredCommands.Count());
            }
        }
        private List<ICommand> staticHandlers = new List<ICommand>();

        public void AllStaticCommandsAreRegistered()
        {
            foreach (var command in all)
            {
                foreach (var handler in command.RegisteredCommands)
                    staticHandlers.Add(handler);
            }
        }
        private IEnumerable<DefaultableCompositeCommand> all
        {
            get
            {
                return typeof(ClientCommands).GetFields()
                    .Where(p => p.FieldType == typeof(DefaultableCompositeCommand))
                    .Select(f => (DefaultableCompositeCommand)f.GetValue(this));
            }
        }
        public IEnumerable<ICommand> allHandlers()
        {
            var handlers = new List<ICommand>();
            foreach (var command in all)
                foreach (var handler in command.RegisteredCommands)
                    handlers.Add(handler);
            return handlers.ToList();
        }
        public void UnregisterAllCommands()
        {
            foreach (var command in all)
                foreach (var handler in command.RegisteredCommands)
                    if (!staticHandlers.Contains(handler))
                        command.UnregisterCommand(handler);
        }
        public string which(ICommand command)
        {
            foreach (var field in typeof(ClientCommands).GetFields())
                if (field.GetValue(this) == command)
                    return field.Name;
            return "Not a member of commands";
        }
        public DefaultableCompositeCommand called(string name)
        {
            return (DefaultableCompositeCommand)typeof(ClientCommands).GetField(name).GetValue(this);
        }
        public void RequerySuggested()
        {
            RequerySuggested(all.ToArray());
        }
        public void RequerySuggested(params DefaultableCompositeCommand[] commands)
        {
            foreach (var command in commands)
                Requery(command);
        }
        private void Requery(DefaultableCompositeCommand command)
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
        public static void ExecuteAsync(this DefaultableCompositeCommand command, object arg)
        {
            if (command.CanExecute(arg))
            {
                command.Execute(arg);
            }
        }
        public static void RegisterCommandToDispatcher<T>(this DefaultableCompositeCommand command, DelegateCommand<T> handler)
        {
            command.RegisterCommand(new DelegateCommand<T>(arg =>
            {
                try
                {
                    var dispatcher = Application.Current.Dispatcher;
                    if (!dispatcher.CheckAccess())
                        dispatcher.Invoke((Action)delegate
                        {
                            if (handler.CanExecute(arg)) handler.Execute(arg);
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


    static class CompositeCommandExtensions
    {
        public static object lastValue(this CompositeCommand command,ClientCommands commands)
        {
            if (CommandParameterProvider.parameters.ContainsKey(command))
                return CommandParameterProvider.parameters[command];
            throw new NotSetException(string.Format("MeTLLib::CompositeCommandExtensions:lastValue Could not find value of {0}", commands.which(command)));
        }

    }
    public class NotSetException : Exception
    {
        public NotSetException(string command)
            : base(command)
        {

        }
    }
    static class DefaultableCompositeCommandExtensions
    {
        public static void RegisterCommandToDispatcher<T>(this DefaultableCompositeCommand command, DelegateCommand<T> handler)
        {
            command.RegisterCommand(new DelegateCommand<T>(arg =>
            {
                try
                {
                    var dispatcher = Application.Current.Dispatcher;
                    if (!dispatcher.CheckAccess())
                        dispatcher.Invoke((Action)delegate
                        {
                            if (handler.CanExecute(arg)) handler.Execute(arg);
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
