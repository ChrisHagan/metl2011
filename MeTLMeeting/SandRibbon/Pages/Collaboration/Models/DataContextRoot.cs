using SandRibbon.Components;

namespace SandRibbon.Pages.Collaboration.Models
{
    public class DataContextRoot
    {
        public UserGlobalState UserGlobalState
        {
            get; set;
        }
        public UserServerState UserServerState
        {
            get; set;
        }
        public UserConversationState UserConversationState
        {
            get; set;
        }
        public ConversationState ConversationState
        {
            get; set;
        }        
        public NetworkController NetworkController
        {
            get; set;
        }        
    }
}
