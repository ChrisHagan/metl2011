using Awesomium.Core;
using MeTLLib.DataTypes;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
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
        public bool isBanhammerActive { get; set; }
    }
    public class UserConversationState
    {
        public Privacy privacy { get; set; }
        public bool synched { get; set; }
        public int teacherSlide { get; set; }
        public List<ContentVisibilityDefinition> contentVisibility { get; set; }
    }
    public class UserServerState
    {
        public WebSession authenticatedWebSession { get; set; }
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
