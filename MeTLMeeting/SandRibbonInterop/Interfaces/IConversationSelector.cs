using System.Collections.Generic;
using MeTLLib.DataTypes;
//using SandRibbonObjects;

namespace SandRibbonInterop.Interfaces
{
    public interface IConversationSelector
    {
        void List(IEnumerable<MeTLLib.DataTypes.ConversationDetails> conversations);
    }
}
