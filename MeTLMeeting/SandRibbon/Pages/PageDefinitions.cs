using Awesomium.Core;
using MeTLLib.DataTypes;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.Conversations.Models;
using SandRibbon.Providers;
using SandRibbon.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System;
using MeTLLib;

namespace SandRibbon.Pages
{
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
        private NetworkController networkController;        
        public ConversationState(ConversationDetails requestedConversation, NetworkController networkController)
        {         
            this.networkController = networkController;
            Bind(networkController);
            Jid = requestedConversation.Jid;
            Events_ConversationDetailsAvailable(this, new ConversationDetailsAvailableEventArgs { conversationDetails = requestedConversation });
        }

        internal void Broadcast()
        {/*If you don't see something going out to participants, this is where the details were reconstructed from bound state*/
            var details = new ConversationDetails {
                Jid = Jid,
                Title = Title,
                Author = Author,
                Slides = Slides,
                blacklist = Blacklist,
                Permissions = new Permissions("",true,StudentsCanPublish,!StudentsCanMoveFreely)
            };
            networkController.client.UpdateConversationDetails(details);
        }

        public void Bind(NetworkController source)
        {/*If you don't see something coming into the application from participants, this is where the client properties were converters
            to DependencyProperties.*/
            this.networkController = source;
            source.client.events.ConversationDetailsAvailable -= Events_ConversationDetailsAvailable;
            source.client.events.ConversationDetailsAvailable += Events_ConversationDetailsAvailable;            
        }        

        private void Events_ConversationDetailsAvailable(object sender, MeTLLib.ConversationDetailsAvailableEventArgs e)
        {
            var details = e.conversationDetails;
            if (details.Jid == Jid)
            {
                Title = details.Title;
                StudentsCanMoveFreely = !details.Permissions.NavigationLocked;
                StudentsCanPublish = details.Permissions.studentCanPublish;
                Slides = details.Slides.OrderBy(s => s.index).ToList();
                var summaries = Slides.Select(s => new LocatedActivity("",s,0,0)).ToArray();
                foreach (var slide in Slides.AsParallel()) {
                    Console.WriteLine(slide.index);
                    var desc = networkController.client.historyProvider.Describe(slide.id);
                    summaries[slide.index] = new LocatedActivity("",slide, desc.stanzaCount, desc.voices);
                    SlideSummaries = summaries.ToList();
                }
            }
        }

        public List<string> Blacklist
        {
            get { return (List<string>)GetValue(BlacklistProperty); }
            set { SetValue(BlacklistProperty, value); Broadcast(); }
        }        
        public static readonly DependencyProperty BlacklistProperty =
            DependencyProperty.Register("Blacklist", typeof(List<string>), typeof(ConversationState), new PropertyMetadata(new List<string>()));

        public void SetConversation(ConversationDetails conversationDetails)
        {
            Jid = conversationDetails.Jid;
            Events_ConversationDetailsAvailable(this, new MeTLLib.ConversationDetailsAvailableEventArgs { conversationDetails = conversationDetails });
        }

        public delegate void LocationAnalysis();
        public event LocationAnalysis LocationAnalyzed;
        
        public List<LocatedActivity> SlideSummaries
        {
            get { return (List<LocatedActivity>)GetValue(SlideSummariesProperty); }
            set { SetValue(SlideSummariesProperty, value); }
        }        
        public static readonly DependencyProperty SlideSummariesProperty =
            DependencyProperty.Register("SlideSummaries", typeof(List<LocatedActivity>), typeof(ConversationState), new PropertyMetadata(new List<LocatedActivity>()));

        public string Jid
        {
            get { return (string)GetValue(JidProperty); }
            set { SetValue(JidProperty, value); }
        }
        public static readonly DependencyProperty JidProperty =
            DependencyProperty.Register("Jid", typeof(string), typeof(ConversationState), new PropertyMetadata(""));

        public QuizData QuizData
        {
            get { return (QuizData)GetValue(QuizDataProperty); }
            set { SetValue(QuizDataProperty, value); }
        }
        public static readonly DependencyProperty QuizDataProperty =
            DependencyProperty.Register("QuizData", typeof(QuizData), typeof(ConversationState), new PropertyMetadata(new QuizData()));
        
        public string Author
        {
            get { return (string)GetValue(AuthorProperty); }
            set { SetValue(AuthorProperty, value); }
        }        
        public static readonly DependencyProperty AuthorProperty =
            DependencyProperty.Register("Author", typeof(string), typeof(ConversationState), new PropertyMetadata(""));        

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ConversationState), new PropertyMetadata(""));

        public bool StudentsCanPublish
        {
            get { return (bool)GetValue(StudentsCanPublishProperty); }
            set { SetValue(StudentsCanPublishProperty, value); }
        }
        public static readonly DependencyProperty StudentsCanPublishProperty =
            DependencyProperty.Register("StudentsCanPublish", typeof(bool), typeof(ConversationState), new PropertyMetadata(false));

        public bool StudentsCanMoveFreely
        {
            get { return (bool)GetValue(StudentsCanMoveFreelyProperty); }
            set { SetValue(StudentsCanMoveFreelyProperty, value); }
        }
        public static readonly DependencyProperty StudentsCanMoveFreelyProperty =
            DependencyProperty.Register("StudentsCanMoveFreely", typeof(bool), typeof(ConversationState), new PropertyMetadata(false));

        public List<Slide> Slides
        {
            get { return (List<Slide>)GetValue(SlidesProperty); }
            set { SetValue(SlidesProperty, value); }
        }

        public bool IsAuthor {
            get {
                return networkController.credentials.name.ToLower() == Author.ToLower();
            }
        }

        public bool AnyoneCanPublish {
            get {
                return IsAuthor || StudentsCanPublish;
            }
        }
       
        public bool StudentsCanDuplicate
        {
            get { return (bool)GetValue(StudentsCanDuplicateProperty); }
            set { SetValue(StudentsCanDuplicateProperty, value); }
        }

        public bool CanDuplicate {
            get {
                return IsAuthor || StudentsCanDuplicate;
            }
        }

        internal void DuplicateSlide()
        {
        }

        public static readonly DependencyProperty StudentsCanDuplicateProperty =
            DependencyProperty.Register("StudentsCanDuplicate", typeof(bool), typeof(ConversationState), new PropertyMetadata(false));
        
        public static readonly DependencyProperty SlidesProperty =
            DependencyProperty.Register("Slides", typeof(List<Slide>), typeof(ConversationState), new PropertyMetadata(new List<Slide>()));

        
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
        public GlobalAwarePage() { }
        public UserGlobalState UserGlobalState { get; set; }
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
        public UserConversationState UserConversationState { get; set; }
        public ConversationState ConversationState { get; set; }
    }
    public class SlideAwarePage : ConversationAwarePage
    {
        public SlideAwarePage() { }
        public Slide Slide { get; set; }
        public UserSlideState UserSlideState { get; set; }
    }
}
