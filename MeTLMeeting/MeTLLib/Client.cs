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

namespace MeTLLib
{
    public class ClientConnection
    {
        public ClientConnection()
        {
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
        public bool isConnected { get { return wire.IsConnected(); } }
        #endregion
        #region connection
        public bool Connect(string username, string password)
        {
            AuthorisationProvider.attemptAuthentication(username, password);
            return isConnected;
        }
        public bool Disconnect()
        {
            wire.Logout();
            wire = null;
            return isConnected;
        }
        #endregion
        #region sendStanzas
        public void SendTextBox(TargettedTextBox textbox)
        {
            if (wire == null) return;
            Commands.SendTextBox.Execute(textbox);
        }
        public void SendStroke(TargettedStroke stroke)
        {
            if (wire == null) return;
            Commands.SendStroke.Execute(stroke);
        }
        public void SendImage(TargettedImage image)
        {
            if (wire == null) return;
            Commands.SendImage.Execute(image);
        }
        public void SendVideo(TargettedVideo video)
        {
            if (wire == null) return;
            Commands.SendVideo.Execute(video);
        }
        public void SendDirtyTextBox(TargettedDirtyElement tde)
        {
            if (wire == null) return;
            Commands.SendDirtyText.Execute(tde);
        }
        public void SendDirtyStroke(TargettedDirtyElement tde)
        {
            if (wire == null) return;
            Commands.SendDirtyStroke.Execute(tde);
        }
        public void SendDirtyImage(TargettedDirtyElement tde)
        {
            if (wire == null) return;
            Commands.SendDirtyImage.Execute(tde);
        }
        public void SendDirtyVideo(TargettedDirtyElement tde)
        {
            if (wire == null) return;
            Commands.SendDirtyVideo.Execute(tde);
        }
        public void SendSubmission(TargettedSubmission ts)
        {
            if (wire == null) return;
            Commands.SendScreenshotSubmission.Execute(ts);
        }
        public void SendQuizAnswer(QuizAnswer qa)
        {
            if (wire == null) return;
            Commands.SendQuizAnswer.Execute(qa);
        }
        public void SendQuizQuestion(QuizQuestion qq)
        {
            if (wire == null) return;
            Commands.SendQuiz.Execute(qq);
        }
        public void SendFile(TargettedFile tf)
        {
            if (wire == null) return;
            Commands.SendFileResource.Execute(tf);
        }
        public void UploadAndSendImage(MeTLLib.DataTypes.MeTLStanzas.LocalImageInformation lii)
        {
            var newPath = ResourceUploader.uploadResource("/Resource/" + lii.room, lii.file, false);
            Image newImage = lii.image;
            newImage.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(newPath);
            Commands.SendFileResource.Execute(new TargettedImage
            {
                author = lii.author,
                privacy = lii.privacy,
                slide = lii.slide,
                target = lii.target,
                image = newImage
            });
        }
        public void UploadAndSendFile(MeTLLib.DataTypes.MeTLStanzas.LocalFileInformation lfi)
        {
            var newPath = ResourceUploader.uploadResource(lfi.path, lfi.file, lfi.overwrite);
            Commands.SendFileResource.Execute(new TargettedFile
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
        }
        public void UploadAndSendVideo(MeTLLib.DataTypes.MeTLStanzas.LocalVideoInformation lvi)
        {
            var newPath = ResourceUploader.uploadResource("/Resource/" + lvi.room, lvi.file, false);
            MeTLLib.DataTypes.Video newVideo = lvi.video;
            newVideo.MediaElement.Source = new Uri(newPath, UriKind.Absolute);
            Commands.SendFileResource.Execute(new TargettedVideo
            {
                author = lvi.author,
                privacy = lvi.privacy,
                slide = lvi.slide,
                target = lvi.target,
                video = newVideo
            });
        }
        #endregion
        #region conversationCommands
        public void MoveTo(int slide)
        {
            if (wire == null) return;
            Commands.MoveTo.Execute(slide);
        }
        public void SneakInto(string room)
        {
            if (wire == null) return;
            Commands.SneakInto.Execute(room);
        }
        public void SneakOutOf(string room)
        {
            if (wire == null) return;
            Commands.SneakOutOf.Execute(room);
        }
        public void AsyncRetrieveHistoryOf(int room)
        {
            if (wire == null) return;
            wire.GetHistory(room);
        }
        public PreParser RetrieveHistoryOf(string room)
        {
            //This doesn't work yet.  The completion of the action never fires.  There may be a deadlock somewhere preventing this.
            return null;
            if (wire == null) return null;
            bool isFinished = false;
            var provider = new HttpHistoryProvider();
            PreParser finishedParser = new PreParser(PreParser.ParentRoom(room));
            provider.Retrieve<PreParser>(null, null, preParser =>
            {
                finishedParser = preParser;
                isFinished = true;
            }, room);
            while (!isFinished)
            {
            }
            return finishedParser;
        }
        public void UpdateConversationDetails(ConversationDetails details)
        {
            if (wire == null) return;
            Commands.UpdateConversationDetails.Execute(details);
        }
        public List<ConversationDetails> AvailableConversations
        {
            get
            {
                if (wire == null) return null;
                return ConversationDetailsProviderFactory.Provider.ListConversations().ToList();
            }
        }
        public List<ConversationDetails> CurrentConversations
        {
            get
            {
                if (wire == null) return null;
                return wire.CurrentClasses;
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
            Commands.LoggedIn.RegisterCommand(new DelegateCommand<object>(loggedIn));
            Commands.ReceiveLogMessage.RegisterCommand(new DelegateCommand<string>(receiveLogMessage));
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<QuizQuestion>(receiveQuiz));
            Commands.ReceiveQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(receiveQuizAnswer));
            Commands.ReceiveFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(receiveFileResource));
            Commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(receiveSubmission));
        }
        #region CommandMethods
        private void loggedIn(object _unused)
        {
            Logger.Log("Logged in\r\n");
            StatusChanged(this, new StatusChangedEventArgs { isConnected = true });
        }
        private void setIdentity(Credentials c)
        {
            var credentials = new Credentials { authorizedGroups = c.authorizedGroups, name = c.name, password = "examplePassword" };
            wire = new JabberWire(credentials);
            wire.Login(new Location { currentSlide = 101, activeConversation = "100" });
            Commands.MoveTo.Execute(101);
            Logger.Log("set up jabberwire");
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
        private void receiveLogMessage(string logMessage)
        {
            LogMessageAvailable(this, new LogMessageAvailableEventArgs { logMessage = logMessage });
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
            /*Logger.Log(
                "Location:" + pp.location.currentSlide.ToString() +
                " Ink:" + pp.ink.Count.ToString() +
                " Images:" + pp.images.Count.ToString() +
                " Text:" + pp.text.Count.ToString() +
                " Videos:" + pp.videos.Count.ToString() +
                " Submissions:" + pp.submissions.Count.ToString() +
                " Quizzes:" + pp.quizzes.Count.ToString() +
                " QuizAnswers:" + pp.quizAnswers.Count.ToString()
                );*/
        }
        #endregion
        #endregion
        #region EventHandlers
        public delegate void SubmissionAvailableEventHandler(object sender, SubmissionAvailableEventArgs e);
        public delegate void FileAvailableEventHandler(object sender, FileAvailableEventArgs e);
        public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
        public delegate void PreParserAvailableEventHandler(object sender, PreParserAvailableEventArgs e);
        public delegate void LogMessageAvailableEventHandler(object sender, LogMessageAvailableEventArgs e);
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
        public class LogMessageAvailableEventArgs : EventArgs { public string logMessage; }
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
        public event LogMessageAvailableEventHandler LogMessageAvailable;
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
        protected virtual void onLogMessageAvailable(LogMessageAvailableEventArgs e)
        { LogMessageAvailable(this, e); }
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
    }
}