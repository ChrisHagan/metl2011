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

namespace MeTLLib.Providers
{
    public class ResourceCache
    {
        HttpResourceProvider resourceProvider;
        ResourceUploader resourceUploader;
        public ResourceCache(HttpResourceProvider provider, ResourceUploader uploader) {
            resourceProvider= provider;
            resourceUploader = uploader;
        }
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
            var XDoc = "<CachedUris>";
            foreach (var key in CacheDict.Keys)
            {
                XDoc += string.Format("<CachedUri remote='{0}' local='{1}'/>", key, CacheDict[key].ToString());
            }
            XDoc += "</CachedUris>";
            File.WriteAllText(cacheXMLfile, XDoc);
        }
        public Uri LocalSource(string uri)
        {
            return LocalSource(new Uri(uri, UriKind.RelativeOrAbsolute));
        }
        public Uri LocalSource(Uri remoteUri)
        {
            if (remoteUri.ToString().StartsWith(cacheName + "\\"))
                return remoteUri;
           //This is a quick and dirty fix for a problem that should be solved elsewhere.  
            //Somwehow, the source attribute of some stanzas is having the server section repeated when it's constructed.
            // eg:  https://madam.adm.monash.edu.au:1188/https://madam.adm.monash.edu.au:1188/https://madam.adm.monash.edu.au:1188/Resource/101/Bear.wmv"
            if (remoteUri.ToString().StartsWith("https://") && remoteUri.ToString().Contains(":1188/https://"))
                remoteUri = new System.Uri("https://"+Constants.SERVER+remoteUri.ToString().Substring(remoteUri.ToString().LastIndexOf(":1188/")), UriKind.Absolute);
            if (!CacheDict.ContainsKey(remoteUri.ToString()))
            {
                if (!Directory.Exists(cacheName))
                    Directory.CreateDirectory(cacheName);
                var localUriString = cacheName + "\\" + remoteUri.ToString().Split('/').Reverse().First();
                File.WriteAllBytes(localUriString, resourceProvider.secureGetData(remoteUri.ToString()));
                var localUri = new Uri(localUriString, UriKind.Relative);
                Add(remoteUri.ToString(), localUri);
            }
            return CacheDict[remoteUri.ToString()];
        }
        public Uri RemoteSource(Uri media)
        {

            if (media.ToString().StartsWith("Resource\\"))
                return media;
            var uri = CacheDict.Where(kv => kv.Value == media).FirstOrDefault().Key;
            return uri == null ? null : new Uri(uri, UriKind.RelativeOrAbsolute);
        }
    }
    class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = (WebRequest)base.GetWebRequest(address);
            request.Timeout = int.MaxValue;
            return request;
        }
    }
}