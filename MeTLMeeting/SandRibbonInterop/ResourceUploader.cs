using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SandRibbon.Utils.Connection
{
    class ResourceUploader
    {
        private static readonly string RESOURCE_SERVER_UPLOAD = string.Format("http://{0}:1188/upload_nested.yaws", JabberWire.SERVER);
        public static string uploadResource(string path, string file)
        {
            return uploadResource(path, file, false);
        }
        public static string uploadResource(string path, string file, bool overwrite)
        {
            try
            {
                var fullPath = string.Format("{0}?path=Resource/{1}&overwrite={2}", RESOURCE_SERVER_UPLOAD, path, overwrite);
                var res = new WebClient().UploadFile(
                    fullPath,
                    file);
                var url = XElement.Parse(Encoding.UTF8.GetString(res)).Attribute("url").Value;
                return url;
            }
            catch(WebException e)
            {
                MessageBox.Show("Cannot upload resource: " + e.Message);
            }
            return "failed"; 
        }
        public static string absolutePath(string relativePath)
        {
            return string.Format("http://{0}:1188{1}", JabberWire.SERVER, relativePath);
        }
        public static string uploadResourceToPath(byte[] resourceData, string path, string name)
        {
            return uploadResourceToPath(resourceData, path, name, true);
        }
        public static string uploadResourceToPath(byte[] resourceData, string path, string name, bool overwrite)
        {
            var url = string.Format("{0}?path={1}&overwrite={2}&filename={3}", RESOURCE_SERVER_UPLOAD, path, overwrite.ToString().ToLower(), name);
            var res = new WebClient().UploadData(url, resourceData);
            return XElement.Parse(Encoding.UTF8.GetString(res)).Attribute("url").Value;
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