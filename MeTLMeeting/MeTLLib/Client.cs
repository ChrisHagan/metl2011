using System;
using System.Collections.Generic;
using System.Linq;
using MeTLLib.Providers;
using MeTLLib.Providers.Connection;
using MeTLLib.Providers.Structure;
using MeTLLib.DataTypes;
using System.Threading;
using System.Diagnostics;
//using Ninject;
using agsXMPP.Xml.Dom;
using System.Net;

namespace MeTLLib
{
    public interface IClientBehaviour
    {
        MetlConfiguration server { get; }
        IReceiveEvents events { get; }
        AuthorisationProvider authorisationProvider { get; }
        IResourceUploader resourceUploader { get; }
        IHistoryProvider historyProvider { get; }
        IConversationDetailsProvider conversationDetailsProvider { get; }
        ResourceCache cache { get; }
        JabberWireFactory jabberWireFactory { get; }
        IWebClientFactory downloaderFactory { get; }
        UserOptionsProvider userOptionsProvider { get; }
        HttpResourceProvider resourceProvider { get; }
        void AskForTeachersStatus(string teacher, string where);
        Location location { get; }
        void HandleShutdown();
        string UploadResourceToPath(byte[] data, string file, string name, bool overwrite);
        void LeaveConversation(string conversation);
        void UpdateSlideCollection(Int32 conversationJid);
        bool Connect(Credentials credentials);
        bool Disconnect();
        void SendTextBox(TargettedTextBox textbox);
        void SendStroke(TargettedStroke stroke);
        void SendImage(TargettedImage image);
        void SendDirtyTextBox(TargettedDirtyElement tde);
        void SendDirtyStroke(TargettedDirtyElement tde);
        void SendDirtyImage(TargettedDirtyElement tde);
        void SendDirtyVideo(TargettedDirtyElement tde);
        void SendSubmission(TargettedSubmission ts);
        void SendTeacherStatus(TeacherStatus status);
        void SendMoveDelta(TargettedMoveDelta tmd);
        void GetAllSubmissionsForConversation(string conversationJid);
        void SendStanza(string where, Element stanza);
        void SendAttendance(string where, Attendance att);
        void SendQuizAnswer(QuizAnswer qa, string jid);
        void SendQuizQuestion(QuizQuestion qq, string jid);
        void SendFile(TargettedFile tf);
        void SendSyncMove(int slide);
        void UploadAndSendImage(MeTLLib.DataTypes.MeTLStanzas.LocalImageInformation lii);
        void UploadAndSendFile(MeTLLib.DataTypes.MeTLStanzas.LocalFileInformation lfi);
        string UploadResource(Uri uri, string slideId);
        void LeaveLocation();
        void LoadQuiz(int conversationJid, long quizId);
        void LoadQuizzes(string conversationJid);
        void LoadAttachments(string conversationJid);
        void UploadAndSendSubmission(MeTLStanzas.LocalSubmissionInformation lii);
        ConversationDetails AppendSlide(string Jid);
        ConversationDetails AppendSlideAfter(int slide, string Jid);
        ConversationDetails AppendSlideAfter(int slide, string Jid, Slide.TYPE type);
        ConversationDetails UpdateConversationDetails(ConversationDetails details);
        ConversationDetails CreateConversation(ConversationDetails details);
        ConversationDetails DeleteConversation(ConversationDetails details);
        ConversationDetails DuplicateSlide(ConversationDetails details, Slide slide);
        ConversationDetails DuplicateConversation(ConversationDetails conversation);
        ConversationDetails DetailsOf(String jid);
        void JoinRoom(string room);
        void LeaveRoom(string room);
        string NoAuthUploadResource(Uri file, int Room);
        string NoAuthUploadResourceToPath(string fileToUpload, string pathToUploadTo, string nameToUpload);
        string NoAuthUploadResource(byte[] data, string filename, int Room);
        void SaveUserOptions(string username, UserOptions options);
        List<SearchConversationDetails> ConversationsFor(String query, int maxResults);
        UserOptions UserOptionsFor(string username);
        void LeaveAllRooms();
        void JoinConversation(string jid);
        void MoveTo(int slide);
        void SneakInto(string room);
        void SneakOutOf(string room);
        void AsyncRetrieveHistoryOf(int v);
        //List<MeTLUserInformation> getMeTLUserInformations(List<string> usernames);
    }
    public class DisconnectedClientConnection : IClientBehaviour
    {
        public MetlConfiguration server { get; }
        public IReceiveEvents events { get; }
        public AuthorisationProvider authorisationProvider { get; }
        public IResourceUploader resourceUploader { get; }
        public IHistoryProvider historyProvider { get; }
        public IConversationDetailsProvider conversationDetailsProvider { get; }
        public ResourceCache cache { get; }
        public JabberWireFactory jabberWireFactory { get; }
        public IWebClientFactory downloaderFactory { get; }
        public UserOptionsProvider userOptionsProvider { get; }
        public HttpResourceProvider resourceProvider { get; }
        protected static readonly Uri DisconnectedUri = new Uri("noscheme://not.a.uri");
        public Location location { get { return Location.Empty; } }
        public void HandleShutdown() { }
        public string UploadResourceToPath(byte[] data, string file, string name, bool overwrite) { return ""; }
        public void LeaveConversation(string conversation) { }
        public void UpdateSlideCollection(Int32 conversationJid) { }
        public void AskForTeachersStatus(string teacher, string where) { }
        public bool Connect(Credentials credentials) { return false; }
        public bool Disconnect() { return false; }
        public void SendTextBox(TargettedTextBox textbox) { }
        public void SendStroke(TargettedStroke stroke) { }
        public void SendImage(TargettedImage image) { }
        public void SendDirtyTextBox(TargettedDirtyElement tde) { }
        public void SendDirtyStroke(TargettedDirtyElement tde) { }
        public void SendDirtyImage(TargettedDirtyElement tde) { }
        public void SendDirtyVideo(TargettedDirtyElement tde) { }
        public void SendSubmission(TargettedSubmission ts) { }
        public void SendTeacherStatus(TeacherStatus status) { }
        public void SendMoveDelta(TargettedMoveDelta tmd) { }
        public void GetAllSubmissionsForConversation(string conversationJid) { }
        public void SendStanza(string where, Element stanza) { }
        public void SendAttendance(string where, Attendance att) { }
        public void SendQuizAnswer(QuizAnswer qa, string jid) { }
        public void SendQuizQuestion(QuizQuestion qq, string jid) { }
        public void SendFile(TargettedFile tf) { }
        public void SendSyncMove(int slide) { }
        public void UploadAndSendImage(MeTLLib.DataTypes.MeTLStanzas.LocalImageInformation lii) { }
        public void UploadAndSendFile(MeTLLib.DataTypes.MeTLStanzas.LocalFileInformation lfi) { }
        public string UploadResource(Uri uri, string slideId) { return ""; }
        public void MoveTo(int slide) { }
        public void LeaveLocation() { }
        public void LoadQuiz(int convesationJid, long quizId) { }
        public void LoadQuizzes(string conversationJid) { }
        public void LoadAttachments(string conversationJid) { }
        public void UploadAndSendSubmission(MeTLStanzas.LocalSubmissionInformation lii) { }
        public ConversationDetails AppendSlide(string Jid) { return ConversationDetails.Empty; }
        public ConversationDetails AppendSlideAfter(int slide, string Jid) { return ConversationDetails.Empty; }
        public ConversationDetails AppendSlideAfter(int slide, string Jid, Slide.TYPE type) { return ConversationDetails.Empty; }
        public ConversationDetails UpdateConversationDetails(ConversationDetails details) { return ConversationDetails.Empty; }
        public ConversationDetails CreateConversation(ConversationDetails details) { return ConversationDetails.Empty; }
        public ConversationDetails DeleteConversation(ConversationDetails details) { return ConversationDetails.Empty; }
        public ConversationDetails DetailsOf(String jid) { return ConversationDetails.Empty; }
        public ConversationDetails DuplicateSlide(ConversationDetails details, Slide slide) { return ConversationDetails.Empty; }
        public ConversationDetails DuplicateConversation(ConversationDetails conversation) { return ConversationDetails.Empty; }
        public void JoinRoom(string room) { }
        public void LeaveRoom(string room) { }
        public string NoAuthUploadResource(Uri file, int Room) { return ""; }
        public string NoAuthUploadResourceToPath(string fileToUpload, string pathToUploadTo, string nameToUpload) { return ""; }
        public string NoAuthUploadResource(byte[] data, string filename, int Room) { return ""; }
        public void SaveUserOptions(string username, UserOptions options) { }
        public List<SearchConversationDetails> ConversationsFor(String query, int maxResults) { return new List<SearchConversationDetails>(); }
        public UserOptions UserOptionsFor(string username) { return UserOptions.DEFAULT; }

        public void LeaveAllRooms()
        {
            throw new NotImplementedException();
        }

        public void JoinConversation(string jid)
        {
            throw new NotImplementedException();
        }

        public void SneakInto(string room)
        {
            throw new NotImplementedException();
        }

        public void SneakOutOf(string room)
        {
            throw new NotImplementedException();
        }

        public void AsyncRetrieveHistoryOf(int v)
        {
            throw new NotImplementedException();
        }
    }

    public class ClientConnection : IClientBehaviour
    {
        public MetlConfiguration server { get; protected set; }
        public IReceiveEvents events { get; protected set; }
        public AuthorisationProvider authorisationProvider { get; protected set; }
        public IResourceUploader resourceUploader { get; protected set; }
        public IHistoryProvider historyProvider { get { return jabberWireFactory.cachedHistoryProvider; } }
        public IConversationDetailsProvider conversationDetailsProvider { get; protected set; }
        public ResourceCache cache { get; protected set; }
        public JabberWireFactory jabberWireFactory { get; protected set; }
        public IWebClientFactory downloaderFactory { get; protected set; }
        public UserOptionsProvider userOptionsProvider { get; protected set; }
        public HttpResourceProvider resourceProvider { get; protected set; }
        //public IUserInformationProvider userInformationProvider { private get; set; }
        public IAuditor auditor { get; protected set; }

        public ClientConnection(
            MetlConfiguration address,
            IReceiveEvents _events,
            AuthorisationProvider _authProvider,
            IResourceUploader _resourceUploader,
            IConversationDetailsProvider _conversationDetailsProvider,
            ResourceCache _resourceCache,
            JabberWireFactory _jabberWireFactory,
            IWebClientFactory _downloaderFactory,
            UserOptionsProvider _userOptionsProvider,
            HttpResourceProvider _resourceProvider,
            IAuditor _auditor
            )
        {
            server = address;
            events = _events;
            authorisationProvider = _authProvider;
            resourceUploader = _resourceUploader;
            conversationDetailsProvider = _conversationDetailsProvider;
            cache = _resourceCache;
            jabberWireFactory = _jabberWireFactory;
            downloaderFactory = _downloaderFactory;
            userOptionsProvider = _userOptionsProvider;
            resourceProvider = _resourceProvider;
            auditor = _auditor;
        }
        #region fields
        private JabberWire wire;
        public Location location
        {
            get
            {
                return wire.location;
            }
        }
        public string username
        {
            get
            {
                if (wire != null && wire.credentials != null && wire.credentials.name != null)
                    return wire.credentials.name;
                else return "";
            }
        }
        public bool isConnected
        {
            get
            {
                if (wire == null) return false;
                return wire.IsConnected();
            }
        }
        #endregion
        public IHistoryProvider getHistoryProvider()
        {
            return historyProvider;
        }
        #region connection
        public void LeaveLocation()
        {
            LeaveConversation(location.activeConversation);
            wire.location = Location.Empty;
        }

        public void LoadQuiz(int conversationJid, long quizId)
        {
            tryIfConnected(() => wire.LoadQuiz(conversationJid, quizId));
        }

        public void LoadQuizzes(string conversationJid)
        {
            tryIfConnected(() => wire.LoadQuizzes(conversationJid));
        }
        public void LoadAttachments(string conversationJid)
        {
            tryIfConnected(() => wire.LoadAttachments(conversationJid));
        }
        public void AskForTeachersStatus(string teacher, string jid)
        {
            Action work = () => wire.AskForTeacherStatus(teacher, jid);
            tryIfConnected(work);
        }
        public bool Connect(Credentials credentials)
        {
            if (credentials != null && credentials.isValid)
            {
                //jabberWireFactory.credentials = credentials;
                wire = jabberWireFactory.wire();
                return true;
            }
            else
            {
                events.statusChanged(false, credentials);
                return false;
            }
        }
        public bool Disconnect()
        {
            if (wire != null)
                wire.Logout();
            return true;
        }
        #endregion
        #region sendStanzas
        public void SendTeacherStatus(TeacherStatus status)
        {
            Action work = delegate
            {
                wire.SendTeacherStatus(status);
            };
            tryIfConnected(work);
        }
        public void SendTextBox(TargettedTextBox textbox)
        {
            Action work = delegate
            {
                wire.SendTextbox(textbox);
            };
            tryIfConnected(work);
        }
        public void SendStroke(TargettedStroke stroke)
        {
            Action work = delegate
            {
                wire.SendStroke(stroke);
            };
            tryIfConnected(work);
        }
        public void SendImage(TargettedImage image)
        {
            Action work = delegate
            {
                image.injectDependencies(server, downloaderFactory.client(), resourceProvider);
                wire.SendImage(image);
            };
            tryIfConnected(work);
        }
        public void SendDirtyTextBox(TargettedDirtyElement tde)
        {
            Action work = delegate
            {
                wire.SendDirtyText(tde);
            };
            tryIfConnected(work);
        }
        public void SendDirtyStroke(TargettedDirtyElement tde)
        {
            Action work = delegate
            {
                wire.sendDirtyStroke(tde);
            };
            tryIfConnected(work);
        }
        public void SendDirtyImage(TargettedDirtyElement tde)
        {
            Action work = delegate
            {
                wire.SendDirtyImage(tde);
            };
            tryIfConnected(work);
        }
        public void SendAttendance(string where, Attendance att)
        {
            Action work = delegate
            {

                wire.SendAttendance(where, att);
            };
            tryIfConnected(work);
        }

        public void SendDirtyVideo(TargettedDirtyElement tde)
        {
            Action work = delegate
            {
                wire.SendDirtyVideo(tde);
            };
            tryIfConnected(work);
        }
        public void SendMoveDelta(TargettedMoveDelta tmd)
        {
            Action work = delegate
            {
                wire.SendMoveDelta(tmd);
            };
            tryIfConnected(work);
        }
        public void SendStanza(string where, Element stanza)
        {
            tryIfConnected(delegate
            {
                wire.SendStanza(where, stanza);
            });
        }
        public void UploadAndSendSubmission(MeTLStanzas.LocalSubmissionInformation lii)
        {
            Action work = delegate
            {
                auditor.wrapAction((a =>
                {
                    try
                    {
                        var newPath = resourceUploader.uploadResource(lii.slide.ToString(), lii.file, lii.file);
                        a(GaugeStatus.InProgress, 33);
                        wire.SendScreenshotSubmission(new TargettedSubmission(lii.slide, lii.author, lii.target, lii.privacy, lii.timestamp, lii.identity, newPath, lii.currentConversationName, DateTimeFactory.Now().Ticks, lii.blacklisted));
                        a(GaugeStatus.InProgress, 66);
                        if (System.IO.File.Exists(lii.file)) System.IO.File.Delete(lii.file);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("MeTLLib::ClientConnection:UploadAndSendSubmission {0}", e.Message);
                        UploadAndSendSubmission(lii);
                    }
                }), "uploadAndSendSubmission", "clientConnection");
            };
            tryIfConnected(work);
        }
        public void SendSubmission(TargettedSubmission ts)
        {
            Action work = delegate
            {
                wire.SendScreenshotSubmission(ts);
            };
            tryIfConnected(work);
        }
        public void GetAllSubmissionsForConversation(string conversationJid)
        {
            Action work = () =>
                              {
                                  wire.LoadSubmissions(conversationJid);
                              };
            tryIfConnected(work);
        }
        public void SendQuizAnswer(QuizAnswer qa, string jid)
        {
            Action work = delegate
            {
                wire.SendQuizAnswer(qa, jid);
            };
            tryIfConnected(work);
        }
        public void SendQuizQuestion(QuizQuestion qq, string jid)
        {
            Action work = delegate
            {
                wire.SendQuiz(qq, jid);
            };
            tryIfConnected(work);
        }
        public void SendFile(TargettedFile tf)
        {
            Action work = delegate
            {
                try
                {
                    wire.sendFileResource(tf);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.Message);
                    SendFile(tf);
                }
            };
            tryIfConnected(work);
        }
        public void UploadAndSendImage(MeTLLib.DataTypes.MeTLStanzas.LocalImageInformation lii)
        {
            Action work = delegate
            {
                try
                {
                    var newPath = resourceUploader.uploadResource(lii.slide.ToString(), lii.file, lii.file);

                    //MeTLImage newImage = lii.image;
                    //var previousTag = newImage.tag();
                    //previousTag.resourceIdentity = newPath;
                    //lii.image.tag(previousTag);

                    /*
                    newImage.Dispatcher.adopt(() =>
                    {
                        //newImage.tag(lii.image.tag());
                        wire.SendImage(new TargettedImage(lii.slide, lii.author, lii.target, lii.privacy, lii.image.tag().id, newImage, newPath, lii.image.tag().timestamp));
                    });
                    */
                    wire.SendImage(new TargettedImage(lii.slide, lii.author, lii.target, lii.privacy, newPath, lii.X,lii.Y,lii.Width,lii.Height, newPath, -1L));
                }
                catch (Exception e)
                {
                    Trace.TraceError("MeTLLib::ClientConnection:UploadAndSendImage: {0}", e.Message);
                    // rethrow the exeception so the action will be requeued
                    throw e;
                }
            };
            tryIfConnected(work);
        }
        public void UploadAndSendFile(MeTLLib.DataTypes.MeTLStanzas.LocalFileInformation lfi)
        {
            Action work = delegate
            {
                var newPath = resourceUploader.uploadResource(lfi.slide.ToString(), lfi.file, lfi.file);
                wire.sendFileResource(new TargettedFile(lfi.slide, lfi.author, lfi.target, lfi.privacy, lfi.identity, lfi.timestamp, newPath, lfi.uploadTime, lfi.size, lfi.name));
            };
            tryIfConnected(work);
        }
        public string UploadResource(Uri file, string muc)
        {

            string returnValue = "";
            Action work = delegate
            {
                returnValue = resourceUploader.uploadResource(muc, file.OriginalString, file.OriginalString);
            };
            tryIfConnected(work);
            return returnValue;
        }

        public string UploadResourceToPath(byte[] data, string file, string name, bool overwrite)
        {

            string returnValue = "";
            Action work = delegate
            {
                returnValue = resourceUploader.uploadResourceToPath(data, file, name, overwrite);
            };
            tryIfConnected(work);
            return returnValue;
        }
        #endregion
        #region conversationCommands
        public void SendSyncMove(int slide)
        {
            Action work = delegate
            {
                wire.SendSyncMoveTo(slide);
            };
            tryIfConnected(work);
        }
       
        public void HandleShutdown()
        {
            Action work = delegate
            {
                wire.leaveRooms();
            };
            tryIfConnected(work);
        }
        public void JoinRoom(string room)
        {
            Action work = delegate
            {
                if (String.IsNullOrEmpty(room))
                {
                    Trace.TraceError("ERROR: Cannot join empty room");
                    return;
                }
                wire.JoinRoom(room);
            };
            tryIfConnected(work);
        }
        public void LeaveRoom(string room)
        {
            Action work = delegate
            {
                if (String.IsNullOrEmpty(room)) return;
                wire.LeaveRoom(room);
            };
            tryIfConnected(work);
        }
        public void NoAuthAsyncRetrieveHistoryOfRoom(int room)
        {
            string muc = room.ToString();
            historyProvider.Retrieve<PreParser>(
                () => { },
                (current, total) => { },
                preParser =>
                {
                    events.receivePreParser(preParser);
                },
                muc);
        }
        public ConversationDetails UpdateConversationDetails(ConversationDetails details)
        {
            ConversationDetails cd = ConversationDetails.Empty;
            Action work = delegate
            {
                cd = conversationDetailsProvider.Update(details);
            };
            tryIfConnected(work);
            return cd;
        }
        public void UpdateSlideCollection(Int32 conversationJid)
        {
            tryIfConnected(() =>
            {
                wire.sendSlideCollectionUpdatedCommand(conversationJid);
            });
        }
        public ConversationDetails DetailsOf(string room)
        {
            ConversationDetails cd = ConversationDetails.Empty;
            Action work = delegate
            {
                cd = conversationDetailsProvider.DetailsOf(room);
            };
            tryIfConnected(work);
            return cd;
        }
        public ConversationDetails CreateConversation(ConversationDetails details)
        {
            return tryUntilConnected<ConversationDetails>(() =>
                conversationDetailsProvider.Create(details));
        }
        public ConversationDetails DeleteConversation(ConversationDetails details)
        {
            if (details != null)
            {
                details.Subject = "Deleted";
                return UpdateConversationDetails(details);
            }

            return ConversationDetails.Empty;
        }
        public ConversationDetails DuplicateSlide(ConversationDetails details, Slide slide)
        {
            return tryUntilConnected<ConversationDetails>(() =>
            {
                var newConv = conversationDetailsProvider.DuplicateSlide(details, slide);
                events.receiveConversationDetails(newConv);
                return newConv;
            });
        }
        public ConversationDetails DuplicateConversation(ConversationDetails conversation)
        {
            return tryUntilConnected<ConversationDetails>(() =>
            {
                var newConv = conversationDetailsProvider.DuplicateConversation(conversation);
                events.receiveConversationDetails(newConv);
                return newConv;
            });

        }
        public ConversationDetails AppendSlide(string Jid)
        {
            return tryUntilConnected<ConversationDetails>(() =>
            {
                var newConv = conversationDetailsProvider.AppendSlide(Jid);
                events.receiveConversationDetails(newConv);
                return newConv;
            });
        }
        public ConversationDetails AppendSlideAfter(int slide, String Jid)
        {
            return tryUntilConnected<ConversationDetails>(() =>
            {
                var newConv = conversationDetailsProvider.AppendSlideAfter(slide,Jid);
                /*If you don't fire this on yourself, you'll have to wait until it comes back dirty.  Then you'll know that everyone else has it too.*/
                //events.receiveConversationDetails(newConv);
                return newConv;
            });            
        }
        public ConversationDetails AppendSlideAfter(int slide, String Jid, Slide.TYPE type)
        {
            return tryUntilConnected<ConversationDetails>(() =>
            {
                var newConv = conversationDetailsProvider.AppendSlideAfter(slide, Jid, type);
                events.receiveConversationDetails(newConv);
                return newConv;
            });
        }
        #endregion
        #region UserCommands
        /*
        public List<MeTLUserInformation> getMeTLUserInformations(List<string> usernames)
        {
            return userInformationProvider.lookupUsers(usernames);
        }
        */
        #endregion
        #region HelperMethods
        private void requeue(Action action)
        {
            if (wire.IsConnected() == false)
                wire.AddActionToReloginQueue(action);
            else
                tryIfConnected(action);
        }
        private void tryIfConnected(Action action)
        {
            try
            {
                if (wire.IsConnected())
                {
                    action();
                } else
                {
                    requeue(action);
                }
            }
            catch (WebException e)
            {
                requeue(action);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private T tryUntilConnected<T>(Func<T> function)
        {
            var wait = new ManualResetEvent(false);
            T result = default(T);
            bool complete = false;
            try
            {
                tryIfConnected(delegate
                {
                    result = function();
                    complete = true;
                    wait.Set();
                });
            }
            catch (Exception e)
            {
                wait.Set();
                throw e;
            }

            //TODO: Find out who would release this WaitHandle. If function causes an exception, wait is set in the catch block but complete is still false.
            if (!complete)
                wait.WaitOne();
            return result;
        }
        private string decodeUri(Uri uri)
        {
            return uri.Host;
        }
        #endregion
        #region noAuth
        public string NoAuthUploadResource(Uri file, int Room)
        {
            return resourceUploader.uploadResource(Room.ToString(), file.ToString(), file.ToString());
        }
        public string NoAuthUploadResourceToPath(string fileToUpload, string pathToUploadTo, string nameToUpload)
        {
            return resourceUploader.uploadResourceToPath(fileToUpload, pathToUploadTo, nameToUpload);
        }
        public string NoAuthUploadResource(byte[] data, string filename, int Room)
        {
            return resourceUploader.uploadResourceToPath(data, Room.ToString(), filename, false);
        }
        public List<SearchConversationDetails> ConversationsFor(String query, int maxResults)
        {
            return conversationDetailsProvider.ConversationsFor(query, maxResults).ToList();
        }
        #endregion
        public void SaveUserOptions(string username, UserOptions options)
        {
            Action work = delegate
            {
                userOptionsProvider.Set(username, options);
            };
            tryIfConnected(work);
        }
        public UserOptions UserOptionsFor(string username)
        {
            var res = UserOptions.DEFAULT;
            Action work = delegate
            {
                res = userOptionsProvider.Get(username);
            };
            tryIfConnected(work);
            return res;
        }

        public void MoveTo(int slide)
        {
            Action work = delegate
            {
                wire.MoveTo(slide);
            };
            tryIfConnected(work);
        }
        public void LeaveAllRooms()
        {
            Action work = delegate
            {
                wire.leaveRooms();
            };
            tryIfConnected(work);
        }
        public void LeaveConversation(string conversation)
        {
            Action work = delegate
            {
                if (String.IsNullOrEmpty(conversation)) return;
                if (conversation == wire.location.activeConversation)
                    wire.resetLocation();
            };
            tryIfConnected(work);
        }
        public void JoinConversation(string conversation)
        {
            Action work = delegate
            {
                auditor.wrapAction((a =>
                {
                    if (String.IsNullOrEmpty(conversation)) return;
                    //                    Trace.TraceInformation("JoinConversation {0}", conversation);
                    var cd = conversationDetailsProvider.DetailsOf(conversation);
                    a(GaugeStatus.InProgress, 25);
                    location.activeConversation = cd.Jid;
                    location.availableSlides = cd.Slides.Select(s => s.id).ToList();
                    if (location.availableSlides.Count > 0)
                        location.currentSlide = location.availableSlides[0];
                    else
                    {
                        Trace.TraceError("CRASH: FIXED: I would have crashed in Client.JoinConversation due to location.AvailableSlides not having any elements");
                    }
                    a(GaugeStatus.InProgress, 50);
                    wire.JoinConversation();
                    a(GaugeStatus.InProgress, 75);
                    events.receiveConversationDetails(cd);
                }), "joinConversation: " + conversation, "clientConnection");
            };
            tryIfConnected(work);
        }
        public void SneakInto(string room)
        {
            Action work = delegate
            {
                if (String.IsNullOrEmpty(room)) return;
                wire.SneakInto(room);
            };
            tryIfConnected(work);
        }
        public void SneakOutOf(string room)
        {
            Action work = delegate
            {
                if (String.IsNullOrEmpty(room)) return;
                wire.SneakOutOf(room);
            };
            tryIfConnected(work);
        }
        
        public void AsyncRetrieveHistoryOf(int v)
        {
            throw new NotImplementedException();
        }
    }
}
