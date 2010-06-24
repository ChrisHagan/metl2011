using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;

namespace SandRibbon.Providers
{
    class MediaElementCache
    {
        private static readonly string videoCacheXMLfile = "videoCache\\videoCache.xml";
        private static Dictionary<string, Uri> ActualDict;
        private static Dictionary<string, System.Uri> CacheDict()
        {
            if (ActualDict == null)
                ActualDict = ReadDictFromFile();
            return ActualDict;
        }
        private static Dictionary<string, Uri> ReadDictFromFile()
        {
            var newDict = new Dictionary<string, Uri>();
            if (!System.IO.Directory.Exists("videoCache"))
                System.IO.Directory.CreateDirectory("videoCache");
            if (System.IO.File.Exists(videoCacheXMLfile))
            {
                var XDoc = XElement.Load(videoCacheXMLfile);
                foreach (XElement name in XDoc.Elements("CachedUri"))
                {
                    newDict.Add(name.Attribute("remote").Value.ToString(), 
                        new Uri(name.Attribute("local").Value.ToString(),UriKind.Relative));
                }
            }
            return newDict;
        }
        private static void Add(string remoteUri, Uri localUri)
        {
            CacheDict().Add(remoteUri, localUri);
            if (!System.IO.Directory.Exists("videoCache"))
                System.IO.Directory.CreateDirectory("videoCache");
            var XDoc = "<CachedUris>";
            foreach (KeyValuePair<string, Uri> kv in CacheDict())
            {
                XDoc += "<CachedUri remote='" + remoteUri + "' local='" + localUri + "'/>";
            }
            XDoc += "</CachedUris>";
            System.IO.File.WriteAllText(videoCacheXMLfile, XDoc);
        }
        public static Uri LocalSource(Uri remoteUri)
        {
            if (remoteUri.ToString().StartsWith("videoCache\\_"))
                return remoteUri;
            if (!CacheDict().ContainsKey(remoteUri.ToString()))
            {
                if (!System.IO.Directory.Exists("videoCache"))
                    System.IO.Directory.CreateDirectory("videoCache");
                var localFileName = "";
                foreach (string seg in remoteUri.Segments)
                    localFileName += seg.ToString().Replace("/", "_").Replace("\\", "_");
                var localUriString = "videoCache\\" + localFileName;
                System.IO.File.WriteAllBytes(localUriString, HttpResourceProvider.secureGetData(remoteUri.ToString()));
                var localUri = new Uri(localUriString, UriKind.Relative);
                Add(remoteUri.ToString(), localUri);
            }
            return CacheDict()[remoteUri.ToString()];
        }
        public static Uri RemoteSource(Uri media)
        {
            var results = new List<KeyValuePair<string, Uri>>();
            foreach (KeyValuePair<string, Uri> kv in CacheDict())
            {
                if (kv.Value == media)
                    results.Add(kv);
            }
            if (results.Count > 0)
                return results.FirstOrDefault().Value;
            return null;
        }
    }
}
