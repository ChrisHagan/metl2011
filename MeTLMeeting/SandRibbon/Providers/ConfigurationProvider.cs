using System.Windows;
using System.Xml.Linq;
using System.Xml;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SandRibbon.Components.Sandpit;
using SandRibbon.Components.Pedagogicometry;

namespace SandRibbon.Providers
{
    public class ConfigurationProvider
    {
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
        public PedagogyLevel getMeTLPedagogyLevel()
        {
            int level;
            var type = getMeTLType();
            switch (type)
            {
                case Globals.METL:
                    level = 2;
                    break;
                case Globals.METLPRESENTER:
                    level = 2;
                    break;
                case Globals.METLCOLLABORATOR:
                    level = 3;
                    break;
                case Globals.METLDEMONSTRATOR:
                    level = 3;
                    break;
                default:
                    level = 2;
                    break;
            }
            Commands.MeTLType.ExecuteAsync(type);
            return Pedagogicometer.level(level);
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
