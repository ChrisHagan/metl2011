using System;
using MeTLLib;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using Application = System.Windows.Application;

namespace SandRibbon.Components
{
    public class CivicServerAddress : MeTLServerAddress
    {
        public CivicServerAddress()
        {
            stagingUri = new Uri("http://civic.adm.monash.edu.au", UriKind.Absolute);
            productionUri = new Uri("http://civic.adm.monash.edu.au", UriKind.Absolute);
        }
    }

    public class StagingSearchAddress : MeTLGenericAddress
    {
        public StagingSearchAddress()
        {
            Uri = new Uri("http://meggle-staging.adm.monash.edu:8080/search?query=", UriKind.Absolute);
        }
    }

    public class ProductionSearchAddress : MeTLGenericAddress
    {
        public ProductionSearchAddress()
        {
            Uri = new Uri("http://meggle-prod.adm.monash.edu:8080/search?query=", UriKind.Absolute);
        }
    }

    public class ExternalSearchAddress : MeTLGenericAddress
    {
        public ExternalSearchAddress()
        {
            Uri = new Uri("http://meggle-ext.adm.monash.edu:8080/search?query=", UriKind.Absolute);
        }
    }

    public class NetworkController
    {
        private ClientConnection client;
        private Action deregister;
        public NetworkController()
        {
            App.mark("NetworkController instantiating");
            client = buildServerSpecificClient();
            MeTLLib.MeTLLibEventHandlers.StatusChangedEventHandler checkValidity = null;
            checkValidity = (sender,e)=>{
                if (e.isConnected && e.credentials.authorizedGroups.Count > 0)
                {
                    registerCommands();
                    attachToClient();
                    Commands.AllStaticCommandsAreRegistered();
                    Commands.SetIdentity.ExecuteAsync(e.credentials);
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
            deregister = () => { client.events.StatusChanged -= checkValidity; };
            client.events.StatusChanged += checkValidity;
        }
        public void Deregister()
        {
            deregister();
        }
        private ClientConnection buildServerSpecificClient()
        //This throws the TriedToStartMeTLWithNoInternetException if in prod mode without any network connection.
        {
            ClientConnection result;
            if (App.isExternal)
                result = MeTLLib.ClientFactory.Connection(new CivicServerAddress(), new ExternalSearchAddress());
            else
            {
                if (App.isStaging)
                    result = MeTLLib.ClientFactory.Connection(MeTLServerAddress.serverMode.STAGING, new StagingSearchAddress());
                else
                    result = MeTLLib.ClientFactory.Connection(MeTLServerAddress.serverMode.PRODUCTION, new ProductionSearchAddress());
            }
            Constants.JabberWire.SERVER = result.server.host;
            return result;
        }
        #region commands
        private void registerCommands()
        {
            Commands.RequestTeacherStatus.RegisterCommand(new DelegateCommand<TeacherStatus>(RequestTeacherStatus));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.LeaveConversation.RegisterCommand(new DelegateCommand<string>(LeaveConversation));
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
            Commands.SneakOutOf.RegisterCommand(new DelegateCommand<string>(SneakOutOf));
            Commands.LeaveAllRooms.RegisterCommand(new DelegateCommand<object>(leaveAllRooms));
            Commands.SendSyncMove.RegisterCommand(new DelegateCommand<int>(sendSyncMove));
            Commands.SendNewSlideOrder.RegisterCommand(new DelegateCommand<int>(sendNewSlideOrder));
            Commands.LeaveLocation.RegisterCommand(new DelegateCommand<object>(LeaveLocation));
        }
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
        private void LeaveLocation(object _unused)
        {
            Commands.UpdateConversationDetails.Execute(ConversationDetails.Empty);
            client.LeaveLocation();
        }
        private void SendTextBox(TargettedTextBox ttb)
        {
            client.SendTextBox(ttb);
        }
        private void SendVideo(TargettedVideo tv)
        {
            client.SendVideo(tv);
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
            client.events.StrokeAvailable += strokeAvailable;
            client.events.SubmissionAvailable += submissionAvailable;
            client.events.PresenceAvailable += presenceAvailable;
            client.events.TextBoxAvailable += textBoxAvailable;
            client.events.VideoAvailable += videoAvailable;
            client.events.SyncMoveRequested += syncMoveRequested;
            client.events.StatusChanged += statusChanged;
            client.events.SlideCollectionUpdated += slideCollectionChanged;
        }

        private void detachFromClient()
        {
            client.events.TeacherStatusReceived -= teacherStatusReceived;
            client.events.TeacherStatusRequest -= teacherStatusRequest;
            client.events.AutoshapeAvailable -= autoShapeAvailable;
            client.events.BubbleAvailable -= bubbleAvailable;
            client.events.ChatAvailable -= chatAvailable;
            client.events.CommandAvailable -= commandAvailable;
            client.events.ConversationDetailsAvailable -= conversationDetailsAvailable;
            client.events.DirtyAutoShapeAvailable -= dirtyAutoshapeAvailable;
            client.events.DirtyImageAvailable -= dirtyImageAvailable;
            client.events.DirtyLiveWindowAvailable -= dirtyLiveWindowAvailable;
            client.events.DirtyStrokeAvailable -= dirtyStrokeAvailable;
            client.events.DirtyTextBoxAvailable -= dirtyTextBoxAvailable;
            client.events.DirtyVideoAvailable -= dirtyVideoAvailable;
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
            client.events.VideoAvailable -= videoAvailable;
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
            if (e.conversationDetails != null && e.conversationDetails.Jid.GetHashCode() == ClientFactory.Connection().location.activeConversation.GetHashCode())
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
            Commands.ReceiveImage.ExecuteAsync(new[] { e.image });
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
        private void videoAvailable(object sender, VideoAvailableEventArgs e)
        {
            Commands.ReceiveVideo.ExecuteAsync(e.video);
        }
        #endregion
    }
}
