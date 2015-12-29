using Awesomium.Core;
using MeTLLib.DataTypes;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Pages.Conversations.Models;
using SandRibbon.Providers;
using SandRibbon.Utils;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System;
using MeTLLib.Providers.Connection;
using System.Threading;
using System.Diagnostics;

namespace SandRibbon.Pages
{
    public class ServerAware<T>
    {
        public T Value { get; protected set; }
        public NetworkController NetworkController { get; protected set; }
        public ServerAware(NetworkController nc, T value)
        {
            this.NetworkController = nc;
            this.Value = value;
        }
    }
    public class ConversationAware<T> : ServerAware<T>
    {
        public ConversationDetails ConversationDetails { get; protected set; }
        public ConversationAware(NetworkController nc, ConversationDetails cd, T value) : base(nc, value)
        {
            ConversationDetails = cd;
        }
    }
    public class SlideAware<T> : ConversationAware<T>
    {
        public Slide Slide { get; protected set; }
        public SlideAware(NetworkController nc, ConversationDetails cd, Slide sl, T value) : base(nc, cd, value)
        {
            Slide = sl;
        }
    }

    public class UserSlideState : DependencyObject
    {
        public bool BanhammerActive
        {
            get { return (bool)GetValue(BanhammerActiveProperty); }
            set { SetValue(BanhammerActiveProperty, value); }
        }
        public static readonly DependencyProperty BanhammerActiveProperty =
            DependencyProperty.Register("BanhammerActive", typeof(bool), typeof(UserSlideState), new PropertyMetadata(false));
    }
    public class ConversationState : DependencyObject
    {
        public QuizData QuizData
        {
            get { return (QuizData)GetValue(QuizDataProperty); }
            set { SetValue(QuizDataProperty, value); }
        }
        public static readonly DependencyProperty QuizDataProperty =
            DependencyProperty.Register("QuizData", typeof(QuizData), typeof(ConversationState), new PropertyMetadata(new QuizData()));


    }
    public class UserConversationState : DependencyObject
    {
        public bool IsAuthor
        {
            get { return (bool)GetValue(IsAuthorProperty); }
            set { SetValue(IsAuthorProperty, value); }
        }
        public static readonly DependencyProperty IsAuthorProperty =
            DependencyProperty.Register("IsAuthor", typeof(bool), typeof(UserConversationState), new PropertyMetadata(false));

        public Privacy Privacy
        {
            get { return (Privacy)GetValue(PrivacyProperty); }
            set { SetValue(PrivacyProperty, value); }
        }
        public static readonly DependencyProperty PrivacyProperty =
            DependencyProperty.Register("Privacy", typeof(Privacy), typeof(UserConversationState), new PropertyMetadata(Privacy.NotSet));

        public bool Synched
        {
            get { return (bool)GetValue(SynchedProperty); }
            set { SetValue(SynchedProperty, value); }
        }
        public static readonly DependencyProperty SynchedProperty =
            DependencyProperty.Register("Synched", typeof(bool), typeof(UserConversationState), new PropertyMetadata(false));

        public int TeacherSlide
        {
            get { return (int)GetValue(TeacherSlideProperty); }
            set { SetValue(TeacherSlideProperty, value); }
        }
        public static readonly DependencyProperty TeacherSlideProperty =
            DependencyProperty.Register("TeacherSlide", typeof(int), typeof(UserConversationState), new PropertyMetadata(-1));

        public List<ContentVisibilityDefinition> ContentVisibility
        {
            get { return (List<ContentVisibilityDefinition>)GetValue(ContentVisibilityProperty); }
            set { SetValue(ContentVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ContentVisibilityProperty =
            DependencyProperty.Register("ContentVisibility", typeof(List<ContentVisibilityDefinition>), typeof(UserConversationState), new PropertyMetadata(ContentFilterVisibility.defaultVisibilities));

        public UndoHistory UndoHistory
        {
            get { return (UndoHistory)GetValue(UndoHistoryProperty); }
            set { SetValue(UndoHistoryProperty, value); }
        }
        public static readonly DependencyProperty UndoHistoryProperty =
            DependencyProperty.Register("UndoHistory", typeof(UndoHistory), typeof(UserConversationState), new PropertyMetadata(null));

        public UserConversationState()
        {
            UndoHistory = new UndoHistory(this);
        }
    }
    public class UserServerState : DependencyObject
    {
        public WebSession AuthenticatedWebSession { get; set; }
        public OneNoteConfiguration OneNoteConfiguration { get; set; }

        public ThumbnailProvider ThumbnailProvider { get; set; }
    }
    public class UserGlobalState
    {

    }
    public class GlobalAwarePage : Page
    {
        public UserGlobalState UserGlobalState { get; set; }
        public GlobalAwarePage() : base()
        {
            if (!Resources.Contains(SystemParameters.VerticalScrollBarWidth)) Resources.Add(SystemParameters.VerticalScrollBarWidth, 50);
            if (!Resources.Contains(SystemParameters.HorizontalScrollBarHeight)) Resources.Add(SystemParameters.HorizontalScrollBarHeight, 50);
        }
    }
    public class ServerAwarePage : GlobalAwarePage
    {
        public ServerAwarePage() { }
        public NetworkController NetworkController { get; set; }
        public UserServerState UserServerState { get; set; }
    }
    public class ConversationAwarePage : ServerAwarePage
    {
        public ConversationAwarePage() { }
        public ConversationDetails ConversationDetails { get; set; }
        public UserConversationState UserConversationState { get; set; }
        public ConversationState ConversationState { get; set; }
        public bool IsAuthor
        {
            get
            {
                return ConversationDetails.isAuthor(NetworkController.credentials.name);
            }
        }
    }
    public class SlideAwarePage : ConversationAwarePage
    {
        public SlideAwarePage() { }
        public Slide Slide { get; set; }
        public UserSlideState UserSlideState { get; set; }

        public delegate void PreParserHandler(PreParser p);
        public event PreParserHandler PreParserAvailable;

        public void PublishPreParser(PreParser p) {
            PreParserAvailable?.Invoke(p);
        }
        private CancellationTokenSource canceller;
        internal void MoveToSlide(Slide selected)
        {
            canceller?.Cancel();
            canceller = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(delegate
            {
                var previousSlide = Slide;
                Slide = selected;
                if (previousSlide != selected)
                {
                    NetworkController.client.LeaveRoom(previousSlide.id.ToString());
                    NetworkController.client.LeaveRoom(previousSlide.id.ToString() + NetworkController.credentials.name);
                }
                var cancellationToken = canceller.Token;
                if (!cancellationToken.IsCancellationRequested)
                {
                    NetworkController.client.JoinRoom(Slide.id.ToString());
                    NetworkController.client.JoinRoom(Slide.id.ToString() + NetworkController.credentials.name);
                }
                else {
                    Trace.TraceInformation("Cancelling movement to slide {0} at xmpp", selected.id);
                }
                if (!cancellationToken.IsCancellationRequested)
                {
                    var p = NetworkController.client.historyProvider;
                    p.Retrieve<PreParser>(delegate { }, delegate { }, PublishPreParser, Slide.id.ToString());
                    p.Retrieve<PreParser>(delegate { }, delegate { }, PublishPreParser, String.Format("{0}/{1}", NetworkController.credentials.name, Slide.id));
                }
                else {
                    Trace.TraceInformation("Cancelling movement to slide {0} at history", selected.id);
                }
            });
            Commands.SendSyncMove.Execute(selected.id);
            Commands.MovingTo.Execute(selected);
        }
    }
}
