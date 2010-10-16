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
        private ClientConnection client = MeTLLib.ClientFactory.Connection();
        public NetworkController()
        {
            registerCommands();
            attachToClient();
            Commands.UpdateConversationDetails.Execute(new ConversationDetails("", "", "", new List<Slide>(), new Permissions("", false, false, false), ""));
        }
        #region commands
        private void registerCommands()
        {
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(joinConversation));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(moveTo));
            Commands.SendAutoShape.RegisterCommand(new DelegateCommand<TargettedAutoShape>(sendAutoshape));
            Commands.SendChatMessage.RegisterCommand(new DelegateCommand<object>(sendChatMessage));
            Commands.SendDirtyAutoShape.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(sendDirtyAutoshape));
            Commands.SendDirtyImage.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(sendDirtyImage));
            Commands.SendDirtyLiveWindow.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(sendDirtyLiveWindow));
            Commands.SendDirtyStroke.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(sendDirtyStroke));
            Commands.SendDirtyText.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(sendDirtyText));
            Commands.SendDirtyVideo.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(sendDirtyVideo));
            Commands.SendFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(sendFile));
            Commands.SendImage.RegisterCommand(new DelegateCommand<TargettedImage>(sendImage));
            Commands.SendLiveWindow.RegisterCommand(new DelegateCommand<LiveWindowSetup>(sendLiveWindow));
            Commands.SendNewBubble.RegisterCommand(new DelegateCommand<TargettedBubbleContext>(sendBubble));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<QuizQuestion>(sendQuiz));
            Commands.SendQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(sendQuizAnswer));
            Commands.SendScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(sendSubmission));
            Commands.SendStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(sendStroke));
            Commands.SendTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>(sendTextBox));
            Commands.SendVideo.RegisterCommand(new DelegateCommand<TargettedVideo>(sendVideo));
            Commands.SneakInto.RegisterCommand(new DelegateCommand<string>(sneakInto));
            Commands.SneakOutOf.RegisterCommand(new DelegateCommand<string>(sneakOutOf));
            //Commands._.RegisterCommand(new DelegateCommand<_>(_));
        }
        private void joinConversation(object Jid)
        {
            if (Jid is string)
                client.JoinConversation((string)Jid);
        }
        private void moveTo(int slide)
        {
            client.MoveTo(slide);
        }
        private void sendAutoshape(TargettedAutoShape tas)
        {
        }
        private void sendChatMessage(object _obj)
        {
        }
        private void sendDirtyAutoshape(TargettedDirtyElement tde)
        {
        }
        private void sendDirtyImage(TargettedDirtyElement tde)
        {
            client.SendDirtyImage(tde);
        }
        private void sendDirtyLiveWindow(TargettedDirtyElement tde)
        {
        }
        private void sendDirtyStroke(TargettedDirtyElement tde)
        {
            client.SendDirtyStroke(tde);
        }
        private void sendDirtyText(TargettedDirtyElement tde)
        {
            client.SendDirtyTextBox(tde);
        }
        private void sendDirtyVideo(TargettedDirtyElement tde)
        {
            client.SendDirtyVideo(tde);
        }
        private void sendFile(TargettedFile tf)
        {
            client.SendFile(tf);
        }
        private void sendImage(TargettedImage ti)
        {
            client.SendImage(ti);
        }
        private void sendLiveWindow(LiveWindowSetup lws)
        {
        }
        private void sendBubble(TargettedBubbleContext tbc)
        {
        }
        private void sendQuiz(QuizQuestion qq)
        {
            client.SendQuizQuestion(qq);
        }
        private void sendQuizAnswer(QuizAnswer qa)
        {
            client.SendQuizAnswer(qa);
        }
        private void sendSubmission(TargettedSubmission ts)
        {
            client.SendSubmission(ts);
        }
        private void sendStroke(TargettedStroke ts)
        {
            client.SendStroke(ts);
        }
        private void sendTextBox(TargettedTextBox ttb)
        {
            client.SendTextBox(ttb);
        }
        private void sendVideo(TargettedVideo tv)
        {
            client.SendVideo(tv);
        }
        private void sneakInto(string room)
        {
            client.SneakInto(room);
        }
        private void sneakOutOf(string room)
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
            Commands.ReceiveAutoShape.Execute(e.autoshape);
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
            Commands.UpdateConversationDetails.Execute(e.conversationDetails);
        }
        private void dirtyAutoshapeAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyAutoShape.Execute(e.dirtyElement);
        }
        private void dirtyImageAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyImage.Execute(e.dirtyElement);
        }
        private void dirtyLiveWindowAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyLiveWindow.Execute(e.dirtyElement);
        }
        private void dirtyStrokeAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyStrokes.Execute(new[] { e.dirtyElement });
        }
        private void dirtyTextBoxAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyText.Execute(e.dirtyElement);
        }
        private void dirtyVideoAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            Commands.ReceiveDirtyVideo.Execute(e.dirtyElement);
        }
        private void discoAvailable(object sender, DiscoAvailableEventArgs e)
        {
        }
        private void fileAvailable(object sender, FileAvailableEventArgs e)
        {
            Commands.ReceiveFileResource.Execute(e.file);
        }
        private void imageAvailable(object sender, ImageAvailableEventArgs e)
        {
            Commands.ReceiveImage.Execute(new []{e.image});
        }
        private void liveWindowAvailable(object sender, LiveWindowAvailableEventArgs e)
        {
            Commands.ReceiveLiveWindow.Execute(e.livewindow);
        }
        private void preParserAvailable(object sender, PreParserAvailableEventArgs e)
        {
            Commands.PreParserAvailable.Execute(e.parser);
        }
        private void quizAnswerAvailable(object sender, QuizAnswerAvailableEventArgs e)
        {
            Commands.ReceiveQuizAnswer.Execute(e.quizAnswer);
        }
        private void quizQuestionAvailable(object sender, QuizQuestionAvailableEventArgs e)
        { Commands.ReceiveQuiz.Execute(e.quizQuestion); }
        private void statusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.isConnected)
            {
                Commands.AllStaticCommandsAreRegistered();
                Commands.ConnectWithAuthenticatedCredentials.Execute(e.credentials);
                Commands.SetIdentity.Execute(e.credentials);
            }
            else
                App.Now("MeTLLib has indicated that it is disconnected");
        }
        private void strokeAvailable(object sender, StrokeAvailableEventArgs e)
        {
            Commands.ReceiveStroke.Execute(e.stroke);
        }
        private void submissionAvailable(object sender, SubmissionAvailableEventArgs e)
        {
            Commands.ReceiveScreenshotSubmission.Execute(e.submission);
        }
        private void textBoxAvailable(object sender, TextBoxAvailableEventArgs e)
        {
            Commands.ReceiveTextBox.Execute(e.textBox);
        }
        private void videoAvailable(object sender, VideoAvailableEventArgs e)
        {
            Commands.ReceiveVideo.Execute(e.video);
        }
        #endregion
    }
}
