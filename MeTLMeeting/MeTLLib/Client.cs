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

namespace MeTLLib
{
    public class ClientConnection
    {
        private JabberWire wire;
        public Location location { get { return wire.location; } }
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
        public ClientConnection()
        {
            attachCommandsToEvents();
        }
        public bool Connect(string username, string password)
        {
            AuthorisationProvider.attemptAuthentication(username, password);
            return isConnected;
        }
        public void MoveTo(int slide)
        {
            Commands.MoveTo.Execute(slide);
        }
        #region Boilerplate
        #region sendStanzas
        public void SendTextBox(TargettedTextBox textbox)
        {
            Commands.SendTextBox.Execute(textbox);
        }
        public void SendStroke(TargettedStroke stroke)
        {
            Commands.SendStroke.Execute(stroke);
        }
        public void SendImage(TargettedImage image)
        {
            Commands.SendImage.Execute(image);
        }
        public void UpdateConversationDetails(ConversationDetails details)
        {
            Commands.UpdateConversationDetails.Execute(details);
        }
        public List<ConversationDetails> AvailableConversations
        {
            get
            {
                return ConversationDetailsProviderFactory.Provider.ListConversations().ToList();
            }
        }
        #endregion
        #region commandToEventBridge
        private void attachCommandsToEvents()
        {
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<Credentials>((s) =>
            {
                var credentials = new Credentials { authorizedGroups = s.authorizedGroups, name = s.name, password = "examplePassword" };
                wire = new JabberWire(credentials);
                wire.Login(new Location { currentSlide = 101, activeConversation = "100" });
                Commands.MoveTo.Execute(101);
                Logger.Log("set up jabberwire");
            }));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>((s) =>
                PreParserAvailable(this, new PreParserAvailableEventArgs { parser = s })));
            Commands.ReceiveStroke.RegisterCommand(new DelegateCommand<TargettedStroke>((s) =>
                StrokeAvailable(this, new StrokeAvailableEventArgs { stroke = s })
                ));
            Commands.ReceiveTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>((s) =>
                TextBoxAvailable(this, new TextBoxAvailableEventArgs { textBox = s })
                ));
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<TargettedStroke>((s) =>
                StrokeAvailable(this, new StrokeAvailableEventArgs { stroke = s })));
            Commands.ReceiveVideo.RegisterCommand(new DelegateCommand<TargettedVideo>((s) =>
                VideoAvailable(this, new VideoAvailableEventArgs { video = s })));
            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<TargettedImage[]>((s) =>
                {
                    foreach (TargettedImage ti in s)
                        ImageAvailable(this, new ImageAvailableEventArgs { image = ti });
                }
                ));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<object>((s) =>
            Logger.Log(
                "Location:" + ((PreParser)s).location.currentSlide.ToString() +
                " Ink:" + ((PreParser)s).ink.Count.ToString() +
                " Images:" + ((PreParser)s).images.Count.ToString() +
                " Text:" + ((PreParser)s).text.Count.ToString() +
                " Videos:" + ((PreParser)s).videos.Count.ToString() +
                " Submissions:" + ((PreParser)s).submissions.Count.ToString() +
                " Quizzes:" + ((PreParser)s).quizzes.Count.ToString() +
                " QuizAnswers:" + ((PreParser)s).quizAnswers.Count.ToString()
                )));
            Commands.LoggedIn.RegisterCommand(new DelegateCommand<object>((s) =>
            {
                Logger.Log("Logged in\r\n");
                StatusChanged(this, new StatusChangedEventArgs { isConnected = true });
            }));
            Commands.ReceiveLogMessage.RegisterCommand(new DelegateCommand<string>((s) =>
                LogMessageAvailable(this, new LogMessageAvailableEventArgs { logMessage = s })));
        }
        #endregion
        #region EventHandlers
        public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
        public delegate void PreParserAvailableEventHandler(object sender, PreParserAvailableEventArgs e);
        public delegate void LogMessageAvailableEventHandler(object sender, LogMessageAvailableEventArgs e);
        public delegate void StrokeAvailableEventHandler(object sender, StrokeAvailableEventArgs e);
        public delegate void ImageAvailableEventHandler(object sender, ImageAvailableEventArgs e);
        public delegate void TextBoxAvailableEventHandler(object sender, TextBoxAvailableEventArgs e);
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
        public class StatusChangedEventArgs : EventArgs { public bool isConnected;}
        public class PreParserAvailableEventArgs : EventArgs { public PreParser parser; }
        public class LogMessageAvailableEventArgs : EventArgs { public string logMessage; }
        public class StrokeAvailableEventArgs : EventArgs { public TargettedStroke stroke;}
        public class ImageAvailableEventArgs : EventArgs { public TargettedImage image;}
        public class VideoAvailableEventArgs : EventArgs { public TargettedVideo video;}
        public class TextBoxAvailableEventArgs : EventArgs { public TargettedTextBox textBox;}
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
        public event StatusChangedEventHandler StatusChanged;
        public event PreParserAvailableEventHandler PreParserAvailable;
        public event LogMessageAvailableEventHandler LogMessageAvailable;
        public event StrokeAvailableEventHandler StrokeAvailable;
        public event ImageAvailableEventHandler ImageAvailable;
        public event VideoAvailableEventHandler VideoAvailable;
        public event TextBoxAvailableEventHandler TextBoxAvailable;
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
        protected virtual void onStatusChanged(StatusChangedEventArgs e)
        { StatusChanged(this, e); }
        protected virtual void onPreParserAvailable(PreParserAvailableEventArgs e)
        { PreParserAvailable(this, e); }
        protected virtual void onLogMessageAvailable(LogMessageAvailableEventArgs e)
        { LogMessageAvailable(this, e); }
        protected virtual void onStrokeAvailable(StrokeAvailableEventArgs e)
        { StrokeAvailable(this, e); }
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
        #endregion
    }
}