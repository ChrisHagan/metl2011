using MeTLLib.DataTypes;
using SandRibbon.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandRibbon.Pages
{
    public class UserSlideState
    {
        Privacy privacy { get; set; }
    }
    public class UserConversationState
    {
        bool synched { get; set; }
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
