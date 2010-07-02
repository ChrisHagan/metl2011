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
        private static readonly string MeTLCertificateSubject = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        private static readonly string MeTLCertificateIssuer = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        private static readonly string MonashCertificateSubject = "CN=my.monash.edu.au, OU=ITS, O=Monash University, L=Clayton, S=Victoria, C=AU";
        private static readonly string MonashCertificateIssuer = "E=premium-server@thawte.com, CN=Thawte Premium Server CA, OU=Certification Services Division, O=Thawte Consulting cc, L=Cape Town, S=Western Cape, C=ZA";
        private static bool firstRun = true;
        private static WebClient client()
        {
            if (firstRun)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
                ServicePointManager.DefaultConnectionLimit = 10;
                firstRun = false;
            }
            return new WebClient { Credentials = new NetworkCredential("exampleUsername", "examplePassword") };
        }
        private static bool bypassAllCertificateStuff(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            if ((cert.Subject == MonashCertificateSubject && cert.Issuer == MonashCertificateIssuer)
                ||(cert.Subject == MeTLCertificateSubject && cert.Issuer == MeTLCertificateIssuer))
                return true;
            return false;
        }
        public static string secureGetString(string resource)
        {
            return client().DownloadString(resource);
        }
        public static string insecureGetString(string resource)
        {
            return client().DownloadString(resource);
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
