using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MeTLLib;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;

namespace SandRibbon.Components
{
    public class NetworkController
    {
        private ClientConnection client;
        public NetworkController()
        {
            if (App.isStaging)
                client = MeTLLib.ClientFactory.Connection(MeTLServerAddress.serverMode.STAGING);
            else
                client = MeTLLib.ClientFactory.Connection(MeTLServerAddress.serverMode.PRODUCTION);
            registerCommands();
            attachToClient();
            Commands.UpdateConversationDetails.ExecuteAsync(new ConversationDetails("", "", "", new List<Slide>(), new Permissions("", false, false, false), ""));
        }
        #region commands
        private void registerCommands()
        {
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(JoinConversation));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
            Commands.SendAutoShape.RegisterCommand(new DelegateCommand<TargettedAutoShape>(SendAutoshape));
            Commands.SendChatMessage.RegisterCommand(new DelegateCommand<object>(SendChatMessage));
            Commands.SendDirtyAutoShape.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyAutoshape));
            Commands.SendDirtyImage.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyImage));
            Commands.SendDirtyLiveWindow.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyLiveWindow));
            Commands.SendDirtyStroke.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyStroke));
            Commands.SendDirtyText.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyText));
            Commands.SendDirtyVideo.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyVideo));
            Commands.SendFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(SendFile));
            Commands.SendImage.RegisterCommand(new DelegateCommand<TargettedImage>(SendImage));
            Commands.SendLiveWindow.RegisterCommand(new DelegateCommand<LiveWindowSetup>(SendLiveWindow));
            Commands.SendNewBubble.RegisterCommand(new DelegateCommand<TargettedBubbleContext>(SendBubble));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<QuizQuestion>(SendQuiz));
            Commands.SendQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(SendQuizAnswer));
            Commands.SendScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(SendSubmission));
            Commands.SendStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(SendStroke));
            Commands.SendTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>(SendTextBox));
            Commands.SendVideo.RegisterCommand(new DelegateCommand<TargettedVideo>(SendVideo));
            Commands.SneakInto.RegisterCommand(new DelegateCommand<string>(SneakInto));
            Commands.SneakIntoAndDo.RegisterCommand(new DelegateCommand<Projector.RoomAndAction>(SneakIntoAndDo));
            Commands.SneakOutOf.RegisterCommand(new DelegateCommand<string>(SneakOutOf));
        }
        private void JoinConversation(object Jid)
        {
            if (Jid is string)
                client.JoinConversation((string)Jid);
        }
        private void MoveTo(int slide)
        {
            client.MoveTo(slide);
        }
        private void SendAutoshape(TargettedAutoShape tas)
        {
        }
        private void SendChatMessage(object _obj)
        {
        }
        private void SendDirtyAutoshape(TargettedDirtyElement tde)
        {
        }
        private void SendDirtyImage(TargettedDirtyElement tde)
        {
            client.SendDirtyImage(tde);
        }
        private void SendDirtyLiveWindow(TargettedDirtyElement tde)
        {
        }
        private void SendDirtyStroke(TargettedDirtyElement tde)
        {
            client.SendDirtyStroke(tde);
        }
        private void SendDirtyText(TargettedDirtyElement tde)
        {
            client.SendDirtyTextBox(tde);
        }
        private void SendDirtyVideo(TargettedDirtyElement tde)
        {
            client.SendDirtyVideo(tde);
        }
        private void SendFile(TargettedFile tf)
        {
            client.SendFile(tf);
        }
        private void SendImage(TargettedImage ti)
        {
            client.SendImage(ti);
        }
        private void SendLiveWindow(LiveWindowSetup lws)
        {
        }
        private void SendBubble(TargettedBubbleContext tbc)
        {
        }
        private void SendQuiz(QuizQuestion qq)
        {
            client.SendQuizQuestion(qq);
        }
        private void SendQuizAnswer(QuizAnswer qa)
        {
            client.SendQuizAnswer(qa);
        }
        private void SendSubmission(TargettedSubmission ts)
        {
            client.SendSubmission(ts);
        }
        private void SendStroke(TargettedStroke ts)
        {
            client.SendStroke(ts);
        }
        private void SendTextBox(TargettedTextBox ttb)
        {
            client.SendTextBox(ttb);
        }
        private void SendVideo(TargettedVideo tv)
        {
            client.SendVideo(tv);
        }
        private void SneakIntoAndDo(Projector.RoomAndAction roomAndAction)
        {
            client.SneakIntoAndDo(roomAndAction.room, roomAndAction.action);
        }
        private void SneakInto(string room)
        {
            client.SneakInto(room);
        }
        private void SneakOutOf(string room)
        {
            client.SneakOutOf(room);
        }
        #endregion

        #region events
        private void attachToClient()
        {
            client.events.AutoshapeAvailable += autoShapeAvailable;
            client.events.BubbleAvailable += bubbleAvailable;
            client.events.ChatAvailable += chatAvailable;
            client.events.CommandAvailable += commandAvailable;
            client.events.ConversationDetailsAvailable += conversationDetailsAvailable;
            client.events.DirtyAutoShapeAvailable += dirtyAutoshapeAvailable;
            client.events.DirtyImageAvailable += dirtyImageAvailable;
            client.events.DirtyLiveWindowAvailable += dirtyLiveWindowAvailable;
            client.events.DirtyStrokeAvailable += dirtyStrokeAvailable;
            client.events.DirtyTextBoxAvailable += dirtyTextBoxAvailable;
            client.events.DirtyVideoAvailable += dirtyVideoAvailable;
            client.events.DiscoAvailable += discoAvailable;
            client.events.FileAvailable += fileAvailable;
            client.events.ImageAvailable += imageAvailable;
            client.events.LiveWindowAvailable += liveWindowAvailable;
            client.events.PreParserAvailable += preParserAvailable;
            client.events.QuizAnswerAvailable += quizAnswerAvailable;
            client.events.QuizQuestionAvailable += quizQuestionAvailable;
            client.events.StatusChanged += statusChanged;
            client.events.StrokeAvailable += strokeAvailable;
            client.events.SubmissionAvailable += submissionAvailable;
            client.events.TextBoxAvailable += textBoxAvailable;
            client.events.VideoAvailable += videoAvailable;
        }
        private void autoShapeAvailable(object sender, AutoshapeAvailableEventArgs e)
        {
            Commands.ReceiveAutoShape.ExecuteAsync(e.autoshape);
        }
        private void bubbleAvailable(object sender, BubbleAvailableEventArgs e)
        {
        }
        private void chatAvailable(object sender, ChatAvailableEventArgs e)
        {
        }
        private void commandAvailable(object sender, CommandAvailableEventArgs e)
        {
        }
        private void conversationDetailsAvailable(object sender, ConversationDetailsAvailableEventArgs e)
        {
            Commands.UpdateConversationDetails.ExecuteAsync(e.conversationDetails);
        }
        private void dirtyAutoshapeAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyAutoShape.ExecuteAsync(e.dirtyElement);
        }
        private void dirtyImageAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyImage.ExecuteAsync(e.dirtyElement);
        }
        private void dirtyLiveWindowAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyLiveWindow.ExecuteAsync(e.dirtyElement);
        }
        private void dirtyStrokeAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyStrokes.ExecuteAsync(new[] { e.dirtyElement });
        }
        private void dirtyTextBoxAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyText.ExecuteAsync(e.dirtyElement);
        }
        private void dirtyVideoAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyVideo.ExecuteAsync(e.dirtyElement);
        }
        private void discoAvailable(object sender, DiscoAvailableEventArgs e)
        {
        }
        private void fileAvailable(object sender, FileAvailableEventArgs e)
        {
            Commands.ReceiveFileResource.ExecuteAsync(e.file);
        }
        private void imageAvailable(object sender, ImageAvailableEventArgs e)
        {
            Commands.ReceiveImage.ExecuteAsync(new []{e.image});
        }
        private void liveWindowAvailable(object sender, LiveWindowAvailableEventArgs e)
        {
            Commands.ReceiveLiveWindow.ExecuteAsync(e.livewindow);
        }
        private void preParserAvailable(object sender, PreParserAvailableEventArgs e)
        {
            Commands.PreParserAvailable.ExecuteAsync(e.parser);
        }
        private void quizAnswerAvailable(object sender, QuizAnswerAvailableEventArgs e)
        {
            Commands.ReceiveQuizAnswer.ExecuteAsync(e.quizAnswer);
        }
        private void quizQuestionAvailable(object sender, QuizQuestionAvailableEventArgs e)
        { Commands.ReceiveQuiz.ExecuteAsync(e.quizQuestion); }
        private void statusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.isConnected)
            {
                Commands.AllStaticCommandsAreRegistered();
                Commands.ConnectWithAuthenticatedCredentials.ExecuteAsync(e.credentials);
                Commands.SetIdentity.ExecuteAsync(e.credentials);
            }
            else
                App.Now("MeTLLib has indicated that it is disconnected");
        }
        private void strokeAvailable(object sender, StrokeAvailableEventArgs e)
        {
            Commands.ReceiveStroke.ExecuteAsync(e.stroke);
        }
        private void submissionAvailable(object sender, SubmissionAvailableEventArgs e)
        {
            Commands.ReceiveScreenshotSubmission.ExecuteAsync(e.submission);
        }
        private void textBoxAvailable(object sender, TextBoxAvailableEventArgs e)
        {
            Commands.ReceiveTextBox.ExecuteAsync(e.textBox);
        }
        private void videoAvailable(object sender, VideoAvailableEventArgs e)
        {
            Commands.ReceiveVideo.ExecuteAsync(e.video);
        }
        #endregion
    }
}
