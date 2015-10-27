using System;
using MeTLLib;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using Application = System.Windows.Application;
using System.Diagnostics;
using System.Collections.Generic;

namespace SandRibbon.Components
{
    public class NetworkController
    {
        public ClientConnection client { get; protected set; }
        public MetlConfiguration config { get; protected set; }
        public Credentials creds { get; protected set; }
        public MeTLLib.Commands commands { get; protected set; }
        public NetworkController(MetlConfiguration _config,Credentials _creds)
        {
            config = _config;
            creds = _creds;
            commands = new MeTLLib.Commands();
            commands.Mark.Execute(String.Format("NetworkController instantiating: {0}",config));
            client = buildServerSpecificClient(config,creds,commands);
            MeTLLib.MeTLLibEventHandlers.StatusChangedEventHandler checkValidity = null;
            checkValidity = (sender,e)=>{
                if (e.isConnected && e.credentials.authorizedGroups.Count > 0)
                {
                    registerCommands();
                    attachToClient();
                    commands.AllStaticCommandsAreRegistered();
                    client.events.StatusChanged -= checkValidity;
                }
                else
                {
                    if (WorkspaceStateProvider.savedStateExists())
                    {
                        commands.LogOut.Execute(true);
                    }
                }
            };
            client.events.StatusChanged += checkValidity;
        }       
        private ClientConnection buildServerSpecificClient(MetlConfiguration c,Credentials creds,MeTLLib.Commands cmds)
        //This throws the TriedToStartMeTLWithNoInternetException if in prod mode without any network connection.
        {
            return MeTLLib.ClientFactory.Connection(c,creds,cmds);
        }
        #region commands
        private void registerCommands()
        {
            //commands.RequestMeTLUserInformations.RegisterCommand(new DelegateCommand<List<string>>(RequestUserInformations));
            commands.RequestTeacherStatus.RegisterCommand(new DelegateCommand<TeacherStatus>(RequestTeacherStatus));
            commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            commands.LeaveConversation.RegisterCommand(new DelegateCommand<string>(LeaveConversation));
            commands.MoveToCollaborationPage.RegisterCommand(new DelegateCommand<int>(MoveTo));
            commands.SendChatMessage.RegisterCommand(new DelegateCommand<object>(SendChatMessage));
            commands.SendDirtyAutoShape.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyAutoshape));
            commands.SendDirtyImage.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyImage));
            commands.SendDirtyLiveWindow.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyLiveWindow));
            commands.SendDirtyStroke.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyStroke));
            commands.SendDirtyText.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyText));
            commands.SendFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(SendFile));
            commands.SendImage.RegisterCommand(new DelegateCommand<TargettedImage>(SendImage));
            commands.SendLiveWindow.RegisterCommand(new DelegateCommand<LiveWindowSetup>(SendLiveWindow));
            commands.SendQuiz.RegisterCommand(new DelegateCommand<QuizQuestion>(SendQuiz));
            commands.SendQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(SendQuizAnswer));
            commands.SendScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(SendSubmission));
            commands.SendStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(SendStroke));
            commands.SendTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>(SendTextBox));
            commands.SendMoveDelta.RegisterCommand(new DelegateCommand<TargettedMoveDelta>(SendMoveDelta));
            commands.SneakInto.RegisterCommand(new DelegateCommand<string>(SneakInto));
            commands.SneakOutOf.RegisterCommand(new DelegateCommand<string>(SneakOutOf));
            commands.LeaveAllRooms.RegisterCommand(new DelegateCommand<object>(leaveAllRooms));
            commands.SendSyncMove.RegisterCommand(new DelegateCommand<int>(sendSyncMove));
            commands.SendNewSlideOrder.RegisterCommand(new DelegateCommand<int>(sendNewSlideOrder));
            commands.LeaveLocation.RegisterCommand(new DelegateCommand<object>(LeaveLocation));
        }
        /*
        private void RequestUserInformations(List<string> usernames)
        {
            var results = client.getMeTLUserInformations(usernames);
            commands.ReceiveMeTLUserInformations.Execute(results);
        }
        */
        private void RequestTeacherStatus(TeacherStatus obj)
        {
            client.AskForTeachersStatus(obj.Teacher, obj.Conversation);
        }
        private void sendNewSlideOrder(int conversationJid)
        {
            client.UpdateSlideCollection(conversationJid);
        }
        private void sendSyncMove(int slide)
        {
            client.SendSyncMove(slide);
        }
        private void leaveAllRooms(object _obj)
        {
            detachFromClient(); // don't care about events anymore
            client.LeaveAllRooms();
        }
        private void LeaveConversation(string Jid)
        {
            client.LeaveConversation(Jid);
        }
        private void JoinConversation(string jid)
        {
            client.JoinConversation(jid);
            commands.CheckExtendedDesktop.ExecuteAsync(null);
        }
        private void MoveTo(int slide)
        {
            client.MoveTo(slide);
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
        private void LeaveLocation(object _unused)
        {
            commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
            client.LeaveLocation();
        }
        private void SendTextBox(TargettedTextBox ttb)
        {
            client.SendTextBox(ttb);
        }
        private void SendMoveDelta(TargettedMoveDelta moveDelta)
        {
            client.SendMoveDelta(moveDelta);
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
            client.events.TeacherStatusReceived += teacherStatusReceived;
            client.events.TeacherStatusRequest += teacherStatusRequest;
            client.events.ChatAvailable += chatAvailable;
            client.events.CommandAvailable += commandAvailable;
            client.events.ConversationDetailsAvailable += conversationDetailsAvailable;
            client.events.DirtyAutoShapeAvailable += dirtyAutoshapeAvailable;
            client.events.DirtyImageAvailable += dirtyImageAvailable;
            client.events.DirtyLiveWindowAvailable += dirtyLiveWindowAvailable;
            client.events.DirtyStrokeAvailable += dirtyStrokeAvailable;
            client.events.DirtyTextBoxAvailable += dirtyTextBoxAvailable;
            client.events.MoveDeltaAvailable += moveDeltaAvailable;
            client.events.DiscoAvailable += discoAvailable;
            client.events.FileAvailable += fileAvailable;
            client.events.ImageAvailable += imageAvailable;
            client.events.LiveWindowAvailable += liveWindowAvailable;
            client.events.PreParserAvailable += preParserAvailable;
            client.events.QuizAnswerAvailable += quizAnswerAvailable;
            client.events.QuizQuestionAvailable += quizQuestionAvailable;
            client.events.StrokeAvailable += strokeAvailable;
            client.events.SubmissionAvailable += submissionAvailable;
            client.events.PresenceAvailable += presenceAvailable;
            client.events.TextBoxAvailable += textBoxAvailable;
            client.events.SyncMoveRequested += syncMoveRequested;
            client.events.StatusChanged += statusChanged;
            client.events.SlideCollectionUpdated += slideCollectionChanged;
        }

        private void detachFromClient()
        {
            client.events.TeacherStatusReceived -= teacherStatusReceived;
            client.events.TeacherStatusRequest -= teacherStatusRequest;
            client.events.ChatAvailable -= chatAvailable;
            client.events.CommandAvailable -= commandAvailable;
            client.events.ConversationDetailsAvailable -= conversationDetailsAvailable;
            client.events.DirtyAutoShapeAvailable -= dirtyAutoshapeAvailable;
            client.events.DirtyImageAvailable -= dirtyImageAvailable;
            client.events.DirtyLiveWindowAvailable -= dirtyLiveWindowAvailable;
            client.events.DirtyStrokeAvailable -= dirtyStrokeAvailable;
            client.events.DirtyTextBoxAvailable -= dirtyTextBoxAvailable;
            client.events.MoveDeltaAvailable -= moveDeltaAvailable;
            client.events.DiscoAvailable -= discoAvailable;
            client.events.FileAvailable -= fileAvailable;
            client.events.ImageAvailable -= imageAvailable;
            client.events.LiveWindowAvailable -= liveWindowAvailable;
            client.events.PreParserAvailable -= preParserAvailable;
            client.events.QuizAnswerAvailable -= quizAnswerAvailable;
            client.events.QuizQuestionAvailable -= quizQuestionAvailable;
            client.events.StrokeAvailable -= strokeAvailable;
            client.events.SubmissionAvailable -= submissionAvailable;
            client.events.PresenceAvailable -= presenceAvailable;
            client.events.TextBoxAvailable -= textBoxAvailable;
            client.events.SyncMoveRequested -= syncMoveRequested;
            client.events.StatusChanged -= statusChanged;
            client.events.SlideCollectionUpdated -= slideCollectionChanged;
        }
        private void teacherStatusReceived(object sender, TeacherStatusRequestEventArgs e)
        {
            commands.ReceiveTeacherStatus.Execute(e.status);
        }
        private void teacherStatusRequest(object sender, TeacherStatusRequestEventArgs e)
        {
            if(e.status.Conversation == Globals.conversationDetails.Jid && Globals.isAuthor)
            {
                client.SendTeacherStatus(new TeacherStatus
                                             {
                                                 Joining = true,
                                                 Teacher = Globals.me,
                                                 Conversation = Globals.conversationDetails.Jid,
                                                 Slide = Globals.location.currentSlide.ToString()
                                             });
            }
        }
        private void presenceAvailable(object sender, PresenceAvailableEventArgs e)
        {
           commands.ReceiveTeacherStatus.Execute(new TeacherStatus
           {
               Conversation = e.presence.Where,
               Joining = e.presence.Joining,
               Slide = e.presence.Where,
               Teacher = e.presence.Who
           });
        }

        private void slideCollectionChanged(object sender, SlideCollectionUpdatedEventArgs e)
        {
            commands.UpdateNewSlideOrder.Execute(e.Conversation);
        }
        private void syncMoveRequested(object sender, SyncMoveRequestedEventArgs e)
        {
            commands.SyncedMoveRequested.Execute(e.where);
        }
        private void chatAvailable(object sender, ChatAvailableEventArgs e)
        {
        }
        private void commandAvailable(object sender, CommandAvailableEventArgs e)
        {
        }
        
        private void conversationDetailsAvailable(object sender, ConversationDetailsAvailableEventArgs e)
        {
            if (e.conversationDetails != null && e.conversationDetails.Jid.GetHashCode() == client.location.activeConversation.GetHashCode())
                commands.UpdateConversationDetails.Execute(e.conversationDetails);
            else 
            {
                Application.Current.Dispatcher.adopt(() => commands.UpdateForeignConversationDetails.Execute(e.conversationDetails));
            }
        }
        private void dirtyAutoshapeAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            commands.ReceiveDirtyAutoShape.ExecuteAsync(e.dirtyElement);
        }
        private void dirtyImageAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            commands.ReceiveDirtyImage.ExecuteAsync(e.dirtyElement);
        }
        private void dirtyLiveWindowAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            commands.ReceiveDirtyLiveWindow.ExecuteAsync(e.dirtyElement);
        }
        private void dirtyStrokeAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            commands.ReceiveDirtyStrokes.ExecuteAsync(new[] { e.dirtyElement });
        }
        private void dirtyTextBoxAvailable(object sender, DirtyElementAvailableEventArgs e)
        {
            commands.ReceiveDirtyText.ExecuteAsync(e.dirtyElement);
        }
        private void discoAvailable(object sender, DiscoAvailableEventArgs e)
        {
        }
        private void fileAvailable(object sender, FileAvailableEventArgs e)
        {
            commands.ReceiveFileResource.ExecuteAsync(e.file);
        }
        private void imageAvailable(object sender, ImageAvailableEventArgs e)
        {
            commands.ReceiveImage.ExecuteAsync(e.image);
        }
        private void moveDeltaAvailable(object sender, MoveDeltaAvailableEventArgs e)
        {
            commands.ReceiveMoveDelta.ExecuteAsync(e.moveDelta);
        }
        private void liveWindowAvailable(object sender, LiveWindowAvailableEventArgs e)
        {
            commands.ReceiveLiveWindow.ExecuteAsync(e.livewindow);
        }
        private void preParserAvailable(object sender, PreParserAvailableEventArgs e)
        {
            commands.PreParserAvailable.ExecuteAsync(e.parser);
        }
        private void quizAnswerAvailable(object sender, QuizAnswerAvailableEventArgs e)
        {
            commands.ReceiveQuizAnswer.ExecuteAsync(e.QuizAnswer);
        }
        private void quizQuestionAvailable(object sender, QuizQuestionAvailableEventArgs e)
        { commands.ReceiveQuiz.ExecuteAsync(e.quizQuestion); }
        private void statusChanged(object sender, StatusChangedEventArgs e)
        {
            commands.Reconnecting.Execute(e.isConnected);
        }
        private void strokeAvailable(object sender, StrokeAvailableEventArgs e)
        {
            commands.ReceiveStroke.ExecuteAsync(e.stroke);
        }
        private void submissionAvailable(object sender, SubmissionAvailableEventArgs e)
        {
            commands.ReceiveScreenshotSubmission.ExecuteAsync(e.submission);
        }
        private void textBoxAvailable(object sender, TextBoxAvailableEventArgs e)
        {
            commands.ReceiveTextBox.ExecuteAsync(e.textBox);
        }
        #endregion
    }
}
