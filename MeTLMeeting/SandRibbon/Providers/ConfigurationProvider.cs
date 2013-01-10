using System.Xml.Linq;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SandRibbon.Components.Sandpit;
using SandRibbon.Components.Pedagogicometry;
using System.Reflection;

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
            var executableNames = new List<String>();

            executableNames.Add(Assembly.GetEntryAssembly().Location);
            executableNames.Add(Assembly.GetExecutingAssembly().Location);

            return Path.GetFileNameWithoutExtension(executableNames.Find((location) => { return !String.IsNullOrEmpty(location); }));
        }
        public PedagogyLevel getMeTLPedagogyLevel()
        {
            return Pedagogicometer.level(PedagogyCode.CollaborativePresentation);
        }
        private string metlVersion = "Unknown";
        public string getMetlVersion()
        {
            if (metlVersion == "Unknown")
            {
                try
                {
                    string MeTLType = getMeTLType();
                    var doc = XDocument.Load(MeTLType + ".exe.manifest");
                    if (doc != null)
                    {
                        var node = doc.Root.Descendants().Where(n =>
                                n.Attribute("name") != null && n.Attribute("name").Value.Equals(MeTLType + ".exe")).FirstOrDefault();
                        if (node != null)
                        {
                            metlVersion = node.Attribute("version").Value.ToString();
                        }
                    }
                }
                catch (Exception) { 
                    //Don't log it, logging it calls this
                }
            }
            return metlVersion;
        }
    }
}
