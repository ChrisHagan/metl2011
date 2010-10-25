using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Ionic.Zip;
using System.IO;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;
using Ninject;

namespace MeTLLib.Providers.Structure
{
    class FileConversationDetailsProvider : HttpResourceProvider, IConversationDetailsProvider
    {
        [Inject]
        public IProviderMonitor providerMonitor { private get; set; }
        [Inject]
        public MeTLServerAddress server { private get; set; }
        private IResourceUploader resourceUploader;
        public FileConversationDetailsProvider(IWebClientFactory factory, IResourceUploader uploader)
            : base(factory)
        {
            resourceUploader = uploader;
            ListConversations();
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
            //So, from a design perspective, conversationJid must be a string, which must be non-empty, non-null.  But it might be a string that isn't currently a conversation Jid.
            //Expected behavious is return a valid conversationDetails, or return an empty conversationDetails to reflect that the conversation doesn't exist.
            if (String.IsNullOrEmpty(conversationJid)) throw new ArgumentNullException("conversationJid", "Argument cannot be null or empty");
            try
            {
                var url = new System.Uri(string.Format("{0}/{1}/{2}/{3}", ROOT_ADDRESS, STRUCTURE, conversationJid, DETAILS));
                var response = XElement.Parse(secureGetString(url));
                var result = ConversationDetails.ReadXml(response);
                return result;
            }
            catch (UriFormatException e)
            {
                throw new Exception(string.Format("Could not create valid Uri for DetailsOf, using conversationJid: {0}", conversationJid), e);
            }
            catch (XmlException e)
            {
                throw new Exception(string.Format("Could not parse retrieved details of {0}", conversationJid), e);
            }
            catch (WebException e)
            {
                providerMonitor.HealthCheck(() => { });
                return ConversationDetails.Empty;
            }
        }
        public ConversationDetails AppendSlideAfter(int currentSlide, string title)
        {
            return AppendSlideAfter(currentSlide, title, Slide.TYPE.SLIDE);
        }
        public ConversationDetails AppendSlideAfter(int currentSlide, string title, Slide.TYPE type)
        {
            try
            {
                var details = DetailsOf(title);
                var slideId = details.Slides.Select(s => s.id).Max() + 1;
                var position = getPosition(currentSlide, details.Slides);
                if (position == -1) return details;
                var slide = new Slide(slideId, details.Author, type, position + 1, 720, 540);
                foreach (var existingSlide in details.Slides)
                    if (existingSlide.index >= slide.index)
                        existingSlide.index++;
                details.Slides.Insert(slide.index, slide);
                Update(details);
                return details;
            }
            catch (WebException)
            {
                providerMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return null;
            }
        }
        private int getPosition(int slide, List<Slide> slides)
        {
            for (var i = 0; i < slides.Count; i++)
                if (slides[i].id == slide)
                    return i;
            return -1;
        }
        public ConversationDetails AppendSlide(string title)
        {
            try
            {
                var details = DetailsOf(title);
                var slideId = details.Slides.Select(s => s.id).Max() + 1;
                details.Slides.Add(new Slide(slideId, details.Author, Slide.TYPE.SLIDE, details.Slides.Count, 720, 540));
                Update(details);
                return details;
            }
            catch (WebException)
            {
                providerMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return null;
            }
        }
        public void ReceiveDirtyConversationDetails(string jid)
        {
            var newDetails = DetailsOf(jid);
            if (Globals.authorizedGroups.Select(g => g.groupKey).Contains("Superuser"))
            {
                conversationsCache = (conversationsCache.Where(c => c.Jid != jid).Union(new[] { newDetails })).ToList();
            }
            else
            {
                conversationsCache = RestrictToAccessible(conversationsCache.Where(c => c.Jid != jid).Union(new[] { newDetails }), Globals.authorizedGroups.Select(g => g.groupKey)).ToList();
            }
        }
        private bool DetailsAreAccurate(ConversationDetails details)
        {
            var url = string.Format("{0}/{1}/{2}/{3}", ROOT_ADDRESS, STRUCTURE, details.Jid, DETAILS);
            var currentServerString = secureGetString(new System.Uri(url));
            var currentServerCD = ConversationDetails.ReadXml(XElement.Parse(currentServerString));
            if (details.ValueEquals(currentServerCD))
                return true;
            else
                return false;
        }
        public ConversationDetails Update(ConversationDetails details)
        {
            try
            {
                var url = string.Format("{0}/{1}?overwrite=true&path={2}/{3}&filename={4}", ROOT_ADDRESS, UPLOAD, STRUCTURE, details.Jid, DETAILS);
                securePutData(new System.Uri(url), details.GetBytes());
                Commands.SendDirtyConversationDetails.Execute(details.Jid);
                if (DetailsAreAccurate(details))
                    return details;
                else
                    throw new Exception("ConversationDetails not successfully uploaded");
            }
            catch (WebException e)
            {
                providerMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return null;
            }
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
        private List<ConversationDetails> conversationsCache;
        public IEnumerable<ConversationDetails> ListConversations()
        {
            try
            {
                var myGroups = Globals.authorizedGroups.Select(g => g.groupKey);
                if (conversationsCache != null && conversationsCache.Count() > 0)
                {
                    if (Globals.authorizedGroups.Select(g => g.groupKey).Contains("Superuser"))
                    {
                        return conversationsCache;
                    }
                    conversationsCache = RestrictToAccessible(conversationsCache, myGroups);
                    return conversationsCache;
                }
                if (server == null) return new List<ConversationDetails>();
                var data = secureGetData(new System.Uri(string.Format("https://{0}:1188/Structure/all.zip", server.host)));
                using (var zip = ZipFile.Read(data))
                {
                    var summary = zip
                        .Entries
                        .Where(e => e.FileName.Contains("details.xml"))
                        .Select(e =>
                                    {
                                        using (var stream = new MemoryStream())
                                        {
                                            e.Extract(stream);
                                            return
                                                ConversationDetails.ReadXml(
                                                    XElement.Parse(Encoding.UTF8.GetString(stream.ToArray())));
                                        }
                                    }).ToList();
                    if (Globals.authorizedGroups.Select(g => g.groupKey).Contains("Superuser"))
                    {
                        conversationsCache = summary;
                        return summary;
                    }
                    conversationsCache = RestrictToAccessible(summary, myGroups);
                    return conversationsCache;
                }
            }
            catch (NotSetException e)
            {
                return new List<ConversationDetails>();
            }
        }
        private List<ConversationDetails> RestrictToAccessible(IEnumerable<ConversationDetails> summary, IEnumerable<string> myGroups)
        {
            return summary.ToList();
            /*return summary
                .Where(c => !String.IsNullOrEmpty(c.Subject))
                .Where(c => myGroups.Contains(c.Subject))
                .Distinct(new UniqueConversationComparator()).ToList();
        */
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
            //resourceUploader.uploadResourceToPath(Encoding.UTF8.GetBytes(details.WriteXml().ToString(SaveOptions.DisableFormatting)),
            //    string.Format("Structure/{0}", details.Jid), DETAILS);
            Update(details);
            return details;
        }
        public ApplicationLevelInformation GetApplicationLevelInformation()
        {
            return new ApplicationLevelInformation(Int32.Parse(secureGetString(new System.Uri(NEXT_AVAILABLE_ID))));
        }
    }
}
