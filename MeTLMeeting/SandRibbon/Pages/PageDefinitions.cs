using Awesomium.Core;
using MeTLLib.DataTypes;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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
    public class GlobalAwarePage : Page
    {
        public UserGlobalState UserGlobalState { get; protected set; }
        public GlobalAwarePage(UserGlobalState _userGlobal)
        {
            UserGlobalState = _userGlobal;
        }
    }
    public class ServerAwarePage : GlobalAwarePage
    {
        public NetworkController NetworkController { get; protected set; }
        public UserServerState UserServerState { get; protected set; }
        public ServerAwarePage(UserGlobalState _userGlobal, UserServerState _userServer, NetworkController _controller) : base(_userGlobal)
        {
            UserServerState = _userServer;
            NetworkController = _controller;
        }
    }
    public class ConversationAwarePage : ServerAwarePage
    {
        public ConversationDetails Details { get; protected set; }
        public UserConversationState UserConversationState { get; protected set; }
        public ConversationAwarePage(UserGlobalState _userGlobal, UserServerState _userServer, UserConversationState _userConversation, NetworkController _controller, ConversationDetails _details) : base(_userGlobal, _userServer, _controller)
        {
            UserConversationState = _userConversation;
            Details = _details;
        }
    }
    public class SlideAwarePage : ConversationAwarePage
    {
        public Slide Slide { get; protected set; }
        public UserSlideState UserSlideState { get; protected set; }
        public SlideAwarePage(UserGlobalState _userGlobal, UserServerState _userServer, UserConversationState _userConversation, UserSlideState _userSlide, NetworkController _controller, ConversationDetails _details, Slide _slide) : base(_userGlobal, _userServer, _userConversation, _controller, _details)
        {
            UserSlideState = _userSlide;
            Slide = _slide;
        }
    }
}
