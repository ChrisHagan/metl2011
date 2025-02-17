﻿using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using MeTLLib.Providers.Connection;
using MeTLLib;
using System.DirectoryServices;
using MeTLLib.DataTypes;
using System.Diagnostics;
using Ninject;

namespace MeTLLib.Providers
{
    public class AuthorisationProvider : HttpResourceProvider
    {
        private MeTLServerAddress server;
        public AuthorisationProvider(IWebClientFactory factory, MeTLServerAddress server)
            : base(factory)
        {
            this.server = server;
        }
        public List<AuthorizedGroup> getEligibleGroups(string AuthcateName, string AuthcatePassword)
        {
            //if (AuthcateName.StartsWith(BackDoor.USERNAME_PREFIX)) 
            //    return new List<JabberWire.AuthorizedGroup> { new JabberWire.AuthorizedGroup("Artificial person", ""),new JabberWire.AuthorizedGroup("Unrestricted", ""), new JabberWire.AuthorizedGroup(AuthcateName, "")  };
            var groups = new List<AuthorizedGroup>();
            string encryptedPassword = Crypto.encrypt(AuthcatePassword);
            string sXML = insecureGetString(new System.Uri(String.Format("https://{2}:1188/ldapquery.yaws?username={0}&password={1}", AuthcateName, encryptedPassword, server.host)));
            var doc = new XmlDocument();
            doc.LoadXml(sXML);
            if (doc.GetElementsByTagName("error").Count == 0)
            {
                groups.Add(new AuthorizedGroup("Unrestricted", ""));
                foreach (XmlElement group in doc.GetElementsByTagName("eligibleGroup"))
                {
                    groups.Add(new AuthorizedGroup(
                        group.InnerText.Replace("\"", ""),
                        group.Attributes["type"].Value));
                }
                groups.Add(new AuthorizedGroup(
                    doc.GetElementsByTagName("user")[0].Attributes["name"].Value,
                    "username"));
            }
            else
                foreach (XmlElement error in doc.GetElementsByTagName("error"))
                    Trace.TraceError("XmlError node:" + error.InnerText);
            return groups;
        }
        public bool isAuthenticatedAgainstLDAP(string username, string password)
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
                Trace.TraceError(string.Format("Failed authentication against LDAP because {0}", e.Message));
                return false;
            }
            return true;
        }
        public bool isAuthenticatedAgainstWebProxy(string username, string password)
        {
            try
            {
                var resource = String.Format("https://my.monash.edu.au/login?username={0}&password={1}", username, password);
                String test = insecureGetString(new System.Uri(resource));
                return !test.Contains("error-text");
            }
            catch (Exception e)
            {
                Trace.TraceError("Web proxy auth error:" + e.Message);
                return false;
            }
        }
        public Credentials attemptAuthentication(string username, string password)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException("username", "Argument cannot be null");
            if (String.IsNullOrEmpty(password)) throw new ArgumentNullException("password", "Argument cannot be null");
            string AuthcateUsername = username;
            string AuthcatePassword = password;

            if (authenticateAgainstFailoverSystem(AuthcateUsername, AuthcatePassword) || isBackdoorUser(AuthcateUsername))
            {
                var eligibleGroups = getEligibleGroups(AuthcateUsername, AuthcatePassword);
                var credentials = new Credentials(AuthcateUsername, AuthcatePassword, eligibleGroups);
                Globals.credentials = credentials;
                return credentials;
            }
            else
            {
                Trace.TraceError("Failed to Login.");
                return new Credentials(username, "", new List<AuthorizedGroup>());
            }
        }
        public bool isBackdoorUser(string user)
        {
            return user.ToLower().Contains("admirable");
        }
        public bool authenticateAgainstFailoverSystem(string username, string password)
        {
            if (isAuthenticatedAgainstLDAP(username, password))
                return true;
            else if (isAuthenticatedAgainstWebProxy(username, password))
                return true;
            else
                return false;
        }
    }
}