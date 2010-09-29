using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading;
using System.Windows;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using agsXMPP.Xml.Dom;
using Microsoft.Practices.Composite.Presentation.Commands;
using agsXMPP.protocol.iq.disco;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Structure;
using System.Diagnostics;
using Ninject;

namespace MeTLLib.Providers.Connection
{
    public class JabberWireFactory{
        public Credentials credentials { private get; set; }
        [Inject] public ConfigurationProvider configurationProvider { private get; set; }
        [Inject] public IConversationDetailsProvider conversationDetailsProvider { private get; set; } 
        [Inject] public HttpHistoryProvider historyProvider{private get;set;} 
        [Inject] public CachedHistoryProvider cachedHistoryProvider{private get;set;} 
        [Inject] public MeTLServerAddress metlServerAddress{private get;set;}
        public JabberWire wire() {
            if (credentials == null) throw new InvalidOperationException("The JabberWireFactory does not yet have credentials to create a wire");
            return new JabberWire(
                credentials, 
                conversationDetailsProvider,
                historyProvider,
                cachedHistoryProvider,
                metlServerAddress);
        }
        public PreParser preParser(int room){
            if (credentials == null) throw new InvalidOperationException("The JabberWireFactory does not yet have credentials to create a preParser");
            return new PreParser(
                credentials,
                room,
                conversationDetailsProvider,
                historyProvider,
                cachedHistoryProvider,
                metlServerAddress);
        }
        public PreParser create<T>(int room) where T:PreParser{
            return preParser(room);
        }
    }
    public partial class JabberWire
    {
        protected const string WORM = "/WORM_MOVES";
        protected const string SUBMISSION = "/SUBMISSION";
        protected const string UPDATE_CONVERSATION_DETAILS = "/UPDATE_CONVERSATION_DETAILS";
        protected const string SYNC_MOVE = "/SYNC_MOVE";
        protected const string GO_TO_SLIDE = "/GO_TO_SLIDE";
        protected const string WAKEUP = "/WAKEUP";
        protected const string SLEEP = "/SLEEP";
        protected const string PING = "/PING";
        protected const string PONG = "/PONG";
        public Credentials credentials;
        public Location location;
        private static string privacy = "PUBLIC";
        protected XmppClientConnection conn;
        private Jid jid;
        public bool LoggedIn = false;

        private void registerCommands()
        {
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.SendSyncMove.RegisterCommand(new DelegateCommand<int>(SendSyncMoveTo));
            Commands.SendDirtyConversationDetails.RegisterCommand(new DelegateCommand<string>(SendDirtyConversationDetails));
            Commands.SendTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>((textbox) => SendTextbox(textbox)));
            Commands.SendDirtyText.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyText));
            Commands.SendStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(SendStroke));
            Commands.SendDirtyStroke.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(sendDirtyStroke));
            Commands.SendImage.RegisterCommand(new DelegateCommand<TargettedImage>(SendImage));
            Commands.SendVideo.RegisterCommand(new DelegateCommand<TargettedVideo>(SendVideo));
            Commands.SendDirtyImage.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyImage));
            Commands.SendFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(sendFileResource));
            Commands.SendDirtyVideo.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyVideo));
            Commands.SendAutoShape.RegisterCommand(new DelegateCommand<TargettedAutoShape>(SendAutoShape));
            Commands.SendDirtyAutoShape.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyAutoShape));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<QuizQuestion>(SendQuiz));
            Commands.SendQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(SendQuizAnswer));
            Commands.SendChatMessage.RegisterCommand(new DelegateCommand<TargettedTextBox>(SendChat));
            Commands.SendLiveWindow.RegisterCommand(new DelegateCommand<LiveWindowSetup>(SendLiveWindow));
            Commands.SendDirtyLiveWindow.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyLiveWindow));
            Commands.SendWormMove.RegisterCommand(new DelegateCommand<WormMove>(SendWormMove));
            Commands.SendWakeUp.RegisterCommand(new DelegateCommand<string>(WakeUp, CanWakeUp));
            Commands.SendSleep.RegisterCommand(new DelegateCommand<string>(GoToSleep));
            Commands.SendMoveBoardToSlide.RegisterCommand(new DelegateCommand<BoardMove>(SendMoveBoardToSlide));
            Commands.SendPing.RegisterCommand(new DelegateCommand<string>(SendPing));
            Commands.SendNewBubble.RegisterCommand(new DelegateCommand<TargettedBubbleContext>(SendNewBubble));
            Commands.SneakInto.RegisterCommand(new DelegateCommand<string>(SneakInto));
            Commands.SneakOutOf.RegisterCommand(new DelegateCommand<string>(SneakOutOf));
            Commands.SendScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(SendScreenshotSubmission));
            Commands.getCurrentClasses.RegisterCommand(new DelegateCommand<object>(getCurrentClasses));
        }
        public ResourceCache cache;
        public JabberWire(Credentials credentials, IConversationDetailsProvider conversationDetailsProvider, HttpHistoryProvider historyProvider, CachedHistoryProvider cachedHistoryProvider, MeTLServerAddress metlServerAddress)
        {
            this.credentials = credentials;
            this.conversationDetailsProvider = conversationDetailsProvider;
            this.historyProvider = historyProvider;
            this.cachedHistoryProvider = cachedHistoryProvider;
            this.metlServerAddress = metlServerAddress;
            setUpWire();
            registerCommands();
        }
        internal List<ConversationDetails> CurrentClasses
        {
            get
            {
                try
                {
                    var currentRooms = new List<string>();
                    if (conn != null && conn.Username == Globals.me)
                    {
                        var discoIq = new agsXMPP.protocol.iq.disco.DiscoItemsIq(IqType.get);
                        discoIq.To = new Jid(Constants.MUC);
                        //discoIq.From = conn.MyJID;
                        var IQResponse = conn.IqGrabber.SendIq(discoIq);
                        if (IQResponse == null) return null;
                        foreach (object item in IQResponse.Query.ChildNodes.ToArray())
                        {
                            if (item.GetType().Equals(typeof(agsXMPP.protocol.iq.disco.DiscoItem)))
                            {
                                var name = ((agsXMPP.protocol.iq.disco.DiscoItem)item).Name.ToString();
                                name = name.Remove(name.Length - 4);
                                currentRooms.Add(name);
                            }
                        }
                        var populatedConversations = new List<ConversationDetails>();
                        var conversations = conversationDetailsProvider.ListConversations();
                        int dummy;
                        var conversationJids = currentRooms.Where(r => Int32.TryParse(r, out dummy))
                            .Select(r => Int32.Parse(r))
                            .Where(r => Slide.conversationFor(r) == r).Distinct().Select(r => r.ToString());
                        foreach (ConversationDetails conversation in conversations.Where(c => conversationJids.Contains(c.Jid)))
                        {
                            var discoOccupants = new agsXMPP.protocol.iq.disco.DiscoItemsIq(IqType.get);
                            discoOccupants.To = new Jid(conversation.Jid.ToString() + "@" + Constants.MUC);
                            var newIQResponse = conn.IqGrabber.SendIq(discoOccupants);
                            if (newIQResponse.Query.ChildNodes.ToArray()
                                .Where(n => n is agsXMPP.protocol.iq.disco.DiscoItem)
                                .Select(di => ((DiscoItem)di).Name.Split('@')[0])
                                .Any(name => name.Contains(conversation.Author)))
                            {
                                populatedConversations.Add(conversation);
                            }
                        }
                        return populatedConversations;
                    }
                    return null;
                }
                catch (Exception) { return null; }
            }
        }
        public void getCurrentClasses(object _unused)
        {
            Thread newThread = new Thread(new ThreadStart(() => { if (CurrentClasses != null)Commands.receiveCurrentClasses.Execute(CurrentClasses); }));
            newThread.Start();
        }
        private void SendNewBubble(TargettedBubbleContext selection)
        {
            stanza(new MeTLStanzas.Bubble(selection));
        }
        private void setUpWire()
        {
            conn = null;
            new MeTLLib.DataTypes.MeTLStanzasConstructor();
            this.jid = createJid(credentials.name);
            //WHAT?  Of COURSE it's null!  You just SET IT to null!  Besides, what are you afraid is going to happen?  Is the old value
            //going to leave residue on the new value?  It's better than null now!  It's orphaned!
            if (conn == null)
            {
                conn = new XmppClientConnection(jid.Server);
                conn.UseSSL = false;
                conn.AutoAgents = false;
            }
        }
        private void SendPing(string who)
        {
            directCommand(who, string.Format("{0} {1}", PING, credentials.name));
        }
        private Jid createJid(string username)
        {
            return new Jid(username + "@" + metlServerAddress.uri.Host);
        }
        private void SendWormMove(WormMove move)
        {
            send(credentials.name, string.Format("{0} {1}", WORM, move.direction));
        }
        public void SendLiveWindow(LiveWindowSetup window)
        {
            stanza(new MeTLStanzas.LiveWindow(window));
        }
        public void SendDirtyLiveWindow(TargettedDirtyElement dirty)
        {
            stanza(new MeTLStanzas.DirtyLiveWindow(dirty));
        }
        public void SendMoveBoardToSlide(BoardMove boardMove)
        {
            directCommand(boardMove.boardUsername, string.Format("{0} {1}", GO_TO_SLIDE, boardMove.roomJid));
        }
        private void OnAuthError(object _sender, Element error)
        {
            if (error.TagName == "failure")
            {
                conn.RegisterAccount = true;
                openConnection(jid.User);
            }
            else
            {
                throw new AuthenticationException(error.ToString());
            }
        }
        protected virtual void openConnection(string username)
        {
            conn.Open(username, "examplePassword", DateTimeFactory.Now().Ticks.ToString(), 1);
        }
        private void OnLogin(object o)
        {
            this.LoggedIn = true;
            joinRooms();
        }
        private void OnMessage(object sender, Message message)
        {
            ReceivedMessage(message);
        }
        private void HandlerError(object sender, Exception ex)
        {
            Trace.TraceError(string.Format("Handler error: {0}", ex.Message));
            Reset("Handler");
        }
        private void ElementError(object sender, Element element)
        {
            Trace.TraceError(string.Format("Element error: {0}", element.ToString()));
            Reset("Element");
        }
        protected virtual void ReadXml(object sender, string xml)
        {
            if (!xml.Contains("/WORM_MOVES"))
                Trace.TraceInformation("IN:" + xml);
        }
        protected virtual void WriteXml(object sender, string xml)
        {
            if (!xml.Contains("/WORM_MOVES"))
                Trace.TraceInformation("OUT:" + xml);
        }
        private void OnClose(object sender)
        {
            unregisterHandlers();
            Trace.TraceWarning("Closed manually, unregistered handlers");
        }
        private void OnPresence(object sender, Element element)
        {
            Commands.ReceiveWormMove.Execute("=");
        }
        public void Logout()
        {
            unregisterHandlers();
            conn.Close();
        }
        public void Login(Location location)
        {
            if (this.location == null)
                this.location = location;
            conn.OnAuthError += OnAuthError;
            conn.OnLogin += OnLogin;
            conn.OnMessage += OnMessage;
            conn.OnSocketError += HandlerError;
            conn.OnError += HandlerError;
            conn.OnRegisterError += ElementError;
            conn.OnStreamError += ElementError;
            conn.OnPresence += OnPresence;
            conn.OnClose += OnClose;
#if DEBUG
            conn.OnReadXml += ReadXml;
            conn.OnWriteXml += WriteXml;
#endif
            openConnection(jid.User);
        }
        private object resetLock = new object();
        private Credentials credentials_2;
        private IConversationDetailsProvider conversationDetailsProvider;
        private HttpHistoryProvider historyProvider;
        private CachedHistoryProvider cachedHistoryProvider;
        private MeTLServerAddress metlServerAddress;
        private void unregisterHandlers()
        {
            conn.OnAuthError -= OnAuthError;
            conn.OnLogin -= OnLogin;
            conn.OnMessage -= OnMessage;
            conn.OnSocketError -= HandlerError;
            conn.OnError -= HandlerError;
            conn.OnRegisterError -= ElementError;
            conn.OnStreamError -= ElementError;
            conn.OnPresence -= OnPresence;
            conn.OnReadXml -= ReadXml;
            conn.OnWriteXml -= WriteXml;
        }
        public void Reset(string caller)
        {
            try
            {
                lock (resetLock)
                {
                    if (conn.XmppConnectionState == XmppConnectionState.Disconnected)
                    {
                        Trace.TraceWarning(string.Format("Resetting.  Blame {0}", caller));
                        conn.Close();
                        Login(location);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("Xmpp error: {0}\nReconnecting", e.Message));
                Reset("Reset exception handling (recursive)");
            }
        }
        private void joinRooms()
        {
            var rooms = new[]
            {
                metlServerAddress.global,
                new Jid(credentials.name, metlServerAddress.muc, jid.Resource),
                new Jid(location.activeConversation,metlServerAddress.muc,jid.Resource),
                new Jid(location.currentSlide.ToString(), metlServerAddress.muc,jid.Resource),
                new Jid(string.Format("{0}{1}", location.currentSlide, credentials.name), metlServerAddress.muc,jid.Resource)
            };
            foreach (var room in rooms.Where(r => r.User != null && r.User != "0"))
            {
                try
                {
                    joinRoom(room);
                }
                catch (Exception e)
                {
                    Trace.TraceError(string.Format("Couldn't join room {0}: {1}", room, e.Message));
                }
            }
        }
        private void joinRoom(Jid room)
        {
            var alias = credentials.name + conn.Resource;
            new MucManager(conn).JoinRoom(room, alias, true);
            Trace.TraceInformation(string.Format("Joined room {0}", room));
        }
        private void send(string target, string message)
        {
            try
            {
                if (target.ToLower() == "global")
                    Trace.TraceInformation(string.Format("{0} fired on the global thread", message));
                send(new Message(new Jid(target + "@" + Constants.MUC), jid, MessageType.groupchat, message));
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("Exception in send: {0}", e.Message));
            }
        }
        protected virtual void send(Message message)
        {
            conn.Send(message);
        }
        public void stanza(string target, Element stanza)
        {
            var message = new Message();
            string modifiedTarget =
                stanza.GetTag(MeTLStanzas.privacyTag) == "private" ?
                string.Format("{0}{1}", target, stanza.GetTag("author")) : target;
            message.To = new Jid(string.Format("{0}@{1}", modifiedTarget, Constants.MUC));
            message.From = jid;
            message.Type = MessageType.groupchat;
            message.AddChild(stanza);
            send(message);
        }
        public void stanza(Element stanza)
        {
            this.stanza(location.currentSlide.ToString(), stanza);
        }
        private void command(string where, string message)
        {
            send(where, message);
        }
        private void command(string message)
        {
            send("global", message);
        }
        private void directCommand(string target, string message)
        {
            conn.Send(new Message(new Jid(target + "@" + Constants.SERVER), jid, MessageType.chat, message));
        }
        private void onStart()
        {
            Commands.UpdateWormInterval.Execute(TimeSpan.FromMilliseconds(15000));
        }
        private void onProgress(int upTo, int outOf)
        {
            //Commands.RetrievedHistoryPortion.Execute(new[] { upTo, outOf });
        }
        private void retrieveHistory(string room)
        {
            historyProvider.Retrieve<PreParser>(
                onStart,
                onProgress,
                null,
                room);
        }
        public void SendDirtyText(TargettedDirtyElement element)
        {
            stanza(new MeTLStanzas.DirtyText(element));
        }
        public void SendDirtyVideo(TargettedDirtyElement element)
        {
            stanza(new MeTLStanzas.DirtyVideo(element));
        }
        public void SendDirtyImage(TargettedDirtyElement element)
        {
            stanza(new MeTLStanzas.DirtyImage(element));
        }
        public void SendDirtyAutoShape(TargettedDirtyElement element)
        {
            stanza(new MeTLStanzas.DirtyImage(element));
        }
        public bool IsConnected()
        {
            return conn != null && conn.Authenticated;
        }
        public void GetHistory(int where)
        {
            historyProvider.Retrieve<PreParser>(
                onStart,
                onProgress,
                finishedParser => Commands.PreParserAvailable.Execute(finishedParser),
                location.currentSlide.ToString());
            historyProvider.RetrievePrivateContent<PreParser>(
                onStart,
                onProgress,
                finishedParser => Commands.PreParserAvailable.Execute(finishedParser),
                credentials.name,
                location.currentSlide.ToString());
        }
        public void MoveTo(int where)
        {
            new MucManager(conn).LeaveRoom(
                new Jid(string.Format("{0}{1}", location.currentSlide, credentials.name), metlServerAddress.muc, jid.Resource), credentials.name);
            location.currentSlide = where;
            joinRooms();
            historyProvider.Retrieve<PreParser>(
                onStart,
                onProgress,
                finishedParser => Commands.PreParserAvailable.Execute(finishedParser),
                location.currentSlide.ToString());
            historyProvider.RetrievePrivateContent<PreParser>(
                onStart,
                onProgress,
                finishedParser => Commands.PreParserAvailable.Execute(finishedParser),
                credentials.name,
                location.currentSlide.ToString());
        }
        public void SendStroke(TargettedStroke stroke)
        {
            stanza(stroke.slide.ToString(), new MeTLStanzas.Ink(stroke));
        }
        private void SendScreenshotSubmission(TargettedSubmission submission)
        {
            stanza(Globals.location.activeConversation, new MeTLStanzas.ScreenshotSubmission(submission));
        }
        public void SendSyncMoveTo(int where)
        {
            command(location.activeConversation, SYNC_MOVE + " " + where);
        }
        public void SendImage(TargettedImage image)
        {
            stanza(image.slide.ToString(), new MeTLStanzas.Image(image));
        }
        private void sendFileResource(TargettedFile file)
        {
            stanza(Globals.location.activeConversation, new MeTLStanzas.FileResource(file));
        }
        public void SendVideo(TargettedVideo video)
        {
            stanza(video.slide.ToString(), new MeTLStanzas.Video(video));
        }
        public void SendAutoShape(TargettedAutoShape autoshape)
        {
            stanza(autoshape.slide.ToString(), new MeTLStanzas.AutoShape(autoshape));
        }
        private void SendChat(TargettedTextBox message)
        {
            stanza(location.activeConversation, new MeTLStanzas.TextBox(message));
        }
        public void SendTextbox(TargettedTextBox box)
        {
            stanza(box.slide.ToString(), new MeTLStanzas.TextBox(box));
        }
        public void SendQuiz(QuizQuestion parameters)
        {
            var quiz = new MeTLStanzas.Quiz(parameters);
            stanza(Globals.location.activeConversation, quiz);
        }
        private void SendQuizAnswer(QuizAnswer parameters)
        {
            var quiz = new MeTLStanzas.QuizResponse(parameters);
            stanza(Globals.location.activeConversation, quiz);
        }
        public void SendDirtyConversationDetails(string jid)
        {
            command(UPDATE_CONVERSATION_DETAILS + " " + (jid));
        }
        public bool CanWakeUp(string _param)
        {
            return true;
        }
        public void WakeUp(string room)
        {
            Commands.WakeUpBoards.Execute(room);
            /*This next bit needs to be moved back into the application
            foreach (var board in BoardManager.boards[room])
            {
                directCommand(board.name, WAKEUP);
            }*/
        }
        public void CommandBoardToMoveTo(string board, string slide)
        {
            directCommand(board, string.Format("{0} {1}", GO_TO_SLIDE, slide));
        }
        public void GoToSleep(string room)
        {
            Commands.SleepBoards.Execute(room);
            /*This bit needs to be moved back into the application
            foreach (var board in BoardManager.boards[room])
                directCommand(room, SLEEP);*/
        }
        public void sendDirtyStroke(TargettedDirtyElement element)
        {
            stanza(new MeTLStanzas.DirtyInk(element));
        }
        public virtual void ReceiveCommand(string message)
        {
            try
            {
                if (message.Length == 0) return;
                var parts = message.Split(new[] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);
                switch (parts[0].ToUpper())
                {
                    case SYNC_MOVE:
                        handleSyncMoveReceived(parts);
                        break;
                    case UPDATE_CONVERSATION_DETAILS:
                        handleConversationDetailsUpdated(parts);
                        break;
                    case WORM:
                        handleWormMoved(parts);
                        break;
                    case GO_TO_SLIDE:
                        handleGoToSlide(parts);
                        break;
                    case WAKEUP:
                        handleWakeUp(parts);
                        break;
                    case SLEEP:
                        handleSleep(parts);
                        break;
                    case PING:
                        handlePing(parts);
                        break;
                    case PONG:
                        handlePong(parts);
                        break;
                    default:
                        handleUnknownMessage(message);
                        break;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("Uncaught exception in ReceivedMessage: {0}", e.Message));
            }
        }
        public virtual void handlePing(string[] parts)
        {
            directCommand(parts[1], string.Format("{0} {1}", PONG, credentials.name));
        }
        public virtual void handlePong(string[] parts)
        {
            Commands.ReceivePong.Execute(parts[1]);
        }
        public virtual void handleGoToConversation(string[] parts)
        {
            Application.Current.Dispatcher.adopt((Action)delegate
            {
                Commands.JoinConversation.Execute(parts[1]);
            });
        }
        public virtual void handleGoToSlide(string[] parts)
        {
            var id = Int32.Parse(parts[1]);
            var desiredConversation = Slide.conversationFor(id).ToString();
            if (desiredConversation != location.activeConversation)
            {
                DelegateCommand<ConversationDetails> joinedConversation = null;
                joinedConversation = new DelegateCommand<ConversationDetails>(
                    _conversationJid =>
                    {
                        Commands.UpdateConversationDetails.UnregisterCommand(joinedConversation);
                        Application.Current.Dispatcher.adopt((Action)delegate
                        {
                            Commands.MoveTo.Execute(id);
                            Commands.ReceiveMoveBoardToSlide.Execute(id);
                        });
                    });
                Commands.UpdateConversationDetails.RegisterCommand(joinedConversation);
                Application.Current.Dispatcher.adopt((Action)delegate
                {
                    Commands.JoinConversation.Execute(desiredConversation);
                });
            }
            else
            {
                Application.Current.Dispatcher.adopt((Action)delegate
                {
                    Commands.MoveTo.Execute(id);
                });
            }
        }
        public virtual void handleWakeUp(string[] parts)
        {
            Application.Current.Dispatcher.adopt((Action)delegate
            {
                Commands.ReceiveWakeUp.Execute(null);
            });
        }
        public virtual void handleSleep(string[] parts)
        {
            Application.Current.Dispatcher.adopt((Action)delegate
            {
                Commands.ReceiveSleep.Execute(null);
            });
        }
        public virtual void handleWormMoved(string[] parts)
        {
            Commands.ReceiveWormMove.Execute(parts[1]);
        }
        public virtual void ReceivedMessage(object obj)
        {
            var message = (Element)obj;
            if (message.GetAttribute("type") == "error")
            {
                Trace.TraceError(message.ToString());
                return;
            }
            if (message.SelectSingleElement("body") != null)
            {
                ReceiveCommand(message.SelectSingleElement("body").InnerXml);
                return;
            }
            try
            {
                cachedHistoryProvider.HandleMessage(message.GetAttribute("from").Split('@')[0], message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (Application.Current == null) return;
            Application.Current.Dispatcher.adoptAsync(
                () =>
                    ActOnUntypedMessage(message));
        }
        public virtual void ActOnUntypedMessage(Element message)
        {
            foreach (var ink in message.SelectElements<MeTLStanzas.Ink>(true))
                actOnStrokeReceived(ink.Stroke);
            foreach (var submission in message.SelectElements<MeTLStanzas.ScreenshotSubmission>(true))
                actOnScreenshotSubmission(submission.parameters);
            foreach (var box in message.SelectElements<MeTLStanzas.TextBox>(true))
                actOnTextReceived(box.Box);
            foreach (var image in message.SelectElements<MeTLStanzas.Image>(true))
                actOnImageReceived(image.Img);
            foreach (var autoshape in message.SelectElements<MeTLStanzas.AutoShape>(true))
                actOnAutoShapeReceived(autoshape.autoshape);
            foreach (var quiz in message.SelectElements<MeTLStanzas.Quiz>(true))
                actOnQuizReceived(quiz.parameters);
            foreach (var quizAnswer in message.SelectElements<MeTLStanzas.QuizResponse>(true))
                actOnQuizAnswerReceived(quizAnswer.parameters);
            foreach (var liveWindow in message.SelectElements<MeTLStanzas.LiveWindow>(true))
                actOnLiveWindowReceived(liveWindow.parameters);
            foreach (var dirtyLiveWindow in message.SelectElements<MeTLStanzas.DirtyLiveWindow>(true))
                actOnDirtyLiveWindowReceived(dirtyLiveWindow.element);
            foreach (var dirtyText in message.SelectElements<MeTLStanzas.DirtyText>(true))
                actOnDirtyTextReceived(dirtyText);
            foreach (var dirtyInk in message.SelectElements<MeTLStanzas.DirtyInk>(true))
                actOnDirtyStrokeReceived(dirtyInk);
            foreach (var dirtyImage in message.SelectElements<MeTLStanzas.DirtyImage>(true))
                actOnDirtyImageReceived(dirtyImage);
            foreach (var dirtyAutoShape in message.SelectElements<MeTLStanzas.DirtyAutoshape>(true))
                actOnDirtyAutoshapeReceived(dirtyAutoShape);
            foreach (var bubble in message.SelectElements<MeTLStanzas.Bubble>(true))
                actOnBubbleReceived(bubble.context);
            foreach (var video in message.SelectElements<MeTLStanzas.Video>(true))
            {
                var vid = video.Vid;
                actOnVideoReceived(video.Vid);
            }
            foreach (var dirtyVideo in message.SelectElements<MeTLStanzas.DirtyVideo>(true))
                actOnDirtyVideoReceived(dirtyVideo);
            foreach (var file in message.SelectElements<MeTLStanzas.FileResource>(true))
                actOnFileResource(file);
        }
        public virtual void actOnFileResource(MeTLStanzas.FileResource resource)
        {
            Commands.ReceiveFileResource.Execute(resource.fileResource);
        }
        public virtual void actOnScreenshotSubmission(TargettedSubmission submission)
        {
            Commands.ReceiveScreenshotSubmission.Execute(submission);
        }
        public virtual void actOnVideoReceived(TargettedVideo video)
        {
            Commands.ReceiveVideo.Execute(video);
        }
        public virtual void actOnBubbleReceived(TargettedBubbleContext bubble)
        {
            Commands.ReceiveNewBubble.Execute(bubble);
        }
        public virtual void actOnDirtyAutoshapeReceived(MeTLStanzas.DirtyAutoshape dirtyAutoShape)
        {
            Commands.ReceiveDirtyAutoShape.Execute(dirtyAutoShape.element);
        }
        public virtual void actOnDirtyVideoReceived(MeTLStanzas.DirtyVideo dirtyVideo)
        {
            Commands.ReceiveDirtyVideo.Execute(dirtyVideo.element);
        }
        public virtual void actOnDirtyImageReceived(MeTLStanzas.DirtyImage dirtyImage)
        {
            Commands.ReceiveDirtyImage.Execute(dirtyImage.element);
        }
        public virtual void actOnDirtyStrokeReceived(MeTLStanzas.DirtyInk element)
        {
            Commands.ReceiveDirtyStrokes.Execute(new[] { element.element });
        }
        public virtual void actOnDirtyTextReceived(MeTLStanzas.DirtyText dirtyText)
        {
            Commands.ReceiveDirtyText.Execute(dirtyText.element);
        }
        public virtual void actOnImageReceived(TargettedImage image)
        {
            Commands.ReceiveImage.Execute(new[] { image });
        }
        public virtual void actOnAutoShapeReceived(TargettedAutoShape autoshape)
        {
            Commands.ReceiveAutoShape.Execute(autoshape);
        }
        public virtual void actOnQuizReceived(QuizQuestion quiz)
        {
            Commands.ReceiveQuiz.Execute(quiz);
        }
        public virtual void actOnQuizAnswerReceived(QuizAnswer answer)
        {
            Commands.ReceiveQuizAnswer.Execute(answer);
        }
        public virtual void actOnStrokeReceived(TargettedStroke stroke)
        {
            Commands.ReceiveStroke.Execute(stroke);
        }
        public virtual void actOnTextReceived(TargettedTextBox box)
        {
            if (box.target == "chat")
                Commands.ReceiveChatMessage.Execute(box);
            else
                Commands.ReceiveTextBox.Execute(box);
        }
        public virtual void actOnLiveWindowReceived(LiveWindowSetup window)
        {
            Commands.ReceiveLiveWindow.Execute(window);
        }
        public virtual void actOnDirtyLiveWindowReceived(TargettedDirtyElement element)
        {
            Commands.ReceiveDirtyLiveWindow.Execute(element);
        }
        public void SneakInto(string room)
        {
            var muc = new MucManager(conn);
            joinRoom(new Jid(room + "@" + Constants.MUC));

            historyProvider.Retrieve<PreParser>(
                onStart,
                onProgress,
                finishedParser =>
                {
                    Trace.TraceInformation(string.Format("JabberWire retrievalComplete action invoked for {0}", location.currentSlide));
                    Commands.PreParserAvailable.Execute(finishedParser);
                },
                room);
            historyProvider.RetrievePrivateContent<PreParser>(
                onStart,
                onProgress,
                finishedParser => Commands.PreParserAvailable.Execute(finishedParser),
                credentials.name,
                room);
        }
        public void SneakOutOf(string room)
        {
            var muc = new MucManager(conn);
            muc.LeaveRoom(new Jid(room + "@" + Constants.MUC), credentials.name);
        }
        public void JoinConversation(string room)
        {
            if (location.activeConversation != null)
            {
                var muc = new MucManager(conn);
                muc.LeaveRoom(new Jid(location.activeConversation + "@" + Constants.MUC), credentials.name);
                foreach (var slide in conversationDetailsProvider.DetailsOf(location.activeConversation).Slides.Select(s => s.id))
                    muc.LeaveRoom(new Jid(slide + "@" + Constants.MUC), credentials.name);
            }
            location.activeConversation = room;
            joinRooms();

        }
        private void handleSyncMoveReceived(string[] parts)
        {
            var where = parts[1];
            Commands.SyncedMoveRequested.Execute(Int32.Parse(where));
        }
        private void handleConversationDetailsUpdated(string[] parts)
        {
            var jid = parts[1];
            Commands.ReceiveDirtyConversationDetails.Execute(jid);
            var newDetails = conversationDetailsProvider.DetailsOf(jid);
            Commands.UpdateForeignConversationDetails.Execute(newDetails);
            if (isCurrentConversation(jid))
                Commands.UpdateConversationDetails.Execute(newDetails);
        }
        private bool isCurrentConversation(string jid)
        {
            return location != null
                && location.activeConversation != null
                && location.activeConversation.Equals(jid);
        }
        protected virtual void handleUnknownMessage(string message)
        {
            Trace.TraceWarning(string.Format("Received unknown message: {0}", message));
        }
    }
}