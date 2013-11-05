using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using MeTLLib.Providers.Connection;
using System.Xml.Linq;

namespace MeTLLib.Providers
{
    public class MeTLUserInformation
    {
        public string username { private set; get; }
        public string emailAddress { private set; get; }
        public List<KeyValuePair<string, string>> allItems { private set; get; }
        public MeTLUserInformation(string incomingUsername, string incomingEmail, List<KeyValuePair<string,string>> incomingItems)
        {
            username = incomingUsername;
            emailAddress = incomingEmail;
            allItems = incomingItems;
        }
        public static MeTLUserInformation fromXml(XElement input)
        {
            var illegalTrimChars = new List<char>();
            illegalTrimChars.Add('/');
            illegalTrimChars.Add('"');
            var trimChars = illegalTrimChars.ToArray();
            Func<string,string> trimInput = (inputString => (string)inputString.TrimStart(trimChars).TrimEnd(trimChars));
            var username = "not found";
            try
            {
                username = trimInput(input.Descendants("{metl.monash}user").First().Value);
            }
            catch { }
            var emailAddress = "not found";
            try
            {
                var email = input.Descendants("{metl.monash}information").Where(d => d.Attribute("type").Value == "mail").FirstOrDefault();
                if (email != null)
                {
                    emailAddress = trimInput(email.Value);
                }
            }
            catch { }

            var allItems = input.Descendants().Select(d => {
                var key = d.Name.LocalName;
                var type = d.Attribute("type");
                if (type != null && type.Value.Length > 0)
                    key = type.Value;
                return new KeyValuePair<string, string>(key, d.Value);
            }).ToList();
            return new MeTLUserInformation(username,emailAddress,allItems);
        }
        public static MeTLUserInformation emtpy = new MeTLUserInformation("", "", new List<KeyValuePair<string,string>>());
    }
    public interface IUserInformationProvider
    {
        List<MeTLUserInformation> lookupUsers(List<string> usernames);
    }
    public class TestUserInformationProvider : IUserInformationProvider
    {
        public List<MeTLUserInformation> lookupUsers(List<string> usernames)
        {
            return new List<MeTLUserInformation>();
        }
    }
    public class ProductionUserInformationProvider : IUserInformationProvider
    {
        [Inject]
        public MeTLServerAddress server { private get; set; }
        [Inject]
        public IWebClientFactory webClientFactory { private get; set; }

        public List<MeTLUserInformation> lookupUsers(List<string> usernames)
        {
            string url = String.Format("https://{0}:{1}/mldapquery.yaws?usernames={1}",server.host,server.port,usernames.Aggregate("",(acc,item) => acc + "," + item));
            var webClient = webClientFactory.client();
            string xmlString = webClient.downloadString(new System.Uri(url));
            XElement doc = XElement.Parse(xmlString);
            return doc.Descendants("{metl.monash}result").Select(r => MeTLUserInformation.fromXml(r)).ToList();
        }
    }
}
