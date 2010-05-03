using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SandRibbonObjects;

namespace SandRibbon.Providers.Structure
{
    public class StubConversationDetailsProvider : IConversationDetailsProvider
    {
        public ConversationDetails DetailsOf(string conversationTitle)
        {
            var details = new ConversationDetails{Title = conversationTitle, Author="anAuthor"};
            switch (conversationTitle)
            {
                case "Sample":
                        details.Slides = Enumerable.Range(1, 20).Select(i=>new Slide{id=i}).ToList();
                        break;
                case "First":
                        details.Slides = Enumerable.Range(21, 40).Select(i=>new Slide{id=i}).ToList();
                        break;
                case "Second":
                        details.Slides = Enumerable.Range(41, 60).Select(i=>new Slide{id=i}).ToList();
                        break;
                case "Third":
                        details.Slides = Enumerable.Range(61, 80).Select(i=>new Slide{id=i}).ToList();
                        break;
                default:
                        details.Slides = new[] { 0 }.Select(i=>new Slide{id=i}).ToList();
                        details.Title = "Not found";
                        break;
            }
            return details;
        }
        public ConversationDetails Update(ConversationDetails details)
        {
            return details;
        }
        public IEnumerable<ConversationDetails> ListConversations()
        {
            return new List<ConversationDetails>();
        }
        public ConversationDetails Create(ConversationDetails details)
        {
            return details;
        }
        public ConversationDetails Create(string title)
        {
            return new ConversationDetails();
        }
        public ApplicationLevelInformation GetApplicationLevelInformation()
        {
            return new ApplicationLevelInformation
                       {
                           currentId = 0
                       };
        }
        public ApplicationLevelInformation UpdateApplicationLevelInformation(ApplicationLevelInformation info)
        {
            return info;
        }
    }
}
