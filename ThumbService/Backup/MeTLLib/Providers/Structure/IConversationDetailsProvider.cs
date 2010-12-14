using System.Collections.Generic;
using MeTLLib.DataTypes;

namespace MeTLLib.Providers.Structure
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
        void ReceiveDirtyConversationDetails(string jid);
        bool isAccessibleToMe(string jid);
    }
}
