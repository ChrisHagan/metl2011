namespace MeTLLib
{
    using System;
    using System.Configuration;
    using System.Diagnostics;

    public static class MeTLConfiguration
    {
        private static MeTLConfigurationSection conf = null;
        public static MeTLConfigurationSection Config 
        { 
            get
            {
                if (conf == null)
                    throw new InvalidOperationException("MeTLConfiguration.Load must be called before using Config");
                else
                    return conf;
            }
            private set
            {
                conf = value; 
            }
        }

        public static void Load()
        {
            try
            {
                Config = ConfigurationManager.GetSection("metlConfigurationGroup/metlConfiguration") as MeTLConfigurationSection;
            }
            catch (ConfigurationErrorsException e)
            {
                Trace.TraceError("Unable to load MeTL Configuration from app.config. Reason: " + e.Message);
            }
        }
    }

    public class MeTLConfigurationSection : ConfigurationSection 
    {
        [ConfigurationProperty("production")]
        public ProductionServerElement Production
        {
            get
            {
                return (ProductionServerElement)this["production"];
            }
            set
            {
                this["production"] = value;
            }
        }

        [ConfigurationProperty("staging")]
        public StagingServerElement Staging
        {
            get
            {
                return (StagingServerElement)this["staging"];
            }
            set
            {
                this["staging"] = value;
            }
        }

        [ConfigurationProperty("external")]
        public ExternalServerElement External
        {
            get
            {
                return (ExternalServerElement)this["external"];
            }
            set
            {
                this["external"] = value;
            }
        }

        [ConfigurationProperty("logging")]
        public ServerElement Logging
        {
            get
            {
                return (ServerElement)this["logging"];
            }
            set
            {
                this["logging"] = value; 
            }
        }

        [ConfigurationProperty("thumbnail")]
        public ServerElement Thumbnail
        {
            get
            {
                return (ServerElement)this["thumbnail"];
            }
            set
            {
                this["thumbnail"] = value; 
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
    }

    public class ProductionServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired=false)]
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

        [ConfigurationProperty("isBootstrapUrl", DefaultValue=true, IsRequired=false)]
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

        [ConfigurationProperty("meggleUrl", DefaultValue="http://meggle-prod.adm.monash.edu:8080/search?query=", IsRequired=true)]
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

        [ConfigurationProperty("host", DefaultValue="http://metl.adm.monash.edu.au/server.xml", IsRequired=true)]
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

    public class StagingServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired=false)]
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

        [ConfigurationProperty("isBootstrapUrl", DefaultValue=true, IsRequired=false)]
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

        [ConfigurationProperty("meggleUrl", DefaultValue="http://meggle-staging.adm.monash.edu:8080/search?query=", IsRequired=true)]
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

        [ConfigurationProperty("host", DefaultValue="http://metl.adm.monash.edu.au/stagingServer.xml", IsRequired=true)]
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

    public class ExternalServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired=false)]
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

        [ConfigurationProperty("isBootstrapUrl", DefaultValue=false, IsRequired=false)]
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

        [ConfigurationProperty("host", DefaultValue="http://civic.adm.monash.edu.au", IsRequired=true)]
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

        [ConfigurationProperty("meggleUrl", DefaultValue="http://meggle-ext.adm.monash.edu:8080/search?query=", IsRequired=true)]
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
    }

    public class ServerElement : ConfigurationElement
    {
        [ConfigurationProperty("host", IsRequired=true)]
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
        [ConfigurationProperty("username", IsRequired=true)]
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

        [ConfigurationProperty("password", IsRequired=true)]
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
}