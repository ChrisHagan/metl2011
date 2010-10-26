using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System;
using System.Threading;
using System.Diagnostics;
using Ninject;

namespace MeTLLib.Providers.Connection
{
    public class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            //Permissions failure appeared here.
            WebRequest request = (WebRequest)base.GetWebRequest(address);
            request.Timeout = int.MaxValue;
            return request;
        }
    }
    public class MeTLWebClient : IWebClient
    {
        WebClientWithTimeout client;
        public MeTLWebClient(ICredentials credentials)
        {
            this.client = new WebClientWithTimeout();
            this.client.Credentials = credentials;
        }
        public long getSize(Uri resource)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(resource);
            request.Credentials = client.Credentials;
            request.Method = "HEAD";
            request.Timeout = 3000;
            try
            {
                var response = request.GetResponse();
                return response.ContentLength;
            }
            catch (WebException ex)
            {
                return -1;
            }
        }
        public bool exists(Uri resource)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(resource);
            request.Credentials = client.Credentials;
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
        public void downloadStringAsync(Uri resource)
        {
            client.DownloadStringAsync(resource);
        }
        private void retryUpToXTimes(Action action, int attempts)
        {
            while (attempts > 0)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation("retrying (up to {0} times): {1}, exception:  on {2}", attempts, action.Method.ToString(), ex.Message.ToString());
                    attempts--;
                    if (attempts > 0)
                        retryUpToXTimes(action, attempts);
                    else throw new Exception("retry failed on method ("+action.Method.ToString()+")", ex);
                }
            }
        }
        public string downloadString(Uri resource)
        {
            string result = "";
            retryUpToXTimes(delegate { result = client.DownloadString(resource); }, 5);
            return result;
        }
        public byte[] downloadData(Uri resource)
        {
            byte[] result = new byte[0];
            retryUpToXTimes(delegate{ result = client.DownloadData(resource); },5);
            return result;
        }
        public String uploadData(Uri resource, byte[] data)
        {
            string result = "";
            retryUpToXTimes(delegate{ result = decode(client.UploadData(resource.ToString(), data)); },5);
            return result;
        }
        public void uploadDataAsync(Uri resource, byte[] data)
        {
            throw new NotImplementedException();
        }
        public void uploadFileAsync(Uri resource, string filename)
        {
            throw new NotImplementedException();
        }
        byte[] IWebClient.uploadFile(Uri resource, string filename)
        {
            byte[] result = new byte[0];
            retryUpToXTimes(delegate { 
                result = client.UploadFile(resource.ToString(), filename); 
            }, 5);
            return result;
        }
        private string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
    public class MeTLCredentials : NetworkCredential
    {
        private readonly static String USERNAME = "exampleUsername";
        private readonly static String PASSWORD = "examplePassword";
        public MeTLCredentials() : base(USERNAME, PASSWORD) { }
    }
    public class HttpFileUploadResultArgs
    {
        public byte[] Result { get; set; }
    }
    public interface IWebClient
    {
        long getSize(Uri resource);
        bool exists(Uri resource);
        void downloadStringAsync(Uri resource);
        string downloadString(Uri resource);
        byte[] downloadData(Uri resource);
        string uploadData(Uri resource, byte[] data);
        void uploadDataAsync(Uri resource, byte[] data);
        byte[] uploadFile(Uri resource, string filename);
        void uploadFileAsync(Uri resource, string filename);
    }
    public interface IWebClientFactory
    {
        IWebClient client();
    }
    public class WebClientFactory : IWebClientFactory
    {
        private static readonly string StagingMeTLCertificateSubject = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        private static readonly string StagingMeTLCertificateIssuer = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        private static readonly string DeifiedMeTLCertificateSubject = "E=root@deified.adm, CN=localhost, OU=deified, O=adm";
        private static readonly string DeifiedMeTLCertificateIssuer = "E=root@deified.adm, CN=localhost, OU=deified, O=adm";
        private static readonly string ReifierMeTLCertificateSubject = "E=root@reifier.adm.monash.edu.au, CN=localhost, OU=reifier, O=adm.monash.edu.au";
        private static readonly string ReifierMeTLCertificateIssuer = "E=root@reifier.adm.monash.edu.au, CN=localhost, OU=reifier, O=adm.monash.edu.au";

        private static readonly string MonashCertificateSubject = "CN=my.monash.edu.au, OU=ITS, O=Monash University, L=Clayton, S=Victoria, C=AU";
        private static readonly string MonashCertificateIssuer = "E=premium-server@thawte.com, CN=Thawte Premium Server CA, OU=Certification Services Division, O=Thawte Consulting cc, L=Cape Town, S=Western Cape, C=ZA";
        private static readonly string MonashExternalCertificateIssuer = "CN=Thawte SSL CA, O=\"Thawte, Inc.\", C=US";
        private ICredentials credentials;
        public WebClientFactory(ICredentials credentials)
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
            this.credentials = credentials;
        }
        public IWebClient client()
        {
            return new MeTLWebClient(this.credentials);
        }
        private bool bypassAllCertificateStuff(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            if (cert == null) return false;
            if (((HttpWebRequest)sender).Address.Host.Contains("my.monash.edu")) return true;
            if ((cert.Subject == MonashCertificateSubject && (cert.Issuer == MonashCertificateIssuer || cert.Issuer == MonashExternalCertificateIssuer))
                || (cert.Subject == StagingMeTLCertificateSubject && cert.Issuer == StagingMeTLCertificateIssuer)
                //|| (cert.Subject == ReifierMeTLCertificateSubject && cert.Issuer == ReifierMeTLCertificateIssuer)
                || (cert.Subject == DeifiedMeTLCertificateSubject && cert.Issuer == DeifiedMeTLCertificateIssuer))
                return true;
            return false;
        }
    }
    public class HttpResourceProvider
    {
        IWebClientFactory _clientFactory;
        public HttpResourceProvider(IWebClientFactory factory)
        {
            _clientFactory = factory;
        }
        private IWebClient client()
        {
            return _clientFactory.client();
        }
        public long getSize(System.Uri resource)
        {
            return client().getSize(resource);
        }
        public bool exists(System.Uri resource)
        {
            return client().exists(resource);
        }
        public string secureGetString(System.Uri resource)
        {
            return client().downloadString(resource);
        }
        public string insecureGetString(System.Uri resource)
        {
            return client().downloadString(resource);
        }
        public string securePutData(System.Uri uri, byte[] data)
        {
            return client().uploadData(uri, data);
        }
        public byte[] secureGetData(System.Uri resource)
        {
            return client().downloadData(resource);
        }
        public string securePutFile(System.Uri uri, string filename)
        {
            return decode(client().uploadFile(uri, filename));
        }
        private string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
