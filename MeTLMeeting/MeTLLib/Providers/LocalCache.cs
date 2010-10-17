using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using MeTLLib.Providers.Connection;
using Ninject;

namespace MeTLLib.Providers
{
    public class ResourceCache
    {
        [Inject] public HttpResourceProvider resourceProvider{private get; set;}
        [Inject] public IResourceUploader resourceUploader{private get;set;}
        [Inject] public MeTLServerAddress server { private get; set; }
        public static readonly string cacheName = "resourceCache";
        private string cacheXMLfile = cacheName + "\\" + cacheName + ".xml";
        private Dictionary<string, System.Uri> ActualDict = null;
        private Dictionary<string, System.Uri> CacheDict
        {
            get
            {
                if (ActualDict == null)
                {
                    ActualDict = ReadDictFromFile();
                }
                return ActualDict;
            }
            set
            {
                ActualDict = value;
            }
        }
        private Dictionary<string, Uri> ReadDictFromFile()
        {
            var newDict = new Dictionary<string, Uri>();
            if (!System.IO.Directory.Exists(cacheName))
                System.IO.Directory.CreateDirectory(cacheName);
            if (System.IO.File.Exists(cacheXMLfile))
            {
                var XDoc = XElement.Load(cacheXMLfile);
                foreach (XElement name in XDoc.Elements("CachedUri"))
                {
                    newDict.Add(name.Attribute("remote").Value.ToString(),
                        new Uri(name.Attribute("local").Value.ToString(), UriKind.Relative));
                }
            }
            
            return newDict;
        }
        //we keep 7 days worth of cached images, 
        public void CleanUpCache()
        {
            var XDoc = "<CachedUris>";
            foreach (var uri in Directory.GetFiles(cacheName + "\\"))
            {
                if (Directory.GetLastAccessTime(uri).Ticks < DateTimeFactory.Now().Subtract(new TimeSpan(7, 0, 0, 0)).Ticks)
                    File.Delete(cacheName + "\\" + uri);
                else
                    XDoc += string.Format("<CachedUri remote='{0}' local='{1}'/>", uri, RemoteSource(new Uri(uri, UriKind.RelativeOrAbsolute)));
            }
            XDoc += "</CachesUris>";
            File.WriteAllText(cacheXMLfile, XDoc);
        }
        private void Add(string remoteUri, Uri localUri)
        {
            if (CacheDict.Contains(new KeyValuePair<string, Uri>(remoteUri, localUri))) return;
            CacheDict.Add(remoteUri, localUri);
            if (!System.IO.Directory.Exists(cacheName))
                System.IO.Directory.CreateDirectory(cacheName);
            var XDoc = new XDocument();
            var root = new XElement("CachedUris");
            XDoc.Add(root);
            foreach (var key in CacheDict.Keys)
            {
                root.Add(new XElement("CachedUri", new XAttribute("remote", key), new XAttribute("local", CacheDict[key].ToString())));
            }
            if (File.Exists(cacheXMLfile))
                File.Delete(cacheXMLfile);
            XDoc.Save(cacheXMLfile);
        }
        public Uri LocalSource(string uri)
        {
            return LocalSource(new Uri(uri, UriKind.RelativeOrAbsolute));
        }
        public Uri LocalSource(Uri remoteUri)
        {
            if (remoteUri.ToString().StartsWith(cacheName + "\\"))
                return remoteUri;
            if (!CacheDict.ContainsKey(remoteUri.ToString()))
            {
                if (!Directory.Exists(cacheName))
                    Directory.CreateDirectory(cacheName);
                var splitString = remoteUri.ToString().Split('/').Reverse();
                if (!Directory.Exists(cacheName + "\\"+splitString.ElementAt(1)))
                    Directory.CreateDirectory(cacheName + "\\"+splitString.ElementAt(1));
                var localUriString = cacheName + "\\" +splitString.ElementAt(1) + "\\" + splitString.ElementAt(0);
                File.WriteAllBytes(localUriString, resourceProvider.secureGetData(remoteUri));
                var localUri = new Uri(localUriString, UriKind.Relative);
                Add(remoteUri.ToString(), localUri);
            }
            return CacheDict[remoteUri.ToString()];
        }
        public Uri RemoteSource(Uri media)
        {

            if (media.ToString().StartsWith("Resource\\") || media.ToString().StartsWith("https://"))
                return media;
            if (!CacheDict.Values.Contains(media))
                CacheDict = ReadDictFromFile();
            var uri = CacheDict.Where(kv => kv.Value == media).FirstOrDefault().Key;
            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }
    }
}