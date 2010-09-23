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
    class ConfigurationProvider : HttpResourceProvider
    {
        private static string server;
        private static string stagingServer;
        private static object instanceLock = new object();
        private static ConfigurationProvider instanceProperty;
        public bool isStaging = false;
        public static ConfigurationProvider instance
        {
            get{
                lock (instanceLock)
                    if (instanceProperty == null)
                        instanceProperty = new ConfigurationProvider();
                return instanceProperty;
            }
        }
        public string SERVER
        {
            get 
            {
                if (isStaging)
                {
                    if (stagingServer == null)
                        stagingServer = XElement.Parse(HttpResourceProvider.insecureGetString(string.Format("http://metl.adm.monash.edu.au/stagingServer.xml"))).Value;
                    server = stagingServer;
                }
                else
                {
                    if (server == null || server == stagingServer)
                        server = XElement.Parse(HttpResourceProvider.insecureGetString(string.Format("http://metl.adm.monash.edu.au/server.xml"))).Value;
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
                case "MeTL":
                    level = 3;
                    break;
                case "MeTL Presenter":
                    level = 2;
                    break;
                case "MeTL Collaborator":
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
