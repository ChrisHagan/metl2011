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

        public UserOptions Get(string username) {
            try
            {
                var path = new Uri(resourceUploader.getStemmedPathForResource("userOptions/" + username, "options.xml"));
                var options = Encoding.UTF8.GetString(resourceProvider.secureGetData(path));
                return UserOptions.ReadXml(options);
            }
            catch (Exception e) {
                
                return UserOptions.DEFAULT;
            }
        }
        public void Set(string username, UserOptions options) {
            resourceUploader.uploadResourceToPath(Encoding.UTF8.GetBytes(UserOptions.WriteXml(options)), "userOptions/" + username, "options.xml", true);
        }
    }
}
