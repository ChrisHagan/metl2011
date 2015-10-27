namespace MeTLLib
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;

    public class MetlConfiguration
    {
        public MetlConfiguration(
            string _name,
            string _imageUrl,
            string _xmppHost,
            string _xmppPort,
            string _xmppDomain,
            string _xmppUsername,
            string _xmppPassword,
            string _conversationsUrl,
            string _authenticationUrl,
            string _thumbnailUrl,
            string _resourceUrl,
            string _historyUrl,
            string _resourceUsername,
            string _resourcePassword,
            string _structureDirectory,
            string _resourceDirectory,
            string _uploadPath,
            string _primaryKeyGenerator,
            string _cryptoKey,
            string _cryptoIV
       )
        {
            name = _name;
            imageUrl = _imageUrl;
            xmppHost = _xmppHost;
            xmppPort = _xmppPort;
            xmppDomain = _xmppDomain;
            xmppUsername = _xmppUsername;
            xmppPassword = _xmppPassword;
            conversationsUrl = _conversationsUrl;
            authenticationUrl = _authenticationUrl;
            thumbnailUrl = _thumbnailUrl;
            resourceUrl = _resourceUrl;
            historyUrl = _historyUrl;
            resourceUsername = _resourceUsername;
            resourcePassword = _resourcePassword;
            structureDirectory = _structureDirectory;
            resourceDirectory = _resourceDirectory;
            uploadPath = _uploadPath;
            primaryKeyGenerator = _primaryKeyGenerator;
            cryptoKey = _cryptoKey;
            cryptoIV = _cryptoIV;
        }
        public string name { get; protected set; }
        public string imageUrl { get; protected set; }
        public string xmppHost { get; protected set; }
        public string xmppPort { get; protected set; }
        public string xmppDomain { get; protected set; }
        public string xmppUsername { get; protected set; }
        public string xmppPassword { get; protected set; }
        public string conversationsUrl { get; protected set; }
        public string authenticationUrl { get; protected set; }
        public string thumbnailUrl { get; protected set; }
        public string resourceUrl { get; protected set; }
        public string historyUrl { get; protected set; }
        public string resourceUsername { get; protected set; }
        public string resourcePassword { get; protected set; }
        public string resourceDirectory { get; protected set; }
        public string structureDirectory { get; protected set; }
        public string uploadPath { get; protected set; }
        public string primaryKeyGenerator { get; protected set; }
        public string cryptoKey { get; protected set; }
        public string cryptoIV { get; protected set; }
        public string muc
        {
            get { return "conference." + xmppDomain; }
        }
        public string globalMuc
        {
            get { return "global@" + muc; }
        }
        public static readonly MetlConfiguration empty = new MetlConfiguration("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
    }
    public abstract class MetlConfigurationManager
    {
        public MetlConfigurationManager()
        {
            loadConfigs();
        }
        public List<MetlConfiguration> Configs
        {
            get;
            protected set;
        }
        protected abstract void loadConfigs();
        public void reload()
        {
            loadConfigs();
        }
    }
    public class LocalAppMeTLConfigurationManager : MetlConfigurationManager
    {
        override protected void loadConfigs()
        {
            deprecatedLib.MeTLConfiguration.Load();
            var config = deprecatedLib.MeTLConfiguration.Config;
            Configs = new List<deprecatedLib.StackServerElement> { config.Production, config.Staging, config.External }.Select(conf =>
              {
                  return new MetlConfiguration(
                      conf.Name,
                      conf.Image,
                          conf.Host,
                          conf.XmppPort,
                          conf.xmppServiceName,
                          config.XmppCredential.Username,
                          config.XmppCredential.Password,
                          conf.MeggleUrl,
                          conf.WebAuthenticationEndpoint,
                          conf.Thumbnail,
                          String.Format("{0}://{1}:{2}", conf.Protocol, conf.Host, conf.ResourcePort),
                          String.Format("{0}://{1}:{2}", conf.Protocol, conf.Host, conf.HistoryPort),
                          config.ResourceCredential.Username,
                          config.ResourceCredential.Password,
                          "Structure",
                          "Resources",
                          conf.UploadEndpoint,
                          "primarykey.yaws",
                          config.Crypto.Key,
                          config.Crypto.IV
                      );
              }).ToList();
        }
    }
}

namespace deprecatedLib
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using MeTLLib;
    using System.Linq;
    public static class MeTLConfiguration
    {
        private static MeTLConfigurationSection conf = null;
        public static MeTLConfigurationSection Config
        {
            get
            {
                if (conf == null)
                {
                    throw new InvalidOperationException("MeTLConfiguration.Load must be called before using Config");
                }
                else
                {
                    return conf;
                }
            }
            private set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("MeTLConfiguration cannot be set to a null value");
                }
                conf = value;
            }
        }

        public static void Load()
        {
            try
            {
                var section = ConfigurationManager.GetSection("metlConfigurationGroup/metlConfiguration");
                var mc = section as MeTLConfigurationSection;
                if (mc != null)
                {
                    Config = mc;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Unable to load MeTL Configuration from app.config. Reason: " + e.Message);
                throw e;
            }
        }
    }

    public class MeTLConfigurationSection : ConfigurationSection
    {
        /*
        public MeTLServerAddress.serverMode ActiveStackEnum
        {
            get
            {
                return (MeTLServerAddress.serverMode)Enum.Parse(typeof(MeTLServerAddress.serverMode), ActiveStackConfig.Name, true);
            }
        }
        */
        public StackServerElement ActiveStack
        {
            get
            {
                var active = ActiveStackConfig.Name;
                if (Properties.Contains(active))
                    return (StackServerElement)this[active];
                else
                    throw new ArgumentException(String.Format("Active Stack Server {0} specified not found", active));
            }
        }

        [ConfigurationProperty("activeStackConfig")]
        public ActiveStackElement ActiveStackConfig
        {
            get
            {
                return (ActiveStackElement)this["activeStackConfig"];
            }
            set
            {
                this["activeStackConfig"] = value;
            }
        }

        [ConfigurationProperty("production")]
        public StackServerElement Production
        {
            get
            {
                return (StackServerElement)this["production"];
            }
            set
            {
                this["production"] = value;
            }
        }

        [ConfigurationProperty("staging")]
        public StackServerElement Staging
        {
            get
            {
                return (StackServerElement)this["staging"];
            }
            set
            {
                this["staging"] = value;
            }
        }

        [ConfigurationProperty("external")]
        public StackServerElement External
        {
            get
            {
                return (StackServerElement)this["external"];
            }
            set
            {
                this["external"] = value;
            }
        }



        [ConfigurationProperty("resourceCredential")]
        public CredentialElement ResourceCredential
        {
            get
            {
                return (CredentialElement)this["resourceCredential"];
            }
            set
            {
                this["resourceCredential"] = value;
            }
        }

        [ConfigurationProperty("xmppCredential")]
        public CredentialElement XmppCredential
        {
            get
            {
                return (CredentialElement)this["xmppCredential"];
            }
            set
            {
                this["xmppCredential"] = value;
            }
        }

        [ConfigurationProperty("crypto")]
        public CryptoElement Crypto
        {
            get
            {
                return (CryptoElement)this["crypto"];
            }
            set
            {
                this["crypto"] = value;
            }
        }
    }

    public class StackServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = false)]
        public String Name
        {
            get
            {
                return (String)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }
        [ConfigurationProperty("imageUrl", IsRequired = false)]
        public String Image
        {
            get
            {
                return (String)this["imageUrl"];
            }
            set
            {
                this["imageUrl"] = value;
            }
        }

        [ConfigurationProperty("webAuthenticationEndpoint", IsRequired = true)]
        public String WebAuthenticationEndpoint
        {
            get
            {
                return (String)this["webAuthenticationEndpoint"];
            }
            set
            {
                this["webAuthenticationEndpoint"] = value;
            }
        }
        [ConfigurationProperty("isBootstrapUrl", IsRequired = true)]
        public Boolean IsBootstrapUrl
        {
            get
            {
                return (Boolean)this["isBootstrapUrl"];
            }
            set
            {
                this["isBootstrapUrl"] = value;
            }
        }

        [ConfigurationProperty("meggleUrl", IsRequired = true)]
        public String MeggleUrl
        {
            get
            {
                return (String)this["meggleUrl"];
            }
            set
            {
                this["meggleUrl"] = value;
            }
        }
        [ConfigurationProperty("thumbnail")]
        public String Thumbnail
        {
            get
            {
                return (String)this["thumbnail"];
            }
            set
            {
                this["thumbnail"] = value;
            }
        }
        [ConfigurationProperty("protocol", IsRequired = true)]
        public String Protocol
        {
            get
            {
                return (String)this["protocol"];
            }
            set
            {
                this["protocol"] = value;
            }
        }
        [ConfigurationProperty("host", IsRequired = true)]
        public String Host
        {
            get
            {
                return (String)this["host"];
            }
            set
            {
                this["host"] = value;
            }
        }

        [ConfigurationProperty("xmppPort", IsRequired = true)]
        public String XmppPort
        {
            get
            {
                return (String)this["xmppPort"];
            }
            set
            {
                this["port"] = value;
            }
        }
        [ConfigurationProperty("historyPort", IsRequired = true)]
        public String HistoryPort
        {
            get
            {
                return (String)this["historyPort"];
            }
            set
            {
                this["port"] = value;
            }
        }
        [ConfigurationProperty("resourcePort", IsRequired = true)]
        public String ResourcePort
        {
            get
            {
                return (String)this["resourcePort"];
            }
            set
            {
                this["port"] = value;
            }
        }
        [ConfigurationProperty("xmppServiceName", IsRequired = true)]
        public String xmppServiceName
        {
            get
            {
                return (String)this["xmppServiceName"];
            }
            set
            {
                this["xmppServiceName"] = value;
            }
        }
        [ConfigurationProperty("uploadEndpoint", IsRequired = true)]
        public string UploadEndpoint
        {
            get
            {
                return (String)this["uploadEndpoint"];
            }
            set
            {
                this["uploadEndpoint"] = value;
            }
        }
    }

    public class ServerElement : ConfigurationElement
    {
        [ConfigurationProperty("host", IsRequired = true)]
        public String Host
        {
            get
            {
                return (String)this["host"];
            }
            set
            {
                this["host"] = value;
            }
        }
    }

    public class CredentialElement : ConfigurationElement
    {
        [ConfigurationProperty("username", IsRequired = true)]
        public String Username
        {
            get
            {
                return (String)this["username"];
            }
            set
            {
                this["username"] = value;
            }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public String Password
        {
            get
            {
                return (String)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }
    }

    public class ActiveStackElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public String Name
        {
            get
            {
                return (String)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }
    }

    public class CryptoElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public String Key
        {
            get
            {
                return (String)this["key"];
            }
            set
            {
                this["key"] = value;
            }
        }

        [ConfigurationProperty("iv", IsRequired = true)]
        public String IV
        {
            get
            {
                return (String)this["iv"];
            }
            set
            {
                this["iv"] = value;
            }
        }
    }
}

