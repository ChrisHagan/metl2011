using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using SandRibbon.Providers;

namespace SandRibbon.Utils.Connection
{
    public class ResourceUploader
    {
        private static readonly string RESOURCE_SERVER_UPLOAD = string.Format("https://{0}:1188/upload_nested.yaws", Constants.JabberWire.SERVER);
        public static string uploadResource(string path, string file)
        {
            return uploadResource(path, file, false);
        }
        public static string uploadResource(string path, string file, bool overwrite)
        {
            try
            {
                var fullPath = string.Format("{0}?path=Resource/{1}&overwrite={2}", RESOURCE_SERVER_UPLOAD, path, overwrite);
                var res = HttpResourceProvider.securePutFile(fullPath,file);
                var url = XElement.Parse(res).Attribute("url").Value;
                return "https://" + url.Split(new[]{"://"}, System.StringSplitOptions.None)[1];
            }
            catch(WebException e)
            {
                MessageBox.Show("Cannot upload resource: " + e.Message);
            }
            return "failed";
        }
        public static string uploadResourceToPath(byte[] resourceData, string path, string name)
        {
            return uploadResourceToPath(resourceData, path, name, true);
        }
        public static string uploadResourceToPath(byte[] resourceData, string path, string name, bool overwrite)
        {
            var url = string.Format("{0}?path={1}&overwrite={2}&filename={3}", RESOURCE_SERVER_UPLOAD, path, overwrite.ToString().ToLower(), name);
            var res = HttpResourceProvider.securePutData(url, resourceData);
            return XElement.Parse(res).Attribute("url").Value;
        }
        /*
         * \Resources
         * \Resources\1011\a.png
         * \Structure
         * \Structure\listing.xml
         * \Structure\myFirstConversation\details.xml
         */
    }
}