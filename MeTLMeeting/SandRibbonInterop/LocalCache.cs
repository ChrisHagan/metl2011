using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;

namespace SandRibbonInterop.LocalCache
{
    public class MediaElementCache : ResourceCache
    {
        private static string cacheName = "videoCache";
    }
    public class FileCache : ResourceCache
    {
        private static string cacheName = "fileCache";
    }
    public class ImageCache : ResourceCache
    {
        private static string cacheName = "imageCache";
    }
    public abstract class ResourceCache
    {
        private static string cacheName = "resourceCache";
        private static readonly string cacheXMLfile = cacheName + "\\" + cacheName + ".xml";
        private static Dictionary<string, Uri> ActualDict;
        private static Dictionary<string, System.Uri> CacheDict()
        {
            if (ActualDict == null)
                try
                {
                    ActualDict = ReadDictFromFile();
                }
                catch (Exception) { return new Dictionary<string, Uri>(); }
            return ActualDict;
        }
        private static Dictionary<string, Uri> ReadDictFromFile()
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
        private static void Add(string remoteUri, Uri localUri)
        {
            if (CacheDict().Contains(new KeyValuePair<string, Uri>(remoteUri, localUri))) return;
            CacheDict().Add(remoteUri, localUri);
            if (!System.IO.Directory.Exists(cacheName))
                System.IO.Directory.CreateDirectory(cacheName);
            var XDoc = "<CachedUris>";
            foreach (KeyValuePair<string, Uri> kv in CacheDict())
            {
                XDoc += "<CachedUri remote='" + kv.Key.ToString() + "' local='" + kv.Value.ToString() + "'/>";
            }
            XDoc += "</CachedUris>";
            System.IO.File.WriteAllText(cacheXMLfile, XDoc);
        }
        public static Uri LocalSource(Uri remoteUri)
        {
            if (remoteUri.ToString().StartsWith(cacheName + "\\_"))
                return remoteUri;
            if (!CacheDict().ContainsKey(remoteUri.ToString()))
            {
                if (!System.IO.Directory.Exists(cacheName))
                    System.IO.Directory.CreateDirectory(cacheName);
                var localFileName = "";
                foreach (string seg in remoteUri.Segments)
                    localFileName += seg.ToString().Replace("/", "_").Replace("\\", "_");
                var localUriString = cacheName + "\\" + localFileName;
               //System.IO.File.WriteAllBytes(localUriString, HttpResourceProvider.secureGetData(remoteUri.ToString()));
                var localUri = new Uri(localUriString, UriKind.Relative);
                Add(remoteUri.ToString(), localUri);
            }
            return CacheDict()[remoteUri.ToString()];
        }
        public static Uri RemoteSource(Uri media)
        {
            var uri = CacheDict().Where(kv => kv.Value == media).Select(k => k.Key).FirstOrDefault();
            return uri== null ? null : new Uri(uri, UriKind.RelativeOrAbsolute);
             
            var results = new List<KeyValuePair<string, Uri>>();
            foreach (KeyValuePair<string, Uri> kv in CacheDict())
            {
                if (kv.Value == media)
                    results.Add(kv);
            }
            if (results.Count > 0)
                return new Uri(results.FirstOrDefault().Key, UriKind.Absolute);
            return null;
        }
    }
}
