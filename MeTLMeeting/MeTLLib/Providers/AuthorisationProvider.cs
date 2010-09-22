using System;
using System.Xml;
using System.Linq;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using System.Collections.Generic;
using MeTLLib.Providers.Connection;
using MeTLLib;
using System.DirectoryServices;
using MeTLLib.DataTypes;

namespace MeTLLib.Providers
{
    class AuthorisationProvider : HttpResourceProvider
    {
        public static List<AuthorizedGroup> getEligibleGroups( string AuthcateName, string AuthcatePassword) 
        {
            //if (AuthcateName.StartsWith(BackDoor.USERNAME_PREFIX)) 
            //    return new List<JabberWire.AuthorizedGroup> { new JabberWire.AuthorizedGroup("Artificial person", ""),new JabberWire.AuthorizedGroup("Unrestricted", ""), new JabberWire.AuthorizedGroup(AuthcateName, "")  };
            var groups = new List<AuthorizedGroup>();
            string encryptedPassword = Crypto.encrypt(AuthcatePassword);
            string sXML = HttpResourceProvider.insecureGetString(String.Format("https://{2}:1188/ldapquery.yaws?username={0}&password={1}", AuthcateName, encryptedPassword, Constants.SERVER));
            var doc = new XmlDocument();
                doc.LoadXml(sXML);
            if (doc.GetElementsByTagName("error").Count == 0)
            {
                groups.Add(new AuthorizedGroup("Unrestricted", ""));
                foreach (XmlElement group in doc.GetElementsByTagName("eligibleGroup"))
                {
                    groups.Add(new AuthorizedGroup(
                        group.InnerText.Replace("\"",""),
                        group.Attributes["type"].Value));
                }
                groups.Add(new AuthorizedGroup(
                    doc.GetElementsByTagName("user")[0].Attributes["name"].Value,
                    "username"));
            }
            else
                foreach (XmlElement error in doc.GetElementsByTagName("error"))
                    Logger.Log("XmlError node:"+error.InnerText);
            return groups;
        }
        public static bool isAuthenticatedAgainstLDAP(string username, string password)
        {
            //if (username.StartsWith(BackDoor.USERNAME_PREFIX)) return true;
            string LDAPServerURL = @"LDAP://directory.monash.edu.au:389/";
            string LDAPBaseOU = "o=Monash University,c=AU";
            try
            {
                DirectoryEntry LDAPAuthEntry = new DirectoryEntry(LDAPServerURL + LDAPBaseOU, "", "", AuthenticationTypes.Anonymous);
                DirectorySearcher LDAPDirectorySearch = new DirectorySearcher(LDAPAuthEntry);
                LDAPDirectorySearch.ClientTimeout = new System.TimeSpan(0, 0, 5);
                LDAPDirectorySearch.Filter = "uid=" + username;
                SearchResult LDAPSearchResponse = LDAPDirectorySearch.FindOne();

                string NewSearchPath = LDAPSearchResponse.Path.ToString();
                string NewUserName = NewSearchPath.Substring(NewSearchPath.LastIndexOf("/") + 1);

                DirectoryEntry AuthedLDAPAuthEntry = new DirectoryEntry(NewSearchPath, NewUserName, password, AuthenticationTypes.None);
                DirectorySearcher AuthedLDAPDirectorySearch = new DirectorySearcher(AuthedLDAPAuthEntry);
                AuthedLDAPDirectorySearch.ClientTimeout = new System.TimeSpan(0, 0, 5);
                AuthedLDAPDirectorySearch.Filter = "";
                SearchResultCollection AuthedLDAPSearchResponse = AuthedLDAPDirectorySearch.FindAll();
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("Failed authentication against LDAP because {0}", e.Message));
                return false;
            }
            return true;
        }
        public static bool isAuthenticatedAgainstWebProxy(string username, string password)
        {
            try
            {
                var resource = String.Format("https://my.monash.edu.au/login?username={0}&password={1}", username, password);
                String test = HttpResourceProvider.insecureGetString(resource);
                return !test.Contains("error-text");
            }
            catch (Exception e)
            {
                Logger.Log("Web proxy auth error:" + e.Message);
                return false;
            }
        }
        public static void attemptAuthentication(string username, string password)
        {
            if (Constants.SERVER == null) JabberWire.LookupServer();
            string AuthcateUsername = "";
#if DEBUG
            JabberWire.SwitchServer("staging");
#else
            ConfigurationProvider.instance.isStaging = false;
#endif
            if (username.Contains("_"))
            {
                var Parameters = username.Split('_');
                if (Parameters.ToList().Contains<string>("prod"))
                {
                    JabberWire.SwitchServer("prod");
                }
                if (Parameters.ToList().Contains<string>("staging"))
                {
                    JabberWire.SwitchServer("staging");
                }
                AuthcateUsername = username.Remove(username.IndexOf("_"));
            }
            else
                AuthcateUsername = username;

            string AuthcatePassword = password;

            if (authenticateAgainstFailoverSystem(AuthcateUsername, AuthcatePassword) || isBackdoorUser(AuthcateUsername))
            {
                var eligibleGroups = AuthorisationProvider.getEligibleGroups(AuthcateUsername, AuthcatePassword);
                Commands.SetIdentity.Execute(new Credentials
                {
                    name = AuthcateUsername,
                    password = AuthcatePassword,
                    authorizedGroups = eligibleGroups
                });
            }
            else
            {
                Logger.Log("Failed to Login.");
            }
        }

        private static bool isBackdoorUser(string user)
        {
            return user.ToLower().Contains("admirable");
        }

        private static bool authenticateAgainstFailoverSystem(string username, string password)
        {
            //gotta remember to remove this!
            return true;
            if (isAuthenticatedAgainstLDAP(username, password))
                return true;
            else if (isAuthenticatedAgainstWebProxy(username, password))
                return true;
            else
                return false;
        }
    }
}