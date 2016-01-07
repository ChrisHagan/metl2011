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
        public FileConversationDetailsProvider(MetlConfiguration _server,IWebClientFactory factory, IResourceUploader uploader,Credentials creds,IAuditor _auditor,JabberWireFactory _jabberWireFactory)
            : base(factory,_auditor)
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
                _auditor.log("DetailsOf: null conversationJid", "FileConversationDetailsProvider");
                return result;
            }
            try
            {
                var url = server.conversationDetails(conversationJid);
                _auditor.log(String.Format("Details of: {0}", url), "FileConversationDetailsProvider");
                result = ConversationDetails.ReadXml(XElement.Parse(secureGetBytesAsString(url)));
            }
            catch (UriFormatException e)
            {
                _auditor.error("DetailsOf: "+conversationJid, "FileConversationDetailsProvider", e);
            }
            catch (XmlException e)
            {
                _auditor.error("DetailsOf: " + conversationJid, "FileConversationDetailsProvider", e);
            }
            catch (WebException e)
            {
                _auditor.error("DetailsOf: " + conversationJid, "FileConversationDetailsProvider", e);
            }
            catch (Exception e)
            {
                _auditor.error("DetailsOf: " + conversationJid, "FileConversationDetailsProvider", e);
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
                _auditor.log("AppendSlideAfter: null jid", "FileConversationDetailsProvider");
                return result;
            }
            try
            {
                var url = server.addSlideAtIndex(jid,currentSlideId);
                _auditor.log(String.Format("Details of: {0}", url),"FileConversationDetailsProvider");
                result = ConversationDetails.ReadXml(XElement.Parse(secureGetBytesAsString(url)));
                wire.SendDirtyConversationDetails(jid);
            }
            catch (UriFormatException e)
            {
                _auditor.error(String.Format("AppendSlideAfter: {0} {1} {2}",currentSlideId,jid, type), "FileConversationDetailsProvider", e);
            }
            catch (XmlException e)
            {
                _auditor.error(String.Format("AppendSlideAfter: {0} {1} {2}", currentSlideId, jid, type), "FileConversationDetailsProvider", e);
            }
            catch (WebException e)
            {
                _auditor.error(String.Format("AppendSlideAfter: {0} {1} {2}", currentSlideId, jid, type), "FileConversationDetailsProvider", e);
            }
            catch (Exception e)
            {
                _auditor.error(String.Format("AppendSlideAfter: {0} {1} {2}", currentSlideId, jid, type), "FileConversationDetailsProvider", e);
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
                _auditor.log(String.Format("Update failed: {0}", details.Jid), "FileConversationDetailsProvider");
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
                _auditor.log(String.Format("ConversationsFor: {0}", uri),"FileConversationDetailsProvider");
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
                _auditor.error("ConversationsFor","ConversationsFor",e);
                return new List<SearchConversationDetails>();
            }
        }
        public ConversationDetails Create(ConversationDetails details)
        {
            try
            {
                var uri = server.createConversation(details.Title.ToString());
                _auditor.log(String.Format("createConvUri: {0}", uri),"FileConversationDetailsProvider");
                var data = insecureGetString(uri);
                return SearchConversationDetails.ReadXML(XElement.Parse(data));
            }
            catch (Exception e)
            {
                _auditor.error("CreateConversation","FileConversationDetailsProvider", e);
                return ConversationDetails.Empty;
            }
        }
        public ConversationDetails DuplicateSlide(ConversationDetails conversation, Slide slide)
        {
            try
            {
                var uri = server.duplicateSlide(slide.id, conversation.Jid.ToString());
                _auditor.log(String.Format("DuplicateSlide: {0}", uri),"FileConversationDetailsProvider");
                var data = insecureGetString(uri);
                return SearchConversationDetails.ReadXML(XElement.Parse(data));
            }
            catch (Exception e)
            {
                _auditor.error("DuplicateSlide","FileConversationDetailsProvider",e);
                return ConversationDetails.Empty;
            }
        }
        public ConversationDetails DuplicateConversation(ConversationDetails conversation)
        {
            try
            {
                var uri = server.duplicateConversation(conversation.Jid.ToString());
                _auditor.log(String.Format("DuplicateConversation: {0}", uri),"FileConversationDetailsProvider");
                var data = insecureGetString(uri);
                return SearchConversationDetails.ReadXML(XElement.Parse(data));
            }
            catch (Exception e)
            {
                _auditor.error("DuplicateConversation","FileConversationDetailsProvider", e);
                return ConversationDetails.Empty;
            }
        }
    }
}
