using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SandRibbon.Components;
using SandRibbonObjects;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Ionic.Zip;
using Divan;
using System.IO;

namespace PowerpointJabber
{
    class FileConversationDetailsProvider : HttpResourceProvider
    {
        private static readonly int HTTP_PORT = 1188;
        private static string ROOT_ADDRESS
        {
            get { return string.Format("http://{0}:{1}", Constants.JabberWire.SERVER, HTTP_PORT); }
        }
        private static readonly string RESOURCE = "Resource";
        private static readonly string STRUCTURE = "Structure";
        private static readonly string UPLOAD = "upload_nested.yaws";
        private static string NEXT_AVAILABLE_ID
        {
            get { return string.Format("{0}/primarykey.yaws", ROOT_ADDRESS); }
        }
        private static readonly string DETAILS = "details.xml";
        private static readonly string SUMMARY = "summary.xml";
        private Credentials credentials;
        public FileConversationDetailsProvider()
        {
            credentials = ThisAddIn.instance.wire.myCredentials;
            //Commands.SetIdentity.RegisterCommand(new DelegateCommand<JabberWire.Credentials>(SetIdentity));
            //Commands.ReceiveDirtyConversationDetails.RegisterCommand(new DelegateCommand<string>(ReceiveDirtyConversationDetails));
        }
        public SandRibbonObjects.ConversationDetails DetailsOf(string conversationJid)
        {
            try
            {
                var url = string.Format("{0}/{1}/{2}/{3}", ROOT_ADDRESS, STRUCTURE, conversationJid, DETAILS);
                var response = XElement.Parse(HttpResourceProvider.secureGetString(url));
                var result = new ConversationDetails().ReadXml(response);
                return result;
            }
            catch (XmlException e)
            {
                throw new Exception(string.Format("Could not retrieve details of {0}", conversationJid), e);
            }
            catch (WebException e)
            {
                //ProviderMonitor.HealthCheck(() => { });
                return new ConversationDetails();
            }
        }
        private void SetIdentity(Credentials credentials)
        {
            this.credentials = credentials;
            ListConversations();
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
                var slide = new Slide
                                {
                                    author = details.Author,
                                    id = slideId,
                                    index = position + 1,
                                    type = type
                                };
                foreach (var existingSlide in details.Slides)
                    if (existingSlide.index >= slide.index)
                        existingSlide.index++;
                details.Slides.Insert(slide.index, slide);
                Update(details);
                return details;
            }
            catch (WebException)
            {
                //  ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return new ConversationDetails();
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
                details.Slides.Add(new Slide
                {
                    author = details.Author,
                    id = slideId,
                    index = details.Slides.Count
                });
                Update(details);
                return details;
            }
            catch (WebException)
            {
                //ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return new ConversationDetails();
            }
        }
        private void ReceiveDirtyConversationDetails(string jid)
        {
            var newDetails = DetailsOf(jid);
            if (credentials.authorizedGroups.Select(g => g.groupKey).Contains("Superuser"))
            {
                conversationsCache = (conversationsCache.Where(c => c.Jid != jid).Union(new[] { newDetails })).ToList();
            }
            else
            {
                conversationsCache = RestrictToAccessible(conversationsCache.Where(c => c.Jid != jid).Union(new[] { newDetails }), credentials.authorizedGroups.Select(g => g.groupKey)).ToList();
            }
        }
        public ConversationDetails Update(SandRibbonObjects.ConversationDetails details)
        {
            try
            {
                var url = string.Format("{0}/{1}?overwrite=true&path={2}/{3}&filename={4}", ROOT_ADDRESS, UPLOAD, STRUCTURE, details.Jid, DETAILS);
                HttpResourceProvider.securePutData(url, details.GetBytes());
                //Commands.SendDirtyConversationDetails.Execute(details.Jid);
                return details;
            }
            catch (WebException e)
            {
                //ProviderMonitor.HealthCheck(() => { /*Everything is AOk*/});
                return new ConversationDetails();
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
        public IEnumerable<SandRibbonObjects.ConversationDetails> ListConversations()
        {
            if (credentials == null) return new List<ConversationDetails>();
            var myGroups = credentials.authorizedGroups.Select(g => g.groupKey);
            if (conversationsCache != null && conversationsCache.Count() > 0)
            {
                if (credentials.authorizedGroups.Select(g => g.groupKey).Contains("Superuser"))
                {
                    return conversationsCache;
                }
                conversationsCache = RestrictToAccessible(conversationsCache, myGroups);
                return conversationsCache;
            }
            var data = HttpResourceProvider.secureGetData(string.Format("http://{0}:1188/Structure/all.zip", Constants.JabberWire.SERVER));
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
                            return new ConversationDetails().ReadXml(XElement.Parse(Encoding.UTF8.GetString(stream.ToArray())));
                        }
                    }).ToList();
                if (credentials.authorizedGroups.Select(g => g.groupKey).Contains("Superuser"))
                {
                    conversationsCache = summary;
                    return summary;
                }
                conversationsCache = RestrictToAccessible(summary, myGroups);
                return conversationsCache;
            }
        }
        private List<ConversationDetails> RestrictToAccessible(IEnumerable<ConversationDetails> summary, IEnumerable<string> myGroups)
        {
            return summary
                .Where(c => !String.IsNullOrEmpty(c.Subject))
                .Where(c => myGroups.Contains(c.Subject))
                .Distinct(new UniqueConversationComparator()).ToList();
        }
        public SandRibbonObjects.ConversationDetails Create(SandRibbonObjects.ConversationDetails details)
        {
            if (details.Slides.Count == 0)
            {
                var id = GetApplicationLevelInformation().currentId;
                details.Jid = id.ToString();
                details.Slides.Add(new Slide
                {
                    author = details.Author,
                    id = id + 1,
                    type = Slide.TYPE.SLIDE
                });
            }
            details.Created = DateTimeFactory.Now();
            ResourceUploader.uploadResourceToPath(
                Encoding.UTF8.GetBytes(details.WriteXml().ToString(SaveOptions.DisableFormatting)),
                string.Format("Structure/{0}", details.Jid),
                DETAILS);
            Update(details);
            return details;
        }
        public SandRibbonObjects.ApplicationLevelInformation GetApplicationLevelInformation()
        {
            return new ApplicationLevelInformation { currentId = Int32.Parse(HttpResourceProvider.secureGetString(NEXT_AVAILABLE_ID)) };
        }
    }
    public class ResourceUploader
    {
        private static readonly string RESOURCE_SERVER_UPLOAD = string.Format("http://{0}:1188/upload_nested.yaws", Constants.JabberWire.SERVER);
        public static string uploadResource(string path, string file)
        {
            return uploadResource(path, file, false);
        }
        public static string uploadResource(string path, string file, bool overwrite)
        {
            try
            {
                var fullPath = string.Format("{0}?path=Resource/{1}&overwrite={2}", RESOURCE_SERVER_UPLOAD, path, overwrite);
                var res = HttpResourceProvider.securePutFile(
                    fullPath,
                    file);
                var url = XElement.Parse(res).Attribute("url").Value;
                return url;
            }
            catch (WebException e)
            {
                System.Windows.Forms.MessageBox.Show("Cannot upload resource: " + e.Message);
            }
            return "failed";
        }
        public static string uploadResourceToPath(byte[] resourceData, string path, string name)
        {
            return uploadResourceToPath(resourceData, path, name, true);
        }
        public static string uploadResourceToPath(byte[] resourceData, string path, string name, bool overwrite)
        {
            var url = string.Format("{0}?path={1}&overwrite={2}&filename={3}", RESOURCE_SERVER_UPLOAD, path, overwrite.ToString().ToLower(), name);
            var res = HttpResourceProvider.securePutData(url, resourceData);
            return XElement.Parse(res).Attribute("url").Value;
        }
    }
}
