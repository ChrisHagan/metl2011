using SandRibbon.Providers;
using MeTLLib.DataTypes;

namespace SandRibbon.Utils
{
    public class StateHelper
    {
        public static bool mustBeInConversation()
        {
            var details = Globals.conversationDetails;
            if (ConversationDetails.Empty.Equals(details)) return false;
            if(details.Subject != "Deleted" && details.Jid != "")
                    return true;
            return false;
        }
    }
}
