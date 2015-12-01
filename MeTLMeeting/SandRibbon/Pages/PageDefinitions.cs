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
        Privacy privacy { get; set; }
        bool isBanhammerActive { get; set; }
    }
    public class UserConversationState
    {
        bool synched { get; set; }
        int teacherSlide { get; set; }
        List<ContentVisibilityDefinition> contentVisibility { get; set; }
    }
    public class UserServerState
    {
    }
    public class UserGlobalState
    {

    }
    public interface GlobalAwarePage
    {
        UserGlobalState getUserGlobalState();
        NavigationService getNavigationService();
    }
    public interface ServerAwarePage
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
