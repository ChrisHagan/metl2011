using System.Net;

namespace SandRibbon.Providers
{
    public class HttpResourceProvider
    {
        private static WebClient client()
        {
            return new WebClient { Credentials = new NetworkCredential("exampleUsername","examplePassword")};
        }
        public static string secureGetString(string resource)
        {
            return client().DownloadString(resource);
        }
        public static string insecureGetString(string resource)
        {
            return new WebClient().DownloadString(resource);
        }
        public static string securePutData(string uri, byte[] data)
        {
            return decode(client().UploadData(uri, data));
        }
        public static byte[] secureGetData(string resource)
        {
            return client().DownloadData(resource);
        }
        public static string securePutFile(string uri, string filename)
        {
            return decode(client().UploadFile(uri, filename));
        }
        private static string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
