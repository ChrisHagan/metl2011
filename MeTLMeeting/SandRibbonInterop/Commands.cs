using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace SandRibbon
{
    public static class CompositeCommandExtensions
    {
        public static object lastValue(this CompositeCommand command)
        {
            if (CommandParameterProvider.parameters.ContainsKey(command))
                return CommandParameterProvider.parameters[command];
            throw new NotSetException(string.Format("CompositeCommandExtensions::lastValue Could not get the lastValue of command {0}", Commands.which(command)));
        }
    }
    public class NotSetException : Exception {
        public NotSetException(string command):base(command)
        {
            
        }
    }
    public class Commands
    {
        public static CompositeCommand ListenToAudio = new CompositeCommand();
        public static CompositeCommand ReorderDragDone = new CompositeCommand();

        public static CompositeCommand ViewSubmissions = new CompositeCommand();
        public static CompositeCommand Reconnecting = new CompositeCommand();
        public static CompositeCommand LeaveAllRooms = new CompositeCommand();
        public static CompositeCommand BackstageModeChanged = new CompositeCommand();
        public static CompositeCommand UpdatePowerpointProgress = new CompositeCommand();
        public static CompositeCommand ShowOptionsDialog = new CompositeCommand();
        public static CompositeCommand SetUserOptions = new CompositeCommand();
        //public static CompositeCommand SaveUserOptions = new CompositeCommand();
        public static CompositeCommand ZoomChanged = new CompositeCommand();

        public static CompositeCommand AddPrivacyToggleButton = new CompositeCommand();
        public static CompositeCommand RemovePrivacyAdorners = new CompositeCommand();
        public static CompositeCommand MirrorVideo = new CompositeCommand();
        public static CompositeCommand VideoMirrorRefreshRectangle = new CompositeCommand();

        #region Sandpit
        public static CompositeCommand SendWakeUp = new CompositeCommand();
        public static CompositeCommand SendSleep = new CompositeCommand();
        public static CompositeCommand ReceiveWakeUp = new CompositeCommand();
        public static CompositeCommand ReceiveSleep = new CompositeCommand();
        public static CompositeCommand SendMoveBoardToSlide = new CompositeCommand();
        public static CompositeCommand ReceiveMoveBoardToSlide = new CompositeCommand();
        public static CompositeCommand CloseBoardManager = new CompositeCommand();
        public static CompositeCommand SendPing = new CompositeCommand();
        public static CompositeCommand ReceivePong = new CompositeCommand();
        public static CompositeCommand DoWithCurrentSelection = new CompositeCommand();
        public static CompositeCommand SendNewBubble = new CompositeCommand();
        public static CompositeCommand BubbleCurrentSelection = new CompositeCommand();
        public static CompositeCommand ReceiveNewBubble = new CompositeCommand();
        public static CompositeCommand ExploreBubble = new CompositeCommand();
        public static CompositeCommand ThoughtLiveWindow = new CompositeCommand();
        public static CompositeCommand SetZoomRect = new CompositeCommand();
        public static CompositeCommand Highlight = new CompositeCommand();
        public static CompositeCommand RemoveHighlight = new CompositeCommand();

        #endregion
        public static CompositeCommand ServersDown= new CompositeCommand();
        public static CompositeCommand GenerateScreenshot = new CompositeCommand();
        public static CompositeCommand ScreenshotGenerated = new CompositeCommand();
        public static CompositeCommand SendScreenshotSubmission = new CompositeCommand();
        public static CompositeCommand ReceiveScreenshotSubmission = new CompositeCommand();
        public static RoutedCommand ImportSubmission = new RoutedCommand();
        public static RoutedCommand SaveFile = new RoutedCommand();
        public static CompositeCommand DummyCommandToProcessCanExecute = new CompositeCommand();
        public static CompositeCommand DummyCommandToProcessCanExecuteForPrivacyTools = new CompositeCommand();

        public static CompositeCommand TogglePens = new CompositeCommand();
        public static CompositeCommand SetPedagogyLevel = new CompositeCommand();
        public static CompositeCommand GetMainScrollViewer = new CompositeCommand();
        public static CompositeCommand ShowConversationSearchBox = new CompositeCommand();
        public static CompositeCommand HideConversationSearchBox = new CompositeCommand();
        public static CompositeCommand AddWindowEffect = new CompositeCommand();
        public static CompositeCommand RemoveWindowEffect = new CompositeCommand();
        public static CompositeCommand NotImplementedYet = new CompositeCommand();
        public static CompositeCommand NoOp = new CompositeCommand();
        public static CompositeCommand MirrorPresentationSpace = new CompositeCommand();
        public static CompositeCommand ProxyMirrorPresentationSpace = new CompositeCommand();
        public static CompositeCommand InitiateDig = new CompositeCommand();
        public static CompositeCommand SendDig = new CompositeCommand();
        public static CompositeCommand DugPublicSpace = new CompositeCommand();
        public static CompositeCommand SendLiveWindow = new CompositeCommand();
        public static CompositeCommand SendDirtyLiveWindow = new CompositeCommand();
        public static CompositeCommand ReceiveLiveWindow = new CompositeCommand();
        public static CompositeCommand ReceiveDirtyLiveWindow = new CompositeCommand();
        public static CompositeCommand DeleteSelectedItems = new CompositeCommand();
        public static CompositeCommand BanhammerSelectedItems= new CompositeCommand();
        //public static CompositeCommand Relogin = new CompositeCommand();
        #region Quizzing
        public static CompositeCommand SendWormMove = new CompositeCommand(); 
        public static CompositeCommand ReceiveWormMove = new CompositeCommand(); 
        public static CompositeCommand ConvertPresentationSpaceToQuiz = new CompositeCommand();
        public static CompositeCommand SendQuiz = new CompositeCommand();
        public static CompositeCommand SendQuizAnswer = new CompositeCommand();
        public static CompositeCommand ReceiveQuiz = new CompositeCommand();
        public static CompositeCommand ReceiveQuizAnswer = new CompositeCommand();
        public static CompositeCommand DisplayQuizResults = new CompositeCommand();
        public static CompositeCommand QuizResultsAvailableForSnapshot = new CompositeCommand();
        public static CompositeCommand QuizResultsSnapshotAvailable = new CompositeCommand();
        public static CompositeCommand PlaceQuizSnapshot = new CompositeCommand();

        #endregion
        #region InkCanvas
        public static CompositeCommand SetInkCanvasMode = new CompositeCommand();
        public static CompositeCommand SetPrivacyOfItems = new CompositeCommand();
        public static CompositeCommand GotoThread = new CompositeCommand();
        public static CompositeCommand SetDrawingAttributes = new CompositeCommand();
        public static CompositeCommand SendStroke = new CompositeCommand();
        public static CompositeCommand ReceiveStroke = new CompositeCommand();
        public static CompositeCommand ReceiveStrokes = new CompositeCommand();
        public static CompositeCommand SendDirtyStroke = new CompositeCommand();
        public static CompositeCommand ReceiveDirtyStrokes = new CompositeCommand();
        public static CompositeCommand SetPrivacy = new CompositeCommand();
        public static CompositeCommand OriginalView = new CompositeCommand();
        public static RoutedCommand Flush = new RoutedCommand();
        public static RoutedCommand CreateQuizStructure = new RoutedCommand();
        public static RoutedCommand ZoomIn = new RoutedCommand();
        public static RoutedCommand ZoomOut = new RoutedCommand();
        public static CompositeCommand ExtendCanvasBothWays = new CompositeCommand();
        #endregion
        #region ImageCanvas
        public static CompositeCommand ImageDropped = new CompositeCommand();
        public static CompositeCommand AddVideo = new CompositeCommand();
        public static CompositeCommand SendVideo = new CompositeCommand();
        public static CompositeCommand ReceiveVideo = new CompositeCommand();
        public static CompositeCommand SendDirtyVideo = new CompositeCommand();
        public static CompositeCommand ReceiveDirtyVideo = new CompositeCommand();
        public static CompositeCommand AddImage = new CompositeCommand();
        public static CompositeCommand FileUpload = new CompositeCommand();
        public static CompositeCommand SendImage = new CompositeCommand();
        public static CompositeCommand ReceiveImage = new CompositeCommand();
        public static CompositeCommand SendDirtyImage = new CompositeCommand();
        public static CompositeCommand ReceiveDirtyImage = new CompositeCommand();
        public static CompositeCommand AddAutoShape = new CompositeCommand();
        public static CompositeCommand SendAutoShape = new CompositeCommand();
        public static CompositeCommand SendDirtyAutoShape = new CompositeCommand();
        public static CompositeCommand ReceiveAutoShape = new CompositeCommand();
        public static CompositeCommand ReceiveDirtyAutoShape = new CompositeCommand();

        public static CompositeCommand SendFileResource = new CompositeCommand();
        public static CompositeCommand ReceiveFileResource = new CompositeCommand();

        #endregion
        #region TextCanvas
        public static CompositeCommand ChangeTextMode = new CompositeCommand();
        public static CompositeCommand TextboxFocused = new CompositeCommand();
        public static CompositeCommand SendDirtyText = new CompositeCommand();
        public static CompositeCommand ReceiveDirtyText = new CompositeCommand();
        public static CompositeCommand SetTextCanvasMode = new CompositeCommand();
        public static CompositeCommand IncreaseFontSize = new CompositeCommand();
        public static CompositeCommand DecreaseFontSize = new CompositeCommand();
        public static CompositeCommand SendTextBox = new CompositeCommand();
        public static CompositeCommand ReceiveTextBox = new CompositeCommand();
        public static CompositeCommand RestoreTextDefaults = new CompositeCommand();
        public static CompositeCommand NewTextCursorPosition = new CompositeCommand();
        public static CompositeCommand InitiateGrabZoom = new CompositeCommand();
        public static CompositeCommand EndGrabZoom = new CompositeCommand();
        public static CompositeCommand MoveCanvasByDelta = new CompositeCommand();
        public static CompositeCommand FitToView = new CompositeCommand();
        public static CompositeCommand FitToPageWidth= new CompositeCommand();
        public static CompositeCommand UpdateTextStyling = new CompositeCommand();

        public static CompositeCommand ToggleBold = new CompositeCommand();
        public static CompositeCommand ToggleItalic = new CompositeCommand();
        public static CompositeCommand ToggleUnderline = new CompositeCommand();
        #endregion
        #region AppLevel
        public static CompositeCommand RegisterPowerpointSourceDirectoryPreference = new CompositeCommand();
        public static CompositeCommand MeTLType = new CompositeCommand();
        public static CompositeCommand LogOut = new CompositeCommand();
        public static CompositeCommand SetIdentity = new CompositeCommand();
        public static CompositeCommand EstablishPrivileges = new CompositeCommand();
        public static RoutedCommand CloseApplication = new RoutedCommand();
        public static CompositeCommand SetLayer = new CompositeCommand();
        public static CompositeCommand SetTutorialVisibility = new CompositeCommand();
        //public static CompositeCommand ThumbnailAvailable = new CompositeCommand();
        public static CompositeCommand UpdateForeignConversationDetails = new CompositeCommand();
        public static CompositeCommand RememberMe = new CompositeCommand();
        #endregion
        public static CompositeCommand Undo = new CompositeCommand();
        public static CompositeCommand Redo = new CompositeCommand();
        public static RoutedCommand ProxyJoinConversation = new RoutedCommand();
        public static CompositeCommand ChangeTab = new CompositeCommand();
        public static CompositeCommand SetRibbonAppearance = new CompositeCommand();
        #region ConversationLevel
        public static CompositeCommand SyncedMoveRequested = new CompositeCommand();
        public static CompositeCommand SendSyncMove = new CompositeCommand();
        public static CompositeCommand MoveTo = new CompositeCommand();
        public static CompositeCommand InternalMoveTo = new CompositeCommand();
        public static CompositeCommand SneakInto = new CompositeCommand();
        public static CompositeCommand SneakIntoAndDo = new CompositeCommand();
        public static CompositeCommand SneakOutOf = new CompositeCommand();
        public static CompositeCommand PreParserAvailable = new CompositeCommand();
        public static CompositeCommand ConversationPreParserAvailable = new CompositeCommand();
        public static CompositeCommand MoveToPrevious = new CompositeCommand();
        public static CompositeCommand MoveToNext = new CompositeCommand();
        public static CompositeCommand SetConversationPermissions = new CompositeCommand();
        public static CompositeCommand JoinConversation = new CompositeCommand();
        public static CompositeCommand LeaveConversation = new CompositeCommand();
        public static CompositeCommand SendDirtyConversationDetails = new CompositeCommand();
        public static CompositeCommand UpdateConversationDetails = new CompositeCommand();
        public static CompositeCommand ReceiveDirtyConversationDetails = new CompositeCommand();
        public static CompositeCommand SetSync = new CompositeCommand();
        public static CompositeCommand AddSlide = new CompositeCommand();
        public static CompositeCommand CreateBlankConversation = new CompositeCommand();
        public static CompositeCommand ShowEditSlidesDialog = new CompositeCommand();
        public static CompositeCommand CreateConversation = new CompositeCommand();
        public static CompositeCommand PreEditConversation = new CompositeCommand();
        public static CompositeCommand EditConversation = new CompositeCommand();
        public static CompositeCommand BlockInput = new CompositeCommand();
        public static CompositeCommand UnblockInput = new CompositeCommand();
        public static CompositeCommand CanEdit = new CompositeCommand();
        public static CompositeCommand PrintConversation = new CompositeCommand();
        public static CompositeCommand HideProgressBlocker = new CompositeCommand();
        public static CompositeCommand SendChatMessage = new CompositeCommand();
        public static CompositeCommand ReceiveChatMessage = new CompositeCommand();
        #endregion
        #region ppt
        public static CompositeCommand ImportPowerpoint = new CompositeCommand();
        public static CompositeCommand UploadPowerpoint = new CompositeCommand();
        public static CompositeCommand PowerpointFinished = new CompositeCommand();
        public static CompositeCommand ReceiveMove = new CompositeCommand();
        public static CompositeCommand ReceiveJoin = new CompositeCommand();
        public static CompositeCommand ReceivePing = new CompositeCommand();
        public static CompositeCommand ReceiveFlush = new CompositeCommand();
        public static CompositeCommand SendMeTLType = new CompositeCommand();
        #endregion
        #region Drawers
        public static CompositeCommand ToggleScratchPadVisibility = new CompositeCommand();
        public static CompositeCommand ToggleFriendsVisibility = new CompositeCommand();
        #endregion
        #region Friends
        public static CompositeCommand ReceivePublicChat = new CompositeCommand();
        public static RoutedCommand HighlightFriend = new RoutedCommand();
        public static RoutedCommand PostHighlightFriend = new RoutedCommand();
        public static RoutedCommand HighlightUser = new RoutedCommand();
        public static RoutedCommand PostHighlightUser = new RoutedCommand();
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
        private static IEnumerable<CompositeCommand> all{
            get
            {
                return typeof(Commands).GetFields()
                    .Where(p => p.FieldType == typeof(CompositeCommand))
                    .Select(f => (CompositeCommand)f.GetValue(null));
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
        public static CompositeCommand called(string name) {
            return (CompositeCommand)typeof(Commands).GetField(name).GetValue(null);
        }
        public static void RequerySuggested()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                RequerySuggested(all.ToArray());
            });
        }
        public static void RequerySuggested(params CompositeCommand[] commands)
        {
            foreach (var command in commands)
                Requery(command);
        }
        private static void Requery(CompositeCommand command)
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
        public static void ExecuteAsync(this CompositeCommand command, object arg) {
            if(command.CanExecute(arg))
                command.Execute(arg);
        }
        public static void RegisterCommandToDispatcher<T>(this CompositeCommand command, DelegateCommand<T> handler) {
            var dispatcher = Application.Current.Dispatcher;
            command.RegisterCommand(new DelegateCommand<T>(arg =>
                                   {
                                       try
                                       {
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