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
        void UploadAndSendVideo(MeTLLib.DataTypes.MeTLStanzas.LocalVideoInformation lvi);
        void UploadAndSendFile(MeTLLib.DataTypes.MeTLStanzas.LocalFileInformation lfi);
        void AsyncRetrieveHistoryOf(int room);
        PreParser RetrieveHistoryOfMUC(string muc);
        List<PreParser> RetrieveHistoryOfRoom(int room);
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
        public IWebClientFactory downloaderFactory{ private get; set; }
        public MeTLServerAddress server { private set; get; }
        public ClientConnection(MeTLServerAddress address)
        {
            server = address;
            Trace.TraceInformation("MeTL client connection started.  Server set to:" + server.ToString(), "Connection");
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
        public HttpHistoryProvider getHistoryProvider() {
            return historyProvider;
        }
        #region connection
        public Credentials Connect(string username, string password)
        {
            Trace.TraceInformation("Attempting authentication with username:" + username);
            var credentials = authorisationProvider.attemptAuthentication(username, password);
            jabberWireFactory.credentials = credentials;
            wire = jabberWireFactory.wire();
            wire.Login(new Location("100", 101, new List<int> { 101, 102, 103, 104, 105, 106 }));
            Trace.TraceInformation("set up jabberwire");
            Trace.TraceInformation("Connection state: " + isConnected.ToString());
            return credentials;
        }
        public bool Disconnect()
        {
            Action work = delegate
            {
                Trace.TraceInformation("Attempting to disconnect from MeTL");
                wire.Logout();
            };
            tryIfConnected(work);
            wire = null;
            Trace.TraceInformation("Connection state: " + isConnected.ToString());
            return isConnected;
        }
        #endregion
        #region sendStanzas
        public void SendTextBox(TargettedTextBox textbox)
        {
            Trace.TraceInformation("Beginning TextBox send: " + textbox.identity);
            Action work = delegate
            {
                wire.SendTextbox(textbox);
            };
            tryIfConnected(work);
        }
        public void SendStroke(TargettedStroke stroke)
        {
            Trace.TraceInformation("Beginning Stroke send: " + stroke.startingChecksum, "Sending data");
            Action work = delegate
            {
                wire.SendStroke(stroke);
            };
            tryIfConnected(work);
        }
        public void SendImage(TargettedImage image)
        {
            Trace.TraceInformation("Beginning Image send: " + image.id);
            Action work = delegate
            {
                image.injectDependencies(server, downloaderFactory.client());
                wire.SendImage(image);
            };
            tryIfConnected(work);
        }
        public void SendVideo(TargettedVideo video)
        {
            Trace.TraceInformation("Beginning Video send: " + video.id);
            Action work = delegate
            {
                video.injectDependancies(server);
                wire.SendVideo(video);
            };
            tryIfConnected(work);
        }
        public void SendDirtyTextBox(TargettedDirtyElement tde)
        {
            Trace.TraceInformation("Beginning DirtyTextbox send: " + tde.identifier);
            Action work = delegate
            {
                wire.SendDirtyText(tde);
            };
            tryIfConnected(work);
        }
        public void SendDirtyStroke(TargettedDirtyElement tde)
        {
            Trace.TraceInformation("Beginning DirtyStroke send: " + tde.identifier);
            Action work = delegate
            {
                wire.sendDirtyStroke(tde);
            };
            tryIfConnected(work);
        }
        public void SendDirtyImage(TargettedDirtyElement tde)
        {
            Trace.TraceInformation("Beginning DirtyImage send: " + tde.identifier);
            Action work = delegate
            {
                wire.SendDirtyImage(tde);
            };
            tryIfConnected(work);
        }
        public void SendDirtyVideo(TargettedDirtyElement tde)
        {
            Trace.TraceInformation("Beginning DirtyVideo send: " + tde.identifier);
            Action work = delegate
            {
                wire.SendDirtyVideo(tde);
            };
            tryIfConnected(work);
        }
        public void uploadAndSendSubmission(MeTLStanzas.LocalSubmissionInformation lii)
        {
            Action work = delegate
            {
                Trace.TraceInformation("Beginning ImageUpload: " + lii.file);
                var newPath = resourceUploader.uploadResource("/Resource/" + lii.slide, lii.file, false);
                Trace.TraceInformation("ImageUpload remoteUrl set to: " + newPath);
                wire.SendScreenshotSubmission(new TargettedSubmission(lii.slide, lii.author, lii.target, lii.privacy, newPath, DateTimeFactory.Now().Ticks));
                if (System.IO.File.Exists(lii.file)) System.IO.File.Delete(lii.file);
            };
            tryIfConnected(work);
        }
        public void SendSubmission(TargettedSubmission ts)
        {
            Trace.TraceInformation("Beginning Submission send: " + ts.url);
            Action work = delegate
            {
                wire.SendScreenshotSubmission(ts);
            };
            tryIfConnected(work);
        }
        public void SendQuizAnswer(QuizAnswer qa)
        {
            Trace.TraceInformation("Beginning QuizAnswer send: " + qa.id);
            Action work = delegate
            {
                wire.SendQuizAnswer(qa);
            };
            tryIfConnected(work);
        }
        public void SendQuizQuestion(QuizQuestion qq)
        {
            Trace.TraceInformation("Beginning QuizQuestion send: " + qq.id);
            Action work = delegate
            {
                wire.SendQuiz(qq);
            };
            tryIfConnected(work);
        }
        public void SendFile(TargettedFile tf)
        {
            Trace.TraceInformation("Beginning File send: " + tf.url);
            Action work = delegate
            {
                wire.sendFileResource(tf);
            };
            tryIfConnected(work);
        }
        public void UploadAndSendImage(MeTLLib.DataTypes.MeTLStanzas.LocalImageInformation lii)
        {
            Action work = delegate
            {
                Trace.TraceInformation("Beginning ImageUpload: " + lii.file);
                var newPath = resourceUploader.uploadResource(lii.slide.ToString(), lii.file, false);
                Trace.TraceInformation("ImageUpload remoteUrl set to: " + newPath);
                Image newImage = lii.image;
                newImage.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(newPath);
                wire.SendImage(new TargettedImage(lii.slide, lii.author, lii.target, lii.privacy, newImage));
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
        public void UploadAndSendVideo(MeTLLib.DataTypes.MeTLStanzas.LocalVideoInformation lvi)
        {
            Action work = delegate
            {
                var newPath = new Uri(resourceUploader.uploadResource(lvi.slide.ToString(), lvi.file, false), UriKind.Absolute);
                MeTLLib.DataTypes.Video newVideo = lvi.video;
                newVideo.VideoSource = newPath;
                newVideo.MediaElement = new MediaElement();
                newVideo.MediaElement.Source = newPath;
                wire.SendVideo(new TargettedVideo(lvi.slide, lvi.author, lvi.target, lvi.privacy, newVideo));
            };
            tryIfConnected(work);
        }
        public Uri UploadResource(Uri file, string muc)
        {
            return new System.Uri(resourceUploader.uploadResource(muc, file.OriginalString, false));
        }
        public Uri UploadResourceToPath(byte[] data,string file, string name, bool overwrite)
        {
            return new System.Uri(resourceUploader.uploadResourceToPath(data, file, name, overwrite));
        }
        #endregion
        #region conversationCommands
        public void MoveTo(int slide)
        {
            Action work = delegate
            {
                if (slide == null) return;
                wire.MoveTo(slide);
                Trace.TraceInformation(String.Format("Location: (conv:{0}),(slide:{1}),(slides:{2})",
                    Globals.conversationDetails.Title + " : " + Globals.conversationDetails.Jid,
                    Globals.slide,
                    Globals.slides.Select(s => s.id.ToString()).Aggregate((total, item) => total += " " + item + "")));
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
                var cd = conversationDetailsProvider.DetailsOf(conversation);
                location.activeConversation = cd.Jid;
                location.availableSlides = cd.Slides.Select(s => s.id).ToList();
                location.currentSlide = location.availableSlides[0];
                events.receiveConversationDetails(cd);
            };
            tryIfConnected(work);
        }
        public void SneakIntoAndDo(string room, Action<PreParser> doAction)
        {
            Action work = delegate
            {
                if (String.IsNullOrEmpty(room)) return;
                wire.SneakIntoAndDo(room, doAction);
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
                if (room == null) return;
                wire.GetHistory(room);
            };
            tryIfConnected(work);
        }
        public PreParser RetrieveHistoryOfMUC(string muc)
        {
            var parserList = new List<PreParser>();
            tryIfConnected(() =>
            {
                if (String.IsNullOrEmpty(muc)) return;
                Thread thread = new Thread(new ParameterizedThreadStart(delegate
                {
                    Thread parserAggregator = new Thread(new ParameterizedThreadStart(delegate
                    {
                        while (parserList.Count == 0)
                        {
                        }
                    }));
                    parserAggregator.Start();
                    historyProvider.Retrieve<PreParser>(
                () =>
                {
                    Trace.TraceInformation("MUC History started (" + muc + ")");
                },
                (current, total) => Trace.TraceInformation("MUC History progress (" + muc + "): " + current + "/" + total),
                preParser =>
                {
                    Trace.TraceInformation("MUC History completed (" + muc + ")");
                    parserList.Add(preParser);
                },
                muc);
                    parserAggregator.Join();
                }));
                thread.Start();
                thread.Join();
            });
            return parserList[0];
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
        public List<PreParser> RetrieveHistoryOfRoom(int room)
        {
            var parserList = new List<PreParser>();
            tryIfConnected(() =>
            {
                if (room == null) return;
                Thread thread = new Thread(new ParameterizedThreadStart(delegate
                {
                    Thread parserAggregator = new Thread(new ParameterizedThreadStart(delegate
                    {
                        while (parserList.Count < 3)
                        {
                        }
                    }));
                    parserAggregator.Start();
                    historyProvider.Retrieve<PreParser>(
                () =>
                {
                    Trace.TraceInformation("History started (" + room + ")");
                },
                (current, total) => Trace.TraceInformation("History progress (" + room + "): " + current + "/" + total),
                preParser =>
                {
                    Trace.TraceInformation("History completed (" + room + ")");
                    parserList.Add(preParser);
                },
                room.ToString());
                    historyProvider.RetrievePrivateContent<PreParser>(
                    () => Trace.TraceInformation("Private History started (" + room + ")"),
                    (current, total) => Trace.TraceInformation("Private History progress (" + room + "): " + current + "/" + total),
                    preParser =>
                    {
                        Trace.TraceInformation("Private History completed (" + room + ")");
                        parserList.Add(preParser);
                    },
                    username,
                    room.ToString());
                    parserAggregator.Join();
                }));
                thread.Start();
                thread.Join();
            });
            return parserList;
        }
        public ConversationDetails UpdateConversationDetails(ConversationDetails details)
        {
            ConversationDetails cd = null;
            Action work = delegate
            {
                cd = conversationDetailsProvider.Update(details);
            };
            tryIfConnected(work);
            return cd;
        }
        public ConversationDetails DetailsOf(string room)
        {
            ConversationDetails cd = null;
            Action work = delegate
            {
                cd = conversationDetailsProvider.DetailsOf(room);
            };
            tryIfConnected(work);
            return cd;
        }
        public ConversationDetails CreateConversation(ConversationDetails details)
        {
            ConversationDetails cd = null;
            Action work = delegate
            {
                cd = conversationDetailsProvider.Create(details);
            };
            tryIfConnected(work);
            return cd;
        }
        public ConversationDetails AppendSlide(string Jid)
        {
            ConversationDetails details = ConversationDetails.Empty;
            Action work = delegate
            {
                details = conversationDetailsProvider.AppendSlide(Jid);
            };
            tryIfConnected(work);
            return details;
        }
        public ConversationDetails AppendSlideAfter(int slide, String Jid)
        {
            ConversationDetails details = ConversationDetails.Empty;
            Action work = delegate
            {
                details = conversationDetailsProvider.AppendSlideAfter(slide, Jid);
            };
            tryIfConnected(work);
            return details;
        }
        public ConversationDetails AppendSlideAfter(int slide, String Jid, Slide.TYPE type)
        {
            ConversationDetails details = ConversationDetails.Empty;
            Action work = delegate
            {
                details = conversationDetailsProvider.AppendSlideAfter(slide, Jid, type);
            };
            tryIfConnected(work);
            return details;
        }
        public List<ConversationDetails> CurrentConversations
        {
            get
            {
                if (wire == null) return null;
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
        private void tryIfConnected(Action action)
        {
            if (wire == null)
            {
                Trace.TraceWarning("Wire is null at tryIfConnected");
                return;
            }
            if (wire.IsConnected() == false)
            {
                Trace.TraceWarning("Wire is disconnected at tryIfConnected");
                return;
            }
            action();
        }
        private string decodeUri(Uri uri)
        {
            return uri.Host;
        }
        #endregion
        #region noAuth
        public Uri NoAuthUploadResource(Uri file, int Room)
        {
            return new System.Uri(resourceUploader.uploadResource(Room.ToString(),file.ToString(),false));
        }
        public Uri NoAuthUploadResourceToPath(string fileToUpload, string pathToUploadTo, string nameToUpload)
        {
            return new System.Uri(resourceUploader.uploadResourceToPath(fileToUpload, pathToUploadTo, nameToUpload),UriKind.Absolute);
        }
        public Uri NoAuthUploadResource(byte[] data, string filename, int Room)
        {
            return new System.Uri(resourceUploader.uploadResourceToPath(data,Room.ToString(),filename,false));
        }
        public List<ConversationDetails> AvailableConversations
        {
            get
            {
                return conversationDetailsProvider.ListConversations().ToList();
                /*var list = new List<ConversationDetails>();
                if (wire.IsConnected() == false) return list;
                Action work = delegate
                {
                    list = conversationDetailsProvider.ListConversations().ToList();
                };
                tryIfConnected(work);
                return list;*/
            }
        }
        #endregion
    }
}
