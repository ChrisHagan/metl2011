using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System;
using System.Threading;
using System.Diagnostics;
using MeTLLib.DataTypes;
//using Ninject;

namespace MeTLLib.Providers.Connection
{
    public class WebClientWithTimeout : WebClient
    {
        protected Credentials metlCreds;
        public WebClientWithTimeout(Credentials _metlCreds)
        {
            metlCreds = _metlCreds;
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);                                    
            request.Headers.Add(HttpRequestHeader.Cookie, metlCreds.cookie);
            request.KeepAlive = false;
            request.Timeout = int.MaxValue;
            return request;
        }
    }
    public class MeTLWebClient : IWebClient
    {
        WebClientWithTimeout client;
        public MeTLWebClient(ICredentials credentials,Credentials metlCreds)
        {
            this.client = new WebClientWithTimeout(metlCreds);
            this.client.Credentials = credentials;
            this.client.Proxy = null;            
        }
        public long getSize(Uri resource)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(resource);
            request.Credentials = client.Credentials;
            request.KeepAlive = false;
            request.Method = "HEAD";
            request.Timeout = 3000;
            try
            {
                var response = request.GetResponse();
                return response.ContentLength;
            }
            catch (WebException)
            {
                return -1;
            }
        }
        public bool exists(Uri resource)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(resource);
            request.Credentials = client.Credentials;
            request.KeepAlive = false;
            request.Method = "HEAD";
            // use the default timeout
            //request.Timeout = 5 * 1000;
            try
            {
                var response = request.GetResponse();
                return true;
            }
            catch (WebException)
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
            action();
        }
        public string downloadString(Uri resource)
        {
            return client.DownloadString(resource);
        }
        public byte[] downloadData(Uri resource)
        {
            try
            {
                return client.DownloadData(resource);
            }
            catch (WebException e)
            {
                if (e.Message.Contains("404")) { return new byte[0]; }
                Trace.TraceError("HttpResourceProvider download data exception: {1} {0}", e.Message, resource.AbsoluteUri);
                throw e;
            }
        }
        public String uploadData(Uri resource, byte[] data)
        {
            return decode(client.UploadData(resource.ToString(), data));
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
            var safeFile = filename;
            if (filename.StartsWith("file:///"))
            {
                safeFile = filename.Substring(8);
            }
            return client.UploadFile(resource.ToString(), safeFile);
        }
        private string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
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
        //private static readonly string StagingMeTLCertificateSubject = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        //private static readonly string StagingMeTLCertificateIssuer = "E=nobody@nowhere.gondwanaland, CN=localhost, OU=Janitorial section, O=Hyber Inc., L=Yawstown, S=Gondwanaland, C=se";
        //private static readonly string DeifiedMeTLCertificateSubject = "E=root@deified.adm, CN=localhost, OU=deified, O=adm";
        //private static readonly string DeifiedMeTLCertificateIssuer = "E=root@deified.adm, CN=localhost, OU=deified, O=adm";
        //private static readonly string ReifierMeTLCertificateSubject = "E=root@reifier.adm.monash.edu.au, CN=localhost, OU=reifier, O=adm.monash.edu.au";
        //private static readonly string ReifierMeTLCertificateIssuer = "E=root@reifier.adm.monash.edu.au, CN=localhost, OU=reifier, O=adm.monash.edu.au";

        //private static readonly string MonashCertificateSubject = "CN=my.monash.edu.au, OU=ITS, O=Monash University, L=Clayton, S=Victoria, C=AU";
        //private static readonly string MonashCertificateIssuer = "E=premium-server@thawte.com, CN=Thawte Premium Server CA, OU=Certification Services Division, O=Thawte Consulting cc, L=Cape Town, S=Western Cape, C=ZA";
        //private static readonly string MonashExternalCertificateIssuer = "CN=Thawte SSL CA, O=\"Thawte, Inc.\", C=US";
        protected ICredentials credentials;
        protected Credentials metlCreds;
        public WebClientFactory(ICredentials credentials, IAuditor auditor,Credentials _metlCreds)
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            this.metlCreds = _metlCreds;
            /*Ssl3 is not compatible with modern servers and IE*/
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            /*This would be a workaround but is not required.  We permit engine to select algorithm.*/
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            this.credentials = credentials;
        }
        public IWebClient client()
        {
            return new MeTLWebClient(this.credentials,metlCreds);
        }
        private bool bypassAllCertificateStuff(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
            if (cert == null) return false;
            if (!(sender is HttpWebRequest)) return true;
            //if (((HttpWebRequest)sender).Address.Host.Contains("my.monash.edu")) return true;
            /*
            if ((cert.Subject == MonashCertificateSubject && (cert.Issuer == MonashCertificateIssuer || cert.Issuer == MonashExternalCertificateIssuer))
                || (cert.Subject == StagingMeTLCertificateSubject && cert.Issuer == StagingMeTLCertificateIssuer)
                //|| (cert.Subject == ReifierMeTLCertificateSubject && cert.Issuer == ReifierMeTLCertificateIssuer)
                || (cert.Subject == DeifiedMeTLCertificateSubject && cert.Issuer == DeifiedMeTLCertificateIssuer))*/
            return true;
            //return false;
        }
    }
    public class HttpResourceProvider
    {
        IWebClientFactory _clientFactory;
        IAuditor _auditor;
        public HttpResourceProvider(IWebClientFactory factory, IAuditor auditor)
        {
            _clientFactory = factory;
            _auditor = auditor;
        }
        private IWebClient client()
        {
            return _clientFactory.client();
        }
        public bool exists(Uri resource)
        {
            return _auditor.wrapFunction(((g) =>
            {
                return client().exists(resource);
            }), "exists: " + resource.ToString(), "httpResourceProvider");
        }
        public long getSize(System.Uri resource)
        {
            return _auditor.wrapFunction(((g) =>
            {
                return client().getSize(resource);
            }), "getSize: " + resource.ToString(), "httpResourceProvider");
        }
        public string secureGetString(System.Uri resource)
        {
            return _auditor.wrapFunction(((g) =>
            {
                return client().downloadString(resource);
            }), "secureGetString: " + resource.ToString(), "httpResourceProvider");
        }
        public string secureGetBytesAsString(System.Uri resource)
        {
            return _auditor.wrapFunction(((g) =>
            {
                return System.Text.Encoding.UTF8.GetString(client().downloadData(resource));
            }), "secureGetBytesAsString: " + resource.ToString(), "httpResourceProvider");
        }
        public string insecureGetString(System.Uri resource)
        {
            return _auditor.wrapFunction(((g) =>
            {
                return client().downloadString(resource);
            }), "insecureGetString: " + resource.ToString(), "httpResourceProvider");
        }
        public string securePutData(System.Uri uri, byte[] data)
        {
            return _auditor.wrapFunction(((g) =>
            {
                return client().uploadData(uri, data);
            }), "securePutData: " + uri.ToString(), "httpResourceProvider");
        }
        public byte[] secureGetData(System.Uri resource)
        {
            return _auditor.wrapFunction(((g) =>
            {
                return client().downloadData(resource);
            }), "secureGetData: " + resource.ToString(), "httpResourceProvider");
        }
        public string securePutFile(System.Uri uri, string filename)
        {
            return _auditor.wrapFunction(((g) =>
            {
                return decode(client().uploadFile(uri, filename));
            }), "securePutFile: " + uri.ToString(), "httpResourceProvider");
        }
        private string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
