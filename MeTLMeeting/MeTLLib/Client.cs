using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MeTLLib;
using MeTLLib.Providers;
using MeTLLib.Providers.Connection;
using MeTLLib.Providers.Structure;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Ink;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.Diagnostics;
using Ninject;
using agsXMPP.Xml.Dom;
using System.Net;

namespace MeTLLib
{
    public abstract class MeTLServerAddress
    {
        public Uri productionUri { get; set; }
        public Uri stagingUri { get; set; }
        public enum serverMode { PRODUCTION, STAGING };
        private serverMode mode;
        public void setMode(serverMode mode)
        {
            this.mode = mode;
        }
        public Uri uri
        {
            get
            {
                return mode == serverMode.PRODUCTION ? productionUri : stagingUri;
            }
        }
        public Uri secureUri { get { return new Uri("https://" + host); } }
        public string host { get { return uri.Host; } }
        public string muc
        {
            get
            {
                return "conference." + host;
            }
        }
        public agsXMPP.Jid global
        {
            get
            {
                return new agsXMPP.Jid("global@" + muc);
            }
        }
    }
    public class MadamServerAddress : MeTLServerAddress
    {
        public MadamServerAddress()
        {
            stagingUri = new Uri("http://madam.adm.monash.edu.au", UriKind.Absolute);
            productionUri = new Uri("http://madam.adm.monash.edu.au", UriKind.Absolute);
        }
    }
    public interface IClientBehaviour
    {
        Credentials Connect(string username, string password);
        bool Disconnect();
        void SendTextBox(TargettedTextBox textbox);
        void SendStroke(TargettedStroke stroke);
        void SendImage(TargettedImage image);
        void SendVideo(TargettedVideo video);
        void SendDirtyTextBox(TargettedDirtyElement tde);
        void SendDirtyStroke(TargettedDirtyElement tde);
        void SendDirtyImage(TargettedDirtyElement tde);
        void SendDirtyVideo(TargettedDirtyElement tde);
        void SendSubmission(TargettedSubmission ts);
        void SendQuizAnswer(QuizAnswer qa);
        void SendQuizQuestion(QuizQuestion qq);
        void SendFile(TargettedFile tf);
        void UploadAndSendImage(MeTLLib.DataTypes.MeTLStanzas.LocalImageInformation lii);
        void UploadAndSendVideo(MeTLLib.DataTypes.MeTLStanzas.LocalVideoInformation videoInformation);
        void UploadAndSendFile(MeTLLib.DataTypes.MeTLStanzas.LocalFileInformation lfi);
        void AsyncRetrieveHistoryOf(int room);
        void MoveTo(int slide);
        void JoinConversation(string conversation);
        ConversationDetails AppendSlide(string Jid);
        ConversationDetails AppendSlideAfter(int slide, string Jid);
        ConversationDetails AppendSlideAfter(int slide, string Jid, Slide.TYPE type);
        ConversationDetails UpdateConversationDetails(ConversationDetails details);
        ConversationDetails CreateConversation(ConversationDetails details);
        ConversationDetails DetailsOf(String jid);
        void SneakInto(string room);
        void SneakOutOf(string room);
        Uri NoAuthUploadResource(Uri file, int Room);
        Uri NoAuthUploadResourceToPath(string fileToUpload, string pathToUploadTo, string nameToUpload);
        Uri NoAuthUploadResource(byte[] data, string filename, int Room);
        void SaveUserOptions(string username, UserOptions options);
        UserOptions UserOptionsFor(string username);
    }
    public class ClientConnection : IClientBehaviour
    {
        [Inject]
        public IReceiveEvents events { get; set; }
        [Inject]
        public AuthorisationProvider authorisationProvider { private get; set; }
        [Inject]
        public IResourceUploader resourceUploader { private get; set; }
        [Inject]
        public HttpHistoryProvider historyProvider { private get; set; }
        [Inject]
        public IConversationDetailsProvider conversationDetailsProvider { private get; set; }
        [Inject]
        public ResourceCache cache { private get; set; }
        [Inject]
        public JabberWireFactory jabberWireFactory { private get; set; }
        [Inject]
        public IWebClientFactory downloaderFactory { private get; set; }
        [Inject]
        public UserOptionsProvider userOptionsProvider { private get; set; }
        [Inject]
        public HttpResourceProvider resourceProvider { private get; set; }
        public MeTLServerAddress server { private set; get; }
        public ClientConnection(MeTLServerAddress address)
        {
            server = address;
        }
        #region fields
        private JabberWire wire;
        public Location location
        {
            get
            {
                if (wire != null && wire.location != null) return wire.location;
                else return new Location("0", 1, new List<int> { 1 });
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
        public HttpHistoryProvider getHistoryProvider()
        {
            return historyProvider;
        }
        #region connection
        public Credentials Connect(string username, string password)
        {
            var credentials = authorisationProvider.attemptAuthentication(username, password);
            jabberWireFactory.credentials = credentials;
            wire = jabberWireFactory.wire();
            wire.Login(new Location("100", 101, new List<int> { 101, 102, 103, 104, 105, 106 }));
            return credentials;
        }
        public bool Disconnect()
        {
            Action work = delegate
            {
                wire.Logout();
            };
            tryIfConnected(work);
            wire = null;
            return isConnected;
        }
        #endregion
        #region sendStanzas
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
        public void SendVideo(TargettedVideo video)
        {
            Action work = delegate
            {
                video.injectDependencies(server, resourceProvider);
                wire.SendVideo(video);
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
        public void SendDirtyVideo(TargettedDirtyElement tde)
        {
            Action work = delegate
            {
                wire.SendDirtyVideo(tde);
            };
            tryIfConnected(work);
        }
        public void SendStanza(string where, Element stanza) {
            tryIfConnected(delegate {
                wire.SendStanza(where, stanza);
            });
        }
        public void uploadAndSendSubmission(MeTLStanzas.LocalSubmissionInformation lii)
        {
            Action work = delegate
            {
                try
                {
                    var newPath = resourceUploader.uploadResource(lii.slide.ToString(), lii.file, false);
                    wire.SendScreenshotSubmission(new TargettedSubmission(lii.slide, lii.author, lii.target, lii.privacy, newPath, DateTimeFactory.Now().Ticks));
                    if (System.IO.File.Exists(lii.file)) System.IO.File.Delete(lii.file);
                }
                catch (Exception e)
                {
                    Trace.TraceError("MeTLLib::ClientConnection:uploadAndSendSubmission {0}",e.Message);
                    uploadAndSendSubmission(lii);
                }
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
        public void SendQuizAnswer(QuizAnswer qa)
        {
            Action work = delegate
            {
                wire.SendQuizAnswer(qa);
            };
            tryIfConnected(work);
        }
        public void SendQuizQuestion(QuizQuestion qq)
        {
            Action work = delegate
            {
                wire.SendQuiz(qq);
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
                    var newPath = resourceUploader.uploadResource(lii.slide.ToString(), lii.file, false);
                    Image newImage = lii.image;
                    newImage.tag(lii.image.tag());
                    newImage.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(newPath);
                    wire.SendImage(new TargettedImage(lii.slide, lii.author, lii.target, lii.privacy, newImage));
                }
                catch (Exception e)
                {
                    Trace.TraceError("MeTLLib::ClientConnection:UploadAndSendImage: {0}",e.Message);
                    UploadAndSendImage(lii);
                }
            };
            tryIfConnected(work);
        }
        public void UploadAndSendFile(MeTLLib.DataTypes.MeTLStanzas.LocalFileInformation lfi)
        {
            Action work = delegate
            {
                var newPath = resourceUploader.uploadResource(lfi.slide.ToString(), lfi.file, lfi.overwrite);
                wire.sendFileResource(new TargettedFile(lfi.slide, lfi.author, lfi.target, lfi.privacy, newPath, lfi.uploadTime, lfi.size, lfi.name));
            };
            tryIfConnected(work);
        }
        public void UploadAndSendVideo(MeTLLib.DataTypes.MeTLStanzas.LocalVideoInformation videoInformation)
        {
            Action work = delegate
            {
                try
                {
                    var newPath = new Uri(resourceUploader.uploadResource(videoInformation.slide.ToString(), videoInformation.file, false), UriKind.Absolute);
                    MeTLLib.DataTypes.Video newVideo = videoInformation.video;
                    newVideo.VideoSource = newPath;
                    newVideo.MediaElement = new MediaElement { Source = newPath };
                    wire.SendVideo(new TargettedVideo(videoInformation.slide, videoInformation.author, videoInformation.target, videoInformation.privacy, newVideo));
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.Message);
                    UploadAndSendVideo(videoInformation);
                }
            };
            tryIfConnected(work);
        }
        public Uri UploadResource(Uri file, string muc)
        {
            System.Uri returnValue = server.uri;
            Action work = delegate
            {
                returnValue = new System.Uri(resourceUploader.uploadResource(muc, file.OriginalString, false));
            };
            tryIfConnected(work);
            return returnValue;
        }
        
        public Uri UploadResourceToPath(byte[] data, string file, string name, bool overwrite)
        {
            System.Uri returnValue = server.uri;
            Action work = delegate
            {
                returnValue = new System.Uri(resourceUploader.uploadResourceToPath(data, file, name, overwrite));
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
                if (slide == null) return;
                wire.SendSyncMoveTo(slide);
            };
            tryIfConnected(work);
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
                var cd = conversationDetailsProvider.DetailsOf(conversation);
                wire.leaveRooms();
                location.activeConversation = null;
                location.availableSlides = null;
                location.currentSlide = 0;
                events.receiveConversationDetails(null);
            };
            tryIfConnected(work);
        }
        public void JoinConversation(string conversation)
        {
            Action work = delegate
            {
                if (String.IsNullOrEmpty(conversation)) return;
                Trace.TraceInformation("JoinConversation {0}", conversation);
                var cd = conversationDetailsProvider.DetailsOf(conversation);
                location.activeConversation = cd.Jid;
                location.availableSlides = cd.Slides.Select(s => s.id).ToList();
                if(location.availableSlides.Count > 0)
                    location.currentSlide = location.availableSlides[0];
                else
                {
                    Trace.TraceError("CRASH: FIXED: I would have crashed in Client.JoinConversation due to location.AvailableSlides not having any elements");
                }
                events.receiveConversationDetails(cd);
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
        public void AsyncRetrieveHistoryOf(int room)
        {
            Action work = delegate
            {
                wire.GetHistory(room);
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
            return tryUntilConnected<ConversationDetails>(()=>conversationDetailsProvider.Create(details));
        }
        public ConversationDetails AppendSlide(string Jid)
        {
            return tryUntilConnected<ConversationDetails>(() => conversationDetailsProvider.AppendSlide(Jid));
        }
        public ConversationDetails AppendSlideAfter(int slide, String Jid)
        {
            return tryUntilConnected<ConversationDetails>(() => conversationDetailsProvider.AppendSlideAfter(slide,Jid));
        }
        public ConversationDetails AppendSlideAfter(int slide, String Jid, Slide.TYPE type)
        {
            return tryUntilConnected<ConversationDetails>(() => conversationDetailsProvider.AppendSlideAfter(slide,Jid,type));
        }
        public List<ConversationDetails> CurrentConversations
        {
            get
            {
                var list = new List<ConversationDetails>();
                Action work = delegate
                {
                    list = wire.CurrentClasses;
                };
                tryIfConnected(work);
                return list;
            }
        }
        #endregion
        #region HelperMethods
        private void requeue(Action action)
        {
            if (wire.IsConnected() == false)
                wire.AddActionToReloginQueue(action);
        }
        private void tryIfConnected(Action action)
        {
            try
            {
                action();
            }
            catch (WebException) {
                requeue(action);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private T tryUntilConnected<T>(Func<T> function) {
            var wait = new ManualResetEvent(false);
            T result = default(T);
            tryIfConnected(delegate {
                result = function();
                wait.Set();
            });  
            wait.WaitOne();
            return result;
        }
        private string decodeUri(Uri uri)
        {
            return uri.Host;
        }
        #endregion
        #region noAuth
        public Uri NoAuthUploadResource(Uri file, int Room)
        {
            return new System.Uri(resourceUploader.uploadResource(Room.ToString(), file.ToString(), false));
        }
        public Uri NoAuthUploadResourceToPath(string fileToUpload, string pathToUploadTo, string nameToUpload)
        {
            var resultantPathString = resourceUploader.uploadResourceToPath(fileToUpload, pathToUploadTo, nameToUpload);
            return new System.Uri(resultantPathString, UriKind.Absolute);
        }
        public Uri NoAuthUploadResource(byte[] data, string filename, int Room)
        {
            return new System.Uri(resourceUploader.uploadResourceToPath(data, Room.ToString(), filename, false));
        }
        public List<ConversationDetails> AvailableConversations
        {
            get
            {
                return conversationDetailsProvider.ListConversations().ToList();
            }
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
    }
}
