using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using MeTLLib.Providers.Connection;
using System.Xml.Linq;
using MeTLLib.DataTypes;

namespace MeTLLib.Providers
{
    public class UserOptionsProvider
    {
        [Inject]
        public HttpResourceProvider resourceProvider{private get;set;}
        [Inject]
        public MeTLServerAddress serverAddress { private get; set; }
        [Inject]
        public IResourceUploader resourceUploader { private get; set; }
        private string OPTIONS_PATH = "https://{0}:1188/userOptions/{1}/options.xml";
        private IEnumerable<string> possibleOptions = "importResolution logLevel pedagogyLevel".Split(' ');

        public UserOptions Get(string username) {
            try
            {
                var options = Encoding.UTF8.GetString(resourceProvider.secureGetData(new Uri(string.Format(OPTIONS_PATH, serverAddress.secureUri, username))));
                return UserOptions.ReadXml(options);
            }
            catch (Exception) {
                return UserOptions.DEFAULT;
            }
        }
        public void Set(string username, UserOptions options) {
            resourceUploader.uploadResourceToPath(Encoding.UTF8.GetBytes(UserOptions.WriteXml(options)), "userOptions/" + username, "options.xml", true);
        }
    }
}
