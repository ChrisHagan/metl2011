using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using MeTLLib.DataTypes;
using SandRibbon.Components.Utility;

//using SandRibbonObjects;

namespace SandRibbon.Providers
{
    public class RecentConversationProvider
    {
        public static MeTLLib.ClientConnection ConversationProvider = MeTLLib.ClientFactory.Connection();
        public static readonly string RECENT_DOCUMENTS = "recentDocuments.xml";
        
        public static IEnumerable<ConversationDetails> loadRecentConversations()
        {
            if (File.Exists(RECENT_DOCUMENTS))
            {
                var recentDocs = XDocument.Load(RECENT_DOCUMENTS);
                
                System.Globalization.CultureInfo current = System.Globalization.CultureInfo.GetCultureInfo("en-AU");
                System.Threading.Thread.CurrentThread.CurrentCulture = current;
                
                var recentConversations = recentDocs.Descendants("conversation").Select(
                    conversation => new ConversationDetails(
                                conversation.Attribute("title").Value,
                                conversation.Attribute("jid").Value,
                                conversation.Attribute("author").Value,
                                new List<Slide>(),
                                new Permissions("", false, false, false),
                                conversation.Attribute("subject") == null ? String.Empty : conversation.Attribute("subject").Value,
                                new DateTime(),
                                SandRibbonObjects.DateTimeFactory.Parse(conversation.Attribute("lastAccessTime").Value))
                        )/*.Where(conv => !ConversationProvider.DetailsOf(conv.Jid).isDeleted)*/.ToList();

                return recentConversations.Count > 0 ? recentConversations.OrderByDescending(c => c.LastAccessed).ToList() : new List<ConversationDetails>();
            }
            return new List<ConversationDetails>();
        }
        public static void addRecentConversation(ConversationDetails document, String me)
        {
            try
            {
                if (document.Title == null) return;
                if (!File.Exists(RECENT_DOCUMENTS))
                    new XDocument(new XElement("recentConversations")).Save(RECENT_DOCUMENTS);
                var recentDocs = XDocument.Load(RECENT_DOCUMENTS);
                var referencesToThisConversation = recentDocs.Descendants("conversation")
                    .Where(c => c.Attribute("title").Value == document.Title);
                switch (referencesToThisConversation.Count())
                {
                    case 0:
                        recentDocs.Root.Add(new XElement("conversation",
                            new XAttribute("title", document.Title),
                            new XAttribute("author", document.Author),
                            new XAttribute("jid", document.Jid),
                            new XAttribute("subject", document.Subject),
                            new XAttribute("lastAccessTime", SandRibbonObjects.DateTimeFactory.Now().ToString())));
                        break;
                    case 1:
                        referencesToThisConversation.Single().SetAttributeValue("lastAccessTime", SandRibbonObjects.DateTimeFactory.Now().ToString());
                        break;
                    default:
                        MeTLMessage.Warning("Too many instances of " + document.Title + " in recent history.  Not listing.");
                        break;
                }
                recentDocs.Save(RECENT_DOCUMENTS);
            }
            catch (IOException)
            {
            }
        }
        public static string DisplayNameFor(ConversationDetails conversation)
        {
            if (conversation.Title == null) return "Untitled by Unknown";
            return String.Format("{0} by {1}",
                conversation.Title.Length > 20 ? conversation.Title.Substring(0, 20) + "..." : conversation.Title,
                conversation.Author);
        }
    }
}
