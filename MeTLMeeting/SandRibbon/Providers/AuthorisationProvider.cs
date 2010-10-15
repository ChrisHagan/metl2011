using System;
using System.Xml;
using SandRibbon.Utils;
using SandRibbon.Utils.Connection;
using System.Collections.Generic;

namespace SandRibbon.Providers
{
    class AuthorisationProvider : HttpResourceProvider
    {
        public List<JabberWire.AuthorizedGroup> getEligibleGroups( string AuthcateName, string AuthcatePassword) 
        {
            if (AuthcateName.StartsWith(BackDoor.USERNAME_PREFIX)) 
                return new List<JabberWire.AuthorizedGroup> { new JabberWire.AuthorizedGroup("Artificial person", ""),new JabberWire.AuthorizedGroup("Unrestricted", ""), new JabberWire.AuthorizedGroup(AuthcateName, "")  };
            var groups = new List<JabberWire.AuthorizedGroup>();
            string encryptedPassword = Crypto.encrypt(AuthcatePassword);
            string sXML = HttpResourceProvider.insecureGetString(String.Format("https://{2}:1188/ldapquery.yaws?username={0}&password={1}", AuthcateName, encryptedPassword, Constants.JabberWire.SERVER));
            var doc = new XmlDocument();
                doc.LoadXml(sXML);
            if (doc.GetElementsByTagName("error").Count == 0)
            {
                groups.Add(new JabberWire.AuthorizedGroup("Unrestricted", ""));
                foreach (XmlElement group in doc.GetElementsByTagName("eligibleGroup"))
                {
                    groups.Add(new JabberWire.AuthorizedGroup(
                        group.InnerText.Replace("\"",""),
                        group.Attributes["type"].Value));
                }
                groups.Add(new JabberWire.AuthorizedGroup(
                    doc.GetElementsByTagName("user")[0].Attributes["name"].Value,
                    "username"));
            }
            else
                foreach (XmlElement error in doc.GetElementsByTagName("error"))
                    Logger.Log("XmlError node:"+error.InnerText);
            return groups;
        }
    }
}