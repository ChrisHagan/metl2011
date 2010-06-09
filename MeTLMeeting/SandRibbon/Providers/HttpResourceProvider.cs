using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace SandRibbon.Providers
{
    public class LaxCertificatePolicy : ICertificatePolicy {
        public bool CheckValidationResult(ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            return true;
        }
    }
    public class HttpResourceProvider
    {
        private static WebClient client()
        {
            var wc = new WebClient { Credentials = new NetworkCredential("exampleUsername", "examplePassword") };
            return wc;
        }
        private static bool bypassAllCertificateStuff(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }
        public static string secureGetString(string resource)
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
            return client().DownloadString(resource);
        }
        public static string insecureGetString(string resource)
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
            return new WebClient().DownloadString(resource);
        }
        public static string securePutData(string uri, byte[] data)
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
            return decode(client().UploadData(uri, data));
        }
        public static byte[] secureGetData(string resource)
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
            return client().DownloadData(resource);
        }
        public static string securePutFile(string uri, string filename)
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
            return decode(client().UploadFile(uri, filename));
        }
        private static string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
