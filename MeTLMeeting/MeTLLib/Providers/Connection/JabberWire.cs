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
using System.Net;
using System.ComponentModel;

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
        public JabberWireFactory(
            MetlConfiguration _config,
            Credentials _credentials,
            ConfigurationProvider _configurationProvider,
            IResourceUploader _resourceUploader,
            ResourceCache _cache,
            IReceiveEvents _receiveEvents,
            IWebClientFactory _clientFactory,
            HttpResourceProvider _resourceProvider,
            IAuditor _auditor
            )
        {
            auditor = _auditor;
            metlServerAddress = _config;
            credentials = _credentials;
            configurationProvider = _configurationProvider;
            resourceProvider = _resourceProvider;
            resourceUploader = _resourceUploader;
            historyProvider = new HttpHistoryProvider(resourceProvider, this, metlServerAddress, _auditor);
            cachedHistoryProvider = new CachedHistoryProvider(historyProvider, resourceProvider, this, metlServerAddress, _auditor);
            cache = _cache;
            receiveEvents = _receiveEvents;
            clientFactory = _clientFactory;
            conversationDetailsProvider = new FileConversationDetailsProvider(_config, _clientFactory, _resourceUploader, _credentials, auditor, this);
        }
        public IAuditor auditor { get; protected set; }
        public MetlConfiguration metlServerAddress { get; protected set; }
        public Credentials credentials { get; protected set; }
        public ConfigurationProvider configurationProvider { get; protected set; }
        public IConversationDetailsProvider conversationDetailsProvider { get; protected set; }
        public HttpHistoryProvider historyProvider { get; protected set; }
        public CachedHistoryProvider cachedHistoryProvider { get; protected set; }
        public ResourceCache cache { get; protected set; }
        public IResourceUploader resourceUploader { get; protected set; }
        public IReceiveEvents receiveEvents { get; protected set; }
        public IWebClientFactory clientFactory { get; protected set; }
        public HttpResourceProvider resourceProvider { get; protected set; }
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
                        resourceProvider, true, auditor);
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
                cache, receiveEvents, clientFactory, resourceProvider, auditor);
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
                resourceProvider,
                auditor
                );
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

    public partial class JabberWire : INotifyPropertyChanged
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
    
        public ResourceCache cache;
        public JabberWire(Credentials credentials, IConversationDetailsProvider conversationDetailsProvider, HttpHistoryProvider historyProvider, CachedHistoryProvider cachedHistoryProvider, MetlConfiguration metlServerAddress, ResourceCache cache, IReceiveEvents events, IWebClientFactory webClientFactory, HttpResourceProvider resourceProvider, bool active, IAuditor _auditor)
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
            this.auditor = _auditor;

            new MeTLLib.DataTypes.MeTLStanzasConstructor();
            this.activeWire = active;
            if (activeWire)
            {
                receiveEvents.StatusChanged += listenToStatusChangedForReset;
                establishHeartBeat();
                NetworkChange.NetworkAvailabilityChanged += networkAvailabilityChanged;
            }
        }
        public IAuditor auditor { get; protected set; }

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
        private void EstablishConnectionTimer()
        {
            ConnectionTimeoutTimer = new Timer((_unused) =>
            {
                if (conn == null || conn.XmppConnectionState != XmppConnectionState.Connected)
                    Reset("Resetting because connection took too long");
            }, null, TIMEOUT_PERIOD, TIMEOUT_PERIOD);
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
            if (e.isConnected)
            {
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
            if (!e.isConnected && !this.IsConnected())
            {
                Reset("Jabberwire::listenToStatusChangedForReset");
            }
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
            if (this.conn != null)
            {
                conn.OnAuthError -= OnAuthError;
                conn.OnLogin -= OnLogin;
                conn.OnMessage -= OnMessage;
                conn.OnSocketError -= HandlerError;
                conn.OnError -= HandlerError;
                conn.OnRegisterError -= ElementError;
                conn.OnStreamError -= ElementError;
                conn.OnClose -= OnClose;
                conn.OnIq -= OnIq;
/*
                conn.OnReadXml -= ReadXml;
                conn.OnWriteXml -= WriteXml;
*/
            }
            this.conn = new XmppClientConnection(jid.Server);
            conn.ConnectServer = metlServerAddress.xmppHost;
            conn.UseStartTLS = true;
            conn.UseSSL = false;
            conn.AutoAgents = false;
            conn.OnAuthError += OnAuthError;
            conn.OnLogin += OnLogin;
            conn.OnMessage += OnMessage;            
            conn.OnPresence += OnPresence;
            conn.OnSocketError += HandlerError;
            conn.OnError += HandlerError;
            conn.OnRegisterError += ElementError;
            conn.OnStreamError += ElementError;
            conn.OnClose += OnClose;
            conn.OnIq += OnIq;
/*
            conn.OnReadXml += ReadXml;
            conn.OnWriteXml += WriteXml;
*/
        }

        private void SendPing(string who)
        {
            directCommand(who, string.Format("{0} {1}", PING, credentials.name));
        }
        private Jid createJid(string username)
        {
            return new Jid(username + "@" + metlServerAddress.xmppDomain);
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
            auditor.wrapAction((a) =>
            {
                var resource = DateTimeFactory.Now().Ticks.ToString();
                jid.Resource = resource;
                makeAvailableNewSocket();
                a(GaugeStatus.InProgress, 33);
                if (!String.IsNullOrEmpty(usernameToBeRegistered) && usernameToBeRegistered == jid.User)
                {
                    conn.RegisterAccount = true;
                    usernameToBeRegistered = null;
                }
                StartConnectionTimeoutTimer();
                a(GaugeStatus.InProgress, 66);
                conn.Open(jid.User, credentials.password, resource, 1);
            }, "openConnection", "xmpp");
        }

        private void OnLogin(object o)
        {
            receiveEvents.statusChanged(true, this.credentials);
            joinRooms();
            catchUpDisconnectedWork();
        }

        private void OnPresence(object sender, Presence pres)
        {
            receiveEvents.receivePresence(new MeTLPresence { Joining = pres.Type.ToString().ToLower() != "unavailable", Who = pres.From.Resource, Where = pres.From.User });
        }
        private void OnMessage(object sender, Message message)
        {
            if (message.To.Resource == jid.Resource)
                ReceivedMessage(message, MessageOrigin.Live);
        }
        protected List<String> unrespondedPings = new List<String>();
        private void OnIq(object sender, IQ iq)
        {
            if (iq.FirstChild != null && iq.FirstChild.Namespace == "urn:ietf:params:xml:ns:xmpp-bind")
            {
                jid = iq.FirstChild.GetTagJid("jid"); ;
            }
            if (iq.Type == IqType.result && unrespondedPings.Contains(iq.Id))
            {
                unrespondedPings.RemoveAll(s => s == iq.Id);
                Console.WriteLine("result ping IQ received: " + iq.ToString());
            }
            if (iq.FirstChild != null && iq.FirstChild.Namespace == "urn:xmpp:ping" )
            {
                if (iq.Type == IqType.error)
                {
                    Console.WriteLine("error ping IQ received: " + iq.ToString());
                }               
                else
                {
                    var pong = new agsXMPP.protocol.component.IQ(IqType.result, iq.To, iq.From);
                    pong.Id = iq.Id;
                    conn.Send(pong);
                }
            }
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
            auditor.wrapAction((a) =>
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
            }, "catchUpDisconnectedWork", "xmpp");
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
        protected MetlConfiguration metlServerAddress;
        private static int resetInProgress = 0;

        private void Reset(string caller)
        {
            if (1 == Interlocked.Increment(ref resetInProgress))
            {
                Trace.TraceWarning(string.Format("JabberWire::Reset.  {0}", caller));
                if (conn != null)
                    switch (conn.XmppConnectionState)
                    {
                        case XmppConnectionState.Disconnected:
                            Trace.TraceWarning(string.Format("JabberWire::Reset: Disconnected.  {0}", caller));
                            openConnection();
                            break;
                        case XmppConnectionState.Connecting:
                            // Timed out trying to connect, so disconnect and start again
                            Trace.TraceWarning(string.Format("JabberWire::Reset: Connecting.  {0}", caller));
                            disconnectSocket();
                            openConnection();
                            break;
                        case XmppConnectionState.Authenticating:
                            Trace.TraceWarning(string.Format("JabberWire::Reset: Authenticating.  {0}", caller));
                            disconnectSocket();
                            openConnection();
                            break;
                    }
                else
                {
                    Trace.TraceWarning(string.Format("JabberWire::Reset: Conn is null - openingConnection.  {0}", caller));
                    openConnection();
                }

                Interlocked.Exchange(ref resetInProgress, 0);
            }
            else
            {
                Trace.TraceWarning(string.Format("++++: JabberWire::Reset: Called {0} times.", resetInProgress));
            }
        }
        public void leaveRooms()
        {            
            foreach (var roomJid in CurrentRooms.ToList())
            {
                leaveRoom(roomJid);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentRooms"));
        }
        private void leaveRoom(Jid room)
        {            
            CurrentRooms.Remove(room);            
            Trace.TraceInformation("Leaving room {0} leaves {1}", room, String.Join(",", CurrentRooms.Select(r => r.ToString())));
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
        public void WatchRoom(string slide)
        {
            joinRoom(new Jid(slide, metlServerAddress.muc,jid.Resource));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentRooms"));
            historyProvider.Retrieve<PreParser>(
                    onStart,
                    onProgress,
                    finishedParser => {
                        receiveEvents.receivePreParser(finishedParser);
                        cachedHistoryProvider.PopulateFromHistory(finishedParser);
                    },
                    slide);
        }
        private void joinRooms()
        {                        
            foreach (var roomJid in CurrentRooms)
            {
                joinRoom(roomJid);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentRooms"));
        }
        public HashSet<Jid> CurrentRooms { get; protected set; } = new HashSet<Jid>();       
        private void joinRoom(Jid room)
        {
            Trace.TraceInformation("Joining room {0}", room);
            try
            {                
                var alias = credentials.name + conn.Resource;
                new MucManager(conn).JoinRoom(room, alias, true);
                CurrentRooms.Add(room);                
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("Couldn't join room {0}: {1}", room, e.Message));
            }
        }
        private void send(string target, string message)
        {
            auditor.wrapAction((a) =>
            {
                send(new Message(new Jid(target + "@" + metlServerAddress.muc), jid, MessageType.groupchat, message));
            }, "send", "xmpp");
        }
        protected virtual void send(Message message)
        {
            conn.Send(message);
            refreshPingTimer();
        }
        private bool compareString(string a, string b)
        {
            if (String.IsNullOrEmpty(a) || String.IsNullOrEmpty(b))
                return false;
            return a.ToLower().Trim() == b.ToLower().Trim();
        }
        public void stanza(string target, Element stanza)
        {
            var message = new Message();
            string modifiedTarget =
                compareString(stanza.GetTag(MeTLStanzas.privacyTag), "private") ?
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
            send(new Message(new Jid(target + "@" + metlServerAddress.xmppDomain), jid, MessageType.chat, message));
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
                return auditor.wrapFunction((a) => {
                    var serverStatus = webClientFactory.client().downloadString(metlServerAddress.serverStatus).Trim().ToLower();
                    return conn.Authenticated &&  serverStatus == "ok";
                }, "healthCheck", "xmpp");
            }
            catch (Exception e)
            {
                //remove the webexception clause and return only false, when all the server have their serverStatus endpoint enabled on their yaws and apache pages
                Console.WriteLine("JabberWire:IsConnected: {0}", e.Message);
                return (e is WebException && (e as WebException).Response != null && ((e as WebException).Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound);
            }
        }
        public void GetHistory(int where)
        {
            auditor.wrapAction((g) =>
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
            }, "getHistory", "xmpp");
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

        public void SendAttendance(string target, Attendance attendance)
        {
            stanza(target, new MeTLStanzas.Attendance(attendance));
        }
        public void SendStroke(TargettedStroke stroke)
        {
            stanza(stroke.slide.ToString(), new MeTLStanzas.Ink(stroke));
        }
        public void SendScreenshotSubmission(TargettedSubmission submission)
        {
            stanza(location.activeConversation, new MeTLStanzas.ScreenshotSubmission(submission));
        }
        public void AskForTeacherStatus(string teacher, string where)
        {
            /*
            Console.WriteLine("Jabberwire:AskTeacherForStatus => sending on conversation: " + where);
            command(where, TEACHER_IN_CONVERSATION + " "+ where);
             */
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
            if (location.availableSlides.Contains(file.slide))
                stanza(location.activeConversation, fileResource);
            else
            {
                var fileConversation = file.conversationJid.ToString();
                var fileConversationJid = new Jid(fileConversation + "@" + metlServerAddress.muc);
                joinRoom(fileConversationJid);
                stanza(fileConversation.ToString(), fileResource);
                if (fileConversation != location.activeConversation)
                    leaveRoom(fileConversationJid);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentRooms"));
            }
        }
        protected int pingTimeout = 30 * 1000; // 30 seconds
        protected Timer pingTimer = null;
        protected void refreshPingTimer()
        {
            if (pingTimer == null)
            {
                pingTimer = new Timer((_state) => {
                    var pingIq = new IQ(IqType.get, jid, new Jid(metlServerAddress.xmppDomain));
                    pingIq.AddChild(new agsXMPP.protocol.extensions.ping.Ping());
                    pingIq.GenerateId();
                    unrespondedPings.Add(pingIq.Id);
                    conn.Send(pingIq);// new agsXMPP.protocol.extensions.ping.PingIq(new Jid(metlServerAddress.xmppDomain),jid));
                }, null, Timeout.Infinite, Timeout.Infinite);
            }
            pingTimer.Change(pingTimeout, pingTimeout);
        }
        public void SendStanza(string where, Element what)
        {
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
        public void SendQuiz(QuizQuestion parameters, string jid)
        {
            var quiz = new MeTLStanzas.Quiz(parameters);
            stanza(jid, quiz);
        }
        public void SendQuizAnswer(QuizAnswer parameters, string jid)
        {
            var quiz = new MeTLStanzas.QuizResponse(parameters);
            stanza(jid, quiz);
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
            //Commands.WakeUpBoards.Execute(room);
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
            //Commands.SleepBoards.Execute(room);
            /*This bit needs to be moved back into the application
            foreach (var board in BoardManager.boards[room])
                directCommand(room, SLEEP);*/
        }
        public void sendDirtyStroke(TargettedDirtyElement element)
        {
            //    stanza(element.slide.ToString(), new MeTLStanzas.DirtyInk(element));
            stanza(new MeTLStanzas.DirtyInk(element));
        }
        public virtual void ReceiveCommand(Element element)
        {
            try
            {
                var command = element.SelectSingleElement("command").InnerXml;
                switch (command)
                {
                    case TEACHER_IN_CONVERSATION:
                        handleTeacherInConversation(element);
                        break;
                    case SYNC_MOVE:
                        handleSyncMoveReceived(element);
                        break;
                    case UPDATE_CONVERSATION_DETAILS:
                        handleConversationDetailsUpdated(element);
                        break;                                      
                    case UPDATE_SLIDE_COLLECTION:
                        handleUpdateSlideCollection(element);
                        break;
                    default:
                        handleUnknownMessage(element);
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
            receiveEvents.handlePong(parts);
        }
        public virtual void handleUpdateSlideCollection(Element el)
        {
            try
            {
                receiveEvents.receiveUpdatedSlideCollection(Int32.Parse(el.SelectElements("parameter", true).Item(0).InnerXml));
            }
            catch (Exception e)
            {
                Trace.TraceError("wire received inappropriate jid in updateSlideCollection: " + e.Message);
            }
        }
        public virtual void handleWakeUp(string[] parts)
        {
            //Commands.ReceiveWakeUp.Execute(null);
        }
        public virtual void handleSleep(string[] parts)
        {
            //Commands.ReceiveSleep.Execute(null);
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void ReceiveAndSortMessages()
        {
            orderedMessages = SortOnTimestamp(unOrderedMessages);
            foreach (MeTLStanzas.TimestampedMeTLElement message in orderedMessages)
            {
                HistoryReceivedMessage(message);
                //ReceivedMessage(message, MessageOrigin.History);
            }
        }

        public virtual void HistoryReceivedMessage(MeTLStanzas.TimestampedMeTLElement element)
        {
            var message = element.element;
            message = HandleRelativePaths(message);

            var timestamp = element.timestamp;
            //var element = new MeTLStanzas.TimestampedMeTLElement(message);

            if (message.GetAttribute("type") == "error")
            {
                Trace.TraceError("Wire received error message: {0}", message);
                return;
            }
            var command = message.SelectSingleElement("command");
            if (command != null)
            {
                ReceiveCommand(command);
                return;
            }

            //ActOnUntypedMessage(message, timestamp);
            ActOnUntypedMessage(element);
        }


        //Converting source tag's path if absolute to relative
        public Element HandleRelativePaths(Element message)
        {
            if (message.HasTag("image"))
            {
                var imageTag = message.SelectSingleElement("image") as Element;
                if (imageTag.HasTag("source"))
                {
                    var sourceValue = imageTag.GetTag("source");
                    if (System.Uri.IsWellFormedUriString(sourceValue, System.UriKind.Absolute))
                    {
                        System.Uri sourceUri = new System.Uri(sourceValue);
                        sourceValue = sourceUri.AbsolutePath;
                        imageTag.SetTag("source", sourceValue);
                    }
                }
                message.ReplaceChild(imageTag);
            }
            return message;
        }

        public virtual void ReceivedMessage(Node node, MessageOrigin messageOrigin)
        {
            var message = node as Element;
            message = HandleRelativePaths(message);

            var element = new MeTLStanzas.TimestampedMeTLElement(message);

            if (message.GetAttribute("type") == "error")
            {
                Trace.TraceError("Wire received error message: {0}", message);
                return;
            }
            if (message.SelectSingleElement("command") != null)
            {
                ReceiveCommand(message.SelectSingleElement("command"));
                return;
            }
            if (messageOrigin == MessageOrigin.Live)
            {
                cachedHistoryProvider.HandleMessage(location.currentSlide, element);
            }
            ActOnUntypedMessage(element);
        }        
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
            {
                var targettedDirtyText = dirtyText.element;
                targettedDirtyText.timestamp = timestampedElement.timestamp;
                dirtyText.element = targettedDirtyText;
                actOnDirtyTextReceived(dirtyText);
            }
            foreach (var dirtyInk in timestampedElement.element.SelectElements<MeTLStanzas.DirtyInk>(true))
            {
                var targettedDirtyInk = dirtyInk.element;
                targettedDirtyInk.timestamp = timestampedElement.timestamp;
                dirtyInk.element = targettedDirtyInk;
                actOnDirtyStrokeReceived(dirtyInk);
            }
            foreach (var dirtyImage in timestampedElement.element.SelectElements<MeTLStanzas.DirtyImage>(true))
            {
                var targettedDirtyImage = dirtyImage.element;
                targettedDirtyImage.timestamp = timestampedElement.timestamp;
                dirtyImage.element = targettedDirtyImage;
                actOnDirtyImageReceived(dirtyImage);
            }
            foreach (var ink in timestampedElement.element.SelectElements<MeTLStanzas.Ink>(true))
            {
                var targettedStroke = ink.Stroke;
                targettedStroke.timestamp = timestampedElement.timestamp;
                var stroke = targettedStroke.stroke;
                stroke.tag(new StrokeTag(stroke.tag(), timestampedElement.timestamp));
                actOnStrokeReceived(targettedStroke);
            }
            foreach (var attendance in timestampedElement.element.SelectElements<MeTLStanzas.Attendance>(true))
            {
                var att = attendance.attendance;
                att.timestamp = attendance.timestamp;
                attendance.attendance = att;
                actOnAttendance(attendance);
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
        public virtual void actOnAttendance(MeTLStanzas.Attendance attendance)
        {
            receiveEvents.attendanceReceived(attendance.attendance);
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
        public void JoinRoom(string room)
        {            
            joinRoom(new Jid(room + "@" + metlServerAddress.muc));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentRooms"));
        }
        public void LeaveRoom(string room)
        {
            leaveRoom(new Jid(room + "@" + metlServerAddress.muc));
        }
        private void handleTeacherInConversation(Element el)
        {
            var where = el.SelectElements("parameter",true).Item(0).InnerXml;
            receiveEvents.teacherStatusRequest(where, "");
        }
        private void handleSyncMoveReceived(Element el)
        {
            var where = Int32.Parse(el.SelectElements("parameter", true).Item(0).InnerXml);
            receiveEvents.syncMoveRequested(where);
        }
        private void handleConversationDetailsUpdated(Element el)
        {
            var jid = el.SelectElements("parameter", true).Item(0).InnerXml;
            var newDetails = conversationDetailsProvider.DetailsOf(jid);
            receiveEvents.receiveConversationDetails(newDetails);
        }
        protected virtual void handleUnknownMessage(Element el)
        {
            Trace.TraceWarning(string.Format("Received unknown message: {0}", el.ToString()));
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