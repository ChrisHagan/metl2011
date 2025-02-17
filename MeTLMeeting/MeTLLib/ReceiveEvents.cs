﻿using System;
using System.Collections.Generic;
using System.Linq;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Diagnostics;

namespace MeTLLib
{
    #region EventHandlers
    public class MeTLLibEventHandlers
    {
        public delegate void QuizzesAvailableRequestEventHandler(object sender, QuizzesAvailableEventArgs e); 
        public delegate void AttachmentsAvailableRequestEventHandler(object sender, AttachmentsAvailableEventArgs e); 
        public delegate void QuizAvailableRequestEventHandler(object sender, QuizzesAvailableEventArgs e); 
        public delegate void TeacherStatusRequestEventHandler(object sender, TeacherStatusRequestEventArgs e);
        public delegate void TeacherStatusReceivedEventHandler(object sender, TeacherStatusRequestEventArgs e);
        public delegate void PresenceAvailableEventHandler(object sender, PresenceAvailableEventArgs e);
        public delegate void SubmissionAvailableEventHandler(object sender, SubmissionAvailableEventArgs e);
        public delegate void SubmissionsAvailableEventHandler(object sender, SubmissionsAvailableEventArgs e);
        public delegate void SlideCollectionUpdatedEventHandler(object sender, SlideCollectionUpdatedEventArgs e);
        public delegate void FileAvailableEventHandler(object sender, FileAvailableEventArgs e);
        public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
        public delegate void PreParserAvailableEventHandler(object sender, PreParserAvailableEventArgs e);
        public delegate void StrokeAvailableEventHandler(object sender, StrokeAvailableEventArgs e);
        public delegate void ImageAvailableEventHandler(object sender, ImageAvailableEventArgs e);
        public delegate void TextBoxAvailableEventHandler(object sender, TextBoxAvailableEventArgs e);
        public delegate void DirtyElementAvailableEventHandler(object sender, DirtyElementAvailableEventArgs e);
        public delegate void MoveDeltaAvailableEventHandler(object sender, MoveDeltaAvailableEventArgs e);
        public delegate void LiveWindowAvailableEventHandler(object sender, LiveWindowAvailableEventArgs e);
        public delegate void DiscoAvailableEventHandler(object sender, DiscoAvailableEventArgs e);
        public delegate void QuizAnswerAvailableEventHandler(object sender, QuizAnswerAvailableEventArgs e);
        public delegate void QuizQuestionAvailableEventHandler(object sender, QuizQuestionAvailableEventArgs e);
        public delegate void ChatAvailableEventHandler(object sender, ChatAvailableEventArgs e);
        public delegate void ConversationDetailsAvailableEventHandler(object sender, ConversationDetailsAvailableEventArgs e);
        public delegate void CommandAvailableEventHandler(object sender, CommandAvailableEventArgs e);
        public delegate void SyncMoveRequestedEventHandler(object sender, SyncMoveRequestedEventArgs e);
    }
    #endregion
    #region EventArgs
    public class QuizzesAvailableEventArgs : EventArgs { public List<QuizInfo> quizzes; }
    public class AttachmentsAvailableEventArgs : EventArgs { public List<TargettedFile> attachments; }
    public class SingleQuizAvailableEventArgs : EventArgs { public QuizInfo quiz; }
    public class TeacherStatusRequestEventArgs : EventArgs { public TeacherStatus status; } 
    public class PresenceAvailableEventArgs : EventArgs { public MeTLPresence presence; }
    public class SubmissionAvailableEventArgs : EventArgs { public TargettedSubmission submission;}
    public class SubmissionsAvailableEventArgs : EventArgs { public List<TargettedSubmission> submissions;}
    public class FileAvailableEventArgs : EventArgs { public TargettedFile file;}
    public class SlideCollectionUpdatedEventArgs : EventArgs { public Int32 Conversation;}
    public class StatusChangedEventArgs : EventArgs { public bool isConnected; public Credentials credentials;}
    public class PreParserAvailableEventArgs : EventArgs { public PreParser parser; }
    public class StrokeAvailableEventArgs : EventArgs { public TargettedStroke stroke;}
    public class ImageAvailableEventArgs : EventArgs { public TargettedImage image;}
    public class TextBoxAvailableEventArgs : EventArgs { public TargettedTextBox textBox;}
    public class DirtyElementAvailableEventArgs : EventArgs { public TargettedDirtyElement dirtyElement;}
    public class MoveDeltaAvailableEventArgs : EventArgs { public TargettedMoveDelta moveDelta;}
    public class LiveWindowAvailableEventArgs : EventArgs { public LiveWindowSetup livewindow;}
    public class DiscoAvailableEventArgs : EventArgs { public string disco;}
    public class QuizAnswerAvailableEventArgs : EventArgs { public QuizAnswer QuizAnswer;}
    public class QuizQuestionAvailableEventArgs : EventArgs { public QuizQuestion quizQuestion;}
    public class ChatAvailableEventArgs : EventArgs { public TargettedTextBox chat;}
    public class ConversationDetailsAvailableEventArgs : EventArgs { public ConversationDetails conversationDetails;}
    public class CommandAvailableEventArgs : EventArgs { public string command;}
    public class SyncMoveRequestedEventArgs: EventArgs {public int where;}
    #endregion
    public interface IReceiveEvents
    {
        void receivePresence(MeTLPresence presence);
        void receiveSubmission(TargettedSubmission ts);
        void receivesubmissions(PreParser parser);
        void receiveQuiz(QuizQuestion qq);
        void receiveUpdatedSlideCollection(Int32 conversationJid);
        void receiveQuizAnswer(QuizAnswer qa);
        void receiveFileResource(TargettedFile tf);
        void receiveStroke(TargettedStroke ts);
        void receiveStrokes(TargettedStroke[] tsa);
        void receiveImages(TargettedImage[] tia);
        void receiveImage(TargettedImage ti);
        void receiveTextBox(TargettedTextBox ttb);
        void receiveDirtyStroke(TargettedDirtyElement tde);
        void receiveDirtyTextBox(TargettedDirtyElement tde);
        void receiveDirtyVideo(TargettedDirtyElement tde);
        void receiveDirtyImage(TargettedDirtyElement tde);
        void receiveMoveDelta(TargettedMoveDelta moveDelta);
        void receiveChat(TargettedTextBox ttb);
        void receivePreParser(PreParser pp);
        void receiveLiveWindow(LiveWindowSetup lws);
        void receiveDirtyLiveWindow(TargettedDirtyElement tde);
        void receiveDirtyAutoShape(TargettedDirtyElement tde);
        void receiveConversationDetails(ConversationDetails cd);
        void statusChanged(bool isConnected, Credentials credentials);
        void syncMoveRequested(int where);
        void teacherStatusRequest(string where, string who);
        void teacherStatusRecieved(TeacherStatus status);
        void receieveQuizzes(PreParser finishedParser);
        void receieveAttachments(PreParser finishedParser);
        void receieveQuiz(PreParser finishedParser, long id);
        event MeTLLibEventHandlers.AttachmentsAvailableRequestEventHandler AttachmentsAvailable;
        event MeTLLibEventHandlers.QuizzesAvailableRequestEventHandler QuizzesAvailable;
        event MeTLLibEventHandlers.QuizAvailableRequestEventHandler QuizAvailable;
        event MeTLLibEventHandlers.TeacherStatusReceivedEventHandler TeacherStatusReceived;
        event MeTLLibEventHandlers.TeacherStatusRequestEventHandler TeacherStatusRequest;
        event MeTLLibEventHandlers.PresenceAvailableEventHandler PresenceAvailable;
        event MeTLLibEventHandlers.SubmissionAvailableEventHandler SubmissionAvailable;
        event MeTLLibEventHandlers.SubmissionsAvailableEventHandler SubmissionsAvailable;
        event MeTLLibEventHandlers.SlideCollectionUpdatedEventHandler SlideCollectionUpdated;
        event MeTLLibEventHandlers.FileAvailableEventHandler FileAvailable;
        event MeTLLibEventHandlers.StatusChangedEventHandler StatusChanged;
        event MeTLLibEventHandlers.PreParserAvailableEventHandler PreParserAvailable;
        event MeTLLibEventHandlers.StrokeAvailableEventHandler StrokeAvailable;
        event MeTLLibEventHandlers.ImageAvailableEventHandler ImageAvailable;
        event MeTLLibEventHandlers.TextBoxAvailableEventHandler TextBoxAvailable;
        event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyTextBoxAvailable;
        event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyImageAvailable;
        event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyVideoAvailable;
        event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyStrokeAvailable;
        event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyLiveWindowAvailable;
        event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyAutoShapeAvailable;
        event MeTLLibEventHandlers.MoveDeltaAvailableEventHandler MoveDeltaAvailable;
        event MeTLLibEventHandlers.LiveWindowAvailableEventHandler LiveWindowAvailable;
        event MeTLLibEventHandlers.DiscoAvailableEventHandler DiscoAvailable;
        event MeTLLibEventHandlers.QuizQuestionAvailableEventHandler QuizQuestionAvailable;
        event MeTLLibEventHandlers.QuizAnswerAvailableEventHandler QuizAnswerAvailable;
        event MeTLLibEventHandlers.ChatAvailableEventHandler ChatAvailable;
        event MeTLLibEventHandlers.ConversationDetailsAvailableEventHandler ConversationDetailsAvailable;
        event MeTLLibEventHandlers.CommandAvailableEventHandler CommandAvailable;
        event MeTLLibEventHandlers.SyncMoveRequestedEventHandler SyncMoveRequested;
    }

    class ProductionReceiveEvents : IReceiveEvents
    {
        public ProductionReceiveEvents()
        {
            this.PresenceAvailable += (sender, args) => { } ;
            this.SlideCollectionUpdated += (sender, args) => { };
            this.ChatAvailable += (sender, args) => { };
            this.CommandAvailable += (sender, args) => { };
            this.ConversationDetailsAvailable += (sender, args) => { };
            this.DirtyAutoShapeAvailable += (sender, args) => { };
            this.DirtyImageAvailable += (sender, args) => { };
            this.DirtyLiveWindowAvailable += (sender, args) => { };
            this.DirtyStrokeAvailable += (sender, args) => { };
            this.DirtyTextBoxAvailable += (sender, args) => { };
            this.DirtyVideoAvailable += (sender, args) => { };
            this.DiscoAvailable += (sender, args) => { };
            this.FileAvailable += (sender, args) => { };
            this.ImageAvailable += (sender, args) => { };
            this.MoveDeltaAvailable += (sender, args) => { };
            this.LiveWindowAvailable += (sender, args) => { };
            this.PreParserAvailable += (sender, args) => { };
            this.QuizAnswerAvailable += (sender, args) => { };
            this.QuizQuestionAvailable += (sender, args) => { };
            this.StatusChanged += (sender,args) =>{ };
            this.StrokeAvailable += (sender, args) => { };
            this.SubmissionAvailable += (sender, args) => { };
            this.TextBoxAvailable += (sender, args) => { };
            this.TeacherStatusRequest += (sender, args) => { };
            this.TeacherStatusReceived += (sender, args)=> { };
            this.SyncMoveRequested += (sender, SyncMoveRequestedEventArgs) => { };
            Commands.ServersDown.RegisterCommand(new DelegateCommand<String>(ServersDown));
        }
        private void ServersDown(string url){
            Trace.TraceError("CRASH: (Fixed) MeTLLib::ProductionReceiveEvents:ServersDown {0}", url);
            statusChanged(false, null);
        }

        void IReceiveEvents.receiveSubmission(TargettedSubmission ts)
        {
            SubmissionAvailable(this, new SubmissionAvailableEventArgs { submission = ts });
        }
        public void receivesubmissions(PreParser parser)
        {
            SubmissionsAvailable(this, new SubmissionsAvailableEventArgs{submissions = parser.submissions});
        }
        void IReceiveEvents.receiveUpdatedSlideCollection(Int32 conversationJid)
        {
            SlideCollectionUpdated(this, new SlideCollectionUpdatedEventArgs { Conversation = conversationJid });
        }
        void IReceiveEvents.receiveQuiz(QuizQuestion qq)
        {
            QuizQuestionAvailable(this, new QuizQuestionAvailableEventArgs { quizQuestion = qq });
        }
        void IReceiveEvents.receiveQuizAnswer(QuizAnswer qa)
        {
            QuizAnswerAvailable(this, new QuizAnswerAvailableEventArgs { QuizAnswer = qa });
        }
        void IReceiveEvents.receiveFileResource(TargettedFile tf)
        {
            FileAvailable(this, new FileAvailableEventArgs { file = tf });
        }
        void IReceiveEvents.receivePresence(MeTLPresence presence)
        {
            PresenceAvailable(this, new PresenceAvailableEventArgs {presence = presence});
        }
        void IReceiveEvents.receiveStroke(TargettedStroke ts)
        {
            StrokeAvailable(this, new StrokeAvailableEventArgs { stroke = ts });
        }
        void IReceiveEvents.receiveStrokes(TargettedStroke[] tsa)
        {
            foreach (TargettedStroke ts in tsa)
                StrokeAvailable(this, new StrokeAvailableEventArgs { stroke = ts });
        }
        void IReceiveEvents.receiveImages(TargettedImage[] tia)
        {
            foreach (TargettedImage ti in tia)
                ImageAvailable(this, new ImageAvailableEventArgs { image = ti });
        }
        void IReceiveEvents.receiveImage(TargettedImage ti)
        {
            ImageAvailable(this, new ImageAvailableEventArgs { image = ti });
        }
        void IReceiveEvents.receiveTextBox(TargettedTextBox ttb)
        {
            TextBoxAvailable(this, new TextBoxAvailableEventArgs { textBox = ttb });
        }
        void IReceiveEvents.receiveDirtyStroke(TargettedDirtyElement tde)
        {
            DirtyStrokeAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        void IReceiveEvents.receiveDirtyTextBox(TargettedDirtyElement tde)
        {
            DirtyTextBoxAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        void IReceiveEvents.receiveDirtyVideo(TargettedDirtyElement tde)
        {
            DirtyVideoAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        void IReceiveEvents.receiveDirtyImage(TargettedDirtyElement tde)
        {
            DirtyImageAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        void IReceiveEvents.receiveMoveDelta(TargettedMoveDelta delta)
        {
            MoveDeltaAvailable(this, new MoveDeltaAvailableEventArgs { moveDelta = delta });
        }
        void IReceiveEvents.receivePreParser(PreParser pp)
        {
            PreParserAvailable(this, new PreParserAvailableEventArgs { parser = pp });
        }
        public void receiveChat(TargettedTextBox ttb)
        {
            ChatAvailable(this, new ChatAvailableEventArgs { chat = ttb });
        }
        public void receiveLiveWindow(LiveWindowSetup lws)
        {
            LiveWindowAvailable(this, new LiveWindowAvailableEventArgs { livewindow = lws });
        }
        public void receiveDirtyLiveWindow(TargettedDirtyElement tde)
        {
            DirtyLiveWindowAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        public void receiveDirtyAutoShape(TargettedDirtyElement tde)
        {
            DirtyAutoShapeAvailable(this, new DirtyElementAvailableEventArgs { dirtyElement = tde });
        }
        public void receiveConversationDetails(ConversationDetails cd)
        {
            ConversationDetailsAvailable(this, new ConversationDetailsAvailableEventArgs { conversationDetails = cd });
        }
        public void statusChanged(bool isConnected, Credentials credentials)
        {
            StatusChanged(this, new StatusChangedEventArgs { isConnected = isConnected, credentials = credentials });
        }
        public void syncMoveRequested(int where)
        {
            SyncMoveRequested(this, new SyncMoveRequestedEventArgs{where = where});
        }

        public void teacherStatusRequest(string where, string who)
        {
            TeacherStatusRequest(this, new TeacherStatusRequestEventArgs
                                           {
                                               status = new TeacherStatus
                                                            {
                                                                Conversation = where,
                                                                Slide = where,
                                                                Teacher = who
                                                            }
                                           });
        }

        public void teacherStatusRecieved(TeacherStatus status)
        {
            TeacherStatusReceived(this, new TeacherStatusRequestEventArgs{status = status});
        }

        public void receieveQuizzes(PreParser finishedParser)
        {
            var quizzes = new List<QuizInfo>();
            finishedParser.quizzes.ForEach(q => quizzes.Add(new QuizInfo(q, finishedParser.quizAnswers.Where( a => a.id == q.Id).ToList())));
            QuizzesAvailable(this,  new QuizzesAvailableEventArgs{ quizzes = quizzes});
        }
        public void receieveAttachments(PreParser finishedParser)
        {
            var files = new List<TargettedFile>();
            finishedParser.files.ForEach(files.Add);
            AttachmentsAvailable(this,  new AttachmentsAvailableEventArgs{ attachments = files});
        }
      
        public void receieveQuiz(PreParser finishedParser, long id)
        {
            var quiz = finishedParser.quizzes.Where(q => q.Id == id).OrderByDescending(q => q.Created).First();
            var quizInfo = new QuizInfo(quiz, finishedParser.quizAnswers.Where(a => a.id == id).ToList());
            QuizAvailable(this, new QuizzesAvailableEventArgs{ quizzes = new List<QuizInfo>{quizInfo}});
        }

        public event MeTLLibEventHandlers.AttachmentsAvailableRequestEventHandler AttachmentsAvailable;
        public event MeTLLibEventHandlers.QuizzesAvailableRequestEventHandler QuizzesAvailable;
        public event MeTLLibEventHandlers.QuizAvailableRequestEventHandler QuizAvailable;
        public event MeTLLibEventHandlers.TeacherStatusReceivedEventHandler TeacherStatusReceived;

        #region Events

        public event MeTLLibEventHandlers.TeacherStatusRequestEventHandler TeacherStatusRequest;
        public event MeTLLibEventHandlers.PresenceAvailableEventHandler PresenceAvailable;
        public event MeTLLibEventHandlers.SubmissionAvailableEventHandler SubmissionAvailable;
        public event MeTLLibEventHandlers.SubmissionsAvailableEventHandler SubmissionsAvailable;
        public event MeTLLibEventHandlers.SlideCollectionUpdatedEventHandler SlideCollectionUpdated;
        public event MeTLLibEventHandlers.FileAvailableEventHandler FileAvailable;
        public event MeTLLibEventHandlers.StatusChangedEventHandler StatusChanged;
        public event MeTLLibEventHandlers.SyncMoveRequestedEventHandler SyncMoveRequested;
        public event MeTLLibEventHandlers.PreParserAvailableEventHandler PreParserAvailable;
        public event MeTLLibEventHandlers.StrokeAvailableEventHandler StrokeAvailable;
        public event MeTLLibEventHandlers.ImageAvailableEventHandler ImageAvailable;
        public event MeTLLibEventHandlers.TextBoxAvailableEventHandler TextBoxAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyTextBoxAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyImageAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyVideoAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyStrokeAvailable;
        public event MeTLLibEventHandlers.MoveDeltaAvailableEventHandler MoveDeltaAvailable;
        public event MeTLLibEventHandlers.LiveWindowAvailableEventHandler LiveWindowAvailable;
        public event MeTLLibEventHandlers.DiscoAvailableEventHandler DiscoAvailable;
        public event MeTLLibEventHandlers.QuizQuestionAvailableEventHandler QuizQuestionAvailable;
        public event MeTLLibEventHandlers.QuizAnswerAvailableEventHandler QuizAnswerAvailable;
        public event MeTLLibEventHandlers.ChatAvailableEventHandler ChatAvailable;
        public event MeTLLibEventHandlers.ConversationDetailsAvailableEventHandler ConversationDetailsAvailable;
        public event MeTLLibEventHandlers.CommandAvailableEventHandler CommandAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyLiveWindowAvailable;
        public event MeTLLibEventHandlers.DirtyElementAvailableEventHandler DirtyAutoShapeAvailable;
        #endregion Events
        #region VirtualEventSubscribers
        protected virtual void onPresenceAvailable(PresenceAvailableEventArgs e)
        { PresenceAvailable(this, e); }
        protected virtual void onSubmissionAvailable(SubmissionAvailableEventArgs e)
        { SubmissionAvailable(this, e); }
        protected virtual void onSlideCollectionUpdated(SlideCollectionUpdatedEventArgs e)
        { SlideCollectionUpdated(this, e); }
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
        protected virtual void onMoveDeltaAvailable(MoveDeltaAvailableEventArgs e)
        { MoveDeltaAvailable(this, e); }
        protected virtual void onImageAvailable(ImageAvailableEventArgs e)
        { ImageAvailable(this, e); }
        protected virtual void onTextBoxAvailable(TextBoxAvailableEventArgs e)
        { TextBoxAvailable(this, e); }
        protected virtual void onLiveWindowAvailable(LiveWindowAvailableEventArgs e)
        { LiveWindowAvailable(this, e); }
        protected virtual void onDiscoAvailable(DiscoAvailableEventArgs e)
        { DiscoAvailable(this, e); }
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
