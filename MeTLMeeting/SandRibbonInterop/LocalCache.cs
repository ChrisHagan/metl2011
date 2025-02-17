﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace SandRibbonInterop.LocalCache
{
    /*
    public class ResourceCache
    {
        public static string cacheName = "resourceCache";
        private static string cacheXMLfile = cacheName + "\\" + cacheName + ".xml";
        private static Dictionary<string, System.Uri> ActualDict = null;
        private static Dictionary<string, System.Uri> CacheDict
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
        //we keep 7 days worth of cached images, 
        public void CleanUpCache()
        {
            var XDoc = "<CachedUris>";
            foreach (var uri in Directory.GetFiles(cacheName + "\\"))
            {
                if (Directory.GetLastAccessTime(uri).Ticks < SandRibbonObjects.DateTimeFactory.Now().Subtract(new TimeSpan(7, 0, 0, 0)).Ticks)
                    File.Delete(cacheName + "\\" + uri);
                else
                    XDoc += string.Format("<CachedUri remote='{0}' local='{1}'/>", uri, RemoteSource(new Uri(uri, UriKind.RelativeOrAbsolute)));
            }
            XDoc += "</CachesUris>";
            File.WriteAllText(cacheXMLfile, XDoc);
        }
        private static void Add(string remoteUri, Uri localUri)
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
        public static Uri LocalSource(string uri)
        {
            return LocalSource(new Uri(uri, UriKind.RelativeOrAbsolute));
        }
        public static Uri LocalSource(Uri remoteUri)
        {
            if (remoteUri.ToString().StartsWith(cacheName + "\\"))
                return remoteUri;
            var file = remoteUri.ToString().Split('/').Reverse().First();
            if(file.Contains("submission"))
            {
                int num;
                if (!(int.TryParse(file.Substring(0, 1), out num)))
                    return remoteUri;
            }
            if (!CacheDict.ContainsKey(remoteUri.ToString()))
            {
                var match = new Regex(@"Resource/(\d+)/").Match(remoteUri.ToString());
                var resourceJid = match.Groups[1].Value;
                var dirName = string.Format(@"{0}\{1}", cacheName, resourceJid);
                if (!Directory.Exists(cacheName))
                    Directory.CreateDirectory(cacheName);
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);
                var localUriString = generateLocalFilename(remoteUri.ToString());
                //check if file exists
                if(!File.Exists(localUriString))
                    File.WriteAllBytes(localUriString, HttpResourceProvider.secureGetData(remoteUri.ToString()));
                var localUri = new Uri(localUriString, UriKind.Relative);
                Add(remoteUri.ToString(), localUri);
            }
            return CacheDict[remoteUri.ToString()];
        }

        private static string generateLocalFilename(string uri)
        {
            var uriParts = uri.Split('/').Reverse();
            var filename = uriParts.First();
            int tmp;
            if (int.TryParse(uriParts.ElementAt(1), out tmp))
            {
                var slideDirectory = uriParts.ElementAt(1);
                if (!Directory.Exists(cacheName + "\\" + slideDirectory))
                    Directory.CreateDirectory(cacheName + "\\" + slideDirectory);
                return cacheName + "\\" + slideDirectory + "\\" + filename;
            }
            else
            {
                return cacheName + "\\" + filename;
            }
        }

        public static Uri RemoteSource(Uri media)
        {

            if (media.ToString().StartsWith("Resource\\"))
                return media;
            var uri = CacheDict.Where(kv => kv.Value == media).FirstOrDefault().Key;
            return uri == null ? null : new Uri(uri, UriKind.RelativeOrAbsolute);
        }
    }
    public class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = (WebRequest)base.GetWebRequest(address);
            request.Timeout = int.MaxValue;
            return request;
        }
    }
    public class HttpResourceProvider
    {
        private static readonly string StagingMeTLCertificateSubject = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        private static readonly string StagingMeTLCertificateIssuer = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        private static readonly string DeifiedMeTLCertificateSubject = "E=root@deified.adm, CN=localhost, OU=deified, O=adm";
        private static readonly string DeifiedMeTLCertificateIssuer = "E=root@deified.adm, CN=localhost, OU=deified, O=adm";
        private static readonly string ReifierMeTLCertificateSubject = "E=root@reifier.adm, CN=localhost, OU=deified, O=adm";
        private static readonly string ReifierMeTLCertificateIssuer = "E=root@reifier.adm, CN=localhost, OU=deified, O=adm";

        private static readonly string MonashCertificateSubject = "CN=my.monash.edu.au, OU=ITS, O=Monash University, L=Clayton, S=Victoria, C=AU";
        private static readonly string MonashCertificateIssuer = "E=premium-server@thawte.com, CN=Thawte Premium Server CA, OU=Certification Services Division, O=Thawte Consulting cc, L=Cape Town, S=Western Cape, C=ZA";
        private static readonly NetworkCredential MeTLCredentials = new NetworkCredential("exampleUsername", "examplePassword");
        private static bool firstRun = true;

        private static WebClient client()
        {
            if (firstRun)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
                ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
                firstRun = false;
            }
            var wc = new WebClientWithTimeout { Credentials = MeTLCredentials };
            return wc;
        }
        private static bool bypassAllCertificateStuff(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            if (cert == null) return false;
            if ((cert.Subject == MonashCertificateSubject && cert.Issuer == MonashCertificateIssuer)
                || (cert.Subject == StagingMeTLCertificateSubject && cert.Issuer == StagingMeTLCertificateIssuer)
                || (cert.Subject == ReifierMeTLCertificateSubject && cert.Issuer == ReifierMeTLCertificateIssuer)
                || (cert.Subject == DeifiedMeTLCertificateSubject && cert.Issuer == DeifiedMeTLCertificateIssuer))
                return true;
            return false;
        }
        public static bool exists(string resource)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(resource);
            request.Method = "HEAD";
            request.Timeout = 30;
            try
            {
                var response = request.GetResponse();
                return true;
            }
            catch (WebException we)
            {
                return false;
            }
        }
        public static string secureGetString(string resource)
        {
            string responseString = "";
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    responseString = client().DownloadString(resource);
                    break;
                }
                catch (Exception)
                {
                    attempts++;
                }
            }
            return responseString;
        }

        public static string insecureGetString(string resource)
        {
            string responseString = "";
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    responseString = client().DownloadString(resource);
                    break;
                }
                catch (Exception)
                {
                    attempts++;
                }
            }
            return responseString;
        }
        public static string securePutData(string uri, byte[] data)
        {
            string responseString = "";
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    responseString = decode(client().UploadData(uri, data));
                    break;
                }
                catch (Exception)
                {
                    //App.Now("Failed securePutData to " + uri);
                    attempts++;
                }
            }
            return responseString;
        }
        public static byte[] secureGetData(string resource)
        {
            byte[] responseBytes = new byte[1];
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    responseBytes = client().DownloadData(resource);
                    break;
                }
                catch (Exception)
                {
                    //App.Now("Failed secureGetData from " + resource);
                    attempts++;
                }
            }
            return responseBytes;
        }
        public static string securePutFile(string uri, string filename)
        {
            string responseString = "";
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    responseString = decode(client().UploadFile(uri, filename));
                    break;
                }
                catch (Exception)
                {
                    attempts++;
                }
            }
            return responseString;
        }
        private static string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }*/
}
