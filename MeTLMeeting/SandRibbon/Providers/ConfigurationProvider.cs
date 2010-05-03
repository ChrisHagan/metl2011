using System.Xml.Linq;

namespace SandRibbon.Providers
{
    public class ConfigurationProvider : HttpResourceProvider
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
    }
}
