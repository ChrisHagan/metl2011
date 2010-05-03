using System;
using System.Collections.Generic;
using System.Linq;
using Divan;
using SandRibbonObjects;
using SandRibbon.Components;

namespace SandRibbon.Providers.Structure
{
    public class CouchConversationDetailsProvider : IConversationDetailsProvider
    {
        private const int slideIncrementer = 200;
        private CouchServer server = getServer();
        private static CouchServer getServer()
        {
            return new CouchServer("drawkward.adm.monash.edu");
        }
        public bool Ping()
        {
            try
            {
                var db = getServer().GetDatabase("conversation");
                return true;
            }
            catch (CouchException e)
            {
                return false;
            }
        }
        public ConversationDetails DetailsOf(string conversationTitle)
        {
            try
            {
                var db = server.GetDatabase("conversation");
                var details = db.GetAllDocuments<ConversationDetails>().Where(cd => cd.Title == conversationTitle);
                if (details.Count() > 0)
                {
                    return (from d in details orderby d.Rev descending select d).First();
                }
                return (ConversationDetails)db.CreateDocument(new ConversationDetails { Author = "Happenstance", Created = DateTime.Now, Slides = Enumerable.Range(1, 5).Select(i => new Slide { author = "Mishap", id = i, index = i - 1 }).ToList(), Title = conversationTitle });
            }
            catch (CouchException e)
            {
                ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return new ConversationDetails();
            }
        }
        public ConversationDetails AppendSlide(string conversation)
        {
            var currentConversation = DetailsOf(conversation);
            var slideId = currentConversation.Slides[0].id + currentConversation.Slides.Count + 1;
            currentConversation.Slides.Add(new Slide
                                               {
                                                   author = currentConversation.Author,
                                                   id = slideId,
                                                   index = currentConversation.Slides.Count
                                               });
            Update(currentConversation);
            return currentConversation;
        }
        public ConversationDetails Update(ConversationDetails details)
        {
            try
            {
                return (ConversationDetails)server.GetDatabase("conversation").SaveDocument(details);
            }
            catch (CouchException e)
            {
                ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return details;
            }
        }
        public IEnumerable<ConversationDetails> ListConversations()
        {
            try
            {
                var db = server.GetDatabase("conversation");
                var result = db.GetAllDocuments<ConversationDetails>().ToList();
                foreach (var conversation in result)
                    if (conversation.Tag == null)
                        conversation.Tag = "unTagged";
                return result;
            }
            catch (CouchException e)
            {
                ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return new List<ConversationDetails>();
            }
        }
        public ConversationDetails Create(string name)
        {
            try
            {
                var conversationDetails = new ConversationDetails { Title = name };
                return Create(conversationDetails);
            }
            catch (CouchException e)
            {
                ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return new ConversationDetails();
            }
        }
        public ConversationDetails Create(ConversationDetails details)
        {
            var info = GetApplicationLevelInformation();
            try
            {
                var slideId = info.currentId;
                if (details.Tag == null) details.Tag = "unTagged";
                info.currentId = info.currentId + slideIncrementer;
                details.Created = DateTime.Now;
                details.Slides = new List<Slide> { new Slide { id = slideId, index = 0 } };
                var newConversation = (ConversationDetails)server.GetDatabase("conversation").SaveDocument(details);
                UpdateApplicationLevelInformation(info);
                return newConversation;
            }
            catch (CouchException e)
            {
                ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return details;
            }
        }
        public ApplicationLevelInformation GetApplicationLevelInformation()
        {
            try
            {
                var db = server.GetDatabase("application");
                var info = db.GetAllDocuments<ApplicationLevelInformation>();
                if (info.Count() == 0)
                    return (ApplicationLevelInformation)db.CreateDocument(new ApplicationLevelInformation { currentId = 0 });
                return (from i in info orderby i.Rev descending select i).First();
            }
            catch (CouchException e)
            {
                ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return new ApplicationLevelInformation();
            }
        }
        public ApplicationLevelInformation UpdateApplicationLevelInformation(ApplicationLevelInformation info)
        {
            try
            {
                return (ApplicationLevelInformation)server.GetDatabase("application").SaveDocument(info);
            }
            catch (CouchException e)
            {
                ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return new ApplicationLevelInformation();
            }
        }
    }
}