using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Utils.Connection;
using SandRibbonObjects;
using System.Text.RegularExpressions;
using SandRibbon.Utils;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Ionic.Zip;
using System.IO;
using MeTLLib.DataTypes;

namespace SandRibbon.Providers.Structure
{
 /*   public class FileConversationDetailsProvider : HttpResourceProvider, IConversationDetailsProvider
    {
        private static readonly int HTTP_PORT = 1188;
        private static string ROOT_ADDRESS
        {
            get { return string.Format("https://{0}:{1}", Constants.JabberWire.SERVER, HTTP_PORT); }
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
        public ConversationDetails DetailsOf(string conversationJid)
        {
            App.Now("Getting details for " + conversationJid);
            try
            {
                var url = string.Format("{0}/{1}/{2}/{3}", ROOT_ADDRESS, STRUCTURE, conversationJid, DETAILS);
                var response = XElement.Parse(HttpResourceProvider.secureGetString(url));
                var result = ConversationDetails.ReadXml(response);
                return result;
            }
            catch (XmlException e)
            {
                throw new Exception(string.Format("Could not retrieve details of {0}", conversationJid), e);
            }
            catch (WebException e)
            {
                ProviderMonitor.HealthCheck(() => { });
                return null;
            }
        }
        public FileConversationDetailsProvider()
        {
            Commands.ReceiveDirtyConversationDetails.RegisterCommand(new DelegateCommand<string>(ReceiveDirtyConversationDetails));
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
                var slide = new Slide(slideId,details.Author,type,position +1,720,540);
                foreach (var existingSlide in details.Slides)
                    if (existingSlide.index >= slide.index)
                        existingSlide.index++;
                details.Slides.Insert(slide.index, slide);
                Update(details);
                return details;
            }
            catch (WebException)
            {
                ProviderMonitor.HealthCheck(() => {});
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
                details.Slides.Add(new Slide(slideId,details.Author,Slide.TYPE.SLIDE,details.Slides.Count,720,540));
                Update(details);
                return details;
            }
            catch (WebException)
            {
                ProviderMonitor.HealthCheck(() => {});
                return null;
            }
        }
        private void ReceiveDirtyConversationDetails(string jid)
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
        public ConversationDetails Update(ConversationDetails details)
        {
            try
            {
                var url = string.Format("{0}/{1}?overwrite=true&path={2}/{3}&filename={4}", ROOT_ADDRESS, UPLOAD, STRUCTURE, details.Jid, DETAILS);
                securePutData(url, details.GetBytes());
                Commands.SendDirtyConversationDetails.ExecuteAsync(details.Jid);
                return details;
            }
            catch (WebException e)
            {
                ProviderMonitor.HealthCheck(() => {});
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
                var data = secureGetData(string.Format("https://{0}:1188/Structure/all.zip", Constants.JabberWire.SERVER));
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
            catch(NotSetException e)
            {
                return new List<ConversationDetails>(); 
            }
        }
        private List<ConversationDetails> RestrictToAccessible(IEnumerable<ConversationDetails> summary, IEnumerable<string> myGroups)
        {
            return summary
                .Where(c => !String.IsNullOrEmpty(c.Subject))
                .Where(c => myGroups.Contains(c.Subject))
                .Where(c => c.Subject != "Deleted")
                .Distinct(new UniqueConversationComparator()).ToList();
        }
        public ConversationDetails Create(ConversationDetails details)
        {
            if (details.Slides.Count == 0)
            {
                var id = GetApplicationLevelInformation().currentId;
                details.Jid = id.ToString();
                details.Slides.Add(new Slide(id+1,details.Author,Slide.TYPE.SLIDE,0,720,540));
            }
            details.Created = DateTimeFactory.Now();
            ResourceUploader.uploadResourceToPath(Encoding.UTF8.GetBytes(details.WriteXml().ToString(SaveOptions.DisableFormatting)),
                string.Format("Structure/{0}", details.Jid), DETAILS);
            Update(details);
            return details;
        }
        public ApplicationLevelInformation GetApplicationLevelInformation()
        {
            return new ApplicationLevelInformation(Int32.Parse(HttpResourceProvider.secureGetString(NEXT_AVAILABLE_ID)));
        }
    }
  */
}
