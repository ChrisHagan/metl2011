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
using System.Windows.Navigation;

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
            DependencyProperty.Register("ContentVisibility", typeof(List<ContentVisibilityDefinition>), typeof(UserConversationState), new PropertyMetadata(new List<ConversationState>()));

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
        public WebSession AuthenticatedWebSession
        {
            get { return (WebSession)GetValue(AuthenticatedWebSessionProperty); }
            set { SetValue(AuthenticatedWebSessionProperty, value); }
        }        
        public static readonly DependencyProperty AuthenticatedWebSessionProperty =
            DependencyProperty.Register("AuthenticatedWebSession", typeof(WebSession), typeof(UserServerState), new PropertyMetadata(null));
        
        public OneNoteConfiguration OneNoteConfiguration
        {
            get { return (OneNoteConfiguration)GetValue(OneNoteConfigurationProperty); }
            set { SetValue(OneNoteConfigurationProperty, value); }
        }
        public static readonly DependencyProperty OneNoteConfigurationProperty =
            DependencyProperty.Register("OneNoteConfiguration", typeof(OneNoteConfiguration), typeof(UserServerState), new PropertyMetadata(new OneNoteConfiguration()));
        
        public ThumbnailProvider ThumbnailProvider
        {
            get { return (ThumbnailProvider)GetValue(ThumbnailProviderProperty); }
            set { SetValue(ThumbnailProviderProperty, value); }
        }
        public static readonly DependencyProperty ThumbnailProviderProperty =
            DependencyProperty.Register("ThumbnailProvider", typeof(ThumbnailProvider), typeof(UserServerState), new PropertyMetadata(null));        
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
        public ConversationDetails ConversationDetails { get; set; }        
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
