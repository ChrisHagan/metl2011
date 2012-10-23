using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Authentication;
using System.Threading;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using agsXMPP.Xml.Dom;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Structure;
using MeTLLib.Utilities;
using Microsoft.Practices.Composite.Presentation.Commands;
using Ninject;

namespace MeTLLib.Providers.Connection
{
    public class ActionsAfterReloginQueue
    {
        private static Object queueLock = new Object();
        private readonly Queue<Action> actionsAfterRelogin = null;

        public ActionsAfterReloginQueue()
        {
            actionsAfterRelogin = new Queue<Action>();
        }

        public int Count
        {
            get
            {
                lock (queueLock)
                {
                    return actionsAfterRelogin.Count;
                }
            }
        }

        public Action Peek()
        {
            lock (queueLock)
            {
                return (Action)actionsAfterRelogin.Peek();
            }
        }
        
        public void Enqueue(Action action)
        {
            lock (queueLock)
            {
                actionsAfterRelogin.Enqueue(action);
            }
        }

        public Action Dequeue()
        {
            lock (queueLock)
            {
                return actionsAfterRelogin.Dequeue();
            }
        }
    }

    public class JabberWireFactory
    {
        public Credentials credentials { private get; set; }
        [Inject]
        public ConfigurationProvider configurationProvider { private get; set; }
        [Inject]
        public IConversationDetailsProvider conversationDetailsProvider { private get; set; }
        [Inject]
        public HttpHistoryProvider historyProvider { private get; set; }
        [Inject]
        public CachedHistoryProvider cachedHistoryProvider { private get; set; }
        [Inject]
        public MeTLServerAddress metlServerAddress { private get; set; }
        [Inject]
        public ResourceCache cache { private get; set; }
        [Inject]
        public IReceiveEvents receiveEvents { private get; set; }
        [Inject]
        public IWebClientFactory clientFactory { private get; set; }
        [Inject]
        public HttpResourceProvider resourceProvider { private get; set; }
        private JabberWire instance;
        private static object instanceLock = new object();
        public JabberWire wire()
        {
            using (DdMonitor.Lock(instanceLock))
            {
                if (credentials == null) throw new InvalidOperationException("The JabberWireFactory does not yet have credentials to create a wire");
                if (instance == null)
                {
                    instance = new JabberWire(
                        credentials,
                        conversationDetailsProvider,
                        historyProvider,
                        cachedHistoryProvider,
                        metlServerAddress,
                        cache,
                        receiveEvents,
                        clientFactory,
                        resourceProvider,true);
                    instance.openConnection();
                }
                return instance;
            };
        }
        public PreParser preParser(int room)
        {
            if (credentials == null) throw new InvalidOperationException("The JabberWireFactory does not yet have credentials to create a preParser");
            return new PreParser(
                credentials,
                room,
                conversationDetailsProvider,
                historyProvider,
                cachedHistoryProvider,
                metlServerAddress,
                cache, receiveEvents, clientFactory, resourceProvider);
        }
        public T preParser<T>(int room) where T : PreParser
        {
            return (T)Activator.CreateInstance(typeof(T),
                credentials,
                room,
                conversationDetailsProvider,
                historyProvider,
                cachedHistoryProvider,
                metlServerAddress,
                cache,
                receiveEvents,
                clientFactory,
                resourceProvider);
        }
        public virtual PreParser create<T>(int room) where T : PreParser
        {
            return preParser<T>(room);
        }
    }

    public enum MessageOrigin
    {
        History,
        Live
    }

    public partial class JabberWire
    {
        protected const string WORM = "/WORM_MOVES";
        protected const string SUBMISSION = "/SUBMISSION";
        protected const string UPDATE_CONVERSATION_DETAILS = "/UPDATE_CONVERSATION_DETAILS";
        protected const string SYNC_MOVE = "/SYNC_MOVE";
        protected const string TEACHER_IN_CONVERSATION = "/TEACHER_IN_CONVERSATION";
        protected const string GO_TO_SLIDE = "/GO_TO_SLIDE";
        protected const string WAKEUP = "/WAKEUP";
        protected const string SLEEP = "/SLEEP";
        protected const string PING = "/PING";
        protected const string PONG = "/PONG";
        protected const string UPDATE_SLIDE_COLLECTION = "/UPDATE_SLIDE_COLLECTION";
        private const uint HEARTBEAT_PERIOD = 20000;
        private const uint TIMEOUT_PERIOD = 10000;
        public Credentials credentials;
        public Location location = Location.Empty; 
        protected IReceiveEvents receiveEvents;
        protected IWebClientFactory webClientFactory;
        protected HttpResourceProvider resourceProvider;
        protected static string privacy = "PUBLIC";
        private XmppClientConnection conn;
        private Timer heartbeat;
        protected Jid jid;
        public bool activeWire { get; private set; }

        private void registerCommands()
        {
            Commands.SendWakeUp.RegisterCommand(new DelegateCommand<string>(WakeUp, CanWakeUp));
            Commands.SendSleep.RegisterCommand(new DelegateCommand<string>(GoToSleep));
            Commands.SendMoveBoardToSlide.RegisterCommand(new DelegateCommand<BoardMove>(SendMoveBoardToSlide));
            Commands.SendPing.RegisterCommand(new DelegateCommand<string>(SendPing));
        }
        public ResourceCache cache;
        public JabberWire(Credentials credentials, IConversationDetailsProvider conversationDetailsProvider, HttpHistoryProvider historyProvider, CachedHistoryProvider cachedHistoryProvider, MeTLServerAddress metlServerAddress, ResourceCache cache, IReceiveEvents events, IWebClientFactory webClientFactory, HttpResourceProvider resourceProvider, bool active)
        {
            this.credentials = credentials;
            this.conversationDetailsProvider = conversationDetailsProvider;
            this.historyProvider = historyProvider;
            this.cachedHistoryProvider = cachedHistoryProvider;
            this.metlServerAddress = metlServerAddress;
            this.cache = cache;
            this.receiveEvents = events;
            this.webClientFactory = webClientFactory;
            this.resourceProvider = resourceProvider;
            this.jid = createJid(credentials.name);
            new MeTLLib.DataTypes.MeTLStanzasConstructor();
            this.activeWire = active;
            if (activeWire)
            {
                receiveEvents.StatusChanged += listenToStatusChangedForReset;
                establishHeartBeat();
                NetworkChange.NetworkAvailabilityChanged += networkAvailabilityChanged;
            }
        }

        private void establishHeartBeat()
        {
            heartbeat = new Timer((_unused) => { checkConnection(); }, null, HEARTBEAT_PERIOD, HEARTBEAT_PERIOD);
        }

        private void shutdownHeartBeat()
        {
            if (heartbeat != null)
            {
                heartbeat.Dispose();
                heartbeat = null;
            }
        }

        private Timer ConnectionTimeoutTimer;
        private void EstablishConnectionTimer(){ 
            ConnectionTimeoutTimer = new Timer((_unused) => {
                if (conn == null || conn.XmppConnectionState != XmppConnectionState.Connected)
                    Reset("Resetting because connection took too long");
            },null,TIMEOUT_PERIOD,TIMEOUT_PERIOD);
        }
        private void StopConnectionTimeoutTimer()
        {
            if (ConnectionTimeoutTimer == null) return;
            ConnectionTimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        private void StartConnectionTimeoutTimer()
        {
            if (ConnectionTimeoutTimer == null) EstablishConnectionTimer();
            ConnectionTimeoutTimer.Change(TIMEOUT_PERIOD, TIMEOUT_PERIOD);
        }

        private void checkConnection()
        {
            if (!this.IsConnected()) Reset("resetting on heartbeat");
         }
        private void listenToStatusChangedForReset(object sender, StatusChangedEventArgs e)
        {
            if (e.isConnected) {
                StopConnectionTimeoutTimer();
            }
            if (conn == null) return;
            switch (conn.XmppConnectionState)
            {
                // fall through
                case XmppConnectionState.Connecting:
                case XmppConnectionState.Authenticating:
                    return;
            }
            if (!e.isConnected && !this.IsConnected()) Reset("Jabberwire::listenToStatusChangedForReset");
        }

        private void networkAvailabilityChanged(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                receiveEvents.statusChanged(true, this.credentials);
                catchUpDisconnectedWork();
            }
            else
                receiveEvents.statusChanged(false, this.credentials);
        }

        private void makeAvailableNewSocket()
        {
            if (this.conn != null) { 
                conn.OnAuthError -= OnAuthError;
                conn.OnLogin -= OnLogin;
                conn.OnMessage -= OnMessage;
                conn.OnSocketError -= HandlerError;
                conn.OnSocketError -= Disconnect;
                conn.OnError -= HandlerError;
                conn.OnRegisterError -= ElementError;
                conn.OnStreamError -= ElementError;
                conn.OnClose -= OnClose;
    #if DEBUG
                conn.OnReadXml -= ReadXml;
                conn.OnWriteXml -= WriteXml;
    #endif
            }
            this.conn = new XmppClientConnection(jid.Server);
            conn.UseSSL = false;
            conn.AutoAgents = false;
            conn.OnAuthError += OnAuthError;
            conn.OnLogin += OnLogin;
            conn.OnMessage += OnMessage;
            conn.OnPresence += OnPresence;
            conn.OnSocketError += HandlerError;
            conn.OnSocketError += Disconnect;
            conn.OnError += HandlerError;
            conn.OnRegisterError += ElementError;
            conn.OnStreamError += ElementError;
            conn.OnClose += OnClose;
#if DEBUG
            conn.OnReadXml += ReadXml;
            conn.OnWriteXml += WriteXml;
#endif
        }

        private void SendPing(string who)
        {
            directCommand(who, string.Format("{0} {1}", PING, credentials.name));
        }
        private Jid createJid(string username)
        {
            return new Jid(username + "@" + metlServerAddress.host);
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
                usernameToBeRegistered = conn.Username;
                Reset("Auth failed");
            }
            else
            {
                throw new AuthenticationException(error.ToString());
            }
        }
        private static string usernameToBeRegistered;
        public void openConnection()
        {
            var resource = DateTimeFactory.Now().Ticks.ToString();
            jid.Resource = resource;
            makeAvailableNewSocket();
            if (!String.IsNullOrEmpty(usernameToBeRegistered) && usernameToBeRegistered == jid.User)
            {
                conn.RegisterAccount = true;
                usernameToBeRegistered = null;
            }
            StartConnectionTimeoutTimer();
            conn.Open(jid.User, "examplePassword", resource, 1);
        }
       
        private void OnLogin(object o)
        {
            receiveEvents.statusChanged(true, this.credentials);
            joinRooms();
            catchUpDisconnectedWork();
        }

        private void OnPresence(object sender, Presence pres)
        {
            receiveEvents.receivePresence(new MeTLPresence{Joining = pres.Type.ToString().ToLower() != "unavailable", Who=pres.From.Resource, Where = pres.From.User});
        }
        private void OnMessage(object sender, Message message)
        {
            if (message.To.Resource == jid.Resource)
                ReceivedMessage(message, MessageOrigin.Live);
        }
        private void HandlerError(object sender, Exception ex)
        {
            Trace.TraceError(string.Format("MeTLLib::Providers::Connection:JabberWire:Handler error: {0}", ex.Message));
            Reset("JabberWire::HandlerError");
        }
        private void Disconnect(object sender, Exception ex)
        {
            Trace.TraceError(string.Format("MeTLLib::Providers::Connection:JabberWire:Disconnect: {0}", ex.Message));
            disconnectSocket();
        }
        private void disconnectSocket()
        {
            if (conn == null) return;

            try
            {
                conn.SocketDisconnect();
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("MeTLLib::Providers::Connection:JabberWire:disconnectSocket: {0}", ex.Message));
            }
        }
        private void ElementError(object sender, Element element)
        {
            Trace.TraceError(string.Format("MeTLLib::Providers::Connection:JabberWire:Element error: {0}", element.ToString()));
            Reset("JabberWire::ElementError");
        }
        protected virtual void ReadXml(object sender, string xml)
        {
#if DEBUG_VERBOSE
            if (!xml.Contains("/WORM_MOVES"))
                Trace.TraceInformation("IN:" + xml);
#endif
        }
        protected virtual void WriteXml(object sender, string xml)
        {
#if DEBUG_VERBOSE
            if (!xml.Contains("/WORM_MOVES"))
                Trace.TraceInformation("OUT:" + xml);
#endif
        }
        private void OnClose(object sender)
        {
            receiveEvents.statusChanged(false, credentials);
            Reset("OnClose");
        }
        private static ActionsAfterReloginQueue actionsAfterRelogin = new ActionsAfterReloginQueue();
        public void AddActionToReloginQueue(Action action)
        {
            receiveEvents.statusChanged(false, credentials);
            actionsAfterRelogin.Enqueue(action);
        }

        private static int catchUpDisconnectedWorkInProgress = 0;
        private void catchUpDisconnectedWork() 
        {
            if (1 == Interlocked.Increment(ref catchUpDisconnectedWorkInProgress))
            {
                try
                {
                    var catchUpTimer = new System.Timers.Timer(500);
                    catchUpTimer.Elapsed += catchUpTimer_Elapsed;
                    catchUpTimer.Start();
               }
                finally
                {
                    Interlocked.Exchange(ref catchUpDisconnectedWorkInProgress, 0);
                }
            }
        }

        private static int catchUpTimerElapsedInProgress = 0;
        private void catchUpTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // guard against timer being called again while in progress
            if (1 == Interlocked.Increment(ref catchUpTimerElapsedInProgress))
            {
                try
                {
                    while (IsConnected() && actionsAfterRelogin.Count > 0)
                    {
                        var item = (Action)actionsAfterRelogin.Peek();//Do not alter the queue, we might be back here any second
                        try
                        {
                            System.Windows.Threading.Dispatcher.CurrentDispatcher.adopt(item);
                            actionsAfterRelogin.Dequeue(); //We only lift it off the top after successful execution.
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("CRASH: MeTLLib::Providers:JabberWire:AttemptReloginAfter Failed to execute item on relogin-queue.  Exception: " + ex.Message);
                            break;
                        }
                    }
        
                    if (actionsAfterRelogin.Count == 0)
                    {
                        var catchUpTimer = sender as System.Timers.Timer;
                        catchUpTimer.Stop();
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref catchUpTimerElapsedInProgress, 0);
                }
            }
        }
        
        
        public void Logout()
        {
            shutdownHeartBeat();
            conn.OnClose -= OnClose;//Don't automatically reconnect this time
            conn.Close();
        }
        public void Login()
        {
        }
        protected IConversationDetailsProvider conversationDetailsProvider;
        protected HttpHistoryProvider historyProvider;
        protected CachedHistoryProvider cachedHistoryProvider;
        protected MeTLServerAddress metlServerAddress;
        private static int resetInProgress = 0;

        private void Reset(string caller)
        {
            if (1 == Interlocked.Increment(ref resetInProgress))
            {
                if (conn != null)
                    switch (conn.XmppConnectionState)
                    {
                        case XmppConnectionState.Disconnected:
                            Trace.TraceWarning(string.Format("CRASH: JabberWire::Reset: Resetting.  {0}", caller));
                            openConnection();
                            break;
                        case XmppConnectionState.Connecting:
                            // Timed out trying to connect, so disconnect and start again
                            disconnectSocket();
                            openConnection();
                            break;
                        case XmppConnectionState.Authenticating:
                            disconnectSocket();
                            openConnection();
                            break;
                    }
                else
                {
                    Trace.TraceWarning(string.Format("CRASH: JabberWire::Reset: Conn is null - openingConnection.  {0}", caller));
                    openConnection();
                }

                Interlocked.Exchange(ref resetInProgress, 0);
            }
            else
            {
                Trace.TraceWarning(string.Format("++++: JabberWire::Reset: Called {0} times.", resetInProgress));
            }
        }
        public void leaveRooms(bool stayInGlobal = false, bool stayInActiveConversation = false)
        {
            var rooms = new List<Jid>();
            if (!stayInGlobal)
            {
                rooms.Add(metlServerAddress.global);
            }
            rooms.Add(new Jid(credentials.name, metlServerAddress.muc, jid.Resource));

            if (!stayInActiveConversation)
            {
                rooms.Add(new Jid(location.activeConversation, metlServerAddress.muc, jid.Resource));
            }
            
            if (location != null)
                rooms.AddRange(
                    new[]{
                        new Jid(location.currentSlide.ToString(), metlServerAddress.muc,jid.Resource),
                        new Jid(string.Format("{0}{1}", location.currentSlide, credentials.name), metlServerAddress.muc,jid.Resource)
                    });
            foreach (var room in rooms)
            {
                leaveRoom(room);
            }
        }
        private bool isLocationValid()
        {
            return (location != null && !String.IsNullOrEmpty(location.activeConversation) && location.availableSlides.Count > 0 && location.currentSlide > 0 && !location.ValueEquals(Location.Empty));
        }
        private void leaveRoom(Jid room)
        {
            var alias = credentials.name + conn.Resource;
            new MucManager(conn).LeaveRoom(room, alias);
        }
        public void resetLocation()
        {
            leaveRooms();
            location = Location.Empty;
            joinRooms();
            receiveEvents.receiveConversationDetails(ConversationDetails.Empty);
        }
        private void joinRooms(bool fastJoin = false, bool alreadyInConversation = false)
        {
            if (!fastJoin)
            {
                leaveRooms();
                joinRoom(metlServerAddress.global);
            }
            if (isLocationValid())
            {
                var rooms = new List<Jid>();
                rooms.AddRange(new[]
                {
                    new Jid(credentials.name, metlServerAddress.muc, jid.Resource),
                    new Jid(location.currentSlide.ToString(), metlServerAddress.muc,jid.Resource),
                    new Jid(string.Format("{0}{1}", location.currentSlide, credentials.name), metlServerAddress.muc,jid.Resource)
                });

                if (!alreadyInConversation)
                {
                    rooms.Add(new Jid(location.activeConversation,metlServerAddress.muc,jid.Resource));
                }
                foreach (var room in rooms.Where(r => r.User != null && r.User != "0"))
                {
                    joinRoom(room);
                }
            }
        }
        private void joinRoom(Jid room)
        {
            try
            {
                Trace.WriteLine(String.Format("Jabberwire::JoinRoom => Joining room {0}", room));
                var alias = credentials.name + conn.Resource;
                new MucManager(conn).JoinRoom(room, alias, true);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("Couldn't join room {0}: {1}", room, e.Message));
            }
        }
        private void send(string target, string message)
        {
            send(new Message(new Jid(target + "@" + metlServerAddress.muc), jid, MessageType.groupchat, message));
        }
        protected virtual void send(Message message)
        {
            conn.Send(message);
        }
        private bool compareString(string a, string b){
            if (String.IsNullOrEmpty(a) || String.IsNullOrEmpty(b))
                return false;
            return a.ToLower().Trim() == b.ToLower().Trim();
        }
        public void stanza(string target, Element stanza)
        {
            var message = new Message();
            string modifiedTarget =
                compareString(stanza.GetTag(MeTLStanzas.privacyTag),"private") ?
                string.Format("{0}{1}", target, stanza.GetTag("author")) : target;
            message.To = new Jid(string.Format("{0}@{1}", modifiedTarget, metlServerAddress.muc));
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
            send(new Message(new Jid(target + "@" + metlServerAddress.host), jid, MessageType.chat, message));
        }
        private void onStart()
        {
        }
        private void onProgress(int upTo, int outOf)
        {
            //Commands.RetrievedHistoryPortion.Execute(new[] { upTo, outOf });
        }
        public void sendSlideCollectionUpdatedCommand(Int32 conversationJid)
        {
            var jid = conversationJid.ToString();
            send(jid, String.Format("{0} {1}", UPDATE_SLIDE_COLLECTION, jid));
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
            try
            {
                Func<Boolean> checkPing = delegate
                {
                    var healthy = false;
                    var uri = metlServerAddress.uri;
                    var ping = new System.Net.NetworkInformation.Ping();
                    var reply = ping.Send(uri.Host, 2000);
                    if (reply != null && reply.Status == IPStatus.Success)
                    {
                        healthy = true;
                    }
                    return healthy;
                };
                return conn.Authenticated && checkPing();
            }
            catch (Exception) {
                return false;
            }
        }
        public void GetHistory(int where)
        {
            historyProvider.Retrieve<PreParser>(
                onStart,
                onProgress,
                finishedParser => receiveEvents.receivePreParser(finishedParser),
                where.ToString());
            historyProvider.RetrievePrivateContent<PreParser>(
                onStart,
                onProgress,
                finishedParser => receiveEvents.receivePreParser(finishedParser),
                credentials.name,
                where.ToString());
        }

        public void JoinConversation()
        {
            try
            {
                leaveRooms();
                joinRooms();
            }
            catch (Exception e)
            {
                Trace.TraceInformation("CRASH: MeTLLib::JabberWire:JoinConversation {0}", e.Message);
            }
        }

        public void MoveTo(int where)
        {
            try
            {
                leaveRooms(stayInGlobal: true, stayInActiveConversation: true);
                new MucManager(conn).LeaveRoom(
                    new Jid(string.Format("{0}{1}", location.currentSlide, credentials.name), metlServerAddress.muc, jid.Resource), credentials.name);
                var currentDetails = conversationDetailsProvider.DetailsOf(location.activeConversation);
                location.availableSlides = currentDetails.Slides.Select(s => s.id).ToList();
                var oldLocation = location.currentSlide.ToString();
                location.currentSlide = where;
                Globals.conversationDetails = currentDetails;
                Globals.slide = where;
                joinRooms(fastJoin: true, alreadyInConversation: true);
                cachedHistoryProvider.ClearCache(oldLocation);
                cachedHistoryProvider.ClearCache(where.ToString());
                historyProvider.Retrieve<PreParser>(
                    onStart,
                    onProgress,
                    finishedParser => { 
                        receiveEvents.receivePreParser(finishedParser); 
                        cachedHistoryProvider.PopulateFromHistory(finishedParser); 
                    },
                    location.currentSlide.ToString());
                historyProvider.RetrievePrivateContent<PreParser>(
                    onStart,
                    onProgress,
                    finishedParser => 
                    { 
                        receiveEvents.receivePreParser(finishedParser); 
                        cachedHistoryProvider.PopulateFromHistory(finishedParser); 
                    },
                    credentials.name,
                    location.currentSlide.ToString());
                

            }
            catch (Exception e) {
                Trace.TraceInformation("CRASH: MeTLLib::JabberWire:MoveTo {0}", e.Message);
            }
        }

        private void MoveDeltaSeparator(TargettedMoveDelta tmd)
        {
            var room = tmd.slide.ToString();
            var strokes = cachedHistoryProvider.GetInks(room);
            var images = cachedHistoryProvider.GetImages(room);
            var texts = cachedHistoryProvider.GetTexts(room);

            var inkIds = tmd.inkIds.Select(elemId => elemId.Identity).ToList();
            var textIds = tmd.textIds.Select(elemId => elemId.Identity).ToList();
            var imageIds = tmd.imageIds.Select(elemId => elemId.Identity).ToList();

            var privateInks = strokes.Where(s => inkIds.Contains(s.identity) && s.privacy == Privacy.Private).ToList();
            var privateImages = images.Where(i => imageIds.Contains(i.identity) && i.privacy == Privacy.Private).ToList();
            var privateTexts = texts.Where(t => textIds.Contains(t.identity) && t.privacy == Privacy.Private).ToList();
            var publicInks = strokes.Where(s => inkIds.Contains(s.identity) && s.privacy == Privacy.Public).ToList();
            var publicImages = images.Where(i => imageIds.Contains(i.identity) && i.privacy == Privacy.Public).ToList();
            var publicTexts = texts.Where(t => textIds.Contains(t.identity) && t.privacy == Privacy.Public).ToList();

            switch (tmd.newPrivacy)
            {
                case Privacy.Public:
                    {
                        var notP = Privacy.Public;
                        TargettedMoveDelta privateDirtier = null;
                        TargettedMoveDelta publicAdjuster = null;

                        var privateInksToPublicise = privateInks.Select(i => i.AlterPrivacy(tmd.newPrivacy).AdjustVisual(tmd.xTranslate, tmd.yTranslate, tmd.xScale, tmd.yScale)).ToList();
                        var privateTextsToPublicise = privateTexts.Select(i => i.AlterPrivacy(tmd.newPrivacy).AdjustVisual(tmd.xTranslate, tmd.yTranslate, tmd.xScale, tmd.yScale)).ToList();
                        var privateImagesToPublicise = privateImages.Select(i => i.AlterPrivacy(tmd.newPrivacy).AdjustVisual(tmd.xTranslate, tmd.yTranslate, tmd.xScale, tmd.yScale)).ToList();

                        if (privateInksToPublicise.Count() > 0 || privateTextsToPublicise.Count() > 0 || privateImagesToPublicise.Count() > 0)
                        {
                            privateDirtier = TargettedMoveDelta.CreateDirtier(tmd, notP, privateInksToPublicise, privateTextsToPublicise, privateImagesToPublicise);                             
                        }
                        if (publicInks.Count() > 0 || publicTexts.Count() > 0 || publicImages.Count() > 0)
                        {
                            publicAdjuster = TargettedMoveDelta.CreateAdjuster(tmd, notP, publicInks, publicTexts, publicImages);
                            publicAdjuster.privacy = notP;
                        }

                        MoveDeltaDispatcher(publicAdjuster, privateDirtier, privateInksToPublicise, privateTextsToPublicise, privateImagesToPublicise);
                    }
                    break;

                case Privacy.Private:
                    {
                        var notP = Privacy.Private;
                        TargettedMoveDelta publicDirtier = null;
                        TargettedMoveDelta privateAdjuster = null;

                        var publicInksToPrivatise = publicInks.Select(i => i.AlterPrivacy(tmd.newPrivacy).AdjustVisual(tmd.xTranslate, tmd.yTranslate, tmd.xScale, tmd.yScale)).ToList();
                        var publicTextsToPrivatise = publicTexts.Select(i => i.AlterPrivacy(tmd.newPrivacy).AdjustVisual(tmd.xTranslate, tmd.yTranslate, tmd.xScale, tmd.yScale)).ToList();
                        var publicImagesToPrivatise = publicImages.Select(i => i.AlterPrivacy(tmd.newPrivacy).AdjustVisual(tmd.xTranslate, tmd.yTranslate, tmd.xScale, tmd.yScale)).ToList();

                        if (publicInksToPrivatise.Count() > 0 || publicTextsToPrivatise.Count() > 0 || publicImagesToPrivatise.Count() > 0)
                        {
                            publicDirtier = TargettedMoveDelta.CreateDirtier(tmd, notP, publicInksToPrivatise, publicTextsToPrivatise, publicImagesToPrivatise);
                        }
                        if (privateInks.Count() > 0 || privateTexts.Count() > 0 || privateImages.Count() > 0)
                        {
                            privateAdjuster = TargettedMoveDelta.CreateAdjuster(tmd, notP, privateInks, privateTexts, privateImages);
                            privateAdjuster.privacy = notP;
                        }

                        MoveDeltaDispatcher(privateAdjuster, publicDirtier, publicInksToPrivatise, publicTextsToPrivatise, publicImagesToPrivatise);
                    }
                    break;
                default:
                    {
                        TargettedMoveDelta privateDelta = null;
                        TargettedMoveDelta publicDelta = null;
                        if (privateInks.Count() > 0 || privateTexts.Count() > 0 || privateImages.Count() > 0)
                        {
                            privateDelta = TargettedMoveDelta.CreateAdjuster(tmd, Privacy.NotSet, privateInks, privateTexts, privateImages);
                            privateDelta.privacy = Privacy.Private;
                        }
                        if (publicInks.Count() > 0 || publicTexts.Count() > 0 || publicImages.Count() > 0)
                        {
                            publicDelta = TargettedMoveDelta.CreateAdjuster(tmd, Privacy.NotSet, publicInks, publicTexts, publicImages);
                            publicDelta.privacy = Privacy.Public;
                        }
                        MoveDeltaDispatcher(privateDelta, publicDelta);
                    }
                    break;
            }
        }

        private void SendCollection<T>(IEnumerable<T> collection, Action<T> sender)
        {
            if (collection != null)
            {
                foreach (var elem in collection)
                {
                    sender(elem);
                }
            }
        }
        private void MoveDeltaDispatcher(TargettedMoveDelta adjuster, TargettedMoveDelta dirtier, IEnumerable<TargettedStroke> strokes = null, IEnumerable<TargettedTextBox> texts = null, IEnumerable<TargettedImage> images = null)
        {
            if (dirtier != null) 
                stanza(dirtier.slide.ToString(), new MeTLStanzas.MoveDeltaStanza(dirtier));
            if (adjuster != null)
                stanza(adjuster.slide.ToString(), new MeTLStanzas.MoveDeltaStanza(adjuster));

            // run appropriate stanza constructors
            SendCollection<TargettedStroke>(strokes, s => SendStroke(s));
            SendCollection<TargettedTextBox>(texts, t => SendTextbox(t));
            SendCollection<TargettedImage>(images, i => SendImage(i));
        }

        public void SendStroke(TargettedStroke stroke)
        {
            stanza(stroke.slide.ToString(), new MeTLStanzas.Ink(stroke));
        }
        public void SendScreenshotSubmission(TargettedSubmission submission)
        {
            stanza(Globals.location.activeConversation, new MeTLStanzas.ScreenshotSubmission(submission));
        }
        public void AskForTeacherStatus(string teacher, string where)
        {
            Console.WriteLine("Jabberwire:AskTeacherForStatus => sending on conversation: " + where);
            command(where, TEACHER_IN_CONVERSATION + " "+ where);
        }
        public void SendSyncMoveTo(int where)
        {
            command(location.activeConversation, SYNC_MOVE + " " + where);
        }
        public void SendImage(TargettedImage image)
        {
            stanza(image.slide.ToString(), new MeTLStanzas.Image(image));
        }
        public void sendFileResource(TargettedFile file)
        {
            var fileResource = new MeTLStanzas.FileResource(file);
            if (Globals.location.availableSlides.Contains(file.slide))
                stanza(Globals.location.activeConversation, fileResource);
            else
            {
                var fileConversation = file.conversationJid.ToString();
                var fileConversationJid = new Jid(fileConversation + "@" + metlServerAddress.muc);
                joinRoom(fileConversationJid);
                stanza(fileConversation.ToString(), fileResource);
                if (fileConversation != Globals.location.activeConversation)
                    leaveRoom(fileConversationJid);
            }
        }
        public void SendStanza(string where, Element what) {
            stanza(where, what);
        }
        private void SendChat(TargettedTextBox message)
        {
            stanza(location.activeConversation, new MeTLStanzas.TextBox(message));
        }
        public void SendTeacherStatus(TeacherStatus status)
        {
            stanza("global", new MeTLStanzas.TeacherStatusStanza(status));
        }
        public void SendTextbox(TargettedTextBox box)
        {
            stanza(box.slide.ToString(), new MeTLStanzas.TextBox(box));
        }
        public void SendMoveDelta(TargettedMoveDelta tmd)
        {
            // split up the TargettedMoveDelta if content is moving across the privacy boundary
            MoveDeltaSeparator(tmd);
        }
        public void SendQuiz(QuizQuestion parameters)
        {
            var quiz = new MeTLStanzas.Quiz(parameters);
            stanza(Globals.location.activeConversation, quiz);
        }
        public void SendQuizAnswer(QuizAnswer parameters)
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
        //    stanza(element.slide.ToString(), new MeTLStanzas.DirtyInk(element));
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
                    case TEACHER_IN_CONVERSATION:
                        handleTeacherInConversation(parts);
                        break;
                    case SYNC_MOVE:
                        handleSyncMoveReceived(parts);
                        break;
                    case UPDATE_CONVERSATION_DETAILS:
                        handleConversationDetailsUpdated(parts);
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
                    case UPDATE_SLIDE_COLLECTION:
                        handleUpdateSlideCollection(parts);
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
        public virtual void handleUpdateSlideCollection(string[] parts)
        {
            try
            {
                receiveEvents.receiveUpdatedSlideCollection(Int32.Parse(parts[1]));
            }
            catch (Exception e)
            {
                Trace.TraceError("wire received inappropriate jid in updateSlideCollection: " + e.Message);
            }
        }
        /*public virtual void handleGoToConversation(string[] parts)
        {
            JoinConversation(parts[1]);
        }*/
        public virtual void handleGoToSlide(string[] parts)
        {
            var id = Int32.Parse(parts[1]);
            var desiredConversation = Slide.ConversationFor(id).ToString();
            MoveTo(id);
        }
        public virtual void handleWakeUp(string[] parts)
        {
            Commands.ReceiveWakeUp.Execute(null);
        }
        public virtual void handleSleep(string[] parts)
        {
            Commands.ReceiveSleep.Execute(null);
        }

        private List<MeTLStanzas.TimestampedMeTLElement> SortOnTimestamp(List<MeTLStanzas.TimestampedMeTLElement> nodeList)
        {
            return nodeList.OrderBy(k => k.timestamp).ToList();
        }

        public MeTLStanzas.TimestampedMeTLElement ContructElement(Node node)
        {
            return new MeTLStanzas.TimestampedMeTLElement(node as Element);
        }

        public List<MeTLStanzas.TimestampedMeTLElement> unOrderedMessages = new List<MeTLStanzas.TimestampedMeTLElement>();
        List<MeTLStanzas.TimestampedMeTLElement> orderedMessages = new List<MeTLStanzas.TimestampedMeTLElement>();

        public void ReceiveAndSortMessages()
        {
            orderedMessages = SortOnTimestamp(unOrderedMessages);
            foreach ( MeTLStanzas.TimestampedMeTLElement message in orderedMessages)
            {
                HistoryReceivedMessage(message);
                //ReceivedMessage(message, MessageOrigin.History);
            }
        }

        public virtual void HistoryReceivedMessage(MeTLStanzas.TimestampedMeTLElement element)
        {
            var message = element.element;
            var timestamp = element.timestamp;
            //var element = new MeTLStanzas.TimestampedMeTLElement(message);

            if (message.GetAttribute("type") == "error")
            {
                Trace.TraceError("Wire received error message: {0}", message);
                return;
            }
            if (message.SelectSingleElement("body") != null)
            {
                ReceiveCommand(message.SelectSingleElement("body").InnerXml);
                return;
            }           

            //ActOnUntypedMessage(message, timestamp);
            ActOnUntypedMessage(element);
        }

        public virtual void ReceivedMessage(Node node, MessageOrigin messageOrigin)
        {
            var message = node as Element;
            var timestamp = TimeStampedMessage.getTimestamp(message);

            var element = new MeTLStanzas.TimestampedMeTLElement(message);

            if (message.GetAttribute("type") == "error")
            {
                Trace.TraceError("Wire received error message: {0}", message);
                return;
            }
            if (message.SelectSingleElement("body") != null)
            {
                ReceiveCommand(message.SelectSingleElement("body").InnerXml);
                return;
            }
            if (messageOrigin == MessageOrigin.Live)
            {
                //cachedHistoryProvider.HandleMessage(location.currentSlide, message, timestamp);
                cachedHistoryProvider.HandleMessage(location.currentSlide, element);
            }


            ActOnUntypedMessage(element);
            //ActOnUntypedMessage(message, timestamp);
        }

        //public void ActOnUntypedMessage(Element message, long timestamp)
        public void ActOnUntypedMessage(MeTLStanzas.TimestampedMeTLElement timestampedElement)
        {
            foreach (var status in timestampedElement.element.SelectElements<MeTLStanzas.TeacherStatusStanza>(true))
                actOnStatusRecieved(status);

            foreach (var moveDelta in timestampedElement.element.SelectElements<MeTLStanzas.MoveDeltaStanza>(true))
            {
                var parameters = moveDelta.parameters;
                parameters.timestamp = timestampedElement.timestamp;
                moveDelta.parameters = parameters;
                actOnMoveDelta(moveDelta);
            }
            foreach (var dirtyText in timestampedElement.element.SelectElements<MeTLStanzas.DirtyText>(true))
                actOnDirtyTextReceived(dirtyText);
            foreach (var dirtyInk in timestampedElement.element.SelectElements<MeTLStanzas.DirtyInk>(true))
                actOnDirtyStrokeReceived(dirtyInk);
            foreach (var dirtyImage in timestampedElement.element.SelectElements<MeTLStanzas.DirtyImage>(true))
                actOnDirtyImageReceived(dirtyImage);

            foreach (var ink in timestampedElement.element.SelectElements<MeTLStanzas.Ink>(true))
            {
                var targettedStroke = ink.Stroke;                
                targettedStroke.timestamp = timestampedElement.timestamp;
                var stroke = targettedStroke.stroke;
                stroke.tag(new StrokeTag(stroke.tag(), timestampedElement.timestamp));
                actOnStrokeReceived(targettedStroke);
            }
            foreach (var submission in timestampedElement.element.SelectElements<MeTLStanzas.ScreenshotSubmission>(true))
                actOnScreenshotSubmission(submission.injectDependencies(metlServerAddress).parameters);
            foreach (var box in timestampedElement.element.SelectElements<MeTLStanzas.TextBox>(true))
            {
                var targettedBox = box.Box;
                targettedBox.timestamp = timestampedElement.timestamp;
                //var textBox = targettedBox.box;
                //textBox.tag(new TextTag(textBox.tag(), timestampedElement.timestamp));
                actOnTextReceived(targettedBox);
            }
            foreach (var image in timestampedElement.element.SelectElements<MeTLStanzas.Image>(true))
            {
                //var targettedImage = image.Img;
                var targettedImage = image.injectDependencies(metlServerAddress, webClientFactory.client(), resourceProvider).Img;
                targettedImage.timestamp = timestampedElement.timestamp;
                //var imageTemp = targettedImage.image;
                //imageTemp.tag(new ImageTag(imageTemp.tag(), timestampedElement.timestamp));
                //actOnImageReceived(image.injectDependencies(metlServerAddress, webClientFactory.client(), resourceProvider).Img);
                actOnImageReceived(targettedImage);
            }
            foreach (var quiz in timestampedElement.element.SelectElements<MeTLStanzas.Quiz>(true))
                actOnQuizReceived(quiz.injectDependencies(metlServerAddress).parameters);
            foreach (var quizAnswer in timestampedElement.element.SelectElements<MeTLStanzas.QuizResponse>(true))
                actOnQuizAnswerReceived(quizAnswer.parameters);
            
            foreach (var file in timestampedElement.element.SelectElements<MeTLStanzas.FileResource>(true))
                actOnFileResource(file.injectDependencies(metlServerAddress));
            
        }

        public virtual void actOnStatusRecieved(MeTLStanzas.TeacherStatusStanza status)
        {
            receiveEvents.teacherStatusRecieved(status.status);
        }

        public virtual void actOnFileResource(MeTLStanzas.FileResource resource)
        {
            receiveEvents.receiveFileResource(resource.fileResource);
        }
        public virtual void actOnScreenshotSubmission(TargettedSubmission submission)
        {
            receiveEvents.receiveSubmission(submission);
        }
        public virtual void actOnDirtyAutoshapeReceived(MeTLStanzas.DirtyAutoshape dirtyAutoShape)
        {
            receiveEvents.receiveDirtyAutoShape(dirtyAutoShape.element);
        }
        public virtual void actOnDirtyVideoReceived(MeTLStanzas.DirtyVideo dirtyVideo)
        {
            receiveEvents.receiveDirtyVideo(dirtyVideo.element);
        }
        public virtual void actOnDirtyImageReceived(MeTLStanzas.DirtyImage dirtyImage)
        {
            receiveEvents.receiveDirtyImage(dirtyImage.element);
        }
        public virtual void actOnDirtyStrokeReceived(MeTLStanzas.DirtyInk element)
        {
            receiveEvents.receiveDirtyStroke(element.element);
        }
        public virtual void actOnDirtyTextReceived(MeTLStanzas.DirtyText dirtyText)
        {
            receiveEvents.receiveDirtyTextBox(dirtyText.element);
        }
        public virtual void actOnMoveDelta(MeTLStanzas.MoveDeltaStanza moveDelta)
        {
            receiveEvents.receiveMoveDelta(moveDelta.parameters);
        }
        public virtual void actOnImageReceived(TargettedImage image)
        {
            receiveEvents.receiveImage(image);
        }
        public virtual void actOnQuizReceived(QuizQuestion quiz)
        {
            receiveEvents.receiveQuiz(quiz);
        }
        public virtual void actOnQuizAnswerReceived(QuizAnswer quizAnswer)
        {
            receiveEvents.receiveQuizAnswer(quizAnswer);
        }
        public virtual void actOnStrokeReceived(TargettedStroke stroke)
        {
            receiveEvents.receiveStroke(stroke);
        }
        public virtual void actOnTextReceived(TargettedTextBox box)
        {
            if (box.target == "chat")
                receiveEvents.receiveChat(box);
            else
                receiveEvents.receiveTextBox(box);
        }
        public virtual void actOnLiveWindowReceived(LiveWindowSetup window)
        {
            receiveEvents.receiveLiveWindow(window);
        }
        public virtual void actOnDirtyLiveWindowReceived(TargettedDirtyElement element)
        {
            receiveEvents.receiveDirtyLiveWindow(element);
        }
        public void SneakInto(string room)
        {
            var muc = new MucManager(conn);
            joinRoom(new Jid(room + "@" + metlServerAddress.muc));
        }
        public void SneakOutOf(string room)
        {
            var muc = new MucManager(conn);
            muc.LeaveRoom(new Jid(room + "@" + metlServerAddress.muc), credentials.name);
        }
        private void handleTeacherInConversation(string[] parts)
        {
            var where = parts[1];
            receiveEvents.teacherStatusRequest(where, "");
        }
        private void handleSyncMoveReceived(string[] parts)
        {
            var where = Int32.Parse(parts[1]);
            receiveEvents.syncMoveRequested(where);
        }
        private void handleConversationDetailsUpdated(string[] parts)
        {
            var jid = parts[1];
            var newDetails = conversationDetailsProvider.DetailsOf(jid);
            receiveEvents.receiveConversationDetails(newDetails);
        }
        protected virtual void handleUnknownMessage(string message)
        {
            Trace.TraceWarning(string.Format("Received unknown message: {0}", message));
        }

        public void LoadSubmissions(string conversationJid)
        {
             historyProvider.Retrieve<PreParser>(
            onStart,
            onProgress,
            finishedParser => receiveEvents.receivesubmissions(finishedParser),
            conversationJid.ToString());
        }
        public void LoadAttachments(string conversationJid)
        {
            historyProvider.Retrieve<PreParser>(
            onStart,
            onProgress,
            finishedParser => receiveEvents.receieveAttachments(finishedParser),
            conversationJid);
        }
        public void LoadQuizzes(string conversationJid)
        {
            historyProvider.Retrieve<PreParser>(
            onStart,
            onProgress,
            finishedParser => receiveEvents.receieveQuizzes(finishedParser),
            conversationJid.ToString());
        }

        public void LoadQuiz(int conversationJid, long quizId)
        {
            historyProvider.Retrieve<PreParser>(
            onStart,
            onProgress,
            finishedParser => receiveEvents.receieveQuiz(finishedParser, quizId),
            conversationJid.ToString());
        }
    }
}