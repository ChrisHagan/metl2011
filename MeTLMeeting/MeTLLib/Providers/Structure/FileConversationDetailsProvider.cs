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
using Ninject;

namespace MeTLLib.Providers.Structure
{
    class FileConversationDetailsProvider : HttpResourceProvider, IConversationDetailsProvider
    {
        [Inject]
        public MeTLServerAddress server { private get; set; }
        [Inject]
        public JabberWireFactory jabberWireFactory { private get; set; }
        private JabberWire _wire;
        [Inject]
        public MeTLGenericAddress searchServer { private get; set; }
        private object wireLock = new object();
        private JabberWire wire{
            get {
                lock (wireLock)
                {
                    if (_wire == null)
                        _wire = jabberWireFactory.wire();
                    return _wire;
                }
            }
        }
        private IResourceUploader resourceUploader;
        public FileConversationDetailsProvider(IWebClientFactory factory, IResourceUploader uploader)
            : base(factory)
        {
            resourceUploader = uploader;
        }
        private readonly int HTTP_PORT = 1188;
        private string ROOT_ADDRESS
        {
            get { return string.Format("https://{0}:{1}", server.host, HTTP_PORT); }
        }
        private readonly string RESOURCE = "Resource";
        private readonly string STRUCTURE = "Structure";
        private readonly string UPLOAD = "upload_nested.yaws";
        private string NEXT_AVAILABLE_ID
        {
            get { return string.Format("{0}/primarykey.yaws", ROOT_ADDRESS); }
        }
        private readonly string DETAILS = "details.xml";
        private readonly string SUMMARY = "summary.xml";
        public bool isAccessibleToMe(string jid)
        {
            var myGroups = Globals.authorizedGroups.Select(g => g.groupKey);
            var details = DetailsOf(jid);
            return myGroups.Contains(details.Subject);
        }
        public ConversationDetails DetailsOf(string conversationJid)
        {
            ConversationDetails result = ConversationDetails.Empty;
            if (String.IsNullOrEmpty(conversationJid))
            {
                Trace.TraceError("CRASH: Fixed: Argument cannot be null or empty - Reconnecting error that happens all the time");
                return result;
            }
            try
            {
                var url = new System.Uri(string.Format("{0}/{1}/{2}/{3}/{4}", ROOT_ADDRESS, STRUCTURE, INodeFix.Stem(conversationJid), conversationJid, DETAILS));
                result = ConversationDetails.ReadXml(XElement.Parse(secureGetString(url)));
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
            var url = string.Format("{0}/{1}/{2}/{3}/{4}", ROOT_ADDRESS, STRUCTURE, INodeFix.Stem(details.Jid), details.Jid, DETAILS);
            var currentServerString = secureGetString(new System.Uri(url));
            var currentServerCD = ConversationDetails.ReadXml(XElement.Parse(currentServerString));
            if (details.ValueEquals(currentServerCD))
                return true;
            else
                return false;
        }
        public ConversationDetails Update(ConversationDetails details)
        {
            var url = string.Format("{0}/{1}?overwrite=true&path={2}/{3}/{4}&filename={5}", ROOT_ADDRESS, UPLOAD, STRUCTURE, INodeFix.Stem(details.Jid), details.Jid, DETAILS);
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
       
        public IEnumerable<SearchConversationDetails> ConversationsFor(String query)
        {
            try
            {
                var uri = new Uri(Uri.EscapeUriString(string.Format("{0}{1}", searchServer.Uri.AbsoluteUri, query)), UriKind.Absolute);
                var data = secureGetString(uri);
                var results = XElement.Parse(data).Descendants("conversation").Select(SearchConversationDetails.ReadXML).ToList();

                var lastModified = from conversation in results
                               group conversation by conversation.Jid into convGroup
                               select new { Jid = convGroup.Key, LastModified = convGroup.Max(c => c.LastModified) };

                return results.Where(conv => lastModified.Contains(new { Jid = conv.Jid, LastModified = conv.LastModified })).OrderBy(conv => conv.relevance);
            }
            catch (Exception e)
            {
                return new List<SearchConversationDetails>();
            }
        }
        private List<ConversationDetails> RestrictToAccessible(IEnumerable<ConversationDetails> summary, IEnumerable<string> myGroups)
        {
            return summary.ToList();
        }
        public ConversationDetails Create(ConversationDetails details)
        {
            if (details.Slides.Count == 0)
            {
                var id = GetApplicationLevelInformation().currentId;
                details.Jid = id.ToString();
                details.Slides.Add(new Slide(id + 1, details.Author, Slide.TYPE.SLIDE, 0, 720, 540));
            }
            details.Created = DateTimeFactory.Parse(DateTimeFactory.Now().ToString());
            Update(details);
            return details;
        }
        public ApplicationLevelInformation GetApplicationLevelInformation()
        {
            return new ApplicationLevelInformation(Int32.Parse(secureGetString(new System.Uri(NEXT_AVAILABLE_ID))));
        }
    }
}
