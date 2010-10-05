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
        public Uri uri { get; set; }
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
            uri = new Uri("http://madam.adm.monash.edu.au", UriKind.Absolute);
        }
    }
    public interface IClientBehaviour
    {

    }
    public class ClientConnection : IClientBehaviour
    {
        [Inject]
        public AuthorisationProvider authorisationProvider { private get; set; }
        [Inject]
        public ResourceUploader resourceUploader { private get; set; }
        [Inject]
        public HttpHistoryProvider historyProvider { private get; set; }
        [Inject]
        public IConversationDetailsProvider conversationDetailsProvider { private get; set; }
        [Inject]
        public JabberWireFactory jabberWireFactory { private get; set; }
        private MeTLServerAddress server;
        public ClientConnection(MeTLServerAddress address)
        {
            server = address;
            Trace.TraceInformation("MeTL client connection started.  Server set to:" + server.ToString(), "Connection");
            attachCommandsToEvents();
        }
        #region fields
        private JabberWire wire;
        public Location location
        {
            get
            {
                if (wire != null && wire.location != null) return wire.location;
                else return null;
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
        #region connection
        public bool Connect(string username, string password)
        {
            Trace.TraceInformation("Attempting authentication with username:" + username);
            authorisationProvider.attemptAuthentication(username, password);
            Trace.TraceInformation("Connection state: " + isConnected.ToString());
            return isConnected;
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
                //Commands.SendTextBox.Execute(textbox);
            };
            tryIfConnected(work);
        }
        public void SendStroke(TargettedStroke stroke)
        {
            Trace.TraceInformation("Beginning Stroke send: " + stroke.startingChecksum, "Sending data");
            Action work = delegate
            {
                wire.SendStroke(stroke);
                //Commands.SendStroke.Execute(stroke);
            };
            tryIfConnected(work);
        }
        public void SendImage(TargettedImage image)
        {
            Trace.TraceInformation("Beginning Image send: " + image.id);
            Action work = delegate
            {
                wire.SendImage(image);
                //Commands.SendImage.Execute(image);
            };
            tryIfConnected(work);
        }
        public void SendVideo(TargettedVideo video)
        {
            Trace.TraceInformation("Beginning Video send: " + video.id);
            Action work = delegate
            {
                wire.SendVideo(video);
                //Commands.SendVideo.Execute(video);
            };
            tryIfConnected(work);
        }
        public void SendDirtyTextBox(TargettedDirtyElement tde)
        {
            Trace.TraceInformation("Beginning DirtyTextbox send: " + tde.identifier);
            Action work = delegate
            {
                wire.SendDirtyText(tde);
                //Commands.SendDirtyText.Execute(tde);
            };
            tryIfConnected(work);
        }
        public void SendDirtyStroke(TargettedDirtyElement tde)
        {
            Trace.TraceInformation("Beginning DirtyStroke send: " + tde.identifier);
            Action work = delegate
            {
                wire.sendDirtyStroke(tde);
                //Commands.SendDirtyStroke.Execute(tde);
            };
            tryIfConnected(work);
        }
        public void SendDirtyImage(TargettedDirtyElement tde)
        {
            Trace.TraceInformation("Beginning DirtyImage send: " + tde.identifier);
            Action work = delegate
            {
                wire.SendDirtyImage(tde);
                //Commands.SendDirtyImage.Execute(tde);
            };
            tryIfConnected(work);
        }
        public void SendDirtyVideo(TargettedDirtyElement tde)
        {
            Trace.TraceInformation("Beginning DirtyVideo send: " + tde.identifier);
            Action work = delegate
            {
                wire.SendDirtyVideo(tde);
                //Commands.SendDirtyVideo.Execute(tde);
            };
            tryIfConnected(work);
        }
        public void SendSubmission(TargettedSubmission ts)
        {
            Trace.TraceInformation("Beginning Submission send: " + ts.url);
            Action work = delegate
            {
                wire.SendScreenshotSubmission(ts);
                //Commands.SendScreenshotSubmission.Execute(ts);
            };
            tryIfConnected(work);
        }
        public void SendQuizAnswer(QuizAnswer qa)
        {
            Trace.TraceInformation("Beginning QuizAnswer send: " + qa.id);
            Action work = delegate
            {
                wire.SendQuizAnswer(qa);
                //Commands.SendQuizAnswer.Execute(qa);
            };
            tryIfConnected(work);
        }
        public void SendQuizQuestion(QuizQuestion qq)
        {
            Trace.TraceInformation("Beginning QuizQuestion send: " + qq.id);
            Action work = delegate
            {
                wire.SendQuiz(qq);
                //Commands.SendQuiz.Execute(qq);
            };
            tryIfConnected(work);
        }
        public void SendFile(TargettedFile tf)
        {
            Trace.TraceInformation("Beginning File send: " + tf.url);
            Action work = delegate
            {
                wire.sendFileResource(tf);
                //Commands.SendFileResource.Execute(tf);
            };
            tryIfConnected(work);
        }
        public void UploadAndSendImage(MeTLLib.DataTypes.MeTLStanzas.LocalImageInformation lii)
        {
            Action work = delegate
            {
                Trace.TraceInformation("Beginning ImageUpload: " + lii.file);
                var newPath = resourceUploader.uploadResource("/Resource/" + lii.slide, lii.file, false);
                Trace.TraceInformation("ImageUpload remoteUrl set to: " + newPath);
                Image newImage = lii.image;
                newImage.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(newPath);
                //Commands.SendImage.Execute(
                wire.SendImage(new TargettedImage
                {
                    author = lii.author,
                    privacy = lii.privacy,
                    slide = lii.slide,
                    target = lii.target,
                    image = newImage
                });
            };
            tryIfConnected(work);
        }
        public void UploadAndSendFile(MeTLLib.DataTypes.MeTLStanzas.LocalFileInformation lfi)
        {
            Action work = delegate
            {
                var newPath = resourceUploader.uploadResource(lfi.path, lfi.file, lfi.overwrite);
                //Commands.SendFileResource.Execute
                wire.sendFileResource(new TargettedFile
                {
                    author = lfi.author,
                    name = lfi.name,
                    privacy = lfi.privacy,
                    size = lfi.size,
                    slide = lfi.slide,
                    target = lfi.target,
                    uploadTime = lfi.uploadTime,
                    url = newPath
                });
            };
            tryIfConnected(work);
        }
        public void UploadAndSendVideo(MeTLLib.DataTypes.MeTLStanzas.LocalVideoInformation lvi)
        {
            Action work = delegate
            {
                var newPath = resourceUploader.uploadResource(lvi.slide.ToString(), lvi.file, false);
                MeTLLib.DataTypes.Video newVideo = lvi.video;
                newVideo.MediaElement = new MediaElement();
                newVideo.MediaElement.Source = new Uri(newPath, UriKind.Absolute);
                //Commands.SendVideo.Execute
                wire.SendVideo(new TargettedVideo
                {
                    author = lvi.author,
                    privacy = lvi.privacy,
                    slide = lvi.slide,
                    target = lvi.target,
                    video = newVideo
                });
            };
            tryIfConnected(work);
        }
        #endregion
        #region conversationCommands
        public void MoveTo(int slide)
        {
            Action work = delegate
            {
                wire.MoveTo(slide);
                //Commands.MoveTo.Execute(slide);
                Trace.TraceWarning("CommandHandlers: "+Commands.allHandlers().Count().ToString());
            };
            tryIfConnected(work);
        }
        public void SneakInto(string room)
        {
            Action work = delegate
            {
                wire.SneakInto(room);
                //Commands.SneakInto.Execute(room);
            };
            tryIfConnected(work);
        }
        public void SneakOutOf(string room)
        {
            Action work = delegate
            {
                wire.SneakOutOf(room);
                //Commands.SneakOutOf.Execute(room);
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
        public PreParser RetrieveHistoryOf(string room)
        {
            //This doesn't work yet.  The completion of the action never fires.  There may be a deadlock somewhere preventing this.
            PreParser finishedParser = jabberWireFactory.preParser(PreParser.ParentRoom(room));
            tryIfConnected(() =>
            {
                historyProvider.Retrieve<PreParser>(
                    () =>
                    {
                        Trace.TraceInformation("History started (" + room + ")");
                    },
                    (current, total) => Trace.TraceInformation("History progress (" + room + "): " + current + "/" + total),
                    preParser =>
                    {
                        Trace.TraceInformation("History completed (" + room + ")");
                        finishedParser = preParser;
                    },
                    room);
            });
            return finishedParser;
        }
        public void UpdateConversationDetails(ConversationDetails details)
        {
            Action work = delegate
            {
                Commands.UpdateConversationDetails.Execute(details);
            };
            tryIfConnected(work);
        }
        public List<ConversationDetails> AvailableConversations
        {
            get
            {
                if (wire == null) return null;
                var list = new List<ConversationDetails>();
                Action work = delegate
                {
                    list = conversationDetailsProvider.ListConversations().ToList();
                };
                tryIfConnected(work);
                return list;
            }
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
        #region commandToEventBridge
        private void attachCommandsToEvents()
        {
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<Credentials>(setIdentity));
            Commands.ReceiveStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(receiveStroke));
            Commands.ReceiveTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>(receiveTextBox));
            Commands.ReceiveDirtyText.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(receiveDirtyStroke));
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<TargettedStroke[]>(receiveStrokes));
            Commands.ReceiveDirtyStrokes.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(receiveDirtyStroke));
            Commands.ReceiveVideo.RegisterCommand(new DelegateCommand<TargettedVideo>(receiveVideo));
            Commands.ReceiveDirtyVideo.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(receiveDirtyVideo));
            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<TargettedImage[]>(receiveImage));
            Commands.ReceiveDirtyImage.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(receiveDirtyImage));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preParserAvailable));
            //Commands.LoggedIn.RegisterCommand(new DelegateCommand<object>(loggedIn));
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<QuizQuestion>(receiveQuiz));
            Commands.ReceiveQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(receiveQuizAnswer));
            Commands.ReceiveFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(receiveFileResource));
            Commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(receiveSubmission));
        }
        #region CommandMethods
        /*private void loggedIn(object _unused)
        {
            Trace.TraceInformation("Logged in\r\n");
            StatusChanged(this, new StatusChangedEventArgs { isConnected = true });
        }
         */
        private void setIdentity(Credentials c)
        {
            Commands.UnregisterAllCommands();
            attachCommandsToEvents();
            var credentials = new Credentials { authorizedGroups = c.authorizedGroups, name = c.name, password = "examplePassword" };
            jabberWireFactory.credentials = c;
            wire = jabberWireFactory.wire();
            wire.Login(new Location { currentSlide = 101, activeConversation = "100" });
            //Commands.MoveTo.Execute(101);
            Trace.TraceInformation("set up jabberwire");
            Commands.AllStaticCommandsAreRegistered();
        }
        private void receiveSubmission(TargettedSubmission ts)
        {
            SubmissionAvailable(this, new SubmissionAvailableEventArgs { submission = ts });
        }
        private void receiveQuiz(QuizQuestion qq)
        {
            QuizQuestionAvailable(this, new QuizQuestionAvailableEventArgs { quizQuestion = qq });
        }
        private void receiveQuizAnswer(QuizAnswer qa)
        {
            QuizAnswerAvailable(this, new QuizAnswerAvailableEventArgs { quizAnswer = qa });
        }
        private void receiveFileResource(TargettedFile tf)
        {
            FileAvailable(this, new FileAvailableEventArgs { file = tf });
        }
        private void receiveImage(TargettedImage[] tia)
        {
            foreach (TargettedImage ti in tia)
                ImageAvailable(this, new ImageAvailableEventArgs { image = ti });
        }
        private void receiveStroke(TargettedStroke ts)
        {
            StrokeAvailable(this, new StrokeAvailableEventArgs { stroke = ts });
        }
        private void receiveStrokes(TargettedStroke[] tsa)
        {
            foreach (TargettedStroke ts in tsa)
                StrokeAvailable(this, new StrokeAvailableEventArgs { stroke = ts });
        }
        private void receiveImages(TargettedImage[] tia)
        {
            foreach (TargettedImage ti in tia)
                ImageAvailable(this, new ImageAvailableEventArgs { image = ti });
        }
        private void receiveImage(TargettedImage ti)
        {
            ImageAvailable(this, new ImageAvailableEventArgs { image = ti });
        }
        private void receiveTextBox(TargettedTextBox ttb)
        {
            TextBoxAvailable(this, new TextBoxAvailableEventArgs { textBox = ttb });
        }
        private void receiveVideo(TargettedVideo tv)
        {
            VideoAvailable(this, new VideoAvailableEventArgs { video = tv });
        }
        private void receiveDirtyStroke(TargettedDirtyElement tde)
        {
            DirtyStrokeAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        private void receiveDirtyTextBox(TargettedDirtyElement tde)
        {
            DirtyTextBoxAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        private void receiveDirtyVideo(TargettedDirtyElement tde)
        {
            DirtyVideoAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        private void receiveDirtyImage(TargettedDirtyElement tde)
        {
            DirtyImageAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        private void preParserAvailable(PreParser pp)
        {
            PreParserAvailable(this, new PreParserAvailableEventArgs { parser = pp });
        }
        #endregion
        #endregion
        #region EventHandlers
        public delegate void SubmissionAvailableEventHandler(object sender, SubmissionAvailableEventArgs e);
        public delegate void FileAvailableEventHandler(object sender, FileAvailableEventArgs e);
        public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
        public delegate void PreParserAvailableEventHandler(object sender, PreParserAvailableEventArgs e);
        public delegate void StrokeAvailableEventHandler(object sender, StrokeAvailableEventArgs e);
        public delegate void ImageAvailableEventHandler(object sender, ImageAvailableEventArgs e);
        public delegate void TextBoxAvailableEventHandler(object sender, TextBoxAvailableEventArgs e);
        public delegate void DirtyElementAvailableEventHandler(object sender, DirtyElementAvailableEventArgs e);
        public delegate void VideoAvailableEventHandler(object sender, VideoAvailableEventArgs e);
        public delegate void AutoshapeAvailableEventHandler(object sender, AutoshapeAvailableEventArgs e);
        public delegate void LiveWindowAvailableEventHandler(object sender, LiveWindowAvailableEventArgs e);
        public delegate void BubbleAvailableEventHandler(object sender, BubbleAvailableEventArgs e);
        public delegate void DiscoAvailableEventHandler(object sender, DiscoAvailableEventArgs e);
        public delegate void QuizAvailableEventHandler(object sender, QuizAvailableEventArgs e);
        public delegate void QuizAnswerAvailableEventHandler(object sender, QuizAnswerAvailableEventArgs e);
        public delegate void QuizQuestionAvailableEventHandler(object sender, QuizQuestionAvailableEventArgs e);
        public delegate void ChatAvailableEventHandler(object sender, ChatAvailableEventArgs e);
        public delegate void ConversationDetailsAvailableEventHandler(object sender, ConversationDetailsAvailableEventArgs e);
        public delegate void CommandAvailableEventHandler(object sender, CommandAvailableEventArgs e);

        #endregion
        #region EventArgsDefinitions
        public class SubmissionAvailableEventArgs : EventArgs { public TargettedSubmission submission;}
        public class FileAvailableEventArgs : EventArgs { public TargettedFile file;}
        public class StatusChangedEventArgs : EventArgs { public bool isConnected;}
        public class PreParserAvailableEventArgs : EventArgs { public PreParser parser; }
        public class StrokeAvailableEventArgs : EventArgs { public TargettedStroke stroke;}
        public class ImageAvailableEventArgs : EventArgs { public TargettedImage image;}
        public class VideoAvailableEventArgs : EventArgs { public TargettedVideo video;}
        public class TextBoxAvailableEventArgs : EventArgs { public TargettedTextBox textBox;}
        public class DirtyElementAvailableEventArgs : EventArgs { public TargettedDirtyElement dirtyElement;}
        public class AutoshapeAvailableEventArgs : EventArgs { public TargettedAutoShape autoshape;}
        public class LiveWindowAvailableEventArgs : EventArgs { public LiveWindowSetup livewindow;}
        public class BubbleAvailableEventArgs : EventArgs { public TargettedBubbleContext bubble;}
        public class DiscoAvailableEventArgs : EventArgs { public string disco;}
        public class QuizAvailableEventArgs : EventArgs { public QuizContainer quiz;}
        public class QuizAnswerAvailableEventArgs : EventArgs { public QuizAnswer quizAnswer;}
        public class QuizQuestionAvailableEventArgs : EventArgs { public QuizQuestion quizQuestion;}
        public class ChatAvailableEventArgs : EventArgs { public string chat;}
        public class ConversationDetailsAvailableEventArgs : EventArgs { public ConversationDetails conversationDetails;}
        public class CommandAvailableEventArgs : EventArgs { public string command;}
        #endregion
        #region Events
        public event SubmissionAvailableEventHandler SubmissionAvailable;
        public event FileAvailableEventHandler FileAvailable;
        public event StatusChangedEventHandler StatusChanged;
        public event PreParserAvailableEventHandler PreParserAvailable;
        public event StrokeAvailableEventHandler StrokeAvailable;
        public event ImageAvailableEventHandler ImageAvailable;
        public event VideoAvailableEventHandler VideoAvailable;
        public event TextBoxAvailableEventHandler TextBoxAvailable;
        public event DirtyElementAvailableEventHandler DirtyTextBoxAvailable;
        public event DirtyElementAvailableEventHandler DirtyImageAvailable;
        public event DirtyElementAvailableEventHandler DirtyVideoAvailable;
        public event DirtyElementAvailableEventHandler DirtyStrokeAvailable;
        public event AutoshapeAvailableEventHandler AutoshapeAvailable;
        public event LiveWindowAvailableEventHandler LiveWindowAvailable;
        public event BubbleAvailableEventHandler BubbleAvailable;
        public event DiscoAvailableEventHandler DiscoAvailable;
        public event QuizAvailableEventHandler QuizAvailable;
        public event QuizQuestionAvailableEventHandler QuizQuestionAvailable;
        public event QuizAnswerAvailableEventHandler QuizAnswerAvailable;
        public event ChatAvailableEventHandler ChatAvailable;
        public event ConversationDetailsAvailableEventHandler ConversationDetailsAvailable;
        public event CommandAvailableEventHandler CommandAvailable;
        #endregion Events
        #region VirtualEventSubscribers
        protected virtual void onSubmissionAvailable(SubmissionAvailableEventArgs e)
        { SubmissionAvailable(this, e); }
        protected virtual void onFileAvailable(FileAvailableEventArgs e)
        { FileAvailable(this, e); }
        protected virtual void onStatusChanged(StatusChangedEventArgs e)
        { StatusChanged(this, e); }
        protected virtual void onPreParserAvailable(PreParserAvailableEventArgs e)
        { PreParserAvailable(this, e); }
        protected virtual void onStrokeAvailable(StrokeAvailableEventArgs e)
        { StrokeAvailable(this, e); }
        protected virtual void onDirtyStrokeAvailable(DirtyElementAvailableEventArgs e)
        { DirtyStrokeAvailable(this, e); }
        protected virtual void onDirtyTextBoxAvailable(DirtyElementAvailableEventArgs e)
        { DirtyTextBoxAvailable(this, e); }
        protected virtual void onDirtyImageAvailable(DirtyElementAvailableEventArgs e)
        { DirtyImageAvailable(this, e); }
        protected virtual void onDirtyVideoAvailable(DirtyElementAvailableEventArgs e)
        { DirtyVideoAvailable(this, e); }
        protected virtual void onImageAvailable(ImageAvailableEventArgs e)
        { ImageAvailable(this, e); }
        protected virtual void onVideoAvailable(VideoAvailableEventArgs e)
        { VideoAvailable(this, e); }
        protected virtual void onTextBoxAvailable(TextBoxAvailableEventArgs e)
        { TextBoxAvailable(this, e); }
        protected virtual void onAutoshapeAvailable(AutoshapeAvailableEventArgs e)
        { AutoshapeAvailable(this, e); }
        protected virtual void onLiveWindowAvailable(LiveWindowAvailableEventArgs e)
        { LiveWindowAvailable(this, e); }
        protected virtual void onBubbleAvailable(BubbleAvailableEventArgs e)
        { BubbleAvailable(this, e); }
        protected virtual void onDiscoAvailable(DiscoAvailableEventArgs e)
        { DiscoAvailable(this, e); }
        protected virtual void onQuizAvailable(QuizAvailableEventArgs e)
        { QuizAvailable(this, e); }
        protected virtual void onQuizQuestionAvailable(QuizQuestionAvailableEventArgs e)
        { QuizQuestionAvailable(this, e); }
        protected virtual void onQuizAnswerAvailable(QuizAnswerAvailableEventArgs e)
        { QuizAnswerAvailable(this, e); }
        protected virtual void onChatAvailable(ChatAvailableEventArgs e)
        { ChatAvailable(this, e); }
        protected virtual void onConversationDetailsAvailable(ConversationDetailsAvailableEventArgs e)
        { ConversationDetailsAvailable(this, e); }
        protected virtual void onCommandAvailable(CommandAvailableEventArgs e)
        { CommandAvailable(this, e); }
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
            Commands.UnregisterAllCommands();
            action();
        }
        private string decodeUri(Uri uri)
        {
            return uri.Host;
        }
        #endregion
    }
}
