using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System;

namespace SandRibbon.Providers
{
    public class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = (WebRequest)base.GetWebRequest(address);
            request.Timeout = int.MaxValue;
            return request;
        }
    }
    public class HttpResourceProvider
    {
        private static readonly string StagingMeTLCertificateSubject = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        private static readonly string StagingMeTLCertificateIssuer = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        private static readonly string DeifiedMeTLCertificateSubject = "E=root@deified.adm, CN=localhost, OU=deified, O=adm";
        private static readonly string DeifiedMeTLCertificateIssuer = "E=root@deified.adm, CN=localhost, OU=deified, O=adm";
        private static readonly string ReifierMeTLCertificateSubject = "E=root@reifier.adm, CN=localhost, OU=deified, O=adm";
        private static readonly string ReifierMeTLCertificateIssuer = "E=root@reifier.adm, CN=localhost, OU=deified, O=adm";

        private static readonly string MonashCertificateSubject = "CN=my.monash.edu.au, OU=ITS, O=Monash University, L=Clayton, S=Victoria, C=AU";
        private static readonly string MonashCertificateIssuer = "E=premium-server@thawte.com, CN=Thawte Premium Server CA, OU=Certification Services Division, O=Thawte Consulting cc, L=Cape Town, S=Western Cape, C=ZA";
        private static readonly NetworkCredential MeTLCredentials = new NetworkCredential("exampleUsername", "examplePassword");
        private static bool firstRun = true;

        private static WebClient client()
        {
            if (firstRun)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
                ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
                firstRun = false;
            }
            var wc = new WebClientWithTimeout { Credentials = MeTLCredentials };
            return wc;
        }
        private static bool bypassAllCertificateStuff(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            if (cert == null) return false;
            if ((cert.Subject == MonashCertificateSubject && cert.Issuer == MonashCertificateIssuer)
                || (cert.Subject == StagingMeTLCertificateSubject && cert.Issuer == StagingMeTLCertificateIssuer)
                || (cert.Subject == ReifierMeTLCertificateSubject && cert.Issuer == ReifierMeTLCertificateIssuer)
                || (cert.Subject == DeifiedMeTLCertificateSubject && cert.Issuer == DeifiedMeTLCertificateIssuer))
                return true;
            return false;
        }
        public static bool exists(string resource)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(resource);
            request.Method = "HEAD";
            request.Timeout = 30;
            try
            {
                var response = request.GetResponse();
                return true;
            }
            catch (WebException we)
            {
                return false;
            }
        }
        public static string secureGetString(string resource)
        {
            string responseString = "";
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    responseString = client().DownloadString(resource);
                    break;
                }
                catch (Exception)
                {
                    App.Now("Failed secureGetString from " + resource);
                    attempts++;
                }
            }
            return responseString;
        }

        public static string insecureGetString(string resource)
        {
            string responseString = "";
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    responseString = client().DownloadString(resource);
                    break;
                }
                catch (Exception)
                {
                    App.Now("Failed insecureGetString from " + resource);
                    attempts++;
                }
            }
            return responseString;
        }
        public static string securePutData(string uri, byte[] data)
        {
            string responseString = "";
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    responseString = decode(client().UploadData(uri, data));
                    break;
                }
                catch (Exception)
                {
                    App.Now("Failed securePutData to " + uri);
                    attempts++;
                }
            }
            return responseString;
        }
        public static byte[] secureGetData(string resource)
        {
            byte[] responseBytes = new byte[1];
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    responseBytes = client().DownloadData(resource);
                    break;
                }
                catch (Exception)
                {
                    App.Now("Failed secureGetData from " + resource);
                    attempts++;
                }
            }
            return responseBytes;
        }
        public static string securePutFile(string uri, string filename)
        {
            string responseString = "";
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                responseString = decode(client().UploadFile(uri, filename));
                break;
                }
                catch (Exception)
                {
                    attempts++;
                }
            }
            return responseString;
        }
        private static string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
