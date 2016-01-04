using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;
using MeTLLib.Utilities;
//using Ninject;

namespace MeTLLib.Providers.Structure
{
    class FileConversationDetailsProvider : HttpResourceProvider, IConversationDetailsProvider
    {
        protected MetlConfiguration server;
        protected JabberWireFactory jabberWireFactory;
        protected JabberWire _wire;
        protected static object wireLock = new object();
        private JabberWire wire
        {
            get 
            {
                using (DdMonitor.Lock(wireLock))
                {
                    if (_wire == null)
                        _wire = jabberWireFactory.wire();
                    return _wire;
                }
            }
        }
        private IResourceUploader resourceUploader;
        public Credentials credentials { get; protected set; }
        public FileConversationDetailsProvider(MetlConfiguration _server,IWebClientFactory factory, IResourceUploader uploader,Credentials creds,IAuditor auditor,JabberWireFactory _jabberWireFactory)
            : base(factory,auditor)
        {
            server = _server;
            resourceUploader = uploader;
            credentials = creds;
            jabberWireFactory = _jabberWireFactory;
        }
        public bool isAccessibleToMe(string jid)
        {
            var myGroups = credentials.authorizedGroups.Select(g => g.groupKey.ToLower());
            var details = DetailsOf(jid);
            return myGroups.Contains(details.Subject.ToLower());
        }
        public virtual ConversationDetails DetailsOf(string conversationJid)
        {
            ConversationDetails result = ConversationDetails.Empty;
            if (String.IsNullOrEmpty(conversationJid))
            {
                Trace.TraceError("CRASH: Fixed: Argument cannot be null or empty - Reconnecting error that happens all the time");
                return result;
            }
            try
            {
                var url = server.conversationDetails(conversationJid);
                Console.WriteLine("Details of: {0}", url);
                result = ConversationDetails.ReadXml(XElement.Parse(secureGetBytesAsString(url)));
            }
            catch (UriFormatException e)
            {
                Trace.TraceError("CRASH: Could not create valid Uri for DetailsOf, using conversationJid: {0}: {1}", conversationJid, e.Message);
            }
            catch (XmlException e)
            {
                Trace.TraceError("CRASH: Could not parse retrieved details of {0}: {1}", conversationJid, e.Message);
            }
            catch (WebException e)
            {
                Trace.TraceError("CRASH: FileConversationDetailsProvider::DetailsOf: {0}", e.Message);
            }
            catch (Exception e)
            {
                Trace.TraceError("CRASH: Unknown Exception in retrieving the conversation details: {0}", e.Message);
            }
            return result;
        }
        public ConversationDetails AppendSlideAfter(int currentSlide, string jid)
        {
            return AppendSlideAfter(currentSlide, jid, Slide.TYPE.SLIDE);
        }
        public ConversationDetails AppendSlideAfter(int currentSlideId, string jid, Slide.TYPE type)
        {
            ConversationDetails result = ConversationDetails.Empty;
            if (String.IsNullOrEmpty(jid))
            {
                Trace.TraceError("CRASH: Fixed: Argument cannot be null or empty - Reconnecting error that happens all the time");
                return result;
            }
            try
            {
                var url = server.addSlideAtIndex(jid,currentSlideId);
                Console.WriteLine("Details of: {0}", url);
                result = ConversationDetails.ReadXml(XElement.Parse(secureGetBytesAsString(url)));
                wire.SendDirtyConversationDetails(jid);
            }
            catch (UriFormatException e)
            {
                Trace.TraceError("CRASH: Could not create valid Uri for DetailsOf, using conversationJid: {0}: {1}", jid, e.Message);
            }
            catch (XmlException e)
            {
                Trace.TraceError("CRASH: Could not parse retrieved details of {0}: {1}", jid, e.Message);
            }
            catch (WebException e)
            {
                Trace.TraceError("CRASH: FileConversationDetailsProvider::DetailsOf: {0}", e.Message);
            }
            catch (Exception e)
            {
                Trace.TraceError("CRASH: Unknown Exception in retrieving the conversation details: {0}", e.Message);
            }
            return result;
        }
        public ConversationDetails AppendSlide(string jid)
        {
            var details = DetailsOf(jid);
            var lastSlide = details.Slides.OrderBy(s => s.index).Select(s => s.id).First();
            return AppendSlideAfter(lastSlide, jid);
        }
        private bool DetailsAreAccurate(ConversationDetails details)
        {
            var newDetails = DetailsOf(details.Jid);
            return details.ValueEquals(newDetails);
        }
        public ConversationDetails Update(ConversationDetails details)
        {
            var url = server.updateConversation(details.Jid);
            var response = securePutData(url, details.GetBytes());
            var responseDetails = ConversationDetails.ReadXml(XElement.Parse(response));
            wire.SendDirtyConversationDetails(details.Jid);
            if (!DetailsAreAccurate(responseDetails))
                Trace.TraceInformation("CRASH: ConversationDetails not successfully uploaded");
            return details;
        }
        class UniqueConversationComparator : IEqualityComparer<ConversationDetails>
        {
            public bool Equals(ConversationDetails x, ConversationDetails y)
            {
                return x.Title.Equals(y.Title);
            }
            public int GetHashCode(ConversationDetails obj)
            {
                return obj.Title.GetHashCode();
            }
        }
        private object cacheLock = new object();
        private class LastModifiedConversation
        {
            public string Jid;
            public string lastModified;
            public LastModifiedConversation(string jid, string modified)
            {
                Jid = jid;
                lastModified = modified;
            }
        }
        public IEnumerable<SearchConversationDetails> ConversationsFor(String query, int maxResults)
        {
            try
            {
                var uri = server.conversationQuery(query);
                Console.WriteLine("ConversationsFor: {0}", uri);
                var data = insecureGetString(uri);
                var results = XElement.Parse(data).Descendants("conversation").Select(SearchConversationDetails.ReadXML).ToList();
                var deletedConversationJids = results.Where(c => c.isDeleted).Select(c => c.Jid);
                var conversations = results.OrderByDescending(c => c.relevance)
                    .ThenByDescending(c => c.LastModified)
                    .ThenByDescending(c => c.Created).ToList();
                var jids = conversations.Select(c => c.Jid).Distinct().ToList();
                var filtered = jids.Where(jid => !deletedConversationJids.Contains(jid)).Select(jid => conversations.First(c => c.Jid == jid)).Take(maxResults).ToList();
                return filtered;
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("FileConversationDetailsProvider:ConversationsFor {0}",e.Message));
                return new List<SearchConversationDetails>();
            }
        }
        public ConversationDetails Create(ConversationDetails details)
        {
            try
            {
                var uri = server.createConversation(details.Title.ToString());
                Console.WriteLine("createConvUri: {0}", uri);
                var data = insecureGetString(uri);
                return SearchConversationDetails.ReadXML(XElement.Parse(data));
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("FileConversationDetailsProvider:CreateConversation {0}", e.Message));
                return ConversationDetails.Empty;
            }
        }
        public ConversationDetails DuplicateSlide(ConversationDetails conversation, Slide slide)
        {
            try
            {
                var uri = server.duplicateSlide(slide.id, conversation.Jid.ToString());
                Console.WriteLine("DuplicateSlide: {0}", uri);
                var data = insecureGetString(uri);
                return SearchConversationDetails.ReadXML(XElement.Parse(data));
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("FileConversationDetailsProvider:DuplicateSlide {0}", e.Message));
                return ConversationDetails.Empty;
            }
        }
        public ConversationDetails DuplicateConversation(ConversationDetails conversation)
        {
            try
            {
                var uri = server.duplicateConversation(conversation.Jid.ToString());
                Console.WriteLine("DuplicateConversation: {0}", uri);
                var data = insecureGetString(uri);
                return SearchConversationDetails.ReadXML(XElement.Parse(data));
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("FileConversationDetailsProvider:DuplicateConversation {0}", e.Message));
                return ConversationDetails.Empty;
            }
        }
    }
}
