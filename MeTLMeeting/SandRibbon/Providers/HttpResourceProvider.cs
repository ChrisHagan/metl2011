using System.Net;
using System.Security.Cryptography.X509Certificates;
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
                ServicePointManager.DefaultConnectionLimit = 10;
                firstRun = false;
            }
            var wc = new WebClientWithTimeout { Credentials = MeTLCredentials };
            return wc;
        }
        private static HttpWebRequest request(string uri, string webmethod)
        {
            if (firstRun)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
                ServicePointManager.DefaultConnectionLimit = 10;
                firstRun = false;
            }
            var hwq = (HttpWebRequest)WebRequest.Create(new System.Uri(uri));
            hwq.Method = webmethod;
            hwq.Credentials = MeTLCredentials;

            hwq.Timeout = -1;
            return hwq;
        }
        private static bool bypassAllCertificateStuff(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            if ((cert.Subject == MonashCertificateSubject && cert.Issuer == MonashCertificateIssuer)
                || (cert.Subject == StagingMeTLCertificateSubject && cert.Issuer == StagingMeTLCertificateIssuer)
                || (cert.Subject == ReifierMeTLCertificateSubject && cert.Issuer == ReifierMeTLCertificateIssuer)
                || (cert.Subject == DeifiedMeTLCertificateSubject && cert.Issuer == DeifiedMeTLCertificateIssuer))
                return true;
            return false;
        }
        public static string secureGetString(string resource)
        {
            /*
            var response = request(resource, "GET").GetResponse();
            var responseStream = response.GetResponseStream();
            var responseBuffer = new byte[response.ContentLength];
            responseStream.Read(responseBuffer, 0, Convert.ToInt32(response.ContentLength));
            var responseString = decode(responseBuffer);
            return responseString;
            */
            return client().DownloadString(resource);
        }
        public static string insecureGetString(string resource)
        {
            /*
            var response = request(resource, "GET").GetResponse();
            var responseStream = response.GetResponseStream();
            var responseBuffer = new byte[response.ContentLength];
            responseStream.Read(responseBuffer, 0, Convert.ToInt32(response.ContentLength));
            var responseString = decode(responseBuffer);
            return responseString;
            */
            return client().DownloadString(resource);
        }
        public static string securePutData(string uri, byte[] data)
        {
            /*
            var rq = request(uri, "POST");
            rq.ContentLength = data.Length;
            System.IO.Stream contentStream = rq.GetRequestStream();
            contentStream.Write(data, 0, data.Length);
            contentStream.Close();
            var response = rq.GetResponse();
            var responseBuffer = new byte[response.ContentLength];
            var responseStream = response.GetResponseStream();
            responseStream.Read(responseBuffer, 0, Convert.ToInt32(response.ContentLength));
            var responseString = decode(responseBuffer);
            return responseString;
            */
            return decode(client().UploadData(uri, data));
        }
        public static byte[] secureGetData(string resource)
        {
            /*var response = request(resource, "GET").GetResponse();
            var responseStream = response.GetResponseStream();
            var responseBuffer = new byte[response.ContentLength];
            responseStream.Read(responseBuffer, 0, Convert.ToInt32(response.ContentLength));
            return responseBuffer;
//          */
            return client().DownloadData(resource);
        }
        public static string securePutFile(string uri, string filename)
        {
            /*
            var data = System.IO.File.ReadAllBytes(filename);
            var rq = request(uri, "POST");
            rq.ContentType = "message/external-body";
            rq.ContentLength = data.Length;
            System.IO.Stream contentStream = rq.GetRequestStream();
            contentStream.Write(data, 0, data.Length);
            contentStream.Close();

            var response = rq.GetResponse();

            return response.GetResponseStream().ToString();
            */
            return decode(client().UploadFile(uri, filename));
        }
        private static string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
