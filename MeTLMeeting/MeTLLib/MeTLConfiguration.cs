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
        public MeTLServerAddress.serverMode ActiveStackEnum
        {
            get
            {
                return (MeTLServerAddress.serverMode)Enum.Parse(typeof(MeTLServerAddress.serverMode), ActiveStackConfig.Name, true);
            }
        }

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

        [ConfigurationProperty("port", IsRequired = true)]
        public String Port
        {
            get
            {
                return (String)this["port"];
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

        [ConfigurationProperty("authenticationEndpoint", IsRequired = true)]
        public string AuthenticationEndpoint
        {
            get
            {
                return (String)this["authenticationEndpoint"];
            }
            set
            {
                this["authenticationEndpoint"] = value;
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