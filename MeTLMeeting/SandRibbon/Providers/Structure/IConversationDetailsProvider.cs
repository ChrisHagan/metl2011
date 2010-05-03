using System.Collections.Generic;
using SandRibbonObjects;

namespace SandRibbon.Providers.Structure
{
    public interface IConversationDetailsProvider
    {
        ConversationDetails DetailsOf(string jid);
        ConversationDetails Update(ConversationDetails details);
        IEnumerable<ConversationDetails> ListConversations();
        ConversationDetails Create(ConversationDetails details);
        ConversationDetails AppendSlide(string title);
        ConversationDetails AppendSlideAfter(int slideId, string title);
        ConversationDetails AppendSlideAfter(int slideId, string title, Slide.TYPE type);
        ApplicationLevelInformation GetApplicationLevelInformation();
    }
}
