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
        public FileConversationDetailsProvider(MetlConfiguration _server,IWebClientFactory factory, IResourceUploader uploader,Credentials creds)
            : base(factory)
        {
            server = _server;
            resourceUploader = uploader;
            credentials = creds;
        }
        /*
        private string ROOT_ADDRESS
        {
            get { return string.Format("{2}://{0}:{1}", server.host, server.port, server.protocol); }
        }
        */

        //private readonly string STRUCTURE = "Structure";
        //private readonly string UPLOAD = "upload_nested.yaws";
        private string NEXT_AVAILABLE_ID
        {
            //            get { return string.Format("{0}/primarykey.yaws", ROOT_ADDRESS); }
            get { return server.resourceUrl + "/" + server.primaryKeyGenerator; }

        }
        private static readonly string DETAILS = "details.xml";
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
//                var url = new System.Uri(string.Format("{0}/{1}/{2}/{3}/{4}", ROOT_ADDRESS, STRUCTURE, INodeFix.Stem(conversationJid), conversationJid, DETAILS));
                var url = new System.Uri(string.Format("{0}/{1}/{2}/{3}/{4}", server.resourceUrl, server.structureDirectory, INodeFix.Stem(conversationJid), conversationJid, DETAILS));
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
        public ConversationDetails AppendSlideAfter(int currentSlide, string title)
        {
            return AppendSlideAfter(currentSlide, title, Slide.TYPE.SLIDE);
        }
        public ConversationDetails AppendSlideAfter(int currentSlideId, string title, Slide.TYPE type)
        {
            var details = DetailsOf(title);
            if (ConversationDetails.Empty.Equals(details)) 
                return ConversationDetails.Empty;
            
            var nextSlideId = details.Slides.Select(s => s.id).Max() + 1;
            var currentSlide = details.Slides.Where(s => s.id == currentSlideId).First();
            if (currentSlide == null)
            {
                Trace.TraceInformation("CRASH: currentSlideId does not belong to the conversation");
                return details;
            }
            var slide = new Slide(nextSlideId, details.Author, type, currentSlide.index + 1, 720, 540);
            foreach (var existingSlide in details.Slides)
                if (existingSlide.index >= slide.index)
                    existingSlide.index++;
            details.Slides.Insert(slide.index, slide);
            Update(details);
            return details;
        }
        public ConversationDetails AppendSlide(string title)
        {
            var details = DetailsOf(title);
            var slideId = details.Slides.Select(s => s.id).Max() + 1;
            details.Slides.Add(new Slide(slideId, details.Author, Slide.TYPE.SLIDE, details.Slides.Count, 720, 540));
            return Update(details);
        }
        private bool DetailsAreAccurate(ConversationDetails details)
        {
            var url = string.Format("{0}/{1}/{2}/{3}/{4}", server.resourceUrl, server.structureDirectory, INodeFix.Stem(details.Jid), details.Jid, DETAILS);
            var currentServerString = secureGetBytesAsString(new System.Uri(url));
            var currentServerCD = ConversationDetails.ReadXml(XElement.Parse(currentServerString));
            if (details.ValueEquals(currentServerCD))
                return true;
            else
                return false;
        }
        public ConversationDetails Update(ConversationDetails details)
        {
            var url = string.Format("{0}/{1}?overwrite=true&path={2}/{3}/{4}&filename={5}", server.resourceUrl, server.uploadPath, server.structureDirectory, INodeFix.Stem(details.Jid), details.Jid, DETAILS);
            securePutData(new System.Uri(url), details.GetBytes());
            wire.SendDirtyConversationDetails(details.Jid);
            if (!DetailsAreAccurate(details))
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
                var uri = new Uri(Uri.EscapeUriString(string.Format("{0}{1}", server.conversationsUrl, query)), UriKind.RelativeOrAbsolute);
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
            if (details.Slides.Count == 0)
            {
                var id = GetApplicationLevelInformation().currentId;
                details.Jid = id.ToString();
                details.Slides.Add(new Slide(id + 1, details.Author, Slide.TYPE.SLIDE, 0, 720, 540));
            }
            details.Created = DateTimeFactory.Now();
            Update(details);
            return details;
        }
        public ApplicationLevelInformation GetApplicationLevelInformation()
        {
            return new ApplicationLevelInformation(Int32.Parse(secureGetString(new System.Uri(NEXT_AVAILABLE_ID))));
        }
    }
}
