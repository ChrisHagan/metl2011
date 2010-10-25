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
using System.Xml;
using System.Diagnostics;

namespace MeTLLib.Providers
{
    public class ResourceCache
    {
        [Inject]
        public HttpResourceProvider resourceProvider { private get; set; }
        [Inject]
        public IResourceUploader resourceUploader { private get; set; }
        [Inject]
        public MeTLServerAddress server { private get; set; }
        public static readonly string cacheName = "resourceCache";
        private string cacheXMLfile = cacheName + "\\" + cacheName + ".xml";
        private readonly System.Uri failUri = new Uri("Resources\\Slide_Not_Loaded.png", UriKind.RelativeOrAbsolute);
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
                try
                {
                    using (XmlReader reader = XmlReader.Create(cacheXMLfile))
                    {
                        var XDoc = XElement.Load(reader);
                        foreach (XElement name in XDoc.Elements("CachedUri"))
                        {
                            newDict.Add(name.Attribute("remote").Value.ToString(),
                                new Uri(name.Attribute("local").Value.ToString(), UriKind.Relative));
                        }
                    }
                }
                catch(Exception e)
                {
                    File.Delete(cacheXMLfile);
                }
            }
            return newDict;
        }
        public void CleanUpCache()
        {
            if (!System.IO.Directory.Exists(cacheName))
                System.IO.Directory.CreateDirectory(cacheName);
            using (XmlWriter writer = XmlWriter.Create(cacheXMLfile))
            {
                var XDoc = new XDocument();
                var root = new XElement("CachedUris");
                XDoc.Add(root);

                foreach (var dir in Directory.GetDirectories(cacheName + "\\"))
                {
                    foreach (var uri in Directory.GetFiles(cacheName + "\\" + dir))
                    {
                        if (Directory.GetLastAccessTime(uri).Ticks < DateTimeFactory.Now().Subtract(new TimeSpan(7, 0, 0, 0)).Ticks)
                            File.Delete(cacheName + "\\" + dir + "\\" + uri);
                        else
                            root.Add(new XElement("CachedUri", new XAttribute("remote", RemoteSource(new Uri(uri, UriKind.RelativeOrAbsolute))), new XAttribute("local", CacheDict + "\\" + dir + "\\" + uri)));
                    }
                }
                XDoc.Save(writer);
            }
        }
        private void Add(string remoteUri, Uri localUri)
        {
            using (XmlWriter writer = XmlWriter.Create(cacheXMLfile))
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
                XDoc.Save(writer);
            }
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
                if (!Directory.Exists(cacheName + "\\" + splitString.ElementAt(1)))
                    Directory.CreateDirectory(cacheName + "\\" + splitString.ElementAt(1));
                var localUriString = cacheName + "\\" + splitString.ElementAt(1) + "\\" + splitString.ElementAt(0);
                try
                {
                    File.WriteAllBytes(localUriString, resourceProvider.secureGetData(remoteUri));
                }
                catch (WebException ex)
                {
                    Trace.TraceInformation("WebException during LocalCache.localSource: " + ex.Message);
                    return failUri;
                }
                var localUri = new Uri(localUriString, UriKind.Relative);
                Add(remoteUri.ToString(), localUri);
            }
            return CacheDict[remoteUri.ToString()];
        }
        public Uri RemoteSource(Uri media)
        {
            //not sure about this next line yet.
            if (media.Equals(failUri)) return media;
            if (media.ToString().StartsWith("Resource\\") || media.ToString().StartsWith("https://"))
                return media;
            if (!CacheDict.Values.Contains(media))
                CacheDict = ReadDictFromFile();
            var uri = CacheDict.Where(kv => kv.Value == media).FirstOrDefault().Key;
            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }
    }
}