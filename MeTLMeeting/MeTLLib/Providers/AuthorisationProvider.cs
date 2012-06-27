using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using MeTLLib.Providers.Connection;
using MeTLLib;
using System.DirectoryServices;
using MeTLLib.DataTypes;
using System.Diagnostics;
using Ninject;
using System.Text;
using System.Xml.Linq;
using System.Threading;

namespace MeTLLib.Providers
{
    public class AuthorisationProvider : HttpResourceProvider
    {
        private MeTLServerAddress server;
        private IWebClientFactory webclientFactory;
        public AuthorisationProvider(IWebClientFactory factory, MeTLServerAddress server)
            : base(factory)
        {
            this.webclientFactory = factory;
            this.server = server;
        }
        public Credentials attemptAuthentication(string username, string password)
        {
            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                string AuthcateUsername = username;
                string AuthcatePassword = password;
                var token = login(AuthcateUsername, AuthcatePassword);
                if (token.authenticated)
                {
                    var eligibleGroups = token.groups;
                    var credentials = new Credentials(AuthcateUsername, AuthcatePassword, eligibleGroups, token.mail);
                    Globals.credentials = credentials;
                    return credentials;
                }
            }

            Trace.TraceError("Failed to Login.");
            return Credentials.Empty;
        }
        public class AuthToken
        {
            public AuthToken(string Username)
            {
                username = Username;
            }
            public string username { get; private set; }
            public List<AuthorizedGroup> groups = new List<AuthorizedGroup>();
            public bool authenticated = false;
            public List<String> errors = new List<String>();
            public string mail { get; set; }
        }
        public AuthToken login(string AuthcateName, string AuthcatePassword)
        {
            var token = new AuthToken(AuthcateName);
            string encryptedPassword = Crypto.encrypt(AuthcatePassword);
            string sXML = insecureGetString(new Uri(String.Format("https://{2}:1188/authentication.yaws?username={0}&password={1}", AuthcateName, encryptedPassword, server.host),UriKind.RelativeOrAbsolute));
            var doc = new XmlDocument();
            if (String.IsNullOrEmpty(sXML))
            {
                Trace.TraceInformation("Authentication Error: Server returned no data");
                return token;
            }
            doc.LoadXml(sXML);
            if (doc.GetElementsByTagName("error").Count == 0 && doc.GetElementsByTagName("eligibleConversationGroups").Count > 0)
            {
                token.groups.Add(new AuthorizedGroup("Unrestricted", ""));
                foreach (XmlElement group in doc.GetElementsByTagName("eligibleGroup"))
                {
                    token.groups.Add(new AuthorizedGroup(
                        group.InnerText.Replace("\"", ""),
                        group.Attributes["type"].Value));
                }
                token.groups.Add(new AuthorizedGroup(
                    doc.GetElementsByTagName("user")[0].Attributes["name"].Value,
                    "username"));
                foreach (XmlElement group in doc.GetElementsByTagName("information"))
                {
                    var mail = group.Attributes["type"];
                    if (mail != null && mail.Value == "mail")
                    {
                        token.mail = group.InnerText.Replace("\"", "");
                        break;
                    }
                }
                token.authenticated = true;
            }
            else
            {
                token.authenticated = false;
                if (doc.GetElementsByTagName("eligibleConversationGroups").Count == 0)
                {
                    Trace.TraceInformation("Authentication Error: No eligibleConversationGroups");
                }
                foreach (XmlElement error in doc.GetElementsByTagName("error"))
                {
                    Trace.TraceInformation("Authentication XmlError node:" + error.InnerText);
                    if (!String.IsNullOrEmpty(error.OuterXml)) token.errors.Add(error.OuterXml);
                }
            }
            return token;
        }
    }
}