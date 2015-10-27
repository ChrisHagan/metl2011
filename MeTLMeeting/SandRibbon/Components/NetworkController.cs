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
        protected DiagnosticGauge gauge = new DiagnosticGauge("networkController startup", "connection", DateTime.Now);
        public IClientBehaviour client { get; protected set; }
        private Action deregister;
        public MetlConfiguration config { get; protected set; }
        public Credentials credentials { get; protected set; }
        public NetworkController(MetlConfiguration _config)
        {
            Commands.DiagnosticGaugeUpdated.Execute(gauge);
            config = _config;
            gauge.update(GaugeStatus.InProgress);
            Commands.DiagnosticGaugeUpdated.Execute(gauge);
            //App.mark(String.Format("NetworkController instantiating: {0}", config));
        }
        public IClientBehaviour connect(Credentials _creds) {
            gauge.update(GaugeStatus.InProgress);
            Commands.DiagnosticGaugeUpdated.Execute(gauge);
            credentials = _creds;
            client = buildServerSpecificClient(config,_creds);
            MeTLLib.MeTLLibEventHandlers.StatusChangedEventHandler checkValidity = null;
            gauge.update(GaugeStatus.InProgress);
            Commands.DiagnosticGaugeUpdated.Execute(gauge);
            checkValidity = (sender,e)=>{
                if (e.isConnected && e.credentials.authorizedGroups.Count > 0)
                {
                    registerCommands();
                    attachToClient();
                    Commands.AllStaticCommandsAreRegistered();
                    client.events.StatusChanged -= checkValidity;
                }
                else
                {
                    if (WorkspaceStateProvider.savedStateExists())
                    {
                        Commands.LogOut.Execute(true);
                    }
                }
            };
            gauge.update(GaugeStatus.InProgress);
            Commands.DiagnosticGaugeUpdated.Execute(gauge);
            client.events.StatusChanged += checkValidity;
            gauge.update(GaugeStatus.Completed);
            Commands.DiagnosticGaugeUpdated.Execute(gauge);
            return client;
        }       
        private ClientConnection buildServerSpecificClient(MetlConfiguration config,Credentials creds)
        //This throws the TriedToStartMeTLWithNoInternetException if in prod mode without any network connection.
        {
            return MeTLLib.ClientFactory.Connection(config,creds);
            /*
            ClientConnection result;
            switch (mode)
            {
                case MeTLServerAddress.serverMode.EXTERNAL:
                    {
                        result = MeTLLib.ClientFactory.Connection(MeTLServerAddress.serverMode.EXTERNAL, new ExternalSearchAddress());
                    }
                    break;
                case MeTLServerAddress.serverMode.STAGING:
                    {
                        result = MeTLLib.ClientFactory.Connection(MeTLServerAddress.serverMode.STAGING, new StagingSearchAddress());
                    }
                    break;
                default:
                    {
                        result = MeTLLib.ClientFactory.Connection(MeTLServerAddress.serverMode.PRODUCTION, new ProductionSearchAddress());
                    }
                    break;
            }            
            return result;
            */
        }
        #region commands
        private void registerCommands()
        {
            //Commands.RequestMeTLUserInformations.RegisterCommand(new DelegateCommand<List<string>>(RequestUserInformations));
            Commands.RequestTeacherStatus.RegisterCommand(new DelegateCommand<TeacherStatus>(RequestTeacherStatus));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.LeaveConversation.RegisterCommand(new DelegateCommand<string>(LeaveConversation));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
            Commands.SendChatMessage.RegisterCommand(new DelegateCommand<object>(SendChatMessage));
            Commands.SendDirtyAutoShape.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyAutoshape));
            Commands.SendDirtyImage.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyImage));
            Commands.SendDirtyLiveWindow.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyLiveWindow));
            Commands.SendDirtyStroke.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyStroke));
            Commands.SendDirtyText.RegisterCommand(new DelegateCommand<TargettedDirtyElement>(SendDirtyText));
            Commands.SendFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(SendFile));
            Commands.SendImage.RegisterCommand(new DelegateCommand<TargettedImage>(SendImage));
            Commands.SendLiveWindow.RegisterCommand(new DelegateCommand<LiveWindowSetup>(SendLiveWindow));
            Commands.SendQuiz.RegisterCommand(new DelegateCommand<QuizQuestion>(SendQuiz));
            Commands.SendQuizAnswer.RegisterCommand(new DelegateCommand<QuizAnswer>(SendQuizAnswer));
            Commands.SendScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(SendSubmission));
            Commands.SendStroke.RegisterCommand(new DelegateCommand<TargettedStroke>(SendStroke));
            Commands.SendTextBox.RegisterCommand(new DelegateCommand<TargettedTextBox>(SendTextBox));
            Commands.SendMoveDelta.RegisterCommand(new DelegateCommand<TargettedMoveDelta>(SendMoveDelta));
            Commands.SneakInto.RegisterCommand(new DelegateCommand<string>(SneakInto));
            Commands.SneakOutOf.RegisterCommand(new DelegateCommand<string>(SneakOutOf));
            Commands.LeaveAllRooms.RegisterCommand(new DelegateCommand<object>(leaveAllRooms));
            Commands.SendSyncMove.RegisterCommand(new DelegateCommand<int>(sendSyncMove));
            Commands.SendNewSlideOrder.RegisterCommand(new DelegateCommand<int>(sendNewSlideOrder));
            Commands.LeaveLocation.RegisterCommand(new DelegateCommand<object>(LeaveLocation));
        }
        /*
        private void RequestUserInformations(List<string> usernames)
        {
            var results = client.getMeTLUserInformations(usernames);
            Commands.ReceiveMeTLUserInformations.Execute(results);
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
            Commands.CheckExtendedDesktop.ExecuteAsync(null);
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
            Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
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
            Commands.ReceiveTeacherStatus.Execute(e.status);
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
           Commands.ReceiveTeacherStatus.Execute(new TeacherStatus
           {
               Conversation = e.presence.Where,
               Joining = e.presence.Joining,
               Slide = e.presence.Where,
               Teacher = e.presence.Who
           });
        }

        private void slideCollectionChanged(object sender, SlideCollectionUpdatedEventArgs e)
        {
            Commands.UpdateNewSlideOrder.Execute(e.Conversation);
        }
        private void syncMoveRequested(object sender, SyncMoveRequestedEventArgs e)
        {
            Commands.SyncedMoveRequested.Execute(e.where);
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
                Commands.UpdateConversationDetails.Execute(e.conversationDetails);
            else 
            {
                Application.Current.Dispatcher.adopt(() => Commands.UpdateForeignConversationDetails.Execute(e.conversationDetails));
            }
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
        private void discoAvailable(object sender, DiscoAvailableEventArgs e)
        {
        }
        private void fileAvailable(object sender, FileAvailableEventArgs e)
        {
            Commands.ReceiveFileResource.ExecuteAsync(e.file);
        }
        private void imageAvailable(object sender, ImageAvailableEventArgs e)
        {
            Commands.ReceiveImage.ExecuteAsync(e.image);
        }
        private void moveDeltaAvailable(object sender, MoveDeltaAvailableEventArgs e)
        {
            Commands.ReceiveMoveDelta.ExecuteAsync(e.moveDelta);
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
            Commands.ReceiveQuizAnswer.ExecuteAsync(e.QuizAnswer);
        }
        private void quizQuestionAvailable(object sender, QuizQuestionAvailableEventArgs e)
        { Commands.ReceiveQuiz.ExecuteAsync(e.quizQuestion); }
        private void statusChanged(object sender, StatusChangedEventArgs e)
        {
            Commands.Reconnecting.Execute(e.isConnected);
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
        #endregion
    }
}
