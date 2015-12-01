using Awesomium.Core;
using MeTLLib.DataTypes;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Pages.Conversations.Models;
using SandRibbon.Providers;
using SandRibbon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SandRibbon.Pages
{
    public class UserSlideState
    {
        public bool isBanhammerActive { get; set; } = false;
    }
    public class UserConversationState
    {
        public Privacy privacy { get; set; } = Privacy.NotSet;
        public bool synched { get; set; } = false;
        public int teacherSlide { get; set; } = -1;
        public List<ContentVisibilityDefinition> contentVisibility { get; set; } = new List<ContentVisibilityDefinition>();
        public UndoHistory undoHistory { get; set; }
        public UserConversationState()
        {
            undoHistory = new UndoHistory(this);
        }
    }
    public class UserServerState
    {
        public WebSession authenticatedWebSession { get; set; }
        public OneNoteConfiguration OneNoteConfiguration { get; set; }
        public ThumbnailProvider thumbnailProvider { get; set; }
    }
    public class UserGlobalState
    {

    }
    public interface GlobalAwarePage
    {
        UserGlobalState getUserGlobalState();
        NavigationService getNavigationService();
    }
    public interface ServerAwarePage : GlobalAwarePage
    {
        NetworkController getNetworkController();
        UserServerState getUserServerState();
    }
    public interface ConversationAwarePage : ServerAwarePage
    {
        ConversationDetails getDetails();
        UserConversationState getUserConversationState();
    }
    public interface SlideAwarePage : ConversationAwarePage
    {
        Slide getSlide();
        UserSlideState getUserSlideState();
    }
}
