using System.Xml.Linq;
using System.Xml;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MeTLLib.Providers.Connection;

namespace MeTLLib.Providers
{
    public class ConfigurationProvider : HttpResourceProvider
    {
        public ConfigurationProvider(IWebClientFactory factory) : base(factory) {}
        private static string server;
        private static string stagingServer;
        private static object instanceLock = new object();
        private static ConfigurationProvider instanceProperty;
        public bool isStaging = false;
        public string SERVER
        {
            get 
            {
                if (isStaging)
                {
                    if (stagingServer == null)
                        stagingServer = XElement.Parse(insecureGetString(new System.Uri(string.Format("http://metl.adm.monash.edu.au/stagingServer.xml")))).Value;
                    server = stagingServer;
                }
                else
                {
                    if (server == null || server == stagingServer)
                        server = XElement.Parse(insecureGetString(new System.Uri("http://metl.adm.monash.edu.au/server.xml"))).Value;
                }
                return server;
            }
        }
        public string getMeTLType()
        {
            var files = Directory.GetFiles(".", "*.exe");
            var docs = new List<string>();
            foreach (string filename in files)
            {
                if (!filename.Contains("vshost"))
                    docs.Add(filename.Substring(2));
            }
            if (docs.Contains("MeTL.exe"))
            {
                docs.Remove("MeTL.exe");
                docs.Add("MeTL.exe");
            }
            return docs.First().Substring(0,docs.First().Length - 4);
        }
        public int getMeTLPedagogyLevel()
        {
            int level;

            switch (getMeTLType())
            {
                case Constants.MeTL:
                    level = 3;
                    break;
                case Constants.MeTlPresenter:
                    level = 2;
                    break;
                case Constants.MeTLCollaborator:
                    level = 3;
                    break;
                case Constants.MeTLDemonstrator:
                    level = 3;
                    break;
                default:
                    level = 3;
                    break;
            }
            return level;
        }
        public string getMetlVersion()
        {
            string MeTLType = getMeTLType();
            var doc = XDocument.Load(MeTLType + ".exe.manifest");
            if (doc != null)
            {
                var node = doc.Root.Descendants().Where(n =>
                        n.Attribute("name") != null && n.Attribute("name").Value.Equals(MeTLType + ".exe")).First();
                if (node != null)
                {
                    var version = node.Attribute("version").Value;
                    return version.ToString();
                }
                return "Unknown";
            }
            return "Unknown";
        }
    }
}
