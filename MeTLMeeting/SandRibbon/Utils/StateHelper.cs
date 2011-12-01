using SandRibbon.Providers;
using MeTLLib.DataTypes;

namespace SandRibbon.Utils
{
    public class StateHelper
    {
        public static bool mustBeInConversation()
        {
            var details = Globals.conversationDetails;
            if (details.IsEmpty) 
                return false;
            if (!details.isDeleted && !details.IsJidEqual(""))
                return true;

            return false;
        }
    }
}
