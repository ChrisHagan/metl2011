using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Xml.Serialization;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace PowerpointJabber
{
public class CouchTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            Logger.Log(message);
        }
        public override void WriteLine(string message)
        {
            Logger.Log(message);
        }
    }
    public class LogMessage 
    {
        public LogMessage()
        {
        }
        public LogMessage(string content, long timestamp, int slide, string user, string server, string version)
        {
            Content = content;
            Timestamp = timestamp;
            Slide = slide;
            User = user;
            Server = server;
            Version = version;
        }
        public string Version;
        public string Content; 
        public long Timestamp; 
        public string User; 
        public string Server; 
        public int Slide;
        public string JSON
        {
            get
            {
                return "{\"version\" : \"" + Version + "\""
                    + ", \"content\" : \"" + Content + "\""
                    + ", \"Timestamp\" : " + Timestamp
                    + ", \"User\" : \"" + User + "\""
                    + ", \"Server\" : \"" + Server + "\""
                    + ", \"Slide\" : " + Slide +"}";
            }
        }
        public NameValueCollection QueryString
        {
            get
            {
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                queryString["program"] = "simplepens";
                queryString["version"] = Version;
                queryString["content"] = Content;
                queryString["timestamp"] = Convert.ToString(Timestamp);
                queryString["user"] = User;
                queryString["server"] = Server;
                queryString["slide"] = Convert.ToString(Slide);

                return queryString;
            }
        }
    }
    public class Logger
    {
        public static readonly string POST_LOG = "http://madam.adm.monash.edu.au:5984/simplepens_log";
        static readonly Uri LoggingServer = new Uri("https://madam.adm.monash.edu.au:1188/log_message.yaws");
        private static WebClient client()
        {
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            //client.Headers = new WebHeaderCollection();
            //client.Headers.Add("Content-Type: application/json");
            return client;
        }
        //private static Uri couchServer = new Uri(POST_LOG);
        private static string[] LogExceptions = new[] {
                "powerpnt.exe Information: 0 :"};
        public static void StartLogger()
        {
            safetyQueue();
            Trace.Listeners.Add(new CouchTraceListener());
        }
        private static System.Threading.Timer backgroundUploadTimer;
        private static void renewTimer()
        {
            if (backgroundUploadTimer == null)
                backgroundUploadTimer = new System.Threading.Timer((state) =>
                    {
                        safetyQueue();
                    });
            backgroundUploadTimer.Change(60000, 60000);
        }
        public static void Info(string message)
        {
            try
            {
                Trace.TraceInformation(message);
            }
            catch (Exception) { }
        }
        public static void Info(string message, params string[] args)
        {
            try
            {
                Trace.TraceInformation(message, args);
            }
            catch (Exception) { }
        }
        public static void Crash(Exception e)
        {
            var crashMessage = string.Format("CRASH: {0} @ {1} INNER: {2}",
                e.Message,
                e.StackTrace,
                e.InnerException == null ? "NONE" : e.InnerException.StackTrace);
            Log(crashMessage);
        }
        public static void Log(string appendThis)
        {
            var now = DateTime.Now;
            putCouch(appendThis, now);
        }
        private static void putCouch(LogMessage message)
        {
            try
            {
                //client().UploadStringAsync(couchServer, message.JSON);
                client().QueryString = message.QueryString;
                client().DownloadStringAsync(LoggingServer);
            }
            catch (Exception)
            {
                saveToQueue(message);
            }
        }
        private static void putCouch(string message, DateTime now)
        {
            if (String.IsNullOrEmpty(message)) return;
            if (message.Contains(POST_LOG)) return;
            if (LogExceptions.Any(prefix => message.StartsWith(prefix))) return;
            var name = (ThisAddIn.instance == null
                || ThisAddIn.instance.Application == null
                || String.IsNullOrEmpty(ThisAddIn.instance.Application.Name)) ? "unknown" : ThisAddIn.instance.Application.Name;
            int currentPosition = (ThisAddIn.instance == null
                || ThisAddIn.instance.Application == null
                || String.IsNullOrEmpty(ThisAddIn.instance.Application.Name)
                || ThisAddIn.instance.Application.ActivePresentation == null
                || ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow == null
                || ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View == null) ? 0 : ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.CurrentShowPosition;
            string pptVersion = (ThisAddIn.instance == null
                || ThisAddIn.instance.Application == null
                || String.IsNullOrEmpty(ThisAddIn.instance.Application.Version)) ? "unknown PowerPoint version" : ThisAddIn.instance.Application.Version;
            string version = ThisAddIn.version;
            var msg = new LogMessage(message,now.Ticks,currentPosition,name,pptVersion,version);
            putCouch(msg);
        }
        private static List<LogMessage> MessageQueue = new List<LogMessage>();
        private static void saveToQueue(LogMessage message)
        {
            MessageQueue.Add(message);
            renewTimer();
        }
        private static void safetyQueue()
        {
            readMessageQueueFromDisk();
            tryToResendMessageQueue();
            saveRemainingMessageQueueToDisk();
            if (MessageQueue.Count == 0 && backgroundUploadTimer != null)
                backgroundUploadTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        private static readonly string backupFile = "cachedLogMessages.xml";
        private static XmlSerializer serializer = new XmlSerializerFactory().CreateSerializer(typeof(List<LogMessage>));
        private static void readMessageQueueFromDisk()
        {
            try
            {
                using (var file = File.Open(backupFile, FileMode.Open))
                {
                    List<LogMessage> tempQueue = (List<LogMessage>)serializer.Deserialize(file);
                    foreach (LogMessage msg in tempQueue)
                        MessageQueue.Add(msg);
                }
            }
            catch (Exception) { }
        }
        private static void tryToResendMessageQueue()
        {
            List<LogMessage> tempQueue = new List<LogMessage>();
            while (MessageQueue.Count > 0)
            {
                tempQueue.Add((LogMessage)MessageQueue[0]);
                MessageQueue.RemoveAt(0);
            }
            foreach (LogMessage msg in tempQueue)
                putCouch(msg);
        }
        private static void saveRemainingMessageQueueToDisk()
        {
            try
            {
                File.Delete(backupFile);
                using (var file = File.Open(backupFile, FileMode.OpenOrCreate))
                {
                    List<LogMessage> tempQueue = new List<LogMessage>();
                    while (MessageQueue.Count > 0)
                    {
                        tempQueue.Add((LogMessage)MessageQueue[0]);
                        MessageQueue.RemoveAt(0);
                    }
                    serializer.Serialize(file, tempQueue);
                }
            }
            catch (Exception) { }
        }
    }
}