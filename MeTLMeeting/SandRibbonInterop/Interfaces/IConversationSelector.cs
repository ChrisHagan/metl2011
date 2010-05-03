using System.Collections.Generic;
using SandRibbonObjects;

namespace SandRibbonInterop.Interfaces
{
    public interface IConversationSelector
    {
        void List(IEnumerable<ConversationDetails> conversations);
    }
}
