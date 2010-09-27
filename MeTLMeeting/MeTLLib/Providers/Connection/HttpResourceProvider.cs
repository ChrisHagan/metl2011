using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System;
using System.Threading;
using System.Diagnostics;

namespace MeTLLib.Providers.Connection
{
    class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = (WebRequest)base.GetWebRequest(address);
            request.Timeout = int.MaxValue;
            return request;
        }
    }
    class HttpResourceProvider
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
        private static readonly NetworkCredential MeTLCredentials = new NetworkCredential("exampleUsername", "examplePassword");
        private static bool firstRun = true;

        private static WebClient client()
        {
            configureServicePointManager();
            var wc = new WebClientWithTimeout { Credentials = MeTLCredentials };
            return wc;
        }
        private static void configureServicePointManager()
        {
            if (firstRun)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(bypassAllCertificateStuff);
                ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
                firstRun = false;
            }
        }
        private static bool bypassAllCertificateStuff(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
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
        public static long getSize(string resource)
        {
            configureServicePointManager();
            var request = (HttpWebRequest)HttpWebRequest.Create(resource);
            request.Credentials = MeTLCredentials;
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
        public static bool exists(string resource)
        {
            configureServicePointManager();
            var request = (HttpWebRequest)HttpWebRequest.Create(resource);
            request.Credentials = MeTLCredentials;
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
            string type = "secureGetString";
            string responseString = "";
            int attempts = 0;
            int percentage = 0;
            bool isDownloaded = false;
            bool isCancelled = false;
            while (!isDownloaded && attempts < 5)
            {
                try
                {
                    bool isPercentage = false;
                    long size = getSize(resource);
                    if (size > 0)
                        isPercentage = true;
                    percentage = 0;
                    isCancelled = false;
                    long recBytes = 0;
                    var webclient = client();
                    var threadAction = new Thread(new ThreadStart(() =>
                    {
                        webclient.DownloadStringAsync(new System.Uri(resource, UriKind.RelativeOrAbsolute));
                        NotifyStatus("starting", type, resource);
                    }));
                    webclient.DownloadProgressChanged += (sender, args) =>
                    {
                        if (!isDownloaded)
                        {
                            recBytes = args.BytesReceived;
                            NotifyProgress(attempts, type, resource, recBytes, size, percentage, isPercentage);
                        }
                    };
                    webclient.DownloadStringCompleted += (sender, args) =>
                    {
                        isCancelled = args.Cancelled;
                        responseString = args.Result;
                        NotifyStatus("completed", type, resource);
                        isDownloaded = true;
                        webclient.CancelAsync();
                        threadAction.Abort();
                        attempts = 6;
                    };
                    threadAction.Start();
                    threadAction.Join();
                    var startTime = DateTime.Now;
                    while (webclient.IsBusy && !isDownloaded)
                    {
                        if (isCancelled || startTime.AddSeconds(5) < DateTime.Now) { throw new Exception(); }
                    }
                    if (isDownloaded) break;
                }
                catch (Exception)
                {
                    NotifyStatus("error", type, resource);
                    attempts++;
                    if (attempts == 5)
                    {
                        try
                        {
                            return client().DownloadString(resource);
                        }
                        catch (Exception) { return ""; }
                    }
                }
            }
            return responseString;
        }

        public static string insecureGetString(string resource)
        {
            string type = "insecureGet";
            string responseString = "";
            int attempts = 0;
            int percentage = 0;
            bool isDownloaded = false;
            bool isCancelled = false;
            while (!isDownloaded && attempts < 5)
            {
                try
                {
                    bool isPercentage = false;
                    long size = getSize(resource);
                    if (size > 0)
                        isPercentage = true;
                    percentage = 0;
                    isCancelled = false;
                    long recBytes = 0;
                    var webclient = client();
                    var threadAction = new Thread(new ThreadStart(() =>
                    {
                        webclient.DownloadStringAsync(new System.Uri(resource, UriKind.RelativeOrAbsolute));
                        NotifyStatus("starting", type, resource);
                    }));
                    webclient.DownloadProgressChanged += (sender, args) =>
                    {
                        if (!isDownloaded)
                        {
                            recBytes = args.BytesReceived;
                            NotifyProgress(attempts, type, resource, recBytes, size, percentage, isPercentage);
                        }
                    };
                    webclient.DownloadStringCompleted += (sender, args) =>
                    {
                        isCancelled = args.Cancelled;
                        responseString = args.Result;
                        NotifyStatus("completed", type, resource);
                        isDownloaded = true;
                        webclient.CancelAsync();
                        threadAction.Abort();
                        attempts = 6;
                    };
                    threadAction.Start();
                    threadAction.Join();
                    var startTime = DateTime.Now;
                    while (webclient.IsBusy && !isDownloaded)
                    {
                        if (isCancelled || startTime.AddSeconds(2) < DateTime.Now) { throw new Exception(); }
                    }
                    if (isDownloaded) break;
                }
                catch (Exception ex)
                {
                    NotifyStatus("error", type, resource);
                    attempts++;
                    if (attempts == 5)
                    {
                        try
                        {
                            return new WebClient().DownloadString(resource);
                        }
                        catch (Exception) { return ""; }
                    }
                }
            }
            return responseString;
        }
        public static string securePutData(string uri, byte[] data)
        {
            string type = "securePutData";
            byte[] responseBytes = new byte[0];
            int attempts = 0;
            int percentage = 0;
            bool isDownloaded = false;
            bool isCancelled = false;
            while (!isDownloaded && attempts < 5)
            {
                try
                {
                    percentage = 0;
                    isCancelled = false;
                    long recBytes = 0;
                    var webclient = client();
                    var threadAction = new Thread(new ThreadStart(() =>
                    {
                        webclient.UploadDataAsync(new System.Uri(uri, UriKind.RelativeOrAbsolute), data);
                        NotifyStatus("starting", type, uri);
                    }));
                    webclient.UploadProgressChanged += (sender, args) =>
                    {
                        if (!isDownloaded)
                        {
                            recBytes = args.BytesSent + args.BytesReceived;
                            percentage = (int)(recBytes / (args.TotalBytesToSend + args.TotalBytesToReceive));
                            NotifyProgress(attempts, type, uri, recBytes, args.TotalBytesToSend, percentage, true);
                        }
                    };
                    webclient.UploadDataCompleted += (sender, args) =>
                    {
                        isCancelled = args.Cancelled;
                        responseBytes = args.Result;
                        NotifyStatus("completed", type, uri);
                        isDownloaded = true;
                        webclient.CancelAsync();
                        threadAction.Abort();
                        attempts = 6;
                    };
                    threadAction.Start();
                    threadAction.Join();
                    while (webclient.IsBusy && !isDownloaded)
                    {
                    }
                    if (isDownloaded) break;
                }
                catch (Exception)
                {
                    NotifyStatus("error", type, uri);
                    attempts++;
                    if (attempts == 5)
                    {
                        try
                        {
                            return decode(client().UploadData(uri, data));
                        }
                        catch (Exception) { return ""; }
                    }
                }
            }
            return decode(responseBytes);
        }
        public static byte[] secureGetData(string resource)
        {
            string type = "secureGetData";
            byte[] responseBytes = new byte[0];
            int attempts = 0;
            int percentage = 0;
            bool isDownloaded = false;
            bool isCancelled = false;
            while (!isDownloaded && attempts < 5)
            {
                try
                {
                    bool isPercentage = false;
                    long size = getSize(resource);
                    if (size > 0)
                        isPercentage = true;
                    percentage = 0;
                    isCancelled = false;
                    long recBytes = 0;
                    var webclient = client();
                    var threadAction = new Thread(new ThreadStart(() =>
                    {
                        webclient.DownloadDataAsync(new System.Uri(resource, UriKind.RelativeOrAbsolute));
                        NotifyStatus("starting", type, resource);
                    }));
                    webclient.DownloadProgressChanged += (sender, args) =>
                    {
                        if (!isDownloaded)
                        {
                            recBytes = args.BytesReceived;
                            NotifyProgress(attempts, type, resource, recBytes, size, percentage, isPercentage);
                        }
                    };
                    webclient.DownloadDataCompleted += (sender, args) =>
                    {
                        isCancelled = args.Cancelled;
                        responseBytes = args.Result;
                        NotifyStatus("completed", type, resource);
                        isDownloaded = true;
                        webclient.CancelAsync();
                        threadAction.Abort();
                        attempts = 6;
                    };
                    threadAction.Start();
                    threadAction.Join();
                    while (webclient.IsBusy && !isDownloaded)
                    {
                    }
                    if (isDownloaded) break;
                }
                catch (Exception)
                {
                    NotifyStatus("error", type, resource);
                    attempts++;
                    if (attempts == 5)
                    {
                        try
                        {
                            return client().DownloadData(resource);
                        }
                        catch (Exception) { return null; }
                    }
                }
            }
            return responseBytes;
        }
        public static string securePutFile(string uri, string filename)
        {
            string type = "securePutFile";
            byte[] responseBytes = new byte[0];
            int attempts = 0;
            int percentage = 0;
            bool isDownloaded = false;
            bool isCancelled = false;
            while (!isDownloaded && attempts < 5)
            {
                try
                {
                    percentage = 0;
                    isCancelled = false;
                    long recBytes = 0;
                    var webclient = client();
                    var threadAction = new Thread(new ThreadStart(() =>
                    {
                        webclient.UploadFileAsync(new System.Uri(uri, UriKind.RelativeOrAbsolute), filename);
                        NotifyStatus("starting", type, uri, filename);
                    }));
                    webclient.UploadProgressChanged += (sender, args) =>
                    {
                        if (!isDownloaded)
                        {
                            recBytes = args.BytesSent + args.BytesReceived;
                            percentage = (int)(recBytes / (args.TotalBytesToSend + args.TotalBytesToReceive));
                            NotifyProgress(attempts, type, uri, recBytes, args.TotalBytesToSend, percentage, true);
                        }
                    };
                    webclient.UploadFileCompleted += (sender, args) =>
                    {
                        isCancelled = args.Cancelled;
                        responseBytes = args.Result;
                        NotifyStatus("completed", type, uri);
                        isDownloaded = true;
                        webclient.CancelAsync();
                        threadAction.Abort();
                        attempts = 6;
                    };
                    threadAction.Start();
                    threadAction.Join();
                    while (webclient.IsBusy && !isDownloaded)
                    {
                    }
                    if (isDownloaded) break;
                }
                catch (Exception)
                {
                    NotifyStatus("error", type, uri);
                    attempts++;
                    if (attempts == 5)
                    {
                        try
                        {
                            return decode(client().UploadFile(uri, filename));
                        }
                        catch (Exception) { return ""; }
                    }
                }
            }
            return decode(responseBytes);
        }
        private static string decode(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        private static void NotifyStatus(string status, string type, string uri)
        {
            NotifyStatus(status, type, uri, "");
        }
        private static void NotifyStatus(string status, string type, string uri, string filename)
        {
            return;
            var filenameDescriptor = string.IsNullOrEmpty(filename) ? "" : " : " + filename;
            Trace.TraceInformation(status + "(" + type + " : " + uri + filenameDescriptor + ")");
        }
        private static void NotifyProgress(int attempts, string type, string resource, long recBytes, long size, int percentage, bool isPercentage)
        {
            return;
            if (isPercentage)
            {
                percentage = (int)(recBytes / size);
                Trace.TraceInformation("Attempt " + attempts + " waiting on " + type + ": " + percentage + "% (" + resource + ")");
            }
            else
                Trace.TraceInformation("Attempt " + attempts + " waiting on " + type + ": " + recBytes + "B (" + resource + ")");
        }
    }
}
